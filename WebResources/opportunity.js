function onLoad(executionContext) {
	
	    //alert("js run on form!");
        let formContext = executionContext.getFormContext();
 //----------------------First task - chack amount of open visits by contact -----------------------------------------------------------
        let contactId = getIdObjectByFeildName(formContext,"parentcontactid");

        // If contact ID exists, call getAmountVisitsByContactId function and display the count in alert
        if (contactId) {
            getAmountVisitsByContactId(contactId).then(function(count) {
                if(count > 0) {
                    formContext.ui.setFormNotification(`כעת ישנם ${count} ביקורים פתוחים עבורו`, "INFO");
                }
            });
        }
    
        // Add on change event to the contact lookup field
        let contactLookup = formContext.getAttribute("parentcontactid");
        if (contactLookup) {
          contactLookup.addOnChange(async function() {
            formContext.ui.clearFormNotification();
            let contactId = getIdObjectByFeildName(formContext,"parentcontactid");
            if (contactId) {
              try {
                let count = await getAmountVisitsByContactId(contactId);
                if(count != null && count > 0) {
                    formContext.ui.setFormNotification(`כעת ישנם ${count} ביקורים פתוחים עבורו`, "INFO");
                }
              } catch (error) {
                console.log(error);
              }
            }
          });
        }
//---------------------- Third task - get the link to google maps by asset and open in new window -----------------------------------------------------------       
	
        let assetLookup = formContext.getAttribute("roe_asset");
        if (assetLookup) {
        assetLookup.addOnChange(async function() {
        let assetId = getIdObjectByFeildName(formContext, "roe_asset");
        if (assetId) {
            let link = await getGoogleMapsLinkByAssetId(assetId);
            if(link) {
                window.open(link, "_blank");
            }
                
        }
        });
    }


}


//get id object by feild(lookup)
function getIdObjectByFeildName(formContext,nameFeild) {
    let objectId = null;
    let feildLookup = formContext.getAttribute(nameFeild);
    if (feildLookup && feildLookup.getValue() && feildLookup.getValue()[0]) {
        objectId = feildLookup.getValue()[0].id;
    }
    return objectId;
}


//requests to server

async function getAmountVisitsByContactId(contactId) {    
    let fetchUrl = Xrm.Utility.getGlobalContext().getClientUrl() + 
        `/api/data/v9.2/roe_visitinassets?$select=roe_dateandtime&$filter=(_regardingobjectid_value eq ${contactId} and roe_dateandtime ge ${new Date().toISOString()})&$count=true`;
    
    let fetchOptions = {
        method: "GET",
        headers: {
            "OData-MaxVersion": "4.0",
            "OData-Version": "4.0",
            "Content-Type": "application/json; charset=utf-8",
            "Accept": "application/json",
            "Prefer": "odata.include-annotations=*"
        }
    };
    
    try {
        let response = await fetch(fetchUrl, fetchOptions);
        let json = await response.json();
        if (response.ok) {
            let count = json["@odata.count"];
            return count;
        } else {
            console.log(json.error.message);
            return null;
        }
    } catch (error) {
        console.log(error.message);
        return null;
    }
}

  
async function getGoogleMapsLinkByAssetId(assetId) {
    const response = await fetch(Xrm.Utility.getGlobalContext().getClientUrl() + `/api/data/v9.2/roe_assetses?$select=roe_linkgooglemaps&$filter=roe_assetsid eq ${assetId}`, {
      method: "GET",
      headers: {
        "OData-MaxVersion": "4.0",
        "OData-Version": "4.0",
        "Content-Type": "application/json; charset=utf-8",
        "Accept": "application/json"
      }
    });
  
    if (response.ok) {
      const responseBody = await response.json();
      const results = responseBody;
  
      if (results.value.length > 0) {
        const result = results.value[0];
        const roe_linkgooglemaps = result["roe_linkgooglemaps"]; // Text
  
        return roe_linkgooglemaps || null;
      }
    } else {
      const error = await response.json();
      console.log(error.message);
    }
  
    return null;
  }
  







































