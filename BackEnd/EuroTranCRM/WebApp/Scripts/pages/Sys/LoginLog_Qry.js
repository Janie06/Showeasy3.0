'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'LoginTime',
        sortOrder: 'desc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['NO'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
            {
                name: "OrgId", title: 'Organization_Upd.OrgID', align: "left", type: "text", width: 150
            },
            {
                name: "UserId", title: 'common.UserId', align: "left", type: "text", width: 200,
                itemTemplate: function (val, item) {
                    return val + '（' + item.UserName + '）';
                }
            },
            { name: "LoginIp", title: 'common.LoginIp', type: "text", align: "center", width: 250 },// ╠common.LoginIp⇒當前登入IP╣
            {
                name: "LoginTime", title: 'common.LoginTime', type: "text", align: "center", width: 200,// ╠common.LoginTime⇒登入時間╣
                itemTemplate: function (val, item) {
                    return newDate(val);
                }
            }
        ],
        pageInit: function (pargs) {
            fnSetUserDrop([{
                Select: $('#Account'),
                ShowId: true,
                Select2: true,
                CallBack: function (data) {
                    pargs._reSetQueryPm();
                    pargs._initGrid();
                }
            }]);
        }
    });
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);