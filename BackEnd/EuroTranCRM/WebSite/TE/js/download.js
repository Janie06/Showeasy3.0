$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        sUniqueID = 'TEFilesDownLoad_' + { 'zh-TW': 'TW', 'zh': 'CN', 'en': 'EN' }[sLang],
        iPageIndex = 1,
        iPageCount = 10,
        /*
        * 目的 抓去活動資訊分頁資訊
        * @return {Object} ajax物件
        */
        fnGetFilesPage = function () {
            return fnGetWebSiteSettingPage({
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                SetType: 'Downloads',
                LangId: sLang,
                CallBack: function (res) {
                    var sHtml = $('#temp_download').render(res.DataList);
                    $('#download').html(sHtml + '<p class="clear">&nbsp;</p>').find('li a').on('click', function () {
                        var sName = $(this).attr('filename'),
                            sPath = $(this).attr('filepath');
                        DownLoadFile(sPath, sName);
                    });
                    $("#page").pagination({
                        items: res.Total,
                        itemsOnPage: iPageCount,
                        currentPage: iPageIndex,
                        displayedPages: 4,
                        cssStyle: 'light-theme',
                        onPageClick: fnChangePage
                    });
                    if (res.Total <= iPageCount) { $("#page").hide(); }
                }
            });
        },
        /*
        * 目的 抓去活動資訊分頁資訊
        */
        fnChangePage = function () {
            iPageIndex = $("#page").pagination('getCurrentPage');
            fnGetFilesPage();
        },
        init = function () {
            var myHelpers = {
                setFileTitle: function (name, desc) {
                    return name + (!desc ? '' : '（' + desc + '）');
                },
                getFileName: function (val) {
                    return val.split('.')[0];
                }
            };
            $.views.helpers(myHelpers);

            g_api.ConnectLite(Service.apiappcom, ComFn.GetSysSet, {
                SetItemID: 'FilesShowCount'
            }, function (res) {
                if (res.RESULT) {
                    iPageCount = parseInt(res.DATA.rel || 10);
                }
            }).always(function () {
                fnGetFilesPage(iPageIndex);
            });
        };

    init();
});