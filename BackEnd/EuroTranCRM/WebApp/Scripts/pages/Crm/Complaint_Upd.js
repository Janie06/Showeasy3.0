'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sViewPrgId = sProgramId.replace('_Upd', '_View'),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
	sCustomerId = getUrlParam('CustomerId'),
	sContactorName = getUrlParam('ContactorName'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckOrder: [], PayeeInfo: [] },
            oForm = $('#form_main'),
            oGrid = null,
            oValidator = null,
            sOptionHtml_ExhibitionName = '',
            sOptionHtml_CoopAgent = '',
            sOptionHtml_GroupUnit = '',
            sOptionHtml_ComplaintSource = '',
			sOptionHtml_Customer = '',
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
                                oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                                setFormVal(oForm, oRes);
								
								$('#ExhibitionName').html(sOptionHtml_ExhibitionName).val(oRes.ExhibitionName).select2();
								$('#ComplaintSource').html(sOptionHtml_ComplaintSource).val(oRes.ComplaintSource).select2();
								$('#CoopAgent').html(sOptionHtml_CoopAgent).val(oRes.CoopAgent).select2();
								$('#GroupUnit').html(sOptionHtml_GroupUnit).val(oRes.GroupUnit).select2();
								$('#CustomerCName').html(sOptionHtml_Customer).val(oRes.CustomerCName).select2();
                                $('#Applicant').text(oCurData.CreateUserName + '(' + oCurData.CreateUser + ')  ' + oCurData.DepartmentName);
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
                                    $("#Handle_DeptID,#Handle_Person").attr('disabled', false);
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
					if(sCustomerId != "" && sCustomerId != null){
						$('#CustomerCName').html(sOptionHtml_Customer).val(sCustomerId).select2();
					} else {
						$('#CustomerCName').html(sOptionHtml_Customer).select2();
					}
					
					if(sContactorName != "" & sContactorName != null){
						
						$('#Complainant').val(decodeURI(atob(sContactorName)));
					}
					$('#ExhibitionName').html(sOptionHtml_ExhibitionName).select2();
					$('#ComplaintSource').html(sOptionHtml_ComplaintSource).select2();
					$('#CoopAgent').html(sOptionHtml_CoopAgent).select2();
					$('#GroupUnit').html(sOptionHtml_GroupUnit).select2();
                    //$('#Applicant').text(oCurData.CreateUserName + '(' + oCurData.CreateUser + ')  ' + oCurData.DepartmentName);
                    oCurData.PayeeInfo = [];
                    oCurData.CheckOrder = [];
                    oCurData.Guid = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param   sFlag{String} 新增或儲存後新增
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Guid = oCurData.Guid;
                data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|IAP|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers)
                data.PayeeInfo = JSON.stringify(oCurData.PayeeInfo);
                data.RemittanceInformation = JSON.stringify(data.RemittanceInformation);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.DataType = 'A';
                data.IsHandled = 'N';
                data.PayeeType = 'P';
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.PayeeName = $('#Payee option:selected').text();
                data.CustomerId = data.CustomerCName;
                delete data.CustomerCName;
                data.ExhibitionNO = data.ExhibitionName;
                g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: parent.UserInfo.OrgID === 'TE' ? '' : parent.UserInfo.OrgID,
                    Flag: 'MinYear',
                    Len: 3,
                    Str: '',
                    AddType: '',
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        data.ComplaintNumber = res.DATA.rel;
                        CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                            Params: {
                                complaint: data
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
                    } else {
                        showMsg(i18next.t('message.CreateBill_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.CreateBill_Failed'), 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
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
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.ExhibitionNO = data.ExhibitionName;
                data.CustomerId = data.CustomerCName;
                delete data.ExhibitionName;
                delete data.CustomerCName;
                return CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        complaint: {
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
                        complaint: {
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
             * 上傳附件
             * @param {Array} files 上傳的文件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'InvoiceApplyForPersonal';
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
                g_api.ConnectLite(sProgramId, 'ComplaintToAudit', {
                    Guid: oCurData.Guid
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
                        $(dom).parent().next().find('select').val(oRes.PrjCode).trigger("change");
                    }
                });
            },
            /**
             * 獲取客訴來源
             */
            setComplaintSourceDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    OrgID: 'TE',
                    ArgClassID: 'CRMComplaintSource'
                }, function (res) {
                    if (res.RESULT) {
                        let QueryData = res.DATA.rel;
                        if (QueryData.length > 0) {
							sOptionHtml_ComplaintSource = createOptions(QueryData, 'id', 'text', true);
                            //$('#ComplaintSource').append(createOptions(QueryData, 'id', 'text', true)).select2();
                        }
                    }
                });
            },
            /**
             * 獲取組團單位
             */
            fnSetGroupUnit = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetGroupUnit', {}, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        sOptionHtml_GroupUnit = createOptions(saList, 'guid', 'CustomerShotCName');
                        //$('#GroupUnit').html(sOptions).select2();
                    }
                });
            },
            /**
             * 獲取配合代理
             */
            fnSetCoopAgent = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetCoopAgent', {}, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        sOptionHtml_CoopAgent = createOptions(saList, 'guid', 'CustomerShotCName');
                        //$('#CoopAgent').html(sOptions).select2();
                    }
                });
            },
            /**
             * 獲取客戶名稱
             */
            fnSetCustomers = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetCustomers', {}, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        sOptionHtml_Customer = createOptions(saList, 'guid', 'CustomerCName');
                        //$('#CustomerCName').html(sOptions).select2();
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
                    setComplaintSourceDrop(),
                    fnSetCoopAgent(),
                    fnSetGroupUnit(),
                    fnSetCustomers(),
					fnSetEpoDrop({
                        CallBack: function (data) {
                            sOptionHtml_ExhibitionName = createOptions(data, 'SN', 'ExhibitioShotName_TW');
                            
                        }
                    }),
                    fnSetFlowDrop({
                        Flow_Type: '008',
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
                                            /*Flag*/
                                            $("#Handle_DeptID").val(parent.UserInfo.DepartmentID); 
                                            $("#Handle_Person").val(parent.UserInfo.MemberID).trigger('change');
                                            if (oRes.Flows_Lock === 'Y') {
                                                $(".checkordertoolbox").hide();
                                            }
                                            else {
                                                $(".checkordertoolbox").show();
                                            }
                                            if (oRes.Handle_Lock === 'Y') {
                                                $("#Handle_DeptID,#Handle_Person").attr('disabled', false);
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
                    
                    fnSetUserDrop([
                        {
                            Select: $('#Handle_Person,#Payee'),
                            Select2: true,
                            ShowId: true,
                            Action: sAction,
                            CallBack: function (data) {
                                saUsers = data;
                                if (sAction === 'Add') {
                                    $('#Payee').val(parent.UserID);
                                }
                            }
                        }
                    ])
					]).done(function () {
                        fnGet();
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