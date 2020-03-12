$(document).ready(function () {
    $.ajaxSetup({ cache: false });
    var selectedPage;

    var defaultPage;
    var kpiList;
    var userID = getToken("userID");
    var companyCode = getToken("companyCode");
    //현재 Layout
    var currentLayout;
    var selectedKPIs = [];
    var pageList;
    var mode = 0; //0:new, 1:edit
    var pageTemplate = "<div class='items settings' style='width:100%' data-id='{0}'><div class=header1>{1}</div><div class=title>{2}</div></div>";
    var pageLineTempate = "<div class='container-flex'>{0}</div>";
    var template = "<div class='items-small' data-id='{0}'><div class=title>{1}</div></div>";

   //카테고리 취득
    function getCategoris() {

        getService(categoryEP, function (data) {
            var items = "";
            $(data).each(function (i, n) {
                items = items + "<button type='button' class='btn btn-primary btn-category'>" + n.title + "</button>";
            });
            $(".btn-group").append(items);
        });
    }

    $("body").on("pagesLoaded", function () {
        init();
    });

    function init() {
        var dpid = getParameterByName('dpid');
        var page = null;
        if (dpid !== "")
            page = getPageByID(dpid);
        else {
            if (pages.length > 0)
                page = pages[0];
        }

        if (page !== null)
            newPage(page);
        else {
            $("#inform").show();
            $("#myDashboardMenu").hide();
        }


    }

    function go(id) {
        var page = getPageByID(id);
        newPage(page);
    }

    //모든 KPI취득하기
    function getKPIBank(url, callback) {
        getService(kpiEP, function (data) {
            kpiList = data;
            bindKPIList(data);
        });

    }

    function bindKPIList(data) {
        $(".kpi-bank").html("");
        var kpis = "";
        $(data).each(function (i, n) {
            kpis += template.format([n.id.toString(), n.title]);
        });
        if (data.length === 0)
            kpis ="<div>등록된 지표가 없습니다.</div>";
        $(".kpi-bank").append(kpis);
        $(".items-small").draggable({ revert: true, helper: "clone" });
    }   

    window.getPageByID = function (id) {
        id = Number(id);
        var result = $.grep(pages, function (page) {
            return page.id === id;
        });

        if (result.length > 0)
            return result[0];
        else
            alert("해당되는 페이지가 없습니다");
    };


    //페이지 생성(신규/기존)
    window.newPage = function(page) {
        $("#kpiContainer").html("");
        var l = page.layout.split("by");
        var lineItems = "";
        var k = 0;
        $.get("app/template/chartTemplateDS.html", function (data) {
            for (var i = 0; i < l[1]; i++) {
                for (var j = 0; j < l[0]; j++) {
                    lineItems = lineItems + data.format([k]);
                    k++;
                }
                var newLine = pageLineTempate.format([lineItems]);
                $("#kpiContainer").append(newLine);
                lineItems = "";
            }

            bindKPI(page);
            //$(".kpi-bank-container").animate({ "bottom": 0 }, 1300, 'easeInOutQuart');

        });
    }


    function getKpiByID(id) {
        var result = $.grep(kpiList, function (kpi) {
            return kpi.id === id;
        });

        if (result.length > 0)
            return result[0];
        else
            alert("해당되는 KPI가 없습니다");
    }

    function getKpiBySeq(seq) {
        var seq2;
        seq = Number(seq);
        $(kpiList).each(function (i, n) {
            if (n.seq === seq) {
                seq2 = i;
            }
        });

        return seq2;
    }

    function getOldKpiBySeq(seq) {
        var seq2 = -1;
        seq = Number(seq);
        $(selectedKPIs).each(function (i, n) {
            if (n.seq === seq) {
                seq2 = i;
            }
        });

        return seq2;
    }



    function bindKPI(page) {
        $(page.kpIs).each(function (i, n) {
            var cTarget = $("#chart_" + n.seq);
            var iTarget = $(cTarget).find("iframe");
            var urlMask = $(cTarget).find(".target-mask");
            var date = getDate(n.close);
            var key = getToken("roleIDKey");
            urlMask.click(function () {
                var params = n.detailURL + "&X_COMPANY=" + companyCode + "&X_DATESETTING=" + date + "&userid=" + key;
                location.href = "../detail.html?url=" + params;
            });
            iTarget.attr("src", n.url + "&X_COMPANY=" + companyCode + "&X_DATESETTING=" + date + "&userid=" + encodeURIComponent(key));
            //iTarget.attr("src", n.url + "&X_COMPANY=" + companyCode + "&X_DATESETTING=");
        });
    }

   

    function reset() {
        $("#title").val("");
       
        mode = 0;
    }
    
    //KPI Item 클릭
    $("body").on('click', ".items", (function () {
        mode = 1;
        $(".save-button").text("수정하기");
        
        var id = $(this).data("id"); 
        selectedPage = getPageByID(id);
        selectedKPIs = selectedPage.kpIs;
        currentLayout = selectedPage.layout;
        $("#title").val(selectedPage.title);
        newPage(selectedPage.layout, function () {
            $(selectedPage.kpIs).each(function (i, n) {
                var cTarget = $("#chart_" + n.seq);
                var iTarget = $(cTarget).find("iframe");
                iTarget.attr("src", n.url);
            });
        });
    }));



    //KPI Item 클릭
    $("body").on('click', ".cancel-button", (function () {
        reset();
        $('.modal').modal('toggle');

    }));



    //카테고리 버튼 클릭
    $("body").on('click', ".btn-category", (function () {
        $(".btn-category").removeClass("active");
        var category = $(this).text();

        if (category === "전체") {
            bindKPIList(kpiList);
            return;
        }

        var data = $.grep(kpiList, function (kpi) {
            return kpi.category === category;
        });
        bindKPIList(data);

    }));

    loadHeader("menu.html", function () {
    });

    getCategoris();
    getKPIBank();
    
   
});

