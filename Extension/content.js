

var elll=document.getElementsByClassName("gLFyf")[0];

elll.focus();
var keyboardEvent = document.createEvent("Events");
keyboardEvent.initEvent("keydown", true, true);
keyboardEvent.keyCode = keyboardEvent.which = 65; // Backspace
elll.dispatchEvent(keyboardEvent);

keyboardEvent = document.createEvent("Events");
keyboardEvent.initEvent("keypress", true, true);
keyboardEvent.keyCode = keyboardEvent.which = 65; // Backspace

elll.dispatchEvent(keyboardEvent);
keyboardEvent = document.createEvent("Events");
keyboardEvent.initEvent("keyup", true, true);
keyboardEvent.keyCode = keyboardEvent.which = 64; // Backspace
elll.dispatchEvent(keyboardEvent);
$(".gLFyf").trigger("keypress",{which:65});
var c=false;
chrome.runtime.sendMessage({ pressEnter: true });
setInterval(() => {
    if(c===false){
    if($(".form").hasClass("ui") ){
        c=true;
        $(".item-node:contains('خدمات')").click();
        setTimeout(() => {
            $(".item-leaf:contains('حمل و نقل')").click();
            setTimeout(() => {
                var el=$(".location-selector__district input" );
                el.click();
                setTimeout(() => {
                    $(".item:contains('شهرک ولیعصر')").click();
                    
                    var inputelement=$(".text-field:contains('عنوان')+div+div div div input");
                    
                    inputelement.focus().val("گاو صندوق");
                    var elll=document.getElementsByClassName("form-control")[0];

                    elll.focus();
                    var keyboardEvent = document.createEvent("Events");
                    keyboardEvent.initEvent("keydown", true, true);
                    keyboardEvent.keyCode = keyboardEvent.which = 8; // Backspace
                    elll.dispatchEvent(keyboardEvent);
                    
                    keyboardEvent = document.createEvent("Events");
                    keyboardEvent.initEvent("keypress", true, true);
                    keyboardEvent.keyCode = keyboardEvent.which = 8; // Backspace
                    
                    elll.dispatchEvent(keyboardEvent);
                    keyboardEvent = document.createEvent("Events");
                    keyboardEvent.initEvent("keyup", true, true);
                    keyboardEvent.keyCode = keyboardEvent.which = 8; // Backspace
                    elll.dispatchEvent(keyboardEvent);

            
              
                    $("#root_description").val("aaaaaaaaaaaaaaa");

                }, 500);
                
             }, 500);
        }, 100);
       
    }
}
 
}, 100);
