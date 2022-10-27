$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        iNewsTopCount = 8,
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
        * 目的 會展花絮輪播圖片
        */
        fnSwiper_Exp = function () {
            if ($(window).width() > 801) {
                new Swiper('.exposlide', {
                    slidesPerView: 4,
                    slidesPerGroup: 4,
                    spaceBetween: 15,
                    autoplay: 5000,
                    loop: true,
                    simulateTouch: false, //重要!影響chrome電腦點擊
                    navigation: {
                        nextEl: '#indexExpo .swiper-button-next',
                        prevEl: '#indexExpo .swiper-button-prev'
                    }
                });
            } else {
                new Swiper('.exposlide', {
                    autoplay: 5000,
                    loop: true,
                    navigation: {
                        nextEl: '#indexExpo .swiper-button-next',
                        prevEl: '#indexExpo .swiper-button-prev'
                    }
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
                    $('#bannerWrapper .swiper-wrapper').html(sHtml);
                    //$(".recThumb").imgLiquid({ fill: true });
                }
                if (saRes.length > 1) {
                    $('.NoShowInMobile').show();
                    fnSwiper();
                }
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
                    $('#indexExpo .indexGallery').html(sHtml);
                    $(".rectThumb").imgLiquid({ fill: true });
                    fnSwiper_Exp();
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
        };

    init();
});