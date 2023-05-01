using Amazon.SecurityToken.Model;
using crmBL.Models;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer.Collaboration;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace crmBL
{
    public class CrmBL
    {


        private readonly IOrganizationService service;
        

        public CrmBL(IOrganizationService service)
        {
            this.service = service;
            
        }
        public bool MainFunction(ContactUsDataModel contactUs) 
        {
            
            try
            {
                //Get datils and create lead
                var result = CreateNewLead(contactUs);
                Entity leadEntity = result.Item1;
                bool accountExist = result.Item2;

                //if account not exist create new account in system
                bool createAccount = accountExist is true ? false : true;

                //if lead type is buyer create opportunity if seller not
                bool createOpportunity = contactUs.LeadType is LeadType.buyer ? true : false;

                //The condition in which the lead will be approved
                if (contactUs.LeadType == LeadType.buyer && accountExist || contactUs.LeadType == LeadType.seller)
                {
                    EntityReference createdRefernce = ConfirmLead(leadEntity, createAccount, createOpportunity);  //Function will return reference to oppertunity/account 

                    //The condition in which the asset will be created
                    if (createdRefernce!=null&&contactUs.LeadType == LeadType.seller)
                    {
                        Guid createdAsset = CreateNewAsset(contactUs, createdRefernce);
                    }

                }
            }
            catch(Exception err)
            {
                throw new Exception("Error retrieving account record", err);
            }

            return true;
        }
        private (Entity,bool) CreateNewLead(ContactUsDataModel contactUs)
        {
            Guid account = FindAccountByPhoneOrEmail(contactUs.Phone, contactUs.Email);
            bool accountExist = false;
            Entity lead = new Entity("lead");
            lead.Attributes.Add("subject", "ליד חדש");
            lead.Attributes.Add("firstname", contactUs.FirstName);
            lead.Attributes.Add("lastname", contactUs.LastName);
            lead.Attributes.Add("emailaddress1", contactUs.Email);
            lead.Attributes.Add("mobilephone", contactUs.Phone);
            lead.Attributes.Add("roe_typeturn",new OptionSetValue((int)contactUs.LeadType));
            if (account != Guid.Empty)
            {
                lead.Attributes.Add("parentaccountid", new EntityReference("account", account));
                accountExist= true;
            }
            lead.Id= service.Create(lead);
            return (lead,accountExist);
        }
        private Guid FindAccountByPhoneOrEmail(string phone, string email)
        {
            Guid accountId = Guid.Empty;
            try
            {
                QueryExpression query = new QueryExpression("account");
                FilterExpression filter = new FilterExpression(LogicalOperator.Or);
                filter.AddCondition(new ConditionExpression("telephone1", ConditionOperator.Equal, phone));
                filter.AddCondition(new ConditionExpression("emailaddress1", ConditionOperator.Equal, email));
                query.Criteria.AddFilter(filter);
                query.TopCount = 1;
                EntityCollection ec = service.RetrieveMultiple(query);
                if (ec!=null&& ec.Entities.Count > 0)
                {
                    accountId = ec.Entities[0].Id;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving account record", ex);
            }
            return accountId;
        }

        private EntityReference ConfirmLead(Entity lead, bool createAccount, bool createOpportunity)
        {

            QualifyLeadRequest qualifyRequest = new QualifyLeadRequest()
            {
                CreateContact = createAccount,
                CreateOpportunity = createOpportunity,
                Status = new OptionSetValue(3),
                LeadId = new EntityReference("lead", lead.Id)
            };
            QualifyLeadResponse qualifyResponse = (QualifyLeadResponse)service.Execute(qualifyRequest);
            EntityReference refObj = qualifyResponse.CreatedEntities.FirstOrDefault();
            if (refObj != null && refObj.LogicalName == "opportunity")
                return refObj;
            else if (refObj != null && refObj.LogicalName == "contact")
            {
                return CreateNewAccount(refObj.Id, lead);
            }
            else if (!createAccount && refObj is null) 
            {
                return (EntityReference)lead.Attributes["parentaccountid"];
            }
            return null;
        }


        private Guid CreateNewAsset(ContactUsDataModel contactUs, EntityReference assetOwner)
        {
            Entity asset = new Entity("roe_assets");
            asset.Attributes.Add("roe_name", contactUs.TitleOfAsset);
            asset.Attributes.Add("roe_owner", assetOwner);
            asset.Attributes.Add("roe_priceofamount",new Money(contactUs.AssetPrice));
            asset.Attributes.Add("roe_dealtype", new OptionSetValue((int)contactUs.DealType));
            Entity cityEntity = FindCityRefByName(contactUs.City);
            if (cityEntity != null)
            {
                asset.Attributes.Add("roe_citiesandsat", new EntityReference("roe_citiesandsat", cityEntity.Id));     //entity ref
                asset.Attributes.Add("roe_areaid", (EntityReference)cityEntity.Attributes["roe_area"]);    //entity ref
            }
            asset.Attributes.Add("roe_address", contactUs.FullAddress);
            asset.Attributes.Add("roe_numburrooms", contactUs.NumburRooms);
            return service.Create(asset);
        }

        private Entity FindCityRefByName(string name)
        {
            Entity cityEntity = null;
            try
            {
                QueryExpression query = new QueryExpression("roe_citiesandsat");
                query.Criteria.AddCondition(new ConditionExpression("roe_name", ConditionOperator.Equal, name));
                query.TopCount = 1;
                query.ColumnSet = new ColumnSet("roe_area");
                EntityCollection ec = service.RetrieveMultiple(query);
                if (ec != null && ec.Entities.Count > 0)
                {
                    cityEntity = ec.Entities[0];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving account record", ex);
            }
            return cityEntity;
        }

        private EntityReference CreateNewAccount(Guid contactId,Entity lead)
        {
            Entity account = new Entity("account");
            account.Attributes.Add("name", lead.Attributes["firstname"]+" "+lead.Attributes["lastname"]);
            account.Attributes.Add("primarycontactid", new EntityReference("contact",contactId));
            return new EntityReference("account", service.Create(account));
        }
    }
}
