$(function () {
    'use strict';

    if ($('.counter').length > 0) {
        $('.counter').counterUp({
            delay: 100,
            time: 2000
        });
    }
    if ($('.wow').length > 0) {
        new WOW().init();
    }
    if ($('#gotop').length > 0) {
        $('#gotop').gotop({
            content: 980,
            bottom: 60,
            margin: "none",
            position: "right",
            scrollTop: 100,
            duration: 700
        });
    }

    $('.color-wechat,.wechat').on('click', function () {
        $.fancybox.open({
            content: '<img src="../img/wx_euro.jpg" />',
            padding: 5,
            closeBtn: true
        });
    });

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW';
    g_api.ConnectLite(Service.apite, ComFn.GetNewsCount, {
        Lang: sLang
    }, function (res) {
        if (res.RESULT) {
            var iNewsCount = res.DATA.rel;
            if (iNewsCount > 0) {
                $('.news-tips').show();
            }
            else {
                $('.news-tips').hide();
            }
        }
    });
    g_ul.SetLang(sLang);

    (function (i, s, o, g, r, a, m) {
        i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () { (i[r].q = i[r].q || []).push(arguments) }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
    })(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');

    ga('create', 'UA-97376330-1', 'auto');
    ga('send', 'pageview');
});