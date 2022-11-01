'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'RuleID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'RuleID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "RuleID", title: 'common.RuleID', width: 200 },
            { name: "RuleName", title: 'common.RuleName', width: 300 },
            { name: "ExFeild1", title: 'common.MemberID', width: 500 }
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