
function onLoad(executionContext) {
	let formContext = executionContext.getFormContext();   //get all context
	showFeildByChoice(formContext);
	formContext.getAttribute("roe_dealtype").addOnChange(             //get deal type, in events
	  (executionContext) => {
		let formContext = executionContext.getFormContext();         
		showFeildByChoice(formContext);		   
	  }
	);

	//secound task
	formContext.getAttribute("roe_citiesandsat").addOnChange(
		(executionContext)=>{
			let fromContext=executionContext.getFormContext();
			let feildCityAcsses=fromContext.getAttribute("roe_citiesandsat");
		    let feildAreaControl=fromContext.getControl("roe_areaid");
			let city=feildCityAcsses.getValue(); 
			let areaValue=fromContext.getAttribute("roe_areaid").getValue();  

			if(city && !areaValue){
				feildCityAcsses.setValue(null); 
				feildAreaControl.setNotification("יש למלא איזור קודם");
			}
			else{
				feildAreaControl.clearNotification();
			}			
		}
  );

  formContext.getAttribute("roe_address").addOnChange(async (executionContext) => {
	let fromContext = executionContext.getFormContext();
	var address = fromContext.getAttribute("roe_address").getValue();
	var linkToGoogleMaps = await getGoogleMapsLink(address);
	if (linkToGoogleMaps) {
	  fromContext.getAttribute("roe_linkgooglemaps").setValue(linkToGoogleMaps);
	}
  });
}


async function getGoogleMapsLink(address) {
	try {
	  const bingMapsUrl = 'https://dev.virtualearth.net/REST/v1/Locations?q=' + encodeURIComponent(address) + '&key=Ambf01b9cxK38kENR2doOxGoThngcPlHlqdHgOtPvpzNcvYJkznnjNRs5ZEVq13T';
	  const response = await fetch(bingMapsUrl);
	  const data = await response.json();
	  const coordinates = data.resourceSets[0].resources[0].point.coordinates;
	  const googleMapsUrl = 'https://www.google.com/maps/search/?api=1&query=' + coordinates[0] + ',' + coordinates[1];
	  return googleMapsUrl;
	} catch (error) {
	  console.log('Error fetching coordinates from Bing Maps API:', error);
	  return null;
	}
  }


function showFeildByChoice(formContext){
	let codeDealType = formContext.getAttribute("roe_dealtype").getValue();
	let feildPriceControle=formContext.getControl("roe_priceofamount");
	let feildMonthlyPriceControle = formContext.getControl("roe_monthlypay");
	if(codeDealType===913200001){    //השכרה
		feildPriceControle.setVisible(false);
		feildMonthlyPriceControle.setVisible(true);
	}
	else{
		feildPriceControle.setVisible(true);
		feildMonthlyPriceControle.setVisible(false);
	}
}