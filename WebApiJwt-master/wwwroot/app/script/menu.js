﻿$(document).ready(function () {
    $.ajaxSetup({ cache: false });

    // 메뉴 조회
    $.ajax({
        url: "/api/menu/GetMenuList",
        data: null,
        type: "GET",
        success: function (data) {
            $("#ul_navbar").children().remove();

            var menus = "";

            $(data).each(function (i, n) {
                menus += "  <li class=\"dropdown\">";
                menus += "      <a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">";

                if (n.category == "FineReport") {
                    menus += "          <span class=\"glyphicon glyphicon-print\" aria-hidden=\"true\"></span> " + n.category + " <span class=\"caret\"></span>";
                } else if (n.category == "경영관리") {
                    menus += "          <span class=\"glyphicon glyphicon-usd\" aria-hidden=\"true\"></span> " + n.category + " <span class=\"caret\"></span>";
                } else if (n.category == "영업관리") {
                    menus += "          <span class=\"glyphicon glyphicon-envelope\" aria-hidden=\"true\"></span> " + n.category + " <span class=\"caret\"></span>";
                } else if (n.category == "생산관리") {
                    menus += "          <span class=\"glyphicon glyphicon-wrench\" aria-hidden=\"true\"></span> " + n.category + " <span class=\"caret\"></span>";
                } else {
                    menus += "          <span class=\"glyphicon glyphicon-comment\" aria-hidden=\"true\"></span> " + n.category + " <span class=\"caret\"></span>";
                }



                menus += "      </a>";
                menus += "      <ul class=\"dropdown-menu\">";

                $(n.menus).each(function (j, m) {
                    if (m.level == "" || m.level == 1) {
                        var menus_level2 = $.grep(n.menus, function (o, i) {
                            return (o.parentID == m.id && o.level == 2);
                        });

                        if (menus_level2 != null && menus_level2.length > 0) {
                            menus += "      <li class=\"dropdown dropdown-submenu\">";
                        } else {
                            menus += "      <li class=\"dropdown\">";
                        }

                        if (m.url != "") {
                            menus += "          <a href=\"" + m.url + "\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + m.close + "\">" + m.title + "</a>";
                        } else {
                            menus += "          <a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + m.close + "\">" + m.title + "</a>";
                        }

                        if (menus_level2 != null && menus_level2.length > 0) {
                            menus += "      <ul class=\"dropdown-menu\">";

                            for (var x = 0; x < menus_level2.length; x++) {
                                var menus_level3 = $.grep(n.menus, function (p, i) {
                                    return (p.parentID == menus_level2[x].id && p.level == 3);
                                });

                                if (menus_level3 != null && menus_level3.length > 0) {
                                    menus += "      <li class=\"dropdown dropdown-submenu\">";
                                } else {
                                    menus += "      <li class=\"dropdown\">";
                                }

                                if (menus_level2[x].url != "") {
                                    menus += "          <a href=\"" + menus_level2[x].url + "\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + menus_level2[x].close + "\">" + menus_level2[x].title + "</a>";
                                } else {
                                    menus += "          <a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + menus_level2[x].close + "\">" + menus_level2[x].title + "</a>";
                                }

                                if (menus_level3 != null && menus_level3.length > 0) {
                                    menus += "      <ul class=\"dropdown-menu\">";

                                    for (var y = 0; y < menus_level3.length; y++) {
                                        menus += "      <li class=\"dropdown\">";

                                        if (menus_level3[y].url != "") {
                                            menus += "          <a href=\"" + menus_level3[y].url + "\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + menus_level3[y].close + "\">" + menus_level3[y].title + "</a>";
                                        } else {
                                            menus += "          <a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" data-close=\"" + menus_level3[y].close + "\">" + menus_level3[y].title + "</a>";
                                        }

                                        menus += "      </li>";
                                    }

                                    menus += "      </ul>";
                                }

                                menus += "      </li>";
                            }

                            menus += "      </ul>";
                        }

                        menus += "      </li>";
                    }
                });

                menus += "  </ul>";
                menus += "</li>";
            });

            $("#ul_navbar").append(menus);

            // My DashBoard 조회
            $.ajax({
                url: "/api/page",
                type: "GET",
                data: null,
                success: function (data) {
                    if (data == "") {
                        return;
                    }

                    var html = "<li class=\"dropdown\">";
                    html += "<a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                    html += "<span class=\"glyphicon glyphicon-user\" aria-hidden=\"true\"></span>";

                    if (data == "") {
                        html += " My DashBoard ";
                    } else {
                        html += " My DashBoard <span class=\"caret\"></span>";
                    }

                    html += "</a>";
                    html += "<ul class=\"dropdown-menu\">";

                    $(data).each(function (i, n) {
                        html += "<li onclick=\"moveDashBoard(" + n.id + ")\" class=\"dropdown\"><a href=\"#\" class=\"dropdown-toggle\" data-toggle=\"dropdown\">" + n.title + "</a></li>";
                    });

                    html += "</ul></li>";

                    $("#ul_navbar").prepend(html);
                },
                error: function (data) {
                    // 세션이 끊긴 상태
                    if (data.status == 600) {
                        location.href = "../login.html";
                    } else {
                        alert("error : " + data.responseText);
                    }
                }
            });

            $('ul.dropdown-menu [data-toggle=dropdown]').on('click', document, function (event) {
                event.preventDefault();
                event.stopPropagation();
                $(this).parent().siblings().removeClass('open');
                $(this).parent().toggleClass('open');
                
                if ($(this).attr("href") != "" && $(this).attr("href") != "#") {
                    var url = $(this).attr("href");
                    var linkt = url.lastIndexOf("webroot");
                    var flink = url.substring(0, linkt - 1);

                    //linkt = -1 이면 대시보드 호출
                    //linkt > 0 이면 파인리포트 호출
                    if (linkt < 0) {
                        if (url.indexOf("/") == 0) {
                            location.href = url;
                        } else {
                            url += "&X_COMPANY=" + localStorage.getItem("companyCode") + "&X_DATESETTING=" + getDate($(this).attr("data-close")) + "&userid=" + localStorage.getItem("roleIDKey");

                            location.href = "/detail.html?url=" + url;
                        }
                    } else if (linkt > 0) {
                        flink += "?id=" + localStorage.getItem("key") + "&link=" + url.substring(url.lastIndexOf("webroot"));

                        location.href = "/detail.html?url=" + flink;
                    }
                }
            });
        },
        error: function (data) {
            // 세션이 끊긴 상태
            if (data.status == 600) {
                location.href = "/login.html";
            } else {
                alert("error : " + data.responseText);
            }
        }
    });

    // 회사 조회
    if (localStorage.getItem("role") == 1 || localStorage.getItem("role") == 3) {
        $.ajax({
            url: "/api/company/GetCompanies",
            data: null,
            type: "GET",
            success: function (data) {
                var items = "";
                var displayCompanyName = "";

                $(data).each(function (i, n) {
                    if (n.code < 2000) {
                        items = items + "<li><a href=\"#\" onclick=\"changeCompanyInfo('" + n.code + "', '" + n.companyName + "')\"> " + n.companyName + " </a></li>";

                        if (n.code == localStorage.getItem("companyCode")) {
                            displayCompanyName = n.companyName;
                        }
                    }
                });

                $("#liCompanyList").show();
                $("#ulCompanies").append(items);

                $("#ulCompanies").prev("button").html(" " + displayCompanyName + " <span class=\"caret\"></span>");

                $("#liManagingItem1,#liManagingItem2").show();
            },
            error: function (data) {

            }
        });
    } else {
        $.ajax({
            url: "/api/user/getcompanies",
            data: { "id": localStorage.getItem("userID") },
            type: "GET",
            success: function (data) {
                if (data == "" || data.length == 1) {

                } else {
                    var items = "";
                    var displayCompanyName = "";

                    $(data).each(function (i, n) {
                        if (n.code < 2000) {
                            items = items + "<li><a href=\"#\" onclick=\"changeCompanyInfo('" + n.code + "', '" + n.companyName + "')\"> " + n.companyName + " </a></li>";

                            if (n.code == localStorage.getItem("companyCode")) {
                                displayCompanyName = n.companyName;
                            }
                        }
                    });

                    $("#liCompanyList").show();
                    $("#ulCompanies").append(items);

                    $("#ulCompanies").prev("button").html(" " + displayCompanyName + " <span class=\"caret\"></span>");
                }
            }, error: function (data) {

            }
        });
    }

    // 사용자 이름
    $("#lnkUserName").html(localStorage.getItem("userName") + " <span class=\"caret\"></span>");

    // 로고 이미지
    $("#navLogoImage").addClass("orange_bar_" + localStorage.getItem("companyCode"));
});

$(window).load(function () {
    // 대메뉴 폰트 weight 삭제
    $("#ul_navbar").find("a[role='button']").each(function () {
        $(this).css("font-weight", "initial");
    });
});

function changeCompanyInfo(companyCode, companyName) {
    // 현재 회사 코드로 정보를 업데이트
    $.ajax({
        url: "/api/user/ChangeUserCompanyInfo",
        data: { "companyCode": companyCode, "companyName": companyName },
        type: "GET",
        success: function (data) {
            localStorage.setItem("companyCode", companyCode);
            localStorage.setItem("companyName", companyName);

            location.href = "/index.html";
        },
        error: function (message) {
        }
    });
}

//fine report
$("body").on('click', ".level", (function () {

    var url = $(this).data("url");

    if (typeof url === 'undefined')
        return;
    if (url === "")
        return;

    var linkt = url.lastIndexOf("webroot");
    var flink = url.substring(0, linkt - 1);
    rlink = url.substring(linkt);
    var date = getDate($(this).data("close"));
    var id = getToken("key");

    flink += "?id=" + id + "&link=" + rlink;
    location.href = "../detail.html?url=" + flink;
}));

function logout() {
    // 사용자 정보 세션 삭제
    $.ajax({
        url: "/api/user/InitializeSessionUserInfo",
        data: null,
        type: "GET",
        contentType: "application/json",
        success: function (message) {
            localStorage.removeItem("userName");
            localStorage.removeItem("userID");
            localStorage.removeItem("company");
            localStorage.removeItem("companies");
            localStorage.removeItem("companyCode");
            localStorage.removeItem("companyName");
            localStorage.removeItem("isAdmin");
            localStorage.removeItem("role");
            localStorage.removeItem("roleID");
            localStorage.removeItem("key");
            localStorage.removeItem("roleIDKey");

            location.href = "/login.html";
        },
        error: function (message) {
            location.href = "/login.html";
        }
    });
}

function moveDashBoard(id) {
    location.href = "/index.html?dpid=" + id;
}

function getDate(type) {
    //1. 전일 -1D
    //2. 전월 -1D-1M (YYYYMM)
    //3. 당월 -1D (YYYYMM)
    //4. 마감월 전역
    //5. 전분기  

    if (typeof type === 'undefined')
        return "";
    var now = moment();
    var isClose = localStorage.getItem("close");
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

function showUserInfo() {
    $("#txtOldPassword").val("");
    $("#txtNewPassword").val("");
    $("#txtConfirmPassword").val("");

    $("#divUserModal").show();
}

function closeModal() {
    $("#divUserModal").hide();

    removeErrorStyle($("#txtOldPassword"));
    removeErrorStyle($("#txtNewPassword"));
    removeErrorStyle($("#txtConfirmPassword"));
}

function removeErrorStyle(obj) {
    $(obj).val("");
    $(obj).parent("div").removeClass("has-error");
    $(obj).next("div").text("");
}

function savePassword() {
    if ($("#txtOldPassword").val() == "") {
        $("#txtOldPassword").parent("div").addClass("has-error");
        $("#txtOldPassword").next("div").text("이전 비밀번호를 입력하세요.");
        return;
    } else if ($("#txtOldPassword").val().length < 6) {
        $("#txtOldPassword").parent("div").addClass("has-error");
        $("#txtOldPassword").next("div").text("비밀번호는 6자 이상이어야 합니다.");
        return;
    }

    if ($("#txtNewPassword").val() == "") {
        $("#txtNewPassword").parent("div").addClass("has-error");
        $("#txtNewPassword").next("div").text("변경할 비밀번호를 입력하세요.");
        return;
    } else if ($("#txtNewPassword").val().length < 6) {
        $("#txtNewPassword").parent("div").addClass("has-error");
        $("#txtNewPassword").next("div").text("비밀번호는 6자 이상이어야 합니다.");
        return;
    }

    if ($("#txtConfirmPassword").val() == "") {
        $("#txtConfirmPassword").parent("div").addClass("has-error");
        $("#txtConfirmPassword").next("div").text("변경할 비밀번호를 동일하게 입력하세요.");
        return;
    } else if ($("#txtConfirmPassword").val().length < 6) {
        $("#txtConfirmPassword").parent("div").addClass("has-error");
        $("#txtConfirmPassword").next("div").text("비밀번호는 6자 이상이어야 합니다.");
        return;
    }

    if ($("#txtOldPassword").val() == $("#txtNewPassword").val()) {
        $("#txtNewPassword").parent("div").addClass("has-error");
        $("#txtNewPassword").next("div").text("이전 비밀번호와 변경할 비밀번호가 일치합니다.");
        $("#txtNewPassword").val("");
        return;
    }

    if ($("#txtNewPassword").val() != $("#txtConfirmPassword").val()) {
        $("#txtConfirmPassword").parent("div").addClass("has-error");
        $("#txtConfirmPassword").next("div").text("변경할 비밀번호가 일치하지 않습니다.");
        $("#txtConfirmPassword").val("");
        return;
    }

    $.ajax({
        url: "/api/user/ChangeUserPassword",
        data: { oldpassword: $("#txtOldPassword").val(), newpassword: $("#txtNewPassword").val() },
        type: "GET",
        contentType: "application/json",
        success: function (data) {
            if (data.indexOf("success") >= 0) {
                alert(data.split(":")[1]);

                location.href = "/login.html";
            } else {
                alert(data.split(":")[1]);
            }
        },
        error: function (data) {
            // 세션이 끊긴 상태
            if (data.status == 600) {
                location.href = "/login.html";
            } else {
                alert("오류가 발생하였습니다. [" + data.responseText + "]");
            }
        }
    });
}