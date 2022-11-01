$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        /*
        * 目的 抓去展示圖片
        */
        fnGetPicShow = function (org) {
            return g_api.ConnectLite(Service.apiappcom, ComFn.GetFileList, {
                ParentID: bEn ? org.PicShowId_EN : org.PicShowId
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel,
                        sHtml = $('#temp_aboutpic').render(saRes);
                    if (saRes.length > 0) {
                        $('.aboutPic').html(sHtml + '<p class="clear">&nbsp;</p>');
                    }
                }
            });
        },
        /*
        * 目的 設置簡介
        */
        fnSetIntroduction = function (org) {
            if (bEn) {
                if (org.Introduction_EN) {
                    $('#Introduction').html(org.Introduction_EN);
                }
            }
            else {
                if (org.Introduction) {
                    $('#Introduction').html(org.Introduction);
                }
            }
        },
        init = function () {
            var myHelpers = {
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                }
            };
            $.views.helpers(myHelpers);

            runByOrgInfo(fnSetIntroduction);
            runByOrgInfo(fnGetPicShow);
        };

    init();
});