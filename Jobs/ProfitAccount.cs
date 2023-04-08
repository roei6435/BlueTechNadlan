using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Data.Edm.Library.Values;
using Newtonsoft.Json.Linq;

namespace ConsoleAppTest
{
    public class ProfitAccount
    {
        IOrganizationService service;
        public OrganizationServiceContext svcContext = null;
        EntityCollection ec = new EntityCollection();
        public ProfitAccount(IOrganizationService m_service)
        {
            service = m_service;
            svcContext = new OrganizationServiceContext(service);
        }

        //task niv: לרוץ על כל התיקי לקוח ולסכום את שווי הנכסים של כל לקוח
        public void CalculateProfitAccountByAssets()
        {

            EntityCollection accounts = GetAllAccountActivity();
            foreach (Entity account in accounts.Entities)   //לרוץ על כל התיקי לקוחות
            {
                EntityCollection assetsOfThisAccount = GetAllAssetsByAccountId(account.Id);    //להביא את כל הנכסים של כל הלקוח
                if (assetsOfThisAccount.Entities.Count != 0)
                {
                    decimal total = 0;
                    foreach (Entity asset in assetsOfThisAccount.Entities)
                    {
                        //לסכום את כל שווי נכסי הלקוח
                        total += asset.Attributes.Contains("roe_priceofamount") ? ((Money)asset["roe_priceofamount"]).Value : 0;
                    }
                    UpdateFeildSumOfAllAssets(account.Id, total);   //לעדכן שדה מתאים בתיק לקוח
                }

            }

        }

        private void UpdateFeildSumOfAllAssets(Guid accountId, decimal total)
        {
            Entity account = new Entity("account", accountId);
            account["roe_sumofamountassets"] = new Money(total);
            account["roe_dateupdateassets"] = DateTime.Now;
            this.service.Update(account);
            Console.WriteLine("update");

        }

        //-------------------------------------------------------------------------------------------------------------------------------

        //
        //roei //
        //    //   //roei mam.Open().rom().AllAssets.col(decimel out   )
        //task gadi: לרוץ על כל התיקי לקוח ולעדכן בשדות המתאימים כמה אחוז נכסיו מהווים כמותית וכספית
        public void CalculatePrecentFromAllTheCompany()
        {

            EntityCollection collaction = GetAllAssetsActivity();
            int countAssets = collaction.Entities.Count();             // להביא את כמות כל הנכסים ואת שווים
            decimal sumOfvalues = 0;
            foreach (Entity asset in collaction.Entities)
            {
                try
                {
                    sumOfvalues += asset.Attributes.Contains("roe_priceofamount") ? ((Money)asset["roe_priceofamount"]).Value : 0;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
               
            }

            collaction = GetAllAccountActivity();
            foreach (Entity account in collaction.Entities)
            {
                decimal precentInCount, precentInValue = 0;
                EntityCollection allAssetsOfThisAccount = GetAllAssetsByAccountId(account.Id);
                if (allAssetsOfThisAccount.Entities.Count > 0)
                {
                    precentInCount = allAssetsOfThisAccount.Entities.Count / (decimal)countAssets * 100;
                    foreach (Entity asset in allAssetsOfThisAccount.Entities)
                    {
                        
                        precentInValue += asset.Attributes.Contains("roe_priceofamount") ? ((Money)asset["roe_priceofamount"]).Value : 0;
                    }
                    precentInValue = precentInValue / sumOfvalues * 100;
                    UpdateFeildsPercentsCountAndValue(account.Id, precentInValue, precentInCount);
                    // Console.WriteLine($"For account: {account.Id} \nprecent from count {precentInCount}% , precent from value {precentInValue}%\n--------------------------------");
                }
            }

        }

        private EntityCollection GetAllAccountActivity()
        {
            QueryExpression query = new QueryExpression("account");
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            return this.service.RetrieveMultiple(query);

        }
        private EntityCollection GetAllAssetsActivity()
        {
            QueryExpression query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.ColumnSet = new ColumnSet("roe_priceofamount", "roe_name");
            return this.service.RetrieveMultiple(query);

        }

        private EntityCollection GetAllAssetsByAccountId(Guid accountId)
        {
            QueryExpression query = new QueryExpression("roe_assets");
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.Criteria.AddCondition(new ConditionExpression("roe_owner", ConditionOperator.Equal, accountId));
            query.ColumnSet = new ColumnSet("roe_priceofamount", "roe_name");
            return this.service.RetrieveMultiple(query);
        }


        private void UpdateFeildsPercentsCountAndValue(Guid accountId, decimal precentValue, decimal precentCount)
        {
            Entity account = new Entity("account", accountId);
            account["roe_percentincount"] = precentCount;
            account["roe_percentinvalue"] = precentValue;
            this.service.Update(account);
            Console.WriteLine("update");
        }











        //-------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {

            GetAllAssets(service, out Dictionary<Guid, List<Entity>> assetAccountsDic, out decimal totalAssetAmount, out int totalAssetsCount);

            foreach (var accId in assetAccountsDic.Keys)                //עבור כל מפתח במילון שהוא תז לקוח
            {
                try
                {
                    var assetList = assetAccountsDic[accId];                    //לקחת את הליסט נכסים שלו
                    Entity accountToUpdate = SetAssetPercentageCount(totalAssetsCount, assetList.Count, accId);     //מקבל ה
                    decimal accountsAssetsValue = 0;
                    foreach (var asset in assetList)
                    {
                        var assetvalue = asset.Attributes.Contains("sa_asset_amount") ? ((Money)asset["sa_asset_amount"]).Value : 0;
                        accountsAssetsValue += assetvalue;
                    }
                    SetAccountAssetsPercentageValue(totalAssetAmount, accountsAssetsValue, accountToUpdate);
                }
                catch (Exception ex)
                {
                    //UseLog;
                }
            }
        }

        private void GetAllAssets(IOrganizationService service, out Dictionary<Guid, List<Entity>> assetAccountsDic, out decimal totalAssetAmount, out int totalAssetsCount)
        {
            totalAssetAmount = 0;
            QueryExpression query = new QueryExpression("sa_asset");
            query.ColumnSet = new ColumnSet("sa_asset_amount", "sa_account");
            EntityCollection allAssets = service.RetrieveMultiple(query);     //להביא את כל הנכסים במערכת
            totalAssetsCount = allAssets.Entities.Count;
            assetAccountsDic = new Dictionary<Guid, List<Entity>>();        //לייצר מיליון: מפתח-תז לקוח| ערך-ליסט של הנכסים של הלקוח
            foreach (Entity asset in allAssets.Entities)                //ריצה על כל הנכסים
            {
                try
                {
                    if (asset.Attributes.Contains("sa_asset_amount") && asset.Attributes.Contains("sa_account"))
                    {
                        totalAssetAmount += ((Money)asset["sa_asset_amount"]).Value;            //לסכום את שווי כל הנכסים
                        EntityReference accId = (EntityReference)asset["sa_account"];           //תז לקוח
                        if (assetAccountsDic.ContainsKey(accId.Id))                         //אם המילון מכיל כבר את תז לקוח הזה
                        {
                            assetAccountsDic[accId.Id].Add(asset);          //להוסיף לליסט נכסים שלו עוד נכס
                        }
                        else
                        {
                            assetAccountsDic.Add(accId.Id, new List<Entity>());         //אחרת לייצר ליסט שיקושר לתז לקוח
                            assetAccountsDic[accId.Id].Add(asset);              //להוסיף לליסט נכסים שלו עוד נכס
                        }
                    }
                }
                catch (Exception ex)
                {

                    //log
                }
            }

        }







        private void SetAccountAssetsPercentageValue(decimal totalAssetAmount, decimal accountsAssetsValue, Entity accountToUpdate)
        {
            var percentage = accountsAssetsValue / totalAssetAmount * 100;
            accountToUpdate.Attributes.Add("", percentage);
            service.Update(accountToUpdate);
        }

        private Entity SetAssetPercentageCount(int accountAssets, int allAsets, Guid accId)
        {

            //מקבל כמה נכסים משויכם ללקוח, כמה נכסים יש במערכת, ותז של לקוח 
            var percentage = accountAssets / allAsets * 100;
            Entity accountToUpdate = new Entity("account", accId);
            accountToUpdate.Attributes.Add("", percentage);             //להוסיף שדה ולעשות השמה לערך המחושב
            return accountToUpdate;                 //להחזיר תיישות

        }

     




    }
}