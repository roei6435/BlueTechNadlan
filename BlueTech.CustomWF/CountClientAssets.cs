using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.CustomWF
{
    public class CountClientAssets : CodeActivity
    {

        [Input("Owner of the asset")]
        [ReferenceTarget("account")]
        public InArgument<EntityReference> Owner { get; set; }

        [Output("Number of client assets")]
        public OutArgument<int> AssetCount { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {

            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var ownerId = Owner.Get(executionContext).Id;   

 
            var query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("roe_owner", ConditionOperator.Equal, ownerId));
            query.ColumnSet = new ColumnSet(false);

            var results = service.RetrieveMultiple(query);
            var count = results.Entities.Count;

            AssetCount.Set(executionContext, count);
        }
    }
}
