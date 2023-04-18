using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Actions.Plugins
{
    public class CreateVisitInAsset : IPlugin
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
                string name = context.InputParameters.Contains("name") ? context.InputParameters["name"].ToString() : null;
                string phone = context.InputParameters.Contains("phone") ? context.InputParameters["phone"].ToString() : null;
                string email = context.InputParameters.Contains("email") ? context.InputParameters["email"].ToString() : null;
                DateTime? date = context.InputParameters.Contains("date") ? (DateTime)context.InputParameters["date"] : (DateTime?)null;
                EntityReference assetRef = context.InputParameters.Contains("assetRef") ? (EntityReference)context.InputParameters["assetRef"] : null;
                EntityReference visitorRef = context.InputParameters.Contains("contactRef") ? (EntityReference)context.InputParameters["contactRef"] : null;
                ActionsBL actionBL = new ActionsBL(service);
                context.OutputParameters["visitRef"] = actionBL.CreateVisitFromBL(name, phone, email, date, assetRef, visitorRef);
            }
            catch (Exception err)
            {

                tracingService.Trace(err.Message);
            }


        }
    }
}
