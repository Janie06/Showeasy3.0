'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('ImportBillNO'),
    sFlag = getUrlParam('Flag'),
    sGoTab = getUrlParam('GoTab'),
    sBillNOGO = getUrlParam('BillNO'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var FeeItemCurrency = "TE,TG".indexOf(parent.UserInfo.OrgID) > -1 ? 'NTD' : 'RMB';
        var oValidator = null,
            oForm = $('#form_main'),
            oGrid = null,
            sServiceCode = '',
            sDeptCode = '',
            sTransportOptionsHtml = '',
            sCustomersOptionsHtml = '',
            sCustomersNotAuditOptionsHtml = '',
            sCurrencyOptionsHtml = '',
            sAccountingCurrencyOptionsHtml = '',
            oAddItem = {},
            oCurData = { Suppliers: [] },
            oPrintMenu = {},
            saCustomers = [],
            saFeeClass = [],
            saPort = [],
            saCurrency = [],
            saAccountingCurrency = [],
            nowResponsiblePerson = '',
            /**
             * 獲取資料
             */
            fnGet = function () {
                if (sDataId) {
                    return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oRes.Import = (oRes.Import) ? JSON.parse(oRes.Import) : {};
                                $('#VoidReason').text(oRes.VoidReason);
                                if (oRes.IsVoid == 'Y') {
                                    $('.voidreason').show();
                                    $('#Toolbar_Void').attr({ 'id': 'Toolbar_OpenVoid', 'data-i18n': 'common.Toolbar_OpenVoid' });
                                }
                                else {
                                    $('.voidreason').hide();
                                    $('#Toolbar_OpenVoid').attr({ 'id': 'Toolbar_Void', 'data-i18n': 'common.Toolbar_Void' });
                                }
                                if (!oRes.Import.ExhibitionWarehouse) {
                                    $('#Import_ExhibitionWarehouse_Checked').parents('.form-group').remove();
                                }
                                transLang($('#Toolbar'));
                            }
                        });
                }
                else {
                    oCurData.Quote = { guid: guid(), KeyName: 'Quote', AuditVal: '0', FeeItems: [] };
                    oCurData.EstimatedCost = { guid: guid(), KeyName: 'EstimatedCost', AuditVal: '0', FeeItems: [] };
                    oCurData.ActualCost = { guid: guid(), KeyName: 'ActualCost', AuditVal: '0', FeeItems: [] };
                    oCurData.Bills = [];
                    oCurData.ReturnBills = [];
                    oCurData.Suppliers = [];
                    $('#Contactor').html(createOptions([]));
                    fnInitfileInput('');
                    fntBindCheckBoxEvent();
                    fnSetPermissions();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增或修改完之后重新查询资料
             */
            fnReSet = function () {
                fnGet().done(function (res) {
                    var oRes = res.DATA.rel;
                    setFlowBox(true);
                    $('#VoidReason').text(oRes.VoidReason);
                    if (oRes.VoidReason) { $('.voidreason').show(); } else { $('.voidreason').hide(); }
                    getPageVal(); //緩存頁面值，用於清除
                });
            },
            /**
             * 取得帳單log資料
             * @return {Object}
             */
            fnGetBillLogData = function (Bill) {
                var LogData = {};
                LogData.OrgID = parent.OrgID;
                LogData.BillNO = Bill.BillNO;
                LogData.ExhibitioName = oCurData.Exhibitioname_TW; //ExhibitioName 
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
             * 獲取貨物狀態
             */
            fnGetFlowStatus = function (data) {
                var sFlowStatus = '';
                if (data.ReImports.length > 0) {
                    $.each(data.ReImports, function (idx, item) {
                        var iLen = idx + 1;
                        if (item.ReImport.Sign && item.ReImport.Sign.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re6';
                        }
                        else if (item.ReImport.ReachDestination && item.ReImport.ReachDestination.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re5';
                        }
                        else if (item.ReImport.HuiYun && item.ReImport.HuiYun.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re4';
                        }
                        else if (item.ReImport.ReCargoRelease && item.ReImport.ReCargoRelease.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re3';
                        }
                        else if (item.ReImport.ReCustomsDeclaration && item.ReImport.ReCustomsDeclaration.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re2';
                        }
                        else if (item.ReImport.FileValidation && item.ReImport.FileValidation.Checked) {
                            sFlowStatus += 'ReImport' + iLen + '-Re1';
                        }
                    });
                }
                else if (data.Import.Sign && data.Import.Sign.Checked) {
                    sFlowStatus = 'Import-6';
                }
                else if (data.Import.ExhibitionWarehouse && data.Import.ExhibitionWarehouse.Checked) {
                    sFlowStatus = 'Import-5';
                }
                else if (data.Import.CargoRelease && data.Import.CargoRelease.Checked) {
                    sFlowStatus = 'Import-4';
                }
                else if (data.Import.CustomsDeclaration && data.Import.CustomsDeclaration.Checked) {
                    sFlowStatus = 'Import-3';
                }
                else if (data.Import.GoodsArrival && data.Import.GoodsArrival.Checked) {
                    sFlowStatus = 'Import-2';
                }
                else if (data.Import.ReceiveFile && data.Import.ReceiveFile.Checked) {
                    sFlowStatus = 'Import-1';
                }
                return sFlowStatus;
            },
            /*
             * 新增資料
             * @param {String}flag 新增或儲存後新增
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.RefNumber = 'SerialNumber+3|' + parent.UserInfo.OrgID + '|C' + parent.UserInfo.OrgID + 'I|MinYear|3|' + sServiceCode + '|' + sServiceCode;
                data.Release = data.Import.CargoRelease.Checked ? 'Y' : 'N';
                data.ReturnLoan = data.ReturnLoan || {};
                data.ReturnLoan = JSON.stringify(data.ReturnLoan);
                data.TaxInformation = data.TaxInformation || {};
                data.TaxInformation = JSON.stringify(data.TaxInformation);
                data.Weight = data.Weight == '' ? 0 : data.Weight;
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
                data.ReturnBills = oCurData.ReturnBills || [];
                data.Suppliers = oCurData.Suppliers || [];
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                data.ReturnBills = JSON.stringify(data.ReturnBills);
                data.Suppliers = JSON.stringify(data.Suppliers);
                if (data.Contactor) {
                    data.ContactorName = $('#Contactor option:selected').text();
                }
                else {
                    data.ContactorName = '';
                }
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

                data.ReImports = [];
                for (var idx = 1; idx < 11; idx++) {
                    var oReImport = data['ReImport' + idx];
                    if (oReImport) {
                        var oReImports = {};
                        oReImports.SignatureFileId = data['SignatureFileId' + idx];
                        oReImports.ReImport = oReImport;
                        oReImports.ReImportData = data['ReImportData' + idx];
                        data.ReImports.push(oReImports);
                        data['SignatureFileId' + idx] = undefined;
                        data['ReImport' + idx] = undefined;
                        data['ReImportData' + idx] = undefined;
                    }
                    else {
                        break;
                    }
                }
                data.Flow_Status = fnGetFlowStatus(data);
                data.Import = JSON.stringify(data.Import);
                data.ReImports = JSON.stringify(data.ReImports);

                if (!data.ArrivalTime) delete data.ArrivalTime;
                if (!data.FreePeriod) delete data.FreePeriod;
                if (!data.ApproachTime) delete data.ApproachTime;
                if (!data.ExitTime) delete data.ExitTime;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;
                data.ImportBillNO = sDataId = guid();
                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: { importexhibition: data }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (flag == 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&ImportBillNO=' + sDataId); // ╠message.Save_Success⇒新增成功╣
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
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);
                data = packParams(data, 'upd');
                data.Release = data.Import.CargoRelease.Checked ? 'Y' : 'N';
                data.ReturnLoan = data.ReturnLoan || {};
                data.ReturnLoan = JSON.stringify(data.ReturnLoan);
                data.TaxInformation = data.TaxInformation || {};
                data.TaxInformation = JSON.stringify(data.TaxInformation);
                data.Weight = data.Weight == '' ? 0 : data.Weight;
                data.IsVoid = oCurData.IsVoid;
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
                data.ReturnBills = oCurData.ReturnBills || [];
                data.Suppliers = oCurData.Suppliers || [];
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                data.ReturnBills = JSON.stringify(data.ReturnBills);
                data.Suppliers = JSON.stringify(data.Suppliers);
                if (data.Contactor) {
                    data.ContactorName = $('#Contactor option:selected').text();
                }
                else {
                    data.ContactorName = '';
                }
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

                data.ReImports = [];
                for (var idx = 1; idx < 11; idx++) {
                    var oReImport = data['ReImport' + idx];
                    if (oReImport) {
                        var oReImports = {};
                        oReImports.SignatureFileId = data['SignatureFileId' + idx];
                        oReImports.ReImport = oReImport;
                        oReImports.ReImportData = data['ReImportData' + idx];
                        data.ReImports.push(oReImports);
                        data['SignatureFileId' + idx] = undefined;
                        data['ReImport' + idx] = undefined;
                        data['ReImportData' + idx] = undefined;
                    }
                    else {
                        break;
                    }
                }
                data.Flow_Status = fnGetFlowStatus(data);
                data.Import = JSON.stringify(data.Import);
                data.ReImports = JSON.stringify(data.ReImports);

                delete data.ImportBillNO;
                if (!data.ArrivalTime) delete data.ArrivalTime;
                if (!data.FreePeriod) delete data.FreePeriod;
                if (!data.ApproachTime) delete data.ApproachTime;
                if (!data.ExitTime) delete data.ExitTime;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: data,
                            keys: { ImportBillNO: sDataId }
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
             */
            fnDel = function () {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        importexhibition: {
                            ImportBillNO: sDataId
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
                                importexhibition: {
                                    values: data,
                                    keys: { ImportBillNO: sDataId }
                                }
                            }
                        }, function (res) {
                            if (res.d > 0) {
                                oCurData.IsVoid = 'Y';
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
             * 資料啟用
             */
            fnOpenVoid = function () {
                var data = {
                    IsVoid: 'N',
                    VoidReason: ''
                };
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: data,
                            keys: { ImportBillNO: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        oCurData.IsVoid = 'N';
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
             * 匯入資料
             */
            fnImport = function () {
                var oConfig = {
                    Get: fnGetPopData_ImportData,
                    SearchFields: [
                        { id: "Pop_ImportBillName", type: 'text', i18nkey: 'ExhibitionImport_Upd.ImportBillName' },
                        { id: "Pop_ImportBillEName", type: 'text', i18nkey: 'ExhibitionImport_Upd.ImportBillEName' },
                        { id: "Pop_Supplier", type: 'text', i18nkey: 'ExhibitionImport_Upd.Supplier' },
                        { id: "Pop_Agent", type: 'text', i18nkey: 'ExhibitionImport_Upd.Agent' }
                    ],
                    Fields: [
                        { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 40 },
                        { name: "RefNumber", title: 'ExhibitionImport_Upd.RefNumber', width: 100 },
                        { name: "ImportBillName", title: 'ExhibitionImport_Upd.ImportBillName', width: 150 },
                        { name: "ImportBillEName", title: 'ExhibitionImport_Upd.ImportBillEName', width: 150 },
                        { name: "SupplierCName", title: 'ExhibitionImport_Upd.Supplier', width: 150 },
                        { name: "AgentName", title: 'ExhibitionImport_Upd.Agent', width: 150 }
                    ],
                    Callback: function (item) {
                        var oData = {};

                        oData.RefNumberEmail = item.RefNumberEmail;
                        oData.ResponsiblePerson = item.ResponsiblePerson;
                        oData.ExhibitionNO = item.ExhibitionNO;
                        oData.ImportBillName = item.ImportBillName;
                        oData.ImportBillEName = item.ImportBillEName;
                        oData.ExhibitionDateStart = item.ExhibitionDateStart;
                        oData.ExhibitionDateEnd = item.ExhibitionDateEnd;
                        oData.Hall = item.Hall;
                        oData.MuseumMumber = item.MuseumMumber;
                        oData.Supplier = item.Supplier;
                        oData.Contactor = item.Contactor;
                        oData.Telephone = item.Telephone;
                        oData.SupplierEamil = item.SupplierEamil;
                        oData.Agent = item.Agent;
                        oData.AgentContactor = item.AgentContactor;
                        oData.AgentTelephone = item.AgentTelephone;
                        oData.AgentEmail = item.AgentEmail;

                        setFormVal(oForm, oData);
                        if (oData.Supplier) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == oData.Supplier; }).First(),
                                saContactors = JSON.parse(oCur.Contactors || '[]');
                            $('#Contactor').html(createOptions(saContactors, 'guid', 'FullName')).val(oData.Contactor);
                        }
                        else {
                            $('#Contactor').html(createOptions([]));
                        }
                        if (oData.Agent) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == oData.Agent; }).First(),
                                saContactors = JSON.parse(oCur.Contactors || '[]');
                            $('#AgentContactor').html(createOptions(saContactors, 'guid', 'FullName')).val(oData.Contactor);
                        }
                        else {
                            $('#AgentContactor').html(createOptions([]));
                        }
                        $('#ExhibitionDateStart').val(newDate(oData.ExhibitionDateStart, 'date', true));
                        $('#ExhibitionDateEnd').val(newDate(oData.ExhibitionDateEnd, 'date', true));
                    }
                };
                oPenPops(oConfig);
            },
            /**
             * 流程修改發送郵件
             * @param (int) iflag 第幾個流程
             * @param (function) callback 回調函數
             */
            fnSendEmail = function (iflag, callback) {
                if (parent.SysSet.IsOpenMail != 'Y') {
                    layer.alert(i18next.t('message.NotOpenMail'), { icon: 0 }, function () {// ╠message.NotOpenMail⇒系統沒有開放郵件發送功能，請聯絡管理員！╣
                        callback(iflag);
                    });
                    return false;
                }
                CallAjax(ComFn.W_Com, ComFn.SendMail, {
                    Params: {
                        FromOrgID: parent.OrgID,
                        FromUserID: parent.UserID,
                        EmailTo: [{
                            ToUserID: $('#ResponsiblePerson').val(),
                            Type: 'to'
                        }],
                        MailTempId: 'FlowChange',
                        MailData: {
                            RefNumber: $('#RefNumber').val(),
                            ExhibitionType: '進口',
                            DataDources: '進口管理',
                            ChangeItem: iflag === 0 ? i18next.t("ExhibitionImport_Upd.Import") : i18next.t("ExhibitionImport_Upd.ReImport" + iflag),
                            SupplierName: '',
                            ModifyUser: parent.UserInfo.MemberName,
                            ModifyDate: newDate(null, 'date')
                        }
                    }
                }, function (res) {
                    if (res.d === '1') {
                        callback();
                        showMsg(i18next.t("message.SendEmail_Success"), 'success'); // ╠message.SendEmail_Success⇒郵件寄送成功╣
                    }
                    else {
                        showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                });
            },
            /**
             * 發送Tracking number給客戶格式
             */
            fnSendTrackingNumberEmail = function () {
                var sSupplierEamil = $('#RefNumberEmail').val(),
                    saSupplierEamil = [],
                    bAllRight = true,
                    sMsg = '';
                if (parent.SysSet.IsOpenMail != 'Y') {
                    showMsg(i18next.t("message.NotOpenMail"));// ╠message.NotOpenMail⇒系統沒有開放郵件發送功能，請聯絡管理員！╣
                    return false;
                }
                if (!sSupplierEamil) {
                    sMsg += i18next.t("ExhibitionImport_Upd.SupplierEamil_required") + '<br/>'; // ╠ExhibitionImport_Upd.SupplierEamil_required⇒請輸入付款人╣
                }
                if (!$('#BillLadNO').val()) {
                    sMsg += i18next.t("common.BillLadNO_required") + '<br/>'; // ╠common.BillLadNO_required⇒請輸入提單號碼╣
                }
                if (!$('#ShipmentPort').val()) {
                    sMsg += i18next.t("common.ShipmentPort_required") + '<br/>'; // ╠common.ShipmentPort_required⇒請輸入起運地╣
                }
                if (!$('#DestinationPort').val()) {
                    sMsg += i18next.t("common.DestinationPort_required") + '<br/>'; // ╠common.DestinationPort_required⇒請輸入目的地╣
                }
                if (!$('#BoxNo').val()) {
                    sMsg += i18next.t("message.Number_required") + '<br/>'; // ╠message.Number_required⇒請輸入件數╣
                }
                if (!$('#Unit').val()) {
                    sMsg += i18next.t("message.Unit_required"); // ╠message.Unit_required⇒ 請輸入（件數）單位╣
                }
                if (sMsg) {
                    showMsg(sMsg);
                    return false;
                }
                saSupplierEamil = sSupplierEamil.split(/[;；，,/|]/);

                if (saSupplierEamil.length > 0) {
                    var saEmailTo = [],
                        toSendMail = function () {
                            CallAjax(ComFn.W_Com, ComFn.SendMail, {
                                Params: {
                                    FromOrgID: parent.OrgID,
                                    FromUserName: parent.SysSet.FromName || '系統郵件',
                                    EmailTo: saEmailTo,
                                    MailTempId: 'TrackingNumberNotice',
                                    MailData: {
                                        RefNumber: $('#RefNumber').val(),
                                        BillLadNO: $('#BillLadNO').val(),
                                        BillLadNOType: '',
                                        ExhibitionName: oCurData.Exhibitioname_TW || '',
                                        ExhibitionEName: oCurData.Exhibitioname_EN || '',
                                        Shipment: $('#ShipmentPortCode').val(),
                                        Destination: $('#DestinationPortCode').val(),
                                        Number: $('#BoxNo').val() + ' ' + $('#Unit').val()
                                    }
                                }
                            }, function (res) {
                                if (res.d === '1') {
                                    showMsg(i18next.t("message.SendEmail_Success"), 'success'); // ╠message.SendEmail_Success⇒郵件寄送成功╣
                                    CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                        Params: {
                                            importexhibition: {
                                                values: { IsSendMail: 'Y' },
                                                keys: { ImportBillNO: sDataId }
                                            }
                                        }
                                    }, function (res) {
                                        if (res.d > 0) {
                                            oCurData.IsSendMail = 'Y';
                                        }
                                    });
                                }
                                else {
                                    showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                                }
                            }, function () {
                                showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                            });
                        };
                    $.each(saSupplierEamil, function (idx, email) {
                        if (isEmail(email)) {
                            saEmailTo.push({
                                ToUserName: email,
                                ToEmail: email,
                                Type: 'to'
                            });
                        }
                        else {
                            bAllRight = false;
                            return false;
                        }
                    });
                    if (bAllRight) {
                        if (oCurData.IsSendMail !== 'Y') {
                            toSendMail();
                        }
                        else {// ╠message.IsSendTrackingNumberEmail⇒已寄送过，是否再次寄送？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendTrackingNumberEmail'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                toSendMail();
                                layer.close(index);
                            });
                        }
                    }
                    else {
                        showMsg(i18next.t("message.IncorrectEmail"), 'error'); // ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    }
                }
            },
            /**
             * 設定完成按鈕
             * @param {Object} flow 父層dom對象
             */
            setSuccessBtn = function (flow) {
                var iCheckBox = flow.find(':input[type=checkbox]').length,
                    iChecked = flow.find(':input[type=checkbox]').not("input:checked").length;
                if (iChecked == 0 && iCheckBox != iChecked) {
                    flow.find(':input.complete').removeAttr('disabled');
                }
                else {
                    flow.find(':input.complete').attr('disabled', true);
                }
            },
            /**
             * 註冊checkbox點擊事件
             */
            fntBindCheckBoxEvent = function () {
                $('.complete').off('click').click(function () {
                    var oBtn = this,
                        sId = $(this).parents('.flow').attr('id'),
                        data = getFormSerialize(oForm);

                    data[sId].complete = 'Y';

                    $('#' + sId + '_complete').val('Y');
                    $(oBtn).attr('disabled', true);
                    $('#' + sId).find('[name]').attr('disabled', true);
                    if ($('.addreimport').length > 0) {
                        $('.addreimport').removeAttr('disabled').off('click').click(function () {
                            fnAddReimport();
                        });
                    }
                    if ($('#' + sId).find('.addnextreimport').length > 0) {
                        $('#' + sId).find('.addnextreimport').removeAttr('disabled').off('click').click(function () {
                            fnAddNextReimport();
                            fnAddReturnData();
                            fnAddNextUpload(1);
                        });
                    }
                });

                //報關作業追蹤 Check Clike事件
                $(':input[type=checkbox]').off('click').on('click', function (e) {
                    setTime(this);
                    setSuccessBtn($(this).parents('.flow'));
                });
            },
            /**
             * 設定完成按鈕
             * @param {Object} that 父層dom對象
             */
            setTime = function (checkbox) {
                let sDate = newDate(null, true);
                let divFormGroup = checkbox.parentNode.parentNode;
                let FormGroupDate = divFormGroup.querySelector('.date-picker');

                if (checkbox.checked) {
                    switch (checkbox.id) {
                        case 'Import_GoodsArrival_Checked':
                            let ArrivalTime = document.querySelector('#ArrivalTime')
                            sDate = ArrivalTime.value == '' ? sDate : ArrivalTime.value;
                            break;
                        case 'Import_Sign_Checked':
                            let ApproachTime = document.querySelector('#ApproachTime');
                            sDate = ApproachTime.value === '' ? sDate : ApproachTime.value;
                            break;
                        case 'Import_InTransit1_Checked':
                            //運輸中
                            let DesPortCode = document.querySelector('#DestinationPortCode'); //目的地
                            let InTransitETA = document.querySelector('#Import_InTransit1_ETA'); //運送中-ETA
                            InTransitETA.value = DesPortCode.value;
                            break;
                    }

                    //勾選後設定日期(當天)
                    FormGroupDate.value = sDate
                }
                else {
                    FormGroupDate.value = '';

                    switch (checkbox.id) {
                        case 'Import_InTransit1_Checked':
                            //運輸中
                            let InTransitETA = document.querySelector('#Import_InTransit1_ETA'); //運送中-ETA
                            let IntransitDate = document.querySelector('#Import_InTransit1_ETADate'); //運輸中-日期
                            InTransitETA.value = '';
                            IntransitDate.value = '';
                            break;
                    }
                }
            },
            /**
             * 獲取簽名檔
             */
            fnGetSignatureFile = function (flag) {
                var sFileId = $('#SignatureFileId' + flag).val();
                if (sFileId) {
                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            files: {
                                ParentID: sFileId,
                                OrgID: parent.OrgID
                            }
                        }
                    }, function (res) {
                        if (res.d) {
                            var oFiles = $.parseJSON(res.d);
                            if (oFiles.FileID) {
                                oFiles.CreateDate = newDate(oFiles.CreateDate);
                                var sHtml = $("#temp_files_list").render(oFiles);
                                $('#section' + flag).hide();
                                $('#ShowSignatureFile' + flag).html(sHtml);

                                $('#ShowSignatureFile' + flag + ' .downloadfile').click(function () {
                                    var sPath = $(this).attr('data-path');
                                    DownLoadFile(sPath);
                                });
                                $('#ShowSignatureFile' + flag + ' .deletefile').click(function () {
                                    var sId = $(this).attr('data-fileid');
                                    // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
                                    layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                        DelFile(sId).done(function () {
                                            fnGetSignatureFile(flag);
                                        });
                                        layer.close(index);
                                    });
                                });
                            }
                            else {
                                $('#ShowSignatureFile' + flag).html('');
                                $('#section' + flag).show();
                            }
                        }
                    });
                }
                else {
                    if (flag == '') {
                        fnInitfileInput('');
                    }
                }
            },
            /**
             * 增加退運流程
             */
            fnAddReimport = function () {
                layer.open({
                    type: 1,
                    title: i18next.t('common.AddReimport'),// ╠common.AddReimport⇒增加退運╣
                    area: ['300px', '120px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    content: '<div class="pop-box">\
                             <button type="button" data-i18n="common.SingleReturn" id="SingleReturn" class="btn-custom green">單筆退運</button>\
                             <button type="button" data-i18n="common.MultipleReturn" id="MultipleReturn" class="btn-custom green">多筆退運</button>\
                          </div>',//╠common.SingleReturn⇒單筆退運╣╠common.MultipleReturn⇒多筆退運╣
                    success: function (layero, idx) {
                        $('#SingleReturn').click(function () {
                            fnAddNextReimport('one');
                            fnAddReturnData('one');
                            fnAddNextUpload(1, 'one');
                            $('#ReImport1').find(':input').removeAttr('disabled');
                            $('#ReImport1').find(':input[type=button]').attr('disabled', true);
                            layer.close(idx);
                        });
                        $('#MultipleReturn').click(function () {
                            fnAddNextReimport([{ index: '1' }, { index: '2' }]);
                            fnAddReturnData([{ index: '1' }, { index: '2' }]);
                            fnAddNextUpload(2, [{ index: '1' }, { index: '2' }]);
                            $('#ReImport1,#ReImport2').find(':input').removeAttr('disabled');
                            $('#ReImport1,#ReImport2').find(':input[type=button]').attr('disabled', true);
                            layer.close(idx);
                        });
                        transLang(layero);
                    }
                });
            },
            /**
             * 增加下一筆退運流程
             */
            fnAddNextReimport = function (data) {
                var iIndex = $('.flow').length;
                data = data == 'one' ? [{ index: iIndex, oneflow: true }] : data == undefined ? [{ index: iIndex }] : data;
                var sHtml = $("#temp_flows").render(data);
                $('#tab2').append(sHtml);
                fnSGMod();//上海駒驛須移除選項
                setFlowBox();
                transLang($('#tab2'));
            },
            /**
             * 增加退運資料
             */
            fnAddReturnData = function (data) {
                var iIndex = $('.flow').length - 1;
                data = data == 'one' ? [{ index: iIndex, oneflow: true }] : data == undefined ? [{ index: iIndex }] : data;
                var sHtml = $("#temp_returndata").render(data);
                $('#tab5').append(sHtml);

                var sNumber = $('#BoxNo').val(),
                    sUnit = $('#Unit').val();
                $.each(data, function (i, _data) {
                    if (sNumber) {
                        $('#ReImportData' + _data.index + '_Number').val(sNumber);
                    }
                    if (sUnit) {
                        $('#ReImportData' + _data.index + '_Unit').val(sUnit);
                    }
                });

                $('.date-picker').datepicker({
                    changeYear: true,
                    changeMonth: true,
                    altFormat: 'yyyy/MM/dd'
                });
                $('.Control_Transport').html(sTransportOptionsHtml);

                $('#tab5').find('.quickquery-city').off('keyup').on('keyup', function () {
                    this.value = this.value.toUpperCase();
                }).on('blur', function () {
                    var sId = this.value,
                        oPort = Enumerable.From(saPort).Where(function (e) { return e.id == sId; });
                    if (oPort.Count() === 1) {
                        $(this).parent().next().next().find(':input').val(oPort.First().text);
                    }
                }).autocompleter({
                    // marker for autocomplete matches
                    highlightMatches: true,
                    // object to local or url to remote search
                    source: saPort,
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
                transLang($('#tab5'));
            },
            /**
             * 增加下一筆上傳控件
             */
            fnAddNextUpload = function (flag, data) {
                var iIndex = $('.flow').length - 1;
                data = data == 'one' ? [{ index: iIndex, oneflow: true }] : data == undefined ? [{ index: iIndex }] : data;
                var sHtml = $("#temp_file_upload").render(data);
                $('#tab8').append(sHtml);
                if (flag == 2) {
                    fnInitfileInput(1);
                    fnInitfileInput(2);
                }
                else if (flag == 1) {
                    fnInitfileInput(iIndex);
                }

                transLang($('#tab8'));
            },
            /**
             * 上傳控件註冊事件
             */
            fnInitUploadEvents = function () {
                $('.upload').off('click').click(function () {
                    var that = this,
                        sId = that.id,
                        sIndex = $(that).attr('data-index') || '',
                        sFileId = $('#SignatureFileId' + sIndex).val();

                    if ($(that).parents('section').find('[type=file]')[0].files.length > 0) {
                        //上傳文件
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=upload&source=ExhibitionImport&userid=' + parent.UserID + '&orgid=' + parent.OrgID + '&parentid=' + sFileId,
                            secureuri: false,
                            fileElementId: 'fileInput' + sIndex,
                            success: function (data, status) {
                                $('#' + sId).parents('.form-group').find('.jFiler-items,.jFiler').remove();
                                fnInitfileInput(sIndex);
                                fnGetSignatureFile(sIndex);
                            },
                            error: function (data, status, e) {
                                showMsg(i18next.t("message.FilesUpload_Failed"), 'error'); // ╠message.FilesUpload_Failed⇒文件上傳失敗╣
                            }
                        });
                    }
                    else {
                        showMsg(i18next.t("message.PleaseSelectFile")); // ╠common.PleaseSelectFile⇒請選擇文件╣
                    }
                });
            },
            /**
             * 下載簽收證明|回運指示單
             * @param {String}flowtype
             */
            fnToDownLoadSignDocuments = function (flowtype, dataidx, supplierid) {
                var fnDownload = function () {
                    g_api.ConnectLite(sProgramId, 'OutputSignDocuments', {
                        ImportBillNO: sDataId,
                        Type: flowtype,
                        FileType: dataidx === '0' ? 'pdf' : 'word',
                        SupplierID: supplierid || ''
                    }, function (res) {
                        if (res.RESULT) {
                            var sPath = res.DATA.rel;
                            if (dataidx === '0') {
                                var index = layer.open({
                                    type: 2,// ╠common.DownLoadSignDocuments_Preview⇒簽收證明預覽╣ ╠common.DownLoadReturnSheet_Preview⇒回運指示單預覽╣
                                    title: flowtype === 'Import' ? i18next.t('common.DownLoadSignDocuments_Preview') : i18next.t('common.DownLoadReturnSheet_Preview'),
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
                        else {// ╠common.Preview_Failed⇒預覽失敗╣ ╠common.DownLoad_Failed⇒下載失敗╣
                            showMsg(i18next.t(dataidx === '0' ? 'common.Preview_Failed' : "common.DownLoad_Failed") + '<br>' + res.MSG, 'error');
                        }
                    }, function () {// ╠common.Preview_Failed⇒預覽失敗╣ ╠common.DownLoad_Failed⇒下載失敗╣
                        showMsg(i18next.t(dataidx === '0' ? 'common.Preview_Failed' : "common.DownLoad_Failed"), 'error');
                    }, true, i18next.t(dataidx === '0' ? 'message.Dataprocessing' : 'message.Downloading'));// ╠message.Dataprocessing⇒資料處理中...╣ ╠message.Downloading⇒文件下载中...╣
                };
                fnDownload();
            },
            /**
             * 設定流程頁簽
             */
            setFlowBox = function (flag) {
                var iFlowLength = $('.flow').length;
                if (iFlowLength > 1) {
                    $('.addreimport').hide();
                }
                else {
                    $('.addreimport').show();
                }
                if ($('.addnextreimport').length > 1) {
                    $('.addnextreimport').not(':last').hide();
                    $('.addnextreimport:last').show();
                }
                if (iFlowLength >= 6 || iFlowLength == 2) {
                    $('.addnextreimport').remove();
                }

                if ($('#Import_complete').val() == 'Y') {
                    $('#Import').find('[name]').attr('disabled', true);
                    $('#Import').find(':input.complete').attr('disabled', true);
                    if (flag) {
                        $('#Import').find(':input.undo').removeAttr('disabled').off('click').click(function () {
                            var fnCallBack = function () {
                                $('#Import').find(':input').removeAttr('disabled');
                                $('#Import').find(':input.undo').attr('disabled', true);
                                $('#Import').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(0, fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    }
                    $('.addreimport').removeAttr('disabled').off('click').click(function () {
                        fnAddReimport();
                    });
                } else {
                    setSuccessBtn($('#Import'));
                }

                if (flag) {
                    $('#Import').find('.dropdown-toggle').removeAttr('disabled');
                    $('#Import').find('.downsigndoc').off('click').click(function () {
                        fnToDownLoadSignDocuments('Import', $(this).attr('data-index'));
                    });
                }

                if (oCurData.ReImports) {
                    $.each(oCurData.ReImports, function (idx, reImport) {
                        var iIndex = idx + 1,
                            oFlow = $('#ReImport' + iIndex);
                        if (reImport.ReImport && (reImport.ReImport.complete || $('#ReImport' + iIndex + '_complete').val())) {
                            oFlow.find('[name]').attr('disabled', true);
                            oFlow.find(':input.complete').attr('disabled', true);
                            oFlow.find(':input.undo').removeAttr('disabled').off('click').click(function () {
                                var fnCallBack = function () {
                                    oFlow.find(':input').removeAttr('disabled');
                                    oFlow.find(':input.undo').attr('disabled', true);
                                    oFlow.find(':input[type=hidden]').val('');
                                };
                                // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                                layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                    fnSendEmail(iIndex, fnCallBack);
                                    layer.close(index);
                                }, function () {
                                    fnCallBack();
                                });
                            });
                        } else {
                            setSuccessBtn(oFlow);
                        }
                        if (flag) {
                            oFlow.find('.dropdown-toggle').removeAttr('disabled');
                            oFlow.find('.downsigndoc').off('click').click(function () {
                                fnToDownLoadSignDocuments('ReImport' + (oCurData.ReImports.length > 1 ? '-' + iIndex : ''), $(this).attr('data-index'));
                            });
                        }
                    });
                }

                $('.addnextreimport').off('click').click(function () {
                    fnAddNextReimport();
                    fnAddReturnData();
                    fnAddNextUpload(1);
                });
                $('.ui-icon-close').off('click').on('click', function () {
                    var index = $(this).attr('data-index');
                    switch (index) {
                        case '0':
                            $(this).parents('.form-group').remove();
                            break;
                        case '1':
                            $('.flow').not('#Import').remove();
                            $('#tab5').html('');
                            $('#filepanel_1,#filepanel_2,#filepanel_3,#filepanel_4,#filepanel_5').remove();
                            $('#filecontrol_1,#filecontrol_2,#filecontrol_3,#filecontrol_4,#filecontrol_5').remove();
                            break;
                        case '3':
                            $('#ReImport3,#ReImport4,#ReImport5').remove();
                            $('#ReImportData_3,#ReImportData_4,#ReImportData_5').remove();
                            $('#filepanel_3,#filepanel_4,#filepanel_5').remove();
                            $('#filecontrol_3,#filecontrol_4,#filecontrol_5').remove();
                            break;
                        case '4':
                            $('#ReImport4,#ReImport5').remove();
                            $('#ReImportData_4,#ReImportData_5').remove();
                            $('#filepanel_4,#filepanel_5').remove();
                            $('#filecontrol_4,#filecontrol_5').remove();
                            break;
                        case '5':
                            $('#ReImport5').remove();
                            $('#ReImportData_5').remove();
                            $('#filepanel_5').remove();
                            $('#filecontrol_5').remove();
                            break;
                    }
                    setFlowBox();
                });
                fntBindCheckBoxEvent();
            },
            /**
             * 設定客戶下拉選單
             */
            fnSetCustomersDrop = function () {
                return g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
                    if (res.RESULT) {
                        saCustomers = res.DATA.rel;
                        var saContactors = [];
                        if (saCustomers.length > 0) {
                            sCustomersOptionsHtml = createOptions(saCustomers, 'id', 'text');
                            $('#Supplier').html(sCustomersOptionsHtml).on('change', function () {
                                var sId = this.value;
                                if (sId) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (item) { return item.id === sId; }).First();
                                    saContactors = JSON.parse(oCur.Contactors || '[]');
                                    $('#Contactor').html(createOptions(saContactors, 'guid', 'FullName')).off('change').on('change', function () {
                                        var sContactor = this.value;
                                        if (sContactor) {
                                            CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                                Type: '',
                                                Params: {
                                                    customers: {
                                                        guid: sId
                                                    },
                                                }
                                            }, function (res) {
                                                var oRes = $.parseJSON(res.d);
                                                if (oRes.Contactors) {
                                                    oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                                                    var sContactorName = $(this).find('option:selected').text(),
                                                        oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                                    $('#SupplierEamil').val(oContactor.Email);
                                                    $('#Telephone').val(oContactor.TEL1);
                                                    $('#SitiContactor').val(sContactorName);
                                                    $('#SitiTelephone').val(oContactor.TEL1);
                                                }
                                            });
                                        }
                                        else {
                                            $('#SupplierEamil').val('');
                                            $('#Telephone').val('');
                                            $('#SitiContactor').val('');
                                            $('#SitiTelephone').val('');
                                        }
                                    });
                                }
                                else {
                                    $('#Contactor').html(createOptions([]));
                                    $('#Telephone').val('');
                                }
                            });
                            var saContactors_Agent = [];
                            $('#Agent').html(sCustomersOptionsHtml).on('change', function () {
                                var sId = this.value;
                                if (sId) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == sId; }).First();
                                    saContactors_Agent = JSON.parse(oCur.Contactors || '[]');
                                    $('#AgentContactor').html(createOptions(saContactors_Agent, 'guid', 'FullName')).off('change').on('change', function () {
                                        var sContactor = this.value;
                                        if (sContactor) {
                                            var oContactor = Enumerable.From(saContactors_Agent).Where(function (e) { return e.guid == sContactor; }).First();
                                            $('#AgentEmail,#RefNumberEmail').val(oContactor.Email);
                                            $('#AgentTelephone').val(oContactor.TEL1);
                                        }
                                        else {
                                            $('#AgentEmail,#RefNumberEmail').val('');
                                            $('#AgentTelephone').val('');
                                        }
                                        bRequestStorage = true;
                                    });
                                }
                                else {
                                    $('#AgentContactor').html(createOptions([]));
                                }
                            });
                            $('#ImportPerson').html(sCustomersOptionsHtml);
                            var saNotAuditCurs = Enumerable.From(saCustomers).Where(function (e) { return e.IsAudit == 'Y'; }).ToArray();
                            sCustomersNotAuditOptionsHtml = createOptions(saNotAuditCurs, 'id', 'text');
                        }
                        select2Init();
                    }
                });
            },
            /**
             * 抓取用戶角色
             */
            fnInitfileInput = function (flag) {
                $('#upload' + flag).before('<input class="w100p" multiple type="file" class="displayNone" name="files[]" id="fileInput' + flag + '" />');
                if (!$('#SignatureFileId' + flag).val()) {
                    $('#SignatureFileId' + flag).val(guid());
                }
                $('#fileInput' + flag).filer({
                    limit: 1,
                    maxSize: 50,
                    changeInput: true,
                    showThumbs: true,
                    afterShow: function () {
                        transLang($('#tab8'));
                    },
                    onEmpty: function () {
                        setTimeout(function () {
                            transLang($('#tab8'));
                        }, 100);
                    },
                    captions: {
                        errors: {
                            filesLimit: function (opt) {
                                return i18next.t('common.FileLimit').replace('{limit}', opt.limit);// ╠common.FileLimit⇒最多能上傳{limit}個文件╣
                            }
                        }
                    }
                });

                fnInitUploadEvents();

                transLang($('#tab8'));
            },
            /**
             * 獲取展覽名稱資料
             */
            fnGetPopData_ImportData = function (args) {
                args = args || {};
                args.sortField = args.sortField || 'RefNumber';
                args.sortOrder = args.sortOrder || 'desc';
                args.pageIndex = args.pageIndex || 1;
                args.pageSize = args.pageSize || 10;
                args.ImportBillName = $('#Pop_ImportBillName').val();
                args.ImportBillEName = $('#Pop_ImportBillEName').val();
                args.Supplier = $('#Pop_Supplier').val();
                args.Agent = $('#Pop_Agent').val();

                return g_api.ConnectLite(sQueryPrgId, 'GetExcel', args);
            },
            /**
             * 處理多廠商資料行
             * @param (int) type 編輯類型
             * @return (object)item 當前資料
             */
            fnRenderSupplierInfo = function (type, item) {
                var oDiv = $('<div>', {
                    html: '<div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.Contactor">聯絡人</span>：</label>\
                            <div class="col-sm-4" data-box="Contactor">\
                                <select class="form-control w100p" data-id="Contactor"></select>\
                            </div>\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.Telephone">電話</span>：</label>\
                            <div class="col-sm-4" data-box="Telephone">\
                                <input type="text" class="form-control w100p" data-id="Telephone" maxlength="20" data-keytelno="Y">\
                            </div>\
                         </div>\
                       <div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.ApproachTime">進場時間</span>：</label>\
                            <div class="col-sm-4" data-box="ApproachTime">\
                                <input type="text" class="form-control w100p date" data-id="ApproachTime" maxlength="16">\
                            </div>\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.ExitTime">退場時間</span>：</label>\
                            <div class="col-sm-4" data-box="ExitTime">\
                                <input type="text" class="form-control w100p date" data-id="ExitTime" maxlength="16">\
                            </div>\
                        </div>\
                       <div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.Hall">館別</span>：</label>\
                            <div class="col-sm-4" data-box="Hall">\
                                <select class="form-control w100p" data-id="Hall"></select>\
                            </div>\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.MuseumMumber">攤位</span>：</label>\
                            <div class="col-sm-4" data-box="MuseumMumber">\
                                <input type="text" class="form-control w100p" data-id="MuseumMumber" maxlength="10">\
                            </div>\
                        </div>\
                        <div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.BoxNo">件數</span>：</label>\
                            <div class="col-sm-4" data-box="BoxNo">\
                                <div class="col-sm-6" style="padding:0">\
                                    <input type="text" class="form-control w100p" data-id="BoxNo" data-keyint="Y" maxlength="20" placeholderid="ExhibitionImport_Upd.Instruction_BoxNo">\
                                </div>\
                                <div class="col-sm-6" style="padding:0">\
                                    <input type="text" class="form-control w100p" data-id="Unit" maxlength="10">\
                                </div>\
                            </div>\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.Weight">重量</span>：</label>\
                            <div class="col-sm-4" data-box="Weight">\
                                <div class="input-group input-append">\
                                    <input type="text" class="form-control w100p" data-id="Weight" data-keyintp3="Y" maxlength="20" placeholderid="ExhibitionImport_Upd.Instruction_Weight">\
                                    <span class="input-group-addon cusgroup-addon">\
                                        KG\
                                    </span>\
                                </div>\
                            </div>\
                         </div>\
                         <div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.Volume">材積(CBM)</span>：</label>\
                            <div class="col-sm-4" data-box="Volume">\
                                <div class="input-group input-append">\
                                    <input type="text" class="form-control w100p" data-id="Volume" data-keyintp2="Y" maxlength="50" placeholderid="ExhibitionImport_Upd.Instruction_Volume">\
                                    <span class="input-group-addon cusgroup-addon">\
                                        CBM\
                                    </span>\
                                </div>\
                            </div>\
                            <label class="col-sm-2 control-label"><span data-i18n="ExhibitionImport_Upd.VolumeWeight">材積重(C.W.)</span>：</label>\
                            <div class="col-sm-4" data-box="VolumeWeight">\
                                <div class="input-group input-append">\
                                    <input type="text" class="form-control w100p" data-id="VolumeWeight" data-keyintp1="Y" maxlength="50" placeholderid="ExhibitionImport_Upd.Instruction_VolumeWeight">\
                                    <span class="input-group-addon cusgroup-addon">\
                                        KG\
                                    </span>\
                                </div>\
                            </div>\
                        </div>\
                        <div class="form-group">\
                            <label class="col-sm-2 control-label"><span data-i18n="common.Memo">備註</span>：</label>\
                            <div class="col-sm-10" data-box="Memo">\
                                <textarea class="form-control" data-id="Memo" rows="3" cols="20"></textarea>\
                            </div>\
                        </div>'
                });

                if (type === 1) {
                    oDiv.find('[data-box]').each(function () {
                        var sId = $(this).attr('data-box'),
                            sVal = item[sId];
                        if (sId === 'Contactor') {
                            $(this).text(item['ContactorName']);
                        }
                        else if (sId === 'Hall') {
                            $(this).text(item['HallName']);
                        }
                        else {
                            $(this).text(sVal);
                        }
                        if (sId === 'BoxNo') {
                            $(this).append('  ' + item.Unit);
                        }
                        $(this).addClass('span-text');
                    });
                }
                else {
                    oDiv.find('.date').datetimepicker({
                        changeYear: true,
                        changeMonth: true,
                        altFormat: 'yyyy/MM/dd HH:mm'
                    });
                    oDiv.find('[data-id="BoxNo"],[data-id="Weight"],[data-id="Volume"],[data-id="VolumeWeight"],[data-id="Unit"]').on('blur', function () {
                        var sId = $(this).attr('data-id'),
                            sVal = this.value,
                            iLastVal = 0;

                        if (sId === 'Unit') {
                            $('#Unit').val(sVal);
                        }
                        else {
                            $.each(oCurData.Suppliers, function (idx, supplier) {
                                if (item.guid != supplier.guid) {
                                    iLastVal += parseFloat(supplier[sId] || 0);
                                }
                            });
                            iLastVal += parseFloat(sVal === '' ? 0 : sVal);

                            $('#' + sId).val(iLastVal);
                            if (sId === 'Volume') {
                                var sVolumeWeight = (Math.floor(sVal * 100) / 100 * 167).toFloat(2);
                                $('[data-id="VolumeWeight"]').val(sVolumeWeight);
                                item.VolumeWeight = sVolumeWeight;
                            }
                        }
                    });

                    oDiv.find('[data-id="Hall"]').html($('#Hall').html());

                    if (type === 3) {
                        item.Contactors = [];
                        if (item.SupplierID) {
                            var saCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == item.SupplierID; }).ToArray();
                            if (saCur.length > 0) {
                                item.Contactors = JSON.parse(saCur[0].Contactors || '[]');
                            }
                        }

                        oDiv.find('[data-id="Contactor"]').html(createOptions(item.Contactors, 'guid', 'FullName')).on('change', function () {
                            var sContactor = this.value;
                            if (sContactor) {
                                var oContactor = Enumerable.From(item.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                oDiv.find('[data-id="Telephone"]').val(oContactor.TEL1);
                                oAddItem.Contactor = sContactor;
                                oAddItem.ContactorName = oContactor.FullName;
                                oAddItem.Telephone = oContactor.TEL1;
                            }
                            else {
                                oAddItem.Contactor = '';
                                oAddItem.ContactorName = '';
                                oAddItem.Telephone = '';
                                oDiv.find('[data-id="Telephone"]').val('');
                            }
                        });
                        oDiv.find('[data-id]').each(function () {
                            var sId = $(this).attr('data-id');
                            this.value = item[sId];
                        });
                    }
                    else {
                        oDiv.find('[data-id="Contactor"]').html(createOptions([]));
                    }
                    oDiv.find('[data-id]').on('change', function () {
                        var sId = $(this).attr('data-id'),
                            sVal = this.value;
                        item[sId] = sVal;
                        if (sId === 'Contactor') {
                            if (sVal) {
                                var sText = $(this).find('option:selected').text();
                                item.ContactorName = sText;
                            }
                            else {
                                item.ContactorName = '';
                            }
                        }
                        else if (sId === 'Hall') {
                            if (sVal) {
                                var sText = $(this).find('option:selected').text();
                                item.HallName = sText;
                            }
                            else {
                                item.HallName = '';
                            }
                        }
                    });
                }

                transLang(oDiv);
                return oDiv;
            },
            /*
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
             * 檢核審核時間有沒有符合被鎖定的日期
             * @param {Object}date 審核時間
             */
            fnCheckAuditDate = function (date) {
                var bRel = true,
                    dNow = new Date(),
                    dAuditDate = new Date();
                if (date) {
                    dAuditDate = new Date(date);
                    if (dNow.getMonth() > dAuditDate.getMonth() && dNow.getDate() > 4) {
                        bRel = false;
                    }
                }
                return bRel;
            },
            /**
             * 審核通過後禁用頁面欄位
             * @param {Object}dom 當前區塊
             */
            fnSetDisabled = function (dom, data) {
                //AuditVal（0:初始狀態；1：提交審核（報價/帳單）；2：（報價/帳單）審核（通過）；）
                if (data) {
                    dom.find('.bill-status-box').show();
                    dom.find('.notpass-reason-box').hide();
                    let DraftRecipt = false;
                    switch (data.AuditVal) {
                        case '0':// ╠common.NotAudit⇒未提交審核╣
                            dom.find('.bill-status').text(i18next.t("common.NotAudit")).css('color', 'red');
                            dom.find('.submittoaudit,.synquote,.alreadyaudit').show();
                            dom.find('.billpost,.cancelpost,.writeoff,.canceloff,.reedit,.cancelreedit').hide();
                            dom.find('.alreadyaudit,.cancelaudi,.cancelpost,.writeoff').attr('disabled', 'disabled');
                            if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
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
                            if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
                                dom.find('.reedit').removeAttr('disabled');
                            }
                            if (parent.UserInfo.UsersDown.indexOf(oCurData.ResponsiblePerson) > -1 || parent.UserInfo.UsersBranch.indexOf(oCurData.ResponsiblePerson) > -1 || parent.SysSet.BillAuditor.indexOf(parent.UserInfo.MemberID) > -1) {
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
                            if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
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
                            if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
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
                        case '4':// ╠common.NotPass⇒已銷帳╣
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
                            if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {//如果有資料且是null或者N
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
             * 綁定費用項目
             * @param {HTMLELment} dom 當前標籤
             * @param {HTMLELment} parentdata 父層標籤
             * @param {Object} data 當前資料
             * @param {String} flag 綁定狀態
             */
            fnBindFeeItem = function (dom, parentdata, data, flag) {
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
                    iOldBoxTotal = parseFloat((oFinancial.find('.boxtotal').val() || '0').replaceAll(',', '')),
                    oTab = dom.parents('.tab-pane');

                $.each(data.FeeItems, function (idx, item) {
                    sFeeItemsHtml += '<tr>'
                        + '<td class="wcenter">' + (idx + 1) + '</td>'
                        + '<td class="wcenter">' + item.FinancialCode + '</td>'
                        + '<td>' + (!item.FinancialCostStatement ? item.Memo : item.FinancialCostStatement + (!item.Memo ? '' : '（' + item.Memo + '）')) + '</td>'
                        + '<td class="wcenter">' + item.FinancialCurrency + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialUnitPrice, 2) + '</td>'
                        + '<td class="wcenter">' + item.FinancialNumber + '</td>'
                        + '<td>' + item.FinancialUnit + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialAmount, 2) + '</td>'
                        + '<td class="wcenter">' + item.FinancialExchangeRate + '</td>'
                        + '<td class="wright">' + fMoney(item.FinancialTWAmount, 2) + '</td>'
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
                            $('#tab4 .actualsum').val(fMoney(iTaxSubtotal, 2, data.Currency));
                            if (parentdata.ActualCost.AmountTaxSum > parentdata.EstimatedCost.AmountTaxSum) {
                                $('#tab4 #warnning_tips').show();
                            }
                            else {
                                $('#tab4 #warnning_tips').hide();
                            }
                        }
                        else if (oTab[0].id === 'tab10') {
                            oFinancial.find('.actualsum').val(fMoney(iTaxSubtotal, 2, data.Currency));
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
                        else if (oTab[0].id === 'tab9') {
                            //增加退運報價/預估成本-預估成本累加
                            let Return_Estimatedcostsum = $('#tab9 .return_estimatedcostsum').val();
                            let Current_Return_Estimatedcostsum = parseFloat((Return_Estimatedcostsum || '0').toString().replaceAll(',', ''));
                            $('#tab9 .return_estimatedcostsum').val(fMoney(Current_Return_Estimatedcostsum + iSubtotal, 2, data.Currency));
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
                            oTab.css({ 'padding-top': 40 });
                            if (data.AuditVal !== '6') {
                                let LastRowActualsum = parseFloat($('#tab3 .amountsum').val().replaceAll(',', '')) - iOldBoxTotal;
                                $('#tab3 .amountsum').val(fMoney(LastRowActualsum + TabTipUntaxtotal, 2, FeeItemCurrency));
                                $('#tab4 .amountsum').val($('#tab3 .amountsum').val());
                            }
                        }
                        else if (oTab[0].id === 'tab9') {
                            // 每筆退運帳單的預估成本
                            oFinancial.find('.topshowsum').show();
                            oFinancial.find('.estimatedcostsum').val(fMoney(parentdata.EstimatedCost.AmountSum, 2, data.Currency));
                            if (data.AuditVal !== '6') {
                                //退運帳單加總
                                let LastRowActualsum = parseFloat(oFinancial.find('.amountsum').val().replaceAll(',', '')) - iOldBoxTotal;
                                oFinancial.find('.amountsum').val(fMoney(LastRowActualsum + TabTipUntaxtotal, 2, FeeItemCurrency));
                                $('.bill-box-' + data.BillNO).find('.amountsum').val(oFinancial.find('.amountsum').val());
                                //增加退運報價/預估成本-帳單金額累加(有先後順序，無法對調)
                                let LastRow_Return_Amountsum = parseFloat($('#tab9 .return_amountsum').val().replaceAll(',', ''));
                                $('#tab9 .return_amountsum').val(fMoney(LastRow_Return_Amountsum + TabTipUntaxtotal, 2, FeeItemCurrency));

                            }
                        }
                        break;
                }
                /*計算$$*/
                if (oTab[0].id === 'tab3') {
                    if (data.KeyName === 'Bill')
                        fnCalcuBillsFee(oFinancial, '.BillForeignCurrency', '.BillMainCurrency', data.Currency, data.ExchangeRate);
                    else
                        fnCalcuQuotationFee(oFinancial.find('.QuotationForeignCurrency'), oFinancial.find('.QuotationMainCurrency'),
                            parentdata.Quote.QuotationOrBillingCurrency, parentdata.Quote.AccountingExchangeRate);
                }
                else if (oTab[0].id === 'tab9') {
                    if (data.KeyName === 'Bill')
                        fnCalcuBillsFee(oFinancial, '.ReturnBillForeignCurrency', '.ReturnBillMainCurrency',
                            data.Currency, data.ExchangeRate);
                    else {
                        fnCalcuQuotationFee(oFinancial.find('.ReturnQuotationForeignCurrency'), oFinancial.find('.ReturnQuotationMainCurrency'),
                            parentdata.ReturnQuotationOrBillingCurrency, parentdata.ReturnAccountingExchangeRate);
                    }
                }

                dom.parents('.financial').find('.plusfeeitem').prop('disabled', false);
                fnSetDisabled(oFinancial, data);

                inputChange(oFinancial.find('.input-value'), data);

                if (data.KeyName === 'ActualCost') {
                    var saBillPayers = function () {
                        var saRetn = [];
                        $.each(parentdata.Bills, function (idx, bill) {
                            if (!bill.VoidReason) {
                                var sPayer = '',
                                    oCur = {};
                                if (bill.Payer) {
                                    oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == bill.Payer; }).First();
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
                        iOrderBy = Enumerable.From(data.FeeItems).Where(function (e) { return e.guid == sGuid; }).First().OrderBy;

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
             * 添加費用項目
             * @param (Object)that 當前dom對象
             * @return (Object)data 當前費用項目
             */
            fnPlusFeeItem = function (that, parentdata, feeinfo, currency) {
                var oFinancial = $(that).parents('.financial'),
                    oTable = oFinancial.find('tbody'),
                    sId = oTable.attr('data-id'),
                    sBillNO = oTable.attr('data-billno') || '',
                    sMainCurrency = oFinancial.find('[data-id="Currency"]').val() || FeeItemCurrency;

                oTable.find('tr').not('.fee-add').find('.jsgrid-cancel-edit-button').click();
                var fnSum = function () {
                    var iPrice = oUnitPrice.attr('data-value') || 0,
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
                                var oCurrency = Enumerable.From(saCurrency).Where(function (e) { return e.id == sCurrencyId; }).First();
                                oExchangeRate.val(oCurrency.Correlation || '').change();
                            }
                        }
                    }).css('cssText', 'width:80px !important').val(sMainCurrency),
                    oUnitPrice = $('<input />', { class: 'form-control w100p', 'data-type': 'int', 'data-name': 'int', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
                    oNumber = $('<input />', { class: 'form-control w100p', 'data-type': 'int', 'data-name': 'int', keyup: function () { fnSum(); }, change: function () { fnSum(); } }),
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
                        bRequestStorage = true;
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
                                    oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == bill.Payer; }).First();
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
                            var oBillPayer = Enumerable.From(saBillPayers).Where(function (e) { return e.id == sBill; }).First();
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
                moneyInput($('[data-type="int"]'), 2, true);
                oTR.find('.' + Selector + ' option:first').remove();
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
            * 批次添加費用項目&收藏
            * @param：(Object)that 當前dom對象
            * @return：(Object)data 當前費用項目
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
             */
            fnPushBill = function (data, parentid, bRetn) {
                var fnBill = function (billno) {
                    var oNewBill = {};
                    oNewBill.guid = guid();
                    oNewBill.IsRetn = bRetn ? 'Y' : 'N';
                    oNewBill.parentid = parentid || '';
                    oNewBill.KeyName = 'Bill';
                    oNewBill.AuditVal = '0';
                    oNewBill.BillNO = billno || sDataId;
                    oNewBill.BillCreateDate = newDate();
                    oNewBill.BillFirstCheckDate = '';
                    oNewBill.BillCheckDate = '';
                    oNewBill.Currency = 'NTD';
                    oNewBill.ExchangeRate = 1;
                    if (bRetn) {
                        var sRtnQuotationOrBillingCurrency = $('#ReturnQuotationOrBillingCurrency-' + parentid + ' option:selected');
                        oNewBill.ExchangeRate = sRtnQuotationOrBillingCurrency.attr('Correlation');
                        oNewBill.Currency = sRtnQuotationOrBillingCurrency.val();
                    }
                    else {
                        var sQuotationOrBillingCurrency = $('#QuotationOrBillingCurrency option:selected');
                        oNewBill.ExchangeRate = sQuotationOrBillingCurrency.attr('Correlation');
                        oNewBill.Currency = sQuotationOrBillingCurrency.val();
                    }

                    oNewBill.Advance = 0;
                    oNewBill.Memo = data.Quote.Memo || '';
                    oNewBill.FeeItems = clone(data.Quote.FeeItems);
                    oNewBill.InvoiceNumber = '';
                    oNewBill.InvoiceDate = '';
                    oNewBill.ReceiptNumber = '';
                    oNewBill.ReceiptDate = '';
                    oNewBill.Payer = '';
                    oNewBill.CustomerCode = '';
                    oNewBill.UniCode = '';
                    oNewBill.Contactor = '';
                    oNewBill.ContactorName = '';
                    oNewBill.AgentContactor = '';
                    oNewBill.AgentContactorName = '';
                    oNewBill.Telephone = '';
                    oNewBill.Number = bRetn ? $('#ReImportData1_Number').val() || '' : $('#BoxNo').val();
                    oNewBill.Unit = bRetn ? $('#ReImportData1_Unit').val() || '' : $('#Unit').val();
                    oNewBill.Weight = bRetn ? ($('#ReImportData1_ShippedWeight').val() || '').replace(/[^\d.-]/g, '') : $('#Weight').val();
                    oNewBill.Volume = bRetn ? ($('#ReImportData1_HeavyTruckBack').val() || '').replace(/[^\d.-]/g, '') : $('#Volume').val();
                    oNewBill.ReImportNum = 1;
                    data.Bills.push(oNewBill);
                };

                return g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: parent.UserInfo.OrgID + 'I',
                    Flag: 'MinYear',
                    Len: 3,
                    Str: sServiceCode,
                    AddType: sServiceCode,
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        fnBill(res.DATA.rel);
                    }
                    else {
                        showMsg(i18next.t('message.CreateBill_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.CreateBill_Failed'), 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
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
             * 初始化:退運報價/帳單幣別、匯率
             */
            fnInitialReturnAccountCurrency = function (tabName, Guid) {
                $('#ReturnQuotationOrBillingCurrency' + Guid).html(sAccountingCurrencyOptionsHtml).on('change', function () {
                    var sRtnQuotationOrBillingCurrency = $('#ReturnQuotationOrBillingCurrency' + Guid + ' option:selected');
                    var sCurrencyID = this.value;
                    var sExchangeRate = sRtnQuotationOrBillingCurrency.attr('Correlation');
                    var fExchangeRate = parseFloat(sExchangeRate);
                    $('#ReturnAccountingExchangeRate' + Guid).val(sExchangeRate);
                    bRequestStorage = true;//要變更儲存
                    var MainCurrency = fnCheckMainOrForeignCurrency(this.value);
                    var TitleAttr = '';
                    if (MainCurrency) {
                        TitleAttr = 'common.MainCurrencyNontaxedAmountV2';
                        $(tabName + ' .ReturnQuotationForeignCurrency').hide();
                    }
                    else {
                        TitleAttr = 'common.ForeignCurrencyNontaxedAmountV2';
                        $(tabName + ' .ReturnQuotationForeignCurrency').show();
                    }
                    $(tabName + ' .ReturnQuotationAmountTiltle' + Guid).attr('data-i18n', TitleAttr).text(i18next.t(TitleAttr));
                    //儲存資料(forJson)
                    $.each(oCurData.ReturnBills, function (idx, item) {
                        if (Guid.indexOf(item.guid) > -1) {
                            item.ReturnQuotationOrBillingCurrency = sCurrencyID;
                            item.ReturnAccountingExchangeRate = sExchangeRate;
                        }
                    });
                    //重算價格
                    fnCalcuQuotationFee($(tabName + ' .ReturnQuotationForeignCurrency'), $(tabName + ' .ReturnQuotationMainCurrency'), sCurrencyID, fExchangeRate);
                });
                //loading data
                $.each(oCurData.ReturnBills, function (idx, item) {
                    var SuffixSelector = '-' + item.guid;
                    $(tabName + ' #ReturnQuotationOrBillingCurrency' + SuffixSelector).val(item.ReturnQuotationOrBillingCurrency);
                    $(tabName + ' #ReturnAccountingExchangeRate' + SuffixSelector).val(item.ReturnAccountingExchangeRate);
                    $(tabName + ' #ReturnQuotationOrBillingCurrency' + SuffixSelector).change();
                });

            },
            /**
             * 綁定會計區塊
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
                    if (oCurData.EstimatedCost.FeeItems.length === 0 && !oCurData.EstimatedCost.Memo) {
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
                        ImportBillNO: sDataId,
                        Quote: oCurData.Quote,
                        EstimatedCost: oCurData.EstimatedCost
                    }, function (res) {
                        if (res.RESULT) {
                            fnSetDisabled($('#tab3 .quoteandprecost'), oCurData.Quote);
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
                                var fnAuditForQuote = function () {
                                    g_api.ConnectLite(sProgramId, 'AuditForQuote', {
                                        ImportBillNO: sDataId,
                                        Quote: oCurData.Quote,
                                        EstimatedCost: oCurData.EstimatedCost,
                                        Bills: oCurData.Bills
                                    }, function (res) {
                                        if (res.RESULT) {
                                            showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                            if (oCurData.Quote.AuditVal === '2') {
                                                fnBindBillLists();
                                                fnBindFeeItem($('#tab4 [data-id="actualcost-pre-box"]'), oCurData, oCurData.EstimatedCost, true);
                                                $('#tab4 .estimatedcost-memo').text(oCurData.EstimatedCost.Memo || '');
                                            }
                                            parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                                            fnSetDisabled($('#tab3 .quoteandprecost'), oCurData.Quote);
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
                                },
                                    sNotPassReason = $('#NotPassReason').val();

                                if (this.id === 'audit_pass') {
                                    oCurData.Quote.AuditVal = '2';
                                    oCurData.EstimatedCost.AuditVal = '2';
                                    oCurData.Quote.NotPassReason = '';
                                    oCurData.EstimatedCost.NotPassReason = '';
                                    fnPushBill(oCurData, null, false).done(function () {
                                        fnAuditForQuote();
                                    });
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
                                        fnAuditForQuote();
                                    }
                                }
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
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
                                ImportBillNO: sDataId,
                                Bills: oCurData.Bills,
                                Bill: bill
                            }, function (res) {
                                if (res.RESULT) {
                                    $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text(bill.BillCheckDate);
                                    fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             */
            fnBillCancelAudit = function (bill) {
                var sBillCheckDate = bill.BillCheckDate;
                bill.AuditVal = '0';
                bill.BillCheckDate = '';
                g_api.ConnectLite(sProgramId, 'CancelAudit', {
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text('');
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            * 過帳
            * @param bill(object)帳單資料
            */
            fnBillPost = function (bill) {
                bill.AuditVal = '5';
                bill.CreateDate = newDate();
                g_api.ConnectLite(sProgramId, 'BillPost', {
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnBillCancelPost = function (bill) {
                bill.AuditVal = '2';
                g_api.ConnectLite(sProgramId, 'BillCancelPost', {
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             */
            fnBillWriteOff = function (bill) {
                bill.AuditVal = '4';
                bill.BillWriteOffDate = newDate();
                g_api.ConnectLite(sProgramId, 'WriteOff', {
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnBillCancelWriteOff = function (bill) {
                bill.AuditVal = '5';
                bill.BillWriteOffDate = '';
                g_api.ConnectLite(sProgramId, 'CancelWriteOff', {
                    ImportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
                                ImportBillNO: sDataId,
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
                        ImportBillNO: sDataId,
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
            */
            fnBillReEdit = function (bill) {
                bill.AuditVal = '7';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { ImportBillNO: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnBillCancelReEdit = function (bill) {
                bill.AuditVal = '1';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { ImportBillNO: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             * 列印事件
             * @param：templid (string) 模版id
             * @param：action (string) 動作標識
             * @param：bill (objec) 帳單資料
             */
            fnPrint = function (templid, action, bill) {
                var bReceipt = action.indexOf('Receipt') > -1,
                    fnToPrint = function (idx, paydatetext) {
                        g_api.ConnectLite(sProgramId, bReceipt ? 'PrintReceipt' : 'PrintBill', {
                            ImportBillNO: sDataId,
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
                                    sTitle = bReceipt ? 'common.Receipt_Preview' : 'common.Bill_Preview';// ╠common.Receipt_Preview⇒收據預覽╣ ╠common.Bill_Preview⇒帳單預覽╣
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
                                showMsg(i18next.t(action.indexOf('Print_') > -1 ? 'common.Preview_Failed' : 'common.DownLoad_Failed') + '<br>' + res.MSG, 'error');
                            }
                        }, function () {
                            // ╠common.Preview_Failed⇒預覽失敗╣ ╠common.DownLoad_Failed⇒下載失敗╣
                            showMsg(i18next.t(action.indexOf('Print_') > -1 ? 'common.Preview_Failed' : 'common.DownLoad_Failed'), 'error'); //預覽失敗/下載失敗
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
             * 綁定帳單
             */
            fnBindBillLists = function () {
                var oBillsBox = $('.bills-box');
                oBillsBox.html('');
                $('#tab3 .amountsum').val(0);
                if (oCurData.Bills.length > 0) {
                    oCurData.Bills = Enumerable.From(oCurData.Bills).OrderBy("x=>x.BillCreateDate").ToArray();
                    $.each(oCurData.Bills, function (idx, bill) {
                        bill.Index = idx + 1;
                        bill.Advance = bill.Advance || 0;
                        var sHtml = $("#temp_billbox").render([bill]);
                        oBillsBox.append(sHtml);
                        var oBillBox = $('.bill-box-' + bill.BillNO);
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
                            }
                            else {
                                TitleAttr = 'common.ForeignCurrencyNontaxedAmountV2';
                                oBillBox.find('.BillMainCurrencyAdd [data-id="plusfeeitem"]').hide();
                                oBillBox.find('.BillForeignCurrency').show();

                            }
                            bill.Currency = sCurrencyId;
                            bill.ExchangeRate = oCurrency.Correlation || '';
                            oBillBox.find('[data-id="ExchangeRate"]').val(oCurrency.Correlation || '');
                            oBillBox.find('.BillAmountTiltle').attr('data-i18n', TitleAttr).text(i18next.t(TitleAttr));
                            let ExchangeRate = oBillBox.find('[data-id="ExchangeRate"]').val();
                            fnCalcuBillsFee(oBillBox, '.BillForeignCurrency', '.BillMainCurrency', sCurrencyId, ExchangeRate);
                            bRequestStorage = true;
                        }).val(bill.Currency);
                        oBillBox.find('[data-id="Currency"]').change();
                        //oBillBox.find('[data-id="Currency"] option:first').remove();
                        oBillBox.find('[data-id="Payer"]').html(sCustomersNotAuditOptionsHtml).val(bill.Payer);
                        setTimeout(function () {
                            oBillBox.find('[data-id="Payer"]').select2({ width: '250px' });
                        }, 1000);

                        if (bill.Payer) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == bill.Payer; }),
                                saContactors = [];
                            if (oCur.Count() > 0) {
                                saContactors = JSON.parse(oCur.First().Contactors || '[]');
                            }
                            oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).val(bill.Contactor).off('change').on('change', function (e) {
                                var sContactor = this.value;
                                if (sContactor) {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            customers: {
                                                guid: bill.Payer
                                            },
                                        }
                                    }, function (res) {
                                        var oRes = $.parseJSON(res.d);
                                        if (oRes.Contactors) {
                                            oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                                            var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                            bill.ContactorName = oContactor.FullName;
                                            bill.Contactor = sContactor;
                                        }
                                    });
                                }
                                else {
                                    bill.ContactorName = '';
                                    bill.Contactor = '';
                                }
                                bRequestStorage = true;
                            });
                            oBillBox.find('[data-id="Contactor"]').select2();
                        }
                        else {
                            oBillBox.find('[data-id="Contactor"]').html(createOptions([]));
                        }
                        oBillBox.find('[data-id="Telephone"]').val(bill.Telephone);
                        moneyInput(oBillBox.find('[data-id="Advance"]'), 2);
                        moneyInput(oBillBox.find('[data-id="mAdvance"]'), 0);
                        oBillBox.find('[data-id="Advance"]').val(bill.Advance);
                        SetBillPrepayEvent(oBillBox, bill);
                        oBillBox.find('[data-id="Number"]').val(bill.Number).on('keyup blur', function (e) {
                            this.value = this.value.replace(/\D/g, '');
                        });
                        oBillBox.find('[data-id="Unit"]').val(bill.Unit);
                        oBillBox.find('[data-id="Weight"]').val(bill.Weight).on('keyup blur', function (e) {
                            keyIntp(e, this, 3);
                        });
                        oBillBox.find('[data-id="Volume"]').val(bill.Volume).on('keyup blur', function (e) {
                            keyIntp(e, this, 2);
                        });
                        oBillBox.find('[data-id="ExchangeRate"]').val(bill.ExchangeRate || 1.00);
                        fnBindFeeItem($('[data-id=bill_fees_' + bill.BillNO + ']'), oCurData, bill);
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
                                var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == sCustomerId; }).First();
                                saContactors = JSON.parse(oCur.Contactors || '[]');
                                oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).off('change').on('change', function () {
                                    var sContactor = this.value;
                                    if (sContactor) {
                                        CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                            Type: '',
                                            Params: {
                                                customers: {
                                                    guid: sCustomerId
                                                },
                                            }
                                        }, function (res) {
                                            var oRes = $.parseJSON(res.d);
                                            if (oRes.Contactors) {
                                                oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                                                var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                                bill.ContactorName = oContactor.FullName;
                                                bill.Contactor = sContactor;
                                            }
                                        });
                                    }
                                    else {
                                        bill.ContactorName = '';
                                        bill.Contactor = '';
                                    }
                                    bRequestStorage = true;
                                });
                                oBillBox.find('[data-id="Telephone"]').val(oCur.Telephone);
                                oBillBox.find('[data-id="Contactor"]').select2({ width: '250px' });
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
                                                importexhibition: {
                                                    values: { Bills: JSON.stringify(oCurData.Bills) },
                                                    keys: { ImportBillNO: sDataId }
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
                    });
                    $('#bills_add').removeAttr('disabled');
                }
                fnSetPermissions();//設置權限
                $('.bills-add,#topshow_box').show();//當審通過之後才顯示新增帳單按鈕
                transLang(oBillsBox);
            },
            /**
             * 設置操作權限
             */
            fnSetPermissions = function () {
                if (parent.UserInfo.roles.indexOf('Admin') === -1) {
                    if ((parent.UserInfo.roles.indexOf('CDD') > -1 && (oCurData.ResponsiblePerson === parent.UserID || oCurData.DepartmentID === parent.UserInfo.DepartmentID)) || parent.UserInfo.roles.indexOf('CDD') === -1 || parent.SysSet.CDDProUsers.indexOf(parent.UserID) > -1) {//報關作業
                        $('[href="#tab3"],[href="#tab4"],[href="#tab9"],[href="#tab10"]').parent().show();
                    }
                    else {
                        $('[href="#tab3"],[href="#tab4"],[href="#tab9"],[href="#tab10"]').parent().hide();
                    }
                    if (!(parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser)) {//其他
                        $('#tab3,#tab9').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost').attr('disabled', 'disabled');
                        $('#tab3,#tab9').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Business') > -1) {//業務
                        $('#tab4,#tab10').find(':input,button,textarea').attr('disabled', 'disabled');
                        $('#tab4,#tab10').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                        $('#tab1,#tab2,#tab3,#tab5,#tab6,#tab7,#tab8,#tab9').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost,.importfeeitem,.plusfeeitem').attr('disabled', 'disabled');
                        $('#tab3,#tab9').find('.icon-p').addClass('disabled');
                    }
                }
            },
            /**------------------------帳單部分---------------------------End*/

            /**------------------------退運帳單部分---------------------------Start*/

            /**
             * 添加新帳單
             */
            fnRenderReturnBills = function () {
                oCurData.ReturnBills = Enumerable.From(oCurData.ReturnBills).OrderBy("x=>x.CreateDate").ToArray();
                var oReturnQuoteBox = $('.return-quote-box'),
                    oReturnActualcostBox = $('.return-actualcost-box'),
                    sReturnQuoteBoxHtml = $("#temp_return_quote_box").render(oCurData.ReturnBills),
                    sReturnActualcostBoxHtml = $("#temp_return_actualcost_box").render(oCurData.ReturnBills);
                oReturnQuoteBox.html(sReturnQuoteBoxHtml);
                oReturnActualcostBox.html(sReturnActualcostBoxHtml);

                $.each(oCurData.ReturnBills, function (idx, Returns) {
                    var oFieldset_Return = $('.fieldset-' + Returns.guid);
                    fnBindFeeItem(oFieldset_Return.find('[data-id="quote-box"]'), Returns, Returns.Quote);//綁定報價
                    fnBindFeeItem(oFieldset_Return.find('[data-id="estimatedcost-box"]'), Returns, Returns.EstimatedCost);//綁定預估成本
                    if (Returns.EstimatedCost.AuditVal && Returns.EstimatedCost.AuditVal === '2') {//業務審核完
                        fnBindFeeItem(oFieldset_Return.find('[data-id="actualcost-pre-box"]'), Returns, Returns.EstimatedCost, true);
                        oFieldset_Return.find('.estimatedcost-memo').text(Returns.EstimatedCost.Memo || '');
                    }
                    fnBindFeeItem(oFieldset_Return.find('[data-id="actualcost-box"]'), Returns, Returns.ActualCost);//實際成本
                    fnInitialReturnAccountCurrency('.fieldset-' + Returns.guid, '-' + Returns.guid);//會計用匯率
                    oFieldset_Return.find('[data-source="quote"]').text(Returns.Quote.Memo || '');
                    oFieldset_Return.find('[data-source="estimatedcost"]').text(Returns.EstimatedCost.Memo || '');
                    oFieldset_Return.find('[data-source="actualcost"]').text(Returns.ActualCost.Memo || '');

                    oFieldset_Return.find('.plusfeeitem').not('disabled').off('click').on('click', function () {
                        fnPlusFeeItem(this, Returns);
                    });

                    oFieldset_Return.find('.importfeeitem').not('disabled').off('click').on('click', function () {
                        var sType = $(this).attr('data-type');
                        fnImportFeeitems(sType, oFieldset_Return, Returns);
                    });

                    oFieldset_Return.find('.copyfeeitem').not('disabled').off('click').on('click', function () {
                        var sType = $(this).attr('data-type');
                        fnCopyFeeitems(sType, oFieldset_Return, Returns);
                    });

                    oFieldset_Return.find('.plusfeeitemstar').not('disabled').off('click').on('click', function () {
                        fnPlusFeeItemStar(this, oFieldset_Return.find('[data-id="quote-box"]'), Returns);
                    });

                    oFieldset_Return.find('.estimated_submitaudit').not('disabled').off('click').on('click', function () {
                        if (Returns.EstimatedCost.FeeItems.length === 0 && !Returns.EstimatedCost.Memo) {
                            showMsg(i18next.t("message.EstimatedCostMemo_Required")); // ╠message.EstimatedCostMemo_Required⇒預估成本沒有費用明細，請至少填寫備註╣
                            return false;
                        }
                        if (!Returns.ReturnAccountingExchangeRate || !Returns.ReturnQuotationOrBillingCurrency) {
                            showMsg(i18next.t("message.QuotationOrBillingCurrencyField_Required")); // ╠message.QuotationOrBillingCurrencyField_Required⇒報價/帳單幣或匯率未選擇╣
                            return false;
                        }
                        Returns.Quote.AuditVal = '1';
                        Returns.EstimatedCost.AuditVal = '1';
                        g_api.ConnectLite(sProgramId, 'ReturnToAuditForQuote', {
                            ImportBillNO: sDataId,
                            ReturnBills: oCurData.ReturnBills,
                            SourceID: Returns.Quote.guid,
                            Index: idx + 1
                        }, function (res) {
                            if (res.RESULT) {
                                fnSetDisabled(oFieldset_Return.find('.quoteandprecost'), Returns.Quote);
                                showMsg(i18next.t("message.ToAudit_Success"), 'success'); // ╠message.ToAudit_Success⇒提交審核成功╣
                                parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                            }
                            else {
                                Returns.Quote.AuditVal = '0';
                                Returns.EstimatedCost.AuditVal = '0';
                                showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                            }
                        }, function () {
                            Returns.Quote.AuditVal = '0';
                            Returns.EstimatedCost.AuditVal = '0';
                            showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                        });
                    });
                    oFieldset_Return.find('.estimated_audit').not('disabled').off('click').on('click', function () {
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
                            success: function (layero, index) {
                                $('.pop-box :button').click(function () {
                                    var fnAuditForQuote = function () {
                                        g_api.ConnectLite(sProgramId, 'ReturnAuditForQuote', {
                                            ImportBillNO: sDataId,
                                            ReturnBills: oCurData.ReturnBills,
                                            AuditVal: Returns.Quote.AuditVal,
                                            SourceID: Returns.Quote.guid,
                                            Index: idx + 1
                                        }, function (res) {
                                            if (res.RESULT) {
                                                showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                                if (Returns.Quote.AuditVal === '2') {
                                                    fnRenderReturnBills();
                                                }
                                                else {
                                                    fnSetDisabled(oFieldset_Return.find('.quoteandprecost'), Returns.Quote);
                                                }
                                                parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                                            }
                                            else {
                                                Returns.Quote.AuditVal = '1';
                                                Returns.EstimatedCost.AuditVal = '1';
                                                showMsg(i18next.t('message.Audit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                            }
                                        }, function () {
                                            Returns.Quote.AuditVal = '1';
                                            Returns.EstimatedCost.AuditVal = '1';
                                            showMsg(i18next.t('message.Audit_Failed'), 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                        });
                                    },
                                        sNotPassReason = $('#NotPassReason').val();
                                    if (this.id === 'audit_pass') {
                                        Returns.Quote.AuditVal = '2';
                                        Returns.EstimatedCost.AuditVal = '2';
                                        Returns.Quote.NotPassReason = '';
                                        Returns.EstimatedCost.NotPassReason = '';
                                        fnPushBill(Returns, Returns.guid, true).done(function () {
                                            fnAuditForQuote();
                                        });
                                    }
                                    else {
                                        if (!sNotPassReason) {
                                            showMsg(i18next.t("message.NotPassReason_Required")); // ╠message.NotPassReason_Required⇒請填寫不通過原因╣
                                            return false;
                                        }
                                        else {
                                            Returns.Quote.AuditVal = '3';
                                            Returns.EstimatedCost.AuditVal = '3';
                                            Returns.Quote.NotPassReason = sNotPassReason;
                                            Returns.EstimatedCost.NotPassReason = sNotPassReason;
                                            fnAuditForQuote();
                                        }
                                    }
                                    layer.close(index);
                                });
                                transLang(layero);
                            }
                        });
                    });
                    oFieldset_Return.find('.estimated_synquote').not('disabled').off('click').on('click', function () {
                        Returns.EstimatedCost.FeeItems = clone(Returns.Quote.FeeItems);
                        $.each(Returns.EstimatedCost.FeeItems, function (idx, item) {
                            item.FinancialUnitPrice = 0;
                            item.FinancialNumber = 0;
                            item.FinancialAmount = 0;
                            item.FinancialTWAmount = 0;
                            item.FinancialTaxRate = '0%';
                            item.FinancialTax = 0;
                            item.CreateUser = parent.UserID;
                            item.CreateDate = newDate(null, true);
                        });
                        fnBindFeeItem(oFieldset_Return.find('[data-id="estimatedcost-box"]'), Returns, Returns.EstimatedCost);
                    });

                    $.each(Returns.Bills, function (i, bill) {
                        bill.Advance = bill.Advance || 0;
                        var oBillsBox = $('.return-bill-' + bill.parentid),
                            sHtml = $("#temp_returnbillbox").render([bill]);
                        oBillsBox.append(sHtml);
                        var oBillBox = $('.bill-box-' + bill.BillNO);
                        oBillBox.find('[data-id="Currency"]').html(sAccountingCurrencyOptionsHtml).on('change', function () {
                            var sCurrencyId = this.value,
                                oCurrency = Enumerable.From(saAccountingCurrency).Where(function (e) { return e.ArgumentID == sCurrencyId; }).FirstOrDefault();
                            if (oCurrency === undefined)
                                oCurrency = {};
                            let TitleAttr = '';
                            let MainCurrency = fnCheckMainOrForeignCurrency(sCurrencyId);
                            if (MainCurrency) {
                                TitleAttr = 'common.MainCurrencyNontaxedAmountV2';
                                oBillBox.find('.ReturnBillMainCurrencyAdd [data-id="plusfeeitem"]').show();
                                oBillBox.find('.ReturnBillForeignCurrency').hide();
                            }
                            else {
                                TitleAttr = 'common.ForeignCurrencyNontaxedAmountV2';
                                oBillBox.find('.ReturnBillMainCurrencyAdd [data-id="plusfeeitem"]').hide();
                                oBillBox.find('.ReturnBillForeignCurrency').show();

                            }
                            oBillBox.find('.ReturnBillAmountTiltle-' + bill.guid).attr('data-i18n', TitleAttr).text(i18next.t(TitleAttr));
                            bill.Currency = sCurrencyId;
                            bill.ExchangeRate = oCurrency.Correlation || '';
                            oBillBox.find('[data-id="ExchangeRate"]').val(oCurrency.Correlation || '');
                            let ExchangeRate = oBillBox.find('[data-id="ExchangeRate"]').val();
                            fnCalcuBillsFee(oBillBox, '.ReturnBillForeignCurrency', '.ReturnBillMainCurrency', sCurrencyId, ExchangeRate);
                            bRequestStorage = true;
                        }).val(bill.Currency);
                        oBillBox.find('[data-id="Currency"]').change();
                        //oBillBox.find('[data-id="Currency"] option:first').remove();
                        oBillBox.find('[data-id="Payer"]').html(sCustomersNotAuditOptionsHtml).val(bill.Payer);
                        setTimeout(function () {
                            oBillBox.find('[data-id="Payer"]').select2({ width: '250px' });
                        }, 1000);

                        if (bill.Payer) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == bill.Payer; }),
                                saContactors = [];
                            if (oCur.Count() > 0) {
                                saContactors = JSON.parse(oCur.First().Contactors || '[]');
                            }
                            oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).val(bill.Contactor).off('change').on('change', function (e) {
                                var sContactor = this.value;
                                if (sContactor) {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            customers: {
                                                guid: bill.Payer
                                            },
                                        }
                                    }, function (res) {
                                        var oRes = $.parseJSON(res.d);
                                        if (oRes.Contactors) {
                                            oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                                            var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                            bill.ContactorName = oContactor.FullName;
                                            bill.Contactor = sContactor;
                                        }
                                    });
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
                        moneyInput(oBillBox.find('[data-id="Advance"]'), 2);
                        moneyInput(oBillBox.find('[data-id="mAdvance"]'), 0);
                        oBillBox.find('[data-id="Advance"]').val(bill.Advance);
                        SetBillPrepayEvent(oBillBox, bill);
                        oBillBox.find('[data-id="Number"]').val(bill.Number).on('keyup blur', function (e) {
                            this.value = this.value.replace(/\D/g, '');
                        });
                        oBillBox.find('[data-id="Unit"]').val(bill.Unit);
                        oBillBox.find('[data-id="Weight"]').val(bill.Weight).on('keyup blur', function (e) {
                            keyIntp(e, this, 3);
                        });
                        oBillBox.find('[data-id="Volume"]').val(bill.Volume).on('keyup blur', function (e) {
                            keyIntp(e, this, 2);
                        });
                        oBillBox.find('[data-id="ExchangeRate"]').val(bill.ExchangeRate || 1.00);
                        fnBindFeeItem($('[data-id=bill_fees_' + bill.BillNO + ']'), Returns, bill);
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

                        inputChange(oBillBox.find('[data-source="bill"]'), bill);
                        var saContactors = [];
                        oBillBox.find('[data-id="Payer"]').on('change', function () {
                            var sCustomerId = this.value;
                            if (sCustomerId) {
                                var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == sCustomerId; }).First();
                                saContactors = JSON.parse(oCur.Contactors || '[]');

                                oBillBox.find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).off('change').on('change', function () {
                                    var sContactor = this.value;
                                    if (sContactor) {
                                        CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                            Type: '',
                                            Params: {
                                                customers: {
                                                    guid: sCustomerId
                                                },
                                            }
                                        }, function (res) {
                                            var oRes = $.parseJSON(res.d);
                                            if (oRes.Contactors) {
                                                oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                                                var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                                bill.ContactorName = oContactor.FullName;
                                                bill.Contactor = sContactor;
                                            }
                                        });
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
                                    fnPlusFeeItem(that, Returns, null, bill.Currency);
                                    break;
                                case 'createreceiptnumber'://產生收據號碼
                                    fnGetReceiptNumber(that, bill).done(function () {
                                        CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                            Params: {
                                                importexhibition: {
                                                    values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                                                    keys: { ImportBillNO: sDataId }
                                                }
                                            }
                                        });
                                    });
                                    break;
                                case 'bill_submitaudit'://提交審核
                                    fnReturnBillToAudit(bill, that);
                                    break;
                                case 'bill_audit'://主管審核
                                    fnReturnBillAudit(bill);
                                    break;
                                case 'bill_cancelaudit'://取消審核
                                    fnReturnBillCancelAudit(bill);
                                    break;
                                case 'bill_post'://過帳
                                    fnReturnBillPost(bill);
                                    break;
                                case 'bill_cancelpost'://取消過帳
                                    fnReturnBillCancelPost(bill);
                                    break;
                                case 'bill_writeoff'://銷帳
                                    fnReturnBillWriteOff(bill);
                                    break;
                                case 'bill_canceloff'://取消銷帳
                                    fnReturnBillCancelWriteOff(bill);
                                    break;
                                case 'bill_void'://作廢
                                    fnReturnBillVoid(bill);
                                    break;
                                case 'bill_delete'://刪除
                                    fnReturnBillDelete(bill);
                                    break;
                                case 'bill_reedit'://抽單
                                    fnReturnBillReEdit(bill);
                                    break;
                                case 'bill_cancelreedit'://取消抽單
                                    fnReturnBillCancelReEdit(bill);
                                    break;
                            }
                        });
                    });

                    if ((parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) && Returns.Quote.AuditVal === '2') {
                        oFieldset_Return.find('.returnbills_add').removeAttr('disabled').click(function () {
                            fnPushBill(Returns, Returns.guid, true).done(function () {
                                fnRenderReturnBills();
                            });
                        });
                        oFieldset_Return.find('.returnbills-add-box').show();
                    }
                });

                fnSetPermissions();//設置權限
                $('#topshow_box_return').show();//當審通過之後才顯示總金額
                transLang($('#tab9,#tab10'));
            },
            /**
             * 添加新帳單
             */
            fnPushReturnBill = function () {
                var oNewReturnBill = {};
                oNewReturnBill.guid = guid();
                oNewReturnBill.index = oCurData.ReturnBills.length + 1;
                oNewReturnBill.Quote = { guid: guid(), KeyName: 'Quote', AuditVal: '0', FeeItems: [] };
                oNewReturnBill.EstimatedCost = { guid: guid(), KeyName: 'EstimatedCost', AuditVal: '0', FeeItems: [] };
                oNewReturnBill.ActualCost = { guid: guid(), KeyName: 'ActualCost', AuditVal: '0', FeeItems: [] };
                oNewReturnBill.Bills = [];
                oNewReturnBill.CreateDate = newDate();
                oNewReturnBill.ReturnQuotationOrBillingCurrency = ''
                oNewReturnBill.ReturnAccountingExchangeRate = '';
                oCurData.ReturnBills.push(oNewReturnBill);
                fnRenderReturnBills();
            },
            fnReturnBillToAudit = function (bill, el) {
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
                if (!bill.ReImportNum) {
                    sMsg += i18next.t("message.ReFlow_required"); // ╠message.ReFlow_required⇒請輸入退運流程╣
                }
                if (elBillBox.find('.jsgrid-update-button').length > 0) {
                    sMsg += i18next.t("message.DataEditing") + '<br/>'; // ╠message.DataEditing⇒該賬單處於編輯中╣
                }
                if (sMsg) {
                    showMsg(sMsg); // 必填欄位
                    return;
                }

                bill.AuditVal = '1';
                g_api.ConnectLite(sProgramId, 'ReturnToAuditForBill', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             */
            fnReturnBillAudit = function (bill) {
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
                            g_api.ConnectLite(sProgramId, 'ReturnAuditForBill', {
                                ImportBillNO: sDataId,
                                ReturnBills: oCurData.ReturnBills,
                                Bill: bill
                            }, function (res) {
                                if (res.RESULT) {
                                    $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text(bill.BillCheckDate);
                                    fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             */
            fnReturnBillCancelAudit = function (bill) {
                var sBillCheckDate = bill.BillCheckDate;
                bill.AuditVal = '0';
                bill.BillCheckDate = '';
                g_api.ConnectLite(sProgramId, 'ReturnCancelAudit', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text('');
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            * 過帳
            * @param bill(object)帳單資料
            */
            fnReturnBillPost = function (bill) {
                bill.AuditVal = '5';
                bill.CreateDate = newDate();
                g_api.ConnectLite(sProgramId, 'ReturnBillPost', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnReturnBillCancelPost = function (bill) {
                bill.AuditVal = '2';
                g_api.ConnectLite(sProgramId, 'ReturnBillCancelPost', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
             */
            fnReturnBillWriteOff = function (bill) {
                bill.AuditVal = '4';
                bill.BillWriteOffDate = newDate();
                g_api.ConnectLite(sProgramId, 'ReturnWriteOff', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnReturnBillCancelWriteOff = function (bill) {
                bill.AuditVal = '5';
                bill.BillWriteOffDate = '';
                g_api.ConnectLite(sProgramId, 'ReturnCancelWriteOff', {
                    ImportBillNO: sDataId,
                    ReturnBills: oCurData.ReturnBills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            * @param (object)bill 帳單資料
            */
            fnReturnBillVoid = function (bill) {
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
                            g_api.ConnectLite(sProgramId, 'ReturnBillVoid', {
                                ImportBillNO: sDataId,
                                ReturnBills: oCurData.ReturnBills,
                                Bill: bill,
                                LogData: fnGetBillLogData(bill)
                            }, function (res) {
                                if (res.RESULT) {
                                    layer.close(index);
                                    showMsg(i18next.t("message.Void_Success"), 'success'); // ╠message.Void_Success⇒作廢成功╣
                                    fnRenderReturnBills();
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
            * @param (object)bill 帳單資料
            */
            fnReturnBillDelete = function (bill) {
                // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
                layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                    var saNewBills = [];
                    $.each(oCurData.ReturnBills, function (idx, Returns) {
                        if (Returns.guid === bill.parentid) {
                            var saNewBills = [];
                            $.each(Returns.Bills, function (i, _bill) {
                                if (_bill.BillNO !== bill.BillNO) {
                                    saNewBills.push(_bill);
                                }
                            });
                            Returns.Bills = saNewBills;
                            return false;
                        }
                    });
                    g_api.ConnectLite(sProgramId, 'ReturnBillDelete', {
                        ImportBillNO: sDataId,
                        ReturnBills: oCurData.ReturnBills,
                        Bill: bill,
                        LogData: fnGetBillLogData(bill)
                    }, function (res) {
                        if (res.RESULT) {
                            showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                            fnRenderReturnBills();
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
            */
            fnReturnBillReEdit = function (bill) {
                bill.AuditVal = '7';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                            keys: { ImportBillNO: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            */
            fnReturnBillCancelReEdit = function (bill) {
                bill.AuditVal = '1';
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        importexhibition: {
                            values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                            keys: { ImportBillNO: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO), bill);
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
            /**------------------------退運帳單部分---------------------------End*/

            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return;
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
                            return;
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
                    case "Toolbar_Imp":

                        fnImport();

                        break;
                    case "Toolbar_Void":

                        if (fnCheckBillEffective(oCurData.Bills) || fnCheckRtnBillEffective(oCurData.ReturnBills)) {
                            showMsg(i18next.t("message.OpmNotToVoid")); // ╠message.OpmNotToVoid⇒已建立有效的賬單，暫時不可作廢╣
                            return;
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
                    case "MailTrackingNumber":
                        fnSendTrackingNumberEmail();
                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /**
             * 初始化 function
             */
            init = function () {
                var saCusBtns = [];
                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'MailTrackingNumber',
                        value: 'common.MailTrackingNumber'
                    }];
                }
                else {
                    saCusBtns = [{
                        id: 'Toolbar_Imp',
                        value: 'common.Toolbar_Imp'// ╠common.Toolbar_Imp⇒匯入╣
                    }];
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true,
                    tabClick: function (el) {
                        switch (el.id) {
                            case 'litab3':
                            case 'litab4':
                                if (!$(el).data('action')) {
                                    fnBindFinancial();
                                    $('#litab3').data('action', true);
                                    $('#litab4').data('action', true);
                                    fnOpenAccountingArea($('div .OnlyForAccounting'), parent.UserInfo.roles);
                                }
                                break;
                            case 'litab9':
                            case 'litab10':
                                if (!$(el).data('action')) {
                                    if (oCurData.ReturnBills.length > 0) {
                                        fnRenderReturnBills();
                                    }
                                    $('#litab9').data('action', true);
                                    $('#litab10').data('action', true);
                                    fnOpenAccountingArea($('div .OnlyForAccounting'), parent.UserInfo.roles);
                                }
                                break;
                            case 'litab11':
                                if (!$(el).data('action')) {
                                    var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 120;
                                    $("#jsGrid").jsGrid({
                                        width: "100%",
                                        height: "auto",
                                        autoload: true,
                                        //filtering: true,
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
                                                name: "RowIndex", title: 'common.RowNumber', width: 40, align: "center"
                                            },
                                            {
                                                name: "SupplierName", title: 'common.SupplierName', width: 250, inserting: true, editing: false, validate: { validator: 'required', message: i18next.t('common.Supplier_required') },
                                                itemTemplate: function (val, item) {
                                                    return !val ? item.SupplierEName : item.SupplierName;
                                                },
                                                insertTemplate: function () {
                                                    var oSelect = $('<select/>', {
                                                        change: function () {
                                                            var sCustomerId = this.value;
                                                            if (sCustomerId) {
                                                                var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == sCustomerId; }).First();
                                                                oAddItem.guid = guid();
                                                                oAddItem.SupplierID = oCur.id;
                                                                oAddItem.CustomerNO = oCur.CustomerNO;
                                                                oAddItem.UniCode = oCur.UniCode;
                                                                oAddItem.SupplierName = oCur.textcn;
                                                                oAddItem.SupplierEName = oCur.texteg;
                                                                oAddItem.CreateDate = new Date().formate("yyyy/MM/dd HH:mm:ss");
                                                                var saContactors = JSON.parse(oCur.Contactors || '[]');
                                                                oSelect.parent().next().find('[data-id="Contactor"]').html(createOptions(saContactors, 'guid', 'FullName')).on('change', function () {
                                                                    var sContactor = this.value;
                                                                    if (sContactor) {
                                                                        var oContactor = Enumerable.From(saContactors).Where(function (e) { return e.guid == sContactor; }).First();
                                                                        oSelect.parent().next().find('[data-id="Telephone"]').val(oContactor.TEL1);
                                                                        oAddItem.Contactor = sContactor;
                                                                        oAddItem.ContactorName = oContactor.FullName;
                                                                        oAddItem.Telephone = oContactor.TEL1;
                                                                    }
                                                                    else {
                                                                        oAddItem.Contactor = '';
                                                                        oAddItem.ContactorName = '';
                                                                        oAddItem.Telephone = '';
                                                                        oSelect.parent().next().find('[data-id="Telephone"]').val('');
                                                                    }
                                                                });
                                                                oSelect.parent().next().find('[data-id]').each(function () {
                                                                    var sId = $(this).attr('data-id'),
                                                                        sVal = this.value;
                                                                    oAddItem[sId] = sVal;
                                                                    if (sId === 'Contactor') {
                                                                        if (sVal) {
                                                                            var sText = $(this).find('option:selected').text();
                                                                            oAddItem.ContactorName = sText;
                                                                        }
                                                                        else {
                                                                            oAddItem.ContactorName = '';
                                                                        }
                                                                    }
                                                                    else if (sId === 'Hall') {
                                                                        if (sVal) {
                                                                            var sText = $(this).find('option:selected').text();
                                                                            oAddItem.HallName = sText;
                                                                        }
                                                                        else {
                                                                            oAddItem.HallName = '';
                                                                        }
                                                                    }
                                                                });
                                                            }
                                                            else {
                                                                oSelect.parent().next().find('[data-id="Contactor"]').html(createOptions([]));
                                                                oAddItem = {};
                                                            }
                                                            bRequestStorage = true;
                                                        }
                                                    });
                                                    setTimeout(function () {
                                                        oSelect.html(sCustomersOptionsHtml).select2({ width: '250px' });
                                                    }, 1500);
                                                    return this.insertControl = oSelect;
                                                },
                                                insertValue: function () {
                                                    return this.insertControl.val();
                                                },
                                                editTemplate: function (val, item) {
                                                    var oSelect = $('<select/>', {
                                                        html: sCustomersOptionsHtml,
                                                        change: function () {
                                                            var sCustomerId = this.value;
                                                            if (sCustomerId) {
                                                                var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == sCustomerId; }).First();
                                                                item.Contactors = JSON.parse(oCur.Contactors || '[]');
                                                                oSelect.parent().next().find('[data-id="Contactor"]').html(createOptions(item.Contactors, 'guid', 'FullName')).on('change', function () {
                                                                    var sContactor = this.value;
                                                                    if (sContactor) {
                                                                        var oContactor = Enumerable.From(item.Contactors).Where(function (e) { return e.guid == sContactor; }).First();
                                                                        oSelect.parent().next().find('[data-id="Telephone"]').val(oContactor.TEL1);
                                                                        item.Contactor = sContactor;
                                                                        item.ContactorName = oContactor.FullName;
                                                                        item.Telephone = oContactor.TEL1;
                                                                    }
                                                                    else {
                                                                        item.Contactor = '';
                                                                        item.ContactorName = '';
                                                                        item.Telephone = '';
                                                                        oSelect.parent().next().find('[data-id="Telephone"]').val('');
                                                                    }
                                                                });
                                                            }
                                                            else {
                                                                oSelect.parent().next().find('[data-id="Contactor"]').html(createOptions([]));
                                                            }
                                                            bRequestStorage = true;
                                                        }
                                                    }).val(item.SupplierID)
                                                    setTimeout(function () {
                                                        oSelect.select2({ width: '250px' });
                                                    }, 100);
                                                    return this.insertControl = oSelect;
                                                },
                                                editValue: function () {
                                                    return this.insertControl.val();
                                                }
                                            },
                                            {
                                                name: "Info", title: 'common.Other', width: 600,
                                                itemTemplate: function (val, item) {
                                                    return fnRenderSupplierInfo(1, item);
                                                },
                                                insertTemplate: function (val, item) {
                                                    return fnRenderSupplierInfo(2, oAddItem);
                                                },
                                                editTemplate: function (val, item) {
                                                    return fnRenderSupplierInfo(3, item);
                                                }
                                            },
                                            {
                                                title: 'common.SignDocuments', width: 300, align: "center",
                                                itemTemplate: function (val, item) {
                                                    var oDiv = $('<div>', {
                                                        html: '<div class="form-group">\
                                                                  <button type="button" class="btn-custom green downsigndoc" data-index="0" data-i18n="common.Toolbar_Preview">預覽\
                                                                  </button>\
                                                                  <button type="button" class="btn-custom green downsigndoc" data-index="1" data-i18n="common.DownLoad_Word">下載Word\
                                                                  </button>\
                                                            </div>',
                                                        click: function (e) {
                                                        }
                                                    });
                                                    oDiv.find('.downsigndoc').off('click').click(function (e) {
                                                        fnToDownLoadSignDocuments('Import', $(this).attr('data-index'), item.SupplierID);
                                                        e.stopPropagation();
                                                    });
                                                    return oDiv;
                                                }
                                            },
                                            { type: "control", deleteButton: parent.UserInfo.IsManager ? true : false }
                                        ],
                                        controller: {
                                            loadData: function (args) {
                                                if (oCurData.Suppliers.length > 0) {
                                                    oCurData.Suppliers = Enumerable.From(oCurData.Suppliers).OrderBy("x=>x.CreateDate").ToArray();
                                                    $.each(oCurData.Suppliers, function (idx, filter) {
                                                        filter.RowIndex = idx + 1;
                                                    });
                                                }
                                                return {
                                                    data: oCurData.Suppliers,
                                                    itemsCount: oCurData.Suppliers.length //data.length
                                                };
                                            },
                                            insertItem: function (args) {
                                                oCurData.Suppliers.push(oAddItem);
                                                oAddItem = {};
                                            },
                                            updateItem: function (args) {
                                                $.each(oCurData.Suppliers, function (e, item) {
                                                    if (item.guid == args.guid) {
                                                        item = args;
                                                    }
                                                });
                                            },
                                            deleteItem: function (args) {
                                                var saNewList = [];
                                                $.each(oCurData.Suppliers, function (idx, item) {
                                                    if (item.guid != args.guid) {
                                                        saNewList.push(item);
                                                    }
                                                });
                                                oCurData.Suppliers = saNewList;
                                            }
                                        },
                                        onInit: function (args) {
                                            oGrid = args.grid;
                                        }
                                    });
                                    oGrid.loadData();
                                    $(el).data('action', true);
                                }
                                break;
                        }
                    }
                });

                fnSGMod();//上海駒驛須移除選項

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
                            ShowId: true,
                            CallBack: function (data) {
                                sTransportOptionsHtml = createOptions(data, 'id', 'text', true);
                            }
                        },
                        {
                            ArgClassID: 'Hall',
                            Select: $('#Hall')
                        },
                        {
                            ArgClassID: 'Port',
                            CallBack: function (data) {
                                saPort = data;
                                $('.quickquery-city').on('keyup', function () {
                                    this.value = this.value.toUpperCase();
                                }).on('blur', function () {
                                    var sId = this.value,
                                        oPort = Enumerable.From(saPort).Where(function (e) { return e.id == sId; });
                                    if (oPort.Count() === 1) {
                                        $(this).parent().next().next().find(':input').val(oPort.First().text);
                                    }
                                }).autocompleter({
                                    // marker for autocomplete matches
                                    highlightMatches: true,
                                    // object to local or url to remote search
                                    source: saPort,
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
                    ])])
                    .done(function (res) {
                        if (res && res[0].RESULT === 1) {
                            var oRes = res[0].DATA.rel,
                                saReImport = [];
                            fnGetCurrencyThisYear(oRes.CreateDate).done(function () {
                                if (typeof oRes.Import === 'string') {
                                    oRes.Import = JSON.parse(oRes.Import || '{}');
                                }
                                oRes.ReImports = (oRes.ReImports) ? JSON.parse(oRes.ReImports) : [];
                                oRes.ReturnBills = (oRes.ReturnBills) ? JSON.parse(oRes.ReturnBills) : [];
                                oRes.Suppliers = (oRes.Suppliers) ? JSON.parse(oRes.Suppliers) : [];
                                oRes.ReturnLoan = (oRes.ReturnLoan) ? JSON.parse(oRes.ReturnLoan) : {};
                                oRes.TaxInformation = (oRes.TaxInformation) ? JSON.parse(oRes.TaxInformation) : {};

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
                                if (oCurData.Supplier) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == oCurData.Supplier; }),
                                        saContactors = [];
                                    if (oCur.Count() > 0) {
                                        saContactors = JSON.parse(oCur.First().Contactors || '[]');
                                    }
                                    $('#Contactor').html(createOptions(saContactors, 'guid', 'FullName')).val(oCurData.Contactor);
                                }
                                else {
                                    $('#Contactor').html(createOptions([]));
                                }
                                if (oCurData.Agent) {
                                    var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id == oCurData.Agent; }),
                                        saContactors = [];
                                    if (oCur.Count() > 0) {
                                        saContactors = JSON.parse(oCur.First().Contactors || '[]');
                                    }
                                    $('#AgentContactor').html(createOptions(saContactors, 'guid', 'FullName')).val(oCurData.AgentContactor);
                                }
                                else {
                                    $('#AgentContactor').html(createOptions([]));
                                }
                                $.each(oRes.ReImports, function (idx, reImport) {
                                    var sIndex = idx + 1;
                                    oCurData['SignatureFileId' + sIndex] = reImport.SignatureFileId || guid();
                                    oCurData['ReImport' + sIndex] = reImport.ReImport || {};
                                    oCurData['ReImportData' + sIndex] = reImport.ReImportData || {};
                                    saReImport.push({ index: sIndex });
                                });
                                if (oCurData.ReImports.length == 1) {
                                    fnAddNextReimport('one');
                                    fnAddReturnData('one');
                                    fnAddNextUpload(3, 'one');
                                }
                                else if (oCurData.ReImports.length > 1) {
                                    fnAddNextReimport(saReImport);
                                    fnAddReturnData(saReImport);
                                    fnAddNextUpload(3, saReImport);
                                }

                                setFormVal(oForm, oCurData);
                                setFlowBox(true);
                                fnInitfileInput('');
                                fnGetSignatureFile('');
                                $.each(saReImport, function (idx) {
                                    var sIndex = idx + 1;
                                    fnInitfileInput(sIndex);
                                    fnGetSignatureFile(sIndex);
                                });
                                $('#ExhibitionDateStart').val(newDate(oCurData.ExhibitionDateStart, 'date', true));
                                $('#ExhibitionDateEnd').val(newDate(oCurData.ExhibitionDateEnd, 'date', true));
                                if (oCurData.SupplierType === 'M') {
                                    setTimeout(function () {
                                        $('[name=SupplierType][value=M]').click();
                                    }, 100);
                                }
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
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

                                if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
                                    $('#bills_add').click(function () {
                                        fnPushBill(oCurData, null, false).done(function () {
                                            fnBindBillLists();
                                        });
                                    });
                                    $('#returnquote_add').removeAttr('disabled').click(function () {
                                        fnPushReturnBill();
                                    });
                                    $('.returnquote-add').show();
                                }
                                moneyInput($('[data-type="int"]'), 0);

                                var Authorized = ExhibitionBillAuthorize(oCurData);
                                if (Authorized) {
                                    $('[href="#tab3"],[href="#tab4"],[href="#tab9"],[href="#tab10"]').parent().show();
                                    if (sGoTab) {
                                        $('#litab' + sGoTab).find('a').click();
                                        if (sBillNOGO && $('.bill-box-' + sBillNOGO).length > 0) {
                                            goToJys($('.bill-box-' + sBillNOGO));
                                        }
                                    }
                                }
                                else {
                                    $('[href="#tab3"],[href="#tab4"],[href="#tab9"],[href="#tab10"]').parent().hide();
                                }
                            });
                        }
                        else
                            fnGetCurrencyThisYear(new Date());

                    });

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
                                if (parent.OrgID === 'TG' && !$('#Hall').val()) {
                                    $('#Hall').val(oExhibition.ExhibitionAddress);
                                }
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

                $.validator.addMethod("compardate", function (value, element, parms) {
                    if (new Date(value) < new Date($('#ExhibitionDateStart').val())) {
                        return false;
                    }
                    return true;
                });
                oValidator = $("#form_main").validate({
                    ignore: '',
                    rules: {
                        SupplierEamil: {
                            email: true
                        },
                        AgentEmail: {
                            email: true
                        }
                    },
                    messages: {
                        SupplierEamil: i18next.t("message.IncorrectEmail"),// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                        AgentEmail: i18next.t("message.IncorrectEmail")// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    }
                });

                $('#Volume').on('blur', function () {
                    var sVal = this.value;
                    $('#VolumeWeight').val((Math.floor(sVal * 100) / 100 * 167).toFloat(2));
                });

                $('#litab11').hide();
                $('[name=SupplierType]').on('click', function () {
                    if (this.value === 'S') {
                        $('#litab11,.agent-required').hide();
                        $('.supplier-box,.supplier-required').show();
                        $('[aria-labelledby="select2-Agent-container"]').removeClass("highlight");
                        $('#Agent').removeAttr('required');
                        $('#Supplier').attr('required', true);
                    }
                    else {
                        $('#litab11,.agent-required').show();
                        $('.supplier-box,.supplier-required').hide();
                        $('#Supplier,#Contactor,#Telephone,#SupplierEamil').val('');
                        $('#Supplier').removeAttr('required').trigger("change");
                        $('#Agent').attr('required', true);
                        $('[aria-labelledby="select2-Agent-container"]').addClass("highlight");
                    }
                });

                $.timepicker.dateRange($('#ExhibitionDateStart'), $('#ExhibitionDateEnd'),
                    {
                        minInterval: (1000 * 60 * 60 * 24 * 1), // 1 days
                        changeYear: true,
                        changeMonth: true
                    }
                );
                $.timepicker.datetimeRange($('#ApproachTime'), $('#ExitTime'),
                    {
                        minInterval: (1000 * 60 * 60 * 24 * 1), // 1 days
                        changeYear: true,
                        changeMonth: true
                    }
                );
            };

        init();
    };

require(['base', 'select2', 'formatnumber', 'autocompleter', 'jquerytoolbar', 'timepicker', 'jsgrid', 'ajaxfile', 'filer', 'common_opm', 'util'], fnPageInit, 'timepicker');