'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'ArgumentClassID,OrderByValue',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ArgumentClassID', 'ArgumentID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 100, sorting: false },
            { name: "ArgumentClassName", title: 'ArgumentClassMaintain_Upd.ArgumentClassName', width: 200 },
            { name: "ParentArgumentName", title: 'common.Parent', width: 150 },
            { name: "ArgumentID", title: 'ArgumentMaintain_Upd.ArgumentID', width: 100 },
            { name: "ArgumentValue", title: 'ArgumentMaintain_Upd.ArgumentValue', width: 150 },
            { name: "Correlation", title: 'ArgumentMaintain_Upd.Correlation', width: 150 },
            {
                name: "Effective", title: 'common.Status', align: 'center', width: 100, itemTemplate: function (val) {
                    return val === 'Y' ? i18next.t('common.Enable') : i18next.t('common.Disable');
                }
            },
            {
                name: "OrderByValue", title: 'common.OrderByValue', type: "select", width: 150,
                itemTemplate: function (val, item) {
                    return this._createSelect = $("<select>", {
                        class: 'w70',
                        html: createOptions(item.OrderCount),
                        change: function () {
                            var sOldValue = val,
                                sNewValue = this.value;
                            g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
                                ParentId: item.ArgumentClassID,
                                Id: item.ArgumentID,
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
            fnArgumentClassDrop().done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    }),
        /**
        * 設置參數類別下拉單
        * @return {Object} Ajax 物件
        */
        fnArgumentClassDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.getlist, {},
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        var sOptionHtml = createOptions(saList, 'ArgumentClassID', 'ArgumentClassName', true);
                        $('#ArgumentClassID').html(sOptionHtml);
                    }
                });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);