$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        /*
        * 目的 抓去經典案例
        */
        fnGetClassicCase = function () {
            return fnGetWebSiteSetting(function (saRes) {
                var sHtmlTab = $('#temp_classiccasetab').render(saRes),
                    sHtmlDiv = $('#temp_classiccasediv').render(saRes),
                    sCurId = '';
                if (saRes.length > 0) {
                    $('#tabsNav').html(sHtmlTab);
                    $('.tab_contentr').html(sHtmlDiv);
                    $(".rectThumb").imgLiquid({ fill: true });

                    var _showTab = getUrlParam('T') || 0;
                    $('#tabsNav li').css('width', (navigator.userAgent.match(/mobile/i) ? 105 : 100) / saRes.length + '%');
                    if (_showTab === 0) {
                        $('#tabsNav li:first').addClass('active');
                        $('.tab_content').hide().eq(0).show();
                    }
                    else {
                        $('#tabsNav li[data-id="' + _showTab + '"]').addClass('active');
                        $('.tab_content').hide();
                        $('#tab' + _showTab).show();
                    }

                    // 當 li 頁籤被點擊時...
                    // 若要改成滑鼠移到 li 頁籤就切換時, 把 click 改成 mouseover
                    $('#tabsNav li').click(function () {
                        // 找出 li 中的超連結 href(#id)
                        var $this = $(this),
                            _clickTab = $this.find('a').attr('href');
                        // 把目前點擊到的 li 頁籤加上 .active
                        // 並把兄弟元素中有 .active 的都移除 class
                        $this.addClass('active').siblings('.active').removeClass('active');
                        // 淡入相對應的內容並隱藏兄弟元素
                        $(_clickTab).stop(false, true).fadeIn(0).siblings().hide();

                        return false;
                    });

                    $(".iframe").click(function () {
                        sCurId = $(this).attr('data-id');
                    }).fancybox({
                        type: 'iframe',
                        iframe: {
                            css: {
                                width: '1024px',
                                height: '660px'
                            }
                        },
                        afterShow: function (instance, current) {
                            var oInfo = [],
                                sParentId = $('#tabsNav li.active').attr('data-id');
                            oInfo = $.grep(saRes, function (item) {
                                return item.Guid === sParentId;
                            })[0];
                            $.each(oInfo.Infos, function (index, item) {
                                if (item.Guid === sCurId) {
                                    var elIframe = current.$content.find('iframe').contents();
                                    elIframe.find('#imgCoverId').attr('src', gServerUrl + '/' + item.CoverPath.replace(/\\/g, "\/"));
                                    elIframe.find('#parentTitle').html(oInfo.Title);
                                    elIframe.find('#detailTitle').html(item.Title);
                                    elIframe.find('.detailText').html(item.Content);
                                    elIframe.find(".caseThumb").imgLiquid({ fill: true });
                                    return false;
                                }
                            });
                        }
                    });
                }
            }, 'ClassicCase', sLang, null, true);
        },
        init = function () {
            var myHelpers = {
                setTitle: function (val, val2) {
                    return navigator.userAgent.match(/mobile/i) ? val2 || val : val;
                },
                setFilePath: function (val) {
                    val = val || '';
                    return gServerUrl + '/' + val.replace(/\\/g, "\/");
                }
            };
            $.views.helpers(myHelpers);

            fnGetClassicCase();
        };

    init();
});