
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

    if (id == "") {
        $("#msg").text("User Email을 입력하세요.");
        $("#id").focus();
        return false;
    }

    if (pw == "") {
        $("#msg").text("비밀번호를 입력하세요.");
        $("#pw").focus();
        return false;
    }

    var url = apiService + "/account/login";
    var data = { "Email": id, "Password": pw };

    $.ajax({
        type: "POST",
        url: url,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (result) {
            if (result.id == 0) {
                $("#msg").text("User Email 혹은 Password가 틀렸습니다.");
                return false;
            }

            setToken("userName", result.name);
            setToken("userID", result.userID);
            setToken("company", result.companyCode);
            setToken("companies", JSON.stringify(result.companies));
            setToken("companyCode", result.companyCode);
            setToken("companyName", result.companyName);
            setToken("isAdmin", result.isAdmin);
            setToken("role", result.userRole);
            setToken("roleID", result.roleID);
            setToken("key", result.key);
            setToken("roleIDKey", result.roleIDKey);

            setClose();

            location.href = "index.html";
        }, error: function (result) {
            $("#msg").text("User Email 혹은 Password가 틀렸습니다.");
            return false;
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

