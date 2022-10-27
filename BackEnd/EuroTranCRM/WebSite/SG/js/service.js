$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh',
        sAnchor = getUrlParam('anchor'),
        /*
        * 目的 抓去展示圖片
        */
        fnSetServiceItems = function (org) {
            return fnGetWebSiteSetting(function (saRes) {
                var sHtml = $('#temp_service').render(saRes);
                if (saRes.length > 0) {
                    $('.service').html(sHtml + '<p class="clear">&nbsp;</p>');
                    if (sAnchor) {
                        goToJys($('a[name="' + sAnchor + '"]'));
                    }
                }
            }, 'ServiceItems', sLang);
        },
        /*
        * 目的 設置簡介
        */
        fnSetServiceTitle = function (org) {
            var sServiceTitle = { 'zh-TW': org.ServiceTitle, 'zh': org.ServiceTitle_CN, 'en': org.ServiceTitle_EN }[sLang] || '';
            if (sServiceTitle) {
                $('#ServiceTitle').html(sServiceTitle);
            }
        },
        init = function () {
            var myHelpers = {
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                },
                setFileName: function (val) {
                    return val.split('.')[0] || '';
                }
            };
            $.views.helpers(myHelpers);

            runByOrgInfo(fnSetServiceTitle);
            runByOrgInfo(fnSetServiceItems, true);
        };

    init();
});