using BlueTech.Actions.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;
using System.Runtime.Remoting.Contexts;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class VisitTests
    {
        private readonly IOrganizationService _organizationService;

        public VisitTests()
        {
            var connactionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var crmServiceClient = new CrmServiceClient(connactionString);
            _organizationService = crmServiceClient;
        }
        [TestMethod]
        public void TestAction()                //בדיקת הפונקצייה שתייצר ביקור משכבת הבי אל 
        {
            var actionBL = new ActionsBL(_organizationService);
            string name = "רועי בן-דוד";
            string phone = "08-354349933";
            string email = "someonel166@example.com";
            DateTime date = new DateTime(2023, 04, 16, 21, 0, 0);
            EntityReference assetRef = new EntityReference("roe_assets", new Guid("09ddbc12-55c4-ed11-9886-000d3add8f0a"));
            EntityReference visitorRef = new EntityReference("contact", new Guid("c77a103d-53c4-ed11-9886-000d3add8f0a"));
            var result = actionBL.CreateVisitFromBL( name, phone,  email,  date,  assetRef,  visitorRef);
        }
        [TestMethod]
        //קריאה לאקשיין בבקשת WEB API 
        public void callToActionCreateVisitWithWebApiRequest()
        {
            OrganizationRequest request = new OrganizationRequest("roe__Create_Visit_In_Asset");
            request["name"] = "רועי בן-דוד";
            request["phone"] = "08-354349933";
            request["email"] = "someonel166@example.com";
            request["date"] = new DateTime(2023, 04, 16, 21, 0, 0);
            request["assetRef"] = new EntityReference("roe_assets", new Guid("09ddbc12-55c4-ed11-9886-000d3add8f0a"));
            request["contactRef"] = new EntityReference("contact", new Guid("c77a103d-53c4-ed11-9886-000d3add8f0a"));

            // Call the custom action
            OrganizationResponse response = _organizationService.Execute(request);

            // Get the output parameter value
            EntityReference visitRef = (EntityReference)response["visitRef"];
        }

    }
}
