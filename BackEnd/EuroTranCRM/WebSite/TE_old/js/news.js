$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        iPageIndex = 1,
        iPageCount = 10,
        /*
        * 目的 抓去活動資訊明細
        * @param {String} id 當前消息ID
        * @return {Object} ajax物件
        */
        fnGetNewsInfo = function (id) {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsInfo, {
                Id: id
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel,
                        sHtml = $('#temp_newsdetail').render(saRes);
                    $('.news-detail').html(sHtml).find('a').click(function () {
                        $('#p_news_detail').hide();
                        $('#p_news_list').show();
                    });
                    $('#p_news_detail').show();
                    $('#p_news_list').hide();
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
                NewsType: '01'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_newslist').render(oRes.DataList);
                    $('.new-news').html(sHtml).find('a').click(function () {
                        var sId = $(this).attr('data-key');
                        fnGetNewsInfo(sId);
                    });//oRes.Total

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
            fnGetNewsPage();
        },
        init = function () {
            var myHelpers = {
                setDate: function (date) {
                    var dDate = new Date(date),
                        iDay = dDate.getDate(),
                        monName = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");
                    return monName[dDate.getMonth()] + ' ' + (iDay < 10 ? '0' + iDay : iDay) + ', ' + dDate.getFullYear();
                },
                setNewsContent: function (val) {
                    val = val || '';
                    if (window.location.protocol === 'https:') {
                        val = val.replaceAll('http:', 'https:');
                    }
                    return val;
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