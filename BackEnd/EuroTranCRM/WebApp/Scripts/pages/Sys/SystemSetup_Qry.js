'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'SettingItem',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'SettingItem'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "SettingItem", title: 'SystemSetup_Qry.SettingItem', width: 200 },
            { name: "SettingDescription", title: 'SystemSetup_Qry.SettingDescription', width: 200 },
            { name: "SettingValue", title: 'SystemSetup_Qry.SettingValue', width: 200 },
            {
                name: "Effective", title: 'common.Status', align: 'center', width: 200, itemTemplate: function (val) {
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