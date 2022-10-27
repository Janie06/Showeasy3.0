$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        iExhibitionTopCount = 5,
        iNewsTopCount = 4,
        org = null,
        /*
        * 目的 初始化輪播圖片
        */
        fnSwiper = function () {
            var iLen = $(".bannerslide ul li").size(),//抓數量
                iRand_no = Math.floor(Math.random() * iLen);//避免零
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
        * 目的 抓去服務
        */
        fnGetService = function (org) {
            return fnGetWebSiteSetting(function (saRes) {
                var sHtml = $('#temp_service').render(saRes);
                if (saRes.length > 0) {
                    $('#listservice').html(sHtml + '<p class="clear">&nbsp;</p>');
                }
            }, 'ServiceItems', sLang);
        },
        /*
        * 目的 設置視頻專區
        */
        fnGetVideo = function (org) {
            var sVideoUrl = { 'zh-TW': org.VideoUrl, 'zh': org.VideoUrl_CN, 'en': org.VideoUrl_EN }[sLang] || '',
                sVideoDescription = { 'zh-TW': org.VideoDescription, 'zh': org.VideoDescription_CN, 'en': org.VideoDescription_EN }[sLang] || '';
            if (sVideoUrl) {
                $('#VideoUrl').attr('src', sVideoUrl);
            }
            if (org.sVideoDescription) {
                $('#VideoDescription').html(sVideoDescription);
            }
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
                    val = val || '';
                    return val.split('.')[0] || '';
                },
                setContent: function (val) {
                    val = val || '';
                    return val.length > 66 ? val.substr(0, 66) + '...' : val;
                }
            };
            $.views.helpers(myHelpers);

            runByOrgInfo(fnGetBanners);
            runByOrgInfo(fnGetService);
            runByOrgInfo(fnGetVideo, true);
            fnGetNewsTop();
            fnGetExhibitionsTop();
        };

    init();
});