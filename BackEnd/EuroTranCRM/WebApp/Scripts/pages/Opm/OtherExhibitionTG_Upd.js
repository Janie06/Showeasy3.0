'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sFlag = getUrlParam('Flag'),
    sGoTab = getUrlParam('GoTab'),
    sBillNOGO = getUrlParam('BillNO'),
    sAppointNO = getUrlParam('AppointNO'),
	sExhibitionNO = getUrlParam('ExhibitionNO'),
	sNewCustomers = getUrlParam('NewCustomers'),
    sCheckId = sDataId,
    oGlobalItem = {},
    fnPageInit = function () {
        var FeeItemCurrency = "TE,TG".indexOf(parent.UserInfo.OrgID) > -1 ? 'NTD' : 'RMB';
        var oGrid = null,
            oForm = $('#form_main'),
            oValidator = null,
            sServiceCode = '',
            sDeptCode = '',
            sCustomersOptionsHtml = '',
            sCustomersNotAuditOptionsHtml = '',
            sCurrencyOptionsHtml = '',
            sAccountingCurrencyOptionsHtml = '',
            oAddItem = {},
            oPrintMenu = {},
            oCurData = {},
            saGridData = [],
            saCustomers = [],
            saCurrency = [],
            saAccountingCurrency = [],
            saFeeClass = [],
			saNewExhibition = {},
			saNewCustomers = [],
            nowResponsiblePerson = '',

            /**
             * 獲取資料
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnGet = function () {
                if (sDataId) {
                    return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            otherexhibitiontg: {
                                Guid: sDataId
                            }
                        }
                    }, function (res) {
                        var oRes = $.parseJSON(res.d);
                        $('#VoidReason').text(oRes.VoidReason);
                        if (oRes.VoidReason) { $('.voidreason').show(); } else { $('.voidreason').hide(); }
                        if (oRes.IsVoid === 'Y') {
                            $('#Toolbar_Void').attr({ 'id': 'Toolbar_OpenVoid', 'data-i18n': 'common.Toolbar_OpenVoid' });
                        }
                        else {
                            $('#Toolbar_OpenVoid').attr({ 'id': 'Toolbar_Void', 'data-i18n': 'common.Toolbar_Void' });
                        }
						transLang($('#Toolbar'));
                    });
                }
                else {
                    oCurData.Quote = { guid: guid(), KeyName: 'Quote', AuditVal: '0', FeeItems: [] };
                    if (sAppointNO) {
                        oCurData.Quote.FeeItems = [{
                            guid: guid(),
                            FinancialCode: "TEC06",
                            FinancialCostStatement: "堆高機",
                            FinancialCurrency: "NTD",
                            FinancialUnitPrice: 876.19,
                            FinancialNumber: "1",
                            FinancialUnit: "SHPT",
                            FinancialAmount: 876.19,
                            FinancialExchangeRate: "1",
                            FinancialTWAmount: 876.19,
                            FinancialTaxRate: "0.05",
                            FinancialTax: 43.81,
                            Memo: "",
                            CreateUser: "peter.yang",
                            CreateDate: "2018/08/14 16:39:14"
                        }];
                    }
                    oCurData.EstimatedCost = { guid: guid(), KeyName: 'EstimatedCost', AuditVal: '0', FeeItems: [] };
                    oCurData.ActualCost = { guid: guid(), KeyName: 'ActualCost', AuditVal: '0', FeeItems: [] };
                    oCurData.Bills = [];
                    $('#AgentContactor').html(createOptions([]));
                    fnSetPermissions();
                    return $.Deferred().resolve().promise();
                }
            },
			/**
             * 獲取由名單過來的展覽資料
             * @return {Object} ajax物件
             */
            fnGetNewExhibition = function () {
                if (sAction === 'Add' && sExhibitionNO) {
                    return g_api.ConnectLite('Exhibition_Qry', 'QueryOne',
                        {
                            Guid: sExhibitionNO
                        },
                        function (res) {
                            if (res.RESULT) {
                                saNewExhibition = res.DATA.rel;
                            }
                        });
                }
                else {

                    return $.Deferred().resolve().promise();
                }
            },
			/**
             * 獲取由名單過來的參展廠商資料
             * @return {Object} ajax物件
             */
            fnGetNewCustomers = function () {
                if (sExhibitionNO && sNewCustomers) {
                    return g_api.ConnectLite('Exhibition_Upd', 'GetNewCustomers',
                        {
							SN: sExhibitionNO,
                            Guid: sNewCustomers
                        },
                        function (res) {
                            if (res.RESULT) {
                                saNewCustomers = res.DATA.rel;
                            }
                        });
                }
                else {

                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 取得帳單log資料
             * @return {Object}
             */
            fnGetBillLogData = function (Bill) {
                var LogData = {};
                LogData.OrgID = parent.OrgID;
                LogData.BillNO = Bill.BillNO;
                LogData.ExhibitioName = oCurData.ImportBillEName; //ExhibitioName 
                LogData.PayerName = '';
                if (Bill.Payer) {
                    var PayerData = Enumerable.From(saCustomers).Where(function (e) { return e.id === Bill.Payer; }).First();
                    LogData.PayerName = PayerData.text;
                }
                LogData.ResponsiblePersonName = oCurData.ResponsiblePerson;
                LogData.Currency = Bill.Currency;
                LogData.ExchangeRate = Bill.ExchangeRate;
                LogData.Advance = Bill.Advance;
                LogData.AmountSum = Bill.AmountSum;
                LogData.TaxSum = Bill.TaxSum;
                LogData.AmountTaxSum = Bill.AmountTaxSum;
                LogData.TotalReceivable = Bill.TotalReceivable;
                LogData.OpmBillCreateUserName = oCurData.CreateUser;
                LogData.ModifyUser = parent.UserID;
                return LogData;
            },
            /**
             * 新增或修改完之后重新查询资料
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnReSet = function () {
                fnGet().done(function (res) {
                    var oRes = $.parseJSON(res.d);
                    $('#VoidReason').text(oRes.VoidReason);
                    if (oRes.VoidReason) { $('.voidreason').show(); } else { $('.voidreason').hide(); }
                    getPageVal(); //緩存頁面值，用於清除
                });
            },
            /**
             * 新增資料
             * @param   flag{String} 新增或儲存後新增
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Weight = data.Weight === '' ? 0 : data.Weight;
                data.IsVoid = 'N';
                data.DepartmentID = sDeptCode;

                data.Quote = oCurData.Quote || {};
                data.Quote.AuditVal = oCurData.Quote.AuditVal || '0';
                data.Quote.FeeItems = oCurData.Quote.FeeItems || [];
                data.EstimatedCost = oCurData.EstimatedCost || {};
                data.EstimatedCost.AuditVal = oCurData.EstimatedCost.AuditVal || '0';
                data.EstimatedCost.FeeItems = oCurData.EstimatedCost.FeeItems || [];
                data.ActualCost = oCurData.ActualCost || {};
                data.ActualCost.AuditVal = oCurData.ActualCost.AuditVal || '0';
                data.ActualCost.FeeItems = oCurData.ActualCost.FeeItems || [];
                data.Bills = oCurData.Bills || [];
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                data.Exhibitors = JSON.stringify(saGridData);
                if (data.AgentContactor) {
                    data.AgentContactorName = $('#AgentContactor option:selected').text();
                }
                else {
                    data.AgentContactorName = '';
                }

                if (!data.ArrivalTime) delete data.ArrivalTime;
                if (!data.FreePeriod) delete data.FreePeriod;
                if (!data.ApproachTime) delete data.ApproachTime;
                if (!data.ExitTime) delete data.ExitTime;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;

                data.Guid = sDataId = guid();
                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: { otherexhibitiontg: data }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (sAppointNO) {
                            fnUpdAppointTag(sDataId);
                        }
                        if (flag === 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&Guid=' + sDataId); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                });
            },
            /**
             * 修改資料
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);
                data = packParams(data, 'upd');
                data.Weight = data.Weight === '' ? 0 : data.Weight;
                data.IsVoid = oCurData.IsVoid;

                data.Exhibitors = JSON.stringify(saGridData);

                data.Quote = oCurData.Quote || {};
                data.Quote.AuditVal = oCurData.Quote.AuditVal || '0';
                data.Quote.FeeItems = oCurData.Quote.FeeItems || [];
                data.EstimatedCost = oCurData.EstimatedCost || {};
                data.EstimatedCost.AuditVal = oCurData.EstimatedCost.AuditVal || '0';
                data.EstimatedCost.FeeItems = oCurData.EstimatedCost.FeeItems || [];
                data.ActualCost = oCurData.ActualCost || {};
                data.ActualCost.AuditVal = oCurData.ActualCost.AuditVal || '0';
                data.ActualCost.FeeItems = oCurData.ActualCost.FeeItems || [];
                data.Bills = oCurData.Bills || [];
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                if (data.AgentContactor) {
                    data.AgentContactorName = $('#AgentContactor option:selected').text();
                }
                else {
                    data.AgentContactorName = '';
                }
                if (data.ExhibitionNO) {
                    data.ImportBillName = $('#ExhibitionNO option:selected').text();
                }
                else {
                    data.ImportBillName = '';
                }
                if (!data.ArrivalTime) delete data.ArrivalTime;
                if (!data.FreePeriod) delete data.FreePeriod;
                if (!data.ApproachTime) delete data.ApproachTime;
                if (!data.ExitTime) delete data.ExitTime;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;

                delete data.Guid;
                if (!data.ArrivalTime) delete data.ArrivalTime;
                if (!data.FreePeriod) delete data.FreePeriod;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        otherexhibitiontg: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                        if (window.bLeavePage) {
                            setTimeout(function () {
                                pageLeave();
                            }, 1000);
                        }
                        fnUpdateBillInfo(sProgramId, sDataId);
                        oCurData.ResponsiblePerson = data.ResponsiblePerson;
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                        nowResponsiblePerson = oCurData.ResponsiblePerson;
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
					nowResponsiblePerson = oCurData.ResponsiblePerson;
                });
            },
            /**
             * 資料刪除
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnDel = function () {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        otherexhibitiontg: {
                            Guid: sDataId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsgAndGo(i18next.t("message.Delete_Success"), sQueryPrgId); // ╠message.Delete_Success⇒刪除成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
             * 資料作廢
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnVoid = function () {
                layer.open({
                    type: 1,
                    title: i18next.t('common.Toolbar_Void'),// ╠common.Toolbar_Void⇒作廢╣
                    shade: 0.75,
                    maxmin: true, //开启最大化最小化按钮
                    area: ['500px', '250px'],
                    content: '<div class="pop-box">\
                             <textarea name="VoidContent" id="VoidContent" style="min-width:300px;" class="form-control" rows="5" cols="20"></textarea>\
                          </div>',
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    success: function (layero, index) {
                    },
                    yes: function (index, layero) {
                        var data = {
                            IsVoid: 'Y',
                            VoidReason: $('#VoidContent').val()
                        };
                        if (!$('#VoidContent').val()) {
                            showMsg(i18next.t("message.VoidReason_Required")); // ╠message.VoidReason_Required⇒請填寫作廢原因╣
                            return false;
                        }
                        CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                            Params: {
                                otherexhibitiontg: {
                                    values: data,
                                    keys: { Guid: sDataId }
                                }
                            }
                        }, function (res) {
                            if (res.d > 0) {
                                showMsg(i18next.t("message.Void_Success"), 'success'); // ╠message.Void_Success⇒作廢成功╣
                                fnReSet();
                            }
                            else {
                                showMsg(i18next.t('message.Void_Failed'), 'error'); // ╠message.Void_Failed⇒作廢失敗╣
                            }
                        });
                        layer.close(index);
                    }
                });
            },
            /**
             * 資料作廢
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnOpenVoid = function () {
                var data = {
                    IsVoid: 'N',
                    VoidReason: ''
                };
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        otherexhibitiontg: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.OpenVoid_Success"), 'success'); // ╠message.OpenVoid_Success⇒啟用成功╣
                        fnReSet();
                    }
                    else {
                        showMsg(i18next.t("message.OpenVoid_Failed"), 'error'); // ╠message.OpenVoid_Failed⇒啟用失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.OpenVoid_Failed"), 'error'); // ╠message.OpenVoid_Failed⇒啟用失敗╣
                });
            },
            /**
             * 設定客戶下拉選單
             * @param  無
             * @return 無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnSetCustomersDrop = function () {
                return g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
                    if (res.RESULT) {
                        saCustomers = res.DATA.rel;
                        var saContactors = []
                        if (saCustomers.length > 0) {
                            sCustomersOptionsHtml = createOptions(saCustomers, 'id', 'text');
                            $('#Agent').html(sCustomersOptionsHtml).on('change', function () {
                                var sAgent = this.value;
                                if (sAgent) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sAgent; }).First();
                                    saContactors = JSON.parse(oCur.Contactors || '[]');
                                    $('#AgentContactor').html(createOptions(saContactors, 'guid', 'FullName')).off('change').on('change', function () {
                                        var sContactor = this.value;
                                        if (sContactor) {
                                            var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid === sContactor; }).First();
                                            $('#AgentEamil').val(oContactor.Email);
                                            $('#AgentTelephone').val(oContactor.TEL1);
                                        }
                                        else {
                                            $('#AgentEamil').val('');
                                            $('#AgentTelephone').val('');
                                        }
                                        bRequestStorage = true;
                                    });
                                }
                                else {
                                    $('#AgentContactor').html(createOptions([]));
                                }
                            });
                            var saNotAuditCurs = Enumerable.From(saCustomers).Where(function (e) { return e.IsAudit === 'Y'; }).ToArray();
                            sCustomersNotAuditOptionsHtml = createOptions(saNotAuditCurs, 'id', 'text');
                            $('#ImportPerson').html(sCustomersNotAuditOptionsHtml);
                        }
                        select2Init();
                    }
                });
            },

            /**
             * 取得當年度幣值設定
             */
            fnGetCurrencyThisYear = function (BillCreateTime) {
                return fnGetCurrencyByYear({
                    Year: BillCreateTime, CallBack: function (data) {
                        saAccountingCurrency = data;
                        sAccountingCurrencyOptionsHtml = createOptions(saAccountingCurrency, 'ArgumentID', 'ArgumentValue', false, 'Correlation');
                    }
                });
            },


            /**------------------------帳單部分---------------------------Start*/

            /**
             * 添加費用項目
             * @param：that(Object)當前dom對象
             * @return：data(Object)當前費用項目
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnPlusFeeItem = function (that, parentdata, feeinfo, currency) {
                var oFinancial = $(that).parents('.financial'),
                    oTable = oFinancial.find('tbody'),
                    sId = oTable.attr('data-id'),
                    sBillNO = oTable.attr('data-billno') || '',
                    sMainCurrency = oFinancial.find('[data-id="Currency"]').val() || FeeItemCurrency;

                oTable.find('tr').not('.fee-add').find('.jsgrid-cancel-edit-button').click();
                var fnSum = function () {
                    var iPrice = oUnitPrice.val() || 0,
                        iNumber = oNumber.val().replaceAll(',', ''),
                        iExchangeRate = oExchangeRate.val(),
                        sFinancialCurrency = oCurrency.val(),
                        iAmount = 0,
                        bForn = (currency === undefined || currency === 'NTD');
                    bForn = true;
                    iPrice = iPrice === '' ? 0 : parseFloat(iPrice);
                    iExchangeRate = iExchangeRate === '' ? 1 : parseFloat(iExchangeRate);
                    iNumber = iNumber === '' ? 0 : parseFloat(iNumber);
                    iAmount = iPrice * iNumber;
                    oAmount.attr('data-value', iAmount.toFloat(2)).val(fMoney(iAmount, 2));
                    oTWAmount.attr('data-value', (iAmount * iExchangeRate).toFloat(2)).val(fMoney(iAmount * iExchangeRate, 2));
                    if (oTaxRate.val()) {
                        var iTaxRate = oTaxRate.val().toPoint();
                        oTax.attr('data-value', (iAmount * iTaxRate * (bForn ? iExchangeRate : 1)).toFloat(2)).val(fMoney(iAmount * iTaxRate * (bForn ? iExchangeRate : 1), 2));
                    }
                },
                    oTR_Old = null,
                    oTR = $('<tr />', { class: 'jsgrid' }),
                    oTD = $('<td />', { class: 'wcenter', 'style': 'padding: 2px !important;' }),
                    oCode = $('<select />', {
                        class: 'form-control w100p', change: function () {
                            var sFeeVal = this.value,
                                sFeeText = $(this).find("option:selected").text();
                            oCostStatement.val(sFeeText.replace(sFeeVal + '-', '').replace('*', ''));
                            if ('TE001,TE199,TE299,TG001,TG199,TG299,SG001,SG199,SG299'.indexOf(sFeeVal) > -1) {
                                oCostStatement.removeAttr('disabled');
                            }
                            else {
                                oCostStatement.prop('disabled', true);
                            }
                        }
                    }),
                    oCostStatement = $('<input />', { class: 'form-control w100p', 'style': 'width:260px !important;' }),
                    oMemo = $('<textarea />', { class: 'form-control w100p', rows: '2', cols: '10' }),
                    oCurrency = $('<select />', {
                        class: 'form-control w100p', html: sCurrencyOptionsHtml, change: function () {
                            var sCurrencyId = this.value;
                            if (sCurrencyId) {
                                var oCurrency = Enumerable.From(saCurrency).Where(function (e) { return e.id === sCurrencyId; }).First();
                                oExchangeRate.val(oCurrency.Correlation || '').change();
                            }
                        }
                    }).css('cssText', 'width:80px !important').val(sMainCurrency),
                    oUnitPrice = $('<input />', { class: 'form-control w100p', 'data-type': 'float', 'data-name': 'float', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
                    oNumber = $('<input />', { class: 'form-control w100p', 'data-type': 'float', 'data-name': 'float', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
                    oUnit = $('<input />', { class: 'form-control w100p' }),
                    oAmount = $('<input />', { class: 'form-control w100p', 'data-type': 'int', 'data-name': 'int', 'readonly': 'readonly' }),
                    oExchangeRate = $('<input />', { class: 'form-control w100p', value: '1.000', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
                    oTWAmount = $('<input />', { class: 'form-control w100p', 'data-type': 'int', 'data-name': 'int', 'readonly': 'readonly' }),
                    oTaxRate = $('<input />', { class: 'form-control w100p', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
                    oTax = $('<input />', { class: 'form-control w100p', 'data-type': 'int', 'data-name': 'int', 'readonly': 'readonly' });
                let Selector = feeinfo ? 'oBillPayers-' + feeinfo.guid : 'oBillPayers';
                var oBillPayers = $('<select />', { class: 'form-control w100p ' + Selector, 'multiple': 'multiple' });
                var oConfirm = $('<input />', {
                        class: 'jsgrid-button jsgrid-update-button', type: 'button', title: i18next.t('common.Confirm'), click: function () {// ╠common.Confirm⇒確認╣
                            var sError = '';
                            if (!oCode.val()) {
                                sError += i18next.t("common.FinancialCode_required") + '<br/>'; // ╠common.FinancialCode_required⇒請選擇費用代號╣
                            }
                            if (!oCostStatement.val() && !oMemo.val()) {
                                sError += i18next.t("common.FinancialCostStatement_required") + '<br/>'; // ╠common.FinancialCostStatement_required⇒請輸入費用說明或備註╣
                            }
                            if (!oCurrency.val()) {
                                sError += i18next.t("common.Currency_required") + '<br/>'; // ╠common.Currency_required⇒請選擇幣別╣
                            }

                            if (sError) {
                                showMsg(sError);
                                return false;
                            }

                            var data = {};
                            data.FinancialCode = oCode.val();
                            data.FinancialCostStatement = oCostStatement.val();
                            data.Memo = oMemo.val();
                            data.FinancialCurrency = oCurrency.val();
                            data.FinancialUnitPrice = oUnitPrice.val();
                            data.FinancialNumber = oNumber.val();
                            data.FinancialUnit = oUnit.val();
                            data.FinancialAmount = oAmount.val();
                            data.FinancialExchangeRate = oExchangeRate.val();
                            data.FinancialTWAmount = oTWAmount.val();
                            data.FinancialTaxRate = oTaxRate.val() === '' ? 0 : oTaxRate.val();
                            data.FinancialTax = oTax.val();
                            if (sId === 'actualcost-box') {
                                data.BillNO = oBillPayers.val();
                                data.BillPayer = oBillPayers.attr('_payer') || '';
                            }
                            if (data.FinancialNumber.indexOf('.00') > 0)
                                data.FinancialNumber = data.FinancialNumber.replace('.00', '');
                            switch (sId) {
                                case 'quote-box':
                                    if (feeinfo) {
                                        $.each(parentdata.Quote.FeeItems, function (idx, item) {
                                            if (feeinfo.guid === item.guid) {
                                                data = packParams(data, 'upd');
                                                $.extend(item, item, data);
                                                return false;
                                            }
                                        });
                                    }
                                    else {
                                        data = packParams(data);
                                        data.guid = guid();
                                        parentdata.Quote.FeeItems.push(data);
                                    }
                                    fnBindFeeItem(oTable, parentdata, parentdata.Quote);
                                    break;
                                case 'estimatedcost-box':
                                    if (feeinfo) {
                                        $.each(parentdata.EstimatedCost.FeeItems, function (idx, item) {
                                            if (feeinfo.guid === item.guid) {
                                                data = packParams(data, 'upd');
                                                $.extend(item, item, data);
                                                return false;
                                            }
                                        });
                                    }
                                    else {
                                        data = packParams(data);
                                        data.guid = guid();
                                        parentdata.EstimatedCost.FeeItems.push(data);
                                    }
                                    fnBindFeeItem(oTable, parentdata, parentdata.EstimatedCost);
                                    break;
                                case 'bill_fees_' + sBillNO:
                                    $.each(parentdata.Bills, function (idx, bill) {
                                        if (sBillNO === bill.BillNO) {
                                            if (feeinfo) {
                                                $.each(bill.FeeItems, function (idx, item) {
                                                    if (feeinfo.guid === item.guid) {
                                                        data = packParams(data, 'upd');
                                                        $.extend(item, item, data);
                                                        return false;
                                                    }
                                                });
                                            }
                                            else {
                                                data = packParams(data);
                                                data.guid = guid();
                                                bill.FeeItems.push(data);
                                            }
                                            fnBindFeeItem(oTable, parentdata, bill);
                                            return false;
                                        }
                                    });
                                    break;
                                case 'actualcost-box':
                                    if (feeinfo) {
                                        $.each(parentdata.ActualCost.FeeItems, function (idx, item) {
                                            if (feeinfo.guid === item.guid) {
                                                data = packParams(data, 'upd');
                                                $.extend(item, item, data);
                                                return false;
                                            }
                                        });
                                    }
                                    else {
                                        data = packParams(data);
                                        data.guid = guid();
                                        parentdata.ActualCost.FeeItems.push(data);
                                    }
                                    fnBindFeeItem(oTable, parentdata, parentdata.ActualCost);
                                    break;
                            }
                            window.bRequestStorage = true;
                            oTR.remove();
                        }
                    }),
                    oCancel = $('<input />', {
                        class: 'jsgrid-button jsgrid-cancel-edit-button', type: 'button', title: i18next.t('common.Cancel'), click: function () {// ╠common.Cancel⇒取消╣
                            if (feeinfo) {
                                if (feeinfo.BillNO) {
                                    oTR_Old.find('select').val(feeinfo.BillNO);
                                }
                                oTR.after(oTR_Old).remove();
                                oTR_Old = null;
                            }
                            else {
                                oTR.remove();
                                $(that).prop('disabled', false);
                            }
                        }
                    });
                oTR.append(oTD.clone());
                oTR.append(oTD.clone().append(oCode));
                oTR.append(oTD.clone().append([oCostStatement, oMemo]));
                oTR.append(oTD.clone().append(oCurrency));
                oTR.append(oTD.clone().append(oUnitPrice));
                oTR.append(oTD.clone().append(oNumber));
                oTR.append(oTD.clone().append(oUnit));
                oTR.append(oTD.clone().append(oAmount));
                oTR.append(oTD.clone().append(oExchangeRate));
                oTR.append(oTD.clone().append(oTWAmount));
                oTR.append(oTD.clone().append(oTaxRate));
                oTR.append(oTD.clone().append(oTax));
                if (sId === 'actualcost-box') {
                    var saBillPayers = function () {
                        var saRetn = [];
                        $.each(parentdata.Bills, function (idx, bill) {
                            if (!bill.VoidReason) {
                                var sPayer = '',
                                    oCur = {};
                                if (bill.Payer) {
                                    oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === bill.Payer; }).First();
                                }
                                saRetn.push({
                                    id: bill.BillNO,
                                    text: oCur.text,
                                    val: bill.BillNO + '-' + (oCur.text || '') + '-' + bill.BillCreateDate
                                });
                            }
                        });
                        return saRetn;
                    }();
                    oTR.append(oTD.clone().append(oBillPayers.html(createOptions(saBillPayers, 'id', 'id', false)).change(function () {
                        var sBill = this.value;
                        if (sBill) {
                            var oBillPayer = Enumerable.From(saBillPayers).Where(function (e) { return e.id === sBill; }).First();
                            $(this).attr('_payer', oBillPayer.val || '');
                        }
                        else {
                            $(this).attr('_payer', '');
                        }
                    })));
                }
                oTR.append(oTD.clone().css('cssText', 'padding-top:30px !important;').append([oConfirm, oCancel]));

                oCode.html(createOptions(saFeeClass, 'id', 'text', true)).val(!feeinfo ? '' : feeinfo.FinancialCode).select2({ width: '160px' });
                if (parent.SysSet.TaxRate) {
                    oTaxRate.val(parent.SysSet.TaxRate);
                }

                if (feeinfo) {
                    oCode.val(feeinfo.FinancialCode);
                    oCostStatement.val(feeinfo.FinancialCostStatement.replace(feeinfo.FinancialCode + '-', ''));
                    oMemo.val(feeinfo.Memo);
                    oCurrency.val(feeinfo.FinancialCurrency);
                    oUnitPrice.val(feeinfo.FinancialUnitPrice);
                    oNumber.val(feeinfo.FinancialNumber);
                    oUnit.val(feeinfo.FinancialUnit);
                    oAmount.val(feeinfo.FinancialAmount);
                    oExchangeRate.val(feeinfo.FinancialExchangeRate);
                    oTWAmount.val(feeinfo.FinancialTWAmount);
                    oTaxRate.val(feeinfo.FinancialTaxRate);
                    oTax.val(feeinfo.FinancialTax);
                    if (sId === 'actualcost-box') {
                        oBillPayers.val(feeinfo.BillNO);
                    }
                    oTR_Old = $(that).parents('tr').clone(true);
                    $(that).parents('tr').after(oTR).remove();
                    if ('TE001,TE199,TE299,TG001,TG199,TG299,SG001,SG199,SG299'.indexOf(feeinfo.FinancialCode) > -1) {
                        oCostStatement.prop('disabled', false);
                    }
                    else {
                        oCostStatement.prop('disabled', true);
                    }
                }
                else {
                    oTable.append(oTR.addClass('fee-add'));
                    $(that).prop('disabled', true);
                }
                oTR.find('.' + Selector + ' option:first').remove();
                moneyInput($('[data-type="int"]'), 2, true);
                if (sId === 'actualcost-box') {
                    let mySelect = new vanillaSelectBox("." + Selector, {
                        search: true
                    });
                    mySelect.multipleSize = 2;
                    if (feeinfo) {
                        mySelect.setValue(feeinfo.BillNO);
                    }
                }
            },
            /**
             * 預設預約單資料
             */
            fnInitAppoint = function () {
                g_api.ConnectLite(Service.opm, 'InitAppoint', {
                    AppointNO: sAppointNO
                }, function (res) {
                    var oRes = res.DATA.rel;
                    //setFormVal(oForm, oRes.Base);
                    $('#ExhibitionDateStart').val(newDate(oRes.Base.ExhibitionDateStart, 'date', true));
                    $('#ExhibitionDateEnd').val(newDate(oRes.Base.ExhibitionDateEnd, 'date', true)); oAddItem.guid = guid();
                    $('#ExhibitionNO').val(oRes.Base.ExhibitionNO).trigger('change');
                    $('#ImportBillEName').val(oRes.Base.ImportBillEName);
                    $('#Hall').val(oRes.Base.Hall);
                    //$('#MuseumMumber').val(oRes.Base.MuseumMumber);
                    $.grep(oRes.Customers, function (item) {
                        saGridData.push({
                            guid: guid(),
                            AppointNO: item.AppointNO,
                            SupplierID: item.guid,
                            CustomerNO: item.CustomerNO,
                            UniCode: item.UniCode,
                            SupplierName: item.CustomerCName,
                            SupplierEName: item.CustomerEName,
                            Telephone: item.Telephone,
                            Email: item.Email,
                            Contactor: '',
                            ContactorName: '',
                            CreateUser: parent.UserID,
                            CreateDate: new Date().formate("yyyy/MM/dd HH:mm:ss")
                        });
                    });
                    fnGridInit();
                    oGrid.loadData();
                });
            },
            /**
             * 如果有預設預約單資料，新增完則回寫該ID到預約單
             * @param {String} id 【其他】ID
             */
            fnUpdAppointTag = function (id) {
                var saUpdPm = [];
                $.grep(saGridData, function (item) {
                    if (item.AppointNO) {
                        saUpdPm.push({
                            values: {
                                OtherId: id,
                                OtherIdFrom: 'OtherExhibitionTG_Upd'
                            },
                            keys: {
                                AppointNO: item.AppointNO,
                                OrgID: parent.OrgID
                            }
                        });
                    }
                });
                CallAjax(ComFn.W_Com, ComFn.GetUpd,
                    {
                        Params: {
                            packingorder: saUpdPm
                        }
                    });
            },
            /**
             * 綁定費用項目
             * @param {Array} files 上傳的文件
             */
            fnBindFeeItem = function (dom, parentdata, data, flag) {
                oGlobalItem.data = data;
                oGlobalItem.dom = dom;
                oGlobalItem.parentdata = parentdata;
                var iMathDot = 0;
                if (parentdata.Quote.QuotationOrBillingCurrency == "NTD") {
                    iMathDot = 0;
                } else {
                    iMathDot = 2;
                }
                $.each(data.FeeItems, function (idx, item) {
                    item.OrderBy = idx + 1;
                    item.FinancialUnitPrice = parseFloat((item.FinancialUnitPrice || '0').toString().replaceAll(',', ''));
                    item.FinancialAmount = parseFloat((item.FinancialAmount || '0').toString().replaceAll(',', ''));
                    item.FinancialTWAmount = parseFloat((item.FinancialTWAmount || '0').toString().replaceAll(',', ''));
                    item.FinancialTax = parseFloat((item.FinancialTax || '0').toString().replaceAll(',', ''));
                });
                data.FeeItems = Enumerable.From(data.FeeItems).OrderBy("x=>x.OrderBy").ToArray();
                var sFeeItemsHtml = '',
                    iSubtotal = 0,
                    iSubtotal_Tax = 0,
                    iSubtotal_NoTax = 0,
                    iTaxtotal = 0,
                    iTaxSubtotal = 0,
                    oFinancial = dom.parents('.financial'),
                    sDomId = dom.attr('data-id'),
                    bForn = (data.Currency === undefined || data.Currency === 'NTD'),
                    iOldBoxTotal = parseFloat((oFinancial.find('.subtotal').val() || '0').replaceAll(',', '')),
                    oTab = dom.parents('.tab-pane');

                $.each(data.FeeItems, function (idx, item) {
                    sFeeItemsHtml += '<tr>'
                        + '<td class="wcenter">' + (idx + 1) + '</td>'
                        + '<td class="wcenter">' + item.FinancialCode + '</td>'
                        + '<td>' + (!item.FinancialCostStatement ? item.Memo : item.FinancialCostStatement + (!item.Memo ? '' : '（' + item.Memo + '）')) + '</td>'
                        + '<td class="wcenter">' + item.FinancialCurrency + '</td>'
                        + '<td class="wright">' + item.FinancialUnitPrice + '</td>'
                        + '<td class="wcenter">' + item.FinancialNumber + '</td>'
                        + '<td>' + item.FinancialUnit + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialAmount, 2) + '</td>'
                        + '<td class="wcenter">' + item.FinancialExchangeRate + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialTWAmount, iMathDot) + '</td>'
                        + '<td class="wcenter">' + item.FinancialTaxRate + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialTax, 2) + '</td>'
                        + (data.KeyName === 'ActualCost' ? '<td class="wcenter billpayer w15p " data-billno="' + (item.BillNO || '') + '" data-value="' + item.guid + '"></td>' : '')
                        + (!flag ? '<td class="wcenter">'
                            + '<div class="fa-item col-sm-3"><i class="glyphicon glyphicon-pencil icon-p" data-value="' + item.guid + '" title="編輯"></i></div>'
                            + '<div class="fa-item col-sm-3"><i class="glyphicon glyphicon-trash icon-p" data-value="' + item.guid + '" title="刪除"></i></div>'
                            + ((data.FeeItems.length !== idx + 1) ? '<div class="fa-item col-sm-3"><i class="glyphicon glyphicon-arrow-down icon-p" data-value="' + item.guid + '" title="下移"></i></div>' : '<div class="fa-item col-sm-3"><i class="icon-p"></i></div>')
                            + ((idx !== 0) ? '<div class="fa-item col-sm-3"><i class="glyphicon glyphicon-arrow-up icon-p" data-value="' + item.guid + '" title="上移"></i></div>' : '<div class="fa-item col-sm-3"><i class="icon-p"></i></div>')
                            + '</td>' : '') +
                        +'</tr>';
                    if (item.FinancialTaxRate.toString().replace('%', '') !== '0') {
                        iSubtotal_Tax += parseFloat(item.FinancialTWAmount);
                    }
                    else {
                        iSubtotal_NoTax += parseFloat(item.FinancialTWAmount);
                    }
                });
                //計算總計(total)依序:1.參數的幣值 2.抓到財務的Currency設定 3.再來設定台幣。
                var CurrencyType = (data.Currency || oFinancial.find('[data-id="Currency"]').val()) || FeeItemCurrency;
                dom.html(sFeeItemsHtml);
                iSubtotal_Tax = fnRound(iSubtotal_Tax, data.Currency);
                iSubtotal_NoTax = fnRound(iSubtotal_NoTax, data.Currency);
				if(data.KeyName == "Quote"){
					CurrencyType = (data.QuotationOrBillingCurrency || oFinancial.find('[data-id="Currency"]').val()) || FeeItemCurrency;
					iSubtotal_Tax = fnRound(iSubtotal_Tax, data.QuotationOrBillingCurrency);
					iSubtotal_NoTax = fnRound(iSubtotal_NoTax, data.QuotationOrBillingCurrency);
				}
                iSubtotal = fnRound(iSubtotal_Tax + iSubtotal_NoTax, CurrencyType);
                var iTaxRate = parent.SysSet.TaxRate.toPoint();
                iTaxtotal = fnRound(iSubtotal_Tax * (iTaxRate === 0 ? 0.05 : iTaxRate), CurrencyType);
                iTaxSubtotal = iSubtotal + iTaxtotal;
                oFinancial.find('.subtotal').val(fMoney(iSubtotal, 2, CurrencyType));
                oFinancial.find('.taxtotal').val(fMoney(iTaxtotal, 2, CurrencyType));
                oFinancial.find('.boxtotal').val(fMoney(iTaxSubtotal, 2, CurrencyType));

                data.AmountSum = iSubtotal;
                data.TaxSum = iTaxtotal;
                data.AmountTaxSum = iTaxSubtotal;

                switch (data.KeyName) {
                    case 'ActualCost':
                        if (oTab[0].id === 'tab4') {
                            $('#tab4 .topshowsum').show();
                            $('#tab4 .actualsum').val(fMoney(iSubtotal, 2, data.Currency));
                            if (parentdata.ActualCost.AmountTaxSum > parentdata.EstimatedCost.AmountTaxSum) {
                                $('#tab4 #warnning_tips').show();
                            }
                            else {
                                $('#tab4 #warnning_tips').hide();
                            }
                        }
                        else if (oTab[0].id === 'tab6') {
                            oFinancial.find('.actualsum').val(fMoney(iSubtotal, 2, data.Currency));
                            var iAcount = 0;
                            $.each(parentdata.Bills, function (idx, _bill) {
                                if (_bill.AuditVal !== '6') {
                                    iAcount += _bill.AmountTaxSum;
                                }
                            });
                            oFinancial.find('.amountsum').val(fMoney(iAcount, 2, data.Currency));
                            if (parentdata.ActualCost.AmountTaxSum > parentdata.EstimatedCost.AmountTaxSum) {
                                oFinancial.parent().prev().find('.warnningtips').show();
                            }
                            else {
                                oFinancial.parent().prev().find('.warnningtips').hide();
                            }
                        }
                        break;
                    case 'EstimatedCost':
                        if (oTab[0].id === 'tab3') {
                            $('#tab3 .estimatedcostsum').val(fMoney(iSubtotal, 2, data.Currency));
                        }
                        break;
                    case 'Bill':
                        var iAdvance = parseFloat(oFinancial.find('.prepay').val().replaceAll(',', '')),
                            iExchangeRate = data.ExchangeRate || 1;
                        data.TotalReceivable = iTaxSubtotal - iAdvance;

                        oFinancial.find('.subtotal').val(fMoney(iSubtotal, 2, data.Currency));
                        oFinancial.find('.taxtotal').val(fMoney(iTaxtotal, 2, data.Currency));
                        oFinancial.find('.boxtotal').val(fMoney(iTaxSubtotal, 2, data.Currency));
                        oFinancial.find('.paytotal').val(fMoney(iTaxSubtotal - iAdvance, 2, data.Currency));

                        // 匯率
                        let TabTipExchangeRate = (bForn ? 1 : iExchangeRate);
                        // 純稅金(本幣別)
                        let TabTipTaxtotal = fnRound(iTaxtotal * TabTipExchangeRate, FeeItemCurrency);
                        // 未稅總額(本幣別)
                        let TabTipUntaxtotal = fnRound(iSubtotal * TabTipExchangeRate, FeeItemCurrency);

                        iOldBoxTotal = iOldBoxTotal * (bForn ? 1 : iExchangeRate);
                        if (oTab[0].id === 'tab3') {
                            if (data.AuditVal !== '6') {
                                let LastRowActualsum = parseFloat($('#tab3 .amountsum').val().replaceAll(',', '')) - iOldBoxTotal;
                                $('#tab3 .amountsum').val(fMoney(LastRowActualsum + TabTipUntaxtotal, 2, FeeItemCurrency));
                                $('#tab4 .amountsum').val($('#tab3 .amountsum').val());
                            }
                        }
                        else if (oTab[0].id === 'tab5') {
                            // 每筆退運帳單的預估成本
                            oFinancial.find('.topshowsum').show();
                            oFinancial.find('.estimatedcostsum').val(fMoney(parentdata.EstimatedCost.AmountSum, 2, data.Currency));
                            if (data.AuditVal !== '6') {
                                //退運帳單加總
                                let LastRowActualsum = parseFloat(oFinancial.find('.amountsum').val().replaceAll(',', '')) - iOldBoxTotal;
                                oFinancial.find('.amountsum').val(fMoney(LastRowActualsum + TabTipUntaxtotal, 2, FeeItemCurrency));
                                $('.bill-box-' + data.BillNO).find('.amountsum').val(oFinancial.find('.amountsum').val());
                            }
                        }
                        break;
                }

                /*計算$$*/
                if (oTab[0].id === 'tab3') {
                    if (data.KeyName === 'Bill')
                        fnCalcuBillsFee(oFinancial, '.BillForeignCurrency', '.BillMainCurrency', data.Currency, data.ExchangeRate);
                    else
                        fnCalcuQuotationFee(oFinancial.find('.QuotationForeignCurrency'), oFinancial.find('.QuotationMainCurrency'), parentdata.Quote.QuotationOrBillingCurrency, parentdata.Quote.AccountingExchangeRate);

                }

                dom.parents('.financial').find('.plusfeeitem').prop('disabled', false);
                fnSetDisabled(oFinancial, data, parentdata);

                oFinancial.find('.input-value').on('change', function () {
                    var that = this,
                        sId = $(that).attr('data-id');
                    data[sId] = $(that).val();
                    window.bRequestStorage = true;
                });

                if (data.KeyName === 'ActualCost') {
                    var saBillPayers = function () {
                        var saRetn = [];
                        $.each(parentdata.Bills, function (idx, bill) {
                            if (!bill.VoidReason) {
                                var sPayer = '',
                                    oCur = {};
                                if (bill.Payer) {
                                    oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === bill.Payer; }).First();
                                }
                                saRetn.push({
                                    id: bill.BillNO,
                                    text: oCur.text,
                                    val: bill.BillNO + '-' + (oCur.text || '') + '-' + bill.BillCreateDate
                                });
                            }
                        });
                        return saRetn;
                    }();
                    dom.find('.billpayer').each(function () {
                        var sGuid = $(this).attr('data-value'),
                            sBillNO = $(this).attr('data-billno'),
                            Selector = 'oBillPayers-' + sGuid;
                        $(this).append($('<select>', {
                            class: 'form-control w100p ' + Selector, 'multiple': 'multiple',
                            html: createOptions(saBillPayers, 'id', 'id', false),
                            change: function () {
                                var sBill = this.value;
                                $.each(parentdata.ActualCost.FeeItems, function (idx, item) {
                                    if (sGuid === item.guid) {
                                        var SelectedValues = getSelectedValues('.oBillPayers-' + this.guid);
                                        var MappedPayers = Enumerable.From(saBillPayers).Where(function (e) {
                                            return SelectedValues.indexOf(e.id) > -1;
                                        }).ToArray();
                                        item.BillNO = MappedPayers.map(c => c.id).join(',');
                                        item.BillPayer = MappedPayers.map(c => c.val).join(',');
                                        return false;
                                    }
                                });
                            }
                        }));
                        $(this).find('.' + Selector + ' option:first').remove();
                        let mySelect = new vanillaSelectBox('.' + Selector, {
                            search: true,
                            maxHeight: 160,
                            maxWidth: 200,
                        });
                        mySelect.multipleSize = 2;
                        mySelect.setValue(sBillNO);
                        $('#btn-group-\\.' + Selector).click(function (e) {
                            $('.vsb-menu').css('display', 'none');
                            $('#btn-group-\\.' + Selector).find('.vsb-menu').css('display', 'block');
                        })
                    });
                }

                dom.find('.glyphicon-pencil').off('click').on('click', function () {
                    var that = this,
                        sGuid = $(that).attr('data-value'),
                        oCurFee = {},
                        sBillNO = dom.attr('data-billno') || '';

                    if ($(that).hasClass('disabled')) { return; }//如果禁用後就不執行

                    switch (sDomId) {
                        case 'quote-box':
                            $.each(parentdata.Quote.FeeItems, function (idx, item) {
                                if (sGuid === item.guid) {
                                    oCurFee = item;
                                    return false;
                                }
                            });
                            break;
                        case 'estimatedcost-box':
                            $.each(parentdata.EstimatedCost.FeeItems, function (idx, item) {
                                if (sGuid === item.guid) {
                                    oCurFee = item;
                                    return false;
                                }
                            });
                            break;
                        case 'bill_fees_' + sBillNO:
                            $.each(parentdata.Bills, function (idx, bill) {
                                if (sBillNO === bill.BillNO) {
                                    $.each(bill.FeeItems, function (idx, item) {
                                        if (sGuid === item.guid) {
                                            oCurFee = item;
                                            return false;
                                        }
                                    });
                                    return false;
                                }
                            });
                            break;
                        case 'actualcost-box':
                            $.each(parentdata.ActualCost.FeeItems, function (idx, item) {
                                if (sGuid === item.guid) {
                                    oCurFee = item;
                                    return false;
                                }
                            });
                            break;
                    }
                    fnPlusFeeItem(that, parentdata, oCurFee, data.Currency);
                });
                dom.find('.glyphicon-trash').off('click').on('click', function () {
                    var that = this,
                        sGuid = $(that).attr('data-value'),
                        saNewList = [],
                        sBillNO = dom.attr('data-billno') || '';

                    if ($(that).hasClass('disabled')) { return; }//如果禁用後就不執行

                    switch (sDomId) {
                        case 'quote-box':
                            $.each(parentdata.Quote.FeeItems, function (idx, item) {
                                if (sGuid !== item.guid) {
                                    saNewList.push(item);
                                }
                            });
                            parentdata.Quote.FeeItems = saNewList;
                            fnBindFeeItem(dom, parentdata, parentdata.Quote);
                            break;
                        case 'estimatedcost-box':
                            $.each(parentdata.EstimatedCost.FeeItems, function (idx, item) {
                                if (sGuid !== item.guid) {
                                    saNewList.push(item);
                                }
                            });
                            parentdata.EstimatedCost.FeeItems = saNewList;
                            fnBindFeeItem(dom, parentdata, parentdata.EstimatedCost);
                            break;
                        case 'bill_fees_' + sBillNO:
                            $.each(parentdata.Bills, function (idx, bill) {
                                if (sBillNO === bill.BillNO) {
                                    $.each(bill.FeeItems, function (idx, item) {
                                        if (sGuid !== item.guid) {
                                            saNewList.push(item);
                                        }
                                    });
                                    bill.FeeItems = saNewList;
                                    fnBindFeeItem(dom, parentdata, bill);
                                    return false;
                                }
                            });
                            break;
                        case 'actualcost-box':
                            $.each(parentdata.ActualCost.FeeItems, function (idx, item) {
                                if (sGuid !== item.guid) {
                                    saNewList.push(item);
                                }
                            });
                            parentdata.ActualCost.FeeItems = saNewList;
                            fnBindFeeItem(dom, parentdata, parentdata.ActualCost);
                            break;
                    }
                    $(that).parents('tr').remove();
                });

                dom.find('.glyphicon-arrow-down').off('click').on('click', function () {
                    var that = this,
                        sGuid = $(that).attr('data-value'),
                        iOrderBy = 0;

                    if ($(that).hasClass('disabled')) { return; }//如果禁用後就不執行

                    $.each(data.FeeItems, function (n, item) {
                        if (sGuid === item.guid) {
                            iOrderBy = item.OrderBy;
                            item.OrderBy++;
                        }
                        if (iOrderBy !== 0 && iOrderBy === n) {
                            item.OrderBy--;
                            return false;
                        }
                    });
                    data.FeeItems = Enumerable.From(data.FeeItems).OrderBy("x=>x.OrderBy").ToArray();
                    fnBindFeeItem(dom, parentdata, data);
                });

                dom.find('.glyphicon-arrow-up').off('click').on('click', function () {
                    var that = this,
                        sGuid = $(that).attr('data-value'),
                        iOrderBy = Enumerable.From(data.FeeItems).Where(function (e) { return e.guid === sGuid; }).First().OrderBy;

                    if ($(that).hasClass('disabled')) { return; }//如果禁用後就不執行

                    $.each(data.FeeItems, function (n, item) {
                        if (iOrderBy - 2 === n) {
                            item.OrderBy++;
                        }
                        if (sGuid === item.guid) {
                            item.OrderBy--;
                            return false;
                        }
                    });
                    data.FeeItems = Enumerable.From(data.FeeItems).OrderBy("x=>x.OrderBy").ToArray();
                    fnBindFeeItem(dom, parentdata, data);
                });
            },
            /**
             * 審核通過後禁用頁面欄位
             * @param dom{Object}當前區塊
             * @param data{Object}當前區塊
             * @param pdata{Object}當前區塊
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnSetDisabled = function (dom, data, pdata) {
                if (data) {
                    if (data.BillNO) {
                        switch (data.AuditVal) {
                            case '0':// ╠common.NotAudit⇒未提交審核╣
                                dom.find('.status-font').text(i18next.t("common.NotAudit")).css('color', 'red');
                                break;
                            case '1':// ╠common.InAudit⇒提交審核中╣
                                dom.find('.status-font').text(i18next.t("common.InAudit")).css('color', 'blue');
                                break;
                            case '2':// ╠common.Audited⇒已審核╣
                                dom.find('.status-font').text(i18next.t("common.Audited")).css('color', 'green');
                                break;
                            case '3':// ╠common.NotPass⇒不通過╣
                                dom.find('.status-font').text(i18next.t("common.NotPass")).css('color', 'red');
                                break;
                            case '4':// ╠common.NotPass⇒已銷帳╣
                                dom.find('.status-font').text(i18next.t("common.HasBeenRealized")).css('color', 'red');
                                break;
                            case '5':// ╠common.HasBeenPost⇒已過帳╣
                                dom.find('.status-font').text(i18next.t("common.HasBeenPost")).css('color', 'green');
                                break;
                            case '6':// ╠common.HasVoid⇒已作廢╣
                                dom.find('.status-font').text(i18next.t("common.HasVoid")).css('color', '#b2b1b1');
                                break;
                            case '7':// ╠common.HasReEdit⇒抽單中╣
                                dom.find('.status-font').text(i18next.t("common.HasReEdit")).css('color', 'blue');
                                break;
                        }
                    }
                    dom.find('.bill-status-box').show();
                    dom.find('.notpass-reason-box').hide();
                    let DraftRecipt = false;
                    switch (data.AuditVal) {
                        case '0':// ╠common.NotAudit⇒未提交審核╣
                            dom.find('.bill-status').text(i18next.t("common.NotAudit")).css('color', 'red');
                            dom.find('.billpost,.cancelpost,.writeoff,.canceloff,.reedit,.cancelreedit').hide();
                            dom.find('.submittoaudit,.synquote,.alreadyaudit').show();
                            dom.find('.alreadyaudit,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                            if (parent.UserInfo.MemberID === pdata.ResponsiblePerson || parent.UserInfo.MemberID === pdata.CreateUser) {
                                if (data.FeeItems.length > 0) {
                                    dom.find('.submittoaudit').removeAttr('disabled');
                                    dom.parent().next().find('.synquote').removeAttr('disabled');
                                }
                                else {
                                    dom.find('.submittoaudit').prop('disabled', true);
                                    dom.parent().next().find('.synquote').prop('disabled', true);
                                }
                            }
                            else {
                                if (data.KeyName === 'Bill') {
                                    dom.find(':input,textarea,.alreadyaudit,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                                    dom.find('.icon-p').addClass('disabled');
                                }
                            }
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.prepay,.mprepay,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            dom.find('.bills-print').removeAttr('disabled');
                            DraftRecipt = true;
                            break;
                        case '1':// ╠common.InAudit⇒提交審核中╣
                            dom.find('.bill-status').text(i18next.t("common.InAudit")).css('color', 'blue');
                            dom.find('.billpost,.cancelpost,.writeoff,.canceloff,.cancelaudi').hide();
                            dom.find('.submittoaudit,.synquote,.alreadyaudit,.reedit,.cancelreedit').show();
                            dom.find(':input,textarea,.plusfeeitem,.importfeeitem,.copyfeeitem,.plusfeeitemstar,select,.submittoaudit,.synquote,.billpost,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                            dom.find('.icon-p').addClass('disabled');
                            if (parent.UserInfo.MemberID === pdata.ResponsiblePerson || parent.UserInfo.MemberID === pdata.CreateUser) {
                                dom.find('.reedit').removeAttr('disabled');
                            }
                            if (parent.UserInfo.UsersDown.indexOf(pdata.ResponsiblePerson) > -1 || parent.UserInfo.UsersBranch.indexOf(pdata.ResponsiblePerson) > -1 || parent.SysSet.BillAuditor.indexOf(parent.UserInfo.MemberID) > -1) {
                                dom.find('.alreadyaudit').removeAttr('disabled');
                            }
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.reedit,.cancelreedit').hide();
                                dom.find('.prepay,.mprepay,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            dom.find('.bills-print').removeAttr('disabled');
                            break;
                        case '2':// ╠common.Audited⇒已審核╣
                            dom.find('.bill-status').text(i18next.t("common.Audited")).css('color', 'green');
                            dom.find('.submittoaudit,.synquote,.alreadyaudit,.writeoff,.canceloff,.reedit,.cancelreedit').hide();
                            dom.find('.billpost,.cancelpost,.cancelaudi,.writeoff').show();
                            dom.find(':input,textarea,.plusfeeitem,.importfeeitem,.copyfeeitem,.plusfeeitemstar,select,.submittoaudit,.synquote,.billpost,.alreadyaudit,.cancelpost,.writeoff,[data-id="Payer"]').attr('disabled', 'disabled');
                            dom.find('.icon-p').addClass('disabled');
                            dom.find('.bills-print').removeAttr('disabled');
                            if (parent.UserInfo.MemberID === pdata.ResponsiblePerson || parent.UserInfo.MemberID === pdata.CreateUser) {
                                dom.find('.billpost,.receiptnumberbtn,.checkauditdate').removeAttr('disabled');
                            }
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.prepay,.mprepay,.cancelaudi,.billvoid,.writeoff').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                                dom.find('.billpost,.cancelpost').hide();
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            break;
                        case '3':// ╠common.NotPass⇒不通過╣
                            dom.find('.notpass-reason-text').text(data.NotPassReason || '');
                            dom.find('.bill-status').text(i18next.t("common.NotPass")).css('color', 'red');
                            dom.find('.billpost,.cancelpost,.writeoff,.canceloff,.reedit,.cancelreedit').hide();
                            dom.find('.submittoaudit,.synquote,.alreadyaudit,.notpass-reason-box').show();
                            if (parent.UserInfo.MemberID === pdata.ResponsiblePerson || parent.UserInfo.MemberID === pdata.CreateUser) {
                                if (data.FeeItems.length > 0) {
                                    dom.find('.submittoaudit').removeAttr('disabled');
                                    dom.parent().next().find('.synquote').removeAttr('disabled');
                                }
                                else {
                                    dom.find('.submittoaudit').prop('disabled', true);
                                    dom.parent().next().find('.synquote').prop('disabled', true);
                                }
                            }
                            else {
                                dom.find(':input,textarea,.alreadyaudit,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                                dom.find('.icon-p').addClass('disabled');
                            }
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.prepay,.mprepay,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            break;
                        case '4':// ╠common.HasBeenRealized⇒已銷帳╣
                            dom.find('.bill-status').text(i18next.t("common.HasBeenRealized")).css('color', 'red');
                            dom.find('.submittoaudit,.synquote,.alreadyaudit,.cancelaudi,.billpost,.cancelpost,.writeoff,.reedit,.cancelreedit').hide();
                            dom.find('.canceloff').show();
                            dom.find(':input,textarea,.plusfeeitem,.importfeeitem,.copyfeeitem,.plusfeeitemstar,select,.alreadyaudit,.cancelaudi,.cancelpost,.writeoff,.submittoaudit,.synquote,.billpost').attr('disabled', 'disabled');
                            dom.find('.icon-p').addClass('disabled');
                            dom.find('.bills-print').removeAttr('disabled');
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.canceloff,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            break;
                        case '5':// ╠common.HasBeenPost⇒已過帳╣
                            dom.find('.bill-status').text(i18next.t("common.HasBeenPost")).css('color', 'green');
                            dom.find('.billpost,.submittoaudit,.synquote,.alreadyaudit,.cancelaudi,.reedit,.cancelreedit').hide();
                            dom.find('.cancelpost,.writeoff,.canceloff').show();
                            dom.find(':input,textarea,.plusfeeitem,.importfeeitem,.copyfeeitem,.plusfeeitemstar,select,.alreadyaudit,.cancelaudi,.cancelpost,.writeoff,.bills-print,.submittoaudit,.synquote,.billpost').attr('disabled', 'disabled');
                            dom.find('.icon-p').addClass('disabled');
                            dom.find('.bills-print').removeAttr('disabled');
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.cancelpost,.writeoff,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
                            break;
                        case '6':// ╠common.HasVoid⇒已作廢╣
                            dom.find('.notpass-reason-text').text(data.VoidReason || '');
                            dom.find('.bill-status').text(i18next.t("common.HasVoid")).css('color', '#b2b1b1');
                            dom.find('button').not('.plusfeeitem').hide();
                            dom.find('.notpass-reason-box').show();
                            dom.find(':input,textarea').attr('disabled', 'disabled');
                            dom.find('.icon-p').addClass('disabled');
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.billvoid').removeAttr('disabled');
                            }
                            break;
                        case '7':// ╠common.HasReEdit⇒抽單中╣
                            dom.find(':input,textarea').removeAttr('disabled');
                            dom.find('.icon-p').removeClass('disabled');
                            dom.find('.bill-status').text(i18next.t("common.HasReEdit")).css('color', 'blue');
                            dom.find('.submittoaudit,.synquote,.alreadyaudit').show();
                            dom.find('.billpost,.cancelpost,.writeoff,.canceloff').hide();
                            dom.find('.alreadyaudit,.cancelaudi,.cancelpost,.writeoff,.bills-print,.submittoaudit,.synquote,.reedit').attr('disabled', 'disabled');
                            if (parent.UserInfo.MemberID === pdata.ResponsiblePerson || parent.UserInfo.MemberID === pdata.CreateUser) {//如果有資料且是null或者N
                                dom.find('.cancelreedit').removeAttr('disabled');
                            }
                            else {
                                if (data.KeyName === 'Bill') {
                                    dom.find(':input,textarea,.alreadyaudit,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                                    dom.find('.icon-p').addClass('disabled');
                                }
                            }
                            if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                                dom.find('.prepay,.mprepay,.billvoid').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billvoid').hide();
                            }
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {//超級管理員
                                dom.find('.billdelete').removeAttr('disabled');
                            }
                            else {
                                dom.find('.billdelete').hide();
                            }
							dom.find('.bills-print').removeAttr('disabled');
                            break;
                    }

                    if (DraftRecipt) {
                        dom.find("[data-action='Print_Receipt']").hide();
                        dom.find("[data-action='Download_Receipt']").hide();
                    }
                    else {
                        dom.find("[data-action='Print_Receipt']").show();
                        dom.find("[data-action='Download_Receipt']").show();
                    }
                    if (parent.UserInfo.roles.indexOf('Business') > -1) {
                        dom.find('[data-id="ExchangeRate"]').attr('disabled', 'disabled');
                    }
                    fnOpenAccountingArea(dom.find('.OnlyForAccounting'), parent.UserInfo.roles);
                }
            },
            /**
            * 批次添加費用項目&收藏
            * @param：that(Object)當前dom對象
            * @return：data(Object)當前費用項目
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnPlusFeeItemStar = function (that, handle, parentdata) {
                var oFinancial = $(that).parents('.financial'),
                    oTable = oFinancial.find('tbody'),
                    sId = oTable.attr('data-id'),
                    sBillNO = oTable.attr('data-billno') || '',
                    oOption = {};

                oOption.Callback = function (data) {
                    if (data.length > 0) {
                        parentdata.Quote.FeeItems = clone(data);
                        fnBindFeeItem(handle, parentdata, parentdata.Quote);
                        $(that).prev().prop('disabled', false);
                    }
                };
                fnStarFeeItems(oOption);
            },
            /**
             * 匯入費用項目
             * @param：that(Object)當前dom對象
             * @return：data(Object)當前費用項目
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnImportFeeitems = function (type, parentdom, parentdata) {
                $('#importfile').val('').off('change').on('change', function () {
                    if (this.value.indexOf('.xls') > -1 || this.value.indexOf('.xlsx') > -1) {
                        var sFileId = guid(),
                            sFileName = this.value;
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
                            secureuri: false,
                            fileElementId: 'importfile',
                            success: function (data, status) {
                                var that = this;
                                g_api.ConnectLite('Exhibition', 'GetImportFeeitems', {//匯入費用項目
                                    OrgID: parent.OrgID,
                                    FileId: sFileId,
                                    FileName: sFileName
                                }, function (res) {
                                    if (res.RESULT) {
                                        if (res.DATA.rel.length > 0) {
                                            if (type === 'quote') {
                                                parentdata.Quote.FeeItems = res.DATA.rel;
                                                fnBindFeeItem(parentdom.find('[data-id="quote-box"]'), parentdata, parentdata.Quote);
                                            }
                                            else if (type === 'estimatedcost') {
                                                parentdata.EstimatedCost.FeeItems = res.DATA.rel;
                                                fnBindFeeItem(parentdom.find('[data-id="estimatedcost-box"]'), parentdata, parentdata.EstimatedCost);
                                            }
                                            else if (type === 'actualcost') {
                                                parentdata.ActualCost.FeeItems = res.DATA.rel;
                                                fnBindFeeItem(parentdom.find('[data-id="actualcost-box"]'), parentdata, parentdata.ActualCost);
                                            }
                                        }
                                        else {
                                            showMsg(i18next.t("message.NoMatchData")); // ╠message.NoMatchData⇒找不到相關資料╣
                                        }
                                    }
                                    else {
                                        showMsg(i18next.t('message.ProgressError') + '<br>' + res.MSG, 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                    }
                                }, function () {
                                    showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                });
                            },
                            error: function (data, status, e) {
                                showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                            }
                        });
                        bRequestStorage = true;
                    }
                    else {
                        showMsg(i18next.t("message.FileTypeError"), 'error'); // ╠message.FileTypeError⇒文件格式錯誤╣
                    }
                }).click();
            },
            /**
            * 複製費用項目
            * @param：that(Object)當前dom對象
            * @return：quote(Object)當前費用項目
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnCopyFeeitems = function (type, parentdom, parentdata) {
                var oOption = {};
                oOption.Callback = function (data) {
                    if (data.length > 0) {
                        $.each(data, function (idx, item) {
                            item.FinancialUnitPrice = 0;
                            item.FinancialNumber = 0;
                            item.FinancialAmount = 0;
                            item.FinancialTWAmount = 0;
                            item.FinancialTaxRate = '0%';
                            item.FinancialTax = 0;
                            item.CreateUser = parent.UserID;
                            item.CreateDate = newDate(null, true);
                        });
                        if (type === 'quote') {
                            parentdata.Quote.FeeItems = data;
                            fnBindFeeItem(parentdom.find('[data-id="quote-box"]'), parentdata, parentdata.Quote);
                        }
                    }
                };
                fnCopyFee(oOption);
            },
            /**
             * 添加新帳單
             * @param:supplier(object) 廠商資料
             * @return: 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnPushBill = function (supplier, data, parentid) {
                var fnBill = function (billno, associated) {
                    var oNewBill = {};
                    oNewBill.guid = guid();
                    oNewBill.IsRetn = 'N';
                    oNewBill.parentid = parentid || '';
                    oNewBill.KeyName = 'Bill';
                    oNewBill.AuditVal = '0';
                    oNewBill.BillNO = billno;
                    oNewBill.BillCreateDate = newDate();
                    oNewBill.BillFirstCheckDate = '';
                    oNewBill.BillCheckDate = '';
                    oNewBill.Currency = 'NTD';
                    oNewBill.ExchangeRate = 1;
                    var sQuotationOrBillingCurrency = $('#QuotationOrBillingCurrency option:selected');
                    oNewBill.ExchangeRate = sQuotationOrBillingCurrency.attr('Correlation');
                    oNewBill.Currency = sQuotationOrBillingCurrency.val();
                    oNewBill.Advance = 0;
                    oNewBill.Memo = data.Quote.Memo || '';
                    oNewBill.FeeItems = associated.Fees.length === 0 ? clone(data.Quote.FeeItems) : associated.Fees;
                    oNewBill.InvoiceNumber = '';
                    oNewBill.InvoiceDate = '';
                    oNewBill.ReceiptNumber = '';
                    oNewBill.ReceiptDate = '';
                    oNewBill.SupplierGuid = supplier.guid;
                    oNewBill.Payer = supplier.SupplierID;
                    oNewBill.Number = associated.Base.Number || '';
                    oNewBill.Unit = associated.Base.Unit || '';
                    oNewBill.Weight = associated.Base.Weight || '';
                    oNewBill.Volume = associated.Base.Volume || '';
                    oNewBill.CustomerGuid = supplier.SupplierID;
                    oNewBill.CustomerCode = supplier.CustomerNO;
                    oNewBill.UniCode = supplier.UniCode;
                    oNewBill.SupplierName = supplier.SupplierName;
                    oNewBill.SupplierEName = supplier.SupplierEName;
                    oNewBill.Contactor = associated.Base.ContactorId || supplier.Contactor;
                    oNewBill.ContactorName = associated.Base.ContactorName || supplier.ContactorName;
                    oNewBill.Telephone = associated.Base.ContactTel || supplier.Telephone || '';
                    oNewBill.Email = supplier.Email;
                    oNewBill.ReFlow = '';
                    data.Bills.push(oNewBill);
                };

                return $.whenArray([
                    g_api.ConnectLite(Service.opm, 'GetBillAssociated', {
                        AppointNO: sAppointNO,
                        OtherId: sDataId,
                        SupplierID: supplier.SupplierID
                    }),
                    g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                        Type: parent.UserInfo.OrgID + 'O',
                        Flag: 'MinYear',
                        Len: 3,
                        Str: sServiceCode,
                        AddType: sServiceCode,
                        PlusType: ''
                    }, function (res) { },
                        function () {
                            showMsg(i18next.t('message.CreateBill_Failed'), 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                        })
                ]).done(function (res1, res2) {
                    if (res2[0].RESULT) {
                        var oAssociated = res1[0].DATA.rel;
                        fnBill(res2[0].DATA.rel, oAssociated);
                    }
                    else {
                        showMsg(i18next.t('message.CreateBill_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                    }
                });
            },
            /**
             * 初始化:報價/帳單幣別、匯率
             */
            fnInitialAccountCurrency = function (tabName) {
                $('#QuotationOrBillingCurrency').html(sAccountingCurrencyOptionsHtml).on('change', function () {
                    var sQuotationOrBillingCurrency = $('#QuotationOrBillingCurrency option:selected');
                    var sExchangeRate = sQuotationOrBillingCurrency.attr('Correlation');
                    var fExchangeRate = parseFloat(sExchangeRate);
                    let CurrencyID = this.value;
                    oCurData.Quote.QuotationOrBillingCurrency = CurrencyID;
                    oCurData.Quote.AccountingExchangeRate = sExchangeRate;
                    $('#AccountingExchangeRate').val(sExchangeRate);
                    bRequestStorage = true;//要變更儲存
                    //是主幣別(TE、TG:NTD；SG:CNY)，僅顯示主幣別資訊。
                    var MainCurrency = fnCheckMainOrForeignCurrency(CurrencyID);
                    var TitleAttr = '';
                    if (MainCurrency) {
                        TitleAttr = 'common.MainCurrencyNontaxedAmountV2';
                        $(tabName + ' .QuotationForeignCurrency').hide();
                    }
                    else {
                        TitleAttr = 'common.ForeignCurrencyNontaxedAmountV2';
                        $(tabName + ' .QuotationForeignCurrency').show();
                    }
                    $(tabName + ' .QuotationAmountTiltle').attr('data-i18n', TitleAttr).text(i18next.t(TitleAttr));
                    //重算
                    fnCalcuQuotationFee($('.QuotationForeignCurrency'), $('.QuotationMainCurrency'), CurrencyID, fExchangeRate);

                });
                $('#QuotationOrBillingCurrency').val(oCurData.Quote.QuotationOrBillingCurrency);
                $('#AccountingExchangeRate').val(oCurData.Quote.AccountingExchangeRate);
                $('#QuotationOrBillingCurrency').change();

            },
            /**
             * 綁定會計區塊
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBindFinancial = function () {
                fnBindFeeItem($('#tab3 [data-id="quote-box"]'), oCurData, oCurData.Quote);//綁定報價
                fnBindFeeItem($('#tab3 [data-id="estimatedcost-box"]'), oCurData, oCurData.EstimatedCost);//綁定預估成本
                if (oCurData.EstimatedCost.AuditVal && oCurData.EstimatedCost.AuditVal === '2') {//業務審核完
                    fnBindFeeItem($('#tab4 [data-id="actualcost-pre-box"]'), oCurData, oCurData.EstimatedCost, true);
                    $('#tab4 .estimatedcost-memo').text(oCurData.EstimatedCost.Memo || '');
                }
                fnBindFeeItem($('#tab4 [data-id="actualcost-box"]'), oCurData, oCurData.ActualCost);//實際成本
                fnInitialAccountCurrency('#tab3');//會計用匯率
                $('#tab3 [data-source="quote"]').text(oCurData.Quote.Memo || '');
                $('#tab3 [data-source="estimatedcost"]').text(oCurData.EstimatedCost.Memo || '');
                $('#tab4 [data-source="actualcost"]').text(oCurData.ActualCost.Memo || '');

                $('#tab3 .plusfeeitem,#tab4 .plusfeeitem').not('disabled').off('click').on('click', function () {
                    fnPlusFeeItem(this, oCurData);
                });

                $('#tab3 .importfeeitem').not('disabled').off('click').on('click', function () {
                    var sType = $(this).attr('data-type');
                    fnImportFeeitems(sType, $('#tab3'), oCurData);
                });
                $('#tab4 .importfeeitem').not('disabled').off('click').on('click', function () {
                    var sType = $(this).attr('data-type');
                    fnImportFeeitems(sType, $('#tab4'), oCurData);
                });

                $('#tab3 .copyfeeitem').not('disabled').off('click').on('click', function () {
                    var sType = $(this).attr('data-type');
                    fnCopyFeeitems(sType, $('#tab3'), oCurData);
                });

                $('#tab3 .plusfeeitemstar').not('disabled').off('click').on('click', function () {
                    fnPlusFeeItemStar(this, $('#tab3').find('[data-id="quote-box"]'), oCurData);
                });

                $('#tab3 #estimated_submitaudit').not('disabled').off('click').on('click', function () {
                    if (oCurData.EstimatedCost.FeeItems.length === 0 && !$('#EstimatedCost_Memo').val()) {
                        showMsg(i18next.t("message.EstimatedCostMemo_Required")); // ╠message.EstimatedCostMemo_Required⇒預估成本沒有費用明細，請至少填寫備註╣
                        return false;
                    }
                    if (!oCurData.Quote.QuotationOrBillingCurrency || !oCurData.Quote.AccountingExchangeRate) {
                        showMsg(i18next.t("message.QuotationOrBillingCurrencyField_Required")); // ╠message.QuotationOrBillingCurrencyField_Required⇒報價/帳單幣或匯率未選擇╣
                        return false;
                    }
                    oCurData.Quote.AuditVal = '1';
                    oCurData.EstimatedCost.AuditVal = '1';
                    g_api.ConnectLite(sProgramId, 'ToAuditForQuote', {
                        Guid: sDataId,
                        Quote: oCurData.Quote,
                        EstimatedCost: oCurData.EstimatedCost
                    }, function (res) {
                        if (res.RESULT) {
                            fnSetDisabled($('#tab3 .quoteandprecost'), oCurData.Quote, oCurData);
                            showMsg(i18next.t("message.ToAudit_Success"), 'success'); // ╠message.ToAudit_Success⇒提交審核成功╣
                            parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                        }
                        else {
                            oCurData.Quote.AuditVal = '0';
                            oCurData.EstimatedCost.AuditVal = '0';
                            showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                        }
                    }, function () {
                        oCurData.Quote.AuditVal = '0';
                        oCurData.EstimatedCost.AuditVal = '0';
                        showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                    });
                });
                $('#tab3 #estimated_audit').not('disabled').off('click').on('click', function () {
                    layer.open({
                        type: 1,
                        title: i18next.t('common.Leader_Audit'),// ╠common.Leader_Audit⇒主管審核╣
                        area: ['400px', '260px'],//寬度
                        shade: 0.75,//遮罩
                        shadeClose: true,
                        btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                        content: '<div class="pop-box">\
                                 <textarea name="NotPassReason" id="NotPassReason" style="min-width:300px;" class="form-control" rows="5" cols="20" placeholderid="common.NotPassReason" placeholder="不通過原因..."></textarea><br>\
                                 <button type="button" data-i18n="common.Pass" id="audit_pass" class="btn-custom green">通過</button>\
                                 <button type="button" data-i18n="common.NotPass" id="audit_notpass" class="btn-custom red">不通過</button>\
                              </div>',
                        success: function (layero, idx) {
                            $('.pop-box :button').click(function () {
                                var sNotPassReason = $('#NotPassReason').val();
                                if (this.id === 'audit_pass') {
                                    oCurData.Quote.AuditVal = '2';
                                    oCurData.EstimatedCost.AuditVal = '2';
                                    oCurData.Quote.NotPassReason = '';
                                    oCurData.EstimatedCost.NotPassReason = '';
                                }
                                else {
                                    if (!sNotPassReason) {
                                        showMsg(i18next.t("message.NotPassReason_Required")); // ╠message.NotPassReason_Required⇒請填寫不通過原因╣
                                        return false;
                                    }
                                    else {
                                        oCurData.Quote.AuditVal = '3';
                                        oCurData.EstimatedCost.AuditVal = '3';
                                        oCurData.Quote.NotPassReason = sNotPassReason;
                                        oCurData.EstimatedCost.NotPassReason = sNotPassReason;
                                    }
                                }

                                g_api.ConnectLite(sProgramId, 'AuditForQuote', {
                                    Guid: sDataId,
                                    Quote: oCurData.Quote,
                                    EstimatedCost: oCurData.EstimatedCost,
                                    Bills: oCurData.Bills
                                }, function (res) {
                                    if (res.RESULT) {
                                        showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                        if (oCurData.Quote.AuditVal === '2') {
                                            var oTable2 = $('#tab4 [data-id="actualcost-pre-box"]');
                                            fnBindFeeItem(oTable2, oCurData, oCurData.EstimatedCost, true);
                                            $('#tab4 .estimatedcost-memo').text(oCurData.EstimatedCost.Memo || '');
                                        }
                                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                                        fnSetDisabled($('#tab3 .quoteandprecost'), oCurData.Quote, oCurData);
                                    }
                                    else {
                                        oCurData.Quote.AuditVal = '1';
                                        oCurData.EstimatedCost.AuditVal = '1';
                                        showMsg(i18next.t('message.Audit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                    }
                                }, function () {
                                    oCurData.Quote.AuditVal = '1';
                                    oCurData.EstimatedCost.AuditVal = '1';
                                    showMsg(i18next.t('message.Audit_Failed'), 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                });
                                layer.close(idx);
                            });
                            transLang(layero);
                        }
                    });
                });
                $('#tab3 #estimated_synquote').not('disabled').off('click').on('click', function () {
                    oCurData.EstimatedCost.FeeItems = clone(oCurData.Quote.FeeItems);
                    $.each(oCurData.EstimatedCost.FeeItems, function (idx, item) {
                        item.FinancialUnitPrice = 0;
                        item.FinancialNumber = 0;
                        item.FinancialAmount = 0;
                        item.FinancialTWAmount = 0;
                        item.FinancialTaxRate = '0%';
                        item.FinancialTax = 0;
                        item.CreateUser = parent.UserID;
                        item.CreateDate = newDate(null, true);
                    });
                    fnBindFeeItem($('#tab3').find('[data-id="estimatedcost-box"]'), oCurData, oCurData.EstimatedCost);
                });

                fnBindBillLists();
            },
            /**
             * 獲取收據號碼
             * @param btn(object)產生收據號碼按鈕
             * @return bill(object)當前帳單資料
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnGetReceiptNumber = function (btn, bill) {
                return g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: 'SE',
                    Flag: 'MinYear',
                    Len: 6,
                    Str: '',
                    AddType: '',
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        var oBillBox = $(btn).parents('.bill-box-' + bill.BillNO);
                        bill.ReceiptNumber = res.DATA.rel;
                        bill.ReceiptDate = newDate(null, true);
                        oBillBox.find('[data-id="ReceiptNumber"]').val(bill.ReceiptNumber);
                        oBillBox.find('[data-id="ReceiptDate"]').val(bill.ReceiptDate);
                        $(btn).remove();
                    }
                    else {
                        showMsg(i18next.t('message.CreateReceiptNumber_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateReceiptNumber_Failed⇒收據號碼產生失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.CreateReceiptNumber_Failed'), 'error'); // ╠message.CreateReceiptNumber_Failed⇒收據號碼產生失敗╣
                });
            },
            /**
             * 帳單提交審核
             * @param bill(object)帳單資料
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBillToAudit = function (bill, el) {
                var sMsg = '',
                    elBillBox = $(el).parents('.financial');
                if (!bill.Currency) {
                    sMsg += i18next.t("ExhibitionImport_Upd.Currency_required") + '<br/>'; // ╠common.Currency_required⇒請輸入帳單幣別╣
                }
                if (!bill.Payer) {
                    sMsg += i18next.t("ExhibitionImport_Upd.Payer_required") + '<br/>'; // ╠ExhibitionImport_Upd.SupplierEamil_required⇒請輸入付款人╣
                }
                if (!bill.Number) {
                    sMsg += i18next.t("message.Number_required") + '<br/>'; // ╠message.Number_required⇒請輸入件數╣
                }
                if (!bill.Weight) {
                    sMsg += i18next.t("message.Weight_required") + '<br/>'; // ╠message.Weight_required⇒請輸入重量╣
                }
                if (!bill.Volume) {
                    sMsg += i18next.t("message.Volume_required") + '<br/>'; // ╠message.Volume_required⇒請輸入材積(CBM)╣
                }
                if (elBillBox.find('.jsgrid-update-button').length > 0) {
                    sMsg += i18next.t("message.DataEditing") + '<br/>'; // ╠message.DataEditing⇒該賬單處於編輯中╣
                }
                if (sMsg) {
                    showMsg(sMsg); // 必填欄位
                    return;
                }

                bill.AuditVal = '1';
                g_api.ConnectLite(sProgramId, 'ToAuditForBill', {
                    Guid: sDataId,
                    Exhibitors: saGridData,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill, oCurData);
                        showMsg(i18next.t("message.ToAudit_Success"), 'success'); // ╠message.ToAudit_Success⇒提交審核成功╣
                        parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '0';
                        showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                    }
                }, function () {
                    bill.AuditVal = '0';
                    showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                });
            },
            /**
             * 帳單審核
             * @param bill(object)帳單資料
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBillAudit = function (bill) {
                layer.open({
                    type: 1,
                    title: i18next.t('common.Leader_Audit'),// ╠common.Leader_Audit⇒主管審核╣
                    area: ['400px', '260px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                            <textarea name="NotPassReason" id="NotPassReason" style="min-width:300px;" class="form-control" rows="5" cols="20" placeholderid="common.NotPassReason" placeholder="不通過原因..."></textarea><br>\
                            <button type="button" data-i18n="common.Pass" id="audit_pass" class="btn-custom green">通過</button>\
                            <button type="button" data-i18n="common.NotPass" id="audit_notpass" class="btn-custom red">不通過</button>\
                          </div>',
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sNotPassReason = $('#NotPassReason').val();
                            if (this.id === 'audit_pass') {
                                bill.AuditVal = '2';
                                bill.NotPassReason = '';
                            }
                            else {
                                if (!sNotPassReason) {
                                    showMsg(i18next.t("message.NotPassReason_Required")); // ╠message.NotPassReason_Required⇒請填寫不通過原因╣
                                    return false;
                                }
                                else {
                                    bill.AuditVal = '3';
                                    bill.NotPassReason = sNotPassReason;
                                }
                            }
                            bill.BillCheckDate = newDate();
                            bill.CreateDate = newDate();
                            if (!bill.BillFirstCheckDate) {
                                bill.BillFirstCheckDate = bill.BillCheckDate;
                            }
                            g_api.ConnectLite(sProgramId, 'AuditForBill', {
                                Guid: sDataId,
                                Bills: oCurData.Bills,
                                Bill: bill
                            }, function (res) {
                                if (res.RESULT) {
                                    $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text(bill.BillCheckDate);
                                    fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill, oCurData);
                                    parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                                    showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                    if (bill.AuditVal === '2') {
                                        parent.msgs.server.pushTransfer(parent.OrgID, parent.UserID, bill.BillNO, 1);
                                    }
                                    fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                                }
                                else {
                                    bill.AuditVal = '1';
                                    if (bill.BillCheckDate === bill.BillFirstCheckDate) {
                                        bill.BillFirstCheckDate = '';
                                    }
                                    bill.BillCheckDate = '';
                                    showMsg(i18next.t('message.Audit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                }
                            }, function () {
                                bill.AuditVal = '1';
                                if (bill.BillCheckDate === bill.BillFirstCheckDate) {
                                    bill.BillFirstCheckDate = '';
                                }
                                bill.BillCheckDate = '';
                                showMsg(i18next.t('message.Audit_Failed'), 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                            });
                            layer.close(idx);
                        });
                        transLang(layero);
                    }
                });
            },
            /**
             * 會計取消審核
             * @param bill(object)帳單資料
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBillCancelAudit = function (bill) {
                var sBillCheckDate = bill.BillCheckDate;
                bill.AuditVal = '0';
                bill.BillCheckDate = '';
                g_api.ConnectLite(sProgramId, 'CancelAudit', {
                    Guid: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text('');
                        fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill, oCurData);
                        showMsg(i18next.t("message.CancelAudit_Success"), 'success'); // ╠message.CancelAudit_Success⇒取消審核完成╣
                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '2';
                        bill.BillCheckDate = sBillCheckDate;
                        showMsg(i18next.t('message.CancelAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CancelAudit_Failed⇒取消審核失敗╣
                    }
                }, function () {
                    bill.AuditVal = '2';
                    bill.BillCheckDate = sBillCheckDate;
                    showMsg(i18next.t('message.CancelAudit_Failed'), 'error'); // ╠message.CancelAudit_Failed⇒取消審核失敗╣
                });
            },
            /**
             * 列印事件
             * @param：templid (string) 模版id
             * @param：action (string) 動作標識
             * @param：bill (objec) 帳單資料
             * @return：
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnPrint = function (templid, action, bill) {
                var bReceipt = action.indexOf('Receipt') > -1,
                    fnToPrint = function (idx, paydatetext) {
                        g_api.ConnectLite(sProgramId, bReceipt ? 'PrintReceipt' : 'PrintBill', {
                            Guid: sDataId,
                            TemplID: templid,
                            Bill: bill,
                            Action: action,
                            PayDateText: paydatetext || ''
                        }, function (res) {
                            if (res.RESULT) {
                                if (idx) {
                                    layer.close(idx);
                                }
                                var sPath = res.DATA.rel,
                                    sTitle = bReceipt ? 'common.Receipt_Preview' : 'common.Bill_Preview';
                                if (action.indexOf('Print_') > -1) {
                                    var index = layer.open({
                                        type: 2,
                                        title: i18next.t(sTitle),
                                        content: gServerUrl + '/' + sPath,
                                        area: ['900px', '500px'],
                                        maxmin: true
                                    });
                                    //layer.full(index); //弹出即全屏
                                }
                                else {
                                    DownLoadFile(sPath);
                                }
                            }
                            else {
                                // ╠common.Preview_Failed⇒預覽失敗╣ ╠common.DownLoad_Failed⇒下載失敗╣
                                showMsg(i18next.t('common.Preview_Failed') + '<br>' + res.MSG, 'error');
                            }
                        }, function () {
                            // ╠common.Preview_Failed⇒預覽失敗╣ ╠common.DownLoad_Failed⇒下載失敗╣
                            showMsg(i18next.t('common.Preview_Failed'), 'error');
                        }, true, i18next.t('message.Dataprocessing'));// ╠message.Dataprocessing⇒資料處理中...╣
                    };
                if (bReceipt) {
                    fnToPrint();
                }
                else {
                    layer.open({
                        type: 1,
                        title: i18next.t('common.PayDatePopTitle'),// ╠common.PayDatePopTitle⇒付款日期設定╣
                        area: ['300px', '170px'],//寬度
                        shade: 0.75,//遮罩
                        shadeClose: true,
                        btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                        content: '<div class="pop-box">\
                                 <div class="col-sm-12" hidden>\
                                   <input id="radio_1" type="radio" name="printitem" value="a"/>\
                                   <label for="radio_1" data-i18n="common.PayDateText1">請立即安排付款</label>\
                                   <input id="radio_2" type="radio" name="printitem" value="b" />\
                                   <label for="radio_2" data-i18n="common.PayDateText2">a.s.a.p</label>\
                                   <input id="radio_3" type="radio" name="printitem" value="c" checked="checked"/>\
                                   <label for="radio_3" data-i18n="common.PayDateText3">日期</label>\
                                 </div>\
                                 <div class="col-sm-12 print-paydate">\
                                   <input name="PayDate" type="text" maxlength="100" id="PayDate" class="form-control date-picker w100p" />\
                                 </div>\
                              </div>',
                        success: function (layero, idx) {
                            var BillCheckDate = bill.BillCheckDate ? bill.BillCheckDate : new Date();
                            var sDefultDate = newDate(new Date(BillCheckDate).dateAdd('d', 15), true);
                            $('[name=printitem]').click(function () {
                                if (this.value === 'c') {
                                    $('.print-paydate').show();
                                }
                                else {
                                    $('.print-paydate').hide();
                                }
                            });
                            $('#PayDate').val(sDefultDate).datepicker({
                                changeYear: true,
                                changeMonth: true,
                                altFormat: 'yyyy/MM/dd',
                                onSelect: function (d, e) { },
                                afterInject: function (d, e) { }
                            });
                            transLang(layero);
                        },
                        yes: function (index, layero) {
                            var sPayDate = $('#PayDate').val(),
                                sPayDateType = $('[name=printitem]:checked').val(),
                                sPayDateText = $('[name=printitem]:checked').next().text();
                            if (sPayDateType === 'c' && !sPayDate) {
                                showMsg(i18next.t("message.PayDate_required")); // ╠message.PayDate_required⇒請選擇付款日期╣
                                return false;
                            }
                            fnToPrint(index, sPayDateType === 'c' ? sPayDate : sPayDateText);
                        }
                    });
                }
            },
            /**
            * 過帳
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillPost = function (bill) {
                bill.AuditVal = '5';
                bill.CreateDate = newDate();
                g_api.ConnectLite(sProgramId, 'BillPost', {
                    Guid: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.BillPost_Success"), 'success'); // ╠message.BillPost_Success⇒過帳完成╣
                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                        parent.msgs.server.pushTransfer(parent.OrgID, parent.UserID, bill.BillNO, 1);
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '2';
                        showMsg(i18next.t('message.BillPost_Failed') + '<br>' + res.MSG, 'error'); // ╠message.BillPost_Failed⇒過帳失敗╣
                    }
                }, function () {
                    bill.AuditVal = '2';
                    showMsg(i18next.t('message.BillPost_Failed'), 'error'); // ╠message.BillPost_Failed⇒過帳失敗╣
                });
            },
            /**
            * 取消過帳
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillCancelPost = function (bill) {
                bill.AuditVal = '2';
                g_api.ConnectLite(sProgramId, 'BillCancelPost', {
                    Guid: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.BillCancePost_Success"), 'success'); // ╠message.BillCancePost_Success⇒取消過帳完成╣
                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '5';
                        showMsg(i18next.t('message.BillCancePost_Failed') + '<br>' + res.MSG, 'error'); // ╠message.BillCancePost_Failed⇒取消過帳失敗╣
                    }
                }, function () {
                    bill.AuditVal = '5';
                    showMsg(i18next.t('message.BillCancePost_Failed'), 'error'); // ╠message.BillCancePost_Failed⇒取消過帳失敗╣
                });
            },
            /**
             * 會計銷帳
             * @param bill(object)帳單資料
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBillWriteOff = function (bill) {
                bill.AuditVal = '4';
                bill.BillWriteOffDate = newDate();
                g_api.ConnectLite(sProgramId, 'WriteOff', {
                    Guid: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.BillWriteOff_Success"), 'success'); // ╠message.BillWriteOff_Success⇒銷帳完成╣
                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '5';
                        showMsg(i18next.t('message.BillWriteOff_Failed') + '<br>' + res.MSG, 'error'); // ╠message.BillWriteOff_Failed⇒銷帳失敗╣
                    }
                }, function () {
                    bill.AuditVal = '5';
                    showMsg(i18next.t('message.BillWriteOff_Failed'), 'error'); // ╠message.BillWriteOff_Failed⇒銷帳失敗╣
                });
            },
            /**
            * 會計取消銷帳
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillCancelWriteOff = function (bill) {
                bill.AuditVal = '5';
                bill.BillWriteOffDate = '';
                g_api.ConnectLite(sProgramId, 'CancelWriteOff', {
                    Guid: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.BillCancelWriteOff_Success"), 'success'); // ╠message.BillCancelWriteOff_Success⇒取消銷帳完成╣
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '4';
                        showMsg(i18next.t('message.BillCancelWriteOff_Failed') + '<br>' + res.MSG, 'error'); // ╠message.BillCancelWriteOff_Failed⇒取消銷帳失敗╣
                    }
                }, function () {
                    bill.AuditVal = '4';
                    showMsg(i18next.t('message.BillCancelWriteOff_Failed'), 'error'); // ╠message.BillCancelWriteOff_Failed⇒取消銷帳失敗╣
                });
            },
            /**
            * 帳單作廢
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillVoid = function (bill) {
                layer.open({
                    type: 1,
                    title: i18next.t('common.VoidBill'),// ╠common.VoidBill⇒作廢帳單╣
                    area: ['400px', '220px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                             <textarea name="VoidReason" id="VoidReason" style="min-width:300px;" class="form-control" rows="5" cols="20" placeholderid="common.VoidReason" placeholder="作廢原因..."></textarea>\
                          </div>',
                    success: function (layero, idx) {
                        transLang(layero);
                    },
                    yes: function (index, layero) {
                        var sAuditVal = bill.AuditVal,
                            sVoidReason = layero.find('#VoidReason').val();
                        if (sVoidReason) {
                            bill.VoidReason = sVoidReason;
                            bill.AuditVal = '6';
                            g_api.ConnectLite(sProgramId, 'BillVoid', {
                                Guid: sDataId,
                                Bills: oCurData.Bills,
                                Bill: bill,
                                LogData: fnGetBillLogData(bill)
                            }, function (res) {
                                if (res.RESULT) {
                                    layer.close(index);
                                    showMsg(i18next.t("message.Void_Success"), 'success'); // ╠message.Void_Success⇒作廢成功╣
                                    fnBindBillLists();
                                    parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                                    fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                                }
                                else {
                                    bill.AuditVal = sAuditVal;
                                    bill.VoidReason = '';
                                    showMsg(i18next.t('message.Void_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Void_Failed⇒作廢失敗╣
                                }
                            }, function () {
                                bill.AuditVal = sAuditVal;
                                bill.VoidReason = '';
                                showMsg(i18next.t('message.Void_Failed'), 'error'); // ╠message.Void_Failed⇒作廢失敗╣
                            });
                        }
                        else {
                            showMsg(i18next.t("message.VoidReason_Required")); // ╠message.VoidReason_Required⇒請填寫作廢原因╣
                        }
                    }
                });
            },
            /**
            * 帳單刪除
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillDelete = function (bill) {
                // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
                layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                    var saNewBills = [];
                    $.each(oCurData.Bills, function (idx, _bill) {
                        if (_bill.BillNO !== bill.BillNO) {
                            saNewBills.push(_bill);
                        }
                    });
                    g_api.ConnectLite(sProgramId, 'BillDelete', {
                        Guid: sDataId,
                        Bills: saNewBills,
                        Bill: bill,
                        LogData: fnGetBillLogData(bill)
                    }, function (res) {
                        if (res.RESULT) {
                            showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                            oCurData.Bills = saNewBills;
                            fnBindBillLists();
                            parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                            fnDeleteBillInfo(bill.BillNO);
                        }
                        else {
                            showMsg(i18next.t("message.Delete_Failed") + '<br>' + res.MSG, 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                        }
                    }, function () {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    });
                    layer.close(index);
                });
            },
            /**
            * 抽單
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillReEdit = function (bill) {
                bill.AuditVal = '7';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        otherexhibitiontg: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.ReEdit_Success"), 'success'); // ╠message.ReEdit_Success⇒抽單成功╣
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '1';
                        showMsg(i18next.t('message.ReEdit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ReEdit_Failed⇒抽單失敗╣
                    }
                }, function () {
                    bill.AuditVal = '1';
                    showMsg(i18next.t('message.ReEdit_Failed'), 'error'); // ╠message.ReEdit_Failed⇒抽單失敗╣
                });
            },
            /**
            * 取消抽單
            * @param bill(object)帳單資料
            * @return 無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnBillCancelReEdit = function (bill) {
                bill.AuditVal = '1';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        otherexhibitiontg: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill, oCurData);
                        showMsg(i18next.t("message.CancelReEdit_Success"), 'success'); // ╠message.CancelReEdit_Success⇒取消抽單成功╣
                        fnUpdateBillInfo(sProgramId, sDataId, bill.BillNO);
                    }
                    else {
                        bill.AuditVal = '7';
                        showMsg(i18next.t('message.CancelReEdit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CancelReEdit_Failed⇒取消抽單失敗╣
                    }
                }, function () {
                    bill.AuditVal = '7';
                    showMsg(i18next.t('message.CancelReEdit_Failed'), 'error'); // ╠message.CancelReEdit_Failed⇒取消抽單失敗╣
                });
            },
            /**
             * 綁定帳單
             * @param 無
             * @return 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnBindBillLists = function () {
                var oBillsBox = $('#accordion');
                if (oCurData.Bills.length > 0) {//實際帳單
                    oCurData.Bills = Enumerable.From(oCurData.Bills).OrderBy("x=>x.BillCreateDate").ToArray();
                    $.each(oCurData.Bills, function (idx, bill) {
                        if ($('.bill-box-' + bill.BillNO).length === 0) {
                            bill.Index = idx + 1;
                            bill.Advance = bill.Advance || 0;
                            var sHtml = $("#temp_billbox").render([bill]);
                            oBillsBox.append(sHtml);
                            var oBillBox = $('.bill-box-' + bill.BillNO);
                            $('.bills-box').show();
                            oBillBox.find('[data-id="Currency"]').html(sAccountingCurrencyOptionsHtml).on('change', function () {
                                var sCurrencyId = this.value,
                                    oCurrency = Enumerable.From(saAccountingCurrency).Where(function (e) { return e.ArgumentID == sCurrencyId; }).FirstOrDefault();
                                if (oCurrency === undefined)
                                    oCurrency = {};
                                let TitleAttr = '';
                                let MainCurrency = fnCheckMainOrForeignCurrency(sCurrencyId);
                                if (MainCurrency) {
                                    TitleAttr = 'common.MainCurrencyNontaxedAmountV2';
                                    oBillBox.find('.BillMainCurrencyAdd [data-id="plusfeeitem"]').show();
                                    oBillBox.find('.BillForeignCurrency').hide();
                                    oGlobalItem.parentdata.Quote.QuotationOrBillingCurrency = sCurrencyId;
                                    fnBindFeeItem(oGlobalItem.dom, oGlobalItem.parentdata, oGlobalItem.data);
                                }
                                else {
                                    TitleAttr = 'common.ForeignCurrencyNontaxedAmountV2';
                                    oBillBox.find('.BillMainCurrencyAdd [data-id="plusfeeitem"]').hide();
                                    oBillBox.find('.BillForeignCurrency').show();
                                    oGlobalItem.parentdata.Quote.QuotationOrBillingCurrency = sCurrencyId;
                                    fnBindFeeItem(oGlobalItem.dom, oGlobalItem.parentdata, oGlobalItem.data);
                                }
                                bill.Currency = sCurrencyId;
                                bill.ExchangeRate = oCurrency.Correlation || '';
                                oBillBox.find('[data-id="ExchangeRate"]').val(oCurrency.Correlation || '');
                                oBillBox.find('.BillAmountTiltle').attr('data-i18n', TitleAttr).text(i18next.t(TitleAttr));
                                let ExchangeRate = oBillBox.find('[data-id="ExchangeRate"]').val();
                                fnCalcuBillsFee(oBillBox, '.BillForeignCurrency', '.BillMainCurrency', sCurrencyId, ExchangeRate);
                                bRequestStorage = true;
                            }).val(bill.Currency);
                            fnBindFeeItem($('[data-id=bill_fees_' + bill.BillNO + ']'), oCurData, bill);
                            //觸發點擊項目 產生資料內容
                            oBillBox.click(function () {
                                if ($(this).attr('aria-expanded') === 'false') {
                                    oBillBox.find('[data-id="Payer"]').html(sCustomersNotAuditOptionsHtml).val(bill.Payer);
                                    setTimeout(function () {
                                        oBillBox.find('[data-id="Payer"]').select2({ width: '250px' });
                                    }, 1000);

                                    oBillBox.find('[data-id="Currency"]').change(); //花費2秒
                                    //oBillBox.find('[data-id="Currency"] option:first').remove();
                                    if (bill.Payer) {
                                        var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === bill.Payer; }),
                                            saContactors = [];
                                        if (oCur.Count() > 0) {
                                            saContactors = JSON.parse(oCur.First().Contactors || '[]');
                                        }
                                        oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).val(bill.Contactor).off('change').on('change', function (e) {
                                            var sContactor = this.value;
                                            if (sContactor) {
                                                var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid == sContactor; }).First();
												bill.ContactorName = oContactor.FullName;
												bill.Contactor = sContactor;
                                            }
                                            else {
                                                bill.ContactorName = '';
                                                bill.Contactor = '';
                                            }
                                            bRequestStorage = true;
                                        });;
                                        oBillBox.find('[data-id="Contactor"]').select2();
                                    }
                                    else {
                                        oBillBox.find('[data-id="Contactor"]').html(createOptions([]));
                                    }
                                    oBillBox.find('[data-id="Telephone"]').val(bill.Telephone);
                                    oBillBox.find('[data-id="ExchangeRate"]').val(bill.ExchangeRate || 1.00);
                                    oBillBox.find('[data-id="Number"]').val(bill.Number).on('keyup blur', function (e) {
                                        this.value = this.value.replace(/\D/g, '');
                                    });
                                    oBillBox.find('[data-id="Unit"]').val(bill.Unit);
                                    moneyInput(oBillBox.find('[data-id="Advance"]'), 2);
                                    moneyInput(oBillBox.find('[data-id="mAdvance"]'), 0);
                                    oBillBox.find('[data-id="Advance"]').val(bill.Advance);
                                    SetBillPrepayEvent(oBillBox, bill);
                                    oBillBox.find('[data-id="Weight"]').val(bill.Weight).on('keyup blur', function (e) {
                                        keyIntp(e, this, 3);
                                    });
                                    oBillBox.find('[data-id="Volume"]').val(bill.Volume).on('keyup blur', function (e) {
                                        keyIntp(e, this, 2);
                                    });
                                    if (bill.ReceiptNumber) {//如果收據號碼已經產生就移除該按鈕
                                        oBillBox.find('[data-id="ReceiptNumber"]').val(bill.ReceiptNumber);
                                        oBillBox.find('[data-id="ReceiptDate"]').val(bill.ReceiptDate);
                                        oBillBox.find('[data-id="createreceiptnumber"]').remove();
                                    }
                                    oBillBox.find('[data-id="Memo"]').val(bill.Memo);
                                    oBillBox.find('[data-id="InvoiceNumber"]').val(bill.InvoiceNumber).on('blur', function (e) {
                                        var that = this,
                                            sInvoiceNumber = that.value;
                                        if (sInvoiceNumber) {
                                            return g_api.ConnectLite(Service.opm, ComFn.CheckInvoiceNum, {
                                                InvoiceNumber: sInvoiceNumber
                                            }, function (res) {
                                                if (res.RESULT) {
                                                    var bExsit = res.DATA.rel;
                                                    if (bExsit) {
                                                        // ╠message.InvoiceNumberRepeat⇒發票號碼重複，請重新輸入！╣ ╠message.Tips⇒提示╣
                                                        layer.alert(i18next.t("message.InvoiceNumberRepeat"), { icon: 0, title: i18next.t("common.Tips") }, function (index) {
                                                            $(that).val('');
                                                            layer.close(index);
                                                        });
                                                    }
                                                }
                                            });
                                        }
                                    });
                                    oBillBox.find('[data-id="InvoiceDate"]').val(bill.InvoiceDate);
                                    oBillBox.find('.date-picker').datepicker({
                                        changeYear: true,
                                        changeMonth: true,
                                        altFormat: 'yyyy/MM/dd'
                                    });

                                    oBillBox.find('[data-id="Payer"]').on('change', function () {
                                        var sCustomerId = this.value,
                                            saContactors = [];
                                        if (sCustomerId) {
                                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sCustomerId; }).First();
                                            saContactors = JSON.parse(oCur.Contactors || '[]');
                                            //bill.CustomerCode = oCur.CustomerNO;
                                            oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).off('change').on('change', function () {
                                                var sContactor = this.value;
                                                if (sContactor) {
                                                    var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid == sContactor; }).First();
													bill.ContactorName = oContactor.FullName;
													bill.Contactor = sContactor;
                                                }
                                                else {
                                                    bill.ContactorName = '';
                                                    bill.Contactor = '';
                                                }
                                                bRequestStorage = true;
                                            });
                                            oBillBox.find('[data-id="Telephone"]').val(oCur.Telephone);
                                            oBillBox.find('[data-id="Contactor"]').select2();
                                        }
                                        else {
                                            oBillBox.find('[data-id="Contactor"]').html(createOptions([]));
                                            oBillBox.find('[data-id="Telephone"]').val('');
                                        }
                                        oBillBox.find('[data-id="Telephone"]').change();
                                        bRequestStorage = true;
                                    });

                                    oBillBox.find('.bill-print').each(function () {
                                        var that = this,
                                            sTypeId = $(that).attr('data-action'),
                                            oToolBar = null;
                                        switch (sTypeId) {
                                            case 'Print_Bill':
                                                oToolBar = oPrintMenu.InvoicePrintMenu.tmpl
                                                break;
                                            case 'Print_Receipt':
                                                oToolBar = oPrintMenu.ReceiptPrintMenu.tmpl
                                                break;
                                            case 'Download_Bill':
                                                oToolBar = oPrintMenu.InvoiceDownLoadMenu.tmpl
                                                break;
                                            case 'Download_Receipt':
                                                oToolBar = oPrintMenu.ReceiptDownLoadMenu.tmpl
                                                break;
                                        }
                                        $(that).toolbar({
                                            content: oToolBar,
                                            position: 'left',
                                            adjustment: 10,
                                            style: 'yida',
                                        }).on('toolbarItemClick',
                                            function (e, ele) {
                                                fnPrint($(ele).attr('data-id'), sTypeId, bill);
                                            }
                                        );
                                    });
                                    oBillBox.find('.btn-custom').not('disabled').off('click').on('click', function () {
                                        var that = this,
                                            sId = $(that).attr('data-id');
                                        switch (sId) {
                                            case 'plusfeeitem'://新增費用項目
                                                fnPlusFeeItem(that, oCurData, null, bill.Currency);
                                                break;
                                            case 'createreceiptnumber'://產生收據號碼
                                                fnGetReceiptNumber(that, bill).done(function () {
                                                    CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                                        Params: {
                                                            otherexhibitiontg: {
                                                                values: { Bills: JSON.stringify(oCurData.Bills) },
                                                                keys: { Guid: sDataId }
                                                            }
                                                        }
                                                    });
                                                });
                                                break;
                                            case 'bill_submitaudit'://提交審核
                                                fnBillToAudit(bill, that);
                                                break;
                                            case 'bill_audit'://主管審核
                                                fnBillAudit(bill);
                                                break;
                                            case 'bill_cancelaudit'://取消審核
                                                fnBillCancelAudit(bill);
                                                break;
                                            case 'bill_post'://過帳
                                                fnBillPost(bill);
                                                break;
                                            case 'bill_cancelpost'://取消過帳
                                                fnBillCancelPost(bill);
                                                break;
                                            case 'bill_writeoff'://銷帳
                                                fnBillWriteOff(bill);
                                                break;
                                            case 'bill_canceloff'://銷帳
                                                fnBillCancelWriteOff(bill);
                                                break;
                                            case 'bill_void'://作廢
                                                fnBillVoid(bill);
                                                break;
                                            case 'bill_delete'://刪除
                                                fnBillDelete(bill);
                                                break;
                                            case 'bill_reedit'://抽單
                                                fnBillReEdit(bill);
                                                break;
                                            case 'bill_cancelreedit'://取消抽單
                                                fnBillCancelReEdit(bill);
                                                break;
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
                //fnSetPermissions();//設置權限
                $('#tab3').css({ 'padding-top': 40 });
                $('#topshow_box').show();//當審通過之後才顯示總金額
                transLang(oBillsBox);
            },
            /**
             * 設置操作權限
             * @param： 無
             * @return： 無
             * 起始作者：John
             * 起始日期：2017/01/05
             * 最新修改人：John
             * 最新修日期：2017/01/05
             */
            fnSetPermissions = function () {
                if (parent.UserInfo.roles.indexOf('Admin') === -1) {
                    if ((parent.UserInfo.roles.indexOf('CDD') > -1 && (oCurData.ResponsiblePerson === parent.UserID || oCurData.DepartmentID === parent.UserInfo.DepartmentID)) || parent.UserInfo.roles.indexOf('CDD') === -1 || parent.SysSet.CDDProUsers.indexOf(parent.UserID) > -1) {//報關作業
                        $('[href="#tab3"],[href="#tab4"]').parent().show();
                    }
                    else {
                        $('[href="#tab3"],[href="#tab4"]').parent().hide();
                    }
                    if (!(parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser)) {//其他
                        $('#tab3,#tab4').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.estimated_addreturnbills,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost').attr('disabled', 'disabled');
                        $('#tab3,#tab4').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Business') > -1) {//業務
                        $('#tab4,#tab6').find(':input,button,textarea').attr('disabled', 'disabled');
                        $('#tab4,#tab6').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                        $('#tab1,#tab3,#tab4').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.jsgrid-button,.estimated_addreturnbills,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost,.importfeeitem,.plusfeeitem').attr('disabled', 'disabled');
                        $('#tab3,#tab4').find('.icon-p').addClass('disabled');
                    }
                }
            },
            /**------------------------帳單部分---------------------------End*/
            /**
            * Grid初始化
            * @param：無
            * @return：無
            * 起始作者：John
            * 起始日期：2017/01/05
            * 最新修改人：John
            * 最新修日期：2017/01/05
            */
            fnGridInit = function () {
                var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 120;
                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    filtering: true,
                    pageLoading: true,
                    inserting: true,
                    editing: true,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.SysSet.GridRecords || 10,
                    invalidMessage: i18next.t('common.InvalidData'),// ╠common.InvalidData⇒输入的数据无效！╣
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    rowClass: function (item) {
                        if (item.VoidContent) {
                            return 'data-void';
                        }
                    },
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center", sorting: false,
                            itemTemplate: function (val, item) {
                                var sVal = val || '';
                                if (item.VoidContent) {// ╠common.Toolbar_Void⇒作廢╣
                                    sVal = '<span class="tooltips" title="' + i18next.t('common.Toolbar_Void') + '：' + item.VoidContent + '">' + sVal + '</span>';
                                }
                                return sVal;
                            }
                        },
                        {
                            name: "SupplierName", title: 'common.SupplierName', width: 150, filtering: true, inserting: true, editing: false, validate: { validator: 'required', message: i18next.t('common.Supplier_required') },
                            insertTemplate: function () {
                                var oSelect = $('<select/>', {
                                    change: function () {
                                        var sCustomerId = this.value;
                                        if (sCustomerId) {
                                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sCustomerId; }).First();
                                            oAddItem.guid = guid();
                                            oAddItem.SupplierID = oCur.id;
                                            oAddItem.CustomerNO = oCur.CusNO;
                                            oAddItem.UniCode = oCur.UniCode;
                                            oAddItem.SupplierName = oCur.textcn;
                                            oAddItem.SupplierEName = oCur.texteg;
                                            oAddItem.Telephone = oCur.Telephone;
                                            oAddItem.Email = oCur.Email;
                                            oAddItem.ContactorName = '';
                                            oAddItem.CreateUser = parent.UserID;
                                            oAddItem.CreateDate = new Date().formate("yyyy/MM/dd HH:mm:ss");
                                            oSelect.parent().next().find(':input').val(oAddItem.SupplierEName);
                                            var saContactors = JSON.parse(oCur.Contactors || '[]');
                                            oSelect.parent().next().next().find('select').html(createOptions(saContactors, 'guid', 'FullName')).on('change', function () {
                                                var sContactor = this.value;
                                                if (sContactor) {
                                                    var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid === sContactor; }).First();
                                                    $(this).parent().next().find(':input').val(oContactor.TEL1);
                                                    $(this).parent().next().next().find(':input').val(oContactor.Email);
                                                    oAddItem.Contactor = sContactor;
                                                    oAddItem.ContactorName = oContactor.FullName;
                                                    oAddItem.Telephone = oContactor.TEL1;
                                                    oAddItem.Email = oContactor.Email;
                                                }
                                                else {
                                                    oAddItem.Contactor = '';
                                                    oAddItem.ContactorName = '';
                                                    oAddItem.Telephone = '';
                                                    oAddItem.Email = '';
                                                    $(this).parent().next().find(':input').val('');
                                                    $(this).parent().next().next().find(':input').val('');
                                                }
                                            });
                                            oSelect.parent().next().next().next().find(':input').val(oCur.Telephone);
                                            oSelect.parent().next().next().next().next().find(':input').val(oCur.Email);
                                        }
                                        else {
                                            oSelect.parent().next().next().find('select').html(createOptions([]));
                                            oAddItem = {};
                                        }
                                        bRequestStorage = true;
                                    }
                                });
                                setTimeout(function () {
                                    oSelect.html(sCustomersNotAuditOptionsHtml).select2({ width: '180px' });
                                }, 1000);
                                return this.insertControl = oSelect;
                            },
                            insertValue: function () {
                                return this.insertControl.val();
                            },
                            filterTemplate: function () {
                                var oSelect = $('<select/>', {
                                    change: function () {
                                        var sCustomerId = this.value;
                                        if (sCustomerId) {
                                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sCustomerId; }).First();
                                            var saContactors = JSON.parse(oCur.Contactors || '[]');
                                            oSelect.parent().next().next().find('select').html(createOptions(saContactors, 'guid', 'FullName'))
                                        }
                                        else {
                                            oSelect.parent().next().next().find('select').html(createOptions([]));
                                        }
                                        bRequestStorage = true;
                                    }
                                });
                                setTimeout(function () {
                                    oSelect.html(sCustomersNotAuditOptionsHtml).select2({ width: '180px' });
                                }, 1000);
                                return this.filterControl = oSelect;
                            },
                            filterValue: function () {
                                return this.filterControl.val() === '' ? '' : this.filterControl.find("option:selected").text();
                            }
                        },
                        {
                            name: "SupplierEName", title: 'common.SupplierEName', width: 150, filtering: true, inserting: true, editing: false,
                            insertTemplate: function () {
                                return this.insertControl = $('<input/>', { type: 'text', class: 'form-control w100p', disabled: true });
                            }, insertValue: function () {
                                return this.insertControl.val();
                            }, filterTemplate: function () {
                                return this.filterControl = $('<input/>', { type: 'text' });
                            }, filterValue: function () {
                                return this.filterControl.val();
                            }
                        },
                        {
                            name: "Contactor", title: 'common.Contactor', type: "text", filtering: true, width: 100,
                            itemTemplate: function (val, item) {
                                return item.ContactorName;
                            },
                            insertTemplate: function () {
                                return this.insertControl = $('<select/>', {
                                    html: createOptions([])
                                });
                            }, insertValue: function () {
                                return this.insertControl.val();
                            },
                            editTemplate: function (val, item) {
                                var saContactors = [];
                                if (item.SupplierID) {
                                    var saCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === item.SupplierID; }).ToArray();
                                    if (saCur.length > 0) {
                                        saContactors = JSON.parse(saCur[0].Contactors || '[]');
                                    }
                                }
                                return this.editControl = $('<select/>', {
                                    html: createOptions(saContactors, 'guid', 'FullName'),
                                    change: function () {
                                        var sContactor = this.value;
                                        if (sContactor) {
                                            var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid === sContactor; }).First();
                                            $(this).parent().next().find(':input').val(oContactor.TEL1);
                                            $(this).parent().next().next().find(':input').val(oContactor.Email);
                                            item.Contactor = sContactor;
                                            item.ContactorName = oContactor.FullName;
                                            item.Telephone = oContactor.TEL1;
                                            item.Email = oContactor.Email;
                                        }
                                        else {
                                            item.Contactor = '';
                                            item.ContactorName = '';
                                            item.Telephone = '';
                                            item.Email = '';
                                            $(this).parent().next().find(':input').val('');
                                            $(this).parent().next().next().find(':input').val('');
                                        }
                                        bRequestStorage = true;
                                    }
                                }).val(val);
                            },
                            editValue: function () {
                                return this.editControl.val();
                            }
                        },
                        { name: "Telephone", title: 'common.Telephone', type: "text", filtering: true, width: 100 },
                        { name: "Email", title: 'common.Email', type: "text", filtering: true, width: 130 },
                        {
                            title: 'common.Other', width: 200,
                            itemTemplate: function (val, item) {
                                var oDom = [],
                                    oVoid = $('<a/>', {// ╠common.Toolbar_OpenVoid⇒啟用╣  ╠common.Toolbar_Void⇒作廢╣
                                        html: item.VoidContent !== undefined ? i18next.t('common.Toolbar_OpenVoid') : i18next.t('common.Toolbar_Void'),
                                        class: 'a-url',
                                        click: function () {
                                            var that = this;
                                            if (item.VoidContent) {
                                                delete item.VoidContent;
                                                $(that).text(i18next.t('common.Toolbar_Void'));// ╠common.Toolbar_Void⇒作廢╣
                                                $(that).parents('tr').removeClass('data-void').removeAttr('title');
                                            }
                                            else {
                                                layer.open({
                                                    type: 1,
                                                    title: i18next.t('common.Toolbar_Void'),// ╠common.Toolbar_Void⇒作廢╣
                                                    shade: 0.75,
                                                    maxmin: true, //开启最大化最小化按钮
                                                    area: ['500px', '250px'],
                                                    content: '<div class="pop-box">\
                                                               <textarea name="VoidContent" id="VoidContent" style="min-width:300px;" class="form-control" rows="5" cols="20"></textarea>\
                                                          </div>',
                                                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                                                    success: function (layero, index) {
                                                    },
                                                    yes: function (index, layero) {
                                                        var sVoidContent = $('#VoidContent').val();
                                                        item.VoidContent = sVoidContent;
                                                        if (!sVoidContent) {
                                                            showMsg(i18next.t("message.VoidReason_Required")); // ╠message.VoidReason_Required⇒請填寫作廢原因╣
                                                            return false;
                                                        }
                                                        $(that).text(i18next.t('common.Toolbar_OpenVoid'));// ╠common.Toolbar_OpenVoid⇒啟用╣
                                                        $(that).parents('tr').addClass('data-void').attr('title', i18next.t('common.Toolbar_Void') + '：' + sVoidContent);// ╠common.Toolbar_Void⇒作廢╣
                                                        layer.close(index);
                                                    }
                                                });
                                            }
                                            return false;
                                        }
                                    }),
                                    oCreateBill = $('<a/>', {
                                        html: i18next.t('common.CreateBill'),// ╠common.CreateBill⇒建立帳單╣
                                        class: 'a-url',
                                        click: function () {
                                            var that = this;
                                            fnPushBill(item, oCurData, item.guid).done(function () {
                                                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                                    Params: {
                                                        otherexhibitiontg: {
                                                            values: {
                                                                Bills: JSON.stringify(oCurData.Bills),
                                                                Exhibitors: JSON.stringify(saGridData)
                                                            },
                                                            keys: { Guid: sDataId }
                                                        }
                                                    }
                                                }, function (res) {
                                                    if (res.d > 0) {
                                                        fnBindBillLists();
                                                        $(that).remove();
                                                        showMsg(i18next.t("common.Create_Success"), 'success'); // ╠common.Create_Success⇒創建成功╣
                                                    }
                                                    else {
                                                        showMsg(i18next.t("common.Create_Failed"), 'error'); // ╠common.Create_Failed⇒創建失敗╣
                                                    }
                                                });
                                            });
                                            return false;
                                        }
                                    });
                                if ((item.VoidContent && parent.UserInfo.IsManager) || !item.VoidContent) {
                                    oDom.push(oVoid);
                                }
                                if (oCurData.Quote.AuditVal === '2' && !item.VoidContent) {
                                    var iExsitVoid = Enumerable.From(oCurData.Bills).Where(function (e) { return (e.parentid === item.guid && e.VoidReason); }).Count();
                                    var iExsit = Enumerable.From(oCurData.Bills).Where(function (e) { return e.parentid === item.guid; }).Count();
                                    if (iExsitVoid > 0 || iExsit === 0) {
                                        oDom.push(oCreateBill);
                                    }
                                }
                                return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(oDom);
                            }
                        },
                        { type: "control" }
                    ],
                    controller: {
                        loadData: function (args) {
							$.each(saNewCustomers, function (idx, data) {
								oAddItem = {};
								oAddItem.guid = guid();
								oAddItem.SupplierID = data.guid;
								oAddItem.CustomerNO = data.CustomerNO;
								oAddItem.UniCode = data.UniCode;
								oAddItem.SupplierName = data.CustomerCName;
								oAddItem.SupplierEName = data.CustomerEName;
								oAddItem.Telephone = data.Telephone;
								oAddItem.Email = data.Email;
								oAddItem.Contactor = data.ContactorId;
								oAddItem.ContactorName = data.ContactorName;
								oAddItem.CreateUser = parent.UserID;
								oAddItem.CreateDate = new Date().formate("yyyy/MM/dd HH:mm:ss");
								
								saGridData.push(oAddItem);
							});
							
                            if (args.Contactor !== undefined) {
                                var filters = $.grep(saGridData, function (client) {
                                    return (!args.Contactor || client.ContactorName.indexOf(args.Contactor) > -1)
                                        && (!args.Telephone || client.Telephone.indexOf(args.Telephone) > -1)
                                        && (!args.Email || client.Email.indexOf(args.Email) > -1)
                                        && (!args.SupplierEName || client.SupplierEName.indexOf(args.SupplierEName) > -1)
                                        && (!args.SupplierName || args.SupplierName.indexOf(client.SupplierName) > -1);
                                });
                                if (filters.length > 0) {
                                    filters = Enumerable.From(filters).OrderBy("x=>x.CreateDate").ToArray();
                                }
                                $.each(filters, function (idx, filter) {
                                    filter.RowIndex = idx + 1;
                                });
                                return {
                                    data: filters,
                                    itemsCount: filters.length //data.length
                                };
                            }
                            else {
                                if (saGridData.length > 0) {
                                    saGridData = Enumerable.From(saGridData).OrderBy("x=>x.CreateDate").ToArray();
                                }
                                $.each(saGridData, function (idx, filter) {
                                    filter.RowIndex = idx + 1;
                                });
                                return {
                                    data: saGridData,
                                    itemsCount: saGridData.length //data.length
                                };
                            }
                        },
                        insertItem: function (args) {
                            oAddItem.Contactor = args.Contactor;
                            oAddItem.Telephone = args.Telephone;
                            oAddItem.Email = args.Email;
                            saGridData.push(oAddItem);
                            oAddItem = {};
                            if (sAction === 'Add') {
                                showMsg(i18next.t("message.SaveCusFirst"));// ╠message.SaveCusFirst⇒請先儲存再新增廠商╣
                                saGridData = [];
                                return $.Deferred().reject().promise();
                            }
                            else {
                                bRequestStorage = true;
                            }
                        },
                        updateItem: function (args) {
                            $.each(saGridData, function (e, item) {
                                if (item.guid === args.guid) {
                                    item = args;
                                    bRequestStorage = true;
                                    return false;
                                }
                            });
                        },
                        deleteItem: function (args) {
                            if (args.BillNO) {
                                showMsg(i18next.t("message.NotToDelete_Supplier"));// ╠message.NotToDelete_Supplier⇒該筆廠商已經產生帳單或寄送過郵件，不可以刪除！╣
                                return $.Deferred().reject().promise();
                            }
                            else {
                                var saNewList = [];
                                $.each(saGridData, function (idx, item) {
                                    if (item.guid !== args.guid) {
                                        saNewList.push(item);
                                    }
                                });
                                saGridData = saNewList;
                                bRequestStorage = true;
                            }
                        }
                    },
                    onInit: function (args) {
                        oGrid = args.grid;
                        setTimeout(function () {
                            $('.tooltips').each(function () {
                                $(this).parents('tr').attr('title', this.title);
                            });
                        }, 1000);
                    }
                });
            },

            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             * @return  無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        if (sAction === 'Add') {
                            fnAdd('add');
                        }
                        else {
                            fnUpd();
                        }

                        break;
                    case "Toolbar_ReAdd":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }
                        fnAdd('readd');

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        pageLeave();

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Void":

                        if (fnCheckBillEffective(oCurData.Bills)) {
                            showMsg(i18next.t("message.OpmNotToVoid")); // ╠message.OpmNotToVoid⇒已建立有效的賬單，暫時不可作廢╣
                            return false;
                        }
                        fnVoid();

                        break;
                    case "Toolbar_OpenVoid":
                        // ╠message.ToOpenVoid⇒確定要啟用該筆資料嗎？╣ ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.ToOpenVoid'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnOpenVoid();
                            layer.close(index);
                        });

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /**
             * 初始化 function
             * @param 無
             * @return  無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            init = function () {
                var saCusBtns = [];

                var myHelpers = {
                    setSupplierName: function (val1, val2) {
                        return !val1 ? val2 : val1;
                    },
                    dtformate: function (val) {
                        return newDate(val);
                    },
                    setStatus: function (status) {
                        return sStatus;
                    }
                };
                $.views.helpers(myHelpers);

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true,
                    tabClick: function (el) {
                        switch (el.id) {
                            case 'litab2':
                                if (!$(el).data('action')) {
                                    if (!sAppointNO) {
                                        fnGridInit();
                                        //oGrid.loadData();
                                    }
                                    $(el).data('action', true);
                                }
                                break;
                            case 'litab3':
                            case 'litab4':
                                if (!$(el).data('action')) {
                                    fnBindFinancial();
                                    $('#litab3').data('action', true);
                                    $('#litab4').data('action', true);
                                    fnOpenAccountingArea($('div .OnlyForAccounting'), parent.UserInfo.roles);
                                }
                                break;
                        }
                    }
                });

                $('#Volume').on('blur', function () {
                    var sVal = this.value;
                    $('#VolumeWeight').val((Math.floor(sVal * 100) / 100 * 167).toFloat(2));
                });

                //加載報關類別,加載報價頁簽,加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
                $.whenArray([
                    fnGet(),
                    fnSetCustomersDrop(),
                    fnGetOfficeTempls({
                        TemplID: parent.SysSet.InvoiceDownLoadMenu + parent.SysSet.InvoicePrintMenu + parent.SysSet.ReceiptDownLoadMenu + parent.SysSet.ReceiptPrintMenu,
                        CallBack: function (data) {
                            oPrintMenu.InvoiceDownLoadMenu = { tmpl: $('<div data-id="InvoiceDownLoadMenu">') };
                            oPrintMenu.InvoicePrintMenu = { tmpl: $('<div data-id="InvoicePrintMenu">') };
                            oPrintMenu.ReceiptDownLoadMenu = { tmpl: $('<div data-id="ReceiptDownLoadMenu">') };
                            oPrintMenu.ReceiptPrintMenu = { tmpl: $('<div data-id="ReceiptPrintMenu">') };
                            $.each(data, function (idx, item) {
                                var sType = '';
                                if (parent.SysSet.InvoiceDownLoadMenu.indexOf(item.TemplID) > -1) {
                                    sType = 'InvoiceDownLoadMenu';
                                }
                                else if (parent.SysSet.InvoicePrintMenu.indexOf(item.TemplID) > -1) {
                                    sType = 'InvoicePrintMenu';
                                }
                                else if (parent.SysSet.ReceiptDownLoadMenu.indexOf(item.TemplID) > -1) {
                                    sType = 'ReceiptDownLoadMenu';
                                }
                                else if (parent.SysSet.ReceiptPrintMenu.indexOf(item.TemplID) > -1) {
                                    sType = 'ReceiptPrintMenu';
                                }
                                oPrintMenu[sType].tmpl.append('<a href="#" class="print-item" data-id="' + item.TemplID + '">' + item.TemplName + '</a>');
                            });
                        }
                    }),
                    fnSetEpoDrop({
                        Select: $('#ExhibitionNO'),
                        Select2: true
                    }),
                    fnSetUserDrop([
                        {
                            Select: $('#ResponsiblePerson'),
                            ShowId: true,
                            Select2: true,
                            Action: sAction,
                            ServiceCode: parent.SysSet.IMCode,
                            CallBack: function (data) {
                                var sCode = parent.UserInfo.ServiceCode;
                                if (sAction === 'Add' && sCode && parent.SysSet.IMCode.indexOf(sCode) > -1) {
                                    $('#ResponsiblePerson').val(parent.UserInfo.MemberID);
                                    sServiceCode = sCode;
                                    sDeptCode = parent.UserInfo.DepartmentID;
                                }
                            }
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'Clearance',
                            Select: $('#CustomsClearance'),
                            ShowId: true
                        },
                        {
                            ArgClassID: 'Currency',
                            CallBack: function (data) {
                                saCurrency = data;
                                sCurrencyOptionsHtml = createOptions(data, 'id', 'text');
                            }
                        },
                        {
                            ArgClassID: 'DeclClass',
                            Select: $('#DeclarationClass'),
                            ShowId: true
                        },
                        {
                            ArgClassID: 'Transport',
                            Select: $('#TransportationMode'),
                            ShowId: true
                        },
                        {
                            ArgClassID: 'Hall',
                            Select: $('#Hall')
                        },
                        {
                            ArgClassID: 'Port',
                            CallBack: function (data) {
                                $('.quickquery-city').on('keyup', function () {
                                    this.value = this.value.toUpperCase();
                                }).on('blur', function () {
                                    var sId = this.value,
                                        oPort = Enumerable.From(data).Where(function (e) { return e.id == sId; });
                                    if (oPort.Count() === 1) {
                                        $(this).parent().next().next().find(':input').val(oPort.First().text);
                                    }
                                }).autocompleter({
                                    // marker for autocomplete matches
                                    highlightMatches: true,
                                    // object to local or url to remote search
                                    source: data,
                                    // custom template
                                    template: '{{ id }} <span>({{ label }})</span>',
                                    // show hint
                                    hint: true,
                                    // abort source if empty field
                                    empty: false,
                                    // max results
                                    limit: 20,
                                    callback: function (value, index, selected) {
                                        if (selected) {
                                            var that = this;
                                            $(that).parent().find(':input').val(selected.id);
                                            $(that).parent().next().next().find(':input').val(selected.text);
                                        }
                                    }
                                });
                            }
                        },
                        {
                            ArgClassID: 'FeeClass',
                            CallBack: function (data) {
                                saFeeClass = data;
                            }
                        }
                    ]),
					fnGetNewExhibition(),
					fnGetNewCustomers()
					])
                    .done(function (res) {
                        if (res && res[0].d && res[0].d !== '-1') {
                            var oRes = $.parseJSON(res[0].d);
                            fnGetCurrencyThisYear(oRes.CreateDate).done(function () {
                                oRes.Exhibitors = saGridData = (oRes.Exhibitors) ? JSON.parse(oRes.Exhibitors) : [];
                                oRes.Quote = JSON.parse(oRes.Quote || '{}');

                                oRes.EstimatedCost = JSON.parse(oRes.EstimatedCost || '{}');
                                oRes.ActualCost = JSON.parse(oRes.ActualCost || '{}');
                                oRes.Bills = JSON.parse(oRes.Bills || '[]');
                                oRes.Quote.FeeItems = oRes.Quote.FeeItems || [];
                                oRes.EstimatedCost.FeeItems = oRes.EstimatedCost.FeeItems || [];
                                oRes.ActualCost.FeeItems = oRes.ActualCost.FeeItems || [];

                                oRes.Quote.guid = oRes.Quote.guid || guid();
                                oRes.Quote.KeyName = oRes.Quote.KeyName || 'Quote';
                                oRes.Quote.AuditVal = oRes.Quote.AuditVal || '0';
                                oRes.EstimatedCost.guid = oRes.EstimatedCost.guid || guid();
                                oRes.EstimatedCost.KeyName = oRes.EstimatedCost.KeyName || 'EstimatedCost';
                                oRes.EstimatedCost.AuditVal = oRes.EstimatedCost.AuditVal || '0';
                                oRes.ActualCost.guid = oRes.ActualCost.guid || guid();
                                oRes.ActualCost.KeyName = oRes.ActualCost.KeyName || 'ActualCost';
                                oRes.ActualCost.AuditVal = oRes.ActualCost.AuditVal || '0';

                                oCurData = oRes;
                                nowResponsiblePerson = oCurData.ResponsiblePerson;
                                if (oCurData.Agent) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === oCurData.Agent; }).First(),
                                        saContactors = JSON.parse(oCur.Contactors || '[]');
                                    $('#AgentContactor').html(createOptions(saContactors, 'guid', 'FullName'));
                                }
                                else {
                                    $('#AgentContactor').html(createOptions([]));
                                }
                                setFormVal(oForm, oRes);
                                fnSetUserDrop([
                                    {
                                        MemberID: oCurData.ResponsiblePerson,
                                        CallBack: function (data) {
                                            var oRes = data[0];
                                            sServiceCode = oRes.ServiceCode;
                                            sDeptCode = oRes.DepartmentID;
                                        }
                                    }
                                ]);
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
                                $('#ExhibitionDateStart').val(newDate(oCurData.ExhibitionDateStart, 'date', true));
                                $('#ExhibitionDateEnd').val(newDate(oCurData.ExhibitionDateEnd, 'date', true));

                                if (saGridData.length > 0) {
                                    $.each(saGridData, function (idx, item) {
                                        if (!item.guid) {
                                            item.guid = guid();
                                        }
                                    });
                                }

                                moneyInput($('[data-type="int"]'), 0);

                                if (sAction === 'Add' && sAppointNO) {
                                    fnInitAppoint();//如果是匯入預約單進來則預設預約單資料
                                }
                                var Authorized = ExhibitionBillAuthorize(oCurData);
                                if (Authorized) {
                                    $('[href="#tab3"],[href="#tab4"]').parent().show();
                                    if (sGoTab) {
                                        $('#litab' + sGoTab).find('a').click();
                                        if (sBillNOGO && $('.bill-box-' + sBillNOGO).length > 0) {
                                            $('.bill-box-' + sBillNOGO).click();
                                            goToJys($('.bill-box-' + sBillNOGO));
                                        }
                                    }
                                }
                                else {
                                    $('[href="#tab3"],[href="#tab4"]').parent().hide();
                                }
                            });
                        }
                        else{
							fnGetCurrencyThisYear(new Date());
							
							if(sAction === 'Add' && sExhibitionNO){
								$("#ExhibitionNO").val(sExhibitionNO).select2();
								$("#ImportBillEName").val(saNewExhibition.Exhibitioname_EN);
								$('#ExhibitionDateStart').val(newDate(saNewExhibition.ExhibitionDateStart, 'date', true));
								$('#ExhibitionDateEnd').val(newDate(saNewExhibition.ExhibitionDateEnd, 'date', true));
							}
						}

                    });

                $.validator.addMethod("compardate", function (value, element, parms) {
                    if (new Date(value) < new Date($('#ExhibitionDateStart').val())) {
                        return false;
                    }
                    return true;
                });
                oValidator = $("#form_main").validate({
                    ignore: '',
                    rules: {
                        AgentEamil: {
                            email: true
                        }
                    },
                    messages: {
                        AgentEamil: i18next.t("message.IncorrectEmail")// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    }
                });

                $.timepicker.dateRange($('#ExhibitionDateStart'), $('#ExhibitionDateEnd'),
                    {
                        minInterval: 1000 * 60 * 60 * 24 * 1, // 1 days
                        changeYear: true,
                        changeMonth: true
                    }
                );
                $.timepicker.datetimeRange($('#ApproachTime'), $('#ExitTime'),
                    {
                        minInterval: 1000 * 60 * 60 * 24 * 1, // 1 days
                        changeYear: true,
                        changeMonth: true
                    }
                );

                $('#ResponsiblePerson').change(function () {
                    var sVal = this.value;
                    if (sVal) {
                        fnSetUserDrop([
                            {
                                MemberID: sVal,
                                CallBack: function (data) {
                                    var oRes = data[0];
                                    sServiceCode = oRes.ServiceCode;
                                    sDeptCode = oRes.DepartmentID;
                                }
                            }
                        ]);
                    }
                    else {
                        sServiceCode = '';
                        sDeptCode = '';
                    }
                });
                $('#ExhibitionNO').change(function () {
                    var sId = this.value;
                    if (sId) {
                        fnSetEpoDrop({
                            SN: sId,
                            CallBack: function (data) {
                                var oExhibition = data[0];
                                $('#ImportBillEName').val(oExhibition.Exhibitioname_EN);
                                if (oExhibition.ExhibitionDateStart) {
                                    $('#ExhibitionDateStart').val(newDate(oExhibition.ExhibitionDateStart, 'date'));
                                }
                                if (oExhibition.ExhibitionDateEnd) {
                                    $('#ExhibitionDateEnd').val(newDate(oExhibition.ExhibitionDateEnd, 'date'));
                                }
                                $('#Hall').val(oExhibition.ExhibitionAddress);
                            }
                        });
                    }
                    else {
                        $('#ImportBillEName').val('');
                        $('#ExhibitionDateStart').val('');
                        $('#ExhibitionDateEnd').val('');
                    }
                });

                $(window).on('scroll', function () {
                    var h = ($(document).height(), $(this).scrollTop());
                    if (h < 81) {
                        $('.sum-box').css({ top: 125 - h });
                    }
                    else {
                        $('.sum-box').css({ top: 44 });
                    }
                });
            };

        init();
    };

require(['base', 'jsgrid', 'select2', 'formatnumber', 'autocompleter', 'jquerytoolbar', 'timepicker', 'ajaxfile', 'common_opm', 'util'], fnPageInit, 'timepicker');