using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Opportunity.Plugins
{
    public class OpportunityCloseWin : IPlugin
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
                //check that this is a winning opportunity
                if (context.PrimaryEntityName == "opportunity" && context.MessageName == "Win")
                {
                    Entity opportunity = (Entity)context.InputParameters["OpportunityClose"];
                    Guid opportunityId = ((EntityReference)opportunity.Attributes["opportunityid"]).Id;
                    opportunity = GetOpportunityEntityWithFeilds(opportunityId, service);
                    if (opportunity.Attributes.Contains("roe_agent") && opportunity.Attributes.Contains("roe_agent_commision"))
                    {
                        Guid agentId = ((EntityReference)opportunity["roe_agent"]).Id;
                        Money commisionAgent = (Money)opportunity["roe_agent_commision"];
                        EntityCollection releventOpportunities = GetCloseOpperWinByAgent(agentId, service);   //get the relevent opp        
                        if (releventOpportunities.Entities.Count > 0)
                        {

                            int countOfOppClosedWithWin = releventOpportunities.Entities.Count + 1;   //new count opportunities          
                            decimal sumCommisionFromOpp = GetSumOfCommissionOfThisAgent(releventOpportunities);   //sum of commision agent (without this opp)
                            sumCommisionFromOpp += commisionAgent.Value;

                            Entity agentToUpdate = new Entity("roe_agents", agentId);                   //update the agent entity in relevant fields
                            agentToUpdate["roe_qty_deals_total"] = countOfOppClosedWithWin;
                            agentToUpdate["roe_sum_deals_total"] = new Money(sumCommisionFromOpp);
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
        private Entity GetOpportunityEntityWithFeilds(Guid opportunityId, IOrganizationService service)
        {

            ColumnSet columns = new ColumnSet("roe_agent", "roe_agent_commision");
            Entity opportunity = service.Retrieve("opportunity", opportunityId, columns);
            return opportunity;
        }
        private decimal GetSumOfCommissionOfThisAgent(EntityCollection collectionOpperOfThisAgent)
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
            return sumOfOpper;
        }
        private EntityCollection GetCloseOpperWinByAgent(Guid agentId, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("opportunity");
            //unactive opportunity && associated to this agentId //closed with winning
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
            query.Criteria.AddCondition(new ConditionExpression("roe_agent", ConditionOperator.Equal, agentId));
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 3);
            query.ColumnSet = new ColumnSet("roe_agent_commision");
            return service.RetrieveMultiple(query);
        }
    }
}
