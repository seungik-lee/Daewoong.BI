$(document).ready(function () {
    var selectedCategory = {};
    var selectedClose = {};

    var selectedKPI;
    var kpiList;
    var mode = 0; //0:new, 1:edit
    var template = "<div class='items' data-id='{0}'><div class=header1>{1}</div><div class=title><img style='padding:10px;' src='../images/c-{3}.png'></div>{2}</div></div>";

   //카테고리 취득
    function getCategoris() {
        getService(categoryEP, function (data) {
            var items = "";
            $(data).each(function (i, n) {
                items = items + "<li class='cItem' id='" + n.id + "'><a>" + n.title + "</a></li>";
            });
            $("#category").append(items);
        });
    }

    function changeClose(target) {
        var val = $(target).prop("checked");
        var data = {
            key: "close",
            value:val
        };
        saveService(settingEP, JSON.stringify(data), function (result) {
            alert("마감이 변경되었습니다.");
        });
    }

    //회사목록을 취득한다.
    function getCompanies() {
        getService(companyEP, function (data) {
            var items = "";
            var items2 = "";

            companyList = data;
            $(data).each(function (i, n) {
                items = items + "<li class='cpItem' id='" + n.code + "'><a>" + n.companyName + "</a></li>";
                items2 = items2 + "<div class='root-company' id='" + n.code + "'>" + n.companyName + "</div>";

            });
            $("#companies").append(items2);

            $("#company").append(items);
        });
    }

    function setChartTypes() {
        var items = "";
        $(chartTypes).each(function (i, n) {
            items = items + "<li class='cTItem' id='" + n.type + "'><a>" + n.title + "</a></li>";
        })

        $("#chartTypes").append(items);
    }


    //모든 KPI취득하기
    function getData(url, callback) {
        getService(kpiEP, function (data) {
            $("#kpiContainer").html("");
            kpiList = data;
            var kpis = "";
            $(data).each(function (i, n) {
                //kpis += "<div class='items' data-id='" + n.id + "'><div>" + n.title + "</div></div>";
                n.chartType = (n.chartType === "") ? 0 : n.chartType;
                kpis += template.format([n.id.toString(), n.category, n.title, n.chartType]);
            });
            $("#kpiContainer").append(kpis);
        });

    } 

    //설정정보 취득하기
    function getSetting() {
        getService(settingEP + "?key=close", function (data) {
            if (data === true)
                $("#close").prop("checked", "checked");
        });

    }

    //저장하기
    function save(e) {
        if (validate(e)) {
            var data = {
                title: $("#title").val(),
                url: $("#url").val(),
                detailUrl: $("#detailUrl").val(),
                close: selectedClose.value,
                category: selectedCategory.title,
                companyCode: selectedCompany.code,
                chartType: selectedChartType.type
            };
            saveService(kpiEP, JSON.stringify(data), function (result) {
                if (result) {
                    $('.modal').modal('toggle');
                    getData();
                    reset();
                }
            });

        }

    }

    //수정하기
    function update(e) {
        var data = {
            id: selectedKPI.id,
            title: $("#title").val(),
            url: $("#url").val(),
            detailUrl: $("#detailUrl").val(),
            chartType: selectedChartType.type,
            close: selectedClose.value || selectedClose,
            category: selectedCategory.title || selectedCategory,
            companyCode: selectedCompany.code || selectedCompany
        };

        updateService(kpiEP, JSON.stringify(data), function (result) {
            if (result) {
                $('.modal').modal('toggle');
                getData();
                reset();
            }
        });
    }

    function validate(e) {
        var valid = true;
        //에러가 없으면,
        if ($('#myForm').validator('validate').has('.has-error').length === 0) {

            //회사지정안됨.
            if (typeof selectedCompany.code === 'undefined') {
                $(".btn-company").addClass("error");
                $(".valid-company").show();
                e.preventDefault();
                valid = false;
            } else {
                $(".btn-company").removeClass("error");
                $(".valid-company").hide();
            }

            //마감선택안됨.
            if (typeof selectedClose.value === 'undefined') {
                $(".btn-close").addClass("error");
                $(".valid-close").show();
                e.preventDefault();
                valid = false;
            } else {
                $(".btn-close").removeClass("error");
                $(".valid-close").hide();
            }

            //카테고리선택안됨.
            if (typeof selectedCategory.value === 'undefined') {
                $(".btn-category").addClass("error");
                $(".valid-category").show();
                e.preventDefault();
                valid = false;
            } else {
                $(".btn-category").removeClass("error");
                $(".valid-category").hide();
            }
            
            return valid;
        }
        return false;
    }

    //삭제하기
    function deleteKPI()
    {
        deleteService(kpiEP, "?id=" + selectedKPI.id, function (result) {
            if (result) {

                var target = $('div[data-id="' + selectedKPI.id + '"]');
                target.remove();
                selectedKPI = null;
            }
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

    function bindKPI(target) {
        $('.modal').modal('toggle');
        var id = $(target).data("id");
        selectedKPI = getKpiByID(id);
        $("#title").val(selectedKPI.title);
        $("#url").val(selectedKPI.url);
        $("#detailUrl").val(selectedKPI.detailURL);
        $("#selectedCompany").text(selectedKPI.companyName);

        $("#selectedCategory").text(selectedKPI.category);
        $("#selectedClose").text(getDateToString(selectedKPI.close));
        selectedCategory = selectedKPI.category;
        selectedCompany = {
            code: selectedKPI.companyCode,
            name: selectedKPI.companyName
        };
        selectedChartType = getChartTypeName(selectedKPI.chartType);

        $("#selectedChartType").text(selectedChartType.title);
        selectedClose = selectedKPI.close;
        
        mode = 1;
        $("#saveButton").text("수정");    
    }

    function reset() {
        $("#title").val("");
        $("#url").val("");
        $("#detailUrl").val("");
        $("#selectedCategory").text("카테고리 선택");
        $("#selectedCompany").text("회사 선택");
        selectedCategory = {};
        selectedClose = {};
        selectedCompany = {};
        selectedChartType = {};
        mode = 0;
    }

    //저장버튼 클릭
    $("body").on('click', ".save-button", (function (e) {
        if (mode === 0)
            save(e);
        else
            update(e);
    }));

    //KPI Item 클릭
    $("body").on('click', ".items", (function () {
        bindKPI(this);
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

    //회사명 필터링
    $("body").on('click', ".root-company", (function () {
        var id = $(this).attr("id");
        var filteredKPI = $.grep(kpiList, function (kpi) {
            return kpi.companyCode === id;
        });

        var kpis = "";
        $(filteredKPI).each(function (i, n) {
            kpis += template.format([n.id.toString(), n.category, n.title, n.chartType]);

        });
        $("#kpiContainer").empty().append(kpis);


    }));

    


    //마감클릭
    $("body").on('click', "#close", (function () {
        changeClose(this);
    }));
    

    //삭제버튼 클릭
    $("body").on('click', ".delete-button", (function () {
        if (confirm("정말로 삭제하시겠습니까?")) {
            var id = $(this).data("id");
            reset();
            $('.modal').modal('toggle');
            deleteKPI();
        };
    }));

    //마감 버튼 클릭
    $("body").on('click', ".closeItem", (function () {
        selectedClose = {
            title: $(this).text(),
            value: $(this).data("value")
        };

        
        $("#selectedClose").text(selectedClose.title);
    }));

    //카테고리 버튼 클릭
    $("body").on('click', ".cItem", (function () {
        var id = $(this).attr("id");
        selectedCategory = {
            value: id,
            title: $(this).text()
        };

        $("#selectedCategory").text(selectedCategory.title);
    }));

    //차트타입 버튼 클릭
    $("body").on('click', ".cTItem", (function () {
        var id = $(this).attr("id");
        selectedChartType = {
            type: id,
            title: $(this).text()
        };

        $("#selectedChartType").text(selectedChartType.title);
    }));


    //회사 버튼 클릭
    $("body").on('click', ".cpItem", (function () {
        selectedCompany = {
            title: $(this).text(),
            code: $(this).attr("id")
        };
        $("#selectedCompany").text(selectedCompany.title);
    }));


    getCategoris();
    getData();
    getSetting();
    getCompanies();
    setChartTypes();
    loadHeader("../menu.html");

});