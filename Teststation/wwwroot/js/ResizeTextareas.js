$('textarea').each(function () {
    var element = this;
    if (element.scrollHeight > element.clientHeight) {
        element.style.cssText = 'padding:8px;';
        element.style.cssText = 'height:' + element.scrollHeight + 'px !important';
    }    
});