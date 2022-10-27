'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
	objCombineData,
    fnPageInit = function () {
        var saContactorList = [],
			saCustomerShotCNameList = [],
			oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CustomerShotCName',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param  {Object} args 查詢參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (args.Excel) {//匯出
                            DownLoadFile(oRes);
                            layer.close(args.Index);
                        }
                    }
                });
            },
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.confirm("確定要匯出嗎？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                    fnGet({
                        Excel: true,
                        Index: index
                    });
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
                        var iNum = $('#PerPageNum').val();
                        oGrid.pageSize = iNum === '' ? parent.SysSet.GridRecords || 10 : iNum;
                        cacheQueryCondition();
                        oGrid.openPage(window.bToFirstPage ? 1 : oBaseQueryPm.pageIndex);

                        break;
                    case "Toolbar_Save":

                        fnSave('add');

                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        break;

                    case "Toolbar_Add":
                        parent.openPageTab(sEditPrgId, '?Action=Add');

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_Exp":
                        if (oGrid.data.length === 0) {
                            showMsg(i18next.t("message.NoDataExport"));// ╠message.NoDataExport⇒沒有資料匯出╣
                            return false;
                        }
                        fnOpenPopToExcel(); 
                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
			/**
            * 目的 合併聯絡人
            * @param
            */
            fnCombineContactor = function (_guid1,_guid2) {
				
				return g_api.ConnectLite("Contactors_Upd", "GetImmediateSupervisor", {
					Guid1: _guid1,
					Guid2: _guid2
				}, function (res) {
					if (res.RESULT) {
						let saImmediateSupervisor = res.DATA.rel;
						var strCustomerId = "";
						if (saImmediateSupervisor.length > 0) {
							var obj;
							layer.open({
								type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
								title: "合併聯絡人",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
								area: ['70%', '90%'],//寬度
								shade: 0.75,//遮罩
								//maxmin: true, //开启最大化最小化按钮
								id: 'layer_CombineContactor', //设定一个id，防止重复弹出
								offset: '10px',//右下角弹出
								anim: 0,//彈出動畫
								btn: ['合併', i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
								btnAlign: 'c',//按鈕位置
								content: '../POP/Contactor_Combine.html',
								success: function (layero, index) {
									objCombineData = layero;
									
									g_api.ConnectLite('Contactors_Qry', 'QueryTwo', {
										guid1: _guid1,
										guid2 : _guid2
									}, function (res) {
										if (res.RESULT) {
											var oRes = res.DATA.rel;
											var iframe = layero.find('iframe').contents();
											
											let objImmediateSupervisor1 = iframe.find('#ImmediateSupervisor1');
											let objImmediateSupervisor2 = iframe.find('#ImmediateSupervisor2');
											$(objImmediateSupervisor1).html(createOptions(saImmediateSupervisor, 'id', 'text', false)).select();
											$(objImmediateSupervisor2).html(createOptions(saImmediateSupervisor, 'id', 'text', false)).select();
											
											let strCustomerName1 = '';
											let strCustomerName2 = '';
											
											if(oRes[0].CustomerShotCName == '' || oRes[0].CustomerShotCName == null){
												if(oRes[0].CustomerCName == '' || oRes[0].CustomerCName == null){
													strCustomerName1 = oRes[0].CustomerEName;
												} else {
													strCustomerName1 = oRes[0].CustomerCName;
												}
											} else {
												if(oRes[0].CustomerCName == '' || oRes[0].CustomerCName == null){
													strCustomerName1 = '(' + oRes[0].CustomerShotCName + ')' + oRes[0].CustomerEName;
												} else {
													strCustomerName1 = '(' + oRes[0].CustomerShotCName + ')' + oRes[0].CustomerCName;
												}
											}
											
											if(oRes[1].CustomerShotCName == '' || oRes[1].CustomerShotCName == null){
												if(oRes[1].CustomerCName == '' || oRes[1].CustomerCName == null){
													strCustomerName2 = oRes[1].CustomerEName;
												} else {
													strCustomerName2 = oRes[1].CustomerCName;
												}
											} else {
												if(oRes[1].CustomerCName == '' || oRes[1].CustomerCName == null){
													strCustomerName2 = '(' + oRes[1].CustomerShotCName + ')' + oRes[1].CustomerEName;
												} else {
													strCustomerName2 = '(' + oRes[1].CustomerShotCName + ')' + oRes[1].CustomerCName;
												}
											}
											
											iframe.find('#Call1').val(oRes[0].Call);
											iframe.find('#Call2').val(oRes[1].Call);
											iframe.find('#ContactorName1').val(oRes[0].ContactorName);
											iframe.find('#ContactorName2').val(oRes[1].ContactorName);
											iframe.find('#NickName1').val(oRes[0].NickName);
											iframe.find('#NickName2').val(oRes[1].NickName);
											iframe.find('#Birthday1').val(oRes[0].Birthday);
											iframe.find('#Birthday2').val(oRes[1].Birthday);
											iframe.find('#MaritalStatus1').val(oRes[0].MaritalStatus);
											iframe.find('#MaritalStatus2').val(oRes[1].MaritalStatus);
											iframe.find('#PersonalMobilePhone1').val(oRes[0].PersonalMobilePhone);
											iframe.find('#PersonalMobilePhone2').val(oRes[1].PersonalMobilePhone);
											iframe.find('#PersonalEmail1').val(oRes[0].PersonalEmail);
											iframe.find('#PersonalEmail2').val(oRes[1].PersonalEmail);
											iframe.find('#LINE1').val(oRes[0].LINE);
											iframe.find('#LINE2').val(oRes[1].LINE);
											iframe.find('#WECHAT1').val(oRes[0].WECHAT);
											iframe.find('#WECHAT2').val(oRes[1].WECHAT);
											iframe.find('#Personality1').val(oRes[0].Personality);
											iframe.find('#Personality2').val(oRes[1].Personality);
											iframe.find('#Preferences1').val(oRes[0].Preferences);
											iframe.find('#Preferences2').val(oRes[1].Preferences);
											iframe.find('#PersonalAddress1').val(oRes[0].PersonalAddress);
											iframe.find('#PersonalAddress2').val(oRes[1].PersonalAddress);
											iframe.find('#Memo1').val(oRes[0].Memo);
											iframe.find('#Memo2').val(oRes[1].Memo);
											iframe.find('#CustomerCName1').val(strCustomerName1);
											iframe.find('#CustomerCName2').val(strCustomerName2);
											strCustomerId = oRes[0].CustomerId;
											iframe.find('#JobTitle1').val(oRes[0].JobTitle);
											iframe.find('#JobTitle2').val(oRes[1].JobTitle);
											iframe.find('#Department1').val(oRes[0].Department);
											iframe.find('#Department2').val(oRes[1].Department);
											iframe.find('#ImmediateSupervisor1').val(oRes[0].ImmediateSupervisor);
											iframe.find('#ImmediateSupervisor2').val(oRes[1].ImmediateSupervisor);
											iframe.find('#Telephone11').val(oRes[0].Telephone1);
											iframe.find('#Telephone12').val(oRes[1].Telephone1);
											iframe.find('#Telephone21').val(oRes[0].Telephone2);
											iframe.find('#Telephone22').val(oRes[1].Telephone2);
											iframe.find('#Email11').val(oRes[0].Email1);
											iframe.find('#Email12').val(oRes[1].Email1);
											iframe.find('#Email21').val(oRes[0].Email2);
											iframe.find('#Email22').val(oRes[1].Email2);
											iframe.find('#ChoseReason1').val(oRes[0].ChoseReason);
											iframe.find('#ChoseReason2').val(oRes[1].ChoseReason);	
											iframe.find('#Ext11').val(oRes[0].Ext1);
											iframe.find('#Ext12').val(oRes[1].Ext1);
											iframe.find('#Ext21').val(oRes[0].Ext2);
											iframe.find('#Ext22').val(oRes[1].Ext2);											
										} else {
											showMsg(i18next.t('message.NotFindData') + '<br>' + res.MSG, 'error'); // ╠message.NotFindData⇒查不到對應的資料╣
										}
									}, function () {
										showMsg(i18next.t('message.NotFindData'), 'error');
									});
								},
								yes: function (index, layero) {
										
									layer.confirm("確定要合併？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
										var iframe = layero.find('iframe').contents();								
										var combinedata = getFormSerialize($(iframe.find('#form_main')));
																				
										combinedata.guid1=_guid1;										
										combinedata.guid2=_guid2;
										combinedata.CustomerId = strCustomerId;
										
										g_api.ConnectLite('Contactors_Upd', 'CombineContactor', combinedata
										, function (res) {
											if (res.DATA.rel) {
												saContactorList = [];
												saCustomerShotCNameList=[];
												oGrid.loadData();
												showMsg("合併成功", 'success');
												layer.close(index);
											}
											else {
												showMsg("合併失敗", 'error');
											}
										}
										, function () {
											showMsg("合併失敗", 'error');
										});
										
										layer.close(index-1);
									});
								},
								end: function () {
									saContactorList = [];
									saCustomerShotCNameList=[];
									oGrid.loadData();
								}
							});							
						}
					}
				});
				
                
            },
            /**
             * 初始化 function
             */
            init = function () {
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });
                $.whenArray([
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            Select2: true,
                            ShowId: true
                        }
                    ])
				])
                .done(function () {
                    reSetQueryPm(sProgramId);
                    var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;

                    $("#jsGrid").jsGrid({
                        width: "100%",
                        height: iHeight + "px",
                        autoload: true,
                        pageLoading: true,
                        sorting: true,
                        paging: true,
                        pageIndex: window.bToFirstPage ? 1 : window.QueryPageidx || 1,
                        pageSize: parent.SysSet.GridRecords || 10,
                        pageButtonCount: parent.SysSet.GridPages || 15,
                        pagePrevText: "<",
                        pageNextText: ">",
                        pageFirstText: "<<",
                        pageLastText: ">>",
                        onPageChanged: function (args) {
                            cacheQueryCondition(args.pageIndex);
                        },
                        rowClick: function (args) {
                            /* if (navigator.userAgent.match(/mobile/i)) {
                                goToEdit(sEditPrgId, '?Action=Upd&guid=' + args.item.guid);
                            } */
                        },
                        rowDoubleClick: function (args) {
                            parent.openPageTab(sEditPrgId, '?Action=Upd&guid=' + args.item.guid);
                        },
                        fields: [
							{
								name: "CheckCombine", title: '', width: 30, align: "center",
								itemTemplate: function (value, item) {								
									return $("<input>", {
										type: 'checkbox', click: function (e) {
											
											e.stopPropagation();
											if (this.checked) {
												saContactorList.push(item);
												saCustomerShotCNameList.push($(this).parent().parent().find("td:eq(5)").text());
											} else {
												var saNewList = [];
												var intIndex=0;
												$.each(saContactorList, function (idx, data) {
													if (item.guid != data.guid) {
														
														saNewList.push(data);
													} else {
														intIndex = idx;
													}
												});
												
												var saNewCustomerShotCNameList = [];
												$.each(saCustomerShotCNameList, function (idx, data) {
													if (intIndex != idx) {														
														saNewCustomerShotCNameList.push(data);
													}
												});
												
												saContactorList = saNewList;
												saCustomerShotCNameList = saNewCustomerShotCNameList;
												
											}									
										}
									});
								}
							},
							{
								name: "RowIndex", title: 'common.RowNumber', type: "text", width: 30, align: "center", sorting: false
							},
							{
								name: "Call", title: '稱呼', type: "text", width: 40, itemTemplate: function (val, item) {
										if(item.Call == "1"){
											return "Mr.";
										} else if(item.Call == "2"){
											return "Miss.";											
										}
								}
							},
							{
								name: "ContactorName", title: '聯絡人姓名', type: "text", width: 100
							},
							{
								name: "JobTitle", title: '職稱', type: "text", width: 100
							},
							{
								name: "CustomerShotCName", title: '客戶簡稱', type: "text", width: 120
							},
							{
								name: "UniCode", title: '統一編號', type: "text", width: 100
							},
							{
								name: "Telephone1", title: '電話', type: "text", width: 120
							},
							{
								name: "Ext1", title: '分機', type: "text", width: 80
							},
							{
								name: "Email1", title: 'Email', type: "text", width: 150
							},
							{
								name: "Birthday", title: '生日', type: "text", width: 60
							},
							{
								name: "ModifyDate", title: 'common.ModifyDate', type: "text", align: "center", width: 100,itemTemplate: function (val, item) {
									if(val != null){	
										return newDate(val);
									}
								}
							}
						],
                        controller: {
                            loadData: function (args) {
                                return fnGet(args);
                            }
                        },
                        onInit: function (args) {
                            oGrid = args.grid;
                        }
                    });
					
					$("#Toolbar").append(
					'<button type="button" key="Combine" id="Toolbar_Combine" name="Toolbar_Combine" data-i18n="" class="btn-custom orange">合併</button>');
					
					$('#Toolbar_Combine').click(function () {						
						if (saContactorList.length != 2) {
							showMsg("請勾選兩筆資料進行合併");						
							return false;
						}
						
						if(saCustomerShotCNameList[0] != saCustomerShotCNameList[1]){
							showMsg("合併聯絡人只能選擇相同的客戶合併");						
							return false;							
						}
						
						fnCombineContactor(saContactorList[0].guid,saContactorList[1].guid);
						return false;
					});
                });
            };
        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);