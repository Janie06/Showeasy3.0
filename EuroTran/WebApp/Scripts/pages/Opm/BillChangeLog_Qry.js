'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'ModifyDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param {Object}  args 查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGetPro = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                oQueryPm.BillStatus = !oQueryPm.BillStatus ? '' : oQueryPm.BillStatus.join(',');
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, 'QueryPage', oQueryPm, function (res) {
                    var oRes = res.DATA.rel;
                    $.each(oRes.DataList, function (index, item) {
                        item.RowNumber = index + 1;
                    });
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        var iNum = $('#PerPageNum').val();
                        oGrid.pageSize = iNum === '' ? parent.SysSet.GridRecords || 10 : iNum;
                        cacheQueryCondition();
                        oGrid.openPage(window.bToFirstPage ? 1 : oBaseQueryPm.pageIndex);

                        break;
                    case "Toolbar_Save":
                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": 

                        break;
                    default:
                        alert("No handle '" + sId + "'");
                        break;
                }
            },
            /**
             * 頁面初始化
             */
            init = function () {
                commonInit({
                    PrgId: 'BillChangeLog_Qry',
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });

                fnSetUserDrop([
                    {
                        Select: $('#ResponsiblePerson'),
                        Select2: true,
                        ShowId: true,
                        CallBack: function (data) {
                            var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
                            $("#jsGrid").jsGrid({
                                width: "100%",
                                height: iHeight + "px",
                                autoload: true,
                                pageLoading: true,
                                inserting: false,
                                editing: false,
                                sorting: true,
                                paging: true,
                                pageIndex: window.bToFirstPage ? 1 : window.QueryPageidx || 1,
                                pageSize: parent.SysSet.GridRecords || 10,
                                pageButtonCount: parent.SysSet.GridPages || 15,
                                confirmDeleting: true,
                                pagePrevText: "<",
                                pageNextText: ">",
                                pageFirstText: "<<",
                                pageLastText: ">>",
                                rowClass: function (item) {
                                    var sClass = '';
                                    if (item.AuditVal === '6') {
                                        sClass = 'data-void';
                                    }
                                    return sClass;
                                },
                                onPageChanged: function (args) {
                                    cacheQueryCondition(args.pageIndex);
                                },
                                fields: [
                                    { name: "RowNumber", title: 'common.RowNumber', align: "center", type: "text", width: 60, sorting: false },
                                    {
                                        name: "BillNO", title: 'common.BillNO', align: "left", type: "text", width: 80
                                    },
                                    {
                                        name: "ExhibitioName", title: 'common.ExhibitionName', align: "left", type: "text", width: 150
                                    },
                                    {
                                        name: "PayerName", title: 'ExhibitionImport_Upd.Payer', align: "left", type: "text", width: 160
                                    },
                                    {
                                        name: "ResponsiblePersonName", title: 'common.ResponsiblePerson', align: "left", type: "text", width: 50
                                    },
                                    {
                                        name: "Currency", title: 'common.Financial_Currency', align: "left", type: "text", width: 40
                                    },
                                    {
                                        name: "ExchangeRate", title: 'common.ExchangeRate', align: "left", type: "text", width: 50
                                    },
                                    {
                                        name: "Advance", title: 'common.Financial_Advance', align: "right", type: "text", width: 50,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {// ╠common.NOTaxAmount⇒未稅金額╣
                                        name: "AmountSum", title: 'common.NOTaxAmount', align: "right", type: "text", width: 60,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {
                                        name: "TaxSum", title: 'common.TaxAmount', align: "right", type: "text", width: 70,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {// ╠common.Financial_Sum⇒合計╣
                                        name: "AmountTaxSum", title: 'common.Financial_Sum', align: "right", type: "text", width: 70,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {// ╠common.TotalReceivable⇒總應收╣
                                        name: "TotalReceivable", title: 'common.TotalReceivable', align: "right", type: "text", width: 70,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {
                                        name: "OpmBillCreateUserName", title: 'common.CreateUser', align: "left", type: "text", width: 50
                                    },
                                    {
                                        name: "Operation", title: 'common.Operation', type: "text", align: "center", width: 60,
                                    },
                                    {
                                        name: "ModifyDate", title: 'common.ModifyDateTime', type: "text", align: "center", width: 70,
                                        itemTemplate: function (val, item) {
                                            var rDate = !val ? item.ModifyDate : val;
                                            return newDate(rDate);
                                        }
                                    },
                                    {
                                        name: "ModifyUser", title: 'common.ModifyUser', type: "text", align: "center", width: 50
                                    }
                                ],
                                controller: {
                                    loadData: function (args) {
                                        return fnGetPro(args);
                                    }
                                },
                                onInit: function (args) {
                                    oGrid = args.grid;
                                }
                            });
                        }
                    }
                ]);
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);