$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        sTab = getUrlParam('T'),
        /*
        * 目的 抓取公司簡介
        * @param {Object} org 組織明細
        */
        fnSetIntroduction = function (org) {
            $('#right').html({ 'zh-TW': org.Introduction, 'zh': org.Introduction_CN, 'en': org.Introduction_EN }[sLang]);
        },
        /*
        * 目的 抓取公司願景
        * @param {Object} org 組織明細
        */
        fnSetMissionAndVision = function (org) {
            $('#right').html({ 'zh-TW': org.MissionAndVision_TW, 'zh': org.MissionAndVision_CN, 'en': org.MissionAndVision_EN }[sLang]);
        },
        /*
        * 目的 設定地圖
        * @param {Object} org 組織明細
        */
        fnSetMap = function () {
            return fnGetWebSiteSetting(function (oRes) {
                if (oRes.length > 0) {
                    var mapInfo = oRes[0];
                    $('#right').html(' <div id="map" class="googleMap"></div>');
                    var map = null,
                        saPoints = $.parseJSON(mapInfo.Content || '[]'),
                        saStyleJson = $.parseJSON(mapInfo.Memo || '[]'),
                        oCenterPoint = $.parseJSON(mapInfo.Description || '{lat:"8.6",lng:"35.2"}'),
                        sIconUrl = !mapInfo.IconFilePath ? 'images/eurtoran_lo.png' : gServerUrl + '/' + mapInfo.IconFilePath.replace(/\\/g, "\/");
                    if (sLang === 'zh') {
                        map = new BMap.Map("map");
                        var pointArray = new Array(),
                            view = map.getViewport(saPoints),
                            mapZoom = view.zoom,
                            centerPoint = BMap.Point(oCenterPoint.lat, oCenterPoint.lng);
                        map.centerAndZoom(new BMap.Point(oCenterPoint.lat, oCenterPoint.lng), 4); //根据各个点自适应显示地图

                        if (saPoints.length === 0) {
                            saPoints = [
                                { lat: 23.5948856, lng: 121.4214155 }, //台灣
                                { lat: -6.3567715, lng: 145.9033085 }, //巴布亞
                                { lat: -23.6993532, lng: 133.8713752 }, //澳大利亞
                                { lat: 26.5604565, lng: 29.6630058 }, //埃及
                                { lat: 9.5577684, lng: 7.9979073 },//奈及利亞
                                { lat: 0.1953689, lng: 6.6126343 }, //聖多美普林西比
                                { lat: -30.6532947, lng: 23.9345003 }, //南非
                                { lat: -26.6682265, lng: 30.9023096 }, //史瓦濟蘭
                                { lat: -35.425776, lng: -65.9767868 }, //阿根廷
                                { lat: -8.5275228, lng: -55.8778956 }, //巴西
                                { lat: -26.8561446, lng: -71.4021298 },//智利
                                { lat: 4.1454439, lng: -73.5223993 }, //哥倫比亞
                                { lat: -23.4611862, lng: -58.4817976 }, //巴拉圭
                                { lat: -10.1091956, lng: -76.2822092 }, //秘魯
                                { lat: 19.0426862, lng: -70.2479896 }, //多明尼加
                                { lat: 57.8807689, lng: -101.6724135 }, //加拿大
                                { lat: 15.8120328, lng: -90.2990771 }, //瓜地馬拉
                                { lat: 15.0241668, lng: -87.0039418 }, //宏都拉斯
                                { lat: 24.6901136, lng: -102.0043495 },//墨西哥
                                { lat: 12.8889429, lng: -85.0106797 }, //尼加拉瓜
                                { lat: 8.6024001, lng: -80.2711767 }, //巴拿馬
                                { lat: 13.8973856, lng: -60.9842968 }, //聖露西亞
                                { lat: 40.2909474, lng: -101.809408 }, //美國
                                { lat: 41.6351161, lng: 21.711268 }, //馬其頓
                                { lat: 39.862204, lng: -3.2344494 }, //西班牙
                                { lat: 39.1082877, lng: 35.2500578 }, //土耳其
                                { lat: 42.7024418, lng: 25.1189025 }, //保加利亞
                                { lat: 43.9394954, lng: 17.6232668 }, //波士尼亞
                                { lat: 41.1857216, lng: 19.9760582 }, //阿爾巴尼亞
                                { lat: 49.8766304, lng: 15.1698544 }, //捷克
                                { lat: 53.2569956, lng: 18.6683763 }, //波蘭
                                { lat: 47.2047105, lng: 19.5553868 }, //匈牙利
                                { lat: 62.5290963, lng: 93.8171104 }, //俄羅斯
                                { lat: 63.0502944, lng: 16.6267692 }, //瑞典
                                { lat: 57.126913, lng: 26.0037774 }, //拉脫維亞
                                { lat: 48.8459645, lng: 19.4424029 }, //斯洛伐克
                                { lat: 51.1676818, lng: 10.4332196 }, //德國
                                { lat: 46.8463948, lng: 2.4559869 }, //法國
                                { lat: 55.3430558, lng: -3.3820532 }, //英國
                                { lat: 43.3089667, lng: 12.3977752 }, //義大利
                                { lat: 52.2590496, lng: 5.6466364 }, //荷蘭
                                { lat: 36.086822, lng: 103.4147709 }, //中國
                                { lat: 36.7191386, lng: 127.858362 }, //韓國
                                { lat: 37.0092457, lng: 139.8812684, }, //日本
                                { lat: 4.5928227, lng: 114.6181193 }, //汶萊
                                { lat: -3.2025712, lng: 121.9791652 }, //印尼
                                { lat: 4.1650768, lng: 102.0246564 }, //馬來西亞
                                { lat: 21.778682, lng: 96.5156388 }, //緬甸
                                { lat: 13.5488787, lng: 122.8379407 }, //菲律賓
                                { lat: 1.3593895, lng: 103.8616207 }, //新加坡
                                { lat: 15.5393972, lng: 100.9749041 }, //泰國
                                { lat: 15.268843, lng: 107.3972625 }, //越南
                                { lat: 23.6876726, lng: 79.4363431 }, //印度
                                { lat: 26.0593531, lng: 50.5436515 }, //巴林
                                { lat: 24.0776844, lng: 45.2586931 }, //沙烏地阿拉伯
                                { lat: 25.1583957, lng: 55.11235 }, //杜拜
                                { lat: 24.4631591, lng: 54.3505263 }, //阿布達比
                                { lat: 31.2373157, lng: 34.6096371 }, //以色列
                                { lat: 55.153022, lng: 21.6525519 }, //立陶宛
                                { lat: 53.6330892, lng: 23.4928235 }, //白俄羅斯
                            ];
                        }
                        if (saStyleJson.length === 0) {
                            saStyleJson = [
                                {
                                    "featureType": "land",
                                    "elementType": "all",
                                    "stylers": { "color": "#f7f7f7", "visibility": "on" }
                                },
                                {
                                    "featureType": "water",
                                    "elementType": "all",
                                    "stylers": { "color": "#e4e4e4", "visibility": "on" }
                                },
                                {
                                    "featureType": "road",
                                    "elementType": "all",
                                    "stylers": { "color": "#f7f7f7", "visibility": "off" }
                                },
                                {
                                    "featureType": "boundary",
                                    "elementType": "all",
                                    "stylers": { "color": "#d6d6d6" }
                                },
                                {
                                    "featureType": "label",
                                    "elementType": "labels.icon",
                                    "stylers": { "color": "#4c4c4c", "weight": "0.1" }
                                },
                                {
                                    "featureType": "local",
                                    "elementType": "geometry",
                                    "stylers": { "color": "#4c4c4c", "weight": "0.1" }
                                }

                            ];
                        }

                        map.centerAndZoom(centerPoint, mapZoom);
                        map.enableScrollWheelZoom(true);     //开启鼠标滚轮缩放
                        map.enableDoubleClickZoom(true);
                        //添加多个点
                        for (var i = 0; i < saPoints.length; i++) {
                            var item = saPoints[i];
                            var p = new BMap.Point(item.lng, item.lat);
                            pointArray[i] = p;
                            //自定义点图标
                            var iconUrl = sIconUrl;
                            var myIcon = new BMap.Icon(iconUrl, new BMap.Size(10, 35));
                            var marker = new BMap.Marker(p, { icon: myIcon });
                            map.addOverlay(marker);
                        }
                        map.setMapStyle({ styleJson: saStyleJson });
                    }
                    else {
                        if (saPoints.length === 0) {
                            saPoints = [
                                { lat: 23.5948856, lng: 121.4214155 }, //台灣
                                { lat: -6.3567715, lng: 145.9033085 }, //巴布亞
                                { lat: -23.6993532, lng: 133.8713752 }, //澳大利亞
                                { lat: 26.5604565, lng: 29.6630058 }, //埃及
                                { lat: 9.5577684, lng: 7.9979073 },//奈及利亞
                                { lat: 0.1953689, lng: 6.6126343 }, //聖多美普林西比
                                { lat: -30.6532947, lng: 23.9345003 }, //南非
                                { lat: -26.6682265, lng: 30.9023096 }, //史瓦濟蘭
                                { lat: -35.425776, lng: -65.9767868 }, //阿根廷
                                { lat: -8.5275228, lng: -55.8778956 }, //巴西
                                { lat: -26.8561446, lng: -71.4021298 },//智利
                                { lat: 4.1454439, lng: -73.5223993 }, //哥倫比亞
                                { lat: -23.4611862, lng: -58.4817976 }, //巴拉圭
                                { lat: -10.1091956, lng: -76.2822092 }, //秘魯
                                { lat: 19.0426862, lng: -70.2479896 }, //多明尼加
                                { lat: 57.8807689, lng: -101.6724135 }, //加拿大
                                { lat: 15.8120328, lng: -90.2990771 }, //瓜地馬拉
                                { lat: 15.0241668, lng: -87.0039418 }, //宏都拉斯
                                { lat: 24.6901136, lng: -102.0043495 },//墨西哥
                                { lat: 12.8889429, lng: -85.0106797 }, //尼加拉瓜
                                { lat: 8.6024001, lng: -80.2711767 }, //巴拿馬
                                { lat: 13.8973856, lng: -60.9842968 }, //聖露西亞
                                { lat: 40.2909474, lng: -101.809408 }, //美國
                                { lat: 41.6351161, lng: 21.711268 }, //馬其頓
                                { lat: 39.862204, lng: -3.2344494 }, //西班牙
                                { lat: 39.1082877, lng: 35.2500578 }, //土耳其
                                { lat: 42.7024418, lng: 25.1189025 }, //保加利亞
                                { lat: 43.9394954, lng: 17.6232668 }, //波士尼亞
                                { lat: 41.1857216, lng: 19.9760582 }, //阿爾巴尼亞
                                { lat: 49.8766304, lng: 15.1698544 }, //捷克
                                { lat: 53.2569956, lng: 18.6683763 }, //波蘭
                                { lat: 47.2047105, lng: 19.5553868 }, //匈牙利
                                { lat: 62.5290963, lng: 93.8171104 }, //俄羅斯
                                { lat: 63.0502944, lng: 16.6267692 }, //瑞典
                                { lat: 57.126913, lng: 26.0037774 }, //拉脫維亞
                                { lat: 48.8459645, lng: 19.4424029 }, //斯洛伐克
                                { lat: 51.1676818, lng: 10.4332196 }, //德國
                                { lat: 46.8463948, lng: 2.4559869 }, //法國
                                { lat: 55.3430558, lng: -3.3820532 }, //英國
                                { lat: 43.3089667, lng: 12.3977752 }, //義大利
                                { lat: 52.2590496, lng: 5.6466364 }, //荷蘭
                                { lat: 36.086822, lng: 103.4147709 }, //中國
                                { lat: 36.7191386, lng: 127.858362 }, //韓國
                                { lat: 37.0092457, lng: 139.8812684, }, //日本
                                { lat: 4.5928227, lng: 114.6181193 }, //汶萊
                                { lat: -3.2025712, lng: 121.9791652 }, //印尼
                                { lat: 4.1650768, lng: 102.0246564 }, //馬來西亞
                                { lat: 21.778682, lng: 96.5156388 }, //緬甸
                                { lat: 13.5488787, lng: 122.8379407 }, //菲律賓
                                { lat: 1.3593895, lng: 103.8616207 }, //新加坡
                                { lat: 15.5393972, lng: 100.9749041 }, //泰國
                                { lat: 15.268843, lng: 107.3972625 }, //越南
                                { lat: 23.6876726, lng: 79.4363431 }, //印度
                                { lat: 26.0593531, lng: 50.5436515 }, //巴林
                                { lat: 24.0776844, lng: 45.2586931 }, //沙烏地阿拉伯
                                { lat: 25.1583957, lng: 55.11235 }, //杜拜
                                { lat: 24.4631591, lng: 54.3505263 }, //阿布達比
                                { lat: 31.2373157, lng: 34.6096371 }, //以色列
                                { lat: 55.153022, lng: 21.6525519 }, //立陶宛
                                { lat: 53.1839289, lng: 28.0128641 }, //白俄羅斯
                            ];
                        }
                        if (saStyleJson.length === 0) {
                            saStyleJson = [{ "featureType": "all", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "administrative", "elementType": "labels.text.fill", "stylers": [{ "color": "#444444" }, { "visibility": "on" }] }, { "featureType": "administrative.province", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "administrative", "elementType": "labels.text.stroke", "stylers": [{ "visibility": "off" }] }, { "featureType": "administrative.country", "elementType": "geometry", "stylers": [{ "visibility": "on" }, { "color": "#ffffff" }] }, { "featureType": "administrative.province", "elementType": "geometry", "stylers": [{ "visibility": "simplified" }] }, { "featureType": "administrative.province", "elementType": "geometry.stroke", "stylers": [{ "visibility": "off" }] }, { "featureType": "landscape", "elementType": "all", "stylers": [{ "color": "#f7f7f7" }, { "visibility": "on" }] }, { "featureType": "poi", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "road", "elementType": "all", "stylers": [{ "saturation": -100 }, { "lightness": 45 }, { "visibility": "off" }] }, { "featureType": "road.highway", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "road.highway", "elementType": "geometry.fill", "stylers": [{ "visibility": "off" }, { "color": "#b5a265" }] }, { "featureType": "road.arterial", "elementType": "labels.icon", "stylers": [{ "visibility": "off" }] }, { "featureType": "transit", "elementType": "all", "stylers": [{ "visibility": "off" }] }, { "featureType": "transit.line", "elementType": "geometry", "stylers": [{ "visibility": "off" }] }, { "featureType": "transit.station.rail", "elementType": "geometry", "stylers": [{ "visibility": "off" }] }, { "featureType": "water", "elementType": "all", "stylers": [{ "color": "#e4e4e4" }, { "visibility": "on" }] }, { "featureType": "water", "elementType": "labels.text", "stylers": [{ "color": "#444444" }] }, { "featureType": "water", "elementType": "labels.text.stroke", "stylers": [{ "color": "#e4e4e4" }, { "visibility": "on" }] }];
                        }
                        map = new google.maps.Map(document.getElementById('map'), {
                            center: !mapInfo.Description ? { lat: 37.6860867, lng: 6.0952041 } : oCenterPoint,  // 地中海
                            scrollwheel: false,
                            zoom: 2,
                            styles: saStyleJson
                        });
                        var markers = saPoints.map(function (location, i) {
                            var marker = new google.maps.Marker({
                                map: map,
                                position: location,
                                icon: sIconUrl
                            });
                            google.maps.event.addListener(marker, 'dblclick', function () {
                                map.setZoom(map.getZoom() + 1);
                            });
                            return marker;
                        });
                    }
                }
            }, 'ServiceBase', sLang);
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

            $('#left ul li').on('click', function (e) {
                e.preventDefault();
                var index = $(this).index(),
                    sName_EN = '',
                    sName = '';
                $('#left>ul a').each(function () {
                    $(this).css('color', '#666');
                });
                switch (index) {
                    case 0:
                        sName_EN = 'ABOUT US';
                        sName = { 'zh-TW': '公司沿革', 'zh': '公司沿革', 'en': 'Company Introduction' }[sLang];
                        runByOrgInfo(fnSetIntroduction, true);

                        break
                    case 1:
                        sName_EN = 'MISSION & VISION';
                        sName = { 'zh-TW': '使命與願景', 'zh': '使命与愿景', 'en': 'Mission and Vision' }[sLang];
                        runByOrgInfo(fnSetMissionAndVision, true);

                        break
                    case 2:
                        sName_EN = 'SERVICE AREA';
                        sName = { 'zh-TW': '全球服務據點', 'zh': '全球服务据点', 'en': 'Service Area' }[sLang];
                        fnSetMap();

                        break
                }
                $(this).find('a').css('color', '#EC681E');
                $('#TitleEName').html(sName_EN);
                $('#TitleName').html(sName);
            });

            if (sTab) {
                $('#left ul li a[href="about.html?T=' + sTab + '"]').css('color', '#EC681E');
                $('#left ul li').eq(sTab * 1 - 1).trigger('click');
            }
            else {
                $('#left ul li a:first').css('color', '#EC681E');
                $('#left ul li:first').trigger('click');
            }
        };

    init();
});