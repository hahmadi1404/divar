
var c=false;

setInterval(() => {
    if(c == false){
    if($(".form").hasClass("ui") ){
        c=true;
        $(".item-node:contains('خدمات')").click();
        setTimeout(() => {
            $(".item-leaf:contains('حمل و نقل')").click();
            setTimeout(() => {
              ///  var el=$(".location-selector__district input" );
              //  el.click();
                setTimeout(() => {
                   
                //    $(".item:contains('شهرک ولیعصر')").click();
                    setTimeout(() => {
                   //     var inputelement=$(".text-field:contains('عنوان')+div+div div div input");
                    
                    //inputelement.focus().val("گاو صندوق");
                    //var elll=document.getElementsByClassName("form-control")[0];

               

                   // $("#root_description").val("aaaaaaaaaaaaaaa");
                    $(".image-uploader__dropzone i").click();
                    $(".image-uploader__dropzone").click();
                    },500);
                    
                    

                }, 500);
                
             }, 500);
        }, 100);
       
    }
}
 
}, 100);
