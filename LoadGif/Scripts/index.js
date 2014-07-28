var down = false;
var mouse_x;
var interval;
var IE = document.all ? true : false

if (!IE) document.captureEvents(Event.MOUSEMOVE)
document.onmousemove = get_mouse_x;

$(function () {
    $("#div3").click(function () {
        var width1 = $("#div1").width();
        if (width1 != 20) {
            var width = $("#div4").width() - 20;
            $("#div1").css("width", "20px");
            $("#div2").css("width", width + "px");
        }
        else {
            $("#div1").css("width", "50%");
            $("#div2").css("width", "50%");
        }
        
    });
    
});

function get_mouse_x(e) {
    if (IE)
        mouse_x = event.clientX;
    else
        mouse_x = e.pageX;
}

function drag() {
    interval = setInterval("update()", 1);
}

function release() {
    clearInterval(interval);
}

function update() {
    $("#d1").html(mouse_x - document.getElementById('bar1').offsetLeft);
    $("#d2").html(mouse_x);
    //$("#d3").html(document.getElementById('bar1').offsetWidth);
    $("#d4").html(document.getElementById('bar2').offsetWidth);
    if ((mouse_x - document.getElementById('bar1').offsetLeft) >= document.getElementById('bar1').offsetWidth+document.getElementById('bar2').offsetWidth) {
        release();
        $("#d3").html(11);
        document.getElementById('bar1').style.width = document.getElementById('bar1').offsetWidth + document.getElementById('bar2').offsetWidth + "px";
        document.getElementById('bar2').style.width = "0px";
    } else if (mouse_x <= document.getElementById('bar1').offsetLeft) {
        release();
        $("#d3").html(22);
        document.getElementById('bar1').style.width = document.getElementById('bar1').offsetLeft + "px";
        document.getElementById('bar2').style.width = document.getElementById('bar2').offsetWidth + document.getElementById('bar1').offsetWidth + "px";
    } else {
        $("#d3").html((document.getElementById('bar1').offsetWidth + document.getElementById('bar2').offsetWidth - (mouse_x - document.getElementById('bar1').offsetLeft)));
        
        document.getElementById('bar2').style.width = (document.getElementById('bar1').offsetWidth + document.getElementById('bar2').offsetWidth - (mouse_x - document.getElementById('bar1').offsetLeft)) + "px";
        document.getElementById('bar1').style.width = (mouse_x - document.getElementById('bar1').offsetLeft) + "px";
        document.getElementById('bar2').style.marginLeft = document.getElementById('bar1').style.width;
    }
}