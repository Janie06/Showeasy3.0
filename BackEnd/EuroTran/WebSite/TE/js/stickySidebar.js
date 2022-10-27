(function ($) {
    $.fn.stickySidebar = function (options) {
        var config = $.extend({
            headerSelector: 'header',
            navSelector: '#menuWrapper',
            bannerSelector: '#innerTitle',
            contentSelector: 'article',
            footerSelector: 'footer',
            sidebarTopMargin: 155,
            footerThreshold: 180
        }, options);

        var fixSidebr = function () {
            var sidebarSelector = $(this);
            var viewportHeight = $(window).height();
            var documentHeight = $(document).height();
            var headerHeight = $(config.headerSelector).outerHeight();
            var navHeight = $(config.navSelector).outerHeight();
            var bannerHeight = $(config.bannerSelector).outerHeight();
            var sidebarHeight = sidebarSelector.outerHeight();
            var contentHeight = $(config.contentSelector).outerHeight();
            var footerHeight = $(config.footerSelector).outerHeight();
            var scroll_top = $(window).scrollTop();
            var fixPosition = contentHeight - sidebarHeight;
            var breakingPoint1 = headerHeight + navHeight + bannerHeight - 122;
            var breakingPoint2 = documentHeight - (sidebarHeight + footerHeight + config.footerThreshold);

            // calculate
            if ((contentHeight > sidebarHeight) && (viewportHeight > sidebarHeight)) {
                if (scroll_top < breakingPoint1) {
                    sidebarSelector.removeClass('sticky');
                } else if ((scroll_top >= breakingPoint1) && (scroll_top < breakingPoint2)) {
                    sidebarSelector.addClass('sticky').css('top', config.sidebarTopMargin);
                } else {
                    var negative = breakingPoint2 - scroll_top;
                    sidebarSelector.addClass('sticky').css('top', negative);
                }
            }
        };

        return this.each(function () {
            $(window).on('scroll', $.proxy(fixSidebr, this));
            $(window).on('resize', $.proxy(fixSidebr, this))
            $.proxy(fixSidebr, this)();
        });
    };
}(jQuery));

$(document).ready(function () {
    $('#sidebar').stickySidebar({
        //sidebarTopMargin: 70,
        //footerThreshold: 100
    });
});