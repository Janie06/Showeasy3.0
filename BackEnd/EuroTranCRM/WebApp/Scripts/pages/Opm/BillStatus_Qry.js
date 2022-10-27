'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'BillCreateDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param {Object}  args 查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGetPro = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                oQueryPm.BillStatus = !oQueryPm.BillStatus ? Enumerable.From($(':checkbox')).Select("x=>x.value").ToArray().join(',') : oQueryPm.BillStatus.join(',');
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, 'QueryPage', oQueryPm, function (res) {
                    var oRes = res.DATA.rel;
                    
                    if (args.Excel) {//匯出
                        if (res.STATUSCODE != 200) {
                            showMsg(res.MSG, 'error');
                        }
                        else {
                            var oRes = res.DATA.rel;
                            DownLoadFile(oRes);
                            layer.close(args.Index);
                        }
                    }
                });
            },
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['300px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                           <button type="button" data-i18n="ExhibitionImport_Upd.Quotation" id="BillList" class="btn-custom green">帳單資料</button>\
                           <button type="button" data-i18n="common.BillAndPrice" id="BillAndPrice" class="btn-custom green">帳單成本金額</button>\
                         </div>',//╠common.Quotation⇒帳單資料╣╠common.BillAndPrice⇒帳單成本金額╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            fnGetPro({
                                Excel: true,
                                ExcelType: sToExcelType,
                                Index: idx
                            });
                        });
                        transLang(layero);
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
                        fnOpenPopToExcel();
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
                });

                $('#SearchBetween').change(function () {
                    var sValue = this.value;
                    $(':checkbox[name="BillStatus[]"]').prop('checked', false).prop('disabled', true);

                    if (sValue === '1') {
                        //帳單時間
                        //已審核,已銷帳,已過帳
                        $(':checkbox[name="BillStatus[]"][value="2"],:checkbox[name="BillStatus[]"][value="4"],:checkbox[name="BillStatus[]"][value="5"]').prop('checked', true);
                    }
                    else if (sValue === '2') {
                        //銷帳時間
                        //已過帳
                        $(':checkbox[name="BillStatus[]"][value="4"]').prop('checked', true);
                    }
                    else if (sValue === '3') {
                        //帳單創建時間
                        $(':checkbox[name="BillStatus[]"]').prop('checked', false).prop('disabled', false);
                    }

                }).change();

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
                                invalidMessage: '输入的数据无效！',
                                confirmDeleting: true,
                                deleteConfirm: "確定要刪除嗎？",
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
                                rowClick: function (args) {
                                    if (navigator.userAgent.match(/mobile/i)) {
                                        var sPrgId = args.item.BillType,
                                            sId = args.item.ParentId,
                                            sBillNO = args.item.BillNO,
                                            sIdName = 'ImportBillNO',
                                            sGoTab = '2';

                                        if (sPrgId === 'ExhibitionImport_Upd') {
                                            sGoTab = args.item.IsRetn === 'N' ? '3' : '9';
                                        }
                                        else if (sPrgId === 'ExhibitionExport_Upd') {
                                            sIdName = 'ExportBillNO';
                                            sGoTab = args.item.IsRetn === 'N' ? '3' : '5';
                                        }
                                        else if (sPrgId === 'OtherExhibitionTG_Upd') {
                                            sIdName = 'Guid';
                                            sGoTab = '3';
                                        }
                                        goToEdit(sPrgId, '?Action=Upd&GoTab=' + sGoTab + '&' + sIdName + '=' + sId + '&BillNO=' + sBillNO);
                                    }
                                },
                                rowDoubleClick: function (args) {
                                    var sPrgId = args.item.BillType,
                                        sId = args.item.ParentId,
                                        sBillNO = args.item.BillNO,
                                        sIdName = 'ImportBillNO',
                                        sGoTab = '2';

                                    if (sPrgId === 'ExhibitionImport_Upd') {
                                        sGoTab = args.item.IsRetn === 'N' ? '3' : '9';
                                    }
                                    else if (sPrgId === 'ExhibitionExport_Upd') {
                                        sIdName = 'ExportBillNO';
                                        sGoTab = args.item.IsRetn === 'N' ? '3' : '5';
                                    }
                                    else if (sPrgId === 'OtherExhibitionTG_Upd') {
                                        sIdName = 'Guid';
                                        sGoTab = '3';
                                    }
                                    parent.openPageTab(sPrgId, '?Action=Upd&GoTab=' + sGoTab + '&' + sIdName + '=' + sId + '&BillNO=' + sBillNO);
                                },
                                fields: [
                                    { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                                    {
                                        name: "BillNO", title: 'common.BillNO', align: "left", type: "text", width: 80
                                    },
                                    {
                                        name: "ExhibitioName", title: 'common.ExhibitionName', align: "left", type: "text", width: 150
                                    },
                                    {
                                        name: "PayerName", title: 'ExhibitionImport_Upd.Payer', align: "left", type: "text", width: 150
                                    },
                                    {
                                        name: "ResponsiblePersonName", title: 'common.ResponsiblePerson', align: "left", type: "text", width: 60
                                    },
                                    {
                                        name: "Currency", title: 'common.Financial_Currency', align: "left", type: "text", width: 50
                                    },
                                    {
                                        name: "ExchangeRate", title: 'common.ExchangeRate', align: "left", type: "text", width: 50
                                    },
                                    {
                                        name: "Advance", title: 'common.Financial_Advance', align: "right", type: "text", width: 60,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {// ╠common.NOTaxAmount⇒未稅金額╣
                                        name: "AmountSum", title: 'common.NOTaxAmount', align: "right", type: "text", width: 70,
                                        itemTemplate: function (val, item) {
                                            return !val ? '' : fMoney(parseFloat(val.replaceAll(',', '')), 2, item.ForeignCurrencyCode);
                                        }
                                    },
                                    {// ╠common.TaxAmount⇒稅金╣
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
                                        name: "CreateUserName", title: 'common.CreateUser', align: "left", type: "text", width: 70
                                    },
                                    {
                                        name: "BillCreateDate", title: 'common.CreateDate', type: "text", align: "center", width: 100,
                                        itemTemplate: function (val, item) {
                                            var rDate = !val ? item.CreateDate : val;
                                            return newDate(rDate);
                                        }
                                    },
                                    {
                                        name: "AuditVal", title: 'common.Bill_Status', type: "text", align: "center", width: 100,// ╠common.Bill_Status⇒帳單狀態╣
                                        itemTemplate: function (val, item) {
                                            var sStatusName = '';
                                            switch (val) {
                                                case '0':// ╠common.NotAudit⇒未提交審核╣
                                                    sStatusName = i18next.t("common.NotAudit");
                                                    break;
                                                case '1':// ╠common.InAudit⇒提交審核中╣
                                                    sStatusName = i18next.t("common.InAudit");
                                                    break;
                                                case '2':// ╠common.Audited⇒已審核╣
                                                    sStatusName = i18next.t("common.Audited");
                                                    break;
                                                case '3':// ╠common.NotPass⇒不通過╣
                                                    sStatusName = i18next.t("common.NotPass");
                                                    break;
                                                case '4':// ╠common.HasBeenRealized⇒已銷帳╣
                                                    sStatusName = i18next.t("common.HasBeenRealized");
                                                    break;
                                                case '5':// ╠common.HasBeenPost⇒已過帳╣
                                                    sStatusName = i18next.t("common.HasBeenPost");
                                                    break;
                                                case '6':// ╠common.HasVoid⇒已作廢╣
                                                    sStatusName = i18next.t("common.HasVoid");
                                                    break;
                                                case '7':// ╠common.HasReEdit⇒抽單中╣
                                                    sStatusName = i18next.t("common.HasReEdit");
                                                    break;
                                            }
                                            return sStatusName;
                                        }
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
                fnSetEpoDrop({
                    Select: $('#ExhibitionSN'),
                    Select2: true
                });
                fnSetCustomerWithGuid({
                    Select: $('#PayerGuid'),
                    Select2: true
                });
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);