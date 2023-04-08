using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Assets.Plugins
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

                    Entity target = context.InputParameters["Target"] as Entity;
                    Money priceSell = target.Attributes.Contains("roe_priceofamount") ? (Money)target["roe_priceofamount"] :null;
                    Money priceRenting = target.Attributes.Contains("roe_monthlypay") ? (Money)target["roe_monthlypay"] : null;
                    //target.Id ===> id of asset ===> 64Ba
                    //target.LogicalName  ====> name of tabale ==> assets
                    //get account feild about this asset
                    // Entity currAsset = service.Retrieve(target.LogicalName, target.Id,new ColumnSet("roe_owner"));
                    

                    Entity postImg = context.PostEntityImages.Contains("PostImg") ? context.PostEntityImages["PostImg"] : null;
                    EntityReference ownerOfThisAsset = postImg.GetAttributeValue<EntityReference>("roe_owner");
                    

                    if (ownerOfThisAsset != null)
                    {
                        EntityCollection allAssetOfThisOwner = GetAllAssetsByAccountId(service, ownerOfThisAsset.Id);
                        decimal precent= GetPrecentByAccountId(service,ownerOfThisAsset.Id);
                        decimal total = 0;
                        foreach (var entity in allAssetOfThisOwner.Entities)
                        {


                            var priceMonthly = entity.Attributes.Contains("roe_monthlypay") ? (Money)entity["roe_monthlypay"] : new Money(0);
                            var priceOfAsset = entity.Attributes.Contains("roe_priceofamount") ? (Money)entity["roe_priceofamount"] : new Money(0);
                            var dealType = entity.Attributes.Contains("roe_dealtype") ? (OptionSetValue)entity["roe_dealtype"] : null;
                            if (dealType.Value == 913200001) //renting
                                total += priceMonthly.Value;
                            else                             //sale
                                total += priceOfAsset.Value * precent;

                        }
                        Entity accountToUpdate = new Entity("account", ownerOfThisAsset.Id);
                        accountToUpdate["roe_profit"] = total;
                        service.Update(accountToUpdate);

                    }
                }
            }
            catch (Exception err)
            {

                tracingService.Trace($"Message:{err.Message} trace:{err.StackTrace}");
                throw err;
            }


        }
        private EntityCollection GetAllAssetsByAccountId(IOrganizationService service, Guid accountId)
        {
            QueryExpression query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("roe_owner", ConditionOperator.Equal, accountId));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.ColumnSet = new ColumnSet("roe_monthlypay", "roe_priceofamount", "roe_dealtype");

            return service.RetrieveMultiple(query);
        }
        private decimal GetPrecentByAccountId(IOrganizationService service, Guid accountId)
        {

            QueryExpression query = new QueryExpression("account");
            query.Criteria.AddCondition(new ConditionExpression("accountid", ConditionOperator.Equal, accountId));
            query.ColumnSet = new ColumnSet("roe_percent");

            var results = service.RetrieveMultiple(query);
            decimal precent=0;
            foreach (var account in results.Entities)
            {
                precent = account.GetAttributeValue<decimal>("roe_percent");
            }
            return precent/100;
        }




    }
}
