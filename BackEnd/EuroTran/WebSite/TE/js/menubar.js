$(function () {
    var strMenuBar =
           '<ul id="menu">\
                <li id="about">\
                    <a href="about.html?T=1">關於奕達</a>\
                    <ul>\
                        <li><a href="about.html?T=1">公司沿革</a></li>\
                        <li><a href="about.html?T=2">使命與願景</a></li>\
                        <li><a href="about.html?T=3">全球服務據點</a></li>\
                    </ul>\
                </li>\
                <li id="service">\
                    <a href="service.html">服務項目</a>\
                    <ul>\
                        <li><a href="service.html">進口會展物流</a></li>\
                        <li><a href="service.html">出口會展物流</a>\
                            <li><a href="service.html">藝文展演物流</a></li>\
                            <li><a href="service.html">展館佈場服務</a></li>\
                            <li><a href="service.html">客製化全球專案</a></li>\
                    </ul>\
                </li>\
                    <li id="online">\
                        <a href="onlineTrack.html">線上服務</a>\
                        <ul>\
                            <li><a href="onlineTrack.html">貨況查詢服務</a></li>\
                            <li><a href="contact.html?T=O">線上詢價</a></li>\
                            <li><a href="onlineDownload.html">會展物流相關表單</a></li>\
                        </ul>\
                    </li>\
                    <li id="case">\
                        <a href="case.html">經典案例</a>\
                        <ul>\
                            <li><a href="case.html?T=0">台灣</a></li>\
                            <li><a href="case.html?T=1">海外</a></li>\
                            <li><a href="case.html?T=2">藝文展演活動</a></li>\
                            <li><a href="case.html?T=3">客製化全球專案</a></li>\
                        </ul>\
                    </li>\
                    <li id="news">\
                    <a href="news.html">焦點訊息<img class="news-tips" src="images/icon_new.gif"/></a>\
                        <ul>\
                            <li><a href="news.html">最新消息</a></li>\
                            <li><a href="photo.html">展會花絮</a></li>\
                        </ul>\
                    </li>\
                    <li id="video"><a href="video.html">品牌影音</a></li>\
                    <li id="contact"><a href="contact.html">聯絡奕達</a></li>\
            </ul>';
    $('#menuWrapper').append(strMenuBar);
});