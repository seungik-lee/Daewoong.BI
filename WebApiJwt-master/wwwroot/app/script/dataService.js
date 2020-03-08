function getHost() {
    return location.protocol + "//" + location.host + "/";
}
var apiService = getHost();
var userToken;

var companyEP = "/api/company";
var categoryEP = "/api/category";
var kpiEP = "/api/KPI";
var settingEP = "/api/setting";
var userEP = "/api/user";
var registerEP = "/account/Register";
var pageEP = "/api/page";


var chartTypes = [
    { type: 0, title: "미지정" },
    { type: 1, title: "가로바" },
    { type: 2, title: "세로바" },
    { type: 3, title: "도넛" },
    { type: 4, title: "트렌드" },
    { type: 5, title: "테이블" }
];


function getChartTypeName(type) {
    type = Number(type);
    var chart = $.grep(chartTypes, function (chart) {
        return chart.type === type;
    });

    if (chart.length > 0)
        return chart[0];
    return null;
}
function getToken(key) {
    return localStorage.getItem(key);
}

function setToken(key, value) {
    localStorage.setItem(key, value);
}



function login(id, pw) {

    var url = apiService + "/account/login";
    var data = {
        "Email": id,
        "Password": pw
    };
    $.ajax({
        type: "POST",
        url: url,   
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (result) {
            if (typeof result === 'undefined')
                location.href = "login.html";
            else {
                setToken("token", result.token);
                setToken("userName", result.name);
                setToken("userID", result.userID);
                setToken("company", result.companyID);
            }
        }
    });
}


function loadHeader(url, callback) {
    $.get(url, function (data) {
        $(".nav").append(data);
        if (callback)
            callback();
    });
}

function getService(endPoint, callback) {
    //$.getJSON(apiService + endPoint, function (data) {
    //    callback(data);
    //});

    $.ajax({
        url: apiService + endPoint,
        type: "GET",
        headers: {

        },
        beforeSend: function (request) {
            request.setRequestHeader("Authorization",
                "Bearer " + getToken("token"));

            request.setRequestHeader("company",
                getToken("companyCode"));
        },
        success: function (data) {
            callback(data);
        },
        fail: function (m) {
            alert(m);
        }
    });


}

function saveService(endPoint, data, callback) {
    $.ajax({
        url: apiService + endPoint,
        type: "POST",
        data: data,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function () {
            callback(true);
        },
        fail: function (m) {
            alert(m);
        }
    });
}

function updateService(endPoint, data, callback) {
    $.ajax({
        url: apiService + endPoint,
        type: "PUT",
        data: data,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function () {
            callback(true);
        },
        fail: function (m) {
            alert(m);
        }
    });
}

function deleteService(endPoint, data, callback) {
    $.ajax({
        url: apiService + endPoint + data,
        type: "DELETE",
        contentType: "application/json",
        success: function () {
            callback(true);
        },

        fail: function (m) {
            alert(m);
        }
    });
}

String.prototype.format = function (args) {
    var str = this;
    return str.replace(String.prototype.format.regex, function (item) {
        var intVal = parseInt(item.substring(1, item.length - 1));
        var replace;
        if (intVal >= 0) {
            replace = args[intVal];
        } else if (intVal === -1) {
            replace = "{";
        } else if (intVal === -2) {
            replace = "}";
        } else {
            replace = "";
        }
        return replace;
    });
};

String.prototype.format.regex = new RegExp("{-?[0-9]+}", "g");

//if (document.referrer.indexOf(getHost()) === -1) {
//    setToken("token", null);
//    location.href = "../login.html";
//}

//console.log(document.referrer.indexOf(getHost()));
//if (getToken("token") === null) {
//    var ssoid = getParameterByName("ssoid");
//    if (ssoid === "")
//        location.href = "../login.html";
//    else {
//        //login(ssoid);
//        location.href = "../sso.html";
//    }
//    //
//}

//파라미터 타입에 따른 날짜를 가저온다.
function getDate(type) {
//1. 전일 -1D
//2. 전월 -1D-1M (YYYYMM)
//3. 당월 -1D (YYYYMM)
//4. 마감월 전역
//5. 전분기  

    if (typeof type === 'undefined')
        return "";
    var now = moment();
    var isClose = getToken("close");
    var s = (isClose) ? -1 : -2;

    //마감이 된 경우, -2를 리턴한다.

    switch (type.toUpperCase()) {
        case "D": //전일
            return now.add(-1, "days").format("YYYYMMDD");
        case "2M": //전전월
	        //return now.add(-1, "days").add(-1, "months").format("YYYYMM"); //전월 -1
            return now.add(-10, "days").add(-2, "months").format("YYYYMM"); //전전월 -2
        case "1M": //당월
            //return now.add(-1, "days").format("YYYYMM"); //실제
			return now.add(-10, "days").add(-1, "months").format("YYYYMM"); //전월 -1
        case "0M": //당월 : 매출액실적
            return now.add(-10, "days").add(0, "months").format("YYYYMM"); //전월 -1
        case "CM"://마감월 적용
            return now.add(-10, "days").add(s, "months").format("YYYYMM");
        case "Q": //분기
            //var cQ = Math.floor(now.add(-1, "days").format('YYYMMDD') / 3);
            //var cQ = Math.floor(now.add(-1, "days").month() / 3);
            //var cQ = Math.floor(now.add(-1, "days").add(-1, "months").month() / 3);            

            var cQ = Math.ceil((now.add(-1, "days").month() + 1) / 3);

            if (cQ === 1) {
                return now.add(-1, "years").format("YYYY") + 3; //4분기 -1분기
            }
            else {
                return now.format("YYYY") + (cQ - 1);
            }
        default:
            return "";
    }
}

//파라미터 타입에 따른 날짜를 가저온다.
function getDateToString(type) {
    switch (type.toUpperCase()) {
        case "D":
            return "전일";
        case "2M":
            return "전전월";
        case "1M":
            return "전월";
        case "0M":
            return "당월";
        case "CM":
            return "마감월";
        case "Q":
            return "전분기";
        default:
            return "마감선택";
    }
}

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, ""));
}
