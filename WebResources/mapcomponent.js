
function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();

    // קבלת הכתובת מהשדה בטופס
    var addressField = formContext.getAttribute("new_addressfield").getValue();
    var address = addressField.toString();

    // פנייה ל-Web API של Bing Maps כדי לקבל את המיקום של הכתובת במפה
    var query = "https://dev.virtualearth.net/REST/v1/Locations?q=" + address + "&key=Ambf01b9cxK38kENR2doOxGoThngcPlHlqdHgOtPvpzNcvYJkznnjNRs5ZEVq13T";
    var req = new XMLHttpRequest();
    req.open("GET", query, true);
    req.onreadystatechange = function() {
        if (req.readyState == 4 && req.status == 200) {
            var response = JSON.parse(req.responseText);
            // קבלת המיקום של הכתובת מהתשובה של Web API
            var coordinates = response.resourceSets[0].resources[0].point.coordinates;

            // יצירת קומפוננטת Div עם המיקום במפה
            var mapDiv = document.createElement("div");
            mapDiv.style.height = "400px"; // גובה המפה בפיקסלים
            mapDiv.style.width = "100%"; // רוחב המפה בפיקסלים

            // יצירת אובייקט מפה ב- Bing Maps והוספתו לקומפוננטת Div
            var map = new Microsoft.Maps.Map(mapDiv, {
                credentials: "YOUR_BING_MAPS_KEY",
                center: new Microsoft.Maps.Location(coordinates[0], coordinates[1]),
                zoom: 10 // רמת הזום של המפה
            });

            // הוספת הקומפוננטה לטופס
            var formContainer = formContext.ui.controls.get("header_process_" + formContext.data.process.getActiveProcess().getId() + "_container");
            formContainer.addCustomView("mapView", mapDiv.outerHTML);
        }
    };
    req.send();
}
