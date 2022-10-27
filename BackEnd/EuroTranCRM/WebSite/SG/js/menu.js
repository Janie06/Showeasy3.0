$(function () {
    var win = $(window),
        sLang = $('[http-equiv="content-language"]').attr('content') || 'zh',
        bEn = sLang === 'en',
        pathname = window.location.pathname,
        seleLast = pathname.replace("/SG/en/", "").replace("/SG/", "").replace(".html", ""),
        rand_no = Math.floor((Math.random() * 4) + 1),//避免零
        menu = $("#headerWrapper h4"),
        submenu = $("ul#menu"),
        content = $(".article"),
        a_logo = $("#headerWrapper>h1>a"),
        h1_bg = $("h1.addBG"),
        open = function () {
            submenu.toggle(200);
            content.bind("click", close);
            menu.toggleClass("gray");
            win.bind("scroll", close);
        },
        close = function () {
            submenu.fadeOut(200);
            content.unbind("click");
            menu.removeClass("gray");
            win.unbind("scroll");
        },
        /*
        * 目的 設置logo
        */
        fnSetLogo = function (org) {
            g_api.ConnectLite(Service.apiappcom, ComFn.GetFileList, {
                ParentID: org.WebsiteLgoId
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    if (saRes.length > 0 && saRes[0].FilePath) {
                        a_logo.attr('style', 'background-image: url(' + gServerUrl + '/' + saRes[0].FilePath.replace(/\\/g, "\/") + ');');
                    }
                }
            });
        };

    g_ul.SetLang(sLang);

    $("li#" + seleLast).addClass("selected");

    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('#toTop').fadeIn(300);
        }
        else {
            $('#toTop').fadeOut(200);
        }
    });

    $("#toTop").click(function () {
        $('html, body').animate({
            scrollTop: $("body").offset().top
        }, 500);
    });

    menu.bind("click", open);

    window.scrollTo(0, 1);
    if (h1_bg.length > 0) {
        h1_bg.addClass("BG" + rand_no);
    }
    if (a_logo.length > 0) {
        runByOrgInfo(fnSetLogo);
    }
});