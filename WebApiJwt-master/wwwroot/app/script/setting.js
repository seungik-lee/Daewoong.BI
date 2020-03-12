$(document).ready(function () {
    $.ajaxSetup({ cache: false });
    var selectedPage;
    var categoryEP = "/api/category";
    var pageEP = "/api/page";
    var kpiEP = "/api/KPI";
    var kpiList;
    var userID = getToken("userID");
    //현재 Layout
    var currentLayout;
    var selectedKPIs = [];
    var pages = [];
    var pageList;
    var mode = 0; //0:new, 1:edit
    var pageTemplate = "<div class='items settings' style='width:100%;height:70px;padding-top:20px;font-size:16px;' data-id='{0}'>{1}</div>";
    var pageLineTempate = "<div class='container-flex'>{0}</div>";
    var template = "<div class='items-kpi' data-id='{0}'><div class=title>{1}</div></div>";

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

    //모든 페이지취득하기
    function getPages() {
        getService(pageEP + "?userid=" + userID + "&companyCode=" + getToken("companyCode"), function (data) {
            var container = "pageContainer";
            pages = data;
            var pagesString = "";
            $(data).each(function (i, n) {
                pagesString += pageTemplate.format([n.id.toString(), n.title]);
            });
            $("#" + container).html("").append(pagesString);
        });

    }

    //모든 KPI취득하기
    function getKPIBank(url, callback) {
        var companyCode = getToken("companyCode");
        getService(kpiEP, function (data) {
            
            kpiList = $.grep(data, function (kpi) {
                return kpi.companyCode === companyCode;
            });
            bindKPIList(kpiList);
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
        $(".items-kpi").draggable({ revert: true, helper: "clone" });
    }

    //저장하기
    function save() {
        var data = {
            title: $("#title").val(),
            kpis: selectedKPIs,
            layout: currentLayout,
            companyCode: getToken("companyCode")
            
        };
        saveService(pageEP + "/" + userID, JSON.stringify(data), function (result) {
            if (result) {
                getPages();
                alert("정상적으로 등록되었습니다");
            }
        });
    }

    //수정하기
    function update() {

        var data = {
            id: selectedPage.id,
            title: $("#title").val(),
            kpis: selectedKPIs,
            layout: currentLayout,
            companyCode: getToken("companyCode")
        };

        if(typeof currentLayout === "undefined")
            alert();
        updateService(pageEP + "/" + userID, JSON.stringify(data), function (result) {
            if (result) {
                alert("정상적으로 수정되었습니다");
            }
        });
    }

    //삭제하기
    function deletePage()
    {
        deleteService(pageEP, "?id=" + selectedPage.id, function (result) {
            if (result) {

                var target = $('div[data-id="' + selectedPage.id + '"]');
                target.remove();
                selectedPage = null;
                reset();
                $(".new-page").click();
            }
        });
      
    }


    //페이지 생성(신규/기존)
    function newPage(layout, callback) {
        $(".command-group").show();
        $("#pageDetail").html("");
        
        var lineItems = "";
        var k = 0;
        $.get("app/template/" + layout + ".html", function (data) {
            $.get("app/template/chartTemplate.html", function (template) {
                
                $("#pageDetail").append(data);

                //var td = $(data).find("td");
                $("td").each(function (i, n) {
                    var newLine = template.format([i, i]);

                    $(n).html(newLine);
                });
                bindEvent();

                if (callback)
                    callback();
            });
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

    function getPageByID(id) {
        var result = $.grep(pages, function (page) {
            return page.id === id;
        });

        if (result.length > 0)
            return result[0];
        else
            alert("해당되는 페이지가 없습니다");
    }

    function bindKPI(target) {
        $('.modal').modal('toggle');
        var id = $(target).data("id");
        selectedKPI = getKpiByID(id);
        $("#title").val(selectedKPI.title);
        $("#url").val(selectedKPI.url);
        $("#selectedCategory").text(selectedKPI.category);
        mode = 1;
        $("#saveButton").text("수정");
    }

    function reset() {
        $("#title").val("");
       
        mode = 0;
    }

    function bindEvent() {
        $(".dashboard-item-dummy").droppable({
            over: function (event, ui) {
                $(this).addClass("droppable-above");

            },
            out: function (event, ui) {
                $(this).removeClass("droppable-above");

            },
            classes: {
                "ui-droppable-active": "ui-state-active",
                "ui-droppable-hover": "ui-state-hover"
            },
            drop: function (event, ui) {
                $(this).removeClass("droppable-above");
                //현재위치파악
                var seq = $(this).attr("id").split("_")[1];
                //지표 ID취득
                var drag_id = $(ui.draggable).data("id");
                //지표리스트로부터 지표 취득
                var kpi = getKpiByID(drag_id);
                //지표 복사
                var newKpi = jQuery.extend({}, kpi);
                //순서 지정
                newKpi.seq = seq;

                //컨테이너에 이미 있는경우 삭제
                var oldCheck = getOldKpiBySeq(seq);
                if (oldCheck >= 0) {
                    selectedKPIs.splice(oldCheck, 1);
                }

                //목록에 저장
                selectedKPIs.push(newKpi);
                //헬퍼 삭제
                $(ui.helper).remove(); //destroy clone
                //$(ui.draggable).remove(); //remove from list    

                //iFrame에 해당 URL지정
                var iTarget = $(this).find(".chart-title");
                iTarget.text(newKpi.title);
                $(this).find(".delete-button").show();
                $(this).find("img").attr("src", "../../images/c-" + newKpi.chartType + ".png");
            }
        });
    }



    //저장버튼 클릭
    $("body").on('click', ".save-button", (function () {
        if (mode === 0)
            save();
        else
            update();
    }));

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
                var imgTarget = $("#chart_img" + n.seq);
                var iTarget = $(cTarget).find(".chart-title");
                imgTarget.attr("src", "../../images/c-" + n.chartType + ".png");
                $(iTarget).parent().parent().find(".delete-button").show();

                iTarget.text(n.title);
            });
        });
    }));

    //지표등록
    $("body").on('click', "#newButton", (function () {
        mode = 0;
        $("#saveButton").text("저장");

        reset();
    }));
    

    //KPI Item 클릭
    $("body").on('click', ".cancel-button", (function () {
        reset();
        $('.modal').modal('toggle');

    }));

    //신규 페이지 등록 클릭
    $("body").on('click', ".new-page", (function () {
        var newLayout = "3by2";
        reset();
        mode = 0;
        newPage(newLayout);
        selectedKPIs = [];
        selectedPage = null;
        currentLayout = newLayout;
        $(".save-button").text("저장하기");

    }));

    //layout 버튼 클릭
    $("body").on('click', ".layout-button", (function () {
        var layout = $(this).data("layout");
        currentLayout = layout;
       // if (selectedPage) {
            newPage(layout, function () {

                $(selectedKPIs).each(function (i, n) {
                    var cTarget = $("#chart_" + n.seq);
                    var iTarget = $(cTarget).find(".chart-title");
                    var imgTarget = $("#chart_img" + n.seq);
                    $(iTarget).parent().parent().find(".delete-button").show();
                    imgTarget.attr("src", "../../images/c-" + n.chartType + ".png");
                    iTarget.text(n.title);
                });
            });
        //}else
        //    newPage(layout);
    }));
        

    //KPI삭제버튼 클릭
    $("body").on('click', ".delete-button", (function () {  
        var id = $(this).parent().parent().parent().attr("id").split("_")[1];
        var oldCheck = getOldKpiBySeq(id);
        if (oldCheck >=0) {
            selectedKPIs.splice(oldCheck, 1);
        }
        var iTarget = $(this).parent().parent().parent().find(".chart-title");
        iTarget.text("Drag KPI");
        $(this).hide();
        $(this).parent().find("img").attr("src", "");
        //deleteKPI();
    }));

    //페이지삭제버튼 클릭
    $("body").on('click', ".page-delete-button", (function () {
        if (confirm("정말로 페이지를 삭제하시겠습니까?")) {
            deletePage();
        }
        
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

    


    getCategoris();
    getKPIBank();
    getPages();
});

function login() {
        
    var url = "http://localhost:60160/account/Register";
    var data = {
        "Email": "ops72s@naver.com",
        "Password": "##Kimtg0027"
    };
    $.ajax({
        type: "POST", /* the request's method: */
        url: url,    /* the request's location: */
        data: data,
        success: function (json) { /* the callback function */
            if (json.length > 0) {
                $.each(json, function (i, v) {
                    console.info(v);
                });
            } else {
                alert('wtf?!');
            }
        }
    });




}