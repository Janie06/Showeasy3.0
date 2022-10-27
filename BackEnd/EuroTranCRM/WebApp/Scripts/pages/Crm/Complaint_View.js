'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = 'Complaint_Qry',
    sEditPrgId = 'Complaint_Upd',
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckFlows: [], HandleFlows: [], PayeeInfo: [] },
            oForm = $('#form_main'),
            saCheckOrder_Push = [],
            saUsers = [],
            sCurAuditFlowId = null,
            sCurHandleFlowId = null,
			sComplaintType = '',
			saExhibitionName = [],
			saComplaintSource = [],
			saCoopAgent = [],
			saGroupUnit = [],
			saCustomers = [],
            bGoNext = true,
            /**
             * 獲取當前審核人員
             * @param {Object} data 當前審核資料
             */
            fnGetCurAuditor = function (data) {
                var saCheckFlows = $.parseJSON(data.CheckFlows),
                    saNewList = Enumerable.From(saCheckFlows).GroupBy("$.Order").ToArray(),
                    bAuditor = false,
                    bHandle = false,
                    bAllAudit = false;
                if ('B,E'.indexOf(data.DataType) > -1) {//只有審核中或者待經辦的資料才可以出現審核區塊
                    $.each(saNewList, function (idx, _data) {
                        var sSignedWay = _data.source[0].SignedWay,
                            iCount = Enumerable.From(_data.source).Where(function (e) { return (e.SignedDecision === 'Y' || e.SignedDecision === 'O'); }).Count(),
                            sCurAuditor = Enumerable.From(_data.source).ToString("，", "$.SignedId");
                        if ((('flow1,flow3'.indexOf(sSignedWay) > -1 && iCount === 0) || (sSignedWay === 'flow2' && iCount !== _data.source.length))) {
                            if (sCurAuditor.indexOf(parent.UserID) > -1) {
                                var oCurUser = Enumerable.From(_data.source).Where(function (e) { return e.SignedId === parent.UserID; }).First();
                                if (sSignedWay === 'flow2' && iCount < _data.source.length - 1) {
                                    bGoNext = false;
                                }
                                if (!oCurUser.SignedDecision) {
                                    sCurAuditFlowId = oCurUser.FlowId;
                                    bAuditor = true;
                                }
                            }
                            if (!bAllAudit) {//只要有需要審核的人,就不要檢核是否簽辦
                                bAllAudit = true;
                            }
                            return false;
                        }
                    });
                    if (!bAuditor && !bAllAudit) {//當沒有要審核的人員並且審核全部完成時才會檢核簽辦
                        var saHandleFlows = $.parseJSON(data.HandleFlows),
                            sCurAuditor = saHandleFlows[0].SignedId;
                        if (saHandleFlows[0].SignedDecision !== 'Y' && sCurAuditor === parent.UserID) {
                            sCurHandleFlowId = saHandleFlows[0].FlowId;
                            bHandle = true;
                        }
                    }
                }
                return {
                    IsAuditor: bAuditor,
                    IsHandler: bHandle
                };
            },
            /**
             * 註冊加簽事件
             * @param {Number} _grididx 當前Gird 序號
             */
            fnRegisterAddFlows = function (_grididx) {
                $('.flowlink' + _grididx).on('click', function () {
                    var oOption = {};
                    oOption.SignedWay = $(this).data('id');
                    oOption.Callback = function (data) {
                        if (data.Users.length > 0) {
                            var oFlow = {};
                            if (data.FlowType === 'flow1') {
                                $.each(data.Users, function (idx, user) {
                                    oFlow = {};
                                    oFlow.id = guid();
                                    oFlow.Order = saCheckOrder_Push.length + 1;
                                    oFlow.SignedWay = data.FlowType;
                                    oFlow.SignedMember = [{
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    }];
                                    saCheckOrder_Push.push(oFlow);
                                });
                            }
                            else {
                                var saSignedMembers = [];
                                $.each(data.Users, function (idx, user) {
                                    saSignedMembers.push({
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    });
                                });
                                oFlow.id = guid();
                                oFlow.Order = saCheckOrder_Push.length + 1;
                                oFlow.SignedWay = data.FlowType;
                                oFlow.SignedMember = saSignedMembers;
                                saCheckOrder_Push.push(oFlow);
                            }
                            saCheckOrder_Push = releaseGridList(saCheckOrder_Push);
                            $("#jsGrid" + _grididx).jsGrid("loadData");
                        }
                    };
                    oPenUserListPop(oOption);
                });
            },
            /**
             * 獲取資料
             */
            fnGet = function () {
                return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                    {
                        Guid: sDataId
                    },
                    function (res) {
                        if (res.RESULT) {
                            var oRes = res.DATA.rel,
                                saFlowsText = [],
                                sDataType = '';

                            if (oRes.DataType === 'C-O') {
                                sDataType = '(' + i18next.t("common.HasReEdited") + ')';// ╠common.HasReEdited⇒已抽單╣
                            }
                            else if (oRes.DataType === 'D-O') {
                                sDataType = '(' + i18next.t("common.HasReturned") + ')';// ╠common.HasReturned⇒已退件╣
                            }
                            else if (oRes.DataType === 'X') {
                                sDataType = '(' + i18next.t("common.HasVoid") + ')';// ╠common.HasVoid⇒已作廢╣
                            }

                            //根據帳單狀態，移除抽單按鈕(扣除已經辦只能admin抽單)
                            fnCheckReEdit(oRes.DataType, oRes.CreateUser);

                            if (oRes.CreateUser !== parent.UserID) {
                                $('#Toolbar_Copy').remove();
                            }
                            if ('B,Y'.indexOf(oRes.DataType) === -1) {
                                $('#Toolbar_Void').remove();
                            }

                            var SuperAuditor = parent.SysSet.InvoiceApplySuperAuditor.indexOf(parent.UserID) > -1;
                            var oCheck = fnGetCurAuditor(oRes);
                            if (oCheck.IsAuditor) {
                                $('#IsAuditor').show();
                            }
                            if (oCheck.IsHandler) {
                                $('#IsHandler').show();
                            }
                            if (oCheck.IsAuditor || oCheck.IsHandler) {
                                $('.submitdecision').click(function () {
                                    var sSubmitAction = $(this).data('id');
                                    fnSubmitDecision(sSubmitAction);
                                });
                            }

                            //條件:經辦中、SuperAuditor、未經辦(最後一關)
                            /* if ("B".indexOf(oRes.DataType) > -1 && SuperAuditor && !oCheck.IsHandler) {
                                if (!oCheck.IsAuditor) {
                                    $('#SuperAuditor').show();
                                    $('.submitdecisionforall').click(function () {
                                        fnSubmitDecisionForAll();
                                    });
                                }
                            } */


                            if (oRes.VoidReason) {
                                $('#VoidReason').text(oRes.VoidReason);
                                $('.VoidReason').show();
                            }

                            if (oRes.CrosssignTurn === 'N') {
                                $('.crosssignturn').remove();
                            }

                            oCurData = oRes;
                            console.log("data:", oCurData);
                            oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                            oCurData.CheckFlows = $.parseJSON(oCurData.CheckFlows);
                            oCurData.HandleFlows = $.parseJSON(oCurData.HandleFlows);
                            setFormVal(oForm, oRes);
                            $('#Applicant').text(oCurData.CreateUserName + '(' + oCurData.CreateUser + ')  ' + oCurData.DepartmentName);
                            $('.complaintTitle').text(oCurData.ComplaintTitle);
							
							switch(oCurData.ComplaintType){
								case "1":
									sComplaintType = "貨損";
									break;
								case "2":
									sComplaintType = "延誤";
									break;
								case "3":
									sComplaintType = "遺失";
									break;
								case "4":
									sComplaintType = "抱怨";
									break;
							}
							$('#ComplaintType').text(sComplaintType);
							
							$('#ComplaintNumber').text(oCurData.ComplaintNumber);
							saExhibitionName = Enumerable.From(saExhibitionName).Where(function (e) { return e.SN == oCurData.ExhibitionName; }).ToArray();
							$('#ExhibitionName').text(saExhibitionName[0].ExhibitioShotName_TW);
							saComplaintSource = Enumerable.From(saComplaintSource).Where(function (e) { return e.id == oCurData.ComplaintSource; }).ToArray();
							$('#ComplaintSource').text(saComplaintSource[0].text);
							if(oCurData.CoopAgent != "" && oCurData.CoopAgent != null){
								saCoopAgent = Enumerable.From(saCoopAgent).Where(function (e) { return e.guid == oCurData.CoopAgent; }).ToArray();
								$('#CoopAgent').text(saCoopAgent[0].CustomerShotCName);
							}
							if(oCurData.GroupUnit != "" && oCurData.GroupUnit != null){
								saGroupUnit = Enumerable.From(saGroupUnit).Where(function (e) { return e.guid == oCurData.GroupUnit; }).ToArray();
								$('#GroupUnit').text(saGroupUnit[0].CustomerShotCName);
							}
							
							saCustomers = Enumerable.From(saCustomers).Where(function (e) { return e.guid == oCurData.CustomerCName; }).ToArray();
							if(saCustomers.length > 0){
								$('#CustomerCName').text(saCustomers[0].CustomerCName);
							} else {
								$('#CustomerCName').text("");
							}
							
							$('#Department').text(oCurData.Department);
							$('#Complainant').text(oCurData.Complainant);
							$('#Jobtitle').text(oCurData.Jobtitle);
							$('#Telephone1').text(oCurData.Telephone1);
							$('#Telephone2').text(oCurData.Telephone2);
							$('#Email1').text(oCurData.Email1);
							$('#Email2').text(oCurData.Email2);
							
                            $('#DataType').text(sDataType);
                            $('#Payee').text(oCurData.PayeeName);
                            $('#Handle_Person').text(oCurData.Handle_PersonName);// ╠common.Important_1⇒普通╣  ╠common.Important_2⇒重要╣  ╠common.Important_3⇒很重要╣
                            $('#Important').text(oCurData.Important === '1' ? i18next.t("common.Important_1") : oCurData.Important === '2' ? i18next.t("common.Important_2") : i18next.t("common.Important_3"));
                            $('#SignedNumber').text(oCurData.SignedNumber);
                            $('#PaymentTime').val(newDate(oCurData.PaymentTime, true));

                            $('[name="PaymentWay"]').each(function () {
                                var radio = $(this).parents('.radio');
                                if (!this.checked) {
                                    radio.next().remove();
                                }
                                else {
                                    radio.next().addClass('show-text');
                                }
                                radio.remove();
                            });

                            $.each(oCurData.CheckOrder, function (idx, order) {
                                var sFlowType = i18next.t('common.' + order.SignedWay);
                                if (order.SignedWay !== 'flow1') {
                                    saFlowsText.push(sFlowType + '(' + Enumerable.From(order.SignedMember).ToString("，", "$.name") + ')');
                                }
                                else {
                                    saFlowsText.push(Enumerable.From(order.SignedMember).ToString("，", "$.name"));
                                }
                            });
                            $('#Recipient').text(saFlowsText.join('->'));

                            fnGetFiles($('#AdditionalFiles'), oCurData.Guid, oCurData.RelationId, oCurData.ExFeild1, sProgramId);//加載附件
                            fnUpload();//初始化上傳控件（審批與簽辦）
                            fnRead();//如果是被通知的人，則修改為已閱讀狀態
                            $("#jsGrid").jsGrid("loadData");
                            $("#jsGrid1").jsGrid("loadData");
                            $("#jsGrid2").jsGrid("loadData");

                            $('[name="SignedDecision"]').click(function () {
                                if (this.value === 'O') {
                                    $('#addotheraudit').slideDown();
                                }
                                else {
                                    $('#addotheraudit').slideUp();
                                }
                            });

                            $('[name="HandleDecision"]').click(function () {
                                if (this.value === 'O') {
                                    $('#addotherhandle').slideDown();
                                }
                                else {
                                    $('#addotherhandle').slideUp();
                                }
                            });
                        }
                    });
            },
            /**
             * 複製
             */
            fnCopy = function () {
                var data = oCurData;
                data = packParams(data);
                if (data.DataType === 'D-O') {
                    data.RelationId = data.Guid;
                }
                else {
                    delete data.RelationId;
                }
                //data.OrgID = parent.OrgID;
                data.Guid = guid();
                //data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|IAP|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(data, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(data, saUsers);
                data.CheckOrder = JSON.stringify(data.CheckOrder);
                data.PayeeInfo = JSON.stringify(data.PayeeInfo);
                //data.RemittanceInformation = JSON.stringify(data.RemittanceInformation);
                data.DataType = 'A';
                data.IsHandled = 'N';
                //data.PayeeType = 'P';
                data.Inspectors = '';
                data.Reminders = '';
                data.VoidReason = '';
                data.Flows_Lock = data.Flows_Lock || 'N';
                data.Handle_Lock = data.Handle_Lock || 'N';
				data.ExhibitionNO = data.ExhibitionName;
				data.CustomerId = data.CustomerCName;

                delete data.RowIndex;
                delete data.ApplicantName;
                delete data.Handle_PersonName;
                delete data.DeptName;
                delete data.PaymentTime;
                delete data.PaymentType;
                delete data.CheckNumber;
                delete data.PayeeCode;
                delete data.CustomerNO;
                delete data.CreateUserName;
                delete data.ModifyUserName;
                if (!data.EffectTime) {
                    delete data.EffectTime;
                }
				delete data.PayeeInfo;
				delete data.ExhibitionName;
				delete data.CustomerCName;
				delete data.ExhibitioShotName_TW;
				

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        complaint: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsgAndGo(i18next.t("message.Copy_Success"), sEditPrgId, '?Action=Upd&Guid=' + data.Guid); // ╠message.Copy_Success⇒複製成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Copy_Failed"), 'error'); // ╠message.Copy_Failed⇒複製失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Copy_Failed"), 'error'); // ╠message.Copy_Failed⇒複製失敗╣
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
                            DataType: 'X',
                            VoidReason: $('#VoidContent').val()
                        };
                        if (!$('#VoidContent').val()) {
                            showMsg(i18next.t("message.VoidReason_Required")); // ╠message.VoidReason_Required⇒請填寫作廢原因╣
                            return false;
                        }
                        CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                            Params: {
                                complaint: {
                                    values: data,
                                    keys: { Guid: sDataId }
                                }
                            }
                        }, function (res) {
                            if (res.d > 0) {
                                DelTask(sDataId);
                                showMsgAndGo(i18next.t('message.Void_Success'), sProgramId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.Void_Success⇒作廢成功╣
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
             * 讀取
             */
            fnRead = function () {
                var bToUpd = false;

                $.each(oCurData.CheckFlows, function (idx, _data) {
                    if (_data.SignedId === parent.UserID && _data.SignedWay === 'flow4' && _data.SignedDecision === 'T') {
                        _data.SignedDecision = 'R';
                        bToUpd = true;
                        return false;
                    }
                });

                if (bToUpd) {
                    CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                        Params: {
                            invoiceapplyinfo: {
                                values: { CheckFlows: JSON.stringify(oCurData.CheckFlows) },
                                keys: { Guid: sDataId }
                            }
                        }
                    });
                }
            },
            /**
             * 抽單
             */
            fnReEdit = function () {
                var data = {};
                data.DataType = 'C-O';
                data = packParams(data, 'upd');

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        complaint: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.ReEdit_Success"), 'success'); // ╠message.ReEdit_Success⇒抽單成功╣
                        CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                            Params: {
                                task: {
                                    values: { DataType: 'O' },
                                    keys: { SourceID: sDataId }
                                }
                            }
                        });
                        fnGet();
                    }
                    else {
                        showMsg(i18next.t('message.ReEdit_Failed'), 'error'); // ╠message.ReEdit_Failed⇒抽單失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.ReEdit_Failed'), 'error'); // ╠message.ReEdit_Failed⇒抽單失敗╣
                });
            },
            /*
             * 提交決定謙和所有人(該使用者之前)
             */
            fnSubmitDecisionForAll = function () {
                var data = getFormSerialize(oForm);

                g_api.ConnectLite(sEditPrgId, 'ComplaintAuditAll', {
                    Guid: oCurData.Guid,
                    AutoSignedDecision: data.AutoSignedDecision,
                    AutoSignedOpinion: data.AutoSignedOpinion
                }, function (res) {
                    var action = 'SubmitDecisionForAll'
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t('message.' + action + '_Success'), sProgramId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.Void_Success⇒作廢成功╣
                        parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                    }
                    else {
                        showMsg(i18next.t('message.' + action + '_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Signed_Failed⇒簽核失敗╣ ╠message.Handle_Failed⇒簽辦失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.' + action + '_Failed'), 'error'); // ╠message.Signed_Failed⇒簽核失敗╣ ╠message.Handle_Failed⇒簽辦失敗╣
                });
            },

            /**
             * 提交決定
             * @param {String} action 簽核 or 經辦
             */
            fnSubmitDecision = function (action) {
                var data = getFormSerialize(oForm),
                    saNewCheckFlows = clone(oCurData.CheckFlows),
                    saNewHandleFlows = clone(oCurData.HandleFlows),
                    saNewCheckOrderPush = clone(saCheckOrder_Push),
                    saAdds = [],//加簽核順序
                    saNextUsers = [],//下組簽核人員
                    saTipsUsers = [],//需要通知的人員
                    saNextSignedWays = [],//下組需要處理的動作
                    iCurOder = 0,//當前順序
                    sHandlePerson = "";//經辦人

                if (action === 'Signed') {
                    if (data.SignedDecision === 'N' && !data.SignedOpinion) {
                        showMsg(i18next.t("message.SignedOpinion_required")); // ╠message.SignedOpinion_required⇒請填寫簽核意見╣
                        return false;
                    }
                    else if (data.SignedDecision === 'O' && saCheckOrder_Push.length === 0) {
                        showMsg(i18next.t("message.CheckOrderPush_required")); // ╠message.CheckOrderPush_required⇒請選擇加簽人員╣
                        return false;
                    }

                    var oCurFlow = Enumerable.From(oCurData.CheckFlows).Where(function (e) { return e.FlowId == sCurAuditFlowId; }).First(),
                        oNewCurFlow = clone(oCurFlow);
                    iCurOder = oCurFlow.Order;
                    $.each(saNewCheckOrderPush, function (idx, _push) {
                        _push.Order = idx + 1 + iCurOder;
                    });

                    $.each(saNewCheckFlows, function (idx, _flow) {
                        if (sCurAuditFlowId === _flow.FlowId) {
                            _flow.SignedDecision = data.SignedDecision;
                            _flow.SignedOpinion = data.SignedOpinion;
                            _flow.SignedDate = newDate();
                            if (data.SignedDecision === 'O') {
                                _flow.SignedPush = saCheckOrder_Push;
                            }
                        }
                        if (data.SignedDecision === 'O' && _flow.Order > iCurOder) {
                            _flow.Order = _flow.Order + saNewCheckOrderPush.length;
                            if (data.NoreTurn) {//如果再回傳給自己的話排序再增加一個值
                                _flow.Order++;
                            }
                            else {//如果加簽核流程最後一個是通知，並且原流程下一個也是通知的話就合併新舊排序
                                if (saNewCheckFlows[saNewCheckOrderPush.length - 1].SignedWay === 'flow4' && saNewCheckFlows[iCurOder + 1].SignedWay === saNewCheckFlows[saNewCheckOrderPush.length - 1].SignedWay) {
                                    _flow.Order--;
                                }
                            }
                        }
                    });
                    saAdds = fnCheckFlows({ CheckOrder: saNewCheckOrderPush }, false, false, saUsers);
                    if (data.SignedDecision === 'O' && data.NoreTurn) {
                        oNewCurFlow.Order = oNewCurFlow.Order + saNewCheckOrderPush.length + 1;
                        oNewCurFlow.FlowId = guid();
                        saAdds.push(oNewCurFlow);
                    }
                }
                else {
                    if (data.HandleDecision === 'O' && saCheckOrder_Push.length === 0) {
                        showMsg(i18next.t("message.CheckOrderPush_required")); // ╠message.CheckOrderPush_required⇒請選擇加簽人員╣
                        return false;
                    }
                    iCurOder = saNewCheckFlows[saNewCheckFlows.length - 1].Order;//如果是签辦的話當前順序就是原本流程的最大順序
                    $.each(saNewHandleFlows, function (idx, _flow) {
                        if (sCurHandleFlowId === _flow.FlowId) {
                            _flow.SignedDecision = data.HandleDecision;
                            _flow.SignedOpinion = data.HnadleOpinion;
                            _flow.SignedDate = newDate();
                            if (data.HandleDecision === 'O') {
                                _flow.SignedPush = saCheckOrder_Push;
                            }
                            if (data.HandleDecision === 'Y') {
                                _flow.SignedPush = null;
                            }
                        }
                    });
                    if (data.HandleDecision === 'O') {
                        $.each(saNewCheckOrderPush, function (idx, _push) {
                            _push.Order = idx + saNewCheckFlows[saNewCheckFlows.length - 1].Order + 1;
                        });
                        saAdds = fnCheckFlows({ CheckOrder: saNewCheckOrderPush }, false, false, saUsers);
                    }
                }
                saNewCheckFlows = saNewCheckFlows.concat(saAdds);
                saNewCheckFlows = Enumerable.From(saNewCheckFlows).OrderBy("$.Order").ToArray();

                var _List = Enumerable.From(saNewCheckFlows).GroupBy("$.Order").ToArray();
                saNewCheckFlows = [];
                $.each(_List, function (idx, _list) {
                    //找到要通知的人
                    var sSignedWay = _list.source[0].SignedWay;
                    var LastOne = iCurOder >= _List.length
                    if (idx === iCurOder && !LastOne) {
                        switch (sSignedWay) {
                            case "flow1":
                            case "flow2":
                            case "flow3":
                                {
                                    saNextUsers = Enumerable.From(_list.source).Select("$.SignedId").ToArray();
                                    saNextSignedWays.push(sSignedWay);
                                }
                                break;
                            case "flow4":
                                {   //不能算通知到
                                    $.each(_list.source, function (i, _source) {
                                        _source.SignedDecision = 'T';
                                        _source.SignedDate = newDate();
                                    });
                                    saTipsUsers = Enumerable.From(_list.source).Select("$.SignedId").ToArray();
                                    saNextSignedWays.push(sSignedWay);
                                    ++iCurOder
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    saNewCheckFlows = saNewCheckFlows.concat(_list.source);
                });
                //Flow => Handle => END
                if (action != 'Handle') {
                    var CheckNextUser = saNextUsers.length === 0;
                    if (CheckNextUser && iCurOder === _List.length) {
                        sHandlePerson = saNewHandleFlows[0].SignedId;
                        saNextSignedWays.push("flow5");
                    }
                }
				
                g_api.ConnectLite(sEditPrgId, 'ComplaintAudit', {
                    Guid: oCurData.Guid,
                    Action: action,
                    GoNext: bGoNext ? 'Y' : 'N',
                    HandlePerson: sHandlePerson,
                    NextSignedWays: JSON.stringify(saNextSignedWays),
                    NextUsers: JSON.stringify(saNextUsers),
                    TipsUsers: JSON.stringify(saTipsUsers),
                    CheckFlows: JSON.stringify(saNewCheckFlows),
                    HandleFlows: JSON.stringify(saNewHandleFlows),
                    SignedDecision: data.SignedDecision,
                    HandleDecision: data.HandleDecision
                }, function (res) {
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t('message.' + action + '_Success'), sProgramId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.Void_Success⇒作廢成功╣
                        parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
                    }
                    else {
                        showMsg(i18next.t('message.' + action + '_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Signed_Failed⇒簽核失敗╣ ╠message.Handle_Failed⇒簽辦失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.' + action + '_Failed'), 'error'); // ╠message.Signed_Failed⇒簽核失敗╣ ╠message.Handle_Failed⇒簽辦失敗╣
                });
            },
            /**
             * 上傳附件
             * @param {Array} files 上傳的文件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput1');
                option.theme = 'dragdropbox';
                option.folder = 'Complaint';
                option.type = 'list';
                option.parentid = sCurAuditFlowId;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
                option.input = $('#fileInput2');
                option.parentid = sCurHandleFlowId;
                fnUploadRegister(option);
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
                        saComplaintSource = res.DATA.rel;
                        /* if (QueryData.length > 0) {
                            $('#ComplaintSource').append(createOptions(QueryData, 'id', 'text', true));
                        } */
                    }
                });
            },
            /**
             * 獲取組團單位
             */
            fnSetGroupUnit = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetGroupUnit', {}, function (res) {
                    if (res.RESULT) {
						saGroupUnit = res.DATA.rel;
                       /*  var saList = res.DATA.rel;
                        var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                        $('#GroupUnit').html(sOptions); */
                    }
                });
            },
            /**
             * 獲取配合代理
             */
            fnSetCoopAgent = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetCoopAgent', {}, function (res) {
                    if (res.RESULT) {
						saCoopAgent = res.DATA.rel;
                        /* var saList = res.DATA.rel;
                        var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                        $('#CoopAgent').html(sOptions); */
                    }
                });
            },
            /**
             * 獲取客戶名稱
             */
            fnSetCustomers = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetCustomers', {}, function (res) {
                    if (res.RESULT) {
						saCustomers = res.DATA.rel;
                        /* var saList = res.DATA.rel;
                        var sOptions = createOptions(saList, 'guid', 'CustomerCName');
                        $('#CustomerCName').html(sOptions); */
                    }
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object} inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        break;
                    case "Toolbar_ReAdd":

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

                        fnRefreshFlowsThenCopy(oCurData,fnCopy);

                        break;
                    case "Toolbar_Void":

                        fnVoid();

                        break;
                    case "Toolbar_ReEdit":

                        // ╠message.CheckReEdit⇒確定要抽單嗎？╣ ╠common.Tips⇒提示╣
                        layer.confirm(i18next.t('message.CheckReEdit'), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnReEdit();
                            layer.close(index);
                        });
                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_Print":

                        fnPrePrint($(".panel-info"));

                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /*
             * 初始化 function
             */
            init = function () {
                var saCusBtns = null;

                if (sAction === 'Upd') {
                    saCusBtns = [
                        {
                            id: 'Toolbar_ReEdit',
                            value: 'common.ReEdit'// ╠common.ReEdit⇒抽單╣
                        },
                        {
                            id: 'Toolbar_Copy',
                            value: 'common.Toolbar_Copy'// ╠common.Toolbar_Copy⇒複製╣
                        }];
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true
                });

                $.whenArray([
                    fnSetUserDrop([
                        {
                            CallBack: function (data) {
                                saUsers = data;
                            }
                        }
                    ]),
                    setComplaintSourceDrop(),
                    fnSetCoopAgent(),
                    fnSetGroupUnit(),
                    fnSetCustomers(),
                    fnSetEpoDrop({
                        CallBack: function (data) {
							saExhibitionName = data;
                            /* var sOptionHtml_PrjCode = createOptions(data, 'ExhibitionCode', 'ExhibitioShotName_TW');
                            $('#ExhibitionName').html(sOptionHtml_PrjCode); */
                        }
                    }),
				]).done(function () {
					fnGet();
				});

                $("#jsGrid1").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    pageLoading: true,
                    pageIndex: 1,
                    pageSize: 10000,
                    rowClass: function (item, itemIndex) {
                        var sRowClass = '';
                        if (oCurData.CheckFlows.length !== itemIndex + 1) {
                            sRowClass = item.Line ? 'grid-cuscell first-cell' : 'grid-cuscell';
                        }
                        else {
                            sRowClass = 'last-cell';
                        }
                        return sRowClass;
                    },
                    fields: [
                        {
                            type: "Icon", width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                var oIcon = {
                                    flow2: '<img src="../../images/flow2_View.gif">',
                                    flow3: '<img src="../../images/flow3_View.gif">',
                                    flow4: '<img src="../../images/flow4.gif">'
                                };
                                return item.Icon ? oIcon[item.SignedWay] || '' : '';
                            }
                        },
                        {
                            name: "Order", title: 'common.Order', width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                return val < 10 ? '0' + val : val;
                            }
                        },
                        {
                            name: "SignedMember", title: 'common.SignedMember', width: 150,
                            itemTemplate: function (val, item) {
                                return $('<a>', { html: item.Department + ' ' + item.Jobtitle + '<br>' + item.SignedMember + (item.ParentId == '0' ? '(' + i18next.t("common.AgentPerson") + ')' : '') });// ╠common.CheckReEdit⇒代理人╣
                            }
                        },
                        {
                            name: "SignedDecision", title: 'common.Decision', width: 100,
                            itemTemplate: function (val, item) {
                                var sVal = val;
                                if (val === 'Y') {
                                    sVal = i18next.t("common.Agree");// ╠common.Agree⇒同意╣
                                }
                                else if (val === 'N') {
                                    sVal = i18next.t("common.NotAgree");// ╠common.NotAgree⇒不同意╣
                                }
                                else if (val === 'O') {
                                    sVal = i18next.t("common.AddOther");// ╠common.AddOther⇒先加簽╣
                                }
                                else if (val === 'T') {
                                    sVal = i18next.t("common.HasNotice");// ╠common.HasNotice⇒已通知╣
                                }
                                else if (val === 'R') {
                                    sVal = i18next.t("common.HasRead");// ╠common.HasRead⇒已閱讀╣
                                }
                                return $('<a>', { html: sVal });
                            }
                        },
                        {
                            name: "SignedOpinion", title: 'common.SignedOpinion', width: 450,
                            itemTemplate: function (val, item) {
                                var saVal = [],
                                    oDiv = $('<div>');

                                if (item.SignedPush) {
                                    var saFlowsPush = item.SignedPush,
                                        saFlowsPushText = [],
                                        sFlowsPushText = '';
                                    $.each(saFlowsPush, function (idx, flow) {
                                        var sFlowType = i18next.t('common.' + flow.SignedWay);
                                        if (flow.SignedWay !== 'flow1') {
                                            saFlowsPushText.push(sFlowType + '(' + Enumerable.From(flow.SignedMember).ToString("，", "$.name") + ')');
                                        }
                                        else {
                                            saFlowsPushText.push(Enumerable.From(flow.SignedMember).ToString("，", "$.name"));
                                        }
                                    });
                                    sFlowsPushText = saFlowsPushText.join(' → ');
                                    saVal.push($('<a>', { html: sFlowsPushText }));
                                    saVal.push('<br />');
                                }
                                if (val) {
                                    saVal.push(val);
                                    saVal.push('<br />');
                                }
                                fnGetFiles(oDiv.append(saVal), item.FlowId);
                                return oDiv;
                            }
                        },
                        {
                            name: "SignedDate", title: 'common.SignedDate', width: 120,
                            itemTemplate: function (val, item) {
                                return val;
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: oCurData.CheckFlows,
                                itemsCount: oCurData.CheckFlows.length //data.length
                            };
                        }
                    }
                });

                $("#jsGrid2").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    pageLoading: true,
                    pageIndex: 1,
                    pageSize: 10000,
                    rowClass: function (item, itemIndex) {
                        var sRowClass = '';
                        if (oCurData.HandleFlows.length !== itemIndex + 1) {
                            sRowClass = item.Line ? 'grid-cuscell first-cell' : 'grid-cuscell';
                        }
                        else {
                            sRowClass = 'last-cell';
                        }
                        return sRowClass;
                    },
                    fields: [
                        {
                            name: "SignedMember", title: 'common.HandleMembers', width: 150,
                            itemTemplate: function (val, item) {
                                return $('<a>', { html: item.Department + ' ' + item.Jobtitle + '<br>' + item.SignedMember + (item.ParentId == '0' ? '(' + i18next.t("common.AgentPerson") + ')' : '') });// ╠common.CheckReEdit⇒代理人╣
                            }
                        },
                        {
                            name: "SignedDecision", title: 'common.Status', width: 100,
                            itemTemplate: function (val, item) {
                                var sVal = val;
                                if (val === 'Y') {
                                    sVal = i18next.t("common.Hashandle");// ╠common.Hashandle⇒已經辦╣
                                }
                                else if (val === 'N') {
                                    sVal = i18next.t("common.Nothandle");// ╠common.Nothandle⇒未處理╣
                                }
                                else if (val === 'O') {
                                    sVal = i18next.t("common.AddOtherAudit");// ╠common.AddOtherAudit⇒轉呈其它主管審批╣
                                }
                                return $('<a>', { html: sVal });
                            }
                        },
                        {
                            name: "SignedOpinion", title: 'common.SignedOpinion', width: 450,
                            itemTemplate: function (val, item) {
                                var saVal = [],
                                    oDiv = $('<div>');

                                if (item.SignedPush) {
                                    var saFlowsPush = item.SignedPush,
                                        saFlowsPushText = [],
                                        sFlowsPushText = '';
                                    $.each(saFlowsPush, function (idx, flow) {
                                        var sFlowType = i18next.t('common.' + flow.SignedWay);
                                        if (flow.SignedWay !== 'flow1') {
                                            saFlowsPushText.push(sFlowType + '(' + Enumerable.From(flow.SignedMember).ToString("，", "$.name") + ')');
                                        }
                                        else {
                                            saFlowsPushText.push(Enumerable.From(flow.SignedMember).ToString("，", "$.name"));
                                        }
                                    });
                                    sFlowsPushText = saFlowsPushText.join(' → ');
                                    saVal.push($('<a>', { html: sFlowsPushText }));
                                    saVal.push('<br />');
                                }
                                if (val) {
                                    saVal.push(val);
                                    saVal.push('<br />');
                                }
                                fnGetFiles(oDiv.append(saVal), item.FlowId);
                                return oDiv;
                            }
                        },
                        {
                            name: "SignedDate", title: 'common.HandleDate', width: 120,
                            itemTemplate: function (val, item) {
                                return val;
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: oCurData.HandleFlows,
                                itemsCount: oCurData.HandleFlows.length //data.length
                            };
                        }
                    }
                });

                $.each([3, 4], function (idx, _grididx) {
                    fnRegisterAddFlows(_grididx);
                    $("#jsGrid" + _grididx).jsGrid({
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
                                    if (item.Order !== saCheckOrder_Push.length) {
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
                                        class: 'glyphicon glyphicon-pencil',
                                        title: i18next.t('common.Edit'),// ╠common.Edit⇒編輯╣
                                        click: function () {
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
                                                            saCheckOrder_Push.insert(item.Order + idx, oFlow);
                                                        });
                                                    }
                                                    else {
                                                        var saSignedMembers = [];
                                                        $.each(data.Users, function (idx, user) {
                                                            saSignedMembers.push({
                                                                id: user.id,
                                                                name: user.name,
                                                                deptname: user.deptname,
                                                                jobname: user.jobname
                                                            });
                                                        });
                                                        oFlow.id = guid();
                                                        oFlow.Order = item.Order;
                                                        oFlow.SignedWay = data.FlowType;
                                                        oFlow.SignedMember = saSignedMembers;
                                                        saCheckOrder_Push.insert(item.Order, oFlow);
                                                    }
                                                    var iOrder = 1;
                                                    $.each(saCheckOrder_Push, function (idx, _data) {
                                                        if (item.id !== _data.id) {
                                                            _data.Order = iOrder;
                                                            iOrder++;
                                                        }
                                                    });
                                                    saCheckOrder_Push = Enumerable.From(saCheckOrder_Push).Where(function (e) { return e.id !== item.id; }).ToArray();
                                                    saCheckOrder_Push = releaseGridList(saCheckOrder_Push);
                                                    $("#jsGrid" + _grididx).jsGrid("loadData");
                                                }
                                            };
                                            oPenUserListPop(oOption);
                                        }
                                    })),
                                    $('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-trash',
                                        title: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                        click: function () {
                                            var saNewList = Enumerable.From(saCheckOrder_Push).Where(function (e) { return e.id !== item.id; }).ToArray();
                                            saCheckOrder_Push = saNewList;
                                            $.each(saCheckOrder_Push, function (idx, _data) {
                                                _data.Order = idx + 1;
                                            });
                                            saCheckOrder_Push = releaseGridList(saCheckOrder_Push);
                                            $("#jsGrid" + _grididx).jsGrid("loadData");
                                        }
                                    }))];

                                    if (saCheckOrder_Push.length !== item.Order) {
                                        oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                            class: 'glyphicon glyphicon-arrow-down',
                                            title: i18next.t('common.Down'),// ╠common.Down⇒下移╣
                                            click: function () {
                                                var sOrder = Enumerable.From(saCheckOrder_Push).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                    iOrder = sOrder * 1;
                                                $.each(saCheckOrder_Push, function (idx, _data) {
                                                    if (iOrder === _data.Order) {
                                                        _data.Order++;
                                                    }
                                                    else if ((iOrder + 1) === _data.Order) {
                                                        _data.Order--;
                                                    }
                                                });
                                                saCheckOrder_Push = releaseGridList(saCheckOrder_Push);
                                                $("#jsGrid" + _grididx).jsGrid("loadData");
                                            }
                                        })));
                                    }
                                    else {
                                        oBtns.push($('<div>', { class: 'fa-item col-sm-3' }));
                                    }

                                    if (1 !== item.Order) {
                                        oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                            class: 'glyphicon glyphicon-arrow-up',
                                            title: i18next.t('common.Up'),// ╠common.Up⇒上移╣
                                            click: function () {
                                                var sOrder = Enumerable.From(saCheckOrder_Push).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                    iOrder = sOrder * 1;
                                                $.each(saCheckOrder_Push, function (idx, _data) {
                                                    if (iOrder === _data.Order) {
                                                        _data.Order--;
                                                    }
                                                    else if ((iOrder - 1) === _data.Order) {
                                                        _data.Order++;
                                                    }
                                                });
                                                saCheckOrder_Push = releaseGridList(saCheckOrder_Push);
                                                $("#jsGrid" + _grididx).jsGrid("loadData");
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
                                    data: saCheckOrder_Push,
                                    itemsCount: saCheckOrder_Push.length //data.length
                                };
                            }
                        }
                    });
                });
            };

        init();
    };

require(['base', 'jsgrid', 'jqprint', 'filer', 'common_eip', 'util'], fnPageInit);