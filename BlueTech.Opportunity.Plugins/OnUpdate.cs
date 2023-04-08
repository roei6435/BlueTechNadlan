using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Opportunity.Plugins
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
                    if (target.Attributes.Contains("roe_agent") || target.Attributes.Contains("roe_asset"))    //if asset or agent has changed
                    {

                        Entity postImg = context.PostEntityImages.Contains("postImg") ? context.PostEntityImages["postImg"] : null;

                        //task 1
                        decimal agentComm = GetCommisionAgentByEntityRef(postImg, service);
                        decimal assetPrice = GetPriceAssetByEntityRef(postImg, service);
                        Entity opporToUpdate = new Entity("opportunity", target.Id);
                        opporToUpdate["roe_agent_commision"] = new Money(assetPrice * agentComm);               //calculate the commision agent feild
                        service.Update(opporToUpdate);

                        //task 2
                        EntityCollection allOpenOpperByThisAgent = GetOpenOpperByAgent(postImg, service);
                        if (allOpenOpperByThisAgent.Entities.Count > 0)
                        {
                            Guid agentId = postImg.GetAttributeValue<EntityReference>("roe_agent").Id;
                            Entity agentToUpdate = new Entity("roe_agents", agentId);
                            agentToUpdate["roe_open_deals_sum"] = GetSumOfCommissionOfThisAgent(allOpenOpperByThisAgent);
                            agentToUpdate["roe_qty_deals_open"] = allOpenOpperByThisAgent.Entities.Count;
                            service.Update(agentToUpdate);
                        }

                    }
                }
            }
            catch (Exception err)
            {
                tracingService.Trace($"Message:{err.Message} trace:{err.StackTrace}");
                throw err;
            }
        }

        private EntityCollection GetOpenOpperByAgent(Entity postImg, IOrganizationService service)
        {
            //get all the open opportunities with two condition: referencing to agent, and status active, only relevent column (agent_commision).
            EntityReference agentRef = postImg.GetAttributeValue<EntityReference>("roe_agent");
            QueryExpression query = new QueryExpression("opportunity");
            query.Criteria.AddCondition(new ConditionExpression("roe_agent", ConditionOperator.Equal, agentRef.Id));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.ColumnSet = new ColumnSet("roe_agent_commision");

            return service.RetrieveMultiple(query);
        }

        private Money GetSumOfCommissionOfThisAgent(EntityCollection collectionOpperOfThisAgent)
        {

            decimal sumOfOpper = 0;
            foreach (Entity opper in collectionOpperOfThisAgent.Entities)
            {
                try
                {
                    sumOfOpper += opper.Attributes.Contains("roe_agent_commision") ? ((Money)opper["roe_agent_commision"]).Value : 0;
                }
                catch (Exception err)
                {
                    //print to any log
                }
            }
            return new Money(sumOfOpper);
        }

        private decimal GetCommisionAgentByEntityRef(Entity postImg, IOrganizationService service)
        {
            EntityReference agentRef = postImg.GetAttributeValue<EntityReference>("roe_agent");
            Entity agent = service.Retrieve("roe_agents", agentRef.Id, new ColumnSet("roe_commision"));
            decimal agentComm = agent.Contains("roe_commision") ? (decimal)agent["roe_commision"] : 0;
            return agentComm;
        }
        private decimal GetPriceAssetByEntityRef(Entity postImg, IOrganizationService service)
        {
            EntityReference assetRef = postImg.GetAttributeValue<EntityReference>("roe_asset");
            Entity asset = service.Retrieve("roe_assets", assetRef.Id, new ColumnSet("roe_priceofamount"));
            decimal assetPrice = asset.Contains("roe_priceofamount") ? ((Money)asset["roe_priceofamount"]).Value : 0;
            return assetPrice;
        }


    }
}
