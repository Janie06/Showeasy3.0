$(function () {
    'use strict';
    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh',
        sId = getUrlParam('id') || '1',
        /*
        * 目的 抓去服務花絮前n筆
        */
        fnGetNewsTop = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsInfo, {
                Id: sId,
                IncludeFiles: true
            }, function (res) {
                if (res.RESULT) {
                    res.DATA.files = res.DATA.files.sort(function (a, b) {
                        return a.OrderByValue > b.OrderByValue ? 1 : -1;
                    })
                    var oRes = res.DATA.rel[0] || {},
                        saPhotos = res.DATA.files,
                        sHtml = $('#temp_photo').render(saPhotos || []);
                    $('#Title').html(oRes.News_Title).attr('class','orange');
                    $('#Content').html(oRes.News_Content);
                    $('.gallery').html(sHtml + '<p class="clear"></p>');
                    $(".rectThumb").imgLiquid({ fill: true });
                }
            });
        },
        init = function () {
            var myHelpers = {
                setFilePath: function (val) {
                    val = val || '';
                    //return 'http://localhost:3466' + '/' + val.replace(/\\/g, "\/");
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                },
                setFileName: function (val) {
                    val = val || '';
                    return val.split('.')[0] || '';
                }
            };
            $.views.helpers(myHelpers);

            fnGetNewsTop();
        };

    init();
});