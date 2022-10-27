'use strict';
var EditableData = false;
var DefaultState = parent.top.OrgID === 'SG' ? 'CHN' : 'TWN';
var fnPageInit = function () {
    var sLang = g_ul.GetLang(),
        sAction = getUrlParam('Action') || 'Add',
        oAuditFlag = { 'N': 'P', 'Z': 'A', 'Q': 'A' },
		saDealExhibitions = {},
		saUnDealExhibitions = {},
		saContactorList = [],
		oGrid = null,
		oGrid1 = null,
		oGrid2 = null,
		oGrid3 = null,
		oGrid4 = null,
		oGrid5 = null,
        canDo = new CanDo({
            /**
             * 是否返回查詢頁面
             */
            goBack: false,
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['OrgID', 'guid'],
            /**
             * 當前程式所有參數名稱集合
             */
            paramKeys: ['guid', 'From', 'FromId', 'Flag'],
            /**
             * 頁簽回調函數
             */
            tabAction: function (el, pargs) {
                switch (el.id) {
                    case 'litab2':
                        if (!$(el).data('action')) {
                            oGrid.loadData();
							oGrid1.loadData();
                            $(el).data('action', true);
                        }
                        break;
					case 'litab3':
                        if (!$(el).data('action')) {
                            oGrid2.loadData();
                            $(el).data('action', true);
                        }
                        break;
					case 'litab4':
                        if (!$(el).data('action')) {
                            oGrid3.loadData();
                            $(el).data('action', true);
                        }
                        break;
					case 'litab5':
                        if (!$(el).data('action')) {
                            oGrid4.loadData();
                            $(el).data('action', true);
                        }
                        break;
					case 'litab6':
                        if (!$(el).data('action')) {
                            oGrid5.loadData();
                            $(el).data('action', true);
                        }
                        break;
                }
            },
            /**
             * 查詢當前資料
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前資料實體
             */
            getOneBack: function (pargs, data) {
                var oRes = data,
                    sText = '',
                    elAuditReason = $('#AuditReason');
                oRes.Contactors = $.parseJSON(oRes.Contactors || '[]');
                $('.NotPassReason').show();
                switch (oRes.IsAudit) {
                    case 'N':// ╠common.NotAudit⇒未提交審核╣
                        EditableData = true;
                        sText = i18next.t("common.NotAudit");
                        elAuditReason.css('color', 'red');
                        break;
                    case 'Y':// ╠common.Audited⇒已審核╣
                        sText = i18next.t("common.Audited");
                        elAuditReason.css('color', 'green');
                        break;
                    case 'P':// ╠common.InAudit⇒提交審核中╣
                        sText = i18next.t("common.InAudit");
                        elAuditReason.css('color', 'blue');
                        break;
                    case 'A':// ╠common.AuditAgain⇒重新提交審核中╣
                        sText = i18next.t("common.AuditAgain") + '    （' + (oRes.NotPassReason || '') + '）';
                        elAuditReason.css('color', 'blue');
                        break;
                    case 'Z':// ╠common.ApplyforUpdateing⇒申請修改中╣
                        EditableData = true;
                        sText = i18next.t("common.ApplyforUpdateing") + '    （' + (oRes.NotPassReason || '') + '）';
                        elAuditReason.css('color', 'blue');
                        break;
                    case 'Q':// ╠common.NotPass⇒不通過╣
                        sText = i18next.t("common.NotPass") + '    （' + (oRes.NotPassReason || '') + '）';
                        elAuditReason.css('color', 'red');
                        break;
                }
                elAuditReason.text(sText);
                if ('Y,P,A'.indexOf(oRes.IsAudit) > -1) {
                    disableInput(pargs._form, '.plustemplkey,.btn-custom,[data-input],#WebsiteAdress,#State,#Memo,#Industry,[name="IsGroupUnit"],[name="IsImporter"]');
                    if ('P,A'.indexOf(oRes.IsAudit) > -1 && parent.top.SysSet.CustomersAuditUsers.indexOf(parent.top.UserInfo.MemberID) > -1) {
                        $('#Toolbar_Audit').show();
                    }
                    if (oRes.IsAudit === 'Y') {
                        $('#Toolbar_ApplyforUpdate').show();
                    }
                }
                else {
                    disableInput(pargs._form, '#CustomerNO,#Potential,#BlackListReason,#IndustryStatistics', false);
                    $('#Toolbar_ToAudit').show();
                }
                if (pargs.params.Flag === 'Fit') {
                    $('#Toolbar_Leave,#Toolbar_Del').hide();
                }

                $('#Toolbar_CopySync').hide();
                if (data.IsAudit === 'Y' && 'TE,TG'.indexOf(parent.UserInfo.OrgID) > -1) {
                    //只有已審核才顯示
                    $('#Toolbar_CopySync').show();
                }
				
				if(oRes.UniCode == ""){
					$(".APIImport").show();
				}

				/* 
				let saIndustry = oRes.Industry.split(',');
				$.each(oRes.Industry.split(','), function (idx, data) {
									if(data.ListSource != "" && data.ListSource != null ){
										if($.inArray(data.ListSource, saListSource) < 0){
											saListSource.push(data.ListSource);
										}
										
									}
									
								});
								 */
								 //alert(oRes.Industry);
				//$("#Industry").val(oRes.Industry.split(',')).trigger('change');
            },
            /**
             * 處理新增資料參數
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前表單資料
             */
            getInsertParams: function (pargs, data) {
                var sCustomerFirstChart = '',
                    sCustomerNo_O = '',
					sCustomerNo_N = '';
                //若失敗後，資料已經變成字串了。需要再次轉回陣列，才能刪除
                if (typeof pargs.data.Contactors === 'string') {
                    pargs.data.Contactors = JSON.parse(pargs.data.Contactors || '[]');
                }
                data.Contactors = JSON.stringify(pargs.data.Contactors);
                sCustomerFirstChart = getCustomerFirstChart(data.TransactionType, data.CustomerEName, data.CustomerCName);
                if((parent.top.OrgID == "TE" || parent.top.OrgID == "TG") && (data.CustomerNO.substr(0, 2) == "TE" || data.CustomerNO.substr(0, 2) == "TG")){
					sCustomerNo_O = data.CustomerNO.substr(0, 2) + data.TransactionType + sCustomerFirstChart;
				} else {
					sCustomerNo_O = parent.top.OrgID + data.TransactionType + sCustomerFirstChart;
				}
				sCustomerNo_N = data.TransactionType + sCustomerFirstChart;

                if (sAction === 'Upd') {
					//if(data.CustomerNO.substr(0, 2) == "TE" || data.CustomerNO.substr(0, 2) == "TG" || data.CustomerNO.substr(0, 2) == "SG" || data.CustomerNO.substr(0, 2) == "SE"){ //舊編碼
					if(data.CustomerNO.substr(0, 2) == "TE" || data.CustomerNO.substr(0, 2) == "TG" || data.CustomerNO.substr(0, 2) == "SG" || data.CustomerNO.substr(0, 2) == "SE" || data.CustomerNO.substr(0, 2) == "HY"){ //舊編碼
						if (data.CustomerNO.substr(0, 4) !== sCustomerNo_O) {
							//與原先不相同，重新產生編碼
							data.CustomerNO = sCustomerNo_O;
						}
					} else {
						if (data.CustomerNO.substr(0, 2) !== sCustomerNo_N) {
							//與原先不相同，重新產生編碼
							if(parent.top.OrgID == "TE" || parent.top.OrgID == "TG"){
								data.CustomerNO = sCustomerNo_N;
							} else {
								data.CustomerNO = sCustomerNo_O;
							}
						}
					}
                }
                else {
                    if (!data.CustomerNO) {
						if(parent.top.OrgID == "TE" || parent.top.OrgID == "TG"){
							data.CustomerNO = sCustomerNo_N;
						} else {
							data.CustomerNO = sCustomerNo_O;
						}
                    }
                }
				
				let sIndustry = "";
				
				if($("#Industry").val() != "" && $("#Industry").val() != null){
					$.each($("#Industry").val(), function (idx, item) {
						sIndustry = sIndustry + item + ",";
					});
				}
				data.Industry = sIndustry;
				
				delete data.IndustryStatistics;

                return data;
            },
            /**
             * 處理修改資料參數
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前表單資料
             */
            getUpdateParams: function (pargs, data) {
				let sIndustry = "";
				
				if($("#Industry").val() != "" && $("#Industry").val() != null){
					$.each($("#Industry").val(), function (idx, item) {
						sIndustry = sIndustry + item + ",";
					});
				}
				data.Industry = sIndustry;
				
                return pargs.options.getInsertParams(pargs, data);
            },
            /**
             * 新增資料
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前新增的資料
             * @param {String} flag 新增 or 儲存后新增
             */
            getInsertBack: function (pargs, data, flag) {

                if (pargs.params.From === 'Appoint') {
                    fnReEditCustomer($('body').attr('PopId') || '', data.guid).done(function () {
                        if (pargs.params.Flag === 'Pop') {
                            $('#Toolbar button').prop('disabled', true);
                            parent.fnReFresh(data.guid);
                            showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Upd&guid=' + data.guid); // ╠message.Save_Success⇒新增成功╣
                        }
                    });
                }
                else {
                    if (flag == 'add') {
                        showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Upd&guid=' + data.guid); // ╠message.Save_Success⇒新增成功╣
                    }
                    else {
                        showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                    }
                }
            },
            /**
             * 修改資料
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前修改的資料
             */
            getUpdateBack: function (pargs, data) {
                if (typeof pargs.data.Contactors === 'string') {
                    pargs.data.Contactors = $.parseJSON(pargs.data.Contactors || '[]');
                }
                showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                if (window.bLeavePage) {
                    setTimeout(function () {
                        pargs._pageLeave();
                    }, 1000);
                }
                else {
                    pargs._setFormVal(data);
                    //location.reload();
                }
            },
            /**
             * 客製化驗證規則
             * @param  {Object} pargs CanDo 對象
             */
            validRulesCus: function (pargs) {
                $.validator.addMethod("unicoderequired", function (value, element, parms) {
                    var sTransactionType = $('[name="TransactionType"]:checked').val();
                    if ((sTransactionType === 'A' || sTransactionType === 'D') && value === '') {
                        return false;
                    }
                    return true;
                });
                $.validator.addMethod("unicodelen", function (value) {
                    if (value !== '' && value.length != 8) {
                        return false;
                    }
                    return true;
                });
                $.validator.addMethod("customercnamerequired", function (value, element, parms) {
                    var sTransactionType = $('[name="TransactionType"]:checked').val();
                    if (!(sTransactionType === 'B' || sTransactionType === 'C') && value === '') {
                        return false;
                    }
                    return true;
                });
                $.validator.addMethod("customerenamecus", function (value, element, parms) {
                    var p = /[a-z]/i,
                        b = p.test(value.substr(0, 1));
                    if (!b && value !== '') {
                        return false;
                    }
                    return true;
                });
                $.validator.addMethod("customerenamerequired", function (value, element, parms) {
                    var sTransactionType = $('[name="TransactionType"]:checked').val();
                    if ((sTransactionType === 'B' || sTransactionType === 'C') && value === '') {
                        return false;
                    }
                    return true;
                });


                $.validator.addMethod("customershortnamerule", function (value) {
                    var bRetn = true;
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            guid: pargs.ids.guid,
                            CustomerShotCName: value
                        },
                        function (res) {
                            if (res.RESULT && res.DATA.rel > 0) {
                                bRetn = false;
                            }
                        }, null, false);
                    return bRetn;
                });

                $.validator.addMethod("unicoderule", function (value) {
                    var bRetn = true;
                    if (value) {
                        g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                            {
                                guid: pargs.ids.guid,
                                UniCode: value
                            },
                            function (res) {
                                if (res.RESULT && res.DATA.rel > 0) {
                                    bRetn = false;
                                }
                            }, null, false);
                    }
                    return bRetn;
                });
            },
            /**
             * 驗證規則
             */
            validRules: {
                //若是換頁新增會出錯，必須驗證隱藏的tab
                //ignore: 'disabled',
                onfocusout: false,
                rules: {
                    Email: {
                        email: true
                    },
                    UniCode: {
                        unicodelen: true,
                        unicoderule: true

                    },
                    TransactionType: { required: true },
                    CustomerShotCName: { customershortnamerule: true }
                },
                messages: {
                    Email: i18next.t("message.IncorrectEmail"),// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    TransactionType: { required: i18next.t("Customers_Upd.TransactionType_required") },// ╠Customers_Upd.TransactionType_required⇒請選擇交易類型╣
                    UniCode: {
                        unicodelen: i18next.t("message.UniCodeLength"),// ╠message.UniCodeLength⇒客戶統一編號必須是8碼╣
                        unicoderule: i18next.t("message.Data_Repeat")// ╠message.Data_Repeat⇒此筆資料已建檔╣
                    },
                    CustomerShotCName: { customershortnamerule: i18next.t("message.ShotNameExist") }// ╠message.ShotNameExist⇒該簡稱已被使用╣
                }
            },
            /**
             * 客製化按鈕
             * @param  {Object} pargs CanDo 對象
             */
            cusBtns: function (pargs) {
                var saCusBtns = [];
                if (pargs.action !== 'add') {
                    saCusBtns.push({
                        id: 'Toolbar_ToAudit',
                        value: 'common.SubmitAudit',// ╠common.SubmitAudit⇒提交審核╣
                        /**
                         * 業務提交審核
                         */
                        action: function (pargs) {
                            if (pargs.data.Effective === 'N') {
                                showMsg(i18next.t('message.DataHasInvalid')); // ╠message.DataHasInvalid⇒該資料已無效╣
                                return false;
                            }
							if (pargs.data.CustomerNO.length != 7) {
                                showMsg('客戶編號為空或非正式格式，請修改儲存後再提交審核','error');
                                return false;
                            }
							/* if (pargs.data.CustomerShotCName == '' || pargs.data.CustomerShotCName == null) {
                                showMsg('客戶簡稱不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.UniCode == '' || pargs.data.UniCode == null) {
                                showMsg('統一編號不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.CustomerCName == '' || pargs.data.CustomerCName == null) {
                                showMsg('客戶中文名稱不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.Telephone == '' || pargs.data.Telephone == null) {
                                showMsg('電話不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.State == '' || pargs.data.State == null) {
                                showMsg('國家不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.address == '' || pargs.data.address == null) {
                                showMsg('公司地址不可為空，請修改儲存後再提交審核');
                                return false;
                            }
							if (pargs.data.InvoiceAddress == '' || pargs.data.InvoiceAddress == null) {
                                showMsg('發票地址不可為空，請修改儲存後再提交審核');
                                return false;
                            } */

                            var sIsAudit = oAuditFlag[pargs.data.IsAudit];
                            var sCustomerShotCName = $('#CustomerShotCName').val();
                            var sUniCode = $('#UniCode').val();
                            var sTransactionType = $('[name="TransactionType"]:checked').val();
                            var sTaxpayerOrgID = $('#TaxpayerOrgID').val();

                            g_api.ConnectLite(pargs.ProgramId, 'ToAudit', {
                                guid: pargs.ids.guid,
                                IsAudit: sIsAudit,
                                CustomerShotCName: sCustomerShotCName,
                                UniCode: sUniCode,
                                TransactionType: sTransactionType,
                                TaxpayerOrgID: sTaxpayerOrgID
                            }, function (res) {
                                if (res.RESULT) {
                                    $('#Toolbar_ToAudit').hide();
                                    pargs._getOne();
                                    showMsg(i18next.t("message.ToAudit_Success"), 'success'); // ╠message.ToAudit_Success⇒提交審核成功╣
                                    parent.top.msgs.server.pushTips(parent.top.fnReleaseUsers(res.DATA.rel));
                                }
                                else {
                                    showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                                }
                            }, function () {
                                showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                            });
                        }
                    }, {
                            id: 'Toolbar_CopySync',
                            value: 'Customers_Upd.CopySync', // 複製同步,
                            action: function (parg) {

                            },
                            'data-toggle': 'modal',
                            'data-target': '#CopySync'
                        });

                    if (parent.top.SysSet.CustomersAuditUsers.indexOf(parent.top.UserInfo.MemberID) > -1) {
                        saCusBtns.push({
                            id: 'Toolbar_Audit',
                            value: 'common.Audit',// ╠common.Audit⇒審核╣
                            /**
                             * 主管審核
                             */
                            action: function (pargs) {
                                layer.open({
                                    type: 1,
                                    title: i18next.t('common.Audit'),// ╠common.Audit⇒審核╣
                                    area: ['400px', '260px'],//寬度
                                    shade: 0.75,//遮罩
                                    shadeClose: true,
                                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣// ╠common.Cancel⇒取消╣
                                    content: '<div class="pop-box">\
                                                 <textarea name="NotPassReason" id="NotPassReason" style="min-width:300px;" class="form-control" rows="5" cols="20" placeholderid="common.NotPassReason" placeholder="不通過原因..."></textarea><br>\
                                                 <button type="button" data-i18n="common.Pass" id="audit_pass" class="btn-custom green">通過</button>\
                                                 <button type="button" data-i18n="common.NotPass" id="audit_notpass" class="btn-custom red">不通過</button>\
                                              </div>',
                                    success: function (layero, idx) {
                                        $('.pop-box :button').click(function () {
                                            var oAudit = {},
                                                sNotPassReason = $('#NotPassReason').val();
                                            if (this.id === 'audit_pass') {
                                                oAudit.IsAudit = 'Y';
                                                oAudit.NotPassReason = '';
                                            }
                                            else {
                                                oAudit.IsAudit = 'Q';
                                                oAudit.NotPassReason = sNotPassReason;
                                                if (!sNotPassReason) {
                                                    showMsg(i18next.t("message.NotPassReason_Required")); // ╠message.NotPassReason_Required⇒請填寫不通過原因╣
                                                    return false;
                                                }
                                            }

                                            g_api.ConnectLite(pargs.ProgramId, 'Audit', {
                                                IsAudit: oAudit.IsAudit,
                                                NotPassReason: oAudit.NotPassReason,
                                                guid: pargs.ids.guid
                                            }, function (res) {
                                                if (res.RESULT) {
                                                    $('#Toolbar_Audit').hide();
                                                    layer.close(idx);
                                                    pargs._getOne();
                                                    showMsg(i18next.t("message.Audit_Completed"), 'success'); // ╠message.Audit_Completed⇒審核完成╣
                                                    parent.top.msgs.server.pushTip(parent.top.OrgID, res.DATA.rel);
                                                    if (oAudit.IsAudit === 'Y') {
                                                        parent.top.msgs.server.pushTransfer(parent.top.OrgID, parent.top.UserID, pargs.data.CustomerNO, 2);
                                                    }
                                                }
                                                else {
                                                    showMsg(i18next.t('message.Audit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                                }
                                            }, function () {
                                                showMsg(i18next.t('message.Audit_Failed'), 'error'); // ╠message.Audit_Failed⇒審核失敗╣
                                            });
                                        });
                                        pargs._transLang(layero);
                                    }
                                });
                            }
                        });
                    }

                    saCusBtns.push({
                        id: 'Toolbar_ApplyforUpdate',
                        value: 'common.ApplyforUpdate',// ╠common.ApplyforUpdate⇒申請修改╣
                        /**
                         * 申請修改
                         */
                        action: function (pargs) {
                            layer.open({
                                type: 1,
                                title: i18next.t('common.ApplyforUpdate'),// ╠common.ApplyforUpdate⇒申請修改╣
                                area: ['400px', '260px'],//寬度
                                shade: 0.75,//遮罩
                                shadeClose: true,
                                btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                                content: '<div class="pop-box"><textarea name="Reason" id="Reason" style="min-width:300px;" class="form-control" rows="5" cols="20" placeholderid="common.ApplyforUpdateReason" placeholder="申請修改原因..."></textarea></div>',
                                success: function (layero, idx) {
                                    pargs._transLang(layero);
                                },
                                yes: function (index, layero) {
                                    var sReason = $('#Reason').val();
                                    if (!sReason) {
                                        showMsg(i18next.t("message.Reason_Required")); // ╠message.Reason_Required⇒請填寫原因╣
                                        return false;
                                    }
                                    g_api.ConnectLite(pargs.ProgramId, 'ApplyforUpdate', {
                                        Guid: pargs.ids.guid,
                                        NotPassReason: sReason
                                    }, function (res) {
                                        if (res.RESULT) {
                                            layer.close(index);
                                            $('#Toolbar_ApplyforUpdate').hide();
                                            pargs._getOne();
                                            showMsg(i18next.t("message.HasNoticeAditor"), 'success'); // ╠message.HasNoticeAditor⇒已通知審核人員╣
                                            parent.top.msgs.server.pushTips(parent.top.fnReleaseUsers(res.DATA.rel));
                                        }
                                        else {
                                            showMsg(i18next.t('message.ToApplyFailed') + '<br>' + res.MSG, 'error'); // ╠message.ToApplyFailed⇒提交申請失敗╣
                                        }
                                    }, function () {
                                        showMsg(i18next.t('message.ToApplyFailed'), 'error');// ╠message.ToApplyFailed⇒提交申請失敗╣
                                    });
                                }
                            });
                        }
                    });
                }
                return saCusBtns;
            },
            /**
             * 頁面初始化
             * @param  {Object} pargs CanDo 對象
             */
            pageInit: function (pargs) {
                var postArray = [];
                if (pargs.action === 'add') {
                    $("#litab3").hide();
					$("#litab2").hide();
					$("#litab4").hide();
					$("#litab5").hide();
					$("#litab6").hide();
                }

                $('#CopySync').find('label[OrgID=\"' + g_db.GetItem("orgid").toUpperCase() + '\"]').hide();  //隱藏自己組織別

                $('#Toolbar_CopySync').click(function () {
                    $('#CopySync').find('label[OrgID] > :checkbox').prop('checked', false);  //清除已勾選項目
                });

                $('#CopySycnOK').click(function () {
                    var aryCheckOrgID = [];
                    //取得勾選組織別
                    $('#CopySync').find('label[OrgID]').each(function (idx, element) {
                        var sOrgID = $(element).attr('OrgID');
                        if ($(element).find(':checkbox').is(':checked')) {
                            aryCheckOrgID.push(sOrgID);
                        }
                    });

                    if (aryCheckOrgID.length <= 0) {
                        showMsg(i18next.t("請先選擇要複製同步的公司別"), 'error');
                    }
                    else {
                        canDo.data.currOrgID = g_db.GetItem("orgid").toUpperCase();
                        canDo.data.OrgID = aryCheckOrgID[0];

                        var sCustomerFirstChart = getCustomerFirstChart(canDo.data.TransactionType, canDo.data.CustomerEName, canDo.data.CustomerCName),
                            sCustomerNo = canDo.data.OrgID + canDo.data.TransactionType + sCustomerFirstChart;

                        canDo.data.CustomerNO = sCustomerNo;

                        g_api.ConnectLite(pargs.ProgramId, 'CopySync', canDo.data, function (res) {
                            if (res.RESULT) {
                                var data = res.DATA.rel;
                                showMsg(i18next.t("執行成功,同步新客戶編號為" + data.CustomerNO), 'success');
                                parent.top.msgs.server.pushTips(parent.top.fnReleaseUsers(data));
                            }
                            else {
                                showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                        });

                    }

                });

                if (pargs.action === 'upd') {
                    postArray.push(pargs._getOne());
                }
                else {
                    if (g_db.GetItem("orgid").toUpperCase() === 'SG')  // if (parent.OrgID.toUpperCase() === 'SG')
                        $('.APIImport').hide();
                    else
                        $('.APIImport').show();
                    $('.CustomerEName').hide();
                    pargs.data.Contactors = [];
                }
                postArray.push(fnSetArgDrop([
					{
						OrgID: 'TE',
                        ArgClassID: 'Potential',
                        CallBack: function (data) {
                            $('#Potential').html(createOptions(data, 'id', 'text').replace("請選擇...",""))
                        }
                    },
                    {
                        ArgClassID: 'TranType',
                        CallBack: function (data) {
                            $('#transactiontype').html(createRadios(data, 'id', 'text', 'TransactionType', '', true)).find('label:first').click();
                            pargs._uniformInit($('#transactiontype'));
                        }
                    },
					{
						OrgID: 'TE',
						ArgClassID: 'ExhibClass',
						ShowId: true,
						CallBack: function (data) {
							$('#Industry').html(createOptions(data, 'id', 'text', true)).select2();
							$('#IndustryStatistics').html(createOptions(data, 'id', 'text', true)).select2();
						}
					},
					{
						OrgID: 'TE',
                        ArgClassID: 'BlackListReason',
                        CallBack: function (data) {
                            $('#BlackListReason').html(createOptions(data, 'id', 'text'))
                        }
                    },
					{
						OrgID: 'TE',
						ArgClassID: 'TrasportCompany',
						CallBack: function (data) {
							$('#CoopTrasportCompany').html(createOptions(data, 'id', 'text')).select2();
						}
					}
                ]));
				
                //postArray return 要是function 才會生效
                postArray.push(setStateDrop());
                if (parent.top.OrgID === 'SG') {
                    $('.unicode-box').hide();
                    $('.sg-box').show();
                }
				
				

                //加載報關類別,加載報價頁簽,加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
                $.whenArray(postArray).done(function (res) {
                    let resState = '';
                    if (pargs.action === 'upd' && res[0].RESULT) {
                        var oRes = res[0].DATA.rel;
                        pargs._setFormVal(oRes);
                        fnBindContactors();
                        pargs._getPageVal();//緩存頁面值，用於清除
                        resState = oRes.State;
                        switch (pargs.data.IsAudit) {
                            case 'N'://未提交
                            case 'Q'://退件
                            case 'Z'://申請修改中
                                $('#Toolbar_ApplyforUpdate,#Toolbar_Audit').hide();
                                break;
                            case 'Y':// ╠common.Audited⇒已審核╣
                                $('#Toolbar_ToAudit,#Toolbar_Audit').hide();
                                break;
                            case 'P':// ╠common.InAudit⇒提交審核中╣
                            case 'A'://重新提交審核中
                                $('#Toolbar_ApplyforUpdate').hide();
                                $('#Toolbar_ToAudit').hide();
                                break;
                        }
						if(oRes.Industry){
							$("#Industry").val(oRes.Industry.split(',')).trigger('change');
						}

						if(oRes.IndustryStatistics){
							$("#IndustryStatistics").val(oRes.IndustryStatistics.split(',')).trigger('change');
						}
						
						if(oRes.IsBlackList == "Y"){
							$("#BlackListReason" ).attr('disabled', false);
						}
						
						if(oRes.CoopTrasportCompany){
							$("#CoopTrasportCompany").val(oRes.CoopTrasportCompany.split(',')).trigger('change');
						}
                    }

                    if (pargs.params.Flag === 'Pop') {
                        $('#Toolbar_Leave,#Toolbar_ReAdd,#APIImport').hide();
                    }
                    else if (pargs.params.Flag === 'Fit') {
                        $('#Toolbar_Leave,#Toolbar_Del').hide();
                    }
                    if (canDo.params.FromId) {
                        fnGetImportCustomersByAppointNO();
                    }
                    if ('Y,P,A'.indexOf(pargs.data.IsAudit) > -1) {
                        disableInput(pargs._form, '.plustemplkey,.btn-custom,[data-input],#WebsiteAdress,#State,#Memo,#Industry,[name="IsGroupUnit"],[name="IsImporter"]');
                    }
                    $('[name="TransactionType"]').click(function () {
                        if (this.value === 'A' || this.value === 'D') {
                            $('.innercol,.address,.telephone').show();
                            $('#BankName,#BankAcount,#TaxpayerOrgID,#Telephone,#Address').attr('required', true);
                        }
                        else {
                            $('.innercol,.address,.telephone').hide();
                            $('#BankName,#BankAcount,#TaxpayerOrgID,#Telephone,#Address').removeAttr('required');
                        }
                        if (this.value === 'B' || this.value === 'C') {
                            $('.CustomerCName').hide();
                            $('.CustomerEName').show();

                        }
                        else {
                            $('.CustomerCName').show();
                            $('.CustomerEName').hide();
                        }
                        if (EditableData || sAction === 'Add') {
                            var DefaultChangeState = 'A,D,E,F'.indexOf(this.value) > -1;
                            if (DefaultChangeState) {
                                $('#State').val(DefaultState).trigger("change");
                            }
                            else {
                                $('#State').val('').trigger("change");
                            }
                        }
                    });
                    $('[name="TransactionType"]:checked').click();


                    if (!!resState) {
                        $('#State').val(resState);
                    }
                    $('.plustemplkey').on('click', function () {
                        var oNewKey = {};
                        oNewKey.guid = guid();
                        oNewKey.FullName = '';
                        oNewKey.JobtitleName = '';
                        oNewKey.TEL1 = '';
                        oNewKey.TEL2 = '';
                        oNewKey.FAX = '';
                        oNewKey.Email = '';
                        oNewKey.Memo = '';
                        //若失敗後，資料已經變成字串了。需要再次轉回陣列，才能刪除
                        if (typeof pargs.data.Contactors === 'string') {
                            pargs.data.Contactors = JSON.parse(pargs.data.Contactors || '[]');
                        }
                        pargs.data.Contactors.push(oNewKey);
                        fnBindContactors();
                    });
					
                });

                var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 120;
                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						parent.openPageTab("Exhibition_Upd", '?Action=Upd&SN=' + args.item.SN);
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
							type: "control", title: '查詢單號', itemTemplate: function (val, item) {
								var iTips = 0,
									sTipsHtml = '<div class="layui-layer-btn-c">' + '查詢單號' + '</div>',
									saRefNumber = item.RefNumber.split(','),
									oOption = {
										btnAlign: 'c',
										time: 600000 //一個小時（如果不配置，默认是3秒）
									},
									oTips = $('<span>', {
										'class': 'glyphicon glyphicon-info-sign',
										'aria-hidden': true
									}).on({
										click: function () {
											oOption.btn = [i18next.t("common.Close")];// ╠common.Close⇒關閉╣
											layer.msg(sTipsHtml, oOption);
										},
										mouseenter: function (event) {
											delete oOption.btn;
											iTips = layer.msg(sTipsHtml, oOption);
										},
										mouseleave: function (event) {
											layer.close(iTips);
										}
									});
								if (saRefNumber.length > 0) {
									sTipsHtml += '<ul class="bill-status">';
									$.each(saRefNumber, function (idx, data) {
										let saData = data.split(';');
										let sDataType = saData[0];
										let sGuid = saData[1];
										let sDataContent = saData[2];
										let sTypeName = "";
										let sOnClick = "onclick=''";
										
										switch(sDataType){
											case "1":
												sTypeName = "進口：";
												sOnClick = "onclick='parent.openPageTab(\"ExhibitionImport_Upd\",\"?Action=Upd&ImportBillNO=" + sGuid + "\")'";
												break;
											case "2":
												sTypeName = "出口：";
												sOnClick = "onclick='parent.openPageTab(\"ExhibitionExport_Upd\",\"?Action=Upd&GoTab=2&ExportBillNO=" + sGuid + "\")'";
												break;
											case "3":
												sTypeName = "其他：";
												sDataContent = sDataContent.substring(0,10);
												sOnClick = "onclick='parent.openPageTab(\"OtherBusiness_Upd\",\"?Action=Upd&ImportBillNO=" + sGuid + "\")'";
												break;
											case "4":
												sTypeName = "其他駒驛：";
												sDataContent = sDataContent.substring(0,10);
												sOnClick = "onclick='parent.openPageTab(\"OtherExhibitionTG_Upd\",\"?Action=Upd&GoTab=2&Guid=" + sGuid + "\")'";
												break;
										}
										
										sTipsHtml += "<li><a class='gopagetab' " + sOnClick + "><div>" + sTypeName + sDataContent + "</div></a></li>";
									});
									sTipsHtml += '</ul>';
									oOption.area = ['300px'];
									/* if (saRefNumber.length > 15) {
										oOption.area = ['550px', '500px'];
									} */
								}
								else {
									sTipsHtml = '<div></div>';
								}
								
								oTips.css('color', 'blue');
								
								return oTips;
							}
						},
						{
                            name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', width: 150, align: "center"
                        },
                        {
                            name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', width: 150,align: "center"
                        },
                        {
                            name: "Exhibitioname_EN", title: 'Exhibition_Upd.Exhibitioname_EN', width: 150,align: "center"
                        },
						{
							name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150,
							itemTemplate: function (val, item) {
								var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
								return sDateRange === '~' ? '' : sDateRange;
							}
						},
                        {
                            name: "CreateUserName", title: 'common.CreateUser', width: 150,align: "center"
                        }
						
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetGridData();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
						oGrid = args.grid;
                        //pargs.setGrid(args.grid);
                    }
                });
				
				$("#jsGrid1").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						parent.openPageTab("Exhibition_Upd", '?Action=Upd&SN=' + args.item.SN);
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
                            name: "SN", title: '專案代號', width: 50, align: "center"
                        },
						{
                            name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', width: 150, align: "center"
                        },
                        {
                            name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', width: 150,align: "center"
                        },
                        {
                            name: "Exhibitioname_EN", title: 'Exhibition_Upd.Exhibitioname_EN', width: 150,align: "center"
                        },
						{
							name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150,
							itemTemplate: function (val, item) {
								var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
								return sDateRange === '~' ? '' : sDateRange;
							}
						},
                        {
                            name: "CreateUserName", title: 'common.CreateUser', width: 150,align: "center"
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                           return fnGetGridData1();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
						oGrid1 = args.grid;
                        //pargs.setGrid(args.grid);
                    }
                });
				
				$("#jsGrid2").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: true,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						fnEditContactor(args.item.guid);
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 30, align: "center"
                        },
                        {
                            name: "Call", title: '稱呼', width: 40, align: "center", itemTemplate: function (val, item) {
								if(val=="1"){
									return "Mr.";
								} else if (val=="2"){
									return "Miss.";
								}
							}
                        },
                        {
                            name: "ContactorName", title: '聯絡人名稱', width: 150, align: "center"
                        },
                        {
                            name: "JobTitle", title: 'common.JobTitle', width: 100, align: "center"
                        },
                        {
                            name: "Telephone1", title: 'common.Telephone', width: 120, align: "center"
                        },
						{
                            name: "Ext1", title: 'common.EXT', width: 50, align: "center"
                        },
                        {
                            name: "Email1", title: 'common.Email', width: 150, align: "center"
                        },
                        {
                            name: "Memo", title: 'common.Memo', width: 150, align: "center"
                        },
						{
							name: "OrderByValue", title: 'common.OrderByValue', type: "select", width: 50,
							itemTemplate: function (val, item) {
								return this._createSelect = $("<select>", {
									//class: 'w70',
									html: createOptions(parseInt(item.OrderCount)),
									change: function () {
										let sOldValue = val,
											sNewValue = this.value;
										g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
											CustomerId: canDo.ids.guid,
											Id: item.guid,
											OldOrderByValue: sOldValue,
											NewOrderByValue: sNewValue
										}, function (res) {
											if (res.RESULT) {
												showMsg(i18next.t('message.Update_Success'), 'success');// ╠message.Update_Success⇒更新成功╣
												//canDo.Grid.openPage(canDo.options.toFirstPage ? 1 : canDo.options.queryPageidx);
												oGrid2.loadData();
											}
											else {
												showMsg(i18next.t('message.Update_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Update_Failed⇒更新失敗╣
											}
										});
									}
								}).val(val);
							}
						}
                    ],
                    controller: {
                        loadData: function (args) {
							return fnGetGridData2(args);
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        //pargs.setGrid(args.grid);
						oGrid2= args.grid;
                    }
                });
				
				$("#jsGrid3").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						parent.openPageTab("Callout_Upd", '?Action=Upd&SN=' + args.item.SN + '&guid=' + canDo.ids.guid);
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
                            name: "ExhibitionCode", title: 'Exhibition_Upd.ExhibitionCode', width: 150, align: "center"
                        },
                        {
                            name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', width: 150, align: "center"
                        },
                        {
                            name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', width: 150,align: "center"
                        },
                        {
                            name: "Exhibitioname_EN", title: 'Exhibition_Upd.Exhibitioname_EN', width: 150,align: "center"
                        },
						{
							name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150,
							itemTemplate: function (val, item) {
								var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
								return sDateRange === '~' ? '' : sDateRange;
							}
						},
                        {
                            name: "CreateUserName", title: 'common.CreateUser', width: 150,align: "center"
                        }
                    ],
                    controller: {
                        loadData: function (args) {
							return fnGetGridData3();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid3= args.grid;
                    }
                });
				
				$("#jsGrid4").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						if ('A,C'.indexOf(args.item.DataType) > -1 && args.item.CreateUser === parent.UserID) {
							parent.openPageTab('Complaint_Upd', '?Action=Upd&Guid=' + args.item.Guid);
						}
						else {
							parent.openPageTab('Complaint_View', '?Action=Upd&Guid=' + args.item.Guid);
						}
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 30, align: "center"
                        },
						{
                            name: "ComplaintNumber", title: '個案代號', width: 50, align: "center"
                        },
                        {
                            name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', width: 100, align: "center"
                        },
                        {
                            name: "ExhibitionName", title: 'Exhibition_Upd.Exhibitioname_TW', width: 150,align: "center"
                        },
                        {
                            name: "ComplaintType", title: '類型', width: 50, align: "center", itemTemplate: function (val, item) {
								let strComplaintType = "";
								switch(val){
									case "1":
										strComplaintType = "貨損";
										break;
									case "2":
										strComplaintType = "延誤";
										break;
									case "3":
										strComplaintType = "遺失";
										break;
									case "4":
										strComplaintType = "抱怨";
										break;
								}
								return strComplaintType;
							}
                        },
                        {
                            name: "Description", title: '內容', width: 300, align: "center"
                        },
                        {
                            name: "CreateUserName", title: 'common.CreateUser', width: 70,align: "center"
                        }
                    ],
                    controller: {
                        loadData: function (args) {
							return fnGetGridData4();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid4= args.grid;
                    }
                });
				
				$("#jsGrid5").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.top.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
					rowDoubleClick: function (args) {
						parent.openPageTab('SatisfactionCase_Upd',"?Action=Upd&SN=" + args.item.SN );
					},
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
                            name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', width: 150, align: "center"
                        },
                        {
                            name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', width: 150,align: "center"
                        },
                        {
                            name: "Feild01", title: '整體服務品質滿意度', width: 150, align: "center"
                        },
                        /* {
                            name: "SatisfactionDetail", title: '滿意度細節', width: 150, align: "center"
                        }, */
						{
							name: "control1",
							width: 100,
							title: '滿意度細節',
							align: "center",
							itemTemplate: function (val, item) {
								var saBtn = [];
								saBtn.push($('<button />', {
									type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-file"></i>', click: function () {
										fnGetSatisfactionCaseData(item.CustomerSN);
									}
								}));
								return saBtn;
							},
							deleteButton: false,
							editButton: false
						},
                        {
                            name: "CreateUserName", title: 'common.CreateUser', width: 150,align: "center"
                        }
                    ],
                    controller: {
                        loadData: function (args) {
							return fnGetGridData5();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid5= args.grid;
                    }
                });

                $('#UniCode').on('blur', function () {
                    this.value = $.trim(this.value);
                });
                $('#CustomerEName,#CustomerShotCName,#Address,#InvoiceAddress').on('blur', function () {
                    this.value = this.value.toUpperCase();
                });

                $('#APIImport').on('click', function () {
                    var sUniCode = $('#UniCode').val(),
                        sCustomerCName = $('#CustomerCName').val();
                    if (!sUniCode && !sCustomerCName) {
                        showMsg(i18next.t('message.UniCodeOrCustomerCName_Required')); // ╠message.UniCodeOrCustomerCName_Required⇒請輸入客戶統編或者客戶全稱╣
                        return false;
                    }

                    if (sUniCode && sUniCode.length !== 8) {
                        showMsg(i18next.t('message.UniCodeInvalid')); // ╠message.UniCodeInvalid⇒統編號碼不合法╣
                        return false;
                    }
                    g_api.ConnectLite(pargs.ProgramId, 'GetCrmBaseDataByUniCode', {
                        UniCode: sUniCode,
                        KeyWords: sCustomerCName
                    }, function (res) {
                        if (res.RESULT) {
                            var oBase = res.DATA.rel,
                                oFirst = {};
                            if (oBase) {
                                var saBase = $.parseJSON(oBase);
                                if ($.isArray(saBase)) {
                                    oFirst = saBase[0];
                                    if (!$.isEmptyObject(oFirst)) {
                                        $('#UniCode').val(oFirst.Business_Accounting_NO);
                                        $('#CustomerCName').val(oFirst.Company_Name);
                                        $('#Address').val(oFirst.Company_Location);
                                        $('#InvoiceAddress').val(oFirst.Company_Location);
                                    }
                                }
                            }
                        }
                        else {
                            showMsg(i18next.t('message.NotFindData') + '<br>' + res.MSG, 'error'); // ╠message.NotFindData⇒查不到對應的資料╣
                        }
                    }, function () {
                        showMsg(i18next.t('message.NotFindData'), 'error');
                    });
                });
				
				$("[name='IsBlackList']").on('change', function () {
					if($("[name='IsBlackList']:checked").val() == "Y"){
						$("#BlackListReason" ).attr('disabled', false);
					} else {
						$("#BlackListReason").val("");
						$("#BlackListReason" ).attr('disabled', true);
					}
				});
				
				$('#InsertContactor').on('click', function () {
					fnCreateContactor();
				});
				
				$('#InsertExhibitionList').on('click', function () {
					fnInsertExhibitionList();
				});
				
				$('#InsertComplaint').click(function () {
					parent.openPageTab('Complaint_Upd', '?Action=Add&CustomerId=' + canDo.ids.guid);
					return false;
				});
				
				
            }

        }),
        /**
         * 設定國家下拉選單
         */
        setStateDrop = function () {
            return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                OrgID: 'TE',
                ArgClassID: 'Area',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    let saState = res.DATA.rel;
                    if (saState.length > 0) {
                        $('#State').append(createOptions(saState, 'id', 'text', true)).select2();
                    }
                }
            });
        },

        /**
         * 客戶轉為正式資料后回寫動作
         * @param {String} preid 轉正前的客戶id
         * @param {String} afterid 轉正后的客戶id
         */
        fnReEditCustomer = function (previd, afterid) {
            return g_api.ConnectLite('Exhibition_Upd', 'UpdateCustomerTag', {//匯入費用項目
                PrevId: previd,
                AfterId: afterid
            }, function (res) { });
        },
        /**
         * 綁定模版參數
         */
        fnBindContactors = function () {
            var sKeysHtml = '';
            $.each(canDo.data.Contactors, function (idx, item) {
                sKeysHtml += '<tr data-id="' + item.guid + '">\
                              <td class="wcenter">' + (idx + 1) + '</td>\
                              <td><input type="text" data-input="FullName" class="form-control w100p" value="' + item.FullName + '"></td>\
                              <td><input type="text" data-input="JobtitleName" class="form-control w100p" value="' + (item.JobtitleName || '') + '"></td>\
                              <td><input type="text" data-input="TEL1" class="form-control w100p" value="' + item.TEL1 + '" placeholderid="Customers_Upd.Instruction_ContactorTEL" placeholder="含分機"></td>\
                              <td><input type="text" data-input="TEL2" class="form-control w100p" value="' + item.TEL2 + '" placeholderid="Customers_Upd.Instruction_ContactorTEL" placeholder="含分機"></td>\
                              <td><input type="text" data-input="FAX" class="form-control w100p" value="' + item.FAX + '"></td>\
                              <td><input type="text" data-input="Email" class="form-control w100p" value="' + item.Email + '"></td>\
                              <td><input type="text" data-input="Memo" class="form-control w100p" value="' + item.Memo + '"></td>\
                              <td><input type="number" data-input="RowID" class="form-control w100p" data-OriValue="' + (idx + 1) + '" value="' + (idx + 1) + '"></td>\
                              <td class="wcenter">\
                                 <i class="glyphicon glyphicon-trash" data-value="' + item.guid + '" title="刪除"></i>\
                              </td>\
                             </tr>';
            });
            $('#table_box').html(sKeysHtml).find('.glyphicon-trash').on('click', function () {
                var sId = $(this).attr('data-value'),
                    saNewList = [];
                //若失敗後，資料已經變成字串了。需要再次轉回陣列，才能刪除
                if (typeof canDo.data.Contactors === 'string') {
                    canDo.data.Contactors = JSON.parse(canDo.data.Contactors || '[]');
                }

                $.each(canDo.data.Contactors, function (idx, item) {
                    if (sId !== item.guid) {
                        saNewList.push(item);
                    }
                });
                $(this).parents('tr').remove();
                canDo.data.Contactors = saNewList;
            });
            canDo._transLang($('#table_box'));
            $('#table_box [data-input]').on('change click', function () {
                var sKey = $(this).attr('data-input'),
                    sId = $(this).parents('tr').attr('data-id'),
                    sVal = this.value;
                if (sKey === "RowID") {
                    let OriValue = $(this).attr('data-OriValue');
                    let FromIndex = Number(OriValue) - 1;
                    let ToIndex = Number(sVal) - 1;
                    let TempTarget = canDo.data.Contactors[ToIndex];
                    let TempOrigin = canDo.data.Contactors[FromIndex];
                    canDo.data.Contactors.splice(FromIndex, 1);
                    canDo.data.Contactors.splice(ToIndex, 0,TempOrigin);
                    fnBindContactors();
                }
                else {
                    $.each(canDo.data.Contactors, function (idx, item) {
                        if (sId === item.guid) {
                            item[sKey] = sVal;
                            return false;
                        }
                    });
                }
                
            });
        },
        /**
         * 設定交易型態下拉選單
         * @param {String} type 公司類別
         * @param {String} egname 英文名稱
         * @param {String} cnname 中文名稱
         */
        getCustomerFirstChart = function (type, egname, cnname) {
            var sFirstChart = '';
            if (type === 'E') {
                if (egname) {
                    if (egname.indexOf('.') > -1) {
                        if (egname.split('.')[1]) {
                            sFirstChart = egname.split('.')[1].substr(0, 1);
                        }
                        else {
                            sFirstChart = egname.split('.')[0].substr(0, 1);
                        }
                    }
                    else {
                        sFirstChart = egname.substr(0, 1);
                    }
                }
                else {
                    sFirstChart = cnname.substr(0, 1);
                }
            }
            else {
                if (egname) {
                    sFirstChart = egname.substr(0, 1);
                }
                else {
                    sFirstChart = cnname.substr(0, 1);
                }
            }
            sFirstChart = makePy(sFirstChart[0]);
            return sFirstChart[0].toUpperCase();
        },
        /**
         * 依據預約單號查詢匯入廠商并預設
         */
        fnGetImportCustomersByAppointNO = function () {
            g_api.ConnectLite(canDo.ProgramId, 'GetImportCustomersByAppointNO', {
                AppointNO: canDo.params.FromId
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (oRes) {
                        $('#CustomerCName').val(oRes.CustomerCName);
                        $('#Telephone').val(oRes.Telephone);
                        $('#Email').val(oRes.Email);
                        $('#Address').val(oRes.Address);
                        $('#InvoiceAddress').val(oRes.InvoiceAddress);
                        $('body').attr('PopId', oRes.guid);
                        setTimeout(function () {
                            $('.plustemplkey ').trigger('click');
                            var _input = $('#table_box').find(':input');
                            _input.eq(0).val(oRes.Contactor);
                            _input.eq(2).val(oRes.Telephone);
                            _input.eq(5).val(oRes.Email);
                            setTimeout(function () {
                                _input.eq(0).trigger('click');
                                _input.eq(2).trigger('click');
                                _input.eq(5).trigger('click');
                            }, 500);
                        }, 1000);
                    }
                }
            });
        },
        /**
         * 抓去參加展覽已成交列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData = function () {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetDealExhibitionlist', {
                    guid: canDo.ids.guid
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
         * 抓去參加展覽未成交列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData1 = function () {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetUnDealExhibitionlist', {
                    guid: canDo.ids.guid
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
         * 抓去聯絡人列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData2 = function (args) {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetContactorlist', {
                    Guid: canDo.ids.guid,
					sortField: args.sortField,
					sortOrder: args.sortOrder
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
         * 抓去Callout記錄列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData3 = function () {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetCalloutlist', {
                    guid: canDo.ids.guid
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
         * 抓去客訴列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData4 = function () {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetComplaintlist', {
                    guid: canDo.ids.guid
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
         * 抓去滿意度列表資料
         * @return {Object} ajax物件
         */
        fnGetGridData5 = function () {
            if (canDo.ids.guid) {
                return g_api.ConnectLite(canDo.ProgramId, 'GetSatisfactionCaselist', {
                    guid: canDo.ids.guid
                });
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
		/**
		 * 新增聯絡人
		 */
		fnCreateContactor = function () {
			layer.open({
				type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
				title: i18next.t('common.InsertContactor'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
				area: ['70%', '90%'],//寬度
				shade: 0.75,//遮罩
				maxmin: true, //开启最大化最小化按钮
				id: 'layer_InsertContactor', //设定一个id，防止重复弹出
				anim: 0,//彈出動畫
				btnAlign: 'c',//按鈕位置
				content: '../Crm/Contactors_Upd.html?Action=Add&Flag=Pop&CustomerId=' + canDo.ids.guid,
				success: function (layero, index) {
					var iframe = layero.find('iframe').contents();
					iframe.find('#hiddenIndex').val(index);
				},
				end: function () {
					oGrid2.loadData();
				}
			});
		},
		/**
		 * 編輯聯絡人
		 */
		fnEditContactor = function (_guid) {
			layer.open({
				type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
				title: i18next.t('common.InsertContactor'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
				area: ['70%', '90%'],//寬度
				shade: 0.75,//遮罩
				maxmin: true, //开启最大化最小化按钮
				id: 'layer_InsertContactor', //设定一个id，防止重复弹出
				anim: 0,//彈出動畫
				btnAlign: 'c',//按鈕位置
				content: '../Crm/Contactors_Upd.html?Action=Upd&Flag=Pop&guid=' + _guid,
				success: function (layero, index) {
					var iframe = layero.find('iframe').contents();
					iframe.find('#hiddenIndex').val(index);
				},
				end: function () {
					oGrid2.loadData();
				}
			});
		},
		/**
		* 新增至展覽名單
		*/
		fnInsertExhibitionList = function () {
			layer.open({
				type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
				title: '新增至展覽名單', // ╠common.CorrespondImpCus⇒對應正式客戶╣
				area:  ['60%', '90%'],//寬度
				shade: 0.75,//遮罩
				closeBtn: 1,
				//maxmin: true, //开启最大化最小化按钮
				id: 'layer_InsertExhibitionList', //设定一个id，防止重复弹出
				offset: '10px',//右下角弹出
				anim: 0,//彈出動畫
				btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
				btnAlign: 'c',//按鈕位置
				content: '<style>.select2-container{z-index: 39891015;}</style>\
						<div class="pop-box row w100p">\
							<label class="col-sm-2 col-sm-offset-2 control-label" for="input-Default">\
								<span>請選擇展覽</span><span>：</span>\
							</label>\
							<div class="col-sm-5">\
								<select class= "form-control w95p" id="ExhibitionCode" name="ExhibitionCode"></select>\
							</div>\
						</div >\
						<hr>\
						<div class="col-sm-10 col-sm-offset-1">\
							<div id="jsGridChooseContactors"></div>\
						 </div>',
				success: function (layero, index) {
					saContactorList = [];
					fnSetEpoDrop({
                        Select: $('#ExhibitionCode'),
                        Select2: true
                    });
					$("#jsGridChooseContactors").jsGrid({
							width: "100%",
							height: "auto",
							autoload: true,
							filtering: false,
							pageLoading: true,
							inserting: false,
							editing: false,
							sorting: false,
							paging: false,
							pageIndex: 1,
							pageSize: parent.SysSet.GridRecords || 10,
							confirmDeleting: true,
							//deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
							pagePrevText: "<",
							pageNextText: ">",
							pageFirstText: "<<",
							pageLastText: ">>",
							fields: [
								{
									name: "RowIndex", title: 'common.RowNumber', width: 5, align: "center",
									itemTemplate: function (value, item) {
										return $("<input>", {
											type: 'checkbox', click: function (e) {
												e.stopPropagation();
												if (this.checked) {
													item.RowIndex = saContactorList.length + 1;
													saContactorList.push(item);
												}
												else {
													
													var saNewList2 = [];
													$.each(saContactorList, function (idx, data) {
														let i = 0;
														if (item.guid !== data.guid) {
															data.RowIndex = i++;
															saNewList2.push(data);
														}
													});
													saContactorList = saNewList2;
												}
											}
										});
									}
								},
								{
									name: "ContactorName", title: 'common.Contactor', width: 25, align: "center"
								},
								{
									name: "Telephone1", title: 'common.Telephone', width: 30
								},
								{
									name: "Ext1", title: 'common.EXT', width: 15
								},
								{
									name: "Email1", title: 'common.Email', width: 40
								}
							],
							controller: {
								loadData: function (args) {
									return g_api.ConnectLite('Contactors_Qry', 'QueryByCustomer', {
										CustomerId: canDo.ids.guid
									});
								},
								insertItem: function (args) {
								},
								updateItem: function (args) {
								},
								deleteItem: function (args) {
								}
							}
						});
				},
				yes: function (index, layero) {
					g_api.ConnectLite(sProgramId, 'InsertExhibitionList', {
						ExhibitionNO:$("#ExhibitionCode").val(),
						CustomerId: canDo.ids.guid,
						Contactors:saContactorList
					}, function (res) {
						if (res.RESULT) {
							oGrid.loadData();
							oGrid1.loadData();
							showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
							layer.close(index);
						}
						else {
							showMsg(res.MSG, 'error');
						}
					}, function () {
						showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
					});
				}
			});
		},
		fnGetSatisfactionCaseData = function (_SN) {
				return g_api.ConnectLite('SatisfactionCase_Upd', 'GetSatisfactionCaseData', {
					SN: _SN,                      						
                }, function (res) {
					var oRes = res.DATA.rel;
					layer.open({
						type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
						title: "滿意度問卷", // ╠common.CorrespondImpCus⇒對應正式客戶╣
						area: ['40%', '90%'],//寬度
						shade: 0.75,//遮罩
						closeBtn: 1,
						//maxmin: true, //开启最大化最小化按钮
						id: 'layer_SatisfactionCaseData', //设定一个id，防止重复弹出
						offset: '10px',//右下角弹出
						anim: 0,//彈出動畫
						//btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
						//btnAlign: 'c',//按鈕位置
						content: '<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">客戶名稱</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="CustomerName" name="CustomerName" class="form-control w100p" placeholderid="" value="' + oRes.CustomerName + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="FillerName" name="FillerName" class="form-control w100p" placeholderid="" value="' + oRes.FillerName + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人電子郵件</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Email" name="Email" class="form-control w100p" placeholderid="" value="' + oRes.Email + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人聯絡電話</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Phone" name="Phone" class="form-control w100p" placeholderid="" value="' + oRes.Phone + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">備註</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<textarea name="Memo" id="Memo" class="form-control" rows="3" disabled>' + oRes.Memo + '</textarea>\
									</div>\
								</div>\<hr>\
								<div>\
									<table class="w80p text-left" style="border:1px #cccccc solid;margin-Left:10%;font-size:14px"><thead></thead>\
									<tbody>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">奕達提供整體服務品質的滿意度：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild01">' + oRes.Feild01 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">奕達提供的價格是否合理：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild02">' + oRes.Feild02 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">展品送達時間是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild03">' + oRes.Feild03 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">現場人員的專業技能與服務態度是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild04">' + oRes.Feild04 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">承辦同仁的配合度及服務態度是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild05">' + oRes.Feild05 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">「貨況線上查詢系統」是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild06">' + oRes.Feild06 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">為何選擇奕達：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild07">' + oRes.Feild07 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">貴公司年度平均參與海外展會活動次數：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild08">' + oRes.Feild08 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">您是否會推薦奕達給合作夥伴：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild9">' + oRes.Feild09 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">其他建議：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild10">' + oRes.Feild10 + '</div></td>\
									</tr>\
									</tbody>\
								</table>\
								</div><div class="pop-box row w100p"></div>',
						success: function (layero, index) {
							
						},
						yes: function (index, layero) {
							
						}
					});							
				});				
            };


};


require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);
