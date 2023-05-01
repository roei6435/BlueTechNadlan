function createVisit(fromContext) {
    var asset = fromContext.getAttribute("roe_asset").getValue();
    var contact = fromContext.getAttribute("parentcontactid").getValue();
    var datetimeofvisit = fromContext.getAttribute("roe_datetimeofvisit").getValue();
    if (asset&& contact&& datetimeofvisit) {
           
        callToActionWithHttpRequest(asset[0].id, contact[0].id, datetimeofvisit, "defult velue", function(success) {
            if (success) {
                fromContext.ui.setFormNotification("The visit was created successfully.", "INFO");
            } else {
                fromContext.ui.setFormNotification("Oops something went wrong.", "ERROR");
            }
        });
    }
    else{
        fromContext.ui.setFormNotification("To schedule a visit, fields must be filled: asset, contact, date of visit.", "ERROR");
    } 
    fromContext.ui.clearFormNotification();
}




function callToActionWithHttpRequest(assetId, contactId, fullDate, defultVelue, callback) {

    //Put values in all fields required to run the action
    var parameters = {};
    parameters.contactRef = { "@odata.type": "Microsoft.Dynamics.CRM.contact", contactid : contactId };   //entity reference
    parameters.name = defultVelue; 
    parameters.phone = defultVelue;                                                                         //string
    parameters.email = defultVelue; 
    parameters.date = new Date(fullDate).toISOString();                                                      //date
    parameters.assetRef = { "@odata.type": "Microsoft.Dynamics.CRM.roe_assets", roe_assetsid : assetId };

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/roe__Create_Visit_In_Asset", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Accept", "application/json");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200 || this.status === 204) {
                var result = JSON.parse(this.response);
                console.log(result);                  
                var visitref = result["visitRef"];
                callback(true);
            } else {
                console.log(this.responseText);
                callback(false);
            }
        }
    };
    req.send(JSON.stringify(parameters));
}




