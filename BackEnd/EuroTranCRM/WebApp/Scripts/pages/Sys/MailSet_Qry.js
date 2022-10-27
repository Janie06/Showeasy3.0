'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'EmailID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'EmailID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "EmailID", title: 'MailSet_Upd.EmailID', width: 200 },
            { name: "EmailName", title: 'MailSet_Upd.EmailName', width: 200 },
            { name: "EmailSubject", title: 'MailSet_Upd.EmailSubject', width: 200 },
            { name: "EmailDescript", title: 'MailSet_Upd.EmailDescript', width: 250 },
            {
                name: "Effective", title: 'common.Status', width: 100, align: 'center', itemTemplate: function (val) {
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