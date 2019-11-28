// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var slideDisabled = false;
function toggleNav() {
    if (slideDisabled) {
        return;
    }
    if (document.getElementById("mySidenav").style.width < "350px") {
        document.getElementById("mySidenav").style.width = "350px";
        document.getElementById("sideRemnant").style.marginLeft = "350px";
        document.getElementById("navArrow").innerHTML = "&#11164;"
    }
    else {
        document.getElementById("mySidenav").style.width = "0px";
        document.getElementById("sideRemnant").style.marginLeft = "0px";
        document.getElementById("navArrow").innerHTML = "&#11166;"
    }
    slideDisabled = true;
    setTimeout(function () { slideDisabled = false; }, 300);
};