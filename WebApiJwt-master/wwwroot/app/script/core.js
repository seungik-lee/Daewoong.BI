getPagesByUser();
function getPagesByUser() {
    $.getJSON('/api/page', function (data) {
        console.log(data);
    });
}

function newPage() {
    $("#pageDetail").load("app/template/3by3.html", function () {
    });
}


function getData(url, callback) {
    $.getJSON(url, function (data) {
        callback(data);
    });
}
   