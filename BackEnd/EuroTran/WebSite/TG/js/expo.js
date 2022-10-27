$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        iPageIndex = 1,
        iPageCount = 10,
        /*
        * 目的 抓取國家
        */
        fnGetArguments = function () {
            return g_api.ConnectLite(Service.apiappcom, ComFn.GetArguments, {
                ArgClassID: 'Area',
                OrderBy: 'id',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    $('#area').html(createOptions(saRes, 'id', bEn ? 'text_en' : 'text', true)).val('TWN').select2({
                        placeholder: bEn ? 'Select Country' : '請選擇國家'
                    });
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
                sDateEnd = $('#dateend').val();

            if (!sDateStart) {
                sDateStart = newDate();
            }
            g_api.ConnectLite(Service.apiwebcom, ComFn.GetExhibitionPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                IsShowWebSim: "Y",
                KeyWords: sKeyWords,
                Area: sArea,
                DateStart: sDateStart,
                DateEnd: sDateEnd
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_expo').render(oRes.DataList);
                    $('.expoList').html(sHtml);
                    $(".squareThumb").imgLiquid({ fill: false });

                    $("#pager").pagination({
                        items: oRes.Total,
                        itemsOnPage: iPageCount,
                        currentPage: iPageIndex,
                        displayedPages: 4,
                        cssStyle: 'light-theme',
                        onPageClick: fnChangePage
                    });
                    if (oRes.Total <= iPageCount) { $("#pager").hide(); }
                }
            });
        },
        /*
        * 目的 抓去活動資訊分頁資訊
        */
        fnChangePage = function () {
            iPageIndex = $("#pager").pagination('getCurrentPage');
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
                        r2 = new Date(date2).formate('MM/dd');
                    return r1 + '-' + r2;
                },
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
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

            $.whenArray([g_api.ConnectLite(Service.apiappcom, ComFn.GetSysSet, {
                SetItemID: 'ExhibitionsShowCount'
            }, function (res) {
                if (res.RESULT) {
                    iPageCount = parseInt(res.DATA.rel || 10);
                }
            }), fnGetArguments()]).done(function () {
                fnGetExhibitionsTop();
            });
            $('[type="submit"]').on('click', function () {
                iPageIndex = 1;
                fnGetExhibitionsTop();
            });
        };

    init();
});