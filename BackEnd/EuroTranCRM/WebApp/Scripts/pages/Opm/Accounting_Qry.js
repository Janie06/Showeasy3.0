'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            fnGetAllFeeClass = function () {
                var sStr = [];
                $('#FeeClass option').each(function () {
                    if (this.value) {
                        sStr.push(this.value);
                    }
                });
                return sStr.join(',');
            },
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                var oQuery = getFormSerialize(oForm),
                    sFeeClass = oQuery.FeeClass === '' ? fnGetAllFeeClass() : oQuery.FeeClass;

                return g_api.ConnectLite(sProgramId, 'GetBillsList', {
                    FeeClass: sFeeClass,
                    CheckDateStart: oQuery.CheckDateStart,
                    CheckDateEnd: oQuery.CheckDateEnd
                });
            },
            /**
             * 匯出資料
             * @return {Object} Ajax 物件
             */
            fnExcel = function () {
                var oQuery = getFormSerialize(oForm),
                    sFeeClass = oQuery.FeeClass === '' ? fnGetAllFeeClass() : oQuery.FeeClass;

                return g_api.ConnectLite(sProgramId, 'ExcelBillsList', {
                    FeeClass: sFeeClass,
                    CheckDateStart: oQuery.CheckDateStart,
                    CheckDateEnd: oQuery.CheckDateEnd
                }, function (res) {
                    if (res.RESULT) {
                        DownLoadFile(res.DATA.rel);
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
                        cacheQueryCondition();
                        oGrid.openPage(1);

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
                        fnExcel();
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
                    GoTop: true,
                    SearchBar: true
                });
                fnSetArgDrop([
                    {
                        ArgClassID: 'FeeClass',
                        Select: $('#FeeClass'),
                        ShowId: true,
                        Ids: parent.SysSet.FeeClassList
                    }
                ])
                    .done(function () {
                        reSetQueryPm(sProgramId);
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
                            pageSize: 100000,
                            pageButtonCount: parent.SysSet.GridPages || 15,
                            invalidMessage: '输入的数据无效！',
                            confirmDeleting: true,
                            deleteConfirm: "確定要刪除嗎？",
                            pagePrevText: "<",
                            pageNextText: ">",
                            pageFirstText: "<<",
                            pageLastText: ">>",
                            onPageChanged: function (args) {
                                cacheQueryCondition(args.pageIndex);
                            },
                            rowClick: function (args) {
                                if (navigator.userAgent.match(/mobile/i)) {
                                }
                            },
                            rowDoubleClick: function (args) {
                            },
                            fields: [
                                { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                                {
                                    name: "BillNO", title: 'common.BillNO', align: "left", type: "text", width: 100, sorting: false
                                },
                                {
                                    name: "ExhibitioName", title: 'Exhibition_Upd.Exhibitioname_TW', align: "left", type: "text", width: 350, sorting: false
                                },
                                {
                                    name: "Payer", title: 'ExhibitionImport_Upd.Payer', align: "left", type: "text", width: 350, sorting: false
                                },
                                {
                                    name: "Amount", title: 'common.Financial_Amount', align: "right", type: "text", width: 100, sorting: false,
                                    itemTemplate: function (val, item) {
                                        return !val ? '' : fMoney(parseFloat(val), 2, item.ForeignCurrencyCode);
                                    }
                                },
                                {
                                    name: "CreateDate", title: 'common.CreateDate', type: "text", align: "center", width: 150, sorting: false,
                                    itemTemplate: function (val, item) {
                                        return !val ? '' : newDate(val);
                                    }
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return fnGet();
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