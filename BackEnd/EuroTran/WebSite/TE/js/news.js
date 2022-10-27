$(function () {
    'use strict';
    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        iPageIndex = 1,
        iPageCount = 10,
        /*
        * 目的 抓去活動資訊分頁資訊
        * @return {Object} ajax物件
        */
        fnGetNewsPage = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                NewsType: '01'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_news').render(oRes.DataList);
                    $('#newsList').html(sHtml);
                    $(".rectThumb").imgLiquid({ fill: true });

                    $("#page").pagination({
                        items: oRes.Total,
                        itemsOnPage: iPageCount,
                        currentPage: iPageIndex,
                        displayedPages: 4,
                        cssStyle: 'light-theme',
                        onPageClick: fnChangePage
                    });
                    if (oRes.Total <= iPageCount) { $("#page").hide(); }
                }
            });
        },
        /*
        * 目的 抓去活動資訊分頁資訊
        */
        fnChangePage = function () {
            iPageIndex = $("#page").pagination('getCurrentPage');
            fnGetNewsPage();
        },
        init = function () {
            var myHelpers = {
                setDate: function (date) {
                    return new Date(date).formate('yyyy/MM/dd');
                },
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                },
                setContent: function (val) {
                    val = val || '';
                    return val.length > 266 ? val.substr(0, 266) + '...' : val;
                }
            };
            $.views.helpers(myHelpers);

            g_api.ConnectLite(Service.apiappcom, ComFn.GetSysSet, {
                SetItemID: 'NewsShowCount'
            }, function (res) {
                if (res.RESULT) {
                    iPageCount = parseInt(res.DATA.rel || 10);
                }
            }).always(function () {
                fnGetNewsPage();
            });
        };

    init();
});