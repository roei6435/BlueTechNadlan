using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    public class CreateVisitInAsset       //call to action from job
    {
        IOrganizationService service;
        public CreateVisitInAsset(IOrganizationService service)
        {
            this.service = service;
        }

        public void Execute()
        {

            OrganizationRequest request = new OrganizationRequest("roe__Create_Visit_In_Asset");
            request["name"] = "דור בן יאיר";
            request["phone"] = "0505451111";
            request["mail"] = "someon400@exawmple.com";
            request["date"] = new DateTime(2023, 04, 18, 13, 0, 0);
            request["assetid"] = new EntityReference("roe_assets", new Guid("09ddbc12-55c4-ed11-9886-000d3add8f0a"));

            // Call the custom action
            OrganizationResponse response = service.Execute(request);

            // Get the output parameter value
            EntityReference visitRef = (EntityReference)response["visitid"];

            Console.WriteLine($"The visit created in succsefully, the visit id:{visitRef.Id}");

        }


    }
}
