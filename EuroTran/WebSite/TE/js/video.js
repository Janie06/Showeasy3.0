$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        /*
        * 目的 抓取視頻設定
        */
        fnSetVideos = function (org) {
            return fnGetWebSiteSetting(function (saRes) {
                if (saRes.length > 0) {
                    var sHtml = $('#temp_video').render(saRes);
                    $('.forVideo').html(sHtml);
                }
            }, 'Video', sLang)
        },
        init = function () {
            var myHelpers = {
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                },
                setFileName: function (val) {
                    return val.split('.')[0] || '';
                },
                setLink: function (val) {
                    var elVideo = '<iframe src="' + val + '" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen class="video"></iframe>';
                    if (!val) {
                        elVideo = $('<img/>', { width: '100%', height: '100%', src: (sLang === 'zh-TW' ? '' : '../') + 'images/comingsoon.jpg' })[0].outerHTML;
                    }
                    return elVideo;
                },
            };
            $.views.helpers(myHelpers);

            fnSetVideos();
        };

    init();
});