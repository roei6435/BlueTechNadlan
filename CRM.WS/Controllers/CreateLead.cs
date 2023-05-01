using crmBL;
using crmBL.Models;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CRM.WS.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/lead")]
    public class CreateLeadController : ApiController
    {
        [HttpPost]
        [Route("createlead")]
        public IHttpActionResult CreateLead(ContactUsDataModel contactUs)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var organizationService = new CrmServiceClient(connectionString);
            var crmBL=new CrmBL(organizationService);   
            bool res=crmBL.MainFunction(contactUs);
            return Ok();
        }

    }
}
