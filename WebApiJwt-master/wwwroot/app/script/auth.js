
function getHost() {
    return location.protocol + "//" + location.host + "";
}
var apiService = getHost();
var settingEP = "/api/setting";
var sapEP = "http://10.1.20.66:8080/biprws/logon/long";
var sapRequest = "<attrs xmlns=\"http://www.sap.com/rws/bip\">" +
    "<attr name = \"userName\" type = \"string\">{0}</attr>" +
    "<attr name=\"password\" type=\"string\">{1}</attr>" +
    "<attr name=\"auth\" type=\"string\" possibilities=\"secEnterprise,secLDAP,secWinAD,secSAPR3\">secEnterprise</attr>" +
    "</attrs>";

function setToken(key, value) {
    localStorage.setItem(key, value);
}

function deleteToken(key) {
    localStorage.removeItem(key);
}

function login() {
    window.localStorage.clear();
    var id = $("#id").val();
    var pw = $("#pw").val();

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
            $("#msg").text("ID혹은 Password가 바르지 않습니다.");
        }
    });

    return false;
}

//SAP에 로그인
function SAPLogin(id, pw) {
    var data = {
        "Email": id,
        "Password": pw
    };
    $.ajax({
        type: "POST",
        url: sapEP,
        data: sapRequest,
        success: function (result) {
           // alert(result);
        }
    });
}

//설정정보 취득하기
function setClose() {
    $.getJSON(settingEP + "?key=close", function (data) {
        setToken("close", data);
    });
}

function logout() {
    deleteToken("token");
    deleteToken("userName");
    deleteToken("userID");
    deleteToken("company");
    location.href = "../login.html";
}

