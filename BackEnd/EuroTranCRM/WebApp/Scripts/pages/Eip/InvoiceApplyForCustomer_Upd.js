'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sViewPrgId = sProgramId.replace('_Upd', '_View'),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckOrder: [], PayeeInfo: [] },
            oForm = $('#form_main'),
            oGrid = null,
            oValidator = null,
            sOptionHtml_PrjCode = '',
            sOptionHtml_Currency = '',
            sOptionHtml_Bills = '',
            sOptionHtml_ComplaintNumber = '',
            oAddItem = {},
            saUsers = [],
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
                                oCurData = oRes;
                                oCurData.PayeeInfo = $.parseJSON(oCurData.PayeeInfo);
                                oCurData.RemittanceInformation = $.parseJSON(oCurData.RemittanceInformation);
                                oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                                setFormVal(oForm, oRes);
                                $('.Applicant').text(oCurData.ApplicantName + '(' + oCurData.Applicant + ')  ' + oCurData.DeptName);
                                $('#PayeeCode').text(oCurData.CustomerNO);
                                fnGetUploadFiles(oCurData.Guid, fnUpload);
                                if (oCurData.Handle_DeptID) {
                                    fnSetUserDrop([
                                        {
                                            Select: $('#Handle_Person'),
                                            DepartmentID: oCurData.Handle_DeptID,
                                            ShowId: true,
                                            Select2: true,
                                            Action: sAction,
                                            DefultVal: oCurData.Handle_Person
                                        }
                                    ]);
                                }
                                if (oCurData.Flows_Lock === 'Y') {
                                    $(".checkordertoolbox").hide();
                                }
                                else {
                                    $(".checkordertoolbox").show();
                                }
                                if (oCurData.Handle_Lock === 'Y') {
                                    $("#Handle_DeptID,#Handle_Person").attr('disabled', true);
                                }
                                else {
                                    $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                }
                                $("#jsGrid").jsGrid("loadData");
                                $("#jsGrid1").jsGrid("loadData");
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
                            }
                        });
                }
                else {
                    $('.Applicant').text(parent.UserInfo.MemberName + '(' + parent.UserInfo.MemberID + ')  ' + parent.UserInfo.DepartmentName);
                    $('#Applicant').val(parent.UserInfo.MemberID);
                    oCurData.PayeeInfo = [];
                    oCurData.CheckOrder = [];
                    oCurData.Guid = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} sFlag 新增或儲存後新增
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Guid = oCurData.Guid;
                data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|IAC|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.PayeeInfo = JSON.stringify(oCurData.PayeeInfo);
                data.RemittanceInformation = JSON.stringify(data.RemittanceInformation);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Status = 'A';
                data.IsHandled = 'N';
                data.PayeeType = 'C';
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.PayeeName = $('#Payee option:selected').text();

                if (data.PaymentType === 'A') {
                    delete data.PaymentTime;
                }

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        invoiceapplyinfo: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (flag == 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&Guid=' + data.Guid); // ╠message.Save_Success⇒新增成功╣
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
            },
            /**
             * 修改資料
             * @param {Boolean} balert 是否提示
             */
            fnUpd = function (balert) {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.PayeeInfo = JSON.stringify(oCurData.PayeeInfo);
                data.RemittanceInformation = JSON.stringify(data.RemittanceInformation);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.PayeeName = $('#Payee option:selected').text();

                if (data.PaymentType === 'A') {
                    delete data.PaymentTime;
                }

                return CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        invoiceapplyinfo: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        if (!balert) {
                            bRequestStorage = false;
                            showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                            if (window.bLeavePage) {
                                setTimeout(function () {
                                    pageLeave();
                                }, 1000);
                            }
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                });
            },
            /**
             * 資料刪除
             */
            fnDel = function () {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        invoiceapplyinfo: {
                            Guid: sDataId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        DelTask(sDataId);
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
             * 設定受款人下拉選單
             */
            setPayeeDrop = function () {
                return g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
                    if (res.RESULT) {
                        var saPayee = res.DATA.rel;
                        if (saPayee.length > 0) {
                            $('#Payee').html(createOptions(saPayee, 'id', 'text')).change(function () {
                                var sId = this.value,
                                    oPayee = $.grep(saPayee, function (item) { return item.id === sId; })[0];
                                $('#PayeeCode').text(oPayee.CusNO);
                            });
                            select2Init($('#Payee').parent());
                        }
                    }
                });
            },
            /**
             * 上傳附件
             * @param {Array} files 上傳的文件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'InvoiceApplyForCustomer';
                option.type = 'list';
                option.parentid = oCurData.Guid;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 提交簽呈
             */
            fnSubmitPetition = function () {
                g_api.ConnectLite(sProgramId, 'InvoiceApplyForCustomerToAudit', {
                    guid: oCurData.Guid
                }, function (res) {
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t("message.ToAudit_Success"), sViewPrgId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.ToAudit_Success⇒提交審核成功╣
                        parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                    }
                    else {
                        showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                });
            },
            /**
             * 提交費用項目
             * TE、TG以TWD為主，其他幣別為輔。
             * SG以RMB為主，其他幣別為輔。
             */
            fnSumPayeeInfo = function () {
                //TE、TG以TWD為主，其他幣別為輔。
                //SG以RMB為主，其他幣別為輔。
                var iTotal_MainCurrency = 0;
                var iTotal_SecondCurrency = 0;
                var MainRoundingPoint = 0;
                var MainCurrency = 'NTD';
                var SecondCurrency = 'NTD';
                var SecondRoundingPoint = 2;
                if (parent.OrgID === 'SG') {
                    MainCurrency = 'RMB';
                    SecondCurrency = 'RMB';
                    MainRoundingPoint = 2;
                }

                $.each(oCurData.PayeeInfo, function (idx, info) {
                    let PayeeAmount = parseFloat((info.Amount || '0').toString().replaceAll(',', ''));
                    if (info.Currency === MainCurrency) {
                        iTotal_MainCurrency += PayeeAmount;
                    }
                    else {
                        iTotal_SecondCurrency += PayeeAmount;
                        SecondCurrency = info.Currency;
                        if (info.Currency === 'NTD') {
                            SecondRoundingPoint = 0;
                        }
                    }
                });
                
                $('#RemittanceInformation_TotalCurrencyTW').val(MainCurrency);
                $('#RemittanceInformation_InvoiceApplyTotalTW').val(fMoney(iTotal_MainCurrency, MainRoundingPoint, MainCurrency));
                $('#RemittanceInformation_TotalCurrency').val(SecondCurrency);
                $('#RemittanceInformation_InvoiceApplyTotal').val(fMoney(iTotal_SecondCurrency, SecondRoundingPoint, ''));
            },
            /**
             * 通過帳單號碼抓去專案代號
             * @param {HTMLElement} dom select控件
             */
            fnGetPrjCodeByBillNO = function (dom) {
                var sBillNO = dom.value;
                g_api.ConnectLite(Service.eip, 'GetPrjCodeByBillNO', {
                    BillNO: sBillNO
                }, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        $(dom).parent().next().next().find('select').val(oRes.PrjCode).trigger("change");
                    }
                });
            },
            /**
             * 抓取客訴編號
             * @param {string} Guid
             */
            fnGetComplaintNumber = function (o) {
                g_api.ConnectLite('CrmCom', 'GetComplaintNumber', {
                    Guid: o.Guid || ''
                }, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (o.CallBack && typeof o.CallBack === 'function') {
                            o.CallBack(oRes);
                        }
                    }
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param {Object}inst  按鈕物件對象
             * @param {Object} e 事件對象
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
                    case "Toolbar_Petition":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        fnUpd(true).done(function () {
                            fnSubmitPetition();
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
                var saCusBtns = null;

                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'Toolbar_Petition',
                        value: 'common.SubmitPetition'// ╠common.SubmitPetition⇒提交簽呈╣
                    }];
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true
                });
                oValidator = $("#form_main").validate();

                $.whenArray([
                    fnSetDeptDrop($('#Handle_DeptID')),
                    fnGetBills({
                        CallBack: function (data) {
                            sOptionHtml_Bills = createOptions(data, 'BillNO', 'BillNO');
                        }
                    }),
                    fnGetComplaintNumber({
                        CallBack: function (data) {
                            sOptionHtml_ComplaintNumber = createOptions(data, 'ComplaintNumber', 'ComplaintNumber');
                        }
                    }),
                    setPayeeDrop(),
                    fnSetFlowDrop({
                        Flow_Type: parent.SysSet.Eip_006,
                        ShareTo: parent.UserID,
                        CallBack: function (data) {
                            $.each(data, function (idx, item) {
                                var saFlows = $.parseJSON(item.Flows),
                                    saFlowsText = [],
                                    sFlowsText = '';
                                $.each(saFlows, function (idx, flow) {
                                    var sFlowType = i18next.t('common.' + flow.SignedWay);
                                    if (flow.SignedWay !== 'flow1') {
                                        saFlowsText.push(sFlowType + '(' + Enumerable.From(flow.SignedMember).ToString("，", "$.name") + ')');
                                    }
                                    else {
                                        saFlowsText.push(Enumerable.From(flow.SignedMember).ToString("，", "$.name"));
                                    }
                                });
                                sFlowsText = saFlowsText.join(' → ');
                                item.text = item.Flow_Name + ' - ' + (sFlowsText.length > 60 ? sFlowsText.substr(0, 60) + '...' : sFlowsText);
                            });
                            $('#FlowId').html(createOptions(data, 'Guid', 'text')).on('change', function () {
                                var sFlowId = this.value;
                                if (sFlowId) {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            checkflow: {
                                                Guid: sFlowId
                                            }
                                        }
                                    }, function (res) {
                                        if (res.d) {
                                            var oRes = $.parseJSON(res.d);
                                            oRes.Flows = $.parseJSON(oRes.Flows);
                                            oCurData.CheckOrder = oRes.Flows;
                                            oCurData.Flows_Lock = oRes.Flows_Lock;
                                            oCurData.Handle_Lock = oRes.Handle_Lock;
                                            //選擇新流程，變更現有經辦人(Handle_Person)與經辦部門(Handle_DeptID)。
                                            //因為新增或者修改是fnHandleFlows(oCurData,saUsers); //Mark 20190617
                                            oCurData.Handle_Person = oRes.Handle_Person;
                                            oCurData.Handle_DeptID = oRes.Handle_DeptID;
                                            oCurData.Handle_PersonName = oRes.Handle_PersonName;
                                            //設置經辦部門
                                            $("#Handle_DeptID").val(oRes.Handle_DeptID);

                                            //設置經辦人員
                                            fnSetUserDrop([
                                                {
                                                    Select: $('#Handle_Person'),
                                                    DepartmentID: oRes.Handle_DeptID,
                                                    ShowId: true,
                                                    Select2: true,
                                                    Action: sAction,
                                                    DefultVal: oRes.Handle_Person
                                                }
                                            ]);

                                            //$("#Handle_Person").val(oRes.Handle_Person).trigger('change');

                                            if (oRes.Flows_Lock === 'Y') {
                                                $(".checkordertoolbox").hide();
                                            }
                                            else {
                                                $(".checkordertoolbox").show();
                                            }
                                            if (oRes.Handle_Lock === 'Y') {
                                                $("#Handle_DeptID,#Handle_Person").attr('disabled', true);
                                            }
                                            else {
                                                $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                            }
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    });
                                }
                                else {
                                    oCurData.CheckOrder = [];
                                    $(".checkordertoolbox").hide();
                                    $("#jsGrid").jsGrid("loadData");
                                    $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                }
                            });
                        }
                    }),
                    fnSetEpoDrop({
                        CallBack: function (data) {
                            sOptionHtml_PrjCode = createOptions(data, 'ExhibitionCode', 'ExhibitioShotName_TW');
                        }
                    }),
                    fnSetUserDrop([
                        {
                            ShowId: true,
                            Select: $('#Handle_Person'),
                            Select2: true,
                            Action: sAction,
                            CallBack: function (data) {
                                saUsers = data;
                            }
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'Currency',
                            CallBack: function (data) {
                                sOptionHtml_Currency = createOptions(data, 'id', 'id');
                                if (parent.OrgID === 'SG') {
                                    $('#RemittanceInformation_TotalCurrencyTW,#RemittanceInformation_TotalCurrency').html(sOptionHtml_Currency).val('RMB')[0].remove(0);
                                }
                                else {
                                    $('#RemittanceInformation_TotalCurrencyTW,#RemittanceInformation_TotalCurrency').html(sOptionHtml_Currency).val('NTD')[0].remove(0);
                                }
                            }
                        }
                    ])])
                    .done(function () {
                        fnGet();

                        $("#jsGrid1").jsGrid({
                            width: "100%",
                            height: "auto",
                            autoload: true,
                            filtering: false,
                            inserting: true,
                            editing: true,
                            pageLoading: true,
                            confirmDeleting: true,
                            invalidMessage: i18next.t('common.InvalidData'),// ╠common.InvalidData⇒输入的数据无效！╣
                            deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                            pageIndex: 1,
                            pageSize: 10000,
                            fields: [
                                {// ╠common.FeeItemName⇒費用名稱╣
                                    name: "FeeItemName", title: 'common.FeeItemName', width: 150, type: "text", validate: { validator: 'required', message: i18next.t('common.FeeItemName_required') },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p"
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p",
                                            value: val
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    name: "BillNO", title: 'common.BillNO', width: 140, type: "text",
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w50p",
                                            html: sOptionHtml_Bills,
                                            change: function () {
                                                fnGetPrjCodeByBillNO(this);
                                            }
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 100);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w50p",
                                            html: sOptionHtml_Bills,
                                            change: function () {
                                                fnGetPrjCodeByBillNO(this);
                                            }
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 100);
                                        return this.editControl = oControl.val(item.BillNO);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    name: "ComplaintNumber", title: '客訴編號', width: 140, type: "text",
                                    itemTemplate: function (val, item) {
                                        return val + (!item.ComplaintNumber ? '' : '（' + item.ComplaintNumber + '）');
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w80p",
                                            html: sOptionHtml_ComplaintNumber
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 1000);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        var SelectedOption = this.insertControl.find('option:selected');
                                        if (this.insertControl.val() && SelectedOption.val()) {
                                            oAddItem.ComplaintNumber = SelectedOption.text();
                                        }
                                        else {
                                            oAddItem.ComplaintNumber = '';
                                        }
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w80p",
                                            html: sOptionHtml_ComplaintNumber,
                                            change: function () {
                                                var SelectedOption = $(this).find('option:selected');
                                                if (SelectedOption.val()) {
                                                    item.ComplaintNumber = SelectedOption.text();
                                                }
                                                else {
                                                    item.ComplaintNumber = '';
                                                }
                                            }
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 100);
                                        return this.editControl = oControl.val(item.ComplaintNumber);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    } 
                                },
                                {
                                    name: "PrjCode", title: 'common.PrjCode', width: 140, type: "text",
                                    itemTemplate: function (val, item) {
                                        return val + (!item.PrjName ? '' : '（' + item.PrjName + '）');
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w80p",
                                            html: sOptionHtml_PrjCode
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 1000);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        var SelectedOption = this.insertControl.find('option:selected');
                                        if (this.insertControl.val() && SelectedOption.val()) {
                                            oAddItem.PrjName = SelectedOption.text();
                                        }
                                        else {
                                            oAddItem.PrjName = '';
                                        }
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w80p",
                                            html: sOptionHtml_PrjCode,
                                            change: function () {
                                                var SelectedOption = $(this).find('option:selected');
                                                if (SelectedOption.val()) {
                                                    item.PrjName = SelectedOption.text();
                                                }
                                                else {
                                                    item.PrjName = '';
                                                }
                                            }
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '180px' });
                                        }, 100);
                                        return this.editControl = oControl.val(item.PrjCode);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    name: "Currency", title: 'common.Financial_Currency', width: 60, type: "text", validate: { validator: 'required', message: i18next.t('common.Currency_required') },// ╠common.Currency_required⇒請選擇幣別╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_Currency
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_Currency
                                        });
                                        return this.editControl = oControl.val(item.Currency);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    name: "Amount", title: 'common.Financial_Amount', width: 70, type: "text", align: "right", validate: { validator: 'required', message: i18next.t('common.Amount_required') },
                                    itemTemplate: function (val, item) {
                                        return val.toString().toMoney();
                                    },
                                    insertTemplate: function (val, item) {

                                        var oControl = $('<input />', {
                                            class: "form-control w100p",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            value: val
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        if (this._grid.fields[3].insertControl.val() == 'NTD') {
                                            let AmountStr = this.insertControl.val().replaceAll(',', '');
                                            let PositiveInt = parseInt(AmountStr);
                                            this.insertControl.val(PositiveInt);
                                            this.insertControl[0].dataset.value = PositiveInt;
                                        }
                                        return this.insertControl.attr('data-value');
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            value: val
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.editControl = oControl.val(val);
                                    },
                                    editValue: function () {
                                        if (this._grid.fields[3].editControl.val() == 'NTD') {
                                            let AmountStr = this.editControl.val().replaceAll(',', '');
                                            let PositiveInt = parseInt(AmountStr);
                                            this.editControl.val(PositiveInt);
                                            this.editControl[0].dataset.value = PositiveInt;
                                        }
                                        return this.editControl.attr('data-value');
                                    }
                                },
                                {
                                    type: "control", width: 50
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return {
                                        data: oCurData.PayeeInfo,
                                        itemsCount: oCurData.PayeeInfo.length //data.length
                                    };
                                },
                                insertItem: function (args) {
                                    args.guid = guid();
                                    args.Index = oCurData.PayeeInfo.length + 1;
                                    args.PrjName = oAddItem.PrjName;
                                    oCurData.PayeeInfo.push(args);
                                    fnSumPayeeInfo();
                                },
                                updateItem: function (args) {
                                    $.each(oCurData.PayeeInfo, function (idx, _data) {
                                        if (_data.guid === args.guid) {
                                            _data.Amount = args.Amount;
                                            _data.BillNO = args.BillNO;
                                            _data.Currency = args.Currency;
                                            _data.FeeItemName = args.FeeItemName;
                                            _data.PrjCode = args.PrjCode;
                                            _data.PrjName = args.PrjName;
                                        }
                                    });
                                    fnSumPayeeInfo();
                                },
                                deleteItem: function (args) {
                                    var saPayeeInfo = [];
                                    $.each(oCurData.PayeeInfo, function (idx, _data) {
                                        if (_data.guid !== args.guid) {
                                            saPayeeInfo.push(_data);
                                        }
                                    });
                                    $.each(saPayeeInfo, function (idx, _data) {
                                        _data.Index = idx + 1;
                                    });
                                    oCurData.PayeeInfo = saPayeeInfo;
                                    fnSumPayeeInfo();
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                    });
                $('#Handle_DeptID').on('change', function () {
                    fnSetUserDrop([
                        {
                            Select: $('#Handle_Person'),
                            DepartmentID: this.value,
                            ShowId: true,
                            Select2: true,
                            Action: sAction
                        }
                    ]);
                });
                $('#Agent_Person').on('change', function () {
                    oCurData.Agent_Person = this.value;
                });

                $('[name="PaymentType"]').on('click', function () {
                    if (this.value === 'A') {
                        $('#PaymentTime').removeAttr('required');
                    }
                    else {
                        $('#PaymentTime').attr('required', true);
                    }
                });

                $('.flowlink').on('click', function () {
                    var oOption = {};
                    oOption.SignedWay = this.id;
                    oOption.Callback = function (data) {
                        if (data.Users.length > 0) {
                            var oFlow = {};
                            if (data.FlowType === 'flow1') {
                                $.each(data.Users, function (idx, user) {
                                    oFlow = {};
                                    oFlow.id = guid();
                                    oFlow.Order = oCurData.CheckOrder.length + 1;
                                    oFlow.SignedWay = data.FlowType;
                                    oFlow.SignedMember = [{
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    }];
                                    oCurData.CheckOrder.push(oFlow);
                                });
                            }
                            else {
                                var saSignedMember = [];
                                $.each(data.Users, function (idx, user) {
                                    saSignedMember.push({
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    });
                                });
                                oFlow.id = guid();
                                oFlow.Order = oCurData.CheckOrder.length + 1;
                                oFlow.SignedWay = data.FlowType;
                                oFlow.SignedMember = saSignedMember;
                                oCurData.CheckOrder.push(oFlow);
                            }
                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                            $("#jsGrid").jsGrid("loadData");
                        }
                    };
                    oPenUserListPop(oOption);
                });

                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    filtering: false,
                    pageLoading: true,
                    pageIndex: 1,
                    pageSize: 10000,
                    fields: [
                        {
                            name: "Order", title: 'common.Order', width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                return val < 10 ? '0' + val : val;
                            }
                        },
                        {
                            name: "SignedWay", title: 'common.SignedWay', width: 120, align: "center",
                            itemTemplate: function (val, item) {
                                return i18next.t('common.' + val);
                            }
                        },
                        {
                            type: "Icon", width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                var oIcon = {
                                    flow1: '<img src="../../images/flow_check.gif">',
                                    flow2: '<img src="../../images/flow_check.gif"><img src="../../images/flow_check.gif">',
                                    flow3: '<img src="../../images/flow_check.gif"><img src="../../images/flow_nocheck.gif">',
                                    flow4: '<img src="../../images/flow4.gif">'
                                },
                                    sIcon = oIcon[item.SignedWay];
                                if (item.Order !== oCurData.CheckOrder.length) {
                                    sIcon += '<br><img src="../../images/flow_arrow.gif" style="vertical-align:top;">'
                                }
                                return sIcon;
                            }
                        },
                        {
                            name: "SignedMember", title: 'common.SignedMember', width: 500,
                            itemTemplate: function (val, item) {
                                return Enumerable.From(val).ToString("，", "$.name");
                            }
                        },
                        {
                            type: "control", title: 'common.Action', width: 200,
                            itemTemplate: function (val, item) {
                                var oBtns = [$('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                    class: 'glyphicon glyphicon-pencil' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                    title: i18next.t('common.Edit'),// ╠common.Edit⇒編輯╣
                                    click: function () {
                                        if ($(this).hasClass('disabled')) { return false; }
                                        var oOption = {};
                                        oOption.SignedWay = item.SignedWay;
                                        oOption.SignedMember = item.SignedMember;
                                        oOption.Callback = function (data) {
                                            if (data.Users.length > 0) {
                                                var oFlow = {};
                                                if (data.FlowType === 'flow1') {
                                                    $.each(data.Users, function (idx, user) {
                                                        var oFlow = {};
                                                        oFlow.id = guid();
                                                        oFlow.Order = item.Order + idx;
                                                        oFlow.SignedWay = data.FlowType;
                                                        oFlow.SignedMember = [{
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        }];
                                                        oCurData.CheckOrder.insert(item.Order + idx, oFlow);
                                                    });
                                                }
                                                else {
                                                    var saSignedMember = [];
                                                    $.each(data.Users, function (idx, user) {
                                                        saSignedMember.push({
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        });
                                                    });
                                                    oFlow.id = guid();
                                                    oFlow.Order = item.Order;
                                                    oFlow.SignedWay = data.FlowType;
                                                    oFlow.SignedMember = saSignedMember;
                                                    oCurData.CheckOrder.insert(item.Order, oFlow);
                                                }
                                                var iOrder = 1;
                                                $.each(oCurData.CheckOrder, function (idx, _data) {
                                                    if (item.id !== _data.id) {
                                                        _data.Order = iOrder;
                                                        iOrder++;
                                                    }
                                                });
                                                oCurData.CheckOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id !== item.id; }).ToArray();
                                                oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                                $("#jsGrid").jsGrid("loadData");
                                            }
                                        };
                                        oPenUserListPop(oOption);
                                    }
                                })),
                                $('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                    class: 'glyphicon glyphicon-trash' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                    title: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                    click: function () {
                                        if ($(this).hasClass('disabled')) { return false; }

                                        var saNewList = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id !== item.id; }).ToArray();
                                        oCurData.CheckOrder = saNewList;
                                        $.each(oCurData.CheckOrder, function (idx, _data) {
                                            _data.Order = idx + 1;
                                        });
                                        oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                        $("#jsGrid").jsGrid("loadData");
                                    }
                                }))];

                                if (oCurData.CheckOrder.length !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-down' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                        title: i18next.t('common.Down'),// ╠common.Down⇒下移╣
                                        click: function () {
                                            if ($(this).hasClass('disabled')) { return false; }
                                            var sOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(oCurData.CheckOrder, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order++;
                                                }
                                                else if ((iOrder + 1) === _data.Order) {
                                                    _data.Order--;
                                                }
                                            });
                                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    })));
                                }
                                else {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }));
                                }

                                if (1 !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-up' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                        title: i18next.t('common.Up'),// ╠common.Up⇒上移╣
                                        click: function () {
                                            if ($(this).hasClass('disabled')) { return false; }
                                            var sOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(oCurData.CheckOrder, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order--;
                                                }
                                                else if ((iOrder - 1) === _data.Order) {
                                                    _data.Order++;
                                                }
                                            });
                                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    })));
                                }

                                return oBtns;
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: oCurData.CheckOrder,
                                itemsCount: oCurData.CheckOrder.length //data.length
                            };
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    }
                });
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'formatnumber', 'filer', 'common_eip', 'util'], fnPageInit);