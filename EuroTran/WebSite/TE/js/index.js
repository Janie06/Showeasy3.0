$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        win = $(window),
        iExhibitionTopCount = 5,
        iNewsTopCount = 4,
        org = null,
        bMobile = navigator.userAgent.match(/mobile/i),
        /*
        * 目的 初始化輪播圖片
        */
        fnSwiper = function () {
            var iLen = $(".bannerslide ul li").size();//抓數量
            var iRand_no = Math.floor(Math.random() * iLen);//避免零
            if ($(window).width() > 801) {
                var mySwiper = new Swiper('.bannerslide', {
                    paginationClickable: true,
                    slidesPerView: 'auto',
                    loop: true,
                    centeredSlides: true,
                    spaceBetween: 0,
                    initialSlide: 0,
                    navigation: {
                        nextEl: '.banner-next',
                        prevEl: '.banner-prev'
                    },
                    autoplay: {
                        delay: 5000
                    },
                    pagination: {
                        el: '.swiper-pagination',
                        clickable: true
                    }
                });
            }
            else {
                var swiperBanner = new Swiper('.bannerslide', {
                    autoplay: {
                        delay: 5000
                    },
                    pagination: {
                        el: '.swiper-pagination',
                        clickable: true
                    },
                    loop: true,
                    initialSlide: iRand_no
                });
            }
        },
        /*
        * 目的 抓去輪播
        */
        fnGetBanners = function (org) {
            return fnGetWebSiteSetting(function (saRes) {
                var sHtml = $('#temp_banner').render(saRes);
                if (saRes.length > 0) {
                    $('.swiper-wrapper').html(sHtml);
                }
                fnSwiper();
            }, 'Carousel', sLang);
        },
        /*
        * 目的 抓去服務項目
        */
        fnGetService = function () {
            return fnGetWebSiteSetting(function (saRes) {
                var sHtml = $('#temp_service').render(saRes);
                if (saRes.length > 0) {
                    $('#listservice').html(sHtml + '<p class="clear">&nbsp;</p>');
                }
            }, 'ServiceItems', sLang);
        },
        /*
        * 目的 抓去經典案例
        */
        fnGetClassicCase = function () {
            return fnGetWebSiteSetting(function (saRes) {
                if (saRes.length > 0) {
                    var sHtmlTab = $('#temp_classiccasetab').render(saRes),
                        sHtmlDiv = $('#temp_classiccasediv').render(saRes),
                        iTabHeight = 320 / saRes.length + 'px';
                    $('#tabsNav').html(sHtmlTab).find('li a');
                    $('.tab_contentr').html(sHtmlDiv);

                    // 預設顯示第一個 Tab
                    var elTabFirst = $('#tabsNav li').eq(0);
                    elTabFirst.addClass('active');
                    if (!bMobile) {
                        $('#tabsNav').find('li a').css({ height: iTabHeight, 'line-height': iTabHeight });
                        elTabFirst.find('a').css('background-image', elTabFirst.attr('data-activeicon'));
                    }
                    else {
                        $('#tabsNav li').find('a').css('background-image', 'none');
                    }
                    $('.tab_content').hide().eq(0).show();

                    // 當 li 頁籤被點擊時...
                    // 若要改成滑鼠移到 li 頁籤就切換時, 把 click 改成 mouseover
                    $('#tabsNav li').click(function () {
                        // 找出 li 中的超連結 href(#id)
                        var $this = $(this),
                            _clickTab = $this.find('a').attr('href');
                        // 把目前點擊到的 li 頁籤加上 .active
                        // 並把兄弟元素中有 .active 的都移除 class
                        if (!bMobile) {
                        }
                        if (!bMobile) {
                            $this.siblings('.active').each(function () {
                                $(this).find('a').css('background-image', $(this).attr('data-icon'));
                            });
                            $this.find('a').css('background-image', $this.attr('data-activeicon'));
                        }
                        else {
                            $this.siblings('.active').each(function () {
                                $(this).find('a').css('background-image', 'none');
                            });
                            $this.find('a').css('background-image', 'none');
                        }
                        $this.addClass('active').siblings('.active').removeClass('active');
                        // 淡入相對應的內容並隱藏兄弟元素
                        $(_clickTab).stop(false, true).fadeIn(0).siblings().hide();

                        return false;
                    });
                }
            }, 'ClassicCase', sLang);
        },
        /*
        * 目的 設置視頻專區
        */
        fnGetVideo = function (org) {
            var sVideoUrl = { 'zh-TW': org.VideoUrl, 'zh': org.VideoUrl_CN, 'en': org.VideoUrl_EN }[sLang] || '';//_wyZT3laq8c,ztg_uJgfysU
            if (sVideoUrl) {
                $('#videoPlay').YTPlayer({
                    fitToBackground: false,
                    videoId: sVideoUrl,
                    playerVars: {
                        autoplay: 1,
                        controls: 0,
                        showinfo: 0,
                        branding: 0,
                        rel: 0,
                        autohide: 1
                    }
                });
                if (sLang === 'zh') {
                    setTimeout(function () {
                        var iframeVideo = $('<iframe  frameborder=0  allow="autoplay; encrypted-media" allowfullscreen class="video" style="width: 623px; height: 350px; left: -13px; top: 0px;" src="' + sVideoUrl + '"/>');
                        $('[id^="ytplayer-container"]').html('').append(iframeVideo);
                    }, 2000);
                }
            }
            else {
                $('#videoPlay').css('background-image', 'url(' + (sLang === 'zh-TW' ? '' : '../') + 'images/comingsoon.jpg)');
            }
        },
        /*
        * 目的 設置最新消息圖檔
        */
        fnNewPic = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: 1,
                pageSize: 1,
                NewsType: '01'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sNews_PicPath = oRes.DataList[0].News_PicPath || '';
                    $('#newsbox').css('background-image', 'url(' + gServerUrl + '/' + sNews_PicPath.replace(/\\/g, "\/") + ')');
                }
            });
        },
        /*
        * 目的 設置展會花絮圖檔
        */
        fnPhotoPic = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: 1,
                pageSize: 1,
                NewsType: '02'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sNews_PicPath = oRes.DataList[0].News_PicPath || '';
                    $('#photobox').css('background-image', 'url(' + gServerUrl + '/' + sNews_PicPath.replace(/\\/g, "\/") + ')');
                }
            });
        },
        /*
        * 目的 抓去服務花絮前n筆
        */
        fnGetNewsTop = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetNewsPage, {
                pageIndex: 1,
                pageSize: iNewsTopCount,
                NewsType: '02'
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_news').render(oRes.DataList);
                    $('.news-box').html(sHtml);
                    $(".rectThumb").imgLiquid({ fill: true });
                }
            });
        },
        /*
        * 目的 抓去展覽資訊前n筆
        */
        fnGetExhibitionsTop = function () {
            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetExhibitionPage, {
                pageIndex: 1,
                pageSize: iExhibitionTopCount,
                IsShowWebSim: "Y",
                Top: true
            }, function (res) {
                debugger
                if (res.RESULT) {
                    var oRes = res.DATA.rel,
                        sHtml = $('#temp_expo').render(oRes.DataList);
                    $('.expo-box').html(sHtml);
                    $(".squareThumb").imgLiquid({ fill: false });
                }
            });
        },
        init = function () {
            var myHelpers = {
                setDate: function (date) {
                    return new Date(date).formate('yyyy/MM/dd');
                },
                setRangeDate: function (date1, date2) {
                    var r1 = new Date(date1).formate('yyyy/MM/dd'),
                        r2 = new Date(date2).formate('MM/dd');
                    return r1 + '-' + r2;
                },
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                },
                setFileName: function (val) {
                    return val.split('.')[0] || '';
                },
                setContent: function (val) {
                    val = val || '';
                    return val.length > 66 ? val.substr(0, 66) + '...' : val;
                }
            };
            $.views.helpers(myHelpers);

            // ani
            if (win.width() > 801) {
                win.scroll(function () {
                    //service
                    var getSerTop = $("#indexService").offset().top - 300;
                    if ($(this).scrollTop() > getSerTop) {
                        $(".indexServiceLeft").addClass("leftFlyIn");
                        $(".indexServiceRight").addClass("rightFlyIn");
                    }
                    //indexCase
                    var getCaseTop = $(".indexCaseLeft").offset().top - 300;
                    if ($(this).scrollTop() > getCaseTop) {
                        $(".indexCaseLeft, .indexCaseRight").addClass("fadeIn");
                    }
                    //indexlink
                    var getLinkTop = $(".serviceLink").offset().top - 300;
                    if ($(this).scrollTop() > getLinkTop) {
                        $(".serviceLink").addClass("fadeIn");
                    }
                });
            }

            runByOrgInfo(fnGetBanners, true);
            fnGetService();
            fnGetClassicCase();
            runByOrgInfo(fnGetVideo);
            fnNewPic();
            fnPhotoPic();
            //fnGetNewsTop();
            //fnGetExhibitionsTop();
        };

    init();
});