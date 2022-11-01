$(function () {
    var win = $(window),
        sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        mobile_menu = $("h4.openMenu"),
        mobile_submenu = $("ul#menu"),
        mobile_content = $("footer, header, article"),
        a_logo = $("#headerWrapper>header>h1>a"),
        h1_bg = $("#innerTitle.addBG"),
        menu = $("#left h4"),
        submenu = $("#left ul"),
        content = $("#right,h1"),
        rand_no = Math.floor((Math.random() * 3) + 1),//避免零
        paths = window.location.pathname.split(/[\/]/),
        pname = paths[paths.length - 1].split('.')[0],
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
        mobile_open = function () {
            mobile_submenu.toggle(200);
            mobile_content.bind("click", mobile_close);
            mobile_menu.toggleClass("changColor");
            //win.bind("scroll",close);
        },
        mobile_close = function () {
            mobile_submenu.fadeOut(200);
            mobile_content.unbind("click");
            mobile_menu.removeClass("changColor");
            //win.unbind("scroll");
        },
        /*
        * 目的 設置logo
        */
        fnSetLogo = function (org) {
            g_api.ConnectLite(Service.apiappcom, ComFn.GetFileList, {
                ParentID: bEn ? org.WebsiteLgoId_EN : org.WebsiteLgoId
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    if (saRes.length > 0 && saRes[0].FilePath) {
                        a_logo.attr('style', 'background-image: url(' + gServerUrl + '/' + saRes[0].FilePath.replace(/\\/g, "\/") + ');');
                    }
                }
            });
        },
        /*
        * 目的 設置菜單
        */
        fnSetMenu = function (org) {
            return $.whenArray([
                fnGetWebSiteSetting(function (saRes) {
                    var sHtml = $('<script />', { type: 'text/x-jsrender', html: '<li><a href="service.html?Id={{:Guid}}">{{:Title}}</a></li>' }).render(saRes);
                    if (saRes.length > 0) {
                        $('#service>ul').html(sHtml);
                    }
                }, 'ServiceItems', sLang),
                fnGetWebSiteSetting(function (saRes) {
                    var sHtml = $('<script />', { type: 'text/x-jsrender', html: '<li><a href="{{:Link}}?T={{:Guid}}">{{:Title}}</a></li>' }).render(saRes);
                    if (saRes.length > 0) {
                        $('#case>ul').html(sHtml);
                    }
                }, 'ClassicCase', sLang)])
                .done(function () {
                    //menu 效果
                    $('#menu li').hover(function () {
                        if (win.width() > 801) {
                            $(this).css({ backgroundColor: "#333F48" });
                        } else {
                            $(this).css({ backgroundColor: "#cccccc" });
                            $(this).siblings().children("ul").slideUp(400);//若選別的子選項關閉
                            $(this).siblings().removeClass("selected");//若選別的則移除樣式
                        }
                        $(this).children('ul').stop(true, true).slideDown(200);
                    }, function () {
                        $(this).css({ backgroundColor: "" }).children('ul').stop(true, true).slideUp(400);
                    });
                });
        },
        fnGetNewsCount = function () {
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


        };

    g_ul.SetLang(sLang);

    if (pname === "index" || pname === "") { $("li#index").addClass("selected"); }
    if (pname.match('about')) { $("li#about").addClass("selected"); } //只要檔名包含press就抓出來
    if (pname.match('online')) { $("li#online").addClass("selected"); }
    if (pname.match('video')) { $("li#video").addClass("selected"); }
    if (pname.match('news')) { $("li#news").addClass("selected"); }
    if (pname.match('service')) { $("li#service").addClass("selected"); }
    if (pname.match('case')) { $("li#case").addClass("selected"); }
    if (pname.match('contact')) { $("li#contact").addClass("selected"); }

    //手機板選中時子選項要打開
    if (win.width() < 801) {
        $('#menu li.selected').children("ul").show();
    }
    // to top
    if (win.width() > 800) {
        win.scroll(function () {
            if ($(this).scrollTop() > 500) {
                $('#toTop').fadeIn(300);
            }
            else {
                $('#toTop').fadeOut(200);
            }
        });
    }
    $("#toTop").click(function () {
        $('html, body').animate({
            scrollTop: $("body").offset().top
        }, 1000);
    });

    $('.wechat').on('click', function () {
        $.fancybox.open({
            src: (sLang === 'zh-TW' ? '' : '../') + 'images/wx_euro.jpg',
            padding: 5,
            closeBtn: true
        });
    });

    fnSetMenu();
    fnGetNewsCount();

    window.scrollTo(0, 1);
    //menu for mobile
    mobile_menu.bind("click", mobile_open);
    //sub menu
    menu.bind("click", open);

    if (h1_bg.length > 0) {
        h1_bg.addClass("BG" + rand_no);
    }
    if (a_logo.length > 0 && typeof runByOrgInfo === 'function') {
        runByOrgInfo(fnSetLogo);
    }

    (function (i, s, o, g, r, a, m) {
        i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () { (i[r].q = i[r].q || []).push(arguments) }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m);
    })(window, document, 'script', 'https://www.google-analytics.com/analytics.js', 'ga');

    ga('create', 'UA-97376330-1', 'auto');
    ga('send', 'pageview');
});