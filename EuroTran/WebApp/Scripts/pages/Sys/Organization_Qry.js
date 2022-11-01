'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'OrgID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "OrgID", title: 'Organization_Upd.OrgID', width: 100 },
            { name: "OrgName", title: 'Organization_Upd.OrgName', width: 200 },
            { name: "OwnerName", title: 'Organization_Upd.OwnerName', width: 100 },
            { name: "Email", title: 'common.Email', width: 150 },
            { name: "TEL", title: 'common.Telephone', width: 100 },
            { name: "Address", title: 'common.Address', width: 300 }
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