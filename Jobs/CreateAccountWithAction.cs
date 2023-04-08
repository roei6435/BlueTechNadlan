
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    public class CreateAccountWithAction
    {
        IOrganizationService service;
        public CreateAccountWithAction(IOrganizationService service)
        {
            this.service = service;
        }

        public void Execute()
        {
            OrganizationRequest request = new OrganizationRequest("roe__Create_Asset");
            request["accountname"] = "client test";
            request["accountphone"] = "986798798";
            request["assetname"] = "asset test";
            request["assetprice"] = new Money(decimal.Parse("15000000.0"));
            request["assetroomnumburs"] = decimal.Parse("3.0");
            service.Execute(request);
            Environment.Exit(0);


        }






    }
}
