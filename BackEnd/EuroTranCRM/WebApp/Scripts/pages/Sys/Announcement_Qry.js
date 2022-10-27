'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'StartDateTime',
        sortOrder: 'desc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'AnnouncementID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            {
                name: "Ann_Type", title: 'Announcement_Upd.Ann_Type', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return $('#Ann_Type option[value=' + val + ']').text().split('-')[1];
                }
            },
            { name: "Title", title: 'Announcement_Upd.Title', width: 300 },
            {
                name: "Description", title: 'common.Toolbar_Download', width: 200, align: 'center', itemTemplate: function (val, item) {
                    var oVal = $('<div />').css('text-align', 'left'),
                        oDiv = $('<div />', { html: val });
                    oDiv.find('img').each(function () {
                        var sSrc = $(this).attr('src') || '';
                        if (sSrc.indexOf('attachment/fileTypeImages/icon_') > -1) {
                            var sHref = $(this).parent('p').find('a').attr('href') || '';
                            $(this).attr('src', sSrc.replace('http:', 'https:'));
                            if (sSrc.indexOf('icon_jpg') > -1 || sSrc.indexOf('icon_pdf') > -1) {
                                $(this).parent('p').find('a').attr('target', '_new');
                            }
                            $(this).parent('p').find('a').attr('href', sHref.replace('http:', 'https:'));
                            oVal.append($(this).parent('p'));
                        }
                    });
                    return oVal[0].outerHTML;
                }
            },
            {
                name: "StartDateTime", title: 'Announcement_Upd.StartDateTime', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return newDate(val, 'date');
                }
            },
            {
                name: "EndDateTime", title: 'Announcement_Upd.EndDateTime', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return newDate(val, 'date');
                }
            },
            //{
            //    name: "Description", title: 'Announcement_Upd.Description', width: 300, itemTemplate: function (val, item) {
            //        return val.length > 30 ? val.substring(0, 30) + '...' : val;
            //    }
            //}
        ],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            $.when(fnSetArgDrop([
                {
                    ArgClassID: 'Ann_Type',
                    Select: $('#Ann_Type'),
                    ShowId: true
                }
            ])).done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    });
};

require(['base', 'jsgrid', 'cando'], fnPageInit);