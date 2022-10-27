'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('guid'),
	sDataSN = getUrlParam('SN'),
    sFlag = getUrlParam('Flag'),
    sGoTab = getUrlParam('GoTab'),
    sBillNOGO = getUrlParam('BillNO'),
    sCheckId = sDataId,
    sOrganizers = [],
    sSelectedOrganizers = [],
	saContactorList = [],
	saExhibitionContactorslist = [],
    intFormalCustomer = 0,
    oCalendar = null,
	intOpenLayer = 0,
    MaxOrganizerCount = $(".Organizer").length + 1,//organizer count
	fnPageInit = function () {
        var oGrid = null,
            oForm = $('#form_main'),
            oValidator = null,
			sTransportRequireOptionsHtml = '',
            sTransportOptionsHtml = '',
			sProcessingModeOptionsHtml = '',
			sPotentialOptionsHtml = '',
			sCoopTrasportCompanyHtml = '',
            oAddItem = {},
            oPrintMenu = {},
            oCurData = {},
			oGrid2 = null,
			oData1 = null,
			oData2 = null,
            saGridData = [],
            saCustomers = [],
            saBatchArr = [],
            saPort = [],
            saCurrency = [],
            saFeeClass = [],
            saAccountingCurrency = [],
			saRefNumber = [],
			saChooseContactorList = [],
            nowResponsiblePerson = '',
			sColumnWidth = "4",
			sCustomerCName = "",
			/**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                if (sDataId) {
                    //$('#litab3').show();
                    return g_api.ConnectLite('Customers_Qry', ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
								oData1 = res;
                                var oRes = res.DATA.rel;
								$("#CustomerCName").val(oRes.CustomerCName);
								sCustomerCName = oRes.CustomerCName;
								$("#CustomerEName").val(oRes.CustomerEName);
								$("#UniCode").val(oRes.UniCode);
								$("#Telephone").val(oRes.Telephone);
								$("[name='TransactionType'][value='" + oRes.TransactionType + "']").attr("checked", true);
								$("[name='IsBlackList'][value='" + oRes.IsBlackList + "']").click().click();
								$("[name='IsImporter'][value='" + oRes.IsImporter + "']").click().click();
								$("#BlackListReason").val(oRes.BlackListReason);
								
								if(oRes.CoopTrasportCompany){
									$("#CoopTrasportCompany").val(oRes.CoopTrasportCompany.split(',')).trigger('change');
								}
								
								if(oRes.IsAudit == "Y"){
									intFormalCustomer = 1;
									
									$("#btnCorrespondFormalCus").hide();
									$("#CustomerCName").attr('disabled', true);
									$("#CustomerEName").attr('disabled', true);
									$("#UniCode").attr('disabled', true);
									$("#Telephone").attr('disabled', true);
									//$("#CoopTrasportCompany").attr('disabled', true);
									$("[name=IsBlackList]").attr('disabled', true);
									$("#BlackListReason").attr('disabled', true);
								}
                            }
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
        fnGetExhibitionContactorslist = function () {
            if (sDataId) {
                return g_api.ConnectLite(sProgramId, 'GetExhibitionContactorslist', {
					ExhibitionSN : sDataSN,
                    CustomerId: sDataId
                },
				function (res) {
					if (res.RESULT) {
						saExhibitionContactorslist = []
						saExhibitionContactorslist = res.DATA.rel;
						saExhibitionContactorslist = Enumerable.From(saExhibitionContactorslist).Where(function (e) { return e.Mark == ''; }).ToArray();
						$('#Contactor-' + sDataSN).html();
						$('#Contactor-' + sDataSN).html(createOptions(saExhibitionContactorslist, 'Guid', 'ContactorName')).select2();
					}
				});
            }
            else {
                return $.Deferred().resolve().promise();
            }
        },
            /**
             * 獲取資料
             * @return {Object} ajax物件
             */
            fnGetCalloutData = function () {
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetCalloutData',
                        {
							ExhibitionSN : sDataSN,
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
								oData2 = res;
								var oRes = res.DATA.rel;
                            }
                        });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
			fnChooseContactors = function(){
				saContactorList = [];
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "選擇聯絡人", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.01,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_ChooseContactors', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<div class="pop-box col-sm-10 col-sm-offset-1">\
								<div id="jsGridChooseContactors"></div>\
							 </div>',
                    success: function (layero, index) {
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
							deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
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
													saContactorList.push(item.guid);
												}
												else {
													var saNewList2 = [];
													$.each(saContactorList, function (idx, data) {
														let i = 0;
														if (item.guid !== data.guid) {
															data.RowIndex = i++;
															saNewList2.push(data.guid);
														}
													});
													saContactorList = saNewList2;
												}
											}, disabled: function(){
												let blRepeat = false;
 												$.each(saExhibitionContactorslist, function (idx, data) {
													if(item.guid == data.Guid){
														blRepeat = true;
														return;
													}
												});
												return blRepeat;
											}
										});
									}
								},
								{
									name: "ContactorName", title: 'common.Contactor', width: 25, align: "center"
								},
								{
									name: "JobTitle", title: 'common.JobTitle', width: 25, align: "center"
								},
								{
									name: "Telephone1", title: 'common.Telephone', width: 30
								},
								{
									name: "Ext1", title: 'common.EXT', width: 10
								},
								{
									name: "Email1", title: 'common.Email', width: 40
								}
							],
							controller: {
								loadData: function (args) {
									return fnGetContactorsList(sDataId);
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
						g_api.ConnectLite(sProgramId, 'ChooseContactor',
						{
							SN: sDataSN,
							guid: sDataId,
							contactor: saContactorList
						},
						function (res) {
							if (res.RESULT) {
								showMsg(i18next.t("message.Save_Success"), 'success');
							} else {
								showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
							}
						},
						function (res) {
							showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
						})
						
						layer.close(index);
                    },
					end: function () {
						oGrid2.loadData();
					}
                });
			},
			fnGetContactorsList = function (guid) {
                return g_api.ConnectLite('Contactors_Qry', 'QueryByCustomer', {
					CustomerId: guid
				});
            },
			fnCreateContactor = function(){
				layer.open({
					type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
					title: i18next.t('common.InsertContactor'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
					area: ['70%', '90%'],//寬度
					shade: 0.75,//遮罩
					maxmin: true, //开启最大化最小化按钮
					id: 'layer_InsertContactor', //设定一个id，防止重复弹出
					anim: 0,//彈出動畫
					btnAlign: 'c',//按鈕位置
					content: '../Crm/Contactors_Upd.html?Action=Add&Flag=Pop&CustomerId=' + sDataId + '&ExhibitionNO=' + sDataSN,
					success: function (layero, index) {
						var iframe = layero.find('iframe').contents();
						iframe.find('#hiddenIndex').val(index);
					},
					end: function () {
						oGrid2.loadData();
					}
				});
			},
			fnCorrespondFormalCus = function () {
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
                        transLang(layero);
                    },
                    yes: function (index, layero) {
                        let sCustomerId = $('#CustomerId').val();
                        if (!sCustomerId) {
                            showMsg(i18next.t('message.SelectFormalCus'));//╠message.SelectFormalCus⇒請選擇對應的客戶╣
                            return false;
                        }
						
						var combinedata = {};
						combinedata.Type = "2";
						combinedata.guid1 = sDataId;
						combinedata.guid2 = sCustomerId;
						
						g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
						, function (res) {
							if (res.DATA.rel) {
								sDataId = sCustomerId;
								showMsg(i18next.t("message.Correspond_Success"), 'success'); //╠message.Correspond_Success⇒對應成功╣
								layer.close(index);
							}
							else {
								showMsg(i18next.t("message.Correspond_Failed"), 'error');//╠message.Correspond_Failed⇒對應失敗╣
							}
						}
						, function () {
							showMsg(i18next.t("message.Correspond_Failed"), 'error');//╠message.Correspond_Failed⇒對應失敗╣
						});
                    },
					end: function() {
						init();
					}
                });
            },
			/**
             * 綁定帳單
             */
            fnBindBillLists = function () {
                var oBillsBox = $('#accordion');
                oBillsBox.html('');
                //$('#tab3 .amountsum').val(0);

                if (oCurData.length > 0) {//實際帳單
					//oCurData = Enumerable.From(oCurData).OrderBy("x=>x.CreateDate").ToArray();
                 var sContent = '<style>.select2-container--open { z-index: 1000000001;}.jsgrid-header-cell{padding:0 0;}</style>\
                        <div class="row popsrow" style="margin-top:30px;">\
                                <label class="col-sm-4 control-label wright" for="input-Default"><span data-i18n="提醒日期">提醒日期</span>：</label>\
                                <div class="col-sm-6">\
                                    <input class="form-control w100p date-picker" type="text" id="RemindDate" maxlength="10" required>\
                                </div>\
                        </div>\
                        <div class="row popsrow" style="margin-top:30px;">\
                                <label class="col-sm-4 control-label wright " for="input-Default"><span data-i18n="提醒時間">提醒時間</span>：</label>\
                                <div class="col-sm-6">\
                                    <input class="form-control" type="text" id="RemindTime" value = "09:00">\
                                </div>\
                        </div>';
                    $.each(oCurData, function (idx, data) {
                        if ($('.bill-box-' + data.ExhibitionNO).length === 0) {
							data.CreateDate = newDate(data.CreateDate, false, true);
							var sHtml = $("#temp_ExhibitionCustomerbox").render([data]);
							oBillsBox.append(sHtml);
                            
							data.CalloutLog = data.CalloutLog.replace(/\r?\n/g, '<br>');
							var jsonCalloutLogData = JSON.parse(data.CalloutLog);
							var arrCalloutLogData = Enumerable.From(jsonCalloutLogData).ToArray();
							
							$.each(arrCalloutLogData, function (LogIdx, LogData) {
								LogData.ExhibitionNO = data.ExhibitionNO;
								LogData.Index = LogIdx;
								LogData.Memo = LogData.Memo.replace(/<br>/g, '\n');
								var sLogHtml = $("#temp_Memobox").render([LogData]);
								$('#accordion-' + data.ExhibitionNO).append(sLogHtml);
							})
							$('#TransportRequire-' + data.ExhibitionNO).html(sTransportRequireOptionsHtml).val(data.TransportRequire);
							$('#TransportationMode-' + data.ExhibitionNO).html(sTransportOptionsHtml).val(data.TransportationMode);
							$('#ProcessingMode-' + data.ExhibitionNO).html(sProcessingModeOptionsHtml).val(data.ProcessingMode);
							$('#Potential-' + data.ExhibitionNO).html(sPotentialOptionsHtml).val(data.Potential);
							
							if(data.CoopTrasportCompany){
								$('#CoopTrasportCompany-' + data.ExhibitionNO).html(sCoopTrasportCompanyHtml).val(data.CoopTrasportCompany.split(',')).select2();
							} else {
								$('#CoopTrasportCompany-' + data.ExhibitionNO).html(sCoopTrasportCompanyHtml).select2();
							}
							
							if(idx == 0){
								$("#collapse"+ data.ExhibitionNO).addClass("in");
								
								$('#btnCreateLog-' + data.ExhibitionNO).on('click', function () {
									fnCreateCalloutLog(data.ExhibitionNO);
                                });
                                
							} else {
								$('#TransportRequire-' + data.ExhibitionNO).attr('disabled', true);
								$('#TransportationMode-' + data.ExhibitionNO).attr('disabled', true);
								$('#ProcessingMode-' + data.ExhibitionNO).attr('disabled', true);
								$('#VolumeForecasting-' + data.ExhibitionNO).attr('disabled', true);
								$('#Potential-' + data.ExhibitionNO).attr('disabled', true);
								$('#BoothNumber-' + data.ExhibitionNO).attr('disabled', true);
								$('#NumberOfBooths-' + data.ExhibitionNO).attr('disabled', true);
								$('#CoopTrasportCompany-' + data.ExhibitionNO).attr('disabled', true);
								$('#Memo-' + data.ExhibitionNO).attr('disabled', true);
								$("#divLog-" + data.ExhibitionNO).hide();
							}
							
							
                            $('#btnCallOutRemind-' + data.ExhibitionNO).on('click', function () {
                                layer.open({
                                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                                    title: "回撥提醒",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                                    area: '580px;',//寬度
                                    shade: 0.75,//遮罩
                                    //maxmin: true, //开启最大化最小化按钮
                                    id: 'layer_btnCallOutRemind', //设定一个id，防止重复弹出
                                    offset: '10px',//右下角弹出
                                    anim: 0,//彈出動畫
                                    btn: [i18next.t('common.Toolbar_Save'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                                    btnAlign: ['c'],//按鈕位置
                                    content: sContent,
                                    success: function (layero, index) {
                                        //初始化日期格式
                                        $("#RemindDate").datepicker({
                                        });
                                        $('#RemindTime').timepicker({
                                            stepMinute: 30,
                                        });
                                        var dNowDate = new Date();
                                        dNowDate = dNowDate.setDate(dNowDate.getDate() + 1);
                                        dNowDate = new Date(dNowDate);
                                        var sNowDate = dNowDate.toISOString().slice(0, 10);
                                        $("#RemindDate").val(sNowDate);
                                    },
                                    yes: function (index, layero) {
                                        var sRemindDate = $("#RemindDate").val();
                                        var sRemidTime = $("#RemindTime").val();
                                        
                                        var OrgID = parent.OrgID;
                                        var UserID = parent.UserUD;
                                        var CalType = "03";
                                        var Title = data.ExhibitionName + "_回撥提醒";
                                        var dStartDate = new Date(sRemindDate);
                                        var dEndDate = dStartDate.getDate() + 1;
                                        var EndDate = new Date();
                                        var Color = parent.UserInfo.CalColor;
                                        var Importment = "M";
                                        var AllDay = '0';
                                        var Description = "";
                                        var OpenMent = "P";
                                        
                                        //防止使用者未填 提醒日期、提醒時間
                                        if (sRemindDate == "" || sRemindDate == null) {
                                            alert("請填入提醒日期");
                                            return;
                                        } 
                                        if (sRemidTime == "" || sRemidTime == null) {
                                            var saTime = ["09", "00"];
                                        } else {
                                            var saTime = sRemidTime.split(":");
                                        }
                                        var sStartTimeStamp = dStartDate.setHours(saTime[0], saTime[1]);
                                        EndDate = EndDate.setDate(dEndDate);
                                        dStartDate = new Date(sStartTimeStamp);
                                        EndDate = new Date(EndDate);
                                        EndDate.setHours("00", "00");

                                        //date format => yyyy/mm/dd hh:mm
                                        var sStartDate = newDate(dStartDate);
                                        var sEndDate = newDate(EndDate);

                                        data = {
                                            OrgID: OrgID,
                                            UserID: UserID,
                                            CalType: CalType,
                                            Title: Title,
                                            AllDay: '1',
                                            StartDate: sStartDate,
                                            EndDate: sEndDate,
                                            Color: Color,
                                            Importment: Importment,
                                            AllDay: AllDay,
                                            Description: Description,
                                            OpenMent: OpenMent,
                                            GroupMembers: ""
                                        }
                                        g_api.ConnectLite("Calendar", ComFn.GetAdd, data,
                                            function (res) {
                                                if (res.RESULT) {
                                                    var sNo = res.DATA.rel;
                                                    showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                                                    if (parent.Outklook) {
                                                        outlookAPI(outlook.Calendar_Add, {
                                                            NO: sNo,
                                                            ResponseRequested: true
                                                        });
                                                    }
                                                    layer.close(index);
                                                } else {
                                                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                                                }
                                            }
                                        );
                                    },
                                    end: function () {
                                    }
                                });		
                            });
							
                            $('.bills-box').show();
                        }
                    });
                }
            },
			/**
             * 設定運輸需求下拉選單
             */
            setTransportRequireDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'TransportRequire'
                }, function (res) {
                    if (res.RESULT) {
                        if (res.DATA.rel.length > 0) {
							sTransportRequireOptionsHtml = createOptions(res.DATA.rel, 'id', 'text');
                        }
                        else {
							sTransportRequireOptionsHtml = createOptions([]);
                        }
                    }
                });
            },
			/**
             * 設定運輸方式下拉選單
             */
            setTransportDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'Transport'
                }, function (res) {
                    if (res.RESULT) {
                        if (res.DATA.rel.length > 0) {
							sTransportOptionsHtml = createOptions(res.DATA.rel, 'id', 'text');
                        }
                        else {
							sTransportOptionsHtml = createOptions([]);
                        }
                    }
                });
            },
			/**
			 * 抓參加展覽已成交列表資料
			 * @return {Object} ajax物件
			 */
			fnGetDealExhibitionlist = function () {
				if (sDataId) {
					g_api.ConnectLite(sProgramId, 'GetDealExhibitionlist', {
						guid: sDataId
					},
					function (res) {
						if (res.RESULT) {
							$("#divDealExhibitionlist").html('');
							$.each(res.DATA.rel, function (idx, data) {
								$("#divDealExhibitionlist").append('<div class="col-sm-' + sColumnWidth + '" id="divDeal-' + data.SN + '"><a>' + data.ExhibitioShotName_TW + '</a></div>');
								
								let iTips = 0;
								let sTipsHtml = '<div class="layui-layer-btn-c">' + '查詢單號' + '</div>';
								saRefNumber = data.RefNumber.split(',');
								let oOption = {
									btnAlign: 'c',
									time: 600000 //一個小時（如果不配置，默认是3秒）
								};
								
								if (saRefNumber.length > 0) {
									sTipsHtml += '<ul class="bill-status">';
									$.each(saRefNumber, function (idx2, data2) {
										let saData = data2.split(';');
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
								}
								else {
									sTipsHtml = '<div></div>';
								}
								
								$('#divDeal-' + data.SN).on({
									click: function () {
										oOption.btn = [i18next.t("common.Close")];// ╠common.Close⇒關閉╣
										intOpenLayer = layer.msg(sTipsHtml, oOption);
									},
									mouseenter: function (event) {
										delete oOption.btn;
										if($('#layui-layer' + intOpenLayer).length == 0){
											iTips = layer.msg(sTipsHtml, {
												oOption,
												offset: ['40%', '30%']
											});
										}
									},
									mouseleave: function (event) {
										layer.close(iTips);
									}
								});
							})
						}
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
			/**
			 * 新增Callout紀錄
			 * @return {Object} ajax物件
			 */
			fnCreateCalloutLog = function (_sExhibitionNO) {
				if($("#Contactor-"+ _sExhibitionNO).val() == "" || $("#Contactor-"+ _sExhibitionNO).val() == null){
					showMsg('請選擇聯絡人', 'error');
					return false;
				} else if($("#Record-"+ _sExhibitionNO).val() == ""){
					showMsg('紀錄欄位不得為空', 'error');
					return false;
				} else {
					g_api.ConnectLite(sProgramId, 'CreateCalloutLog', {
						ExhibitionNO: _sExhibitionNO,
						CustomerId: sDataId,
						Contactor: $("#Contactor-"+ _sExhibitionNO).val(),
						Memo: $("#Record-"+ _sExhibitionNO).val()
					}, function (res) {
						if (res.RESULT) {
							showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
							
							$('#accordion-' + _sExhibitionNO).html('');
							$.each(res.DATA.rel, function (LogIdx, LogData) {
								LogData.ExhibitionNO = _sExhibitionNO;
								LogData.Index = LogIdx;
								var sLogHtml = $("#temp_Memobox").render([LogData]);
								$('#accordion-' + _sExhibitionNO).append(sLogHtml);
								$("#Contactor-"+ _sExhibitionNO).val("").select2();
								$("#Record-"+ _sExhibitionNO).val("");
							})
						}
						else {
							showMsg(res.MSG, 'error');
						}
					}, function () {
						showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
					});
				}
			},
			fnUpd = function () {
				let oUpdData = {};
				let sCoopTrasportCompany1 = "";
				let sCoopTrasportCompany2 = "";
				
				oUpdData.ExhibitionNO = sDataSN,
				oUpdData.CustomerId =  sDataId,
				
				oUpdData.FormalCustomer = intFormalCustomer;
				
				oUpdData.TransactionType = $("[name=TransactionType]:checked").val();
				oUpdData.CustomerCName = $("#CustomerCName").val();
				oUpdData.CustomerEName = $("#CustomerEName").val();
				oUpdData.UniCode = $("#UniCode").val();
				oUpdData.Telephone = $("#Telephone").val();
				oUpdData.IsBlackList = $("[name=IsBlackList]:checked").val();
				oUpdData.BlackListReason = $("#BlackListReason").val();
				
				/* if($("#CoopTrasportCompany").val() != "" && $("#CoopTrasportCompany").val() != null){
					$.each($("#CoopTrasportCompany").val(), function (idx, item) {
						sCoopTrasportCompany1 = sCoopTrasportCompany1 + item + ",";
					});
				}
				oUpdData.CoopTrasportCompany1 = sCoopTrasportCompany1; */
				
				oUpdData.TransportRequire = $("#TransportRequire-" + sDataSN).val();
				oUpdData.TransportationMode = $("#TransportationMode-" + sDataSN).val();
				oUpdData.ProcessingMode = $("#ProcessingMode-" + sDataSN).val();
				oUpdData.VolumeForecasting = $("#VolumeForecasting-" + sDataSN).val();
				oUpdData.Potential = $("#Potential-" + sDataSN).val();
				oUpdData.BoothNumber = $("#BoothNumber-" + sDataSN).val();
				oUpdData.NumberOfBooths = $("#NumberOfBooths-" + sDataSN).val();
				oUpdData.Memo = $("#Memo-" + sDataSN).val();
				
				if($("#CoopTrasportCompany-" + sDataSN).val() != "" && $("#CoopTrasportCompany-" + sDataSN).val() != null){
					$.each($("#CoopTrasportCompany-" + sDataSN).val(), function (idx, item) {
						sCoopTrasportCompany2 = sCoopTrasportCompany2 + item + ",";
					});
				}
				oUpdData.CoopTrasportCompany = sCoopTrasportCompany2;
				oUpdData.IsImporter = $("[name=IsImporter]:checked").val();
				
				g_api.ConnectLite(sProgramId, 'UpdateCalloutData', oUpdData,
				function (res) {
					if (res.RESULT) {
						showMsg(i18next.t("message.Modify_Success"), 'success');
						
						if (bLeavePage) {
                            setTimeout(function () {
                                fnLocalToLeave();
                            }, 1000);
                        } else {
							init();
						}
					} else {
						showMsg(i18next.t("message.Modify_Failed")+ '<br>' + res.MSG, 'error');
					}
				},
				function (res) {
					showMsg(i18next.t("message.Modify_Failed"), 'error');
				})
			},
			/**
			 * 抓參加展覽未成交列表資料
			 * @return {Object} ajax物件
			 */
			fnGetUnDealExhibitionlist = function () {
				if (sDataId) {
					g_api.ConnectLite(sProgramId, 'GetUnDealExhibitionlist', {
						guid: sDataId
					},
					function (res) {
						if (res.RESULT) {
							$("#divUnDealExhibitionlist").html('');
							$.each(res.DATA.rel, function (idx, data) {
								$("#divUnDealExhibitionlist").append('<div class="col-sm-' + sColumnWidth + '" id="divUnDeal-' + data.SN + '"><a>' + data.ExhibitioShotName_TW + '</a></div>');
								
								$('#divUnDeal-' + data.SN).on({
									click: function () {
										parent.openPageTab('Exhibition_Upd',"?Action=Upd&SN=" + data.SN );
									}
								});
								
							})
						}
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
			/**
			 * 抓未來展覽列表資料
			 * @return {Object} ajax物件
			 */
			fnGetFutureExhibitionlist = function () {
				if (sDataId) {
					g_api.ConnectLite(sProgramId, 'GetFutureExhibitionlist', {
						guid: sDataId
					},
					function (res) {
						if (res.RESULT) {
							$("#divBusinessOpportunity").html('');
							$.each(res.DATA.rel, function (idx, data) {
								$("#divBusinessOpportunity").append('<div class="col-sm-' + sColumnWidth + '" id="divFuture-' + data.SN + '"><a>' + data.ExhibitioShotName_TW + '</a></div>');
								
								$('#divFuture-' + data.SN).on({
									click: function () {
										parent.openPageTab('Exhibition_Upd',"?Action=Upd&SN=" + data.SN );
									}
								});
								
							})
						}
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
			/**
			 * 抓客訴列表資料
			 * @return {Object} ajax物件
			 */
			fnGetComplaintlist = function () {
				if (sDataId) {
					g_api.ConnectLite('Customers_Upd', 'GetComplaintlist', {
						guid: sDataId
					},
					function (res) {
						if (res.RESULT) {
							$("#divComplaint").html('');
							$.each(res.DATA.rel, function (idx, data) {
								$("#divComplaint").append('<div class="col-sm-' + sColumnWidth + '" id="divComplaint-' + data.ComplaintNumber + '"><a>' + data.ComplaintTitle + '</a></div>');
								
								$('#divComplaint-' + data.ComplaintNumber).on({
									click: function () {
										//parent.openPageTab('Complaint_Upd',"?Action=View&Guid=" + data.Guid );
										if ('A,C'.indexOf(data.DataType) > -1 && data.CreateUser === parent.UserID) {
											parent.openPageTab('Complaint_Upd', '?Action=Upd&Guid=' + data.Guid);
										}
										else {
											parent.openPageTab('Complaint_View', '?Action=Upd&Guid=' + data.Guid);
										}
									}
								});
								
							})
						}
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
			/**
			 * 抓滿意度列表資料
			 * @return {Object} ajax物件
			 */
			fnGetSatisfactionCaselist = function () {
				if (sDataId) {
					g_api.ConnectLite('Customers_Upd', 'GetSatisfactionCaselist', {
						guid: sDataId
					},
					function (res) {
						if (res.RESULT) {
							$("#divSatisfactionCase").html('');
							$.each(res.DATA.rel, function (idx, data) {
								$("#divSatisfactionCase").append('<div class="col-sm-' + sColumnWidth + '" id="divSatisfaction-' + data.SN + '"><a>' + data.ExhibitioShotName_TW + '</a></div>');
								
								$('#divSatisfaction-' + data.SN).on({
									click: function () {
										//parent.openPageTab('SatisfactionCase_Upd',"?Action=Upd&SN=" + data.SN );
										fnGetSatisfactionCaseData(data.CustomerSN);
									}
								});
								
							})
						}
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
			fnGetSatisfactionCaseData = function (_SN) {
				return g_api.ConnectLite('SatisfactionCase_Upd', 'GetSatisfactionCaseData', {
					SN: _SN,                      						
                }, function (res) {
					let oResSA = res.DATA.rel;
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
										<input type="text" maxlength="50" id="CustomerName" name="CustomerName" class="form-control w100p" placeholderid="" value="' + oResSA.CustomerName + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="FillerName" name="FillerName" class="form-control w100p" placeholderid="" value="' + oResSA.FillerName + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人電子郵件</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Email" name="Email" class="form-control w100p" placeholderid="" value="' + oResSA.Email + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">填寫人聯絡電話</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<input type="text" maxlength="50" id="Phone" name="Phone" class="form-control w100p" placeholderid="" value="' + oResSA.Phone + '" disabled>\
									</div>\
								</div>\
								<div class="pop-box row w100p">\
									<label class="col-sm-3 control-label" for="input-Default">\
										<span data-i18n="">備註</span><span>：</span>\
									</label>\
									<div class="col-sm-8">\
										<textarea name="Memo" id="Memo" class="form-control" rows="3" disabled>' + oResSA.Memo + '</textarea>\
									</div>\
								</div>\<hr>\
								<div>\
									<table class="w80p text-left" style="border:1px #cccccc solid;margin-Left:10%;font-size:14px"><thead></thead>\
									<tbody>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">奕達提供整體服務品質的滿意度：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild01">' + oResSA.Feild01 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">奕達提供的價格是否合理：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild02">' + oResSA.Feild02 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">展品送達時間是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild03">' + oResSA.Feild03 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">現場人員的專業技能與服務態度是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild04">' + oResSA.Feild04 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">承辦同仁的配合度及服務態度是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild05">' + oResSA.Feild05 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">「貨況線上查詢系統」是否滿意：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild06">' + oResSA.Feild06 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">為何選擇奕達：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild07">' + oResSA.Feild07 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">貴公司年度平均參與海外展會活動次數：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild08">' + oResSA.Feild08 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">您是否會推薦奕達給合作夥伴：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild9">' + oResSA.Feild09 + '</div></td>\
									</tr>\
									<tr>\
									<td class="col-sm-7" style="border:1px #cccccc solid;">其他建議：</td>\
									<td class="col-sm-5" style="border:1px #cccccc solid;"><div id="Feild10">' + oResSA.Feild10 + '</div></td>\
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
            },
			/**
			 * 新增潛在商機
			 * @return {Object} ajax物件
			 */
			fnAddBusinessOpportunity = function (data) {
                data = packParams(data);
                g_api.ConnectLite('BusinessOpportunity_Qry', 'Insert', data,
                    function (res) {
                        if (res.RESULT == '1') {
                            showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                        } else {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        }

                    },
                    function (res) {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                )
            },
			fnLocalToLeave = function () {
                parent.top.msgs.server.removeEditPrg(sProgramId).done(function() {
					var PrevLi = parent.top.tabs.find(".ui-tabs-active").prev().find('a'),
					LiId = parent.top.tabs.find(".ui-tabs-active").remove().attr("aria-controls");
					PrevLi.click();
				})
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
						fnUpd();
                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":
					
                        break;
                    case "Toolbar_Leave":
						//當被lock住，不儲存任何資料，直接離開。
						if (parent.bLockDataForm0430 !== undefined){
							fnLocalToLeave();
						} else if (bRequestStorage) {
							layer.confirm(i18next.t('message.HasDataTosave'), {//╠message.HasDataTosave⇒尚有資料未儲存，是否要儲存？╣
								icon: 3,
								title: i18next.t('common.Tips'),// ╠message.Tips⇒提示╣
								btn: [i18next.t('common.Yes'), i18next.t('common.No')] // ╠message.Yes⇒是╣ ╠common.No⇒否╣
							}, function (index) {
								layer.close(index);
								bLeavePage = true;
								fnUpd();
							}, function () {
								fnLocalToLeave();
							});
							return false;
						} else {
							fnLocalToLeave();
							return false;
						}
                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Imp":

                        break;
                    case "Toolbar_Exp":

                        break;
                    case "Toolbar_Void":

                        break;
                    case "Toolbar_OpenVoid":

                        break;
                    case "Toolbar_Del":

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
					fields: [
						{
							name: "ContactorName", title: '聯絡人名稱', width: 150, align: "center",
							itemTemplate: function (val, item) {
								switch(item.Mark){
									case "N":
										return $('<span />', { text: val }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: val }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: val }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "JobTitle", title: '職位', width: 80, align: "center",
							itemTemplate: function (val, item) {
								switch(item.Mark){
									case "N":
										return $('<span />', { text: val }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: val }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: val }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "Telephone1", title: '電話1', width: 150,align: "center",
							itemTemplate: function (val, item) {
								switch(item.Mark){
									case "N":
										return $('<span />', { text: val }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: val }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: val }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "Ext1", title: '分機1', width:50,align: "center",
							itemTemplate: function (val, item) {
								switch(item.Mark){
									case "N":
										return $('<span />', { text: val }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: val }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: val }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "Email1", title: 'Email', width: 200,align: "center",
							itemTemplate: function (val, item) {
								switch(item.Mark){
									case "N":
										return $('<span />', { text: val }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: val }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: val }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "SourceType", title: '來源', width: 50,align: "center",
							itemTemplate: function (val, item) {
								let sSourceType = "";
								switch(val){
									case "2":
										sSourceType = "匯入檔案";
										break;
									default:
										sSourceType = "資料庫";
										break;
								}
								
								switch(item.Mark){
									case "N":
										return $('<span />', { text: sSourceType }).css('color', 'gray')
										break;
									case "Y":
										return $('<span />', { text: sSourceType }).css('color', 'red')
										break;
									default:
										return $('<span />', { text: sSourceType }).css('color', 'black')
										break;
								}
							}
						},
						{
							name: "IsMain", title: '主要聯絡人', width: 50, align: "center",
							itemTemplate: function (value, item) {
								return $("<input>", {
									type: 'checkbox',
									click: function (e) {
										e.stopPropagation();
										if (this.checked) {
											/* $("#jsGrid").find('[type=checkbox]').each(function () {
												this.checked = false;
											});
											
											this.checked = true;
											 */
											if(value != "Y"){
												layer.confirm("確定將 " + item.ContactorName + " 設為主要聯絡人？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
													g_api.ConnectLite(sProgramId, 'SetContactorIsMain', {
														ExhibitionNO: sDataSN,
														CustomerId: sDataId,
														ContactorId: item.Guid
													}
													, function (res) {
														if (res.DATA.rel) {
															oGrid2.loadData();
															layer.close(index);
														}
														else {
															showMsg("更新失敗", 'error');
														}
													}
													, function () {
														showMsg("更新失敗", 'error');
													});
													oGrid2.loadData();
													layer.close(index);
												});
											}
											
											return false;
										} else {
											if(value == "Y"){
												return false;
											}
										}
									}, checked: function(){
										if(value == "Y"){
											return true;
										} else {
											return false;
										}
									}
								});
							}
						},
						{
							name: "Create", title: '', width: 50, align: "center",
							itemTemplate: function (value, item) {
								if(item.Mark === "N" || item.Mark === "Y"){
									return $("<button>", {
										class:"btn-custom blue", 
										text: (item.Mark === "N" ? "建立" : "加入"),
										click: function (e) {
											layer.confirm("確定" + (item.Mark === "N" ? "建立" : "加入") + "聯絡人？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
												g_api.ConnectLite(sProgramId, 'CreateContactor', {
													ExhibitionNO: sDataSN,
													CustomerId: sDataId,
													ContactorId: item.Guid
												}
												, function (res) {
													if (res.DATA.rel) {
														showMsg((item.Mark === "N" ? "建立" : "加入") + "成功", 'success');
														oGrid2.loadData();
														layer.close(index);
													}
													else {
														showMsg((item.Mark === "N" ? "建立" : "加入") + "失敗", 'error');
													}
												}
												, function () {
													showMsg((item.Mark === "N" ? "建立" : "加入") + "失敗", 'error');
												});
												oGrid2.loadData();
												layer.close(index);
											});
											return false;
										}
									});
								}
							}
						},
						{
							name: "Edit", title: '', width: 50, align: "center",
							itemTemplate: function (value, item) {
								if(item.Mark != "N"){
									return $("<button>", {
										class:"btn-custom blue", 
										text:"編輯",
										click: function (e) {
											layer.open({
													type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
													title: i18next.t('common.InsertContactor'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
													area: ['70%', '90%'],//寬度
													shade: 0.75,//遮罩
													maxmin: true, //开启最大化最小化按钮
													id: 'layer_InsertContactor', //设定一个id，防止重复弹出
													anim: 0,//彈出動畫
													btnAlign: 'c',//按鈕位置
													content: '../Crm/Contactors_Upd.html?Action=Upd&Flag=Pop&guid=' + item.Guid,
													success: function (layero, index) {
														var iframe = layero.find('iframe').contents();
														iframe.find('#hiddenIndex').val(index);
													},
													end: function () {
														oGrid2.loadData();
													}
												});
											return false;
										}
									});
								}
								
							}
						},
						{
							name: "Delete", title: '', width: 50, align: "center",
							itemTemplate: function (value, item) {
								if(item.Mark != "Y"){
									return $("<button>", {
										class:"btn-custom blue", 
										text:"移除",
										click: function (e) {
											layer.confirm("確定將聯絡人從名單移除？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
												g_api.ConnectLite(sProgramId, 'RemoveContactor', {
													ExhibitionNO: sDataSN,
													CustomerId: sDataId,
													ContactorId: item.Guid
												}
												, function (res) {
													if (res.DATA.rel) {
														showMsg("移除成功", 'success');
														oGrid2.loadData();
														layer.close(index);
													}
													else {
														showMsg("移除失敗", 'error');
													}
												}
												, function () {
													showMsg("移除失敗", 'error');
												});
												oGrid2.loadData();
												layer.close(index);
											});
											return false;
										}
									});
								}
							}
						}
					],
					controller: {
						loadData: function (args) {
							return fnGetExhibitionContactorslist();
						},
						insertItem: function (args) {
						},
						updateItem: function (args) {
						},
						deleteItem: function (args) {
						}
					},
					onInit: function (args) {
						oGrid2= args.grid;
					}
				});

				commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true
                });
				
                //加載報關類別,加載成本頁簽
                $.whenArray([
					fnGet(),
					fnGetCalloutData(),
					setTransportRequireDrop(),
					setTransportDrop(),
					fnGetFutureExhibitionlist(),
					fnGetDealExhibitionlist(),
					fnGetUnDealExhibitionlist(),
					fnGetComplaintlist(),
					fnGetSatisfactionCaselist(),
					fnSetArgDrop([
					{
						OrgID: 'TE',
						ArgClassID: 'BlackListReason',
						Select: $('#BlackListReason')
					},
					{
						OrgID: 'TE',
						ArgClassID: 'TrasportCompany',
						CallBack: function (data) {
							sCoopTrasportCompanyHtml = createOptions(data, 'id', 'text');
							$('#CoopTrasportCompany').html(sCoopTrasportCompanyHtml).select2();
						}
					},
					{
						OrgID: 'TE',
						ArgClassID: 'ProcessingMode',
						CallBack: function (data) {
							sProcessingModeOptionsHtml = createOptions(data, 'id', 'text');
						}
					},
					{
						OrgID: 'TE',
						ArgClassID: 'Potential',
						CallBack: function (data) {
							sPotentialOptionsHtml = createOptions(data, 'id', 'text');
						}
					}
					])
                ])
				.done(function (res) {
					if (res && oData1) {
						var oRes = oData2.DATA.rel;
						var oRes1 = oData1.DATA.rel;
						fnSetArgDrop([
							{
								ArgClassID: 'TranType',
								CallBack: function (data) {
									$('#transactiontype').html(createRadios(data, 'id', 'text', 'TransactionType'));
									$('[name=TransactionType][value="' + oRes1.TransactionType + '"]').click();
									uniformInit($('#transactiontype'));
									
									if(intFormalCustomer == 1){
										$("[name=TransactionType]").attr('disabled', true);
									}
								}
							}
						])
						oCurData = oRes;
						
						fnBindBillLists();
							
						oGrid2.loadData();
					}
				});
				
				$("[name='IsBlackList']").on('change', function () {
					if($("[name='IsBlackList']:checked").val() == "Y"){
						$("#BlackListReason" ).attr('disabled', false);
					} else {
						$("#BlackListReason").val("");
						$("#BlackListReason" ).attr('disabled', true);
					}
				});
				
				$('#btnSeeMore').on('click', function () {
					parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + sDataId);
				});
				
				$('#btnCorrespondFormalCus').on('click', function () {
					fnCorrespondFormalCus();
				});
				
				$('#btnChooseContactors').on('click', function () {
					fnChooseContactors();
				});
				
				$('#btnCreateContactor').on('click', function () {
					fnCreateContactor();
				});
				
				$('#btnCreateBusinessOpportunity').on('click', function () {
					layer.open({
						type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
						title: "建立潛在商機",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
						area: ['60%', '90%'],//寬度
						shade: 0.75,//遮罩
						//maxmin: true, //开启最大化最小化按钮
						id: 'layer_CombineContactor', //设定一个id，防止重复弹出
						offset: '10px',//右下角弹出
						anim: 0,//彈出動畫
						btn: ['儲存', i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
						btnAlign: 'c',//按鈕位置
						content: '../CRM/BusinessOpportunity_Upd.html',
						success: function (layero, index) {
                            var iframe = layero.find('iframe').contents();
                            iframe.find("#CustomerName").val(sCustomerCName);
							g_api.ConnectLite(Service.com, ComFn.GetArguments, {
								OrgID: 'TE',
								ArgClassID: 'Area',
								LevelOfArgument: 1
							}, function (res) {
								if (res.RESULT) {
									let saState = res.DATA.rel;
									if (res.DATA.rel.length > 0) {
										iframe.find("#State").html(createOptions(res.DATA.rel, 'id', 'text', true));
									}
								}
							});
							
							g_api.ConnectLite(Service.com, ComFn.GetArguments, {
								OrgID: 'TE',
								ArgClassID: 'ExhibClass',
								LevelOfArgument: 0
							}, function (res) {
								if (res.RESULT) {
									let saState = res.DATA.rel;
									if (res.DATA.rel.length > 0) {
										iframe.find("#Industry").html(createOptions(res.DATA.rel, 'id', 'text', true));
									}
								}
							});
							
							
							
							/* return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
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
							}); */
							
							
							//iframe.find("#State").html($("#State").html());
							//iframe.find("#Industry").html($("#Industry").html());
						},
						yes: function (index, layero) {
							layer.confirm("確定要儲存？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
								var iframe = layero.find('iframe').contents();
								var formData = getFormSerialize($(iframe.find('#form_main')));
								
								fnAddBusinessOpportunity(formData);
								
								layer.close(index);
							});
						},
						end: function () {
							//oGrid.loadData();
						}
					});
				});
				
                $('#btnCreateComplaint').on('click', function () {
                    parent.openPageTab('Complaint_Upd', '?Action=Add&CustomerId=' + sDataId);
                });
                $("#CustomerCName").on('change', function(res){
                    sCustomerCName = $('#CustomerCName').val();
                })
                /* $.validator.addMethod("compardate", function (value, element, parms) {
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
                }); */
            };
		
		init();
	};
	
require(['base', 'select2', 'autocompleter', 'formatnumber', 'jquerytoolbar', 'timepicker', 'jsgrid', 'ajaxfile', 'common_opm', 'util'], fnPageInit, 'timepicker');
//require(['base', 'select2', 'jsgrid', 'ajaxfile', 'util'], fnPageInit);