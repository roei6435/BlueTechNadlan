using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Actions.Plugins
{
    public class CreateAsset : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("the plugin on running");
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            string accountName = context.InputParameters.Contains("accountname") ? context.InputParameters["accountname"].ToString() : null;
            string accountPhone = context.InputParameters.Contains("accountphone") ? context.InputParameters["accountphone"].ToString() : null;

            Entity createdAccount = new Entity("account");
            createdAccount.Attributes.Add("name", accountName);
            createdAccount.Attributes.Add("telephone1", accountPhone);

            Guid accountId = service.Create(createdAccount);

            string assetName = context.InputParameters.Contains("assetname") ? context.InputParameters["assetname"].ToString() : null;
            Money assetPrice = context.InputParameters.Contains("assetprice") ? (Money)context.InputParameters["assetprice"] : null;
            decimal roomNumbur = context.InputParameters.Contains("assetroomnumburs") ? (decimal)context.InputParameters["assetroomnumburs"] : 0;

            Entity createdAsset = new Entity("roe_assets");
            createdAsset.Attributes.Add("roe_name", assetName);
            createdAsset.Attributes.Add("roe_priceofamount", assetPrice);
            createdAsset.Attributes.Add("roe_numburrooms", roomNumbur);
            createdAsset.Attributes.Add("roe_owner", new EntityReference("account", accountId));


            service.Create(createdAsset);



            //Guid assetId = service.Create(createdAsset);

            //context.OutputParameters.Add("accountid", new EntityReference("account", accountId));
            //context.OutputParameters.Add("assetid", new EntityReference("roe_assets", assetId));

        }
    }
}
