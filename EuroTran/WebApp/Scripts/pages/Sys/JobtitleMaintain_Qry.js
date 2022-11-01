'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'JobtitleID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'JobtitleID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "JobtitleID", title: 'JobtitleMaintain_Upd.JobtitleID', width: 200 },
            { name: "JobtitleName", title: 'common.JobtitleName', width: 200 },
            {
                name: "Effective", title: 'common.Status', width: 200, align: 'center', itemTemplate: function (val) {
                    return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');
                }
            }
        ],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            pargs._reSetQueryPm();
            pargs._initGrid();
        }
    });
};

require(['base', 'jsgrid', 'cando'], fnPageInit);