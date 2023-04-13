function createVisit(fromContext) {
        //    alert("js runing!");
        var asset = fromContext.getAttribute("roe_asset").getValue();
        var contact = fromContext.getAttribute("parentcontactid").getValue();
        var datetimeofvisit = fromContext.getAttribute("roe_datetimeofvisit").getValue();
        alert(asset +" "+contact+" "+datetimeofvisit);
        if (asset&& contact&& datetimeofvisit) {
           
            fromContext.ui.clearFormNotification();
           // connectToEnviroment();
           // callToActionWithWebApiRequest(asset[0].id,contact[0].id,datetimeofvisit,"defult velue");
            callToActionWithHttpRequest(asset[0].id,contact[0].id,datetimeofvisit,"defult velue");

        }
        else{
            fromContext.ui.clearFormNotification();
            fromContext.ui.setFormNotification("To schedule a visit, fields must be filled: asset, contact, date of visit.", "ERROR");
        }    
}


function connectToEnviroment(){
var url = "https://org8f386aa3.crm4.dynamics.com/";

// Specify your Dynamics 365 user credentials
var username = "crmadmin@Comblack850.onmicrosoft.com";
var password = "Aa123456!";

// Use OAuth2 authentication to connect to your Dynamics 365 environment
var authOptions = {
    "url": url,
    "clientId": "771ecd09-bd75-4fad-950b-6f35d3aa10b1",
    "redirectUri": "https://org8f386aa3.crm4.dynamics.com/",
    "login": username,
    "pass": password
};
Xrm.WebApi.online.authenticate(authOptions).then(
    function () {
        console.log("Connection successful");
    },
    function (error) {
        console.log("Connection failed: " + error.message);
    }
);

}

function callToActionWithHttpRequest(assetId,contactId,fullDate,defultVelue){
        var parameters = {};
        parameters.contactRef = { "@odata.type": "Microsoft.Dynamics.CRM.contact", contactid : contactId }; // mscrm.contact
        parameters.name = defultVelue; // Edm.String
        parameters.phone = defultVelue; // Edm.String
        parameters.email = defultVelue; // Edm.String
        parameters.date = new Date(fullDate).toISOString(); // Edm.DateTimeOffset
        parameters.assetRef = { "@odata.type": "Microsoft.Dynamics.CRM.roe_assets", roe_assetsid : assetId }; // mscrm.roe_assets

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
                    fromContext.ui.setFormNotification("Visit successfully created.", "INFO");
                    var visitref = result["visitRef"]; // mscrm.roe_visitinasset
                } else {
                    fromContext.ui.setFormNotification("Opps somthing worng.", "ERROR");
                    console.log(this.responseText);
                }
            }
        };
        req.send(JSON.stringify(parameters));
}

function callToActionWithWebApiRequest(assetId,contactId,date,defultVelue) {
    var execute_roe__Create_Visit_In_Asset_Request = {
        // Parameters
        contactRef: { "@odata.type": "Microsoft.Dynamics.CRM.contact", contactid : contactId }, // mscrm.contact
        name: defultVelue, // Edm.String
        phone: defultVelue, // Edm.String
        email: defultVelue, // Edm.String
        date: new Date(date).toISOString(), // Edm.DateTimeOffset
        assetRef: { "@odata.type": "Microsoft.Dynamics.CRM.roe_assets", roe_assetsid : assetId }, // mscrm.roe_assets
    
        getMetadata: function () {
            return {
                boundParameter: null,
                parameterTypes: {
                    contactRef: { typeName: "mscrm.contact", structuralProperty: 5 },
                    name: { typeName: "Edm.String", structuralProperty: 1 },
                    phone: { typeName: "Edm.String", structuralProperty: 1 },
                    email: { typeName: "Edm.String", structuralProperty: 1 },
                    date: { typeName: "Edm.DateTimeOffset", structuralProperty: 1 },
                    assetRef: { typeName: "mscrm.roe_assets", structuralProperty: 5 }
                },
                operationType: 0, operationName: "roe__Create_Visit_In_Asset"
            };
        }
    };
    
    Xrm.WebApi.execute(execute_roe__Create_Visit_In_Asset_Request).then(
        function success(response) {
            if (response.ok) { return response.json(); }
        }
    ).then(function (responseBody) {
        var result = responseBody;
        console.log(result);
        // Return Type: mscrm.roe_visitinasset
    }).catch(function (error) {
        console.log("basa");
        console.log(error.message);
    });
}




