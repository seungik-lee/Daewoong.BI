$(document).ready(function () {

    loadHeader("../menu.html");
    // var url = getParameterByName('url');
    var urlTemp = location.search;

    var newUrlS = urlTemp.lastIndexOf("userid=");
    if (newUrlS === -1) {
        var aTemp = location.search.lastIndexOf("jsp?");
        var root = location.search.substring(5, aTemp + 4);
        var params = location.search.substring(aTemp + 4);
      //  params = encodeURIComponent(params);
        $("#content").attr("src", root + params);
    }
    else {
        var key = urlTemp.substring(newUrlS + 7);
        var newURl = urlTemp.substring(5, newUrlS + 7) + encodeURIComponent(key);
        $("#content").attr("src", newURl);
    }
    
});