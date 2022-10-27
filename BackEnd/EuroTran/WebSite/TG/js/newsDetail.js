$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        sId = getUrlParam('id') || '1',
        iPageIndex = 1,
        iPageCount = 2,
        /*
        * 目的 抓去活動資訊明細
        * @param {String} id 當前消息ID
        * @return {Object} ajax物件
        */
        fnGetNewsInfo = function (id) {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsInfo, {
                Id: sId
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel,
                        sHtml = $('#temp_newsdetail').render(saRes);
                    $('#newsDetail').html(sHtml);
                }
            });
        },
        /*
        * 目的 抓去活動資訊分頁資訊
        * @return {Object} ajax物件
        */
        fnGetNewsPage = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                NewsType: '02'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_news').render(oRes.DataList);
                    $('.listNews').html(sHtml);
                }
            });
        },
        init = function () {
            var myHelpers = {
                setDate: function (date) {
                    return new Date(date).formate('yyyy/MM/dd');
                },
                setNewsContent: function (val) {
                    val = val || '';
                    val = val.replaceAll('http:', 'https:');
                    return val;
                }
            };
            $.views.helpers(myHelpers);

            fnGetNewsInfo(sId);

            g_api.ConnectLite(Service.apiappcom, ComFn.GetSysSet, {
                SetItemID: 'NewsShowCount'
            }, function (res) {
                if (res.RESULT) {
                    iPageCount = parseInt(res.DATA.rel || 10);
                }
            }).always(function () {
                fnGetNewsPage(iPageIndex);
            });
        };

    init();
});