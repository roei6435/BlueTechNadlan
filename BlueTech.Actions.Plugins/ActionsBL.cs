using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace BlueTech.Actions.Plugins
{
    public class ActionsBL
    {
        private readonly IOrganizationService service;
        private readonly ITracingService tracingService;

        public ActionsBL (IOrganizationService service, ITracingService tracingService=null)
        {
            this.service = service;
            this.tracingService = tracingService;
        }
        public ActionsBL(IOrganizationService service)
        {
            this.service = service;
        }


        public EntityReference CreateVisitFromBL(string name, string phone, string email, DateTime ?date, EntityReference assetRef, EntityReference visitorRef)
        {
            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(email) && date != null && assetRef != null)
            {
                if (visitorRef is null)
                {

                    // Find visitor by contact
                    visitorRef = FindContactByPhoneOrMail(phone, email, service);

                    //2. If not found contact, find visitor by lead
                    if (visitorRef is null)
                        visitorRef = FindLeadByPhoneOrMail(phone, email, service);

                    //3. If not found lead, be create lead, and this lead will be visitor
                    if (visitorRef is null)
                        visitorRef = CreateNewLead(name, phone, email, service);

                }

                //4. Now can create visit in asset
                Guid visitId = CreateNewVisit(assetRef, visitorRef, (DateTime)date, service);

                //5. Create visit in asset
                return new EntityReference("roe_visitinasset", visitId);

            }
            else
            {
                throw new Exception("No required fields were received");

            }
        }

        public EntityReference FindContactByPhoneOrMail(string phone, string email, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet(false);

            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition("telephone1", ConditionOperator.Equal, phone);
            filter.AddCondition("emailaddress1", ConditionOperator.Equal, email);


            query.Criteria.AddFilter(filter);

            Entity contact = service.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (contact is null) return null;
            return new EntityReference("contact", contact.Id);

        }
        public EntityReference FindLeadByPhoneOrMail(string phone, string email, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("lead");
            query.ColumnSet = new ColumnSet(false);

            FilterExpression filter1 = new FilterExpression(LogicalOperator.Or);
            filter1.AddCondition("telephone1", ConditionOperator.Equal, phone);
            filter1.AddCondition("emailaddress1", ConditionOperator.Equal, email);

            query.Criteria.AddFilter(filter1);

            Entity lead = service.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (lead != null) return new EntityReference("lead", lead.Id);   //found lead
            return null;
        }
        public EntityReference CreateNewLead(string name, string phone, string email, IOrganizationService service)
        {
            Entity lead = new Entity("lead");
            lead.Attributes.Add("firstname", name.Split(' ')[0]);
            lead.Attributes.Add("lastname", name.Split(' ')[1]);
            lead.Attributes.Add("emailaddress1", email);
            lead.Attributes.Add("mobilephone", phone);
            lead.Id = service.Create(lead);
            return new EntityReference("lead", lead.Id);
        }
        public Guid CreateNewVisit(EntityReference assetRef, EntityReference vistor, DateTime date, IOrganizationService service)
        {
            Entity newVisit = new Entity("roe_visitinasset");
            newVisit.Attributes.Add("roe_asset", assetRef);
            newVisit.Attributes.Add("regardingobjectid", vistor);
            newVisit.Attributes.Add("roe_dateandtime", date);
            return service.Create(newVisit);
        }
    }
}
