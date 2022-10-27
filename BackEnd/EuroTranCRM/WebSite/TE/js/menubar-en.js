$(function () {
    var strMenuBar =
           '<ul id="menu">\
                <li id="about">\
                <a href="about.html" > About Us</a>\
                    <ul>\
                        <li><a href="about.html?T=1">Company Introduction</a></li>\
                        <li><a href="about.html?T=2">Mission and Vision</a></li>\
                        <li><a href="about.html?T=3">Service Area</a></li>\
                    </ul>\
                    </li >\
                <li id="service">\
                    <a href="service.html">Services</a>\
                    <ul>\
                        <li><a href="service.html">Inbound Exhibition Logistics</a></li>\
                        <li><a href="service.html">Outbound Exhibition Logistics</a>\
                        <li><a href="service.html">Events & Performances Logistics</a></li>\
                        <li><a href="service.html">On-site Services</a></li>\
                        <li><a href="service.html">Customized Global Projects</a></li>\
                    </ul>\
                    </li>\
                    <li id="online">\
                        <a href="onlineTrack.html">Online Services</a>\
                        <ul>\
                            <li><a href="onlineTrack.html">Tracking System</a></li>\
                            <li><a href="contact.html?T=O">Online Inquiry</a></li>\
                            <li><a href="onlineDownload.html">Exhibition Logistics Forms</a></li>\
                        </ul>\
                    </li>\
                    <li id="case">\
                        <a href="case.html">Classic Cases</a>\
                        <ul>\
                            <li><a href="case.html?T=0">Taiwan</a></li>\
                            <li><a href="case.html?T=1">Overseas</a></li>\
                            <li><a href="case.html?T=2">Artistic Performances</a></li>\
                            <li><a href="case.html?T=3">Customized Global Cases</a></li>\
                        </ul>\
                    </li>\
                    <li id="news">\
                    <a href="news.html">News<img class="news-tips" src="../images/icon_new.gif" /></a>\
                        <ul>\
                            <li style="width:100%"><a href="news.html">Latest News</a></li>\
                            <li style="width:100%"><a href="photo.html">Photos</a></li>\
                        </ul>\
                    </li>\
                    <li id="video"><a href="video.html">Brand Videos</a></li>\
                    <li id="contact" > <a href="contact.html">Contact Us</a></li>\
            </ul>';
    $('#menuWrapper').append(strMenuBar);
});