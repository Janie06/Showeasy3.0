$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        iPageIndex = 1,
        iPageCount = 10,
		CountryChecked = [],
		CountryCheckedName = [],
		CategoryChecked = [],
		CategoryCheckedName = [],
		sCode = getUrlParam('Code'),
        /*
        * 目的 抓取國家
        */
        fnGetArguments = function () {
            return g_api.ConnectLite(Service.apiappcom, ComFn.GetArguments, {
                ArgClassID: 'Area',
                LevelOfArgument: 0
            }, function (res) {
                if (res.RESULT) {
					let saResArea = res.DATA.rel,
						sHtmlArea = $('#temp_Areas').render(saResArea);
                    $("#hidden-countries").html(sHtmlArea);
					
					$.each(saResArea, function (LogIdx, LogData) {
						g_api.ConnectLite(Service.apiappcom, ComFn.GetArguments, {
							ArgClassID: 'Area',
							LevelOfArgument: 1,
							ParentID: LogData.id
						}, function (res2) {
							if (res2.RESULT) {
								let saResCountry = res2.DATA.rel,
									sHtmlCountry = $('#temp_Countries').render(saResCountry);
									$("#Area_" + LogData.id).html(sHtmlCountry);
							}
						});
					})
                }
            });
        },
		/*
        * 目的 抓取展覽類別
        */
        fnGetExhibitCategory = function () {
            return g_api.ConnectLite(Service.apiappcom, ComFn.GetArguments, {
                ArgClassID: 'ExhibClass',
                LevelOfArgument: 0
            }, function (res) {
                if (res.RESULT) {
                    let saResCategory = res.DATA.rel,
						sHtmlCategory = $('#temp_ExhibitCategory').render(saResCategory);
                    $("#ulExhibitCategory").html(sHtmlCategory);
                }
            });
        },
        /*
        * 目的 抓去展覽資訊前n筆
        */
        fnGetExhibitionsTop = function () {
            var sKeyWords = $('#keyword').val(),
                sArea = $('#area').val(),
                sDateStart = $('#datestart').val(),
                sDateEnd = $('#dateend').val(),
				sCategory = "";

            if (!sDateStart) {
                sDateStart = newDate();
            }
            g_api.ConnectLite(Service.apiwebcom, ComFn.GetExhibitionPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                IsShowWebSim: "Y",
                KeyWords: sKeyWords,
                Area: CountryChecked.toString(),
                DateStart: sDateStart,
                DateEnd: sDateEnd,
				Category: CategoryChecked.toString(),
				Top: true,
				Code: sCode
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_expo').render(oRes.DataList);
                    $('.ExpoList').html(sHtml);

                    $("#page").pagination({
                        items: oRes.Total,
                        itemsOnPage: iPageCount,
                        currentPage: iPageIndex,
                        displayedPages: 4,
                        cssStyle: 'light-theme',
                        onPageClick: fnChangePage
                    });
                    if (oRes.Total <= iPageCount) { $("#pager").hide(); }
					
					if(sCode != null){
						$('html, body').animate({
						scrollTop: $("#expoSearchFilter").offset().top
					  }, "slow")
					}
                }
            });
        },
        /*
        * 目的 抓去活動資訊分頁資訊
        */
        fnChangePage = function () {
            iPageIndex = $("#page").pagination('getCurrentPage');
            fnGetExhibitionsTop();
        },
        init = function () {
            var myHelpers = {
                getYear: function (date) {
                    var y = new Date(date).getFullYear();
                    return y;
                },
                getMonth: function (date) {
                    var m = new Date(date).getMonth();
                    return m + 1;
                },
                setRangeDate: function (date1, date2) {
                    var r1 = new Date(date1).formate('yyyy/MM/dd'),
                        r2 = new Date(date2).formate('yyyy/MM/dd');
                    return r1 + ' ~ ' + r2;
                },
				setDate: function (date) {
					var dt = new Date(date).formate('yyyy/MM/dd');
					if(dt === "1970/01/01"){
						dt = "";
					}
                    return dt;
                },
                setFilePath: function (val) {
                    val = val || '';
					if(val != ''){
						val = gServerUrl + '/' + val.replace(/\\/g, "\/")
					}
                    return val;
                },
                setContent: function (val) {
                    val = val || '';
                    return val.length > 66 ? val.substr(0, 66) + '...' : val;
                }
            };
            $.views.helpers(myHelpers);
            if (bEn) {
                $.datepicker.setDefaults($.datepicker.regional[""]);
            }
            $(".datepicker").datepicker({
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd'
            });

            $.whenArray([
				g_api.ConnectLite(Service.apiappcom, ComFn.GetSysSet, {
					SetItemID: 'ExhibitionsShowCount'
				}, function (res) {
					if (res.RESULT) {
						iPageCount = parseInt(res.DATA.rel || 10);
					}
				}), 
				//fnGetArguments(),
				fnGetExhibitCategory()
			]).done(function () {
                fnGetExhibitionsTop();
				
				$('input[name="category"]').on('change', function(){
					CategoryChecked = [];
					CategoryCheckedName = [];
					$('input[name="category"]:checked').each(function(index, checkbox){
					  CategoryChecked.push($(checkbox).attr('value'));
					  CategoryCheckedName.push($(checkbox).attr('dataName'));
					 });
					let html = "";
					for (let i = 0; i < CategoryChecked.length; ++i) {
						html += "<li>"+CategoryCheckedName[i]+"</li>"
					}
					$( "div.catselect").html("<ul class='tags'>"+html+"</ul>");
				});
				
				//setTimeout(function () {
					$('input[name="country"]').on('change', function(){
						CountryChecked = [];
						CountryCheckedName = [];
						$('input[name="country"]:checked').each(function(index, checkbox){
							CountryChecked.push($(checkbox).attr('value'));
							CountryCheckedName.push($(checkbox).attr('dataName'));
						});
						
						let html = "";
						for (let i = 0; i < CountryChecked.length; ++i) {
							html += "<li>"+CountryCheckedName[i]+"</li>"
						}
						$( "div.countryselect").html("<ul class='tags'>"+html+"</ul>");
					});
				//}, 1000);
            });
			
            $('#btnSearch').on('click', function () {
                iPageIndex = 1;
                fnGetExhibitionsTop();
            });
			
			$('#btnClear').on('click', function () {
                $("#keyword").val("");
				$("#datestart").val("");
				$("#dateend").val("");
				$(".catselect").html("<span class=\"disabledText\">選擇</span>");
				$(".countryselect").html("<span class=\"disabledText\">選擇</span>");
				CategoryChecked = [];
				CategoryCheckedName = [];
				CountryChecked = [];
				CountryCheckedName = [];
				$('input[name="category"]:checked').each(function(index, checkbox){
					$(this).prop('checked', false);
				});
				$('input[name="country"]:checked').each(function(index, checkbox){
					$(this).prop('checked', false);
				});
            });
        };

    init();
});