using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Leads.Plugins
{
    public class OnQualifyLead : IPlugin
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
                if (context.MessageName.ToLower() == "qualifylead" && context.InputParameters["LeadId"] is EntityReference)
                {
                    EntityReference leadEntRef = (EntityReference)context.InputParameters["LeadId"];
                    Entity lead = service.Retrieve(leadEntRef.LogicalName, leadEntRef.Id, new ColumnSet("roe_typeturn"));

                    OptionSetValue choiceValue = lead.Attributes.Contains("roe_typeturn") ? (OptionSetValue)lead["roe_typeturn"] : new OptionSetValue(0);

                    var createReq = new CreateRequest() { Parameters = context.InputParameters };
                    if (choiceValue.Value == 913200000) // buye 
                    {
                        createReq.Parameters["CreateAccount"] = false;

                    }
                    else if (choiceValue.Value == 913200001)   //saler
                    {
                        createReq.Parameters["CreateContact"] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Message:{ex.Message} trace:{ex.StackTrace}");
            }

        }
    }
}








