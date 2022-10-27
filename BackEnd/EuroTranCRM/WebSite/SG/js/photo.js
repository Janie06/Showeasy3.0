$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh',
        iPageIndex = 1,
        iPageCount = 6,
        /*
        * 目的 抓去服務花絮前n筆
        */
        fnGetNewsTop = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                NewsType: '02'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_news').render(oRes.DataList);
                    $('.gallery').html(sHtml + '<p class="clear"></p>');
                    $(".rectThumb").imgLiquid({ fill: true });
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
            fnGetNewsTop();
        },
        init = function () {
            var myHelpers = {
                setDate: function (date) {
                    return new Date(date).formate('yyyy/MM/dd');
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
                setFileName: function (val) {
                    val = val || '';
                    return val.split('.')[0] || '';
                },
                setContent: function (val) {
                    val = val || '';
                    return val.length > 66 ? val.substr(0, 66) + '...' : val;
                }
            };
            $.views.helpers(myHelpers);

            fnGetNewsTop();
        };

    init();
});