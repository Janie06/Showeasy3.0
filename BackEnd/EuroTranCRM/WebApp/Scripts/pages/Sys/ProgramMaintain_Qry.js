'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'ModOrderBy,OrderByValue',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ProgramID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "ModuleName", title: 'common.ModuleName', width: 200 },
            { name: "ProgramID", title: 'common.ProgramCode', width: 200 },
            { name: "ProgramName", title: 'common.ProgramName', width: 200 },
            { name: "ProgramTypeName", title: 'ProgramMaintain_Upd.ProgramType', width: 200 },
            { name: "EffectiveName", title: 'common.Status', align: 'center', width: 100 },
            { name: "ShowInListName", title: 'ProgramMaintain_Upd.ShowInList', align: 'center', width: 100 },
            {
                name: "OrderByValue", title: 'common.OrderByValue', type: "select", width: 100,

                itemTemplate: function (val, item) {
                    return this._createSelect = $("<select>", {
                        class: 'w70',
                        html: createOptions(item.OrderCount),
                        change: function () {
                            var sOldValue = val,
                                sNewValue = this.value;
                            g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
                                ParentId: item.ModuleID,
                                Id: item.ProgramID,
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
            fnModuleNameDrop().done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    }),
        /**
        * 設置模組名稱下拉單
        * @return {Object} Ajax 物件
        */
        fnModuleNameDrop = function () {
            return g_api.ConnectLite('AuthantedPrograms', 'GetModulelist', { IncludeParent: true },
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $('#ModuleID').html(createOptions(saList, 'ModuleID', 'ModuleName', true));
                    }
                });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);