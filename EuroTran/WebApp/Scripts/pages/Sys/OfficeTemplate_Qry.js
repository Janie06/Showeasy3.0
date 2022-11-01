'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'TemplID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'TemplID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "TemplID", title: 'OfficeTemplate_Upd.TemplID', width: 200 },
            { name: "TemplName", title: 'OfficeTemplate_Upd.TemplName', width: 300 },
            { name: "Memo", title: 'common.Memo', width: 300 },
            { name: "ModifyUser", title: 'common.ModifyUser', width: 150 },
            {
                name: "ModifyDate", title: 'common.ModifyDate', width: 150, align: 'center', itemTemplate: function (val, item) {
                    return newDate(val);
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