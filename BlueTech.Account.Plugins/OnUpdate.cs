using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Account.Plugins
{
    public class OnUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("the plugin on running");
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                if (context.InputParameters.Contains("Target") & context.InputParameters["Target"] is Entity)
                {
                    Entity target = (Entity)context.InputParameters["Target"];
                    if (target.Attributes.Contains("roe_percent"))
                    {
                        var precent = (decimal)target.Attributes["roe_percent"]/100;
                        var assetCollection = GetAllAssetsByAccountId(service, target.Id);
                        decimal total = 0;
                        foreach (var entity in assetCollection.Entities)
                        {


                            var priceMonthly = entity.Attributes.Contains("roe_monthlypay") ? (Money)entity["roe_monthlypay"] : new Money(0);
                            var priceOfAsset = entity.Attributes.Contains("roe_priceofamount") ? (Money)entity["roe_priceofamount"] : new Money(0);
                            var dealType = entity.Attributes.Contains("roe_dealtype") ? (OptionSetValue)entity["roe_dealtype"] : null;
                            var description = entity.Attributes.Contains("roe_name") ? entity["roe_name"] : null;

                            if (dealType.Value == 913200001) //renting
                                total += priceMonthly.Value;
                            else                             //sale
                                total += priceOfAsset.Value * precent;

                        }
                        
                        Entity accountToUpdate = new Entity("account", target.Id);
                        accountToUpdate["roe_profit"] = total;
                        CreateLog(service, target, context.UserId, total);
                        service.Update(accountToUpdate);

                    }
                }
            }
            catch (Exception err)
            {

                tracingService.Trace($"Message:{err.Message} trace:{err.StackTrace} בשם השם נעשה ונצליח!");
                throw err;
            }
        }

        public EntityCollection GetAllAssetsByAccountId(IOrganizationService service, Guid accountId)
        {
            QueryExpression query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("roe_owner", ConditionOperator.Equal, accountId));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.ColumnSet = new ColumnSet("roe_monthlypay", "roe_priceofamount", "roe_dealtype", "roe_name");
            return service.RetrieveMultiple(query);

        }
        private void CreateLog(IOrganizationService service, Entity target, Guid adminId, decimal newProfit)
        {

            Entity newRecordInEntity = new Entity("roe_logprofit");
            EntityReference accountOnChange = new EntityReference("account", target.Id);
            newRecordInEntity["roe_name"] = $"New update for :{accountOnChange.Name}";
            newRecordInEntity["roe_account"] = accountOnChange;
            newRecordInEntity["roe_precentprofit"] = (decimal)target.Attributes["roe_percent"];
            newRecordInEntity["roe_sumofprofitnow"] = newProfit;
            newRecordInEntity["ownerid"] = new EntityReference("systemuser", adminId);
            service.Create(newRecordInEntity);
        }
    }
}
