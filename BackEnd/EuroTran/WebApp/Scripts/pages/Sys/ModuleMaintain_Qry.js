'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'OrderByValue',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ModuleID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 100, sorting: false },
            { name: "ModuleID", title: 'common.ModuleID', width: 200 },
            { name: "ModuleName", title: 'common.ModuleName', width: 200 },
            { name: "ProCount", title: 'ModuleMaintain_Qry.Count', width: 200 }, // ╠ModuleMaintain_Qry.Count⇒程式支數╣
            {
                name: "OrderByValue", title: 'common.OrderByValue', type: "select", width: 200,
                itemTemplate: function (val, item) {
                    return this._createSelect = $("<select>", {
                        class: 'w70',
                        html: createOptions(item.OrderCount),
                        change: function () {
                            var sOldValue = val,
                                sNewValue = this.value;
                            g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
                                Id: item.ModuleID,
                                OldOrderByValue: sOldValue,
                                NewOrderByValue: sNewValue
                            }, function (res) {
                                if (res.RESULT) {
                                    showMsg(i18next.t('message.Update_Success'), 'success');// ╠message.Update_Success⇒更新成功╣
                                    canDo.Grid.openPage(canDo.options.toFirstPage ? 1 : canDo.options.queryPageidx);
                                }
                                else {
                                    showMsg(i18next.t('message.Update_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Update_Failed⇒更新失敗╣
                                }
                            });
                        }
                    }).val(val);
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