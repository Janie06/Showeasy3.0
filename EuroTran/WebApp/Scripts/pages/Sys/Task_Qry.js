'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'EventID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'EventID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "SourceFromName", title: 'common.SourceFromName', width: 100 },// ╠common.SourceFromName⇒表單名稱╣
            { name: "EventName", title: 'Task_Upd.EventName', width: 250 },
            { name: "OwnerName", title: 'Task_Upd.Owner', width: 80 },// ╠common.SourceFromName⇒表單名稱╣
            { name: "CreateUserName", title: 'common.CreateUser', width: 80 },
            {
                name: "StartDate", title: 'common.StartDate', align: 'center', width: 120, itemTemplate: function (value) {
                    return newDate(value);
                }
            },
            {
                name: "EndDate", title: 'common.EndDate', align: 'center', width: 120, itemTemplate: function (value) {
                    return newDate(value, false, true);
                }
            },
            {
                name: "Status", title: 'common.Status', width: 60, itemTemplate: function (value) {
                    var oStatus = { U: '未開始', G: '進行中', D: '已完成（待確認）', O: '已確認' };
                    return oStatus[value] || '未開始';
                }
            },
            { name: "ProgressShow", title: 'common.Progress', align: 'center', width: 80 },
            { name: "ImportantName", title: 'common.Important', align: 'center', width: 80 }
        ],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            fnSetUserDrop([
                {
                    Select: $('#CreateUser,#Owner'),
                    Select2: true
                }
            ]).done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    });
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);