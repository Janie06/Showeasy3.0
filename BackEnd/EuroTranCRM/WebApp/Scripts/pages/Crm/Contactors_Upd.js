'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('guid'),
	sUrlCustomerId = getUrlParam('CustomerId'),
	sUrlExhibitionNO = getUrlParam('ExhibitionNO'),
    sCheckId = sDataId,
    bRefresh = false,
    fnPageInit = function () {
		/*
		* 取得直屬上司資料
		*/
		var setImmediateSupervisorDrop = function (_strCustomerId) {			
            return g_api.ConnectLite("Contactors_Upd", "GetImmediateSupervisor", {
                Guid: sDataId,
				CustomerId: _strCustomerId
            }, function (res) {
                if (res.RESULT) {
                    let saState = res.DATA.rel;
					
                    if (saState.length > 0) {
                        $('#ImmediateSupervisor').html(createOptions(saState, 'id', 'text', false)).select2();
                    }
                }
            });
			
        };
		
		/*
		* 取得客戶資料
		*/
		var setCustomerCNameDrop = function () {				
            return g_api.ConnectLite(Service.sys, 'GetAllCustomerlist', { //"Contactors_Upd", "GetCustomerCName"
                //Guid: sDataId
            }, function (res) {
                if (res.RESULT) {
                    let saState = res.DATA.rel;
                    if (saState.length > 0) {
                        $('#CustomerId').html(createOptions(saState, 'id', 'text', false)).select2();
                    }
                }
            });		
        };
			
        var oGrid = null,
            oForm = $('#form_main'),
            oGrid1 = null,
			oGrid2 = null,
            oValidator = null,
            oCurData = {},
            saGridData = [],
			saGridData1 = [],
			saGridData2 = [],
            saState = [],
            saHalls = [],
			sCustomerGuid = '',
			sContactorName = '',
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.top.SysSet.GridRecords || 10,
                sortField: 'CustomerCName',
                sortOrder: 'asc'
            },
			canDo = new CanDo({
				pageInit: function () {
					if (getUrlParam('Flag') === 'Pop') {
                        $('#Toolbar_Leave,#Toolbar_ReAdd,#APIImport').hide();
                    }
				}
			}),
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {				
                if (sDataId) {					
                    $('#litab2').show();
					$('#litab3').show();
					
                    return g_api.ConnectLite(sQueryPrgId, "QueryOne",
					//return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {							
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oCurData = oRes;
								
								sCustomerGuid = oCurData.CustomerId;
								sContactorName = oCurData.ContactorName;
								//$("#CustomerId").val(oRes.CustomerId);
								//console.log(oRes);
								//setFormVal(oForm, oCurData);
                            }
                        });
                } else {
					$('#litab2').hide();
                    $('#litab3').hide();
                    oCurData.LogoFileId = guid();
					
					if(sUrlCustomerId != '' && sUrlCustomerId != null){
						$("#CustomerId").val(sUrlCustomerId);
						$('#CustomerId').select2();
						$("#CustomerId").prop("disabled", true);
						setImmediateSupervisorDrop(sUrlCustomerId);	
					}
					
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param   {String} sFlag  新增或儲存後新增
             */
            fnAdd = function (sFlag) {			
                var data = getFormSerialize(oForm);
                data.LogoFileId = oCurData.LogoFileId;
				data.guid = guid();
				data.ExhibitionNO = sUrlExhibitionNO;
				
				g_api.ConnectLite("Contactors_Upd", "Add", 
					data
				, function (res) {						
					if (res.RESULT > 0) {
						bRequestStorage = false;
						if($("#hiddenIndex").val() === ""){
							if (sFlag === 'add') {							
								showMsgAndGo(i18next.t("message.Save_Success"), sQueryPrgId, '?Action=Upd'); // ╠message.Save_Success⇒新增成功╣
							} else {
								showMsgAndGo(i18next.t("message.Save_Success"), sQueryPrgId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
							}
						} else {
							showMsg(i18next.t("message.Save_Success"), 'success'); //╠message.Save_Success⇒新增成功╣
							setTimeout(function () {
                                parent.layer.close($("#hiddenIndex").val());
                            }, 1000);
						}
					} else {
						showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
					}
				}, function () {
					showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
				});
                    
            },
            
            /**
             * 修改資料
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);             
				data.guid = sDataId;
                data.LogoFileId = oCurData.LogoFileId;
				
                g_api.ConnectLite("Contactors_Upd", "Upd", 
					data
				, function (res) {
                    if (res.RESULT > 0) {
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
                return g_api.ConnectLite("Contactors_Upd", "Delete",					
                {
                    Guid: sDataId
				}, function (res) {
                    if (res.RESULT > 0) {
						if($("#hiddenIndex").val() === ""){
							showMsgAndGo(i18next.t("message.Delete_Success"), sQueryPrgId); // ╠message.Delete_Success⇒刪除成功╣
						} else {
							showMsg(i18next.t("message.Delete_Success"), 'success'); //╠message.Delete_Success⇒刪除成功╣
							setTimeout(function () {
                                parent.layer.close($("#hiddenIndex").val());
                            }, 1000);
						}
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
			
			
            /**
             * 取得 展覽已成交 資料             
             */
            fnGetGridData = function (flag) {
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetDealExhibitionlist', {
                        guid: sDataId
                    });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
			
            /**
             * 取得 展覽未成交 資料             
             */
            fnGetGridData1 = function (flag) {
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetUnDealExhibitionlist', {
                        guid: sDataId
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
			fnGetGridData2 = function () {
				if (sDataId) {
					return g_api.ConnectLite(sProgramId, 'GetComplaintlist', {
						CustomerId: sCustomerGuid,
						ContactorName: sContactorName
					});
				}
				else {
					return $.Deferred().resolve().promise();
				}
			},
            /**
             * 同步新更新行事曆和outlook
             * @param   {Object}_data 序列化資料
             */
            fnSynChronousCalendar = function (_data) {
                var data = {};
                data.CalType = '04';//展覽
                data.Title = _data.ExhibitioShotName_TW;
                data.Description = _data.Exhibitioname_TW + '（' + _data.Exhibitioname_EN + '）    ' + _data.Memo;
                data.StartDate = _data.ExhibitionDateStart;
                data.EndDate = _data.ExhibitionDateEnd;
                data.Color = parent.UserInfo.CalColor;
                data.RelationId = _data.ExhibitionCode;
                data.GroupMembers = '';
                data.Importment = 'M';
                data.OpenMent = 'C';
                data.AllDay = '0';

                g_api.ConnectLite('Calendar', ComFn.GetAdd, data,
                    function (res) {
                        if (res.RESULT) {
                            var sNo = res.DATA.rel;
                            if (parent.Outklook) {
                                outlookAPI(outlook.Calendar_Add, {
                                    NO: sNo,
                                    ResponseRequested: false
                                });
                            }
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
                        } else {
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
            * 目的 對應正式客戶
            * @param  {String}item 預約單資料
            */
            fnCorrespondFormalCus = function (item) {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: '新增至展覽名單', // ╠common.CorrespondImpCus⇒對應正式客戶╣
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
                                     <span>請選擇新增展覽名稱</span><span>：</span>\
                                 </div>\
                                 <div class="col-sm-12">\
                                     <select class= "form-control w95p" id="ExhibitionCode" name="ExhibitionCode"></select>\
                                 </div>\
                              </div >',
                    success: function (layero, index) {
						fnSetEpoDrop({
							Select: $('#ExhibitionCode'),
							Select2: true
						});
                    },
                    yes: function (index, layero) {
						var sExhibitionNO = $('#ExhibitionCode').val();
                        var sCustomerId = $('#CustomerId').val();
                        if (!sExhibitionNO) {
                            showMsg(i18next.t('message.SelectFormalCus'));//╠message.SelectFormalCus⇒請選擇對應的客戶╣
                            return false;
                        }
                        g_api.ConnectLite('Contactors_Upd', 'InsertExhibitionList', {
							ExhibitionNO: sExhibitionNO,
							CustomerId: sCustomerId,
							ContactorId: sDataId
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
            /**
             * 初始化 function
             */
            init = function () {			
                var saCusBtns = null;
                $('#State,#Industry,#ExhibitionDate').prop('required', true);
                $('.ShowNames').hide();
                if (parent.OrgID === 'TG') {
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
					
					$("#CustomerId").prop("disabled", true);
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true,
                    tabClick: function (el) {
						console.log(el.id);
                        switch (el.id) {
                            case 'litab2':
                                if (!$(el).data('action')) {
									oGrid.loadData();
									oGrid1.loadData();
                                }
                                break;
                            case 'litab3':
                                if (!$(el).data('action')) {
                                    oGrid2.loadData();
                                }
                                break;
                        }
                        $(el).data('action', true);
                    }
                });

                $.whenArray([				
					setCustomerCNameDrop()
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
						
						if(sAction == "Add"){
							fnGet().done(function (resDetail) {
								if (resDetail && resDetail.RESULT === 1) {						
									var oresDetail = resDetail.DATA.rel,
										
									oDetailData = oresDetail;
									oDetailData.LogoFileId = oDetailData.LogoFileId || guid();
									setFormVal(oForm, oDetailData);		
								}
							});
						} else if(sAction=="Upd") {
							setImmediateSupervisorDrop().done(function (){						
								fnGet().done(function (resDetail) {
									if (resDetail && resDetail.RESULT === 1) {						
										var oresDetail = resDetail.DATA.rel,
											
										oDetailData = oresDetail;
										oDetailData.LogoFileId = oDetailData.LogoFileId || guid();
										setFormVal(oForm, oDetailData);		
									}
								});
							});
						}														
					}
					//select2Init();				
				});

                oValidator = $("#form_main").validate({
                    ignore: ''
                });

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
                    paging: false,
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
							name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', type: "text", width: 120
						},
						{
							name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 180
						},
						{
							name: "Exhibitioname_EN", title: 'Exhibition_Upd.Exhibitioname_EN', type: "text", width: 180
						},
						{
							name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150, itemTemplate: function (val, item) {
								var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
								return sDateRange === '~' ? '' : sDateRange;
							}
						},
                        {
                            name: "CreateUser", title: '創建人', width: 150
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
                    }
                });
				$("#jsGrid1").jsGrid({
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
                            name: "RefNumber", title: '查詢號碼', width: 200
                        },
						{
							name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', type: "text", width: 120
						},
						{
							name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 180
						},
						{
							name: "Exhibitioname_EN", title: 'Exhibition_Upd.Exhibitioname_EN', type: "text", width: 180
						},
						{
							name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150, itemTemplate: function (val, item) {
								var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
								return sDateRange === '~' ? '' : sDateRange;
							}
						},
                        {
                            name: "CreateUser", title: '創建人', width: 150
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
                    }
                });
                $("#jsGrid2").jsGrid({
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
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
                            name: "ComplaintNumber", title: '個案代號', width: 200
                        },
						{
							name: "ExhibitioShotName_TW", title: 'Exhibition_Upd.ExhibitioShotName_TW', type: "text", width: 120
						},
						{
							name: "ExhibitionName", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 180
						},
						{
							name: "ComplaintType", title: '類型', type: "text", width: 180, itemTemplate: function (val, item) {
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
							name: "Description", title: '內容', type: "text", width: 180
						},
                        {
                            name: "CreateUser", title: '創建人', width: 150
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetGridData2();
                        },
                        insertItem: function (args) {
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid2 = args.grid;
                    }
                });
				
				$('#InsertExhibitionsList').click(function () {
					fnCorrespondFormalCus();
					return false;
				});
				
				$('#InsertComplaint').click(function () {
					//alert(encodeURIComponent(sContactorName));
					parent.top.openPageTab('Complaint_Upd', '?Action=Add&CustomerId=' + sCustomerGuid + '&ContactorName=' + btoa(encodeURI(sContactorName)));
					return false;
				});
				
				$('#CustomerId').change(function () {					
					if(sAction == "Add"){
						setImmediateSupervisorDrop($(this).val());	
						return false;
					}
				});
            };
					
        init();
		
    };


require(['base', 'select2', 'jsgrid', 'daterangepicker', 'convetlng', 'filer', 'ajaxfile', 'util','cando'], fnPageInit, 'daterangepicker');