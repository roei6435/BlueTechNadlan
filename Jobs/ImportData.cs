using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using OfficeDevPnP.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Entity = Microsoft.Xrm.Sdk.Entity;

namespace ConsoleApplication1
{
    class ImportData
    {
        IOrganizationService service;
        public OrganizationServiceContext svcContext = null;
        public int updated = 0, created = 0, failed = 0;
        string inputFile = @"C:\Users\roei6\Downloads\assets.csv";
        public ImportData(IOrganizationService m_service)
        {
            service = m_service;
            svcContext = new OrganizationServiceContext(service);
        }
       
        private DataTable LoadDataFromFile(string inputFile)
        {
            DataTable dt = new DataTable();
            if (System.IO.File.Exists(inputFile))
            {
                StreamReader sr = new StreamReader(inputFile);

                string[] columns = sr.ReadLine().Split(',');

                for (int i = 0; i < (columns.Length); i++)//check 0 was -1
                {
                    dt.Columns.Add(columns[i]);
                }

                string line = null;
                string[] data;

                while ((line = sr.ReadLine()) != null)
                    try
                    {
                        {
                            data = line.Split(',');

                            DataRow dr = dt.NewRow();

                            for (int i = 0; i < (columns.Length); i++)
                            {
                                dr[i] = data[i];
                            }

                            dt.Rows.Add(dr);
                        }
                    }
                    catch (Exception e)
                    {
                        //this.WriteToLog("Error loading to text file ID {0}" + e.ToString() + line.ToString(), true);
                    }
            }
            return dt;
        }

        private Guid FindAccountIdByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Phone number is missing or empty");
            QueryExpression query = new QueryExpression("account");
            query.Criteria.AddCondition(new ConditionExpression("telephone1", ConditionOperator.Equal, phone));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.ColumnSet = new ColumnSet(false);
            EntityCollection res = service.RetrieveMultiple(query);
            if (res.Entities.Count is 0) return Guid.Empty;
            return res.Entities.FirstOrDefault().Id;


        }
        private (Guid,Guid) FindCityAndAreaGuidByNameOfCity(string city)
        {
            //return tuple of guid- city and area connected to the city
            if (string.IsNullOrEmpty(city)) throw new ArgumentException("City name is missing or empty");
            QueryExpression query = new QueryExpression("roe_citiesandsat");
            query.Criteria.AddCondition(new ConditionExpression("roe_name", ConditionOperator.Equal, city));
            query.ColumnSet = new ColumnSet("roe_area");
            EntityCollection res = service.RetrieveMultiple(query);
            if (res.Entities.Count is 0) return (Guid.Empty, Guid.Empty);
            Entity cityEntity = res.Entities.FirstOrDefault();
            if (cityEntity.Contains("roe_area") && cityEntity["roe_area"] is EntityReference areaReference)
            {
                return (cityEntity.Id, areaReference.Id);
            }
            return(Guid.Empty, Guid.Empty); 
        }

        private Entity CheakIfExistAssetWithThisNameByAccount(Guid accId, string assetName)
        {
            // if exist asset with this name of this owner
            QueryExpression query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("roe_owner", ConditionOperator.Equal, accId));
            query.Criteria.AddCondition(new ConditionExpression("roe_name", ConditionOperator.Equal, assetName));
            query.ColumnSet = new ColumnSet(false);
            EntityCollection res = service.RetrieveMultiple(query);
            if (res.Entities.Count is 0) return null;
            return res.Entities.FirstOrDefault();      //else return the asset entity
        }

        private Entity CreateOrUpdateAsset(Entity asset,string assetName,decimal price,int dealCode,DateTime availableOn,Guid city,Guid area, Guid accId)
        {
            if(asset is null) asset = new Entity("roe_assets");  
            asset["roe_name"] = assetName;
            asset["roe_owner"] = new EntityReference("account", accId);
            asset["roe_citiesandsat"] = new EntityReference("roe_citiesandsat", city);
            asset["roe_areaid"] = new EntityReference("roe_areainmap", area);
            asset["roe_priceofamount"] = new Money(price);
            asset["roe_dealtype"] = new OptionSetValue(dealCode);
            asset["roe_availableon"] = availableOn;
            return asset;

        }

        public void ImportAllAssetFromFileToSystem()
        {
            DataTable sourceData = LoadDataFromFile(inputFile);
            for (int i = 0; i < sourceData.Rows.Count; i++)
            {
                string assetName; decimal price; int dealCode; DateTime createdOn; Guid accountId;
                try
                {
                    //Check if exist account with this phone
                    accountId = FindAccountIdByPhone(sourceData.Rows[i][1].ToString());
                    if (accountId == Guid.Empty) throw new Exception($"account with phone {sourceData.Rows[i][1]} not found");


                    //Import the city by name, and the area it is associated 
                    (Guid, Guid) cityAndArea = FindCityAndAreaGuidByNameOfCity(sourceData.Rows[i][5].ToString());
                    if (cityAndArea.Item1 == Guid.Empty && cityAndArea.Item2 == Guid.Empty)
                        throw new Exception($"The city is not proper");

                    //The other simple fields
                    assetName = sourceData.Rows[i][0].ToString();
                    price = decimal.Parse(sourceData.Rows[i][2].ToString());
                    dealCode = int.Parse(sourceData.Rows[i][3].ToString());
                    createdOn = DateTime.Parse(sourceData.Rows[i][4].ToString());

                    //Get the asset if she exist, if not the function will create the object
                    Entity asset= CheakIfExistAssetWithThisNameByAccount(accountId, assetName);


                    //Flag marked me update or create
                    bool assetExist = (asset != null);     //if asset=null assetExist=false else assetExist=true
                                                           
                    //Create or update the entity and call to service
                    asset =CreateOrUpdateAsset(asset,assetName, price, dealCode, createdOn,cityAndArea.Item1,cityAndArea.Item2, accountId);
                    if (assetExist)
                    {
                        service.Update(asset);
                        updated++;
                    }
                    else{
                        service.Create(asset);
                        created++; 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error:{ex.Message}");
                    failed++;
                }
            }
            Console.WriteLine($"All items in file: {sourceData.Rows.Count}\nSucssesfully updated: {updated}.\nSucssesfully created: {created}\nFailed: {failed}");
            Console.ReadKey();
        }




    }
}
