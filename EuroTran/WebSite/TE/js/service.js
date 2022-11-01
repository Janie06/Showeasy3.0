$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        sId = getUrlParam('Id'),
        /*
        * 目的 抓去服務項目菜單
        */
        fnSetServiceItems = function (org) {
            return fnGetWebSiteSetting(function (saRes) {
                if (saRes.length > 0) {
                    var sHtml = $('<script />', { type: 'text/x-jsrender', html: '<li id="{{:Guid}}"><a href="javascript:void(0);">{{:Title}}</a></li>' }).render(saRes);
                    $('#left>ul').html(sHtml).find('li').on('click', function () {
                        var sGuid = this.id;
                        $('#left>ul a').each(function () {
                            $(this).css('color', '#666');
                        });
                        $(this).find('a').css('color', '#EC681E');
                        fnGetServiceItem(sGuid);
                    });
                    if (sId) {
                        $('#' + sId).find('a').css('color', '#EC681E');
                    }
                    else {
                        $('#left>ul a:first').css('color', '#EC681E');
                        fnGetServiceItem(saRes[0].Guid);
                    }
                }
            }, 'ServiceItems', sLang)
        },
        /*
        * 目的 抓去服務項目內容
        * @param {String} id 服務項目id
        */
        fnGetServiceItem = function (id) {
            return fnGetWebSiteSetting(function (oRes) {
                $('#Title').html(oRes.Title);
                $('#Content').html(oRes.Content);
                $('#TitleEName').html(oRes.Memo);
                $('#TitleName').html(oRes.Title);
            }, 'ServiceItems', sLang, id, false, true);
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

            fnSetServiceItems();
            if (sId) {
                fnGetServiceItem(sId);
            }
        };

    init();
});