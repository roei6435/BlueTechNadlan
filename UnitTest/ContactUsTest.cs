using BlueTech.Actions.Plugins;
using crmBL;
using crmBL.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class ContactUsTest
    {
        private readonly IOrganizationService _organizationService;

        public ContactUsTest()
        {
            var connactionString = ConfigurationManager.ConnectionStrings["crm"].ConnectionString;
            var crmServiceClient = new CrmServiceClient(connactionString);
            _organizationService = crmServiceClient;
        }
        [TestMethod]
        public void SellerWithAccountNotExist()            
        {
            // Requirements: 
            //lead confirm
            //create new account without oppertunity
            //create asset linked to account
            var crmBl = new CrmBL(_organizationService);
            var result = crmBl.MainFunction(new ContactUsDataModel()
            {
                FirstName = "דולב",
                LastName = "בן שטרית",
                Email = "dolevNew@example.com",
                Phone = "333331111",
                TitleOfAsset="דירה חדשה בנתיבות",
                AssetPrice=3000000,
                FullAddress= "הרב מסעוד אלפסי",
                City="נתיבות",
                NumburRooms=(decimal)3.5,
                LeadType = LeadType.seller,
                DealType = DealType.buying



            });
        }
        [TestMethod]
        public void SellerWithAccountExist()
        {
            // Requirements: 
            //lead confirm without oppertunity
            //create asset linked to account
            var crmBl = new CrmBL(_organizationService);
            var result = crmBl.MainFunction(new ContactUsDataModel()
            {
                FirstName = "טליה",
                LastName = "פרץ",
                Email = "someone1@example.com",
                Phone = "033333333",
                TitleOfAsset = "דירה חדשה בירוחם",
                AssetPrice = 3000000,
                FullAddress = "הרצל 317",
                City = "ירוחם",
                NumburRooms = (decimal)5.5,
                LeadType = LeadType.seller,
                DealType = DealType.buying



            });
        }
        [TestMethod]
        public void BuyerAccountNotExist()
        {
            // Requirements: 
            //only create laed and confirm by agent after validtion
            var crmBl = new CrmBL(_organizationService);
            var result = crmBl.MainFunction(new ContactUsDataModel()
            {
                FirstName = "יצחק",
                LastName = "עמר",
                Email = "itzthak@example.com",
                Phone = "0444777733",
                LeadType = LeadType.buyer,

            });
        }
        [TestMethod]
        public void BuyerAccountExist()
        {
            // Requirements: 
            //create laed, and confirmed
            //and create new oppertunity
            var crmBl = new CrmBL(_organizationService);
            var result = crmBl.MainFunction(new ContactUsDataModel()
            {
                FirstName = "רוני",
                LastName = "ליזרוביץ",
                Email = "rbdforwork@gmail.com",
                Phone = "0000000",
                LeadType = LeadType.buyer

            });
        }
    }
}
