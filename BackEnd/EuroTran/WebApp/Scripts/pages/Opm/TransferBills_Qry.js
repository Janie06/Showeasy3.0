'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CreateDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param {Object}  args 查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, 'QueryPage', oQueryPm, function (res) {
                    var oRes = res.DATA.rel;
                    if (args.Excel) {//匯出
                        var oRes = res.DATA.rel;
                        DownLoadFile(oRes);
                        layer.close(args.Index);
                    }
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

                        fnSave('add');

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
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_Exp":
                        if (oGrid.data.length === 0) {
                            showMsg(i18next.t("message.NoDataExport"));// ╠message.NoDataExport⇒沒有資料匯出╣
                            return false;
                        }
                        fnGet({
                            Excel: true
                        });
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
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                }).done(function () {
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
                        pagePrevText: "<",
                        pageNextText: ">",
                        pageFirstText: "<<",
                        pageLastText: ">>",
                        onPageChanged: function (args) {
                            cacheQueryCondition(args.pageIndex);
                        },
                        rowClick: function (args) {
                            if (navigator.userAgent.match(/mobile/i)) {
                                var saUrl = args.item.Url.split('|');
                                if (saUrl.length > 1) {
                                    var sPrgId = saUrl[0],
                                        sParam = saUrl[1];
                                    goToEdit(sPrgId, sParam);
                                }
                            }
                        },
                        rowDoubleClick: function (args) {
                            var saUrl = args.item.Url.split('|');
                            if (saUrl.length > 1) {
                                var sPrgId = saUrl[0],
                                    sParam = saUrl[1];
                                parent.openPageTab(sPrgId, sParam);
                            }
                        },
                        fields: [
                            { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                            {
                                name: "BillNO", title: 'common.BillNO', align: "left", type: "text", width: 80
                            },
                            {
                                name: "ResponsiblePersonCodeName", title: 'common.ResponsiblePerson', align: "left", type: "text", width: 70
                            },
                            {
                                name: "Payer", title: 'ExhibitionImport_Upd.Payer', align: "left", type: "text", width: 150
                            },
                            {
                                name: "ForeignCurrencyCode", title: 'common.Financial_Currency', align: "left", type: "text", width: 50,
                                itemTemplate: function (val, item) {
                                    return !val ? (parent.OrgID === 'SG' ? 'RMB' : 'NTD') : val;
                                }
                            },
                            {
                                name: "ExchangeRate", title: 'common.ExchangeRate', align: "left", type: "text", width: 50
                            },
                            {
                                name: "Advance", title: 'common.Financial_Advance', align: "right", type: "text", width: 60,
                                itemTemplate: function (val, item) {
                                    return fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                }
                            },
                            {
                                name: "TWNOTaxAmount", title: 'common.NOTaxAmount', align: "right", type: "text", width: 60,
                                itemTemplate: function (val, item) {
                                    return fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                }
                            },
                            {
                                name: "TaxSum", title: 'common.TaxAmount', align: "right", type: "text", width: 60,
                                itemTemplate: function (val, item) {
                                    return fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                }
                            },
                            {
                                name: "BillAmount", title: 'common.Financial_Sum', align: "right", type: "text", width: 60,
                                itemTemplate: function (val, item) {
                                    return fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                }
                            },
                            {
                                name: "TotalReceivable", title: 'common.TotalReceivable', align: "right", type: "text", width: 60, sorting: false,
                                itemTemplate: function (val, item) {
                                    return fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                }
                            },
                            {// ╠common.AuditSer⇒審核人員╣
                                name: "CreateUser", title: 'common.AuditSer', align: "left", type: "text", width: 70
                            },
                            {// ╠common.BillFirstCheckDate⇒第一次審核時間╣
                                name: "BillFirstCheckDate", title: 'common.BillFirstCheckDate', type: "text", align: "center", width: 100,
                                itemTemplate: function (val, item) {
                                    var rDate = !val ? item.CreateDate : val;
                                    return newDate(rDate);
                                }
                            },
                            {
                                name: "CreateDate", title: 'common.TransferDate', type: "text", align: "center", width: 100,
                                itemTemplate: function (val, item) {
                                    return newDate(val);
                                }
                            }
                        ],
                        controller: {
                            loadData: function (args) {
                                return fnGet(args);
                            }
                        },
                        onInit: function (args) {
                            oGrid = args.grid;
                        }
                    });
                });
            };

        init();
    };

require(['base', 'jsgrid', 'util'], fnPageInit);