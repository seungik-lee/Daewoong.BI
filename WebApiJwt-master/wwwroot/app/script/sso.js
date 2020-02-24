function getHost() {
    return location.protocol + "//" + location.host + "";
}
var apiService = getHost();
var userToken;
function setToken(key, value) {
    localStorage.setItem(key, value);
}

function sso() {
   // var id = getParameterByName("userid");

    var ids = location.search.lastIndexOf("userid=");
    var id = location.search.substring(ids + 7);

    var url = apiService + "/account/login";
    var data = {
        //"Email": encodeURIComponent(id)
        "Email": decodeURIComponent(id)
    };
    $.ajax({
        type: "POST",
        url: url,   
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (result) {
            setToken("token", result.token);
            setToken("userName", result.name);
            setToken("userID", result.userID);
            setToken("company", result.companyCode);
            //2019-12-26 김태규 수정 배포            
            setToken("companies", JSON.stringify(result.companies));
            if (result.userRole !== 2)
                setToken("companyCode", 1200);
            else
                setToken("companyCode", result.companyCode);

            //setToken("companies", result.companyCode);
            //setToken("companyCode", result.companyCode);
            //2019-12-12 김태규 수정 배포
            //console.log('userRole', result.userRole);            
            /*
            if (result.userRole !== 2) {
                setToken("companies", '1200');
                setToken("companyCode", '1200');
            }
            else {
                setToken("companies", JSON.stringify(result.companies);
                setToken("companyCode", result.companyCode);
            }
            */
            setToken("isAdmin", result.isAdmin);
            setToken("role", result.userRole);
            setToken("key", result.key);
            setToken("roleIDKey", result.roleIDKey);


            setToken("roleID", result.roleID);
            setClose();
            location.href = "index.html";
        }, error: function (result) {
            location.href = "login.html";
        }
    });
}


//설정정보 취득하기
function setClose() {
    $.getJSON("/api/setting" + "?key=close", function (data) {
        setToken("close", data);
    });
}

sso();
function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, ""));
}
