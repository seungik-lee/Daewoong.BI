var pages = [];
$("body").css({"opacity":0});
$(document).ready(function () {
    $.ajaxSetup({ cache: false });
    var menuEP = "/api/menu";
    var key = getToken("roleIDKey");    

    var menuTemplate = "<li>"
        + "<a class=\"dropdown-toggle \" role=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">"
        + "<img src=\"../images/{1}.png\">"
        + "<span style=\"font-size: 16px;letter-spacing:-0.3px;\">{0}</span> <span class=\"caret\"></span>"
        + "</a>"
        + "<ul class=\"dropdown-menu menu2\" role = \"menu\" >";
    var menuTemplateEnd = "</ul></li>";
    var myMenuTemplate = "<li role=\"menuitem\" class= mymenu-item id=\"{0}\"><a href=\"#\" onClick=\"\"><i class=\"fa fa-user\"></i>{1}</a></li>";

    var subTemplate = "<li role=\"menuitem\"><a href=\"#\" data-url=\"{1}\"  style=\"padding:10px;\" class=\"sub-menuitem\" data-close=\"{2}\" >{0}</a></li>";
    //현재 Layout

    //카테고리 취득
    function getMenus() {
        getService(menuEP + "?companyID=" + getToken("companyCode"), function (data) {

            var menus = "";
            $(data).each(function (i, n) {
                menus += menuTemplate.format([n.category, i]);
                $(n.menus).each(function (j, m) {
                    if (m.level === "") {
                        menus += subTemplate.format([m.title, m.url, m.close]);
                    }
                });
                menus += menuTemplateEnd;
            });
            $(".menu").append(menus);


            setFineReportMenus(data);

        });
    }

    function setFineReportMenus(data) {
        
        var d1 = getFileByLevel(1, data);
        var d2 = getFileByLevel(2, data);
        var d3 = getFileByLevel(3, data);

        $(d1).each(function (i, n) {
            $("#lvl" + (i + 1)).append("<div class='level level1'id='m_" + n.id + "'><div style='padding-bottom:10px;border-bottom:2px solid orange' >" + n.title + "</div></div>");
        });

        $(d2).each(function (i, n) {
            $("#m_" + n.parentID).append("<div id='m_" + n.id + "'><div class='level level2' data-url='" + n.url + "'>" + n.title + "</div></div>");
        });

        $(d3).each(function (i, n) {
            $("#m_" + n.parentID).append("<div><div class='level level3' data-url='" + n.url + "'>" + n.title + "</div></div>");
        });
       

        $(d1).each(function (i, n) {

        });
    }

    function getFileByLevel(level, data) {  

        var target = $.grep(data, function (d) {
                return d.category === "FineReport";
        });
        if (target.length === 0)
            return;
        return $.grep(target[0].menus, function (menu) {
            return menu.level === level.toString();
        });
    }

    //모든 페이지취득하기
    function getPages() {
       
        getService(pageEP + "?companyCode=" + getToken("companyCode"), function (data) {
            var container = "pageContainer";
            pages = data;
            var pagesString = "";


            $(data).each(function (i, n) {
                pagesString += myMenuTemplate.format([n.id, n.title]);
            });
           
            $("#myMenu").html("").append(pagesString);
            var evt = jQuery.Event("pagesLoaded");
            $("body").trigger(evt);

        });

    }



    function bindKPI(page) {
        $(page.kpIs).each(function (i, n) {
            var cTarget = $("#chart_" + n.seq);
            var iTarget = $(cTarget).find("iframe");
            var urlMask = $(cTarget).find(".target-mask");
            var date = getDate(n.close);
            urlMask.click(function () {
                location.href = n.detailURL;
            });

            var cCode = getToken("companyCode");    
            iTarget.attr("src", n.url + "&X_COMPANY=" + cCode + "&X_DATESETTING=" + date + "&userid=" + key);
        });
    }

    //KPI Item 클릭
    $("body").on('click', ".sub-menuitem", (function () {
        var url = $(this).data("url");
                
        var linkt = url.lastIndexOf("webroot");
        var flink = url.substring(0, linkt - 1);

        //alert("URL === " + url);
        //alert("linkt ==== " + linkt);

        //linkt = -1 이면 대시보드 호출 
        //linkt > 0 이면 파인리포트 호출
        if (linkt < 0) {
            var date = getDate($(this).data("close"));
            var cCode = getToken("companyCode");

            url += "&X_COMPANY=" + cCode + "&X_DATESETTING=" + date + "&userid=" + key;
            location.href = "../detail.html?url=" + url;
        } else if (linkt > 0) {
            if (url === "")
                return;

            var linkt = url.lastIndexOf("webroot");
            var flink = url.substring(0, linkt - 1);
            rlink = url.substring(linkt);
            var date = getDate($(this).data("close"));
            var id = getToken("key");

            flink += "?id=" + id + "&link=" + rlink;
            //alert(flink);
            location.href = "../detail.html?url=" + flink;
        }
        
        
    }));

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


    $("body").on('click', ".dropdown-toggle", function (e) {
        if (e.target.innerText.toUpperCase().trim() === "FINEREPORT") {
            $(e.target).find(".menu2").hide();
            if ($(".fine-menu-container").css("display") !== "none")
                $(".fine-menu-container").hide();
            else
                $(".fine-menu-container").show();
        } else {
            $(e.target).find(".dropdown-menu").show();
            $(".fine-menu-container").hide();
        }
    });

    $("body").on('click', "#fineClose", function (e) {
        $(".fine-menu-container").hide();

    });

    //회사클릭
    $("body").on('click', ".cp2Item", (function () {
        var companyCode = $(this).attr("id");
        setToken("companyCode", companyCode);

        location.href = "../index.html";
    }));

    //대시보드 아이템 클릭
    $("body").on('click', ".mymenu-item", (function () {
        var id = $(this).attr("id");
        location.href = "../index.html?dpid=" + id;
        //var page = getPageByID(Number(id));
        //newPage(page);
    }));

    var items = "";

    var role = getToken("role"); 
    if (role === "1" || role === "3" ) {
        getService(companyEP, function (data) {
            $(data).each(function (i, n) {
                if (n.code < 2000) {
                    items = items + "<li class='cp2Item ccode-" + n.code + "' id = '" + n.code + "' > <a>" + n.companyName + "</a></li > ";
                }
            });
            $("#companyListContainer").show();
            $("#kpiM").show();
            $("#userM").show();
            $("#companyList").append(items);
            var company = getToken("companyCode");
            var text = $(".ccode-" + company).find("a").text();
            $("#selectedCompanyTop").text(text);

        });
    }
	//2019-12-26 김태규 수정 배포
    var _companies = getToken('companies');
    var companies = JSON.parse(_companies);

    if (companies !== null && companies.length > 1) {
        $(companies).each(function (i, n) {
            items = items + "<li class='cp2Item ccode-" + n.code + "' id = '" + n.code + "' > <a>" + n.companyName + "</a></li > ";
        });
        $("#companyList").append(items);
        $("#companyListContainer").show();
        var company = getToken("companyCode");
        var text = $(".ccode-" + company).find("a").text();
        $("#selectedCompanyTop").text(text);
    }

    getMenus();
    getPages();

});

setTimeout(function () {
    $("body").animate({"opacity":1}, 300);
}, 500);