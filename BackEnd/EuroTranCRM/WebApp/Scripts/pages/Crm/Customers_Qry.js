'use strict';
var fnPageInit = function () {
    var saCustomerList = [],
		saState = [],
		sBlackListReasonHtml = '',
		oAuditFlag = {
        'N': 'common.NotAudit',
        'Y': 'common.Audited',
        'P': 'common.InAudit',
        'A': 'common.AuditAgain',
        'Z': 'common.ApplyforUpdateing',
        'Q': 'common.NotPass'
    },
        setStateDrop = function () {
            return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                OrgID: 'TE',
                ArgClassID: 'Area',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    saState = res.DATA.rel;
                    if (saState.length > 0) {
                        $('#State').append(createOptions(saState, 'id', 'text', true)).select2();
                    }
                }
            });
        },
        canDo = new CanDo({
            sortField: 'CreateDate',
            sortOrder: 'desc',
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['OrgID', 'guid'],
            /**
             * Grid欄位設置（可以是 function）
             */
            gridFields: [
                {
					name: "CheckCombine", title: '', width: 30, align: "center",
					itemTemplate: function (value, item) {
						return $("<input>", {
							type: 'checkbox', click: function (e) {
								e.stopPropagation();
								if (this.checked) {
									saCustomerList.push(item);
								}
								else {
									var saNewList = [];
									$.each(saCustomerList, function (idx, data) {
										if (item.guid !== data.guid) {
											saNewList.push(data);
										}
									});
									saCustomerList = saNewList;
								}
							}
						});
					}
				},
				{
                    name: "RowIndex", title: 'common.RowNumber', type: "text", width: 50, align: "center", sorting: false
                },
				{
                    name: "CustomerNO", title: 'Customers_Upd.CustomerNO', type: "text", align: "center", width: 80
                },
				{
                    name: "CustomerShotCName", title: 'Customers_Upd.CustomerShotCName', type: "text", width: 100
                },
                {
                    name: "CustomerCName", title: 'Customers_Upd.CustomerCName', type: "text", width: 200
                },
                {
                    name: "CustomerEName", title: 'Customers_Upd.CustomerEName', type: "text", width: 200
                },
                {
                    name: "UniCode", title: 'Customers_Upd.UniCode', type: "text", align: "center", width: 80
                },
                /* {
                    name: "Contactors", title: 'common.Contactor', type: "text", width: 120, itemTemplate: function (val, item) {
                        var saContactors = JSON.parse(item.Contactors || '[]'),
                            sContactors = '';
                        if (saContactors.length > 0) {
                            sContactors = Enumerable.From(saContactors).Select("$.FullName").ToArray().join('，');
                        }
                        return sContactors;
                    }
                },
                {
                    name: "Address", title: 'common.Address', type: "text", width: 230
                }, */
                {
                    name: "CreateUserName", title: 'common.CreateUser', type: "text", width: 70
                },
                {
                    name: "CreateDate", title: 'common.CreateDate', type: "text", width: 120, itemTemplate: function (val, item) {
                        return newDate(val);
                    }
                },
				{
                    name: "ModifyDate", title: 'common.ModifyDate', type: "text", width: 120, itemTemplate: function (val, item) {
                        return newDate(val);
                    }
                },
                {
                    name: "IsAudit", title: 'common.Audit_Status', type: "text", width: 80, align: "center", itemTemplate: function (val, item) {
                        return i18next.t(oAuditFlag[val]);
                    }
                },
                {
                    name: "Effective", title: 'common.Status', type: "text", width: 50, align: "center", itemTemplate: function (val, item) {
                        return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');// ╠common.Effective⇒有效╣ ╠common.Invalid⇒無效╣
                    }
                }
            ],
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            getExcel: function (pargs) {
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['200px', '280px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '\
                             <div class="pop-box">\
                               <p><button type="button" data-i18n="common.BasicInformation" id="Cus_BasicInformation" class="btn-custom w100p  green">基本資料</button></p>\
                               <p><button type="button" data-i18n="common.Cus_Email" id="Cus_Email" class="btn-custom w100p green">名稱+Email</button></p>\
                               <p><button type="button" data-i18n="common.WenzhongCusFile" id="Cus_WenzhongCusFile" class="btn-custom w100p green">文中客供商檔</button></p>\
                             </div>',//╠common.BasicInformation⇒基本資料╣╠common.Cus_Email⇒名稱+Email╣╠common.WenzhongCusFile⇒文中客供商檔╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            canDo.getPage({
                                Excel: true,
                                ExcelType: sToExcelType,
                                Index: idx
                            });
                        });
                        canDo._transLang(layero);
                    }
                });
            },
            /**
             * 頁面初始化
             * @param  {Object} pargs CanDo 對象
             */
            pageInit: function (pargs) {

                var ss = canDo;
                $.whenArray([
                    setStateDrop(),
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            ShowId: true,
                            Select2: true
                        }
                    ]),
					fnSetArgDrop([
						{
							OrgID: 'TE',
							ArgClassID: 'ExhibClass',
							ShowId: true,
							CallBack: function (data) {
								$('#Industry').html(createOptions(data, 'id', 'text', true));
							}
						},
						{
							OrgID: 'TE',
							ArgClassID: 'BlackListReason',
							CallBack: function (data) {
								sBlackListReasonHtml = createOptions(data, 'id', 'text');
							}
						}
					]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'TranType',
                            CallBack: function (data) {
                                var sOptions = '<label for="TransactionType_7"><input type="radio" id="TransactionType_7" name="TransactionType" value="A,B,C,D,E,F," checked="checked">全部</label>' + createRadios(data, 'id', 'text', 'TransactionType')
                                $('#transactiontype').html(sOptions).find('[name="TransactionType"]').click(function () {
                                    $('#Toolbar_Qry').trigger('click');
                                });
                                pargs._uniformInit($('#transactiontype'));
                            }
                        }
                    ]),
                ])
                    .done(function () {
                        pargs._reSetQueryPm();
                        pargs._initGrid();
                    });
					
				$("#Toolbar").append(
				'<button type="button" key="Combine" id="Toolbar_Combine" name="Toolbar_Combine" data-i18n="" class="btn-custom orange">合併</button>');
				
				$('#Toolbar_Combine').click(function () {
					if (saCustomerList.length != 2) {
						showMsg("請勾選兩筆資料進行合併");
						return false;
					} else {
						g_api.ConnectLite('Customers_Upd', 'CheckCombineCustomer', {
							guid1: saCustomerList[0].guid,
							guid2 : saCustomerList[1].guid
						}, function (res) {
							if (res.DATA.rel) {
								if(res.DATA.rel == 1){
									layer.open({
										type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
										title: "合併客戶",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
										area: ['70%', '90%'],//寬度
										shade: 0.75,//遮罩
										//maxmin: true, //开启最大化最小化按钮
										id: 'layer_CombineCustomer', //设定一个id，防止重复弹出
										offset: '10px',//右下角弹出
										anim: 0,//彈出動畫
										btn: ["合併", i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
										btnAlign: 'c',//按鈕位置
										content: '../POP/Customer_Combine.html',
										success: function (layero, index) {
											g_api.ConnectLite('Customers_Upd', 'Querytwo', {
												guid1: saCustomerList[0].guid,
												guid2 : saCustomerList[1].guid
											}, function (res) {
												if (res.RESULT) {
													var oRes = res.DATA.rel;
													var iframe = layero.find('iframe').contents();
													
													if (saState.length > 0) {
														iframe.find('#State1').html(createOptions(saState, 'id', 'text', true));
														iframe.find('#State2').html(createOptions(saState, 'id', 'text', true));
													}
													
													iframe.find('#BlackListReason1').html(sBlackListReasonHtml).val(oRes[0].BlackListReason);
													iframe.find('#BlackListReason2').html(sBlackListReasonHtml).val(oRes[1].BlackListReason);
													
													iframe.find('#CustomerNO1').val(oRes[0].CustomerNO);
													iframe.find('#UniCode1').val(oRes[0].UniCode);
													iframe.find('#CustomerShotCName1').val(oRes[0].CustomerShotCName);
													iframe.find('#CustomerCName1').val(oRes[0].CustomerCName);
													iframe.find('#CustomerEName1').val(oRes[0].CustomerEName);
													iframe.find('#Telephone1').val(oRes[0].Telephone);
													iframe.find('#Email1').val(oRes[0].Email);
													iframe.find('#FAX1').val(oRes[0].FAX);
													iframe.find('#State1').val(oRes[0].State);
													iframe.find('#Address1').val(oRes[0].Address);
													iframe.find('#InvoiceAddress1').val(oRes[0].InvoiceAddress);
													iframe.find('#WebsiteAddress1').val(oRes[0].WebsiteAddress);
													iframe.find('#Memo1').val(oRes[0].Memo);
													iframe.find('[name=IsBlackList1][value="' + oRes[0].IsBlackList + '"]').attr("checked", true);
													iframe.find('[name=IsGroupUnit1][value="' + oRes[0].IsGroupUnit + '"]').attr("checked", true);
													if(oRes[0].IsBlackList == "Y"){
														iframe.find('#BlackListReason1').attr("disabled", false);
													}
													
													iframe.find('#CustomerNO2').val(oRes[1].CustomerNO);
													iframe.find('#UniCode2').val(oRes[1].UniCode);
													iframe.find('#CustomerShotCName2').val(oRes[1].CustomerShotCName);
													iframe.find('#CustomerCName2').val(oRes[1].CustomerCName);
													iframe.find('#CustomerEName2').val(oRes[1].CustomerEName);
													iframe.find('#Telephone2').val(oRes[1].Telephone);
													iframe.find('#Email2').val(oRes[1].Email);
													iframe.find('#FAX2').val(oRes[1].FAX);
													iframe.find('#State2').val(oRes[1].State);
													iframe.find('#Address2').val(oRes[1].Address);
													iframe.find('#InvoiceAddress2').val(oRes[1].InvoiceAddress);
													iframe.find('#WebsiteAddress2').val(oRes[1].WebsiteAddress);
													iframe.find('#Memo2').val(oRes[1].Memo);
													iframe.find('[name=IsBlackList2][value="' + oRes[1].IsBlackList + '"]').attr("checked", true);
													iframe.find('[name=IsGroupUnit2][value="' + oRes[1].IsGroupUnit + '"]').attr("checked", true);
													if(oRes[1].IsBlackList == "Y"){
														iframe.find('#BlackListReason2').attr("disabled", false);
													}
													
												}
												else {
													showMsg(i18next.t('message.NotFindData') + '<br>' + res.MSG, 'error'); // ╠message.NotFindData⇒查不到對應的資料╣
												}
											}, function () {
												showMsg(i18next.t('message.NotFindData'), 'error');
											});
										},
										yes: function (index, layero) {
											layer.confirm("確定要合併？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
												var iframe = layero.find('iframe').contents();
												//var combinedata = getFormSerialize($(iframe.find('#form_main')));
												var combinedata = ss._getFormSerialize($(iframe.find('#form_main')));
												combinedata.Type = res.DATA.rel;
												combinedata.guid1 = saCustomerList[0].guid;
												combinedata.guid2 = saCustomerList[1].guid;
												
												g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
												, function (res) {
													if (res.DATA.rel) {
														//oGrid.loadData();
														showMsg("合併成功", 'success');
														pargs._initGrid();
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
											saCustomerList = [];
											pargs._initGrid();
										}
									});
								} else if(res.DATA.rel == 2){
									layer.confirm("正式客戶合併非正式客戶，確定要合併？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
										var combinedata = {};
										combinedata.Type = res.DATA.rel;
										combinedata.guid1 = saCustomerList[0].guid;
										combinedata.guid2 = saCustomerList[1].guid;
										
										g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
										, function (res) {
											if (res.DATA.rel) {
												saCustomerList = [];
												//oGrid.loadData();
												showMsg("合併成功", 'success');
												pargs._initGrid();
												layer.close(index);
											}
											else {
												showMsg("合併失敗", 'error');
											}
										}
										, function () {
											showMsg("合併失敗", 'error');
										});
										
										layer.close(index);
									});
								} else if(res.DATA.rel == 3){
									showMsg("所選客戶皆為正式客戶，無法合併", 'error');
								} else if(res.DATA.rel == 4){
									showMsg("所選客戶中有重新提交審核中之客戶，無法合併", 'error');
								} else if(res.DATA.rel == 5){
									showMsg("所選客戶中有提交審核中之客戶，無法合併", 'error');
								} else if(res.DATA.rel == 6){
									showMsg("所選客戶中有申請修改中之客戶，無法合併", 'error');
								} else if(res.DATA.rel == 7){
									showMsg("所選客戶中有狀態無法確認之客戶，無法合併", 'error');
								}
							}
							else {
								showMsg("無法確認客戶資料，無法合併", 'error');
							}
						}
						, function () {
							showMsg("無法確認客戶資料，無法合併", 'error');
						});
						
					}
	
					return false;
				});
            }
        });
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);