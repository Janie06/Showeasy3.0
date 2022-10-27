'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('SN'),
    sCheckId = sDataId,
    bRefresh = false,
    fnPageInit = function () {
        var oGrid = null,
            oForm = $('#form_main'),
            oValidator = null,
            oCurData = {},
			sCustomersOptionsHtml = '',
            saGridData = [],
            saHalls = [],
			saCustomers = [],
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.top.SysSet.GridRecords || 10,
                sortField: 'SN',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {							
                if (sDataId) {
					$(".editbox").show();
					return g_api.ConnectLite(sQueryPrgId, "QueryOne",
                        {
                            SN: sDataId
                        }, function (res) {	
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oCurData = oRes;								
								$("#CaseName").val(oRes.CaseName);
								$("#ExhibitionNO").val(oRes.ExhibitionNO);
								
								$("#ExhibitionNO").change();													
                            }
                        });
                } else {
                    oCurData.LogoFileId = guid();
					$(".editbox").hide();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param   {String} sFlag  新增或儲存後新增
             */
            fnAdd = function (sFlag) {				
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.LogoFileId = oCurData.LogoFileId;

                g_api.ConnectLite("SatisfactionCase_Upd", "Add", 
					data
				, function (res) {
                    if (res.RESULT) {
                        //data.ExhibitionCode = res.DATA.rel;
						
						showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&SN=' + res.DATA.rel); // ╠message.Save_Success⇒新增成功╣
						/*
                        CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                            Params: { exhibition: data }
                        }, function (res1) {
                            if (res1.d > 0) {
                                bRequestStorage = false;
								
                                if (sFlag === 'add') {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            exhibition: {
                                                ExhibitionCode: data.ExhibitionCode
                                            },
                                        }
                                    }, function (res2) {
                                        var oRes = $.parseJSON(res2.d);
                                        showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&SN=' + oRes.SN); // ╠message.Save_Success⇒新增成功╣
                                    });
                                }
                                else {
                                    showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                                }
                                if (data.ExhibitionDateStart && data.ExhibitionDateEnd) {
                                    //如果展覽時間不為空的話就同步更新至行事曆和outlook中
                                    fnSynChronousCalendar(data);
                                }
                            }
                            else {
                                showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        });*/
                    }
                    else {
                        showMsg(i18next.t('message.CreateBill_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.CreateBill_Failed'), 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                });
            },
            /**
             * 檢查名稱
             */
            fnCheckNameThenAction = function (Type) {
                return g_api.ConnectLite(sProgramId, 'CheckExhibitionName', {
                    Type: Type,
                    SN: getUrlParam('SN') ,
                    Exhibitioname_TW: $('#Exhibitioname_TW').val() ? $('#Exhibitioname_TW').val():  "",
                    ExhibitioShotName_TW: $('#ExhibitioShotName_TW').val() ? $('#ExhibitioShotName_TW').val() : ""
                }, function (res) {
                    if (!res.MSG) {
                        switch (Type) {
                            case "add":
                            case "readd":
                                fnAdd(Type);
                                break;
                            case "upd":
                                fnUpd();
                                break;
                            default:
                                break;
                        }
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg("未知錯誤，請聯絡資訊人員", 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                });
            },
            /**
             * 修改資料
             */
            fnUpd = function () {
                //var data = getFormSerialize(oForm);
                //data = packParams(data, 'upd');

                /* CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        contactors: {
                            values: data,
                            keys: { SN: sDataId }
                        }
                    } */
				g_api.ConnectLite(sProgramId, 'UpdateCase', {
                    SN: sDataId,
                    CaseName: $('#CaseName').val() ? $('#CaseName').val():  "",
                    ExhibitionNO: $('#ExhibitionNO').val() ? $('#ExhibitionNO').val() : ""
                }, function (res) {
                    if (res.RESULT) {
                        bRequestStorage = false;
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                        if (window.bLeavePage) {
                            setTimeout(function () {
                                pageLeave();
                            }, 1000);
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
                 return g_api.ConnectLite(sProgramId, 'Delete', {
					SN: sDataId
                }, function (res) {
                    if (res.RESULT > 0) {
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
             * 抓取滿意度列表資料
             * @param  {String} flag 是否匯出
             * @return {Object} ajax物件
             */
            fnGetGridData = function (flag) {				
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetSatisfactionList', {
                        SN: sDataId,
                        Flag: flag,
						pageIndex:oBaseQueryPm.pageIndex,
						pageSize: oBaseQueryPm.pageSize,
						sortField: 'SN',
						sortOrder: 'asc'
                    }, function (res) {
                        if (res.RESULT) {
                            if (flag === 'export') {
                                DownLoadFile(res.DATA.rel, oCurData.Exhibitioname_TW);
                            }
                            else {
                                saGridData = res.DATA.rel;
                            }
                        }
                    });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
			
			/**
             * 重新比對
             */
			fnCompareDB = function (){			
                if (sDataId) {		
                   
                    return g_api.ConnectLite(sProgramId, 'CompareDB', {
                        SN: sDataId,                      
						pageIndex:oBaseQueryPm.pageIndex,
						pageSize: oBaseQueryPm.pageSize,
						sortField: 'SN',
						sortOrder: 'asc'
                    }, function (res) {	
						if (res.RESULT) {
							saGridData = res.DATA.rel;  
						}							
                    });
                } else {
                    return $.Deferred().resolve().promise();
                }		
			},
			
			/**
             * 刪除滿意度案件客戶資料
             */
			fnDelSatisfactionCustomer = function (_args){			
				if (_args.SN) {					
                    return g_api.ConnectLite(sProgramId, 'DeleteSatisfactionCustomer', {
                        SN: _args.SN,                      						
                    }, function (res) {	
						if (res.RESULT > 0) {
							showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
						}
						else {
							showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
						}
                    });
                } else {
                    return $.Deferred().resolve().promise();
                }		
			},
			
			fnImportFile = function () {
                $('#importfile').val('').off('change').on('change', function () {
                    if (this.value.indexOf('.csv') > -1 || this.value.indexOf('.CSV') > -1) {
                        var sFileId = guid(),
                            sFileName = this.value;
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
                            secureuri: false,
                            fileElementId: 'importfile',
                            success: function (data, status) {
								
                                g_api.ConnectLite(sProgramId, 'ImportFile', {
                                    FileId: sFileId,
                                    FileName: sFileName,
                                    SN: sDataId
                                }, function (res) {
									
                                    if (res.RESULT) {
                                        fnCompareDB();
                                        oGrid.loadData();
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
			
			fnGetSatisfactionCaseData = function (_SN) {
				return g_api.ConnectLite(sProgramId, 'GetSatisfactionCaseData', {
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
						btn: ['儲存', i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
						btnAlign: 'c',//按鈕位置
						content: '<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">客戶名稱</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="CustomerName" name="CustomerName" class="form-control w100p" placeholderid="" value="' + oRes.CustomerName + '">\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="FillerName" name="FillerName" class="form-control w100p" placeholderid="" value="' + oRes.FillerName + '">\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人電子郵件</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Email" name="Email" class="form-control w100p" placeholderid="" value="' + oRes.Email + '">\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人聯絡電話</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Phone" name="Phone" class="form-control w100p" placeholderid="" value="' + oRes.Phone + '">\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">備註</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<textarea name="Memo" id="Memo" class="form-control" rows="3">' + oRes.Memo + '</textarea>\
									</div>\
								</div>\
								<hr>\
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
							layer.confirm("確定要儲存？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
								var iframe = layero.find('iframe').contents();								
								var SatisfactionCustomerData = getFormSerialize($(iframe.find('#form_main')));
										
								SatisfactionCustomerData.CustomerName = $("#CustomerName").val();
								SatisfactionCustomerData.FillerName = $("#FillerName").val();
								SatisfactionCustomerData.Email = $("#Email").val();
								SatisfactionCustomerData.Phone = $("#Phone").val();
								SatisfactionCustomerData.Memo = $("#Memo").val();
								SatisfactionCustomerData.SN=_SN;										
											
								g_api.ConnectLite(sProgramId, 'Update', SatisfactionCustomerData
								, function (res) {
									if (res.DATA.rel) {
										showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
										layer.close(index);
									}
									else {
										showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
									}
								}
								, function () {
									showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
								});
								
								layer.close(index-1);
							});
						},
						end: function () {							
							oGrid.loadData();
						}
					});							
				});				
            },
			/**
            * 目的 對應正式客戶
            * @param  {String}item 預約單資料
            */
            fnCorrespondFormalCus = function (item) {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t("common.CorrespondFormalCus"), // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: '640px;',//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_Correspond', //设定一个id，防止重复弹出
                    offset: '100px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style><div class="form-group">\
                                 <div class="col-sm-12">\
                                     <select class= "form-control w95p" id="CustomerId" name="CustomerId"></select>\
                                 </div>\
                              </div >',
                    success: function (layero, index) {
                        g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'id', 'text');
                                $('#CustomerId').html(sOptions).select2();
                            }
                        });
                        //transLang(layero);
                    },
                    yes: function (index, layero) {
                        let sCustomerId = $('#CustomerId').val();
                        if (!sCustomerId) {
                            showMsg(i18next.t('message.SelectFormalCus'));//╠message.SelectFormalCus⇒請選擇對應的客戶╣
                            return false;
                        }
                        return g_api.ConnectLite(sProgramId, 'CorrespondFormalCus', {
							SN: item.SN,
                            CustomerId: sCustomerId
                        }, function (res) {
                            if (res.DATA.rel) {
                                oGrid.loadData();
                                showMsg(i18next.t("message.Correspond_Success"), 'success'); //╠message.Correspond_Success⇒對應成功╣
                                layer.close(index);
                            }
                            else {
                                showMsg(i18next.t("message.Correspond_Failed"), 'error');//╠message.Correspond_Failed⇒對應失敗╣
                            }
                        });
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
                        $('#file_hidden').val($('li.jFiler-item').length || '');
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

                        $('#file_hidden').val($('li.jFiler-item').length || '');
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
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
						
                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

                        break;
                    case "Toolbar_Imp":

                        break;
                    case "Toolbar_Transfer":

                        fnTransfer();

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
                //$('#State,#Industry,#ExhibitionDate').prop('required', true);
                //$('.ShowNames').hide();
                /* if (parent.OrgID === 'TG') {
                    $('#CostRulesId,#State,#ExhibitionAddress,#ExhibitionDate,#file_hidden').prop('required', true);
                    $('#notte').show();
                    $('.simp-box').hide();
                }
                else {
                    $('.costrules').hide();
                }
                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'Toolbar_Transfer',
                        value: 'common.Toolbar_Transfer'// ╠common.Toolbar_Transfer⇒拋轉╣
                    }];
                } */

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true
                });

                $.whenArray([
					fnSetEpoDrop({
                        Select: $('#ExhibitionNO'),
                        Select2: true
                    }).done(function (){
						  fnGet();
					})
                  
					
					
					//fnSetCustomersDrop()
                    /* setExhibitionAddressDrop(),
                    setCostRulesDrop(),
                    fnSetArgDrop([
                        {
                            OrgID: 'TE',
                            ArgClassID: 'ExhibClass',
                            Select: $('#Industry'),
                            ShowId: true
                        }
                    ]) */
                ])
				.done(function (res) {
					
					
					if (res && res.RESULT === 1) {
						var oRes = res.DATA.rel,
							sDateRange = '';
						oCurData = oRes;
						oCurData.LogoFileId = oCurData.LogoFileId || guid();
						setFormVal(oForm, oCurData);
						
						/* if (sTab) {
							$('#litab3 a').click();
						} */
						
						setNameById().done(function () {
							getPageVal();//緩存頁面值，用於清除
						});
						
					}
					
					//select2Init();
					
					var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 150;
					$("#jsGrid").jsGrid({
						width: "100%",
						height: "auto",
						autoload: true,
						filtering: false,
						pageLoading: true,
						inserting: false,
						editing: false,
						sorting: false,
						paging: true,
						pageIndex: 1,
						pageSize: parent.top.SysSet.GridRecords || 10,
						confirmDeleting: true,
						deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
						pagePrevText: "<",
						pageNextText: ">",
						pageFirstText: "<<",
						pageLastText: ">>",
						rowClick: function (args) {
							/* if (navigator.userAgent.match(/mobile/i)) {
								goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
							} */
						},
						rowDoubleClick: function (args) {
							//parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
						},
						fields: [
							{
								name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
							},
							{
								name: "CompareDB", title: '比對資料庫', width: 80, align: "center"
							},
							{// ╠common.IsFormal⇒資料狀態╣
								name: "IsFormal",
								title: '比對資料庫',
								width: 80,
								align: 'center',
                                itemTemplate: function (val, item) {
									var saAction = [];
                                    if (item.CompareDB == "N") {
										saAction.push($('<a/>', {
											html: i18next.t('common.CorrespondFormalCus'),
											class: 'link',
											click: function () {
												fnCorrespondFormalCus(item);
												return false;
											}
										}));
									}
                                    else {
										saAction.push($('<span />', { text: '已對應' }).css('color', 'green'));
									}
									return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
								}
							},
							{
								name: "CustomerName", title: '客戶名稱', width: 200, type: "text"
								/*itemTemplate: function (val, item) {
									if (item.CompareDB != "Y") {
										return this._createSelect = $("<select>", {
											class: 'w100p',
											html: sCustomersOptionsHtml,
											change: function () {
												var sOldValue = val,
													sNewValue = this.value;
												g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
													Id: item.Guid,
													OldOrderByValue: sOldValue,
													NewOrderByValue: sNewValue
												}, function (res) {
													if (res.RESULT) {
														oGrid[sKey].openPage(1);
													}
												});
											}
										}).val(val);
									}
									else {
										var saAction = [];
										saAction.push($('<span />', { text: item.CustomerName }));
										return $('<div>', { 'style': 'width:100%;text-align: left;' }).append(saAction);
									}
									
								} */
							},
							{
								name: "FillerName", title: '填寫人名稱', type: "text", width: 200
							},
							{
								name: "Phone", title: '連絡電話', type: "text", width: 200
							},
							{
								name: "Email", title: 'EMAIL', type: "text", width: 120
							},
							{
								name: "Feild01", title: '整體評分', width: 180, align: "center"
							},
							{
								name: "control1",
								width: 100,
								title: '問券',
								align: "center",
								itemTemplate: function (val, item) {
									var saBtn = [];
									saBtn.push($('<button />', {
										type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-file"></i>', click: function () {
											fnGetSatisfactionCaseData(item.SN);
										}
									}));
									return saBtn;
								},
								deleteButton: false,
								editButton: false
							},
							{
								type: "control",
								width: 100,
								headerTemplate: function () {
									var saBtn = [];
									/* if (sAction === 'Upd') {
										saBtn.push($('<button />', {
											type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-import"></i>', click: function () {
												fnImportCusList();
											}
										}));
										saBtn.push($('<button />', {
											type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Exp'), html: '<i class="glyphicon glyphicon-export"></i>', click: function () {
												fnExportCusList();
											}
										}));
									} */
									return saBtn;
								},
								deleteButton: true,
								editButton: false
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
								return fnDelSatisfactionCustomer(args);
							}
						},
						onInit: function (args) {							
							oGrid = args.grid;
						}
					});
					
				});

                oValidator = $("#form_main").validate({
                    ignore: ''
                });

               				
				
				$('#ExhibitionNO').change(function () {
                    var sId = this.value;
					
                    if (sId) {
                        fnSetEpoDrop({
                            SN: sId,
                            CallBack: function (data) {
                                var oExhibition = data[0];
								$("#ResponsiblePerson").text(oExhibition.ResponsiblePerson);
								$("#ExhibitionDate").text(newDate(oExhibition.ExhibitionDateStart, 'date') + ' ~ ' + newDate(oExhibition.ExhibitionDateEnd, 'date'));
								$("#CreateDate").text(newDate(oExhibition.CreateDate, 'date'));
								 $('#ExhibitionDateStart').text(newDate(oExhibition.ExhibitionDateStart, 'date'));
								 $('#ExhibitionDateEnd').text(newDate(oExhibition.ExhibitionDateEnd, 'date'));
                                /* $('#ExportBillEName').val(oExhibition.Exhibitioname_EN);
                                if (oExhibition.ExhibitionDateStart) {
                                    $('#ExhibitionDateStart').val(newDate(oExhibition.ExhibitionDateStart, 'date'));
                                }
                                if (oExhibition.ExhibitionDateEnd) {
                                    $('#ExhibitionDateEnd').val(newDate(oExhibition.ExhibitionDateEnd, 'date'));
                                } */
                            }
                        });
                    }
                    else {
                        /* $('#ExportBillEName').val('');
                        $('#ExhibitionDateStart').val('');
                        $('#ExhibitionDateEnd').val(''); */
                    }
                });
				
				
				$('#btnImportFile').click(function () {
                    fnImportFile();
					//return false;
				});
				
				$('#btnCompareDB').click(function () {
					var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 150;
					$("#jsGrid").jsGrid({
						width: "100%",
						height: "auto",
						autoload: true,
						filtering: false,
						pageLoading: true,
						inserting: false,
						editing: false,
						sorting: false,
						paging: true,
						pageIndex: 1,
						pageSize: parent.top.SysSet.GridRecords || 10,
						confirmDeleting: true,
						deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
						pagePrevText: "<",
						pageNextText: ">",
						pageFirstText: "<<",
						pageLastText: ">>",
						rowClick: function (args) {
							/* if (navigator.userAgent.match(/mobile/i)) {
								goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
							} */
						},
						rowDoubleClick: function (args) {
							//parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
						},
						fields: [
							{
								name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
							},
							{
								name: "CompareDB", title: '比對資料庫', width: 80, align: "center"
							},
							{// ╠common.IsFormal⇒資料狀態╣
								name: "IsFormal",
								title: '比對資料庫',
								width: 80,
								align: 'center',
								itemTemplate: function (val, item) {
									var saAction = [];
									if (item.CompareDB == "N") {
										saAction.push($('<a/>', {
											html: i18next.t('common.CorrespondFormalCus'),
											class: 'link',
											click: function () {
												fnCorrespondFormalCus(item);
												return false;
											}
										}));
									}
									else {
										saAction.push($('<span />', { text: '已對應' }).css('color', 'green'));
									}
									return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
								}
							},
							{
								name: "CustomerName", title: '客戶名稱', width: 200, type: "text"
								/*itemTemplate: function (val, item) {
									if (item.CompareDB != "Y") {
										return this._createSelect = $("<select>", {
											class: 'w100p',
											html: sCustomersOptionsHtml,
											change: function () {
												var sOldValue = val,
													sNewValue = this.value;
												g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
													Id: item.Guid,
													OldOrderByValue: sOldValue,
													NewOrderByValue: sNewValue
												}, function (res) {
													if (res.RESULT) {
														oGrid[sKey].openPage(1);
													}
												});
											}
										}).val(val);
									}
									else {
										var saAction = [];
										saAction.push($('<span />', { text: item.CustomerName }));
										return $('<div>', { 'style': 'width:100%;text-align: left;' }).append(saAction);
									}
									
								} */
							},
							{
								name: "FillerName", title: '填寫人名稱', type: "text", width: 200
							},
							{
								name: "Phone", title: '連絡電話', type: "text", width: 200
							},
							{
								name: "Email", title: 'EMAIL', type: "text", width: 120
							},
							{
								name: "Feild01", title: '整體評分', width: 180, align: "center"
							},
							{
								name: "control1",
								width: 100,
								title: '問券',
								align: "center",
								itemTemplate: function (val, item) {
									var saBtn = [];
									saBtn.push($('<button />', {
										type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-file"></i>', click: function () {
											fnGetSatisfactionCaseData(item.SN);
										}
									}));
									return saBtn;
								},
								deleteButton: false,
								editButton: false
							},
							{
								type: "control",
								width: 100,
								headerTemplate: function () {
									var saBtn = [];
									/* if (sAction === 'Upd') {
										saBtn.push($('<button />', {
											type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-import"></i>', click: function () {
												fnImportCusList();
											}
										}));
										saBtn.push($('<button />', {
											type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Exp'), html: '<i class="glyphicon glyphicon-export"></i>', click: function () {
												fnExportCusList();
											}
										}));
									} */
									return saBtn;
								},
								deleteButton: true,
								editButton: false
							}
						],
						controller: {
                            loadData: function (args) {	
								return fnCompareDB();		
							},
							insertItem: function (args) {
							},
							updateItem: function (args) {
							},
							deleteItem: function (args) {
								return fnDelSatisfactionCustomer(args);							
							}
						},
						onInit: function (args) {							
							oGrid = args.grid;
						}
					});
								
					return false;
				});
				
				//oGrid.loadData();
            };

        init();		
    };
    

require(['base', 'select2', 'jsgrid', 'daterangepicker', 'convetlng', 'filer', 'ajaxfile', 'common_opm', 'util'], fnPageInit, 'daterangepicker');