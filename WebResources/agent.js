
function onLoad(executionContext) {
   // alert("check js is runing on form!");
    
	let formContext = executionContext.getFormContext();   

   //call to functions on load agent form.
   checkCloseDeals(formContext);  //task 1
   checkOpenDeals(formContext);
   conntectionFirstAndLastName(formContext);            //task 2

	formContext.getAttribute("roe_firstname").addOnChange(                  //call by events      
	  (executionContext) => {
		let formContext = executionContext.getFormContext();         
		conntectionFirstAndLastName(formContext);		   
	  }
	);

    formContext.getAttribute("roe_lastname").addOnChange(        
    (executionContext) => {
      let formContext = executionContext.getFormContext();         
      conntectionFirstAndLastName(formContext);		   
    }
  );

}

//check the feild and visible/unvisible
function checkCloseDeals(formContext){
    let numberCloseDeals= formContext.getAttribute("roe_qty_deals_total").getValue();
	  let sumOfProfit = formContext.getControl("roe_sum_deals_total");
    if(numberCloseDeals>0){
      sumOfProfit.setVisible(true);   
    }
    else{
      sumOfProfit.setVisible(false); 
    } 
}

function checkOpenDeals(formContext){
    let numburOpenDeals= formContext.getAttribute("roe_qty_deals_open").getValue();
    let sumOfPtofit=formContext.getControl("roe_open_deals_sum");
    if(numburOpenDeals>0){
      sumOfPtofit.setVisible(true);   
    }
    else{
      sumOfPtofit.setVisible(false); 
    }
}

function conntectionFirstAndLastName(formContext){

   let fullName=formContext.getAttribute("roe_name");
   fullName.setRequiredLevel("none");                           //cencel name required feild


   let firstName= formContext.getAttribute("roe_firstname");
   formContext.getControl("roe_firstname").clearNotification();
   firstName.setRequiredLevel("required");
   firstName=firstName.getValue();

   let lastName=formContext.getAttribute("roe_lastname").getValue();
   
   if(firstName&&lastName){
        fullName.setValue(firstName+" "+lastName);
   }
   else if(firstName&&!lastName){
    fullName.setValue(firstName);
   }
   else if(!firstName&&lastName){
    fullName.setValue(lastName);
   }
   else{
    fullName.setValue(null);
    formContext.getControl("roe_firstname").setNotification("יש למלא לפחות שם פרטי.")          //notification for user
   }
}





