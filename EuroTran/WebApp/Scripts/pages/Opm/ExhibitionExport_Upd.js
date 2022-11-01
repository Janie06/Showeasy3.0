'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('ExportBillNO'),
    sFlag = getUrlParam('Flag'),
    sGoTab = getUrlParam('GoTab'),
    sBillNOGO = getUrlParam('BillNO'),
    sCheckId = sDataId,
    sOrganizers = [],
    sSelectedOrganizers = [],
    MaxOrganizerCount = $(".Organizer").length + 1,//organizer count
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
            sTransportOptionsHtml = '',
            oAddItem = {},
            oCurSupplierData = {},
            oPrintMenu = {},
            oCurData = {},
            saGridData = [],
            saCustomers = [],
            saBatchArr = [],
            saPort = [],
            saCurrency = [],
            saFeeClass = [],
            saAccountingCurrency = [],
            nowResponsiblePerson = '',
            /**
             * 獲取資料
             * @return {Object} ajax物件
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
                                $('#VoidReason').text(oRes.VoidReason);
                                if (oRes.Organizer.indexOf("]") === -1)
                                    oRes.Organizer = '["' + oRes.Organizer + '"]';
                                sOrganizers = $.parseJSON(oRes.Organizer);
                                if (oRes.IsVoid === 'Y') {
                                    $('.voidreason').show();
                                    $('#Toolbar_Void').attr({ 'id': 'Toolbar_OpenVoid', 'data-i18n': 'common.Toolbar_OpenVoid' });
                                }
                                else {
                                    $('.voidreason').hide();
                                    $('#Toolbar_OpenVoid').attr({ 'id': 'Toolbar_Void', 'data-i18n': 'common.Toolbar_Void' });
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
                    $('#AgentContactor').html(createOptions([]));
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
                    $('#VoidReason').text(oRes.VoidReason);
                    if (oRes.VoidReason) { $('.voidreason').show(); } else { $('.voidreason').hide(); }
                    getPageVal(); //緩存頁面值，用於清除
                });
            },
            /**
             * 獲取貨物狀態
             * @param {Object} data 當前流程對象
             * @return {String} 當前流程名稱
             */
            fnGetFlowStatus = function (data) {
                var sFlowStatus = '';
                if (data.ReturnType !== '') {
                    sFlowStatus = '退運';
                }
                else if (data.ClearanceData.ServiceBooth.Checked) {
                    sFlowStatus = '送達攤位';
                }
                else if (data.ClearanceData.WaitingApproach.Checked) {
                    sFlowStatus = '等待進場';
                }
                else if (data.ClearanceData.CargoRelease.Checked) {
                    sFlowStatus = '貨物放行';
                }
                else if (data.ClearanceData.GoodsArrival.Checked) {
                    sFlowStatus = '貨物抵港';
                }
                else if (data.ExportData.ExportRelease.Checked) {
                    sFlowStatus = '出口放行';
                }
                else if (data.ExportData.CustomsDeclaration.Checked) {
                    sFlowStatus = '報關作業';
                }
                else if (data.ExportData.Intowarehouse.Checked) {
                    sFlowStatus = '貨物進倉';
                }
                else if (data.ExportData.ReceiveFile.Checked) {
                    sFlowStatus = '已收文件';
                }
                return sFlowStatus;
            },
            /**
             * 新增資料
             * @param {String} flag 新增或儲存後新增
             * @return {Object} ajax物件
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm),
                    fnGetAdd = function () {
                        data.Exhibitors = JSON.stringify(saGridData);
                        data.Organizer = JSON.stringify(sOrganizers);
                        CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                            Params: { exportexhibition: data }
                        }, function (res) {
                            if (res.d > 0) {
                                bRequestStorage = false;
                                if (flag === 'add') {
                                    showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&ExportBillNO=' + sDataId); // ╠message.Save_Success⇒新增成功╣
                                }
                                else {
                                    showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                                }
                            }
                            else {
                                showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        });
                    };
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.IsVoid = 'N';
                data.DepartmentID = sDeptCode;
                data.Flow_Status = oCurData.Flow_Status || '';

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
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                data.ReturnBills = JSON.stringify(data.ReturnBills);
                if (data.AgentContactor) {
                    data.AgentContactorName = $('#AgentContactor option:selected').text();
                }
                else {
                    data.AgentContactorName = '';
                }
                if (data.ExhibitionNO) {
                    data.ExportBillName = $('#ExhibitionNO option:selected').text();
                }
                else {
                    data.ExhibitionName = '';
                }

                if (!data.DocumentDeadline) delete data.DocumentDeadline;
                if (!data.ClosingDate) delete data.ClosingDate;
                if (!data.ETC) delete data.ETC;
                if (!data.ETD) delete data.ETD;
                if (!data.ETA) delete data.ETA;
                if (!data.ReminderAgentExecutionDate) delete data.ReminderAgentExecutionDate;
                if (!data.PreExhibitionDate) delete data.PreExhibitionDate;
                if (!data.ExitDate) delete data.ExitDate;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;

                data.ExportBillNO = sDataId = guid();
                return g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: 'C' + parent.UserInfo.OrgID + 'E',
                    Flag: 'MinYear',
                    Len: 3,
                    Str: sServiceCode,
                    AddType: sServiceCode,
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        data.RefNumber = res.DATA.rel;
                        var bRelease = true,
                            sSuppliers = '',
                            saPost = [];
                        $.each(saGridData, function (idx, item) {
                            item.RefSupplierNo = data.RefNumber + rndnum(3);
                            if (!item.VoidContent) {
                                if (bRelease && !item.VoidContent && (item.ExportData && !item.ExportData.ExportRelease.Checked) || !item.ExportData) {
                                    bRelease = false;
                                }
                            }
                            sSuppliers += item.SupplierName + '|';
                        });
                        data.Suppliers = sSuppliers;
                        data.Release = saGridData.length > 0 ? (bRelease ? 'Y' : 'N') : 'N';
                        fnGetAdd();
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed") + '<br>' + res.MSG, 'error');// ╠message.Save_Failed⇒新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                });
            },
            /**
             * 修改資料
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm),
                    bRelease = true,
                    sSuppliers = '';
                data = packParams(data, 'upd');
                data.Organizer = JSON.stringify(sOrganizers);


                $.each(saGridData, function (idx, item) {
                    if (!item.VoidContent) {
                        if (bRelease && (item.ExportData && !item.ExportData.ExportRelease.Checked) || !item.ExportData) {
                            bRelease = false;
                        }
                    }
                    sSuppliers += item.SupplierName + '|';
                });
                data.Suppliers = sSuppliers;
                data.Release = saGridData.length > 0 ? (bRelease ? 'Y' : 'N') : 'N';
                data.IsVoid = oCurData.IsVoid;
                data.Flow_Status = oCurData.Flow_Status || '';
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
                data.Quote = JSON.stringify(data.Quote);
                data.EstimatedCost = JSON.stringify(data.EstimatedCost);
                data.ActualCost = JSON.stringify(data.ActualCost);
                data.Bills = JSON.stringify(data.Bills);
                data.ReturnBills = JSON.stringify(data.ReturnBills);
                data.Exhibitors = JSON.stringify(saGridData);

                if (data.AgentContactor) {
                    data.AgentContactorName = $('#AgentContactor option:selected').text();
                }
                else {
                    data.AgentContactorName = '';
                }
                if (data.ExhibitionNO) {
                    data.ExportBillName = $('#ExhibitionNO option:selected').text();
                }
                else {
                    data.ExhibitionName = '';
                }

                delete data.ExportBillNO;
                if (!data.DocumentDeadline) delete data.DocumentDeadline;
                if (!data.ClosingDate) delete data.ClosingDate;
                if (!data.ETC) delete data.ETC;
                if (!data.ETD) delete data.ETD;
                if (!data.ETA) delete data.ETA;
                if (!data.ReminderAgentExecutionDate) delete data.ReminderAgentExecutionDate;
                if (!data.PreExhibitionDate) delete data.PreExhibitionDate;
                if (!data.ExitDate) delete data.ExitDate;
                if (!data.ExhibitionDateStart) delete data.ExhibitionDateStart;
                if (!data.ExhibitionDateEnd) delete data.ExhibitionDateEnd;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        exportexhibition: {
                            values: data,
                            keys: { ExportBillNO: sDataId }
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
                        exportexhibition: {
                            ExportBillNO: sDataId
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
                                exportexhibition: {
                                    values: data,
                                    keys: { ExportBillNO: sDataId }
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
                        exportexhibition: {
                            values: data,
                            keys: { ExportBillNO: sDataId }
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
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['200px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                              <button type="button" data-i18n="ExhibitionImport_Upd.BusinessTrackingSchedule" id="Export_BusinessTrackingScheduleOne" class="btn-custom green">業務追蹤進度表</button>\
                          </div>',// ╠common.BusinessTrackingSchedule⇒請立即安排付款╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            fnExcel({
                                pageIndex: 1,
                                pageSize: 100000,
                                ToExcelType: sToExcelType
                            }, idx);
                        });
                        transLang(layero);
                    }
                });
            },
            /**
             * 匯入資料
             */
            fnImport = function () {
                var oConfig = {
                    Get: fnGetPopData_ImportData,
                    SearchFields: [
                        { id: "Pop_RefNumber", type: 'text', i18nkey: 'ExhibitionExport_Upd.RefNumber' },
                        { id: "Pop_ExportBillName", type: 'text', i18nkey: 'ExhibitionExport_Upd.ExportBillName' },
                        { id: "Pop_ExportBillEName", type: 'text', i18nkey: 'ExhibitionExport_Upd.ExportBillEName' },
                        { id: "Pop_Agent", type: 'text', i18nkey: 'ExhibitionExport_Upd.Agent' }
                    ],
                    Fields: [
                        { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 50 },
                        { name: "RefNumber", title: 'ExhibitionExport_Upd.RefNumber', width: 100 },
                        { name: "ExportBillName", title: 'ExhibitionExport_Upd.ExportBillName', width: 200 },
                        { name: "ExportBillEName", title: 'ExhibitionExport_Upd.ExportBillEName', width: 200 },
                        { name: "AgentName", title: 'ExhibitionExport_Upd.Agent', width: 200 }
                    ],
                    Callback: function (item) {
                        var oData = {};

                        oData.ResponsiblePerson = item.ResponsiblePerson;
                        oData.ExhibitionNO = item.ExhibitionNO;
                        oData.ExportBillName = item.ExportBillName;
                        oData.ExportBillEName = item.ExportBillEName;
                        oData.ExhibitionDateStart = item.ExhibitionDateStart;
                        oData.ExhibitionDateEnd = item.ExhibitionDateEnd;
                        oData.Organizer = item.Organizer;
                        oData.Agent = item.Agent;
                        oData.AgentContactor = item.AgentContactor;
                        oData.AgentTelephone = item.AgentTelephone;
                        oData.AgentEamil = item.AgentEamil;

                        setFormVal(oForm, oData);
                        if (oData.Agent) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === oData.Agent; }).First(),
                                saContactors = JSON.parse(oCur.Contactors || '[]');
                            $('#AgentContactor').html(createOptions(saContactors, 'guid', 'FullName')).val(oData.AgentContactor);
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
             * 獲取展覽名稱資料
             * @param {Object} args 查詢參數
             * @return {Object} ajax
             */
            fnGetPopData_ImportData = function (args) {
                args = args || {};
                args.sortField = args.sortField || 'RefNumber';
                args.sortOrder = args.sortOrder || 'desc';
                args.pageIndex = args.pageIndex || 1;
                args.pageSize = args.pageSize || 10;
                args.RefNumber = $('#Pop_RefNumber').val();
                args.ExportBillName = $('#Pop_ExportBillName').val();
                args.ExportBillEName = $('#Pop_ExportBillEName').val();
                args.Agent = $('#Pop_Agent').val();

                return g_api.ConnectLite(sQueryPrgId, 'GetExcel', args);
            },
            /**
             * 流程修改發送郵件
             * @param {String} itemname 第幾個流程
             * @param {Function} callback 回調函數
             * @return {Boolean} 是否停止
             */
            fnSendEmail = function (itemname, callback) {
                if (parent.SysSet.IsOpenMail !== 'Y') {
                    layer.alert(i18next.t('message.NotOpenMail'), { icon: 0 }, function () {// ╠message.NotOpenMail⇒系統沒有開放郵件發送功能，請聯絡管理員！╣
                        callback();
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
                            ExhibitionType: '出口',
                            DataDources: '出口管理',
                            ChangeItem: itemname,
                            SupplierName: oCurSupplierData.SupplierName,
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
             * @param {Object} data 單筆廠商資料
             * @return {Boolean} 是否停止
             */
            fnSendTrackingNumberEmail = function (data) {
                var sMsg = '';
                if (parent.SysSet.IsOpenMail !== 'Y') {
                    showMsg(i18next.t("message.NotOpenMail"));// ╠message.NotOpenMail⇒系統沒有開放郵件發送功能，請聯絡管理員！╣
                    return false;
                }
                if (!data.Email) {
                    sMsg += i18next.t("ExhibitionExport_Upd.SupplierEamil_required") + '<br/>'; // ╠ExhibitionImport_Upd.SupplierEamil_required⇒請輸入付款人╣
                }
                if (!$('#ShipmentPort').val()) {
                    sMsg += i18next.t("common.ShipmentPort_required") + '<br/>'; // ╠common.ShipmentPort_required⇒請輸入起運地╣
                }
                if (!$('#Destination').val()) {
                    sMsg += i18next.t("common.DestinationPort_required") + '<br/>'; // ╠common.DestinationPort_required⇒請輸入目的地╣
                }
                if (!(data.ExportData !== undefined && data.ExportData.Intowarehouse !== undefined ? data.ExportData.Intowarehouse.Number : '')) {
                    sMsg += i18next.t("message.Number_required") + '<br/>'; // ╠message.Number_required⇒請輸入件數╣
                }
                if (!(data.ExportData !== undefined && data.ExportData.Intowarehouse !== undefined ? data.ExportData.Intowarehouse.Unit : '')) {
                    sMsg += i18next.t("message.Unit_required"); // ╠message.Unit_required⇒ 請輸入（件數）單位╣
                }
                if (sMsg) {
                    showMsg(sMsg);
                    return false;
                }
                var toSendMail = function (flag) {
                    CallAjax(ComFn.W_Com, ComFn.SendMail, {
                        Params: {
                            FromOrgID: parent.OrgID,
                            FromUserName: parent.SysSet.FromName || '系統郵件',
                            EmailTo: [{
                                ToUserName: data.SupplierName,
                                ToEmail: data.Email,
                                Type: 'to'
                            }],
                            MailTempId: 'TrackingNumberNotice',
                            MailData: {
                                RefNumber: data.RefSupplierNo,
                                BillLadNOType: 'none',
                                ExhibitionName: oCurData.Exhibitioname_TW || '',
                                ExhibitionEName: oCurData.Exhibitioname_EN || '',
                                Shipment: $('#ShipmentPortCode').val(),
                                Destination: $('#DestinationCode').val(),
                                Number: data.ExportData !== undefined && data.ExportData.Intowarehouse !== undefined ? (data.ExportData.Intowarehouse.Number + ' ' + data.ExportData.Intowarehouse.Unit) : ''
                            }
                        }
                    }, function (res) {
                        if (res.d === '1') {
                            showMsg(i18next.t("message.SendEmail_Success"), 'success'); // ╠message.SendEmail_Success⇒郵件寄送成功╣
                            if (flag) {
                                var sIsSendMail = oCurData.IsSendMail + ',' + data.guid;
                                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                    Params: {
                                        exportexhibition: {
                                            values: { IsSendMail: sIsSendMail },
                                            keys: { ExportBillNO: sDataId }
                                        }
                                    }
                                }, function (res1) {
                                    if (res1.d > 0) {
                                        oCurData.IsSendMail = sIsSendMail;
                                        $('.' + data.guid).removeClass('a-url').addClass('a-mailurl');
                                    }
                                });
                            }
                        }
                        else {
                            showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                        }
                    }, function () {
                        showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                    });
                };
                if (isEmail(data.Email)) {
                    if ((oCurData.IsSendMail || '').indexOf(data.guid) === -1) {
                        toSendMail(true);
                    }
                    else {
                        // ╠message.IsSendTrackingNumberEmail⇒已寄送过，是否再次寄送？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendTrackingNumberEmail'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            toSendMail(false);
                            layer.close(index);
                        });
                    }
                }
                else {
                    showMsg(i18next.t("message.IncorrectEmail"), 'error'); // ╠message.IncorrectEmail⇒郵箱格式不正確╣
                }
            },
            /**
             * 批次發送Tracking number給客戶格式
             */
            fnBatchMail = function () {
                var toSendMail = function (data, name) {
                    return CallAjax(ComFn.W_Com, ComFn.SendMail, {
                        Params: {
                            FromOrgID: parent.OrgID,
                            FromUserName: parent.SysSet.FromName || '系統郵件',
                            EmailTo: [{
                                ToUserName: data.SupplierName,
                                ToEmail: data.Email,
                                Type: 'to'
                            }],
                            MailTempId: 'TrackingNumberNotice',
                            MailData: {
                                RefNumber: data.RefSupplierNo,
                                BillLadNOType: 'none',
                                ExhibitionName: oCurData.Exhibitioname_TW || '',
                                ExhibitionEName: oCurData.Exhibitioname_EN || '',
                                Shipment: $('#ShipmentPortCode').val(),
                                Destination: $('#DestinationCode').val(),
                                Number: data.ExportData !== undefined && data.ExportData.Intowarehouse !== undefined ? (data.ExportData.Intowarehouse.Number + ' ' + data.ExportData.Intowarehouse.Unit) : ''
                            }
                        }
                    }, function (res) {
                        if (res.d === '1') {
                            $('.' + data.guid).removeClass('a-url').addClass('a-mailurl');
                            showMsg(name + i18next.t("message.SendEmail_Success"), 'success'); // ╠message.SendEmail_Success⇒郵件寄送成功╣
                        }
                        else {
                            showMsg(name + i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                        }
                    }, function () {
                        showMsg(name + i18next.t("message.SendEmail_Failed"), 'error'); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                    });
                },
                    fnGetData = function () {
                        var d = $.Deferred(),
                            saList = [],
                            iIndex = 1,
                            sName = $('#Pop_SupplierName').val(),
                            sEName = $('#Pop_SupplierEName').val();

                        $.each(saGridData, function (idx, item) {
                            if ((((item.SupplierName || '').indexOf(sName) > -1 || sName === '')
                                && ((item.SupplierEName || '').indexOf(sEName) > -1 || sEName === ''))) {
                                saList.push({
                                    RowIndex: iIndex,
                                    guid: item.guid,
                                    SupplierID: item.SupplierID,
                                    CustomerNO: item.CustomerNO,
                                    UniCode: item.UniCode,
                                    Email: item.Email,
                                    RefSupplierNo: item.RefSupplierNo,
                                    ExportData: item.ExportData,
                                    SupplierName: item.SupplierName || '',
                                    SupplierEName: item.SupplierEName || ''
                                });
                                iIndex++;
                            }
                        });

                        d.resolve({
                            data: saList,
                            itemsCount: saList.length
                        });
                        return d.promise();
                    },
                    oConfig = {
                        Id: 'PopIsBatch',
                        Title: i18next.t('common.SelectBatchList'),// ╠common.SelectBatchList⇒請選擇要批次操作的資料╣
                        PageSize: 10000,
                        Get: fnGetData,
                        SearchFields: [
                            { id: "Pop_SupplierName", type: 'text', i18nkey: 'common.SupplierCName' },
                            { id: "Pop_SupplierEName", type: 'text', i18nkey: 'common.SupplierEName' }
                        ],
                        Fields: [
                            { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 50 },// ╠common.RowNumber⇒項次╣
                            { name: "CustomerNO", title: 'Customers_Upd.CustomerNO', width: 80 },
                            { name: "SupplierName", title: 'common.SupplierCName', width: 150 },
                            { name: "SupplierEName", title: 'common.SupplierEName', width: 150 }
                        ],
                        Callback: function (items) {
                            var sMsg = '',
                                bEmail = true,
                                oPost = [],
                                sIsSendMail = '';
                            $.each(items, function (idx, item) {
                                if (!item.Email || !isEmail(item.Email)) {
                                    sMsg = item.SupplierName === '' ? item.SupplierEName : item.SupplierName;
                                    bEmail = false;
                                    return false;
                                }
                            });
                            if (!bEmail) {
                                showMsg(sMsg + i18next.t("message.IncorrectEmail"), 'error'); // ╠message.IncorrectEmail⇒郵箱格式不正確╣
                                return false;
                            }

                            $.each(items, function (idx, item) {
                                var sSupplierName = item.SupplierName === '' ? item.SupplierEName : item.SupplierName;
                                if ((oCurData.IsSendMail || '').indexOf(item.guid) === -1) {
                                    sIsSendMail += item.guid + ',';
                                    oPost.push(toSendMail(item, sSupplierName));
                                }
                                else {
                                    toSendMail(item, sSupplierName);
                                }
                            });
                            $.whenArray(oPost).done(function () {
                                sIsSendMail = oCurData.IsSendMail + ',' + sIsSendMail;
                                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                    Params: {
                                        exportexhibition: {
                                            values: { IsSendMail: sIsSendMail },
                                            keys: { ExportBillNO: sDataId }
                                        }
                                    }
                                }, function (res) {
                                    if (res.d > 0) {
                                        oCurData.IsSendMail = sIsSendMail;
                                    }
                                });
                            });
                        }
                    };
                oPenPopm(oConfig);
            },
            /**
             * 批次匯入廠商列表
             */
            fnBatchImport = function () {
                var fnGetData = function () {
                    var sId = $('#Pop_ExhibitionName').val(),
                        d = $.Deferred();
                    if (!sId) {
                        return;
                    }
                    g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                if (oRes.ExportBillNO) {
                                    var saExhibitors = $.parseJSON(oRes.Exhibitors),
                                        saNewExhibitors = [];

                                    $.each(saExhibitors, function (idx, supplier) {
                                        var oNewExhibitor = {},
                                            sRefSupplierNo = $('#RefNumber').val() + rndnum(3);
                                        oNewExhibitor.RowIndex = idx + 1;
                                        oNewExhibitor.guid = guid();
                                        oNewExhibitor.SupplierID = supplier.SupplierID;
                                        oNewExhibitor.CustomerNO = supplier.CustomerNO || '';
                                        oNewExhibitor.UniCode = supplier.UniCode;
                                        oNewExhibitor.SupplierName = supplier.SupplierName;
                                        oNewExhibitor.SupplierEName = supplier.SupplierEName || '';
                                        oNewExhibitor.RefSupplierNo = sRefSupplierNo;
                                        oNewExhibitor.Contactor = supplier.Contactor || '';
                                        oNewExhibitor.Telephone = supplier.Telephone || '';
                                        oNewExhibitor.Email = supplier.Email || '';
                                        oNewExhibitor.ContactorName = supplier.ContactorName || '';
                                        oNewExhibitor.CreateUser = parent.UserID;
                                        oNewExhibitor.CreateDate = new Date().formate("yyyy/MM/dd HH:mm:ss");
                                        saNewExhibitors.push(oNewExhibitor);
                                    });
                                    d.resolve({
                                        data: saNewExhibitors,
                                        itemsCount: saNewExhibitors.length
                                    });
                                }
                            }
                        });
                    return d.promise();
                },
                    oConfig = {
                        Id: 'PopIsBatch',
                        Title: i18next.t('ExhibitionExport_Upd.SupplierBatchList'),// ╠ExhibitionExport_Upd.SupplierBatchList⇒批次匯入廠商╣
                        Width: '1000px',
                        PageSize: 10000,
                        Get: fnGetData,
                        SearchFields: [
                            { id: "Pop_ExhibitionName", type: 'select', i18nkey: 'ExhibitionExport_Upd.ExportBillName' }
                        ],
                        Fields: [
                            { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 50 },
                            { name: "SupplierName", title: 'common.SupplierName', width: 200 },
                            { name: "SupplierEName", title: 'common.SupplierEName', width: 200 },
                            { name: "ContactorName", title: 'common.Contactor', width: 100 },
                            { name: "Telephone", title: 'common.Telephone', width: 100 },
                            { name: "Email", title: 'common.Email', width: 150 }
                        ],
                        Callback: function (items) {
                            saGridData = clone(items);
                            oGrid.loadData();
                        }
                    };
                CallAjax(ComFn.W_Com, ComFn.GetList, {
                    Type: '', Params: {
                        exportexhibition: {
                            IsVoid: 'N',
                            OrgID: parent.OrgID
                        }
                    }
                }, function (res) {
                    var saList = $.parseJSON(res.d);
                    oConfig.SearchFields[0].html = createOptions(saList, 'ExportBillNO', 'ExportBillName');
                    oConfig.ContentPlush = '<style>.select2-container--open {z-index: 1000000001; }</style>';
                    oConfig.PopSuccessCallback = function (layero) {
                        setTimeout(function () {
                            $('#Pop_ExhibitionName').select2({ width: '240px' });
                        }, 100);
                    };
                    oPenPopm(oConfig);
                });
            },
            /**
             * 設定完成按鈕
             * @param {Object} flow 父層dom對象
             * @param {Boolean} bcomplete 是否完成
             */
            setSuccessBtn = function (flow, bcomplete) {
                var iCheckBox = flow.find(':input[type=checkbox]').length,
                    iChecked = flow.find(':input[type=checkbox]').not("input:checked").length;
                if (iChecked === 0 && (iCheckBox !== iChecked || !bcomplete)) {
                    flow.find(':input.complete').removeAttr('disabled');
                }
                else {
                    flow.find(':input.complete').attr('disabled', true);
                }
            },
            /**
             * 設定完成按鈕
             * @param {Object}that 父層dom對象
             */
            setTime = function (checkbox) {
                let sDate = newDate(null, true);
                let divFormGroup = checkbox.parentNode.parentNode;
                let FormGroupDate = divFormGroup.querySelector('.date-picker');

                if (checkbox.checked) {
                    switch (checkbox.id) {
                        case 'ExportData_InTransit1_Checked':
                            //運輸中
                            let DesPortCode = document.querySelector('#DestinationCode'); //目的地
                            let InTransitETA = divFormGroup.querySelector('#ExportData_InTransit1_ETA'); //運送中-ETA
                            InTransitETA.value = DesPortCode.value;
                            break;
                    }

                    FormGroupDate.value = sDate;
                }
                else {
                    FormGroupDate.value = '';

                    switch (checkbox.id) {
                        case 'ExportData_InTransit1_Checked':
                            //運輸中
                            let InTransitETA = document.querySelector('#ExportData_InTransit1_ETA'); //運送中-ETA
                            let IntransitDate = divFormGroup.querySelector('#ExportData_InTransit1_ETADate'); //運輸中-日期
                            InTransitETA.value = '';
                            IntransitDate.value = '';
                            break;
                    }
                }
            },
            /**
             * 設定流程頁簽
             * @param {Object}form 表單
             * @param {Object} oData 資料json
             */
            setFlowBox = function (form, oData) {
                if (oData.ExportData) {
                    if (oData.ExportData.complete) {
                        form.find('#ExportData').find('[name]').attr('disabled', true);
                        form.find('#ExportData').find(':input.complete').attr('disabled', true);
                    }
                    else {
                        setSuccessBtn(form.find('#ExportData'), false);
                    }
                    if (oData.ReturnType === 'H') {
                        form.find('#ReImport').show();
                        form.find('.addreturns').attr('disabled', true);
                    }
                    else if (oData.ReturnType === 'T') {
                        form.find('#TranserThird').show();
                        form.find('.addreturns').attr('disabled', true);
                    }
                    else if (oData.ReturnType === 'P') {
                        form.find('#PartThirdAndReImport').show();
                        form.find('.addreturns').attr('disabled', true);
                    }
                    form.find('#ExportData').find(':input.undo').removeAttr('disabled').click(function () {
                        var fnCallBack = function () {
                            //form.find('#ExportData').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                            form.find('#ExportData').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                            form.find('#ExportData').find(':input.undo').attr('disabled', true);
                            form.find('#ExportData').find(':input[type=hidden]').val('');
                        };
                        // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnSendEmail(i18next.t("ExhibitionExport_Upd.ExportData"), fnCallBack);
                            layer.close(index);
                        }, function () {
                            fnCallBack();
                        });
                    });
                } else {
                    setSuccessBtn(form.find('#ExportData'));
                }
                if (oData.ReImport) {
                    if (oData.ReImport.complete) {
                        form.find('#ReImport').find('[name]').attr('disabled', true);
                        form.find('#ReImport').find(':input.complete').attr('disabled', true);
                    }
                    else {
                        setSuccessBtn(form.find('#ReImport'), false);
                    }
                    form.find('#ReImport').find(':input.undo').removeAttr('disabled').click(function () {
                        var fnCallBack = function () {
                            //form.find('#ReImport').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                            form.find('#ReImport').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                            form.find('#ReImport').find(':input.undo').attr('disabled', true);
                            form.find('#ReImport').find(':input[type=hidden]').val('');
                        };
                        // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnSendEmail(i18next.t("ExhibitionExport_Upd.ReImport"), fnCallBack);
                            layer.close(index);
                        }, function () {
                            fnCallBack();
                        });
                    });
                } else {
                    setSuccessBtn(form.find('#ReImport'));
                }
                if (oData.TranserThird) {
                    if (oData.TranserThird.complete) {
                        form.find('#TranserThird').find('[name]').attr('disabled', true);
                        form.find('#TranserThird').find(':input.complete').attr('disabled', true);
                    }
                    else {
                        setSuccessBtn(form.find('#TranserThird'), false);
                    }
                    if (oData.LastReturnType === 'F') {
                        form.find('#TransferFour').show();
                        form.find('#TranserThird').find('.addnextreturn').attr('disabled', true);
                    }
                    else if (oData.LastReturnType === 'H') {
                        form.find('#ReImportFour').show();
                        form.find('.addnextreturn').attr('disabled', true);
                    }
                    form.find('#TranserThird').find(':input.undo').removeAttr('disabled').click(function () {
                        var fnCallBack = function () {
                            //form.find('#TranserThird').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                            form.find('#TranserThird').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                            form.find('#TranserThird').find(':input.undo').attr('disabled', true);
                            form.find('#TranserThird').find(':input[type=hidden]').val('');
                        };
                        // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnSendEmail(i18next.t("ExhibitionExport_Upd.TranserThird"), fnCallBack);
                            layer.close(index);
                        }, function () {
                            fnCallBack();
                        });
                    });

                    if (oData.TransferFour) {
                        if (oData.TransferFour.complete) {
                            form.find('#TransferFour').find('[name]').attr('disabled', true);
                            form.find('#TransferFour').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#TransferFour'), false);
                        }
                        if (oData.ReturnType_4 === 'F') {
                            form.find('#TransferFive').show();
                            form.find('#TransferFour').find('.addnextreturn').attr('disabled', true);
                        }
                        else if (oData.ReturnType_4 === 'H') {
                            form.find('#ReImportFive').show();
                            form.find('#TransferFour').find('.addnextreturn').attr('disabled', true);
                        }
                        form.find('#TransferFour').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                //form.find('#TransferFour').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                                form.find('#TransferFour').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#TransferFour').find(':input.undo').attr('disabled', true);
                                form.find('#TransferFour').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.TransferFourPlace"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#TransferFour'));
                    }
                    if (oData.ReImportFour) {
                        if (oData.ReImportFour.complete) {
                            form.find('#ReImportFour').find('[name]').attr('disabled', true);
                            form.find('#ReImportFour').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#ReImportFour'), false);
                        }
                        form.find('#ReImportFour').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                //form.find('#ReImportFour').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                                form.find('#ReImportFour').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#ReImportFour').find(':input.undo').attr('disabled', true);
                                form.find('#ReImportFour').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.ReImport"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#ReImportFour'));
                    }

                    if (oData.TransferFive) {
                        if (oData.TransferFive.complete) {
                            form.find('#TransferFive').find('[name]').attr('disabled', true);
                            form.find('#TransferFive').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#TransferFive'), false);
                        }
                        if (oData.ReturnType_5 === 'F') {
                            form.find('#TransferSix').show();
                            form.find('#TransferFive').find('.addnextreturn').attr('disabled', true);
                        }
                        else if (oData.ReturnType_5 === 'H') {
                            form.find('#ReImportSix').show();
                            form.find('#TransferFive').find('.addnextreturn').attr('disabled', true);
                        }
                        form.find('#TransferFive').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                form.find('#TransferFive').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#TransferFive').find(':input.undo').attr('disabled', true);
                                form.find('#TransferFive').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.TransferFivePlace"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#TransferFive'));
                    }
                    if (oData.ReImportFive) {
                        if (oData.ReImportFive.complete) {
                            form.find('#ReImportFive').find('[name]').attr('disabled', true);
                            form.find('#ReImportFive').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#ReImportFive'), false);
                        }
                        form.find('#ReImportFive').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                form.find('#ReImportFive').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#ReImportFive').find(':input.undo').attr('disabled', true);
                                form.find('#ReImportFive').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.ReImport"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#ReImportFive'));
                    }

                    if (oData.TransferSix) {
                        if (oData.TransferSix.complete) {
                            form.find('#TransferSix').find('[name]').attr('disabled', true);
                            form.find('#TransferSix').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#TransferSix'), false);
                        }
                        form.find('#TransferSix').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                form.find('#TransferSix').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#TransferSix').find(':input.undo').attr('disabled', true);
                                form.find('#TransferSix').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.TransferSixPlace"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#TransferSix'));
                    }
                    if (oData.ReImportSix) {
                        if (oData.ReImportSix.complete) {
                            form.find('#ReImportSix').find('[name]').attr('disabled', true);
                            form.find('#ReImportSix').find(':input.complete').attr('disabled', true);
                        }
                        else {
                            setSuccessBtn(form.find('#ReImportSix'), false);
                        }
                        form.find('#ReImportSix').find(':input.undo').removeAttr('disabled').click(function () {
                            var fnCallBack = function () {
                                form.find('#ReImportSix').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                                form.find('#ReImportSix').find(':input.undo').attr('disabled', true);
                                form.find('#ReImportSix').find(':input[type=hidden]').val('');
                            };
                            // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                            layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                fnSendEmail(i18next.t("ExhibitionExport_Upd.ReImport"), fnCallBack);
                                layer.close(index);
                            }, function () {
                                fnCallBack();
                            });
                        });
                    } else {
                        setSuccessBtn(form.find('#ReImportSix'));
                    }
                } else {
                    setSuccessBtn(form.find('#TranserThird'));
                }
                if (oData.PartThird) {
                    if (oData.PartThird.complete) {
                        form.find('#PartThird').find('[name]').attr('disabled', true);
                        form.find('#PartThird').find(':input.complete').attr('disabled', true);
                    }
                    else {
                        setSuccessBtn(form.find('#PartThird'), false);
                    }
                    form.find('#PartThird').find(':input.undo').removeAttr('disabled').click(function () {
                        var fnCallBack = function () {
                            //form.find('#PartThird').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                            form.find('#PartThird').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                            form.find('#PartThird').find(':input.undo').attr('disabled', true);
                            form.find('#PartThird').find(':input[type=hidden]').val('');
                        };
                        // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnSendEmail(i18next.t("ExhibitionExport_Upd.ReturnedHomeAndThirdPlace"), fnCallBack);// ╠ExhibitionExport_Upd.ReturnedHomeAndThirdPlace⇒部份轉運其他地區;部份退運回台╣
                            layer.close(index);
                        }, function () {
                            fnCallBack();
                        });
                    });
                } else {
                    setSuccessBtn(form.find('#PartThird'));
                }
                if (oData.PartReImport) {
                    if (oData.PartReImport.complete) {
                        form.find('#PartReImport').find('[name]').attr('disabled', true);
                        form.find('#PartReImport').find(':input.complete').attr('disabled', true);
                    }
                    else {
                        setSuccessBtn(form.find('#PartReImport'), false);
                    }
                    form.find('#PartReImport').find(':input.undo').removeAttr('disabled').click(function () {
                        var fnCallBack = function () {
                            //form.find('#PartReImport').find(':input[type=checkbox]:last').parents('.form-group').find(':input').removeAttr('disabled');
                            form.find('#PartReImport').find(':input[type=checkbox],:input[type=text],.complete,select').removeAttr('disabled');
                            form.find('#PartReImport').find(':input.undo').attr('disabled', true);
                            form.find('#PartReImport').find(':input[type=hidden]').val('');
                        };
                        // ╠message.IsSendEmailToCharGer⇒是否要寄送郵件通知負責業務人員？╣  ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.IsSendEmailToCharGer'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnSendEmail(i18next.t("ExhibitionExport_Upd.ReturnedHomeAndThirdPlace"), fnCallBack);// ╠ExhibitionExport_Upd.ReturnedHomeAndThirdPlace⇒部份轉運其他地區;部份退運回台╣
                            layer.close(index);
                        }, function () {
                            fnCallBack();
                        });
                    });
                } else {
                    setSuccessBtn(form.find('#PartReImport'));
                }

                form.find('.complete').click(function () {
                    var oBtn = this,
                        sId = $(oBtn).parents('.flow').attr('id'),
                        data = getFormSerialize(form);

                    data[sId].complete = 'Y';

                    form.find('#' + sId + '_complete').val('Y');
                    form.find(oBtn).attr('disabled', true);
                    form.find('#' + sId).find('[name]').attr('disabled', true);
                    if ($(oBtn).parents('.flow').next('.flow').length > 0) {
                        $(oBtn).parents('.flow').find('.addreturns,.addnextreturn').removeAttr('disabled');
                    }
                });

                form.find('.addreturns').click(function () {
                    layer.open({
                        type: 1,
                        title: i18next.t('common.ReturnType'),// ╠common.ReturnType⇒請選擇運送類型╣
                        area: ['500px', '120px'],//寬度
                        shade: 0.75,//遮罩
                        shadeClose: true,// ╠ExhibitionExport_Upd.ReturnedHome⇒退運回國╣ ╠ExhibitionExport_Upd.TransferThirdPlace⇒出貨至第三地╣ ╠ExhibitionExport_Upd.ReturnedHomeAndThirdPlace⇒部分運至第三地部分退運回國╣
                        content: '<div class="pop-box">\
                                   <button type="button" data-i18n="ExhibitionExport_Upd.ReturnedHome" id="btn_ReturnedHome" class="btn-custom green">退運回國</button>\
                                   <button type="button" data-i18n="ExhibitionExport_Upd.TransferThirdPlace" id="btn_TransferThirdPlace" class="btn-custom green">出貨至第三地</button>\
                                   <button type="button" data-i18n="ExhibitionExport_Upd.ReturnedHomeAndThirdPlace" id="btn_ReturnedHomeAndThirdPlace" class="btn-custom green">部分運至第三地部分退運回國</button>\
                              </div>',
                        success: function (layero, idx) {
                            layero.find('#btn_ReturnedHome').click(function () {
                                form.find('#ReImport').show();
                                form.find('#TranserThird,#PartThirdAndReImport,#TransferFour,#ReImportFour').hide();
                                form.find('.addreturns').attr('disabled', true);
                                form.find('#ReturnType').val('H');
                                layer.close(idx);
                            });
                            layero.find('#btn_TransferThirdPlace').click(function () {
                                form.find('#TranserThird').show();
                                form.find('#ReImport,#PartThirdAndReImport,#TransferFour,#ReImportFour').hide();
                                form.find('.addreturns').attr('disabled', true);
                                form.find('#ReturnType').val('T');
                                layer.close(idx);
                            });
                            layero.find('#btn_ReturnedHomeAndThirdPlace').click(function () {
                                form.find('#PartThirdAndReImport').show();
                                form.find('#ReImport,#TranserThird,#TransferFour,#ReImportFour').hide();
                                form.find('.addreturns').attr('disabled', true);
                                form.find('#ReturnType').val('P');
                                layer.close(idx);
                            });
                            transLang(layero);
                        }
                    });
                });

                form.find('.addnextreturn').click(function () {
                    var iIndex = $(this).attr('data-index');
                    layer.open({
                        type: 1,
                        title: i18next.t('common.AddReimport'),// ╠common.AddReimport⇒增加退運╣
                        area: ['300px', '120px'],//寬度
                        shade: 0.75,//遮罩
                        shadeClose: true,// ╠common.ReturnedHome⇒退運回國╣ ╠common.TransferFourPlace⇒出貨至第四地╣ ╠common.TransferFivePlace⇒出貨至第五地╣ ╠common.TransferSixPlace⇒出貨至第六地╣
                        content: '<div class="pop-box">\
                                <button type="button" data-i18n="ExhibitionExport_Upd.ReturnedHome" id="btn_ReturnedHome" class="btn-custom green">退運回國</button>\
                                <button type="button" data-i18n="ExhibitionExport_Upd.' + (iIndex === '3' ? 'TransferFourPlace' : iIndex === '4' ? 'TransferFivePlace' : 'TransferSixPlace') + '" id="btn_Transfer" class="btn-custom green">出貨至第n地</button>\
                              </div>',
                        success: function (layero, idx) {
                            layero.find('#btn_ReturnedHome').click(function () {
                                switch (iIndex) {
                                    case '3':
                                        form.find('#TransferFour').hide();
                                        form.find('#ReImportFour').show();
                                        form.find('#TranserThird').find('.addnextreturn').attr('disabled', true);
                                        form.find('#LastReturnType').val('H');
                                        break;
                                    case '4':
                                        form.find('#TransferFive').hide();
                                        form.find('#ReImportFive').show();
                                        form.find('#TransferFour').find('.addnextreturn').attr('disabled', true);
                                        form.find('#ReturnType_4').val('H');
                                        break;
                                    case '5':
                                        form.find('#TransferSix').hide();
                                        form.find('#ReImportSix').show();
                                        form.find('#TransferFive').find('.addnextreturn').attr('disabled', true);
                                        form.find('#ReturnType_5').val('H');
                                        break;
                                }
                                layer.close(idx);
                            });
                            layero.find('#btn_Transfer').click(function () {
                                switch (iIndex) {
                                    case '3':
                                        form.find('#TransferFour').show();
                                        form.find('#ReImportFour').hide();
                                        form.find('#TranserThird').find('.addnextreturn').attr('disabled', true);
                                        form.find('#LastReturnType').val('F');
                                        break;
                                    case '4':
                                        form.find('#TransferFive').show();
                                        form.find('#ReImportFive').hide();
                                        form.find('#TransferFour').find('.addnextreturn').attr('disabled', true);
                                        form.find('#ReturnType_4').val('F');
                                        break;
                                    case '5':
                                        form.find('#TransferSix').show();
                                        form.find('#ReImportSix').hide();
                                        form.find('#TransferFive').find('.addnextreturn').attr('disabled', true);
                                        form.find('#ReturnType_5').val('F');
                                        break;
                                }
                                layer.close(idx);
                            });
                            transLang(layero);
                        }
                    });
                });

                form.find(':input[type=checkbox]').not('#IsBatch').on('click', function (e) {
                    setTime(this);
                    setSuccessBtn($(this).parents('.flow'));
                });

                form.find(':input,select').on('change', function () {
                    bRequestStorage = true;
                });

                form.find('.ui-icon-close').on('click', function (e) {
                    var sFlowId = $(this).parents('.flow')[0].id;
                    switch (sFlowId) {
                        case 'ExportData':
                            $(this).parents('.form-group').remove();
                            break;
                        case 'PartThird':
                            form.find('#PartThirdAndReImport').hide();
                            form.find('.addreturns').removeAttr('disabled');
                            form.find('#ReturnType').val('');
                            break;
                        case 'TranserThird':
                            form.find('#TranserThird,#ReImportFour,#TransferFour,#ReImportFive,#TransferFive,#ReImportSix,#TransferSix').hide();
                            form.find('.addreturns').removeAttr('disabled');
                            form.find('#ReturnType').val('');
                            form.find('#LastReturnType').val('');
                            form.find('#ReturnType_4').val('');
                            form.find('#ReturnType_5').val('');
                            break;
                        case 'ReImport':
                            form.find('#ReImport').hide();
                            form.find('.addreturns').removeAttr('disabled');
                            form.find('#ReturnType').val('');
                            break;
                        case 'TransferFour':
                            form.find('#TransferFour,#ReImportFive,#TransferFive,#ReImportSix,#TransferSix').hide();
                            form.find('#TranserThird').find('.addnextreturn').removeAttr('disabled');
                            form.find('#LastReturnType').val('');
                            form.find('#ReturnType_4').val('');
                            form.find('#ReturnType_5').val('');
                            break;
                        case 'ReImportFour':
                            form.find('#ReImportFour').hide();
                            form.find('#TranserThird').find('.addnextreturn').removeAttr('disabled');
                            form.find('#LastReturnType').val('');
                            break;
                        case 'TransferFive':
                            form.find('#TransferFive,#ReImportSix,#TransferSix').hide();
                            form.find('#TransferFour').find('.addnextreturn').removeAttr('disabled');
                            form.find('#ReturnType_4').val('');
                            form.find('#ReturnType_5').val('');
                            break;
                        case 'ReImportFive':
                            form.find('#ReImportFive').hide();
                            form.find('#TransferFour').find('.addnextreturn').removeAttr('disabled');
                            form.find('#ReturnType_4').val('');
                            break;
                        case 'TransferSix':
                            form.find('#TransferSix').hide();
                            form.find('#TransferFive').find('.addnextreturn').removeAttr('disabled');
                            form.find('#ReturnType_5').val('');
                            break;
                        case 'ReImportSix':
                            form.find('#ReImportSix').hide();
                            form.find('#TransferFive').find('.addnextreturn').removeAttr('disabled');
                            form.find('#ReturnType_5').val('');
                            break;
                    }
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
             * 設定客戶下拉選單
             * @return {Object} ajax物件
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
                            $('#Organizer1,#Organizer2,#Organizer3,#Organizer4').html(sCustomersOptionsHtml);
                            var saNotAuditCurs = Enumerable.From(saCustomers).Where(function (e) { return e.IsAudit === 'Y'; }).ToArray();
                            sCustomersNotAuditOptionsHtml = createOptions(saNotAuditCurs, 'id', 'text');
                        }
                        select2Init();
                        $("#Organizer1,#Organizer2,#Organizer3,#Organizer4").select2();
                        $.each($("#Organizer1,#Organizer2,#Organizer3,#Organizer4"), function (idx, item) {
                            //組團單位-動態產生
                            let OrganizerIdex = idx + 1;
                            $("#Organizer" + OrganizerIdex).on('select2:select', function (e) {
                                var data = e.params.data;
                                sSelectedOrganizers[OrganizerIdex] = { id: data.id, text: data.text, title: data.title };
                                $('[data-id="BillOrganizer"]').html(createOptions(sSelectedOrganizers.filter(e => e != null), 'id', 'text'));
                                //加入到儲存資料內
                                sOrganizers = [];
                                $.each(sSelectedOrganizers, function (idx, e) {
                                    if (e && e.id.length === 36)
                                        sOrganizers.push(e.id);
                                })
                            });
                        })
                        //後端對應也要改
                        sSelectedOrganizers[MaxOrganizerCount] = { id: "SelfCome", text: "自來", title: "自來" };
                    }
                });
            },
            /**
             * 批次操作獲取廠商資料
             * @return {Object} promise物件
             */
            fnGetPop_CusSupplier = function () {
                var saList = [],
                    iIndex = 1,
                    sSupplierName = $('#Pop_SupplierName').val(),
                    sSupplierEName = $('#Pop_SupplierEName').val(),
                    fnCheck = function (itemMatch) {
                        var bMatch = true;
                        if (!((itemMatch.LastReturnType || '') === (oCurSupplierData.LastReturnType || '') && (itemMatch.ReturnType || '') === (oCurSupplierData.ReturnType || ''))) {
                            bMatch = false;
                        }
                        return bMatch;
                    },
                    d = $.Deferred();

                $.each(saGridData, function (idx, item) {
                    if ((((item.SupplierName || '').indexOf(sSupplierName) > -1 || sSupplierName === '')
                        && ((item.SupplierEName || '').indexOf(sSupplierEName) > -1 || sSupplierEName === '') && fnCheck(item))) {
                        saList.push({
                            RowIndex: iIndex,
                            guid: item.guid,
                            SupplierID: item.SupplierID,
                            CustomerNO: item.CustomerNO,
                            UniCode: item.UniCode,
                            SupplierName: item.SupplierName || '',
                            SupplierEName: item.SupplierEName || ''
                        });
                        iIndex++;
                    }
                });
                d.resolve({
                    data: saList,
                    itemsCount: saList.length
                });
                return d.promise();
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
             * 審核通過後禁用頁面欄位
             * @param {Object}dom 當前區塊
             * @param {Object}data 當前資料
             */
            fnSetDisabled = function (dom, data) {
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
                            dom.find('.bills-print').removeAttr('disabled');//新增列印草稿
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
                            dom.find('.bills-print').removeAttr('disabled');//新增列印草稿
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
                    fnOpenAccountingArea(dom.find('.BillOrganizers'), parent.UserInfo.roles);
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
                    iSubtotal = 0, // 未稅總計
                    iSubtotal_Tax = 0,// 未稅總計-有稅率
                    iSubtotal_NoTax = 0,// 未稅總計-沒稅率
                    iTaxtotal = 0,// (純)稅金總計
                    iTaxSubtotal = 0, //總金額(未稅+稅)
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
                                    iAcount += _bill.AmountSum;
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
                        else if (oTab[0].id === 'tab5') {
                            //增加退運報價/預估成本-預估成本累加
                            let Return_Estimatedcostsum = $('#tab5 .return_estimatedcostsum').val();
                            let Current_Return_Estimatedcostsum = parseFloat((Return_Estimatedcostsum || '0').toString().replaceAll(',', ''));
                            $('#tab5 .return_estimatedcostsum').val(fMoney(Current_Return_Estimatedcostsum + iSubtotal, 2, data.Currency));
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
                        //加總 = 上個帳單加總 + 此次帳單的未稅金額
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
                                //增加退運報價/預估成本-帳單金額累加(有先後順序，無法對調)
                                let LastRow_Return_Amountsum = parseFloat($('#tab5 .return_amountsum').val().replaceAll(',', ''));
                                $('#tab5 .return_amountsum').val(fMoney(LastRow_Return_Amountsum + TabTipUntaxtotal, 2, FeeItemCurrency));

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
                else if (oTab[0].id === 'tab5') {
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

                oFinancial.find('.input-value').on('change', function () {
                    var that = this,
                        sId = $(that).attr('data-id');
                    data[sId] = $(that).val();
                    bRequestStorage = true;
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
             * 添加費用項目
             * @param：that(Object)當前dom對象
             * @return：data(Object)當前費用項目
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
                                var oCurrency = Enumerable.From(saCurrency).Where(function (e) { return e.id === sCurrencyId; }).First();
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
            * @param that(Object)當前dom對象
            * @return data(Object)當前費用項目
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
            * @param that(Object)當前dom對象
            * @return quote(Object)當前費用項目
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
             */
            fnPushBill = function (supplier, data, parentid, bRetn) {
                var fnBill = function (billno) {
                    var oNewBill = {};
                    oNewBill.guid = guid();
                    oNewBill.IsRetn = bRetn ? 'Y' : 'N';
                    oNewBill.parentid = parentid || '';
                    oNewBill.KeyName = 'Bill';
                    oNewBill.AuditVal = '0';
                    oNewBill.BillNO = billno;
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
                    oNewBill.SupplierGuid = supplier.guid;
                    oNewBill.Payer = supplier.SupplierID;
                    oNewBill.Number = '';
                    oNewBill.Unit = '';
                    oNewBill.Weight = '';
                    oNewBill.Volume = '';
                    oNewBill.CustomerGuid = supplier.SupplierID;
                    oNewBill.CustomerCode = supplier.CustomerNO;
                    oNewBill.UniCode = supplier.UniCode;
                    oNewBill.SupplierName = supplier.SupplierName;
                    oNewBill.SupplierEName = supplier.SupplierEName;
                    oNewBill.RefNumber = supplier.RefSupplierNo;
                    oNewBill.Contactor = supplier.Contactor;
                    oNewBill.ContactorName = supplier.ContactorName;
                    oNewBill.Telephone = supplier.Telephone || '';
                    oNewBill.Email = supplier.Email;
                    oNewBill.ReFlow = bRetn ? 'ReImport' : '';
                    data.Bills.push(oNewBill);
                    if (!bRetn) {
                        supplier.BillNO = billno;
                    }
                };
                return g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: parent.UserInfo.OrgID + 'E',
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
                        ExportBillNO: sDataId,
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
                        btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣ ╠common.Pass⇒通過╣ ╠common.NotPass⇒不通過╣
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
                                    ExportBillNO: sDataId,
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
                                layer.close(idx);
                            });
                            transLang(layero);
                        }
                    });
                });
                $('#tab3 #estimated_synquote').not('disabled').off('click').on('click', function () {
                    oCurData.EstimatedCost.FeeItems = clone(oCurData.Quote.FeeItems);
                    $.each(oCurData.EstimatedCost.FeeItems, function (idx, item) {
                        //item.FinancialUnitPrice = 0;
                        //item.FinancialNumber = 0;
                        //item.FinancialAmount = 0;
                        //item.FinancialTWAmount = 0;
                        //item.FinancialTaxRate = '0%';
                        //item.FinancialTax = 0;
                        item.CreateUser = parent.UserID;
                        item.CreateDate = newDate(null, true);
                    });
                    fnBindFeeItem($('#tab3').find('[data-id="estimatedcost-box"]'), oCurData, oCurData.EstimatedCost);
                });
                console.log(new Date());
                fnBindBillLists();
                console.log(new Date());

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
                    ExportBillNO: sDataId,
                    Exhibitors: saGridData,
                    Bills: oCurData.Bills,
                    Bill: bill
                }, function (res) {
                    if (res.RESULT) {
                        fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill);
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
                                ExportBillNO: sDataId,
                                Bills: oCurData.Bills,
                                Bill: bill
                            }, function (res) {
                                if (res.RESULT) {
                                    $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text(bill.BillCheckDate);
                                    fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill);
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
                    ExportBillNO: sDataId,
                    Bills: oCurData.Bills,
                    Bill: bill,
                    LogData: fnGetBillLogData(bill)
                }, function (res) {
                    if (res.RESULT) {
                        $('.bill-box-' + bill.BillNO).find('.bill-chewckdate').text('');
                        fnSetDisabled($('.bill-box-' + bill.BillNO).parents('.financial'), bill);
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
             */
            fnPrint = function (templid, action, bill) {
                var bReceipt = action.indexOf('Receipt') > -1,
                    fnToPrint = function (idx, paydatetext) {
                        g_api.ConnectLite(sProgramId, bReceipt ? 'PrintReceipt' : 'PrintBill', {
                            ExportBillNO: sDataId,
                            TemplID: templid,
                            Bill: bill,
                            Action: action,
                            PayDateText: paydatetext || ''
                        }, function (res) {
                            if (res.RESULT) {
                                if (idx) {
                                    layer.close(idx);
                                }
                                var saPath = res.DATA.rel,
                                    sTitle = bReceipt ? 'common.Receipt_Preview' : 'common.Bill_Preview';
                                if (action.indexOf('Print_') > -1) {
                                    var index = layer.open({
                                        type: 2,
                                        title: i18next.t(sTitle),
                                        content: gServerUrl + '/' + (typeof saPath === 'string' ? saPath : saPath[0]),
                                        area: ['900px', '500px'],
                                        maxmin: true
                                    });
                                    //layer.full(index); //弹出即全屏
                                }
                                else {
                                    if (typeof saPath === 'string') {
                                        DownLoadFile(saPath);
                                    }
                                    else {
                                        $.each(saPath, function (i, path) {
                                            setTimeout(function () {
                                                DownLoadFile(path);
                                            }, 500 * i);
                                        });
                                    }
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
                              </div>',// ╠common.PayDateText1⇒請立即安排付款╣ ╠common.PayDateText2⇒a.s.a.p╣ ╠common.PayDateText3⇒日期╣
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
            */
            fnBillPost = function (bill) {
                bill.AuditVal = '5';
                bill.CreateDate = newDate();
                g_api.ConnectLite(sProgramId, 'BillPost', {
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                                ExportBillNO: sDataId,
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
                        ExportBillNO: sDataId,
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
                        exportexhibition: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { ExportBillNO: sDataId }
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
                        exportexhibition: {
                            values: { Bills: JSON.stringify(oCurData.Bills) },
                            keys: { ExportBillNO: sDataId }
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
             * 綁定帳單
             */
            fnBindBillLists = function () {
                var oBillsBox = $('#accordion');
                //oBillsBox.html('');
                //$('#tab3 .amountsum').val(0);

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
                                    oCurrency = Enumerable.From(saAccountingCurrency).Where(function (e) { return e.ArgumentID === sCurrencyId; }).FirstOrDefault();
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
                            fnBindFeeItem($('[data-id=bill_fees_' + bill.BillNO + ']'), oCurData, bill);
                            //觸發點擊項目 產生資料內容
                            oBillBox.click(function () {
                                if ($(this).attr('aria-expanded') === 'false') {
                                    oBillBox.find('[data-id="Payer"]').html(sCustomersNotAuditOptionsHtml).val(bill.Payer);
                                    setTimeout(function () {
                                        oBillBox.find('[data-id="Payer"]').select2({ width: '250px' });
                                    }, 1000);
                                    if (bill.Payer) {
                                        var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === bill.Payer; }),
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
                                                        var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid === sContactor; }).First();
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
                                                            var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid === sContactor; }).First();
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
                                            case 'Download_BatchBill':
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
                                                            exportexhibition: {
                                                                values: { Bills: JSON.stringify(oCurData.Bills) },
                                                                keys: { ExportBillNO: sDataId }
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
                            //createOptions(sSelectedOrganizers.filter(e => e != null ), 'id', 'text')
                            //組團單位
                            oBillBox.find('[data-id="BillOrganizer"]').html(createOptions(sSelectedOrganizers.filter(e => e != null), 'id', 'text')).on('change', function () {
                                bill.BillOrganizer = this.value;
                                bRequestStorage = true;
                            }).val(bill.BillOrganizer);
                        }
                    });
                }
                fnSetPermissions();//設置權限
                $('#tab3').css({ 'padding-top': 40 });
                $('#topshow_box').show();//當審通過之後才顯示總金額
                transLang(oBillsBox);
            },
            /**
             * 設置操作權限
             */
            fnSetPermissions = function () {
                if (parent.UserInfo.roles.indexOf('Admin') === -1) {
                    if ((parent.UserInfo.roles.indexOf('CDD') > -1 && (oCurData.ResponsiblePerson === parent.UserID || oCurData.DepartmentID === parent.UserInfo.DepartmentID)) || parent.UserInfo.roles.indexOf('CDD') === -1 || parent.SysSet.CDDProUsers.indexOf(parent.UserID) > -1) {//報關作業
                        $('[href="#tab3"],[href="#tab4"],[href="#tab5"],[href="#tab6"]').parent().show();
                    }
                    else {
                        $('[href="#tab3"],[href="#tab4"],[href="#tab5"],[href="#tab6"]').parent().hide();
                    }
                    if (!(parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser)) {//其他
                        $('#tab3,#tab5').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.estimated_addreturnbills,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost').attr('disabled', 'disabled');
                        $('#tab3,#tab5').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Business') > -1) {//業務
                        $('#tab4,#tab6').find(':input,button,textarea').attr('disabled', 'disabled');
                        $('#tab4,#tab6').find('.icon-p').addClass('disabled');
                    }
                    if (parent.UserInfo.roles.indexOf('Account') > -1) {//會計
                        $('#tab1,#tab2,#tab3,#tab5').find(':input,button,textarea').not('.alreadyaudit,.cancelaudi,.writeoff,.bills-print,.jsgrid-button,.estimated_addreturnbills,.prepay,.mprepay,.billvoid,.canceloff,.cancelpost,.importfeeitem,.plusfeeitem').attr('disabled', 'disabled');
                        $('#tab3,#tab5').find('.icon-p').addClass('disabled');
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
                            ExportBillNO: sDataId,
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
                                            ExportBillNO: sDataId,
                                            ReturnBills: oCurData.ReturnBills,
                                            AuditVal: Returns.Quote.AuditVal,
                                            SourceID: Returns.Quote.guid,
                                            Index: idx + 1
                                        }, function (res) {
                                            if (res.RESULT) {
                                                showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                                fnSetDisabled(oFieldset_Return.find('.quoteandprecost'), Returns.Quote);
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
                                    }
                                    else {
                                        ;
                                        if (!sNotPassReason) {
                                            showMsg(i18next.t("message.NotPassReason_Required")); // ╠message.NotPassReason_Required⇒請填寫不通過原因╣
                                            return false;
                                        }
                                        else {
                                            Returns.Quote.AuditVal = '3';
                                            Returns.EstimatedCost.AuditVal = '3';
                                            Returns.Quote.NotPassReason = sNotPassReason;
                                            Returns.EstimatedCost.NotPassReason = sNotPassReason;
                                        }
                                    }
                                    fnAuditForQuote();
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

                    if (Returns.Quote.AuditVal === '2' && (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser)) {
                        oFieldset_Return.find('.estimated_addreturnbills').removeAttr('disabled').show().off('click').on('click', function () {
                            var fnGetBillsAdd = function () {
                                var saBillsAdd = [],
                                    iNum = 1,
                                    oSame = {},
                                    d = $.Deferred();
                                $.each(saGridData, function (m, item) {
                                    var iExsit = Enumerable.From(Returns.Bills).Where(function (e) { return (e.CustomerGuid === item.SupplierID && !e.VoidReason); }).Count();
                                    if (!oSame[item.SupplierID]) {
                                        oSame[item.SupplierID] = 1;
                                    }
                                    else {
                                        oSame[item.SupplierID]++;
                                    }
                                    if (iExsit === 0 || oSame[item.SupplierID] > iExsit) {
                                        item.RowIndex = iNum;
                                        saBillsAdd.push(item);
                                        iNum++;
                                        oSame[item.SupplierID]--;
                                    }
                                });
                                d.resolve({
                                    data: saBillsAdd,
                                    itemsCount: saBillsAdd.length
                                });
                                return d.promise();
                            },
                                oConfig = {
                                    Id: 'PopIsBatch',
                                    Title: i18next.t('common.SelectBatchList'),// ╠message.SelectBatchList⇒請選擇要批次操作的資料╣
                                    PageSize: 10000,
                                    Get: fnGetBillsAdd,
                                    SearchFields: [
                                        { id: "Pop_SupplierName", type: 'text', i18nkey: 'common.SupplierCName' },
                                        { id: "Pop_SupplierEName", type: 'text', i18nkey: 'common.SupplierEName' }
                                    ],
                                    Fields: [
                                        { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 40 },
                                        { name: "CustomerNO", title: 'Customers_Upd.CustomerNO', width: 80 },
                                        { name: "SupplierName", title: 'common.SupplierCName', width: 150 },
                                        { name: "SupplierEName", title: 'common.SupplierEName', width: 150 }
                                    ],
                                    Callback: function (items) {
                                        if (items.length > 0) {
                                            var saPost = [];
                                            $.each(items, function (e, _item) {
                                                saPost.push(fnPushBill(_item, Returns, Returns.guid, true));
                                            });
                                            $.whenArray(saPost).done(function () {
                                                fnRenderReturnBills();
                                            });
                                        }
                                    }
                                };
                            oPenPopm(oConfig);
                        });
                    }
                    else {
                        oFieldset_Return.find('.estimated_addreturnbills').hide()
                    }

                    $.each(Returns.Bills, function (i, bill) {
                        bill.Advance = bill.Advance || 0;
                        var oBillsBox = $('.return-bill-' + bill.parentid),
                            sHtml = $("#temp_returnbillbox").render([bill]);
                        oBillsBox.append(sHtml);
                        var oBillBox = $('.bill-box-' + bill.BillNO);
                        oBillBox.find('[data-id="Currency"]').html(sAccountingCurrencyOptionsHtml).on('change', function () {
                            var sCurrencyId = this.value,
                                oCurrency = Enumerable.From(saAccountingCurrency).Where(function (e) { return e.ArgumentID === sCurrencyId; }).FirstOrDefault();
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
                        oBillBox.find('[data-id="ReFlow"]').val(bill.ReFlow || '');
                        setTimeout(function () {
                            oBillBox.find('[data-id="Payer"]').select2({ width: '250px' });
                        }, 1000);

                        if (bill.Payer) {
                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === bill.Payer; }),
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
                                            var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid === sContactor; }).First();
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

                        oBillBox.find('[data-id="ExchangeRate"]').val(bill.ExchangeRate || 1.00);
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

                        oBillBox.find('[data-id="Payer"]').on('change', function () {
                            var sCustomerId = this.value,
                                saContactors = [];
                            if (sCustomerId) {
                                var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sCustomerId; }).First();
                                saContactors = JSON.parse(oCur.Contactors || '[]');
                                //bill.CustomerCode = oCur.CustomerNO;
                                //bill.UniCode = oCur.UniCode;
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
                                                var oContactor = Enumerable.From(oRes.Contactors).Where(function (e) { return e.guid === sContactor; }).First();
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
                                case 'Download_BatchBill':
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
                                                exportexhibition: {
                                                    values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                                                    keys: { ExportBillNO: sDataId }
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
                });

                fnSetPermissions();//設置權限
                $('#topshow_box_return').show();//當審通過之後才顯示總金額
                transLang($('#tab5,#tab6'));
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
                if (!bill.ReFlow) {
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
                    ExportBillNO: sDataId,
                    Exhibitors: saGridData,
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
                                ExportBillNO: sDataId,
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
             * 會計審核
             * @param bill(object)帳單資料
             */
            fnReturnBillAccountAudit = function (bill) {
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
                    ExportBillNO: sDataId,
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
            * @param bill(object)帳單資料
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
                                ExportBillNO: sDataId,
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
            * @param bill(object)帳單資料
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
                        ExportBillNO: sDataId,
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
                        exportexhibition: {
                            values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                            keys: { ExportBillNO: sDataId }
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
                        exportexhibition: {
                            values: { ReturnBills: JSON.stringify(oCurData.ReturnBills) },
                            keys: { ExportBillNO: sDataId }
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
            * Grid初始化
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
                        {//╠common.SupplierName⇒客戶/參展廠商名稱╣
                            name: "SupplierName", title: 'common.SupplierName', width: 150, filtering: true, inserting: true, editing: false, validate: { validator: 'required', message: i18next.t('common.Supplier_required') },
                            insertTemplate: function () {
                                var oSelect = $('<select/>', {
                                    change: function () {
                                        var sCustomerId = this.value,
                                            sRefSupplierNo = $('#RefNumber').val() + rndnum(3);
                                        if (sCustomerId) {
                                            var oCur = Enumerable.From(saCustomers).Where(function (e) { return e.id === sCustomerId; }).First();
                                            oAddItem.guid = guid();
                                            oAddItem.SupplierID = oCur.id;
                                            oAddItem.CustomerNO = oCur.CusNO;
                                            oAddItem.UniCode = oCur.UniCode;
                                            oAddItem.SupplierName = oCur.textcn;
                                            oAddItem.SupplierEName = oCur.texteg;
                                            oAddItem.RefSupplierNo = sRefSupplierNo;
                                            oAddItem.Telephone = oCur.Telephone;
                                            oAddItem.Email = oCur.Email;
                                            oAddItem.ContactorName = '';
                                            oAddItem.CreateUser = parent.UserID;
                                            oAddItem.CreateDate = new Date().formate("yyyy/MM/dd HH:mm:ss");
                                            oSelect.parent().next().find(':input').val(oAddItem.SupplierEName);
                                            oSelect.parent().next().next().find(':input').val(sRefSupplierNo);
                                            var saContactors = JSON.parse(oCur.Contactors || '[]');
                                            oSelect.parent().next().next().next().find('select').html(createOptions(saContactors, 'guid', 'FullName')).on('change', function () {
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
                                            oSelect.parent().next().next().next().next().find(':input').val(oCur.Telephone);
                                            oSelect.parent().next().next().next().next().next().find(':input').val(oCur.Email);
                                        }
                                        else {
                                            oSelect.parent().next().next().next().find('select').html(createOptions([]));
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
                                            oSelect.parent().next().next().next().find('select').html(createOptions(saContactors, 'guid', 'FullName'))
                                        }
                                        else {
                                            oSelect.parent().next().next().next().find('select').html(createOptions([]));
                                        }
                                        bRequestStorage = true;
                                    }
                                });
                                setTimeout(function () {
                                    oSelect.html(sCustomersOptionsHtml).select2({ width: '180px' });
                                }, 1000);
                                return this.filterControl = oSelect;
                            },
                            filterValue: function () {
                                return this.filterControl.val() === '' ? '' : this.filterControl.find("option:selected").text();
                            }
                        },
                        {//╠common.SupplierEName⇒客戶/參展廠商英文名稱╣
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
                        {//╠common.RefSupplierNo⇒客戶/廠商查詢號碼╣
                            name: "RefSupplierNo", title: 'common.RefSupplierNo', width: 100, filtering: true, inserting: true, editing: false,
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
                            name: "Flow_Status", title: 'common.Flow_Status', type: "text", width: 80, filtering: true, editing: false, inserting: false,
                            filterTemplate: function () {
                                return this.filterControl = $('<select/>', {
                                    class: 'form-control w100p',
                                    html: '<option value="">請選擇...</option>'
                                        + '<option value="已收文件">已收文件</option>'
                                        + '<option value="貨物進倉">貨物進倉</option>'
                                        + '<option value="報關作業">報關作業</option>'
                                        + '<option value="出口放行">出口放行</option>'
                                        + '<option value="航行中">航行中</option>'
                                        + '<option value="貨物抵港">貨物抵港</option>'
                                        + '<option value="貨物放行">貨物放行</option>'
                                        + '<option value="等待進場">等待進場</option>'
                                        + '<option value="送達攤位">送達攤位</option>'
                                        + ' <option value="退運">退運</option>'
                                });
                            }, filterValue: function () {
                                return this.filterControl.val();
                            }
                        },
                        {// ╠common.Other⇒其他╣
                            title: 'common.Other', width: 200,
                            itemTemplate: function (val, item) {
                                var oDom = [$("<a/>", {
                                    html: i18next.t('ExhibitionExport_Upd.ExportOperationFlow'),// ╠ExhibitionExport_Upd.ExportOperationFlow⇒出口作業流程╣
                                    class: 'a-url',
                                    click: function () {
                                        showWaiting();
                                        oCurSupplierData = item;

                                        layer.open({
                                            type: 2,
                                            title: i18next.t('ExhibitionExport_Upd.ExportOperationFlow'),// ╠ExhibitionExport_Upd.ExportOperationFlow⇒出口作業流程╣
                                            shade: 0.75,
                                            maxmin: true, //开启最大化最小化按钮
                                            area: ['90%', '80%'],
                                            content: '/Page/Pop/ExportFlowPop.html?' + new Date().valueOf(),
                                            btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                                            success: function (layero, index) {
                                                var iframe = $('iframe').contents(),
                                                    oForm = iframe.find('#form_ExportFlowPop');

                                                //加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
                                                if (item.ClearanceData && !item.ClearanceData.WaitingApproach) {
                                                    oForm.find('#ClearanceData_WaitingApproach_Checked').parents('.form-group').remove();
                                                }
                                                fnSGMod(oForm.find('.shjy-box'));//上海駒驛須移除選項
                                                oForm.find('.Control_Transport').html(sTransportOptionsHtml);

                                                setFormVal(oForm, item);
                                                setFlowBox(oForm, item);
                                                saBatchArr = [];
                                                iframe.find('#IsBatch').click(function () {
                                                    var that = this;
                                                    if (that.checked) {
                                                        if (saGridData.length > 1) {
                                                            var oConfig = {
                                                                Id: 'PopIsBatch',
                                                                Title: i18next.t('common.SelectBatchList'),// ╠common.SelectBatchList⇒請選擇要批次操作的資料╣
                                                                PageSize: 10000,
                                                                Get: fnGetPop_CusSupplier,
                                                                SearchFields: [
                                                                    { id: "Pop_SupplierName", type: 'text', i18nkey: 'common.SupplierName' },
                                                                    { id: "Pop_SupplierEName", type: 'text', i18nkey: 'common.SupplierEName' }
                                                                ],
                                                                Fields: [
                                                                    { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 50 },
                                                                    { name: "CustomerNO", title: 'Customers_Upd.CustomerNO', width: 80 },
                                                                    { name: "SupplierName", title: 'common.SupplierName', width: 150 },
                                                                    { name: "SupplierEName", title: 'common.SupplierEName', width: 150 }
                                                                ],
                                                                Callback: function (items) {
                                                                    $.each(items, function (idx, _item) {
                                                                        saBatchArr.push(_item.guid);
                                                                    });
                                                                    if (saBatchArr.length === 0) {
                                                                        $(that).click();
                                                                    }
                                                                },
                                                                CancelCallback: function () {
                                                                    $(that).click();
                                                                }
                                                            };
                                                            oPenPopm(oConfig);
                                                        }
                                                    }
                                                    else {
                                                        saBatchArr = [];
                                                    }
                                                });

                                                iframe.find('.quickquery-city').off('keyup').on('keyup', function () {
                                                    this.value = this.value.toUpperCase();
                                                }).off('blur').on('blur', function () {
                                                    var sId = this.value,
                                                        oPort = Enumerable.From(saPort).Where(function (e) { return e.id === sId; });
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

                                                transLang(iframe);
                                                closeWaiting();
                                            },
                                            yes: function (index, layero) {
                                                var iframe = $('iframe').contents(),
                                                    oForm = iframe.find('#form_ExportFlowPop'),
                                                    newData = getFormSerialize(oForm);

                                                delete item.ExportData;
                                                delete item.ClearanceData;
                                                delete item.ReImport;
                                                delete item.TranserThird;
                                                delete item.PartThird;
                                                delete item.PartReImport;
                                                delete item.ReImportFour;
                                                delete item.TransferFour;
                                                delete item.ReImportFive;
                                                delete item.TransferFive;
                                                delete item.ReImportSix;
                                                delete item.TransferSix;
                                                delete item.ReturnType;
                                                delete item.LastReturnType;
                                                delete item.ReturnType_4;
                                                delete item.ReturnType_5;

                                                if (newData.ReturnType === '') {
                                                    delete newData.ReImport;
                                                    delete newData.TranserThird;
                                                    delete newData.PartThird;
                                                    delete newData.PartReImport;
                                                    delete newData.TransferFour;
                                                    delete newData.ReImportFour;
                                                    delete newData.ReImportFive;
                                                    delete newData.TransferFive;
                                                    delete newData.ReImportSix;
                                                    delete newData.TransferSix;
                                                    delete newData.LastReturnType;
                                                    delete newData.ReturnType_4;
                                                    delete newData.ReturnType_5;
                                                }
                                                else if (newData.ReturnType === 'H') {
                                                    delete newData.TranserThird;
                                                    delete newData.PartThird;
                                                    delete newData.PartReImport;
                                                    delete newData.TransferFour;
                                                    delete newData.ReImportFour;
                                                    delete newData.ReImportFive;
                                                    delete newData.TransferFive;
                                                    delete newData.ReImportSix;
                                                    delete newData.TransferSix;
                                                    delete newData.LastReturnType;
                                                    delete newData.ReturnType_4;
                                                    delete newData.ReturnType_5;
                                                }
                                                else if (newData.ReturnType === 'T') {
                                                    delete newData.ReImport;
                                                    delete newData.PartThird;
                                                    delete newData.PartReImport;
                                                    if (newData.LastReturnType === '') {
                                                        delete newData.TransferFour;
                                                        delete newData.ReImportFour;
                                                        delete newData.ReImportFive;
                                                        delete newData.TransferFive;
                                                        delete newData.ReImportSix;
                                                        delete newData.TransferSix;
                                                        delete newData.LastReturnType;
                                                        delete newData.ReturnType_4;
                                                        delete newData.ReturnType_5;
                                                    }
                                                    else if (newData.LastReturnType === 'H') {
                                                        delete newData.TransferFour;
                                                        delete newData.ReImportFive;
                                                        delete newData.TransferFive;
                                                        delete newData.ReImportSix;
                                                        delete newData.TransferSix;
                                                        delete newData.ReturnType_4;
                                                        delete newData.ReturnType_5;
                                                    }
                                                    else if (newData.LastReturnType === 'F') {
                                                        delete newData.ReImportFour;
                                                        if (newData.ReturnType_4 === '') {
                                                            delete newData.ReImportFive;
                                                            delete newData.TransferFive;
                                                            delete newData.ReImportSix;
                                                            delete newData.TransferSix;
                                                            delete newData.ReturnType_5;
                                                        }
                                                        else if (newData.ReturnType_4 === 'H') {
                                                            delete newData.TransferFive;
                                                            delete newData.ReImportSix;
                                                            delete newData.TransferSix;
                                                            delete newData.ReturnType_5;
                                                        }
                                                        else if (newData.ReturnType_4 === 'F') {
                                                            delete newData.ReImportFive;
                                                            if (newData.ReturnType_5 === '') {
                                                                delete newData.ReImportSix;
                                                                delete newData.TransferSix;
                                                                delete newData.ReturnType_5;
                                                            }
                                                            else if (newData.ReturnType_5 === 'H') {
                                                                delete newData.TransferSix;
                                                            }
                                                            else if (newData.ReturnType_5 === 'F') {
                                                                delete newData.ReImportSix;
                                                            }
                                                        }
                                                    }
                                                }
                                                else {
                                                    delete newData.TranserThird;
                                                    delete newData.ReImport;
                                                    delete newData.TransferFour;
                                                    delete newData.ReImportFour;
                                                    delete newData.ReImportFive;
                                                    delete newData.TransferFive;
                                                    delete newData.ReImportSix;
                                                    delete newData.TransferSix;
                                                    delete newData.LastReturnType;
                                                    delete newData.ReturnType_4;
                                                    delete newData.ReturnType_5;
                                                }
                                                newData.Flow_Status = oCurData.Flow_Status = fnGetFlowStatus(newData);
                                                $.extend(item, item, newData);
                                                var saOthers = Enumerable.From(saGridData).Where(function (_item) { return _item.guid !== item.guid; }).ToArray();
                                                saOthers.push(item);
                                                saGridData = saOthers;
                                                if (saBatchArr.length > 0) {
                                                    $.each(saGridData, function (idx, _item) {
                                                        if (saBatchArr.indexOf(_item.guid) > -1 && _item.guid !== oCurSupplierData.guid) {
                                                            var oMach = clone(newData),
                                                                iExportData_Intowarehouse_Number = 0,
                                                                iTranserThird_Number = 0;
                                                            if (oMach.ReturnType === '') {
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0;
                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                            }
                                                            else if (oMach.ReturnType === 'H') {
                                                                var iReImport_Number = !(_item.ReImport) ? 0 : _item.ReImport.Number || 0;
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0
                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.ReImport = oMach.ReImport;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                                _item.ReImport.Number = iReImport_Number;
                                                            }
                                                            else if (oMach.ReturnType === 'T' && oMach.LastReturnType === 'H') {
                                                                var iReImportFour_Number = !(_item.ReImportFour) ? 0 : _item.ReImportFour.Number || 0;
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0;
                                                                iTranserThird_Number = !(_item.TranserThird) ? 0 : _item.TranserThird.Number || 0;

                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.TranserThird = oMach.TranserThird;
                                                                _item.ReImportFour = oMach.ReImportFour;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                                _item.TranserThird.Number = iTranserThird_Number;
                                                                _item.ReImportFour.Number = iReImportFour_Number;
                                                            }
                                                            else if (oMach.ReturnType === 'T' && oMach.LastReturnType === 'F') {
                                                                var iTransferFour_Number = !(_item.TransferFour) ? 0 : _item.TransferFour.Number || 0;
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0;
                                                                iTranserThird_Number = !(_item.TranserThird) ? 0 : _item.TranserThird.Number || 0;
                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.TranserThird = oMach.TranserThird;
                                                                _item.TransferFour = oMach.TransferFour;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                                _item.TranserThird.Number = iTranserThird_Number;
                                                                _item.TransferFour.Number = iTransferFour_Number;
                                                            }
                                                            else if (oMach.ReturnType === 'T') {
                                                                iTranserThird_Number = !(_item.TranserThird) ? 0 : _item.TranserThird.Number || 0;
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0;
                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.TranserThird = oMach.TranserThird;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                                _item.TranserThird.Number = iTranserThird_Number;
                                                            }
                                                            else if (oMach.ReturnType === 'P') {
                                                                var iPartThird_Number = !(_item.PartThird) ? 0 : _item.PartThird.Number || 0,
                                                                    iPartReImport_Number = !(_item.PartReImport) ? 0 : _item.PartReImport.Number || 0;
                                                                iExportData_Intowarehouse_Number = !(_item.ExportData) ? 0 : _item.ExportData.Intowarehouse.Number || 0;
                                                                _item.ExportData = oMach.ExportData;
                                                                _item.ClearanceData = oMach.ClearanceData;
                                                                _item.PartThird = oMach.PartThird;
                                                                _item.PartReImport = oMach.PartReImport;
                                                                _item.ExportData.Intowarehouse.Number = iExportData_Intowarehouse_Number;
                                                                _item.PartThird.Number = iPartThird_Number;
                                                                _item.PartReImport.Number = iPartReImport_Number;
                                                            }
                                                            _item.ReturnType = oMach.ReturnType;
                                                            _item.LastReturnType = oMach.LastReturnType;
                                                            _item.Flow_Status = fnGetFlowStatus(_item);
                                                        }
                                                    });
                                                    oGrid.loadData();
                                                }

                                                layer.close(index);
                                            }
                                        });

                                        return false;
                                    }
                                }),//出口作業流程
                                $('<a/>', {
                                    html: i18next.t('common.MailTrackingNumber'),// ╠common.MailTrackingNumber⇒寄送客戶查詢碼╣
                                    class: item.guid + ((oCurData.IsSendMail || '').indexOf(item.guid) > -1 ? ' a-mailurl' : ' a-url'),
                                    click: function () {
                                        fnSendTrackingNumberEmail(item);
                                        return false;
                                    }
                                })
                                ],
                                    oVoid = $('<a/>', {
                                        // ╠common.Toolbar_OpenVoid⇒啟用╣  ╠common.Toolbar_Void⇒作廢╣
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
                                            $(that).hide();
                                            fnPushBill(item, oCurData, item.guid, false).done(function () {
                                                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                                                    Params: {
                                                        exportexhibition: {
                                                            values: {
                                                                Bills: JSON.stringify(oCurData.Bills),
                                                                Exhibitors: JSON.stringify(saGridData)
                                                            },
                                                            keys: { ExportBillNO: sDataId }
                                                        }
                                                    }
                                                }, function (res) {
                                                    if (res.d > 0) {
                                                        fnBindBillLists();
                                                        $(that).remove();
                                                        showMsg(i18next.t("common.Create_Success"), 'success'); // ╠common.Create_Success⇒創建成功╣
                                                    }
                                                    else {
                                                        $(that).show();
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
                                    if (iExsit === 0 || (iExsit - iExsitVoid) === 0) {
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
                            if (sAction === 'Upd') {
                                if (saGridData.length > 0) {
                                    $('.batch-mail').show();
                                    $('.batch-import').hide();
                                }
                                else {
                                    $('.batch-mail').hide();
                                    $('.batch-import').show();
                                }
                            }

                            if (args.Contactor !== undefined) {
                                var filters = $.grep(saGridData, function (client) {
                                    return (!args.Contactor || client.ContactorName.indexOf(args.Contactor) > -1)
                                        && (!args.Telephone || client.Telephone.indexOf(args.Telephone) > -1)
                                        && (!args.Email || client.Email.indexOf(args.Email) > -1)
                                        && (!args.Flow_Status || (client.Flow_Status || '').indexOf(args.Flow_Status) > -1)
                                        && (!args.SupplierEName || client.SupplierEName.indexOf(args.SupplierEName) > -1)
                                        && (!args.SupplierName || args.SupplierName.indexOf(client.SupplierName) > -1)
                                        && (!args.RefSupplierNo || client.RefSupplierNo.indexOf(args.RefSupplierNo) > -1);
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
                                showMsg(i18next.t("message.SaveCusFirst"))// ╠message.SaveCusFirst⇒請先儲存再新增廠商╣
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
                            if (args.BillNO || (oCurData.IsSendMail || '').indexOf(args.guid) > -1) {
                                showMsg(i18next.t("message.NotToDelete_Supplier")); // ╠message.NotToDelete_Supplier⇒該筆廠商已經產生帳單或寄送過郵件，不可以刪除！╣
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
                            }
                            bRequestStorage = true;
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
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Imp":

                        fnImport();

                        break;
                    case "Toolbar_Exp":

                        //fnOpenPopToExcel();

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
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /**
             * 初始化 function
             */
            init = function () {
                var saCusBtns = [],
                    myHelpers = {
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
                if (sAction !== 'Upd') {
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
                            case 'litab2':
                                if (!$(el).data('action')) {
                                    fnGridInit();
                                    oGrid.loadData();
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
                                    fnOpenAccountingArea($('div .BillOrganizers'), parent.UserInfo.roles);
                                }
                                break;
                            case 'litab5':
                            case 'litab6':
                                if (!$(el).data('action')) {
                                    if (oCurData.ReturnBills.length > 0) {
                                        fnRenderReturnBills();
                                    }
                                    $('#litab5').data('action', true);
                                    $('#litab6').data('action', true);
                                    fnOpenAccountingArea($('div .OnlyForAccounting'), parent.UserInfo.roles);
                                }
                                break;
                        }
                    }
                });

                //加載報關類別,加載成本頁簽
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
                            ServiceCode: parent.SysSet.EXCode,
                            CallBack: function (data) {
                                var sCode = parent.UserInfo.ServiceCode;
                                if (sAction === 'Add' && sCode && parent.SysSet.EXCode.indexOf(sCode) > -1) {
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
                            ArgClassID: 'Transport',
                            Select: $('#TransportationMode'),
                            ShowId: true,
                            CallBack: function (data) {
                                sTransportOptionsHtml = createOptions(data, 'id', 'text', true);
                            }
                        },
                        {
                            ArgClassID: 'Port',
                            CallBack: function (data) {
                                saPort = data;
                                $('.quickquery-city').on('keyup', function () {
                                    this.value = this.value.toUpperCase();
                                }).on('blur', function () {
                                    var sId = this.value,
                                        oPort = Enumerable.From(saPort).Where(function (e) { return e.id === sId; });
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
                    ])
                ])
                    .done(function (res) {
                        if (res && res[0].RESULT === 1) {
                            var oRes = res[0].DATA.rel;
                            fnGetCurrencyThisYear(oRes.CreateDate).done(function () {
                                oRes.Exhibitors = saGridData = !oRes.Exhibitors ? [] : JSON.parse(oRes.Exhibitors);

                                oRes.Quote = JSON.parse(oRes.Quote || '{}');
                                oRes.EstimatedCost = JSON.parse(oRes.EstimatedCost || '{}');
                                oRes.ActualCost = JSON.parse(oRes.ActualCost || '{}');
                                oRes.Bills = JSON.parse(oRes.Bills || '[]');
                                oRes.ReturnBills = !oRes.ReturnBills ? [] : JSON.parse(oRes.ReturnBills);
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
                                //預設產生帳單組團單位
                                $.each(sOrganizers, function (idx, sO) {
                                    if (sO) {
                                        let OrganizersIndx = idx + 1
                                        $('#Organizer' + OrganizersIndx).val(sO).trigger('change.select2');
                                        var AKOf = Enumerable.From(saCustomers).Where(function (e) { return e.id === sO; }).FirstOrDefault();
                                        if (AKOf)
                                            sSelectedOrganizers[OrganizersIndx] = { id: AKOf.id, text: AKOf.text, title: AKOf.text };
                                    }
                                });
                                $('[data-id="BillOrganizer"]').html(createOptions(sSelectedOrganizers.filter(e => e != null), 'id', 'text'));
                                //預設產生帳單組團單位-END
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
                                $('#Batch_Mail').on('click', function () {
                                    fnBatchMail();
                                });
                                $('#Batch_Import').on('click', function () {
                                    fnBatchImport();
                                });

                                if (parent.UserInfo.MemberID === oCurData.ResponsiblePerson || parent.UserInfo.MemberID === oCurData.CreateUser) {
                                    $('#returnquote_add').removeAttr('disabled').click(function () {
                                        fnPushReturnBill();
                                    });
                                    $('.returnquote-add').show();
                                }
                                moneyInput($('[data-type="int"]'), 0);
                                var Authorized = ExhibitionBillAuthorize(oCurData);
                                if (Authorized) {
                                    $('[href="#tab3"],[href="#tab4"],[href="#tab5"],[href="#tab6"]').parent().show();
                                    if (sGoTab) {
                                        $('#litab' + sGoTab).find('a').click();
                                        if (sBillNOGO && $('.bill-box-' + sBillNOGO).length > 0) {
                                            $('.bill-box-' + sBillNOGO).click();
                                            goToJys($('.bill-box-' + sBillNOGO));
                                        }
                                    }
                                }
                                else {
                                    $('[href="#tab3"],[href="#tab4"],[href="#tab5"],[href="#tab6"]').parent().hide();
                                }
                            });
                        }
                        else
                            fnGetCurrencyThisYear(new Date());
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
                        minInterval: (1000 * 60 * 60 * 24 * 1), // 1 days
                        changeYear: true,
                        changeMonth: true
                    }
                );

                //$('#Volume').on('blur', function () {
                //    var sVal = this.value;
                //    $('#VolumeWeight').val((Math.floor(sVal * 100) / 100 * 167).toFloat(2));

                //});

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
                                $('#ExportBillEName').val(oExhibition.Exhibitioname_EN);
                                if (oExhibition.ExhibitionDateStart) {
                                    $('#ExhibitionDateStart').val(newDate(oExhibition.ExhibitionDateStart, 'date'));
                                }
                                if (oExhibition.ExhibitionDateEnd) {
                                    $('#ExhibitionDateEnd').val(newDate(oExhibition.ExhibitionDateEnd, 'date'));
                                }
                            }
                        });
                    }
                    else {
                        $('#ExportBillEName').val('');
                        $('#ExhibitionDateStart').val('');
                        $('#ExhibitionDateEnd').val('');
                    }
                });

                $(window).on('scroll', function () {
                    var h = ($(document).height(), $(this).scrollTop());
                    if (h < 81) {
                        $('.sum-box').css({ top: 125 - h });
                        $('.BatchBillsDownLoad').css({ top: 140 - h });
                    }
                    else {
                        $('.sum-box,.BatchBillsDownLoad').css({ top: 44 });
                    }
                });
                $('#Batch_BillsDownLoad').on('click', function () {
                });
                $('#Batch_RerurnBillsDownLoad').on('click', function () {
                });
            };

        init();
    };

require(['base', 'select2', 'autocompleter', 'formatnumber', 'jquerytoolbar', 'timepicker', 'jsgrid', 'ajaxfile', 'common_opm', 'util'], fnPageInit, 'timepicker');