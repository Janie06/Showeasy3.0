'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('SN'),
    sTab = getUrlParam('Tab'),
    sCheckId = sDataId,
    bRefresh = false,
	bNewCustomer = false,
    fnPageInit = function () {
        var oGrid = null,
            oForm = $('#form_main'),
            oGrid1 = null,
            oValidator = null,
			oGrid2 = null,
			oGrid3 = null,
            oCurData = {},
			sIndustryHtml = '',
			sListSourceHtml = '',
			sBlackListReasonHtml = '',
			sSN = '',
			sChooseCustomerId = '',
            saGridData = [],
			saGridData2 = [],
			saGridData3 = [],
            saState = [],
            saHalls = [],
			saNewList = [],
			//saNewList2 = [],
			saLastData = [],
			saCustomerList = [],
			saContactorList = [],
			saAddContactorList = [],
			saChooseContactorList = [],
			saExhibitionList = [],
			saListSource = [],
			saGrid2Data = [],
			intListCount = 0,
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CustomerCName',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
				$('#litab2').hide();
                    $('#litab3').hide();
                if (sDataId) {
                    return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oCurData = oRes;
								oCurData.CostRulesIdTE = oCurData.CostRulesId;
								oCurData.CostRulesIdTG = oCurData.CostRulesId;
								
                                $('.TransferResult').show();
								
								$("#ExhibitionCode2").val(oRes.ExhibitionCode);
								$("#ExhibitioShotName_TW2").val(oRes.ExhibitioShotName_TW);
								$("#ExhibitionDateStart2").val(newDate(oRes.ExhibitionDateStart, 'date'));
								$("#ExhibitionDateEnd2").val(newDate(oRes.ExhibitionDateEnd, 'date'));
								//$("#ListSource").val((oRes.ListSource || '').split(',').clear()).trigger('change');
								//$("#ListSource2").val((oRes.ListSource || '').split(',').clear()).trigger('change');
								
								
								
								$("#ResponsiblePerson2").html($("#ResponsiblePerson").html());
								
                                if (oRes.IsTransfer === 'N') {
                                    $('#TransferResult').text(i18next.t("common.Transfer_No")).css('color', 'red');// ╠common.Transfer_No⇒未拋轉╣
                                }
                                else {// ╠common.Transfer_No⇒已拋轉╣  ╠common.TransferDate⇒最後拋轉時間╣
                                    $('#TransferResult').text(i18next.t("common.Transfer_Yes") + '（' + i18next.t("common.TransferDate") + '：' + newDate(oRes.LastTransfer_Time) + '）').css('color', 'green');
                                }
								
								var strCustomerGuid = '';
								//fnGetGridData().done(function () {
                                        //oGrid.loadData();
										
                                        $('#btnAddList').on('click', function () {
											fnAddList();
										});
										$('#btnCombineCustomer').on('click', function () {
											if (saCustomerList.length != 2) {
												showMsg("請勾選兩筆資料進行合併");
												return false;
											}
											if (saCustomerList[0].IsAudit == 'Y' && saCustomerList[1].IsAudit == 'Y') {
												showMsg("兩筆皆為已審核，無法合併");
												return false;
											}
											
											fnCombineCustomer(saCustomerList[0].guid,saCustomerList[1].guid);
										});
										$('#btnRemoveFromList').on('click', function () {
											if (saCustomerList.length == 0) {
												showMsg("請至少勾選一筆資料");
												return false;
											}
											strCustomerGuid = '';
											$.each(saCustomerList, function (idx, data) {
                                                strCustomerGuid += data.guid + ",";
                                            });
											fnRemoveFromList(strCustomerGuid);
										});
										$('#bthImportExhibitors').on('click', function () {
											if (saCustomerList.length == 0) {
												showMsg("請至少勾選一筆資料");
												return false;
											}
											strCustomerGuid = '';
											$.each(saCustomerList, function (idx, data) {
												if(data.IsFormal != "Y"){
													showMsg("所選客戶中有非正式客戶，無法帶入案件");
													strCustomerGuid = '';
													return false;
												}
                                                strCustomerGuid += data.guid + ",";
                                            });
											if(strCustomerGuid != ''){
												fnImportExhibitors(strCustomerGuid);
											}
										});
                                //    });
                            }
                        });
                }
                else {
					$('#litab1 a').click();
					$('#litab2').hide();
                    $('#litab3').hide();
					$('#litab4').hide();
                    oCurData.LogoFileId = guid();
                    fnUpload();
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
                data.IsTransfer = 'N';
                //data.ExhibitionCode = 'SerialNumber|' + parent.UserInfo.OrgID + '|' + (parent.UserInfo.OrgID === 'TE' ? '' : parent.UserInfo.OrgID) + '|MinYear|3||';

                if (!data.ExhibitionDate) {
                    delete data.ExhibitionDateStart;
                    delete data.ExhibitionDateEnd;
                }
                else {
                    data.ExhibitionDateStart = $.trim(data.ExhibitionDate.split('~')[0]);
                    data.ExhibitionDateEnd = $.trim(data.ExhibitionDate.split('~')[1]);
                }
                delete data.ExhibitionDate;
                delete data.file_hidden;

                if (!data.ShelfTime_Home) {
                    delete data.ShelfTime_Home;
                }
                if (!data.ShelfTime_Abroad) {
                    delete data.ShelfTime_Abroad;
                }
				
				if (!data.SeaReceiveingDate) {
                    delete data.SeaReceiveingDate;
                }
				if (!data.SeaClosingDate) {
                    delete data.SeaClosingDate;
                }
				if (!data.AirReceiveingDate) {
                    delete data.AirReceiveingDate;
                }
				if (!data.AirClosingDate) {
                    delete data.AirClosingDate;
                }
				
				if(parent.OrgID != "TG"){
					data.CostRulesId = data.CostRulesIdTE;
				} else {
					data.CostRulesId = data.CostRulesIdTG;
				}
				delete data.CostRulesIdTE;
				delete data.CostRulesIdTG;

                g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: (parent.UserInfo.OrgID !== 'SG' ? '' : parent.UserInfo.OrgID),
                    Flag: 'MinYear',
                    Len: 3,
                    Str: '',
                    AddType: '',
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        data.ExhibitionCode = res.DATA.rel;
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
                        });
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
                    ExhibitioShotName_TW: $('#ExhibitioShotName_TW').val() ? $('#ExhibitioShotName_TW').val() : "",
					Exhibitioname_EN: $('#Exhibitioname_EN').val() ? $('#Exhibitioname_EN').val():  "",
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
                var data = getFormSerialize(oForm);
                data = packParams(data, 'upd');
                data.LogoFileId = oCurData.LogoFileId;

                if (!data.ExhibitionDate) {
                    delete data.ExhibitionDateStart;
                    delete data.ExhibitionDateEnd;
                }
                else {
                    data.ExhibitionDateStart = $.trim(data.ExhibitionDate.split('~')[0]);
                    data.ExhibitionDateEnd = $.trim(data.ExhibitionDate.split('~')[1]);
                }
                delete data.ExhibitionDate;
                delete data.file_hidden;
				
				//delete data.ResponsiblePerson;
				delete data.ListSource;
				
				delete data.ExhibitionCode2;
				delete data.ExhibitioShotName_TW2;
				delete data.ExhibitionDateStart2;
				delete data.ExhibitionDateEnd2;
				delete data.ResponsiblePerson2;
				delete data.Industry2;
				delete data.State2;
				delete data.ListSource;
				delete data.ListSource2;

                if (!data.ShelfTime_Home) {
                    delete data.ShelfTime_Home;
                }
                if (!data.ShelfTime_Abroad) {
                    delete data.ShelfTime_Abroad;
                }
                if (!data.ExhibitionCode) {
                    data.ExhibitionCode = 'SerialNumber|' + parent.UserInfo.OrgID + '||MinYear|3||';
                }
				
				if (!data.SeaReceiveingDate) {
                    delete data.SeaReceiveingDate
                }
				if (!data.SeaClosingDate) {
                    delete data.SeaClosingDate
                }
				if (!data.AirReceiveingDate) {
                    delete data.AirReceiveingDate
                }
				if (!data.AirClosingDate) {
                    delete data.AirClosingDate
                }
				
				if(parent.OrgID != "TG"){
					data.CostRulesId = data.CostRulesIdTE;
				} else {
					data.CostRulesId = data.CostRulesIdTG;
				}
				delete data.CostRulesIdTE;
				delete data.CostRulesIdTG;
				
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        exhibition: {
                            values: data,
                            keys: { SN: sDataId }
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
				g_api.ConnectLite(sProgramId, 'Delete', {
                    SN: sDataId
                }, function (res) {
                    if (res.RESULT) {
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
             * 設定國家下拉選單
             */
            setStateDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'Area',
                    ParentID: '',
                    LevelOfArgument: 1
                }, function (res) {
                    if (res.RESULT) {
                        saState = res.DATA.rel;
                        if (saState.length > 0) {
                            $('#State').html(createOptions(saState, 'id', 'text', true)).change(function () {
                                setExhibitionAddressDrop();
                            });
                            select2Init($('#State').parent());
                            if (parent.OrgID === 'TG' && sAction === 'Add') {
                                $('#State').val('TWN');
                                setExhibitionAddressDrop();
                            }
                        }
                        else {
                            $('#State').html(createOptions([]));
                        }
						
						$('#State2').html($('#State').html());
                    }
                });
            },
            /**
             * 設定展覽地點下拉選單
             */
            setExhibitionAddressDrop = function () {
                var sState = $('#State').val() || '';
                g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'Area',
                    ParentID: sState,
                    LevelOfArgument: 2
                }, function (res) {
                    if (res.RESULT) {
                        saHalls = res.DATA.rel;
                        if (saHalls.length > 0) {
                            $('#ExhibitionAddress').html(createOptions(saHalls, 'id', 'text', true)).change(function () {
                                var sExhibitionAddressId = this.value;
                                if (sExhibitionAddressId) {
                                    var oState = Enumerable.From(saHalls).Where(function (e) { return e.id === sExhibitionAddressId; }).First();
                                    $('#ExhibitionAddress_CN').text(oState.text_cn);
                                    $('#ExhibitionAddress_EN').text(oState.text_en);
                                }
                                else {
                                    $('#ExhibitionAddress_CN').text('');
                                    $('#ExhibitionAddress_EN').text('');
                                }
                            });
                            if (oCurData.ExhibitionAddress) {
                                $('#ExhibitionAddress').val(oCurData.ExhibitionAddress);
                                if (!$('#ExhibitionAddress').val()) {
                                    $('#ExhibitionAddress_CN').text('');
                                    $('#ExhibitionAddress_EN').text('');
                                }
                            }
                        }
                        else {
                            $('#ExhibitionAddress').html(createOptions([]));
                        }
                    }
                });
            },
            /**
             * 設定費用規則下拉選單
             */
            setCostRulesDrop = function () {
                return CallAjax(ComFn.W_Com, ComFn.GetList, {
                    Type: '', Params: {
                        exhibitionrules: {
                            OrgID: parent.OrgID,
							Effective: "Y"
                        }
                    }
                }, function (res) {
                    var saData = JSON.parse(res.d);
                    if (saData) {
                        $('#CostRulesIdTG').html(createOptions(saData, 'Guid', 'Title'));
						$('#CostRulesIdTE').html(createOptions(saData, 'Guid', 'Title'));
                    }
                });
            },
            /**
             * 設定名單來源下拉選單
             */
            setListSourceDrop = function () {
                return g_api.ConnectLite('Customers_Upd', 'GetListSource', {}, function (res) {
                    if (res.RESULT) {
                        if (res.DATA.rel.length > 0) {
							sListSourceHtml = createOptions(res.DATA.rel, 'id', 'text').replace('<option value="">請選擇...</option>','<option value="">請選擇...</option><option value="SelfCome" title="自來">自來</option>') +
								'<option value="ImportFromDB" title="資料庫匯入">資料庫匯入</option>';
                            $('#ListSource').html(sListSourceHtml).val(saListSource).select2();
							$('#ListSource2').html(sListSourceHtml).val(saListSource).select2();
                        }
                        else {
                            $('#ListSource').html(createOptions([]));
							$('#ListSource2').html(createOptions([]));
                        }
						//$('#ListSource2').html($('#ListSource').html());
                    }
                });
            },
            /**
             * 上傳附件
             * @param {Array}files 要綁定的html物件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'Exhibition';
                option.type = 'one';
                option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
                option.limit = 1;
                option.parentid = oCurData.LogoFileId;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 拋轉文中
             */
            fnTransfer = function () {
                g_api.ConnectLite('Exhibition', 'Transfer', {
                    SN: oCurData.SN
                }, function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.Transfer_Success"), 'success'); // ╠message.Transfer_Success⇒拋轉完成╣
                        parent.msgs.server.pushTransfer(parent.OrgID, parent.UserID, oCurData.ExhibitionCode, 3);
                        fnGet();
                    }
                    else {
                        showMsg(i18next.t('message.Transfer_Failed') + '<br>' + res.MSG, 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Transfer_Failed"), 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                });
            },
            /**
             * 抓去參加展覽列表資料
             * @param  {String} flag 是否匯出
             * @return {Object} ajax物件
             */
            fnGetGridData = function (args) {
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetExhibitionList', {
                        SN: sDataId,
						sortField: args.sortField,
                        sortOrder: args.sortOrder,
                        Excel: args.Excel
                    }, function (res) {
                        if (res.RESULT) {
                            saGridData = res.DATA.rel;
                            if (args.Excel) {
                                DownLoadFile(saGridData);
                                layer.close(args.Index);
                            }
							intListCount = saGridData.length;
							$.each(saGridData, function (idx, data) {
								if(data.ListSource != "" && data.ListSource != null ){
									if($.inArray(data.ListSource, saListSource) < 0){
										saListSource.push(data.ListSource);
									}
									
								}
								
							});
							
							$("#ListSource").val(saListSource).trigger('change');
							$("#ListSource2").val(saListSource).trigger('change');
							
							saCustomerList = [];
                        }
                    });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 抓去參加展覽列表資料
             * @param  {String} flag 是否匯出
             * @return {Object} ajax物件
             */
            fnGetGridData1 = function (args) {
                var oQueryPm = {};
                $.extend(oQueryPm, oBaseQueryPm, args);

                if (sDataId) {
                    oQueryPm.SN = sDataId;
                    return g_api.ConnectLite(sProgramId, 'GetImportCustomers', oQueryPm);
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
			
			
			fnGetGridData2 = function (guid) {
                return g_api.ConnectLite('Contactors_Qry', 'QueryByCustomer', {
					CustomerId: guid
				}, function (res) {
					if (res.RESULT) {
						saGridData2 = res.DATA.rel;
					}
				});
            },
			fnGetGridData3 = function (_SN) {
                return g_api.ConnectLite(sProgramId, 'GetExhibitionList', {
					SN: _SN,
					Flag: ''
				}, function (res) {
					saGridData3 = res.DATA.rel;
				});
            },
			fnGetGridData4 = function (_SN) {
                return g_api.ConnectLite(sProgramId, 'GetExhibitionListFile', {
					SN: _SN,
					Flag: ''
				}, function (res) {
					saGridData3 = res.DATA.rel;
				});
            },
			
            /**
             * 刪除單筆列表資料
             * @param  {String} id 廠商id
             * @return {Object} ajax 物件
             */
            fnDelGrid1Data = function (id) {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        importcustomers: {
                            guid: id
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                        oGrid1.loadData();
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
             * 轉正式資料
             * @param  {Object} item 單筆廠商資料
             * @return {Object} ajax 物件
             */
            fnTransferToFormal = function (item) {
				parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + item.guid);
                /* layer.open({
                    type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                    area: ['1200px', '600px'],//寬度
                    shade: 0.75,//遮罩
                    maxmin: true, //开启最大化最小化按钮
                    id: 'layer_TransferToFormal', //设定一个id，防止重复弹出
                    anim: 0,//彈出動畫
                    btnAlign: 'c',//按鈕位置
                    content: '../Crm/Customers_Upd.html?Action=Add&From=Appoint&Flag=Pop',
                    success: function (layero, index) {
                        //layer.full(index); //弹出即全屏
                        var iframe = layero.find('iframe').contents();
                        iframe.find('#CustomerCName').val(item.CustomerCName);
                        iframe.find('#CustomerEName').val(item.CustomerEName);
                        iframe.find('#UniCode').val(item.UniCode);
                        iframe.find('#Telephone').val(item.Telephone);
                        iframe.find('#Email').val(item.Email);
                        iframe.find('#Address').val(item.Address);
                        iframe.find('#InvoiceAddress').val(item.InvoiceAddress);
                        iframe.find('body').attr('PopId', item.guid);
                        setTimeout(function () {
                            iframe.find('.plustemplkey ').trigger('click');
                            var _input = iframe.find('#table_box').find(':input');
                            _input.eq(0).val(item.Contactor);
                            _input.eq(2).val(item.Telephone);
                            _input.eq(5).val(item.Email);
                            setTimeout(function () {
                                _input.eq(0).trigger('click');
                                _input.eq(2).trigger('click');
                                _input.eq(5).trigger('click');
                            }, 500);
                        }, 1000);
                    },
                    end: function () {
                        if (bRefresh) {
                            bRefresh = false;
                            oGrid1.loadData();
                        }
                    }
                }); */
            },
            /**
            * 匯入廠商資料
            */
            fnImportCusList = function () {
                $('#importfile').val('').off('change').on('change', function () {
					$("#spnFileName").text(this.value.split('/').pop().split('\\').pop());
                    /* if (this.value.indexOf('.xls') > -1 || this.value.indexOf('.xlsx') > -1) {
                        var sFileId = guid(),
                            sFileName = this.value;
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
                            secureuri: false,
                            fileElementId: 'importfile',
                            success: function (data, status) {
                                g_api.ConnectLite(sProgramId, 'ImportCustomers', {
                                    FileId: sFileId,
                                    FileName: sFileName,
                                    SN: sDataId
                                }, function (res) {
                                    if (res.RESULT) {
                                        oGrid1.loadData();
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
                    } */
                }).click();
            },
            /**
             * 匯入廠商資料匯出
             */
            fnExportCusList = function () {
                g_api.ConnectLite(sProgramId, 'ExportCustomers', {
                    SN: sDataId
                }, function (res) {
                    if (res.RESULT) {
                        DownLoadFile(res.DATA.rel);
                    }
                });
            },
            /**
             * 新增匯入客戶
             * @param   {Object}data 序列化資料
             */
            fnAddCustomers = function (data) {
                data.ExhibitionNO = sDataId;
                g_api.ConnectLite(sProgramId, 'InsertImportCustomers', data, function (res) {
                    if (res.RESULT) {
                        oGrid1.loadData();
                        showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                });
            },
            /**
             * 修改資料
             * @param   {Object}data 序列化資料
             */
            fnUpdCustomers = function (data) {
                data = packParams(data, 'upd');
                delete data.Exhibitioname_TW;
                delete data.Exhibitioname_EN;
                data = removeNull(data);

                g_api.ConnectLite(sProgramId, 'UpdImportCustomers', data, function (res) {
                    if (res.RESULT) {
                        bRequestStorage = false;
                        showMsg(i18next.t("message.Modify_Success"), 'success'); // ╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error'); // ╠message.Modify_Failed⇒修改失敗╣
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
                        transLang(layero);
                    },
                    yes: function (index, layero) {
                        let sCustomerId = $('#CustomerId').val();
                        if (!sCustomerId) {
                            showMsg(i18next.t('message.SelectFormalCus'));//╠message.SelectFormalCus⇒請選擇對應的客戶╣
                            return false;
                        }
                        /* return g_api.ConnectLite('Exhibition_Upd', 'UpdateCustomerTag', {//匯入費用項目
                            PrevId: item.guid,
                            AfterId: sCustomerId
                        }, function (res) {
                            if (res.DATA.rel) {
                                oGrid1.loadData();
                                showMsg(i18next.t("message.Correspond_Success"), 'success'); //╠message.Correspond_Success⇒對應成功╣
                                layer.close(index);
                            }
                            else {
                                showMsg(i18next.t("message.Correspond_Failed"), 'error');//╠message.Correspond_Failed⇒對應失敗╣
                            }
                        }); */
						
						var combinedata = {};
						combinedata.Type = "2";
						combinedata.guid1 = item.guid;
						combinedata.guid2 = sCustomerId;
						
						g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
						, function (res) {
							if (res.DATA.rel) {
								oGrid.loadData();
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
                    }
                });
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
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.confirm("確定要匯出嗎？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                    fnGetGridData({
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

                        break;
                    case "Toolbar_Save":
                        $('#file_hidden').val($('li.jFiler-item').length || '');
                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return;
                        }
                        

                        if (sAction === 'Add') {
                            fnCheckNameThenAction('add');//fnAdd('add');
                        }
                        else {
                            fnCheckNameThenAction('upd');//fnUpd();
                        }

                        break;
                    case "Toolbar_ReAdd":

                        $('#file_hidden').val($('li.jFiler-item').length || '');
                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return;
                        }
                        fnCheckNameThenAction('readd'); // fnAdd('readd');

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

                        layer.confirm('確定要刪除 [ <b style="color:red;">展覽管理案件</b> ] 嗎', { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

                        break;
                    case "Toolbar_Imp":

                        break;
                    case "Toolbar_Transfer":

                        fnTransfer();

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
            * 目的 新增名單
            * @param
            */
            fnAddList = function () {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "新增名單", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['200px', '220px'],//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddList', //设定一个id，防止重复弹出
                    //offset: '50px',//右下角弹出
                    anim: 0,//彈出動畫
                    //btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<div class="pop-box">\
                             <p><button type="button" data-i18n="" id="btnAddListSingle" class="btn-custom w100p orange">單筆新增</button></p>\
                             <p><button type="button" data-i18n="" id="btnAddListFile" class="btn-custom w100p orange">檔案匯入</button></p>\
                             <p><button type="button" data-i18n="" id="btnAddListDB" class="btn-custom w100p orange">資料庫匯入</button></p>\
                         </div>',
                    success: function (layero, index) {
						$('#btnAddListSingle').click(function () {
							fnAddListSingle();
							layer.close(index);
							return false;
                        });
						$('#btnAddListFile').click(function () {
							fnAddListFile();
							layer.close(index);
							return false;
                        });
						$('#btnAddListDB').click(function () {
							fnAddListDB();
							layer.close(index);
							return false;
                        });
                    },
                    yes: function (index, layero) {
                    }
                });
            },
			/**
            * 目的 單筆新增
            * @param
            */
            fnAddListSingle = function () {
				sChooseCustomerId = "";
				bNewCustomer = false;
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "單筆新增", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddListSingle', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style><div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">客戶搜尋</span><span>：</span>\
								</label>\
								<div class="col-sm-4 text-left abb">\
									<select class="form-control w100p" data-type="select2" id="CustomerGuid" name="CustomerGuid"></select>\
								</div>\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">統一編號</span><span>：</span>\
								</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="UniCodeQry" id="UniCodeQry" placeholderid="" placeholder="">\
								</div>\
								<div class="col-sm-1">\
									<button type="button" data-i18n="" id="btnSetCustomerData" class="btn-custom w100p blue">帶入資料</button>\
								</div>\
							 </div>\
							 <hr>\
							 <form id="form_AddListSingle" class="form-horizontal">\
							 <div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default"><b class="t-red">*</b>\
									<span data-i18n="">客戶名稱</span><span>：</span>\
								</label>\
								<div class="col-sm-4">\
									<input type="text" class="form-control w100p" name="CustomerCName" id="CustomerCName" placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">統一編號</span><span>：</span>\
								</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="UniCode" id="UniCode" placeholderid="" placeholder="">\
								</div>\
							 </div>\
							 <div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">客戶英文名稱</span><span>：</span>\
								</label>\
								<div class="col-sm-4">\
									<input type="text" class="form-control w100p" name="CustomerEName" id="CustomerEName" placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label" for="input-Default"><b class="t-red">*</b>\
									<span data-i18n="">名單來源</span><span>：</span>\
								</label>\
								<div class="col-sm-3 text-left">\
									<select class="form-control w100p" data-type="select2" id="ListSourceA" name="ListSourceA"></select>\
								</div>\
							 </div>\
							 <div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">地址</span><span>：</span>\
								</label>\
								<div class="col-sm-9">\
									<input type="text" class="form-control w100p" name="Address" id="Address" placeholderid="" placeholder="">\
								</div>\
							 </div>\
							 <div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">官網地址</span><span>：</span>\
								</label>\
								<div class="col-sm-9">\
									<input type="text" class="form-control w100p" name="WebsiteAddress" id="WebsiteAddress" placeholderid="" placeholder="">\
								</div>\
							 </div>\
							 </form>\
							 <div class="pop-box row w100p">\
								<div class="col-sm-2 col-sm-offset-1">\
									<button type="button" data-i18n="" id="btnChooseContactors" class="btn-custom w100p blue" disabled>選擇聯絡人</button>\
								</div>\
								<div class="col-sm-2">\
									<button type="button" data-i18n="" id="btnCreateContactor" class="btn-custom w100p blue">新增聯絡人</button>\
								</div>\
							 </div>\
							 <div class="pop-box col-sm-10 col-sm-offset-1">\
								<div id="jsGridSingle"></div>\
							 </div>',
                    success: function (layero, index) {
						saContactorList = [];
						g_api.ConnectLite(Service.sys, 'GetAllCustomerlist', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'id', 'text');
                                $('#CustomerGuid').html(sOptions).select2();
                            }
                        });
						$('#ListSourceA').html(sListSourceHtml.replace('<option value="ImportFromDB" title="資料庫匯入">資料庫匯入</option>','')).select2();
						/* g_api.ConnectLite('Customers_Upd', 'GetListSource', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                                $('#ListSourceA').html(sOptions).select2();
                            }
                        }); */
						$('#btnSetCustomerData').click(function () {
							sChooseCustomerId = "";
							if($('#CustomerGuid').val() != "" || $('#UniCodeQry').val() != ""){
								saContactorList = [];
								g_api.ConnectLite(sProgramId, 'CustomerQuery', {
									guid: $('#CustomerGuid').val(),
									unicode: $('#UniCodeQry').val()
								}, function (res) {
									if (res.RESULT) {
										bNewCustomer = true;
										var oRes = res.DATA.rel;
										if(oRes != null){
											sChooseCustomerId = oRes.guid;
										
											$("#UniCode").val(oRes.UniCode);
											$("#CustomerCName").val(oRes.CustomerCName);
											$("#CustomerEName").val(oRes.CustomerEName);
											$("#Address").val(oRes.Address);
											$("#WebsiteAddress").val(oRes.WebsiteAdress);
											
											$("#CustomerCName").prop("disabled", true);
											$("#CustomerEName").prop("disabled", true);
											$("#UniCode").prop("disabled", true);
											$("#Address").prop("disabled", true);
											$("#WebsiteAddress").prop("disabled", true);
											
											$("#btnChooseContactors").prop("disabled", false);
										} else {
											showMsg(i18next.t('message.NotFindData'), 'error'); // ╠message.NotFindData⇒查不到對應的資料╣
											
											$("#UniCode").val("");
											$("#CustomerCName").val("");
											$("#CustomerEName").val("");
											$("#Address").val("");
											$("#WebsiteAddress").val("");
											
											$("#CustomerCName").prop("disabled", false);
											$("#CustomerEName").prop("disabled", false);
											$("#UniCode").prop("disabled", false);
											$("#Address").prop("disabled", false);
											$("#WebsiteAddress").prop("disabled", false);
											
											$("#btnChooseContactors").prop("disabled", true);
										}
									}
									else {
										showMsg(i18next.t('message.NotFindData'), 'error'); // ╠message.NotFindData⇒查不到對應的資料╣
									}
								}, function () {
									showMsg(i18next.t('message.NotFindData'), 'error');
								});
								
								oGrid2.loadData();
							}
                        });
						$("#jsGridSingle").jsGrid({
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
							rowClick: function (args) {
								if (navigator.userAgent.match(/mobile/i)) {
									goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
								}
							},
							rowDoubleClick: function (args) {
								parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
							},
							fields: [
								{
									name: "RowIndex", title: 'common.RowNumber', width: 5, align: "center"
								},
								{
									name: "ContactorName", title: 'common.Contactor', width: 25, align: "center"
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
									return {
										data: saContactorList,
										itemsCount: saContactorList.length //data.length
									};
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
						$('#btnCreateContactor').click(function () {
							fnCreateContactor();
							return false;
                        });
						$('#btnChooseContactors').click(function () {
							fnChooseContactors(sChooseCustomerId);
							return false;
                        });
                    },
                    yes: function (index, layero) {
						let data = getFormSerialize($("#form_AddListSingle"))
						
						if(data.CustomerCName == ""){
							showMsg("客戶名稱不可為空");
							return false;
						} else if( $("#ListSourceA").val() == "" ){
							showMsg("名單來源不可為空");
							return false;
						} else {
							data.Contactors = saContactorList;
							data.AddContactors = saAddContactorList;
							data.ChooseContactors = saChooseContactorList;
							if(bNewCustomer){
								data.NewCustomer = "true";
							}else{
								data.NewCustomer = "false";
							}
							data.CustomerId = sChooseCustomerId;
							data.OrgID = parent.OrgID;
							data.ExhibitionNO = sDataId;
							data.SourceType = "1";
							data.ListSource = $("#ListSourceA").val();
							
							g_api.ConnectLite(sProgramId, 'InsertExhibitionListSingle', data, function (res) {
								if (res.RESULT) {
									//oGrid.loadData();
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
                    },
					end: function () {
						saContactorList = [];
						saChooseContactorList = [];
						saAddContactorList = [];
						oGrid.loadData();
					}
                });
            },
			/**
            * 目的 選擇聯絡人
            * @param
            */
            fnChooseContactors = function (_CustomerId) {
				saContactorList = [];
				saChooseContactorList = [];
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
                    content: '<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">客戶名稱</span><span>：</span>\
								</label>\
								<div class="col-sm-4">\
									<input type="text" class="form-control w100p" name="CustomerCNameA" id="CustomerCNameA" placeholderid="" placeholder="" disabled>\
								</div>\
								<label class="col-sm-2 control-label" for="input-Default">\
									<span data-i18n="">統一編號</span><span>：</span>\
								</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="UniCodeA" id="UniCodeA" placeholderid="" placeholder="" disabled>\
								</div>\
							 </div>\
							 <div class="pop-box col-sm-10 col-sm-offset-1">\
								<div id="jsGridChooseContactors"></div>\
							 </div>',
                    success: function (layero, index) {
						/* g_api.ConnectLite('Contactors_Qry', 'QueryPage', {}, function (res) {
                            if (res.RESULT) {
                                //var saList = res.DATA.rel;
                                saGridDataB = res.DATA.rel;
								oGrid2.loadData();
                            }
                        }); */
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
							rowClick: function (args) {
								/* if (navigator.userAgent.match(/mobile/i)) {
									goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
								} */
							},
							rowDoubleClick: function (args) {
								/* parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid); */
							},
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
									name: "Ext1", title: 'common.EXT', width: 10
								},
								{
									name: "Email1", title: 'common.Email', width: 40
								}
							],
							controller: {
								loadData: function (args) {
									return fnGetGridData2(_CustomerId);
								},
								insertItem: function (args) {
								},
								updateItem: function (args) {
								},
								deleteItem: function (args) {
								}
							}
							/* onInit: function (args) {
								oGrid2 = args.grid;
							} */
						});
						/* g_api.ConnectLite('Contactors_Qry', 'QueryPage', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                saGridDataB = res.DATA.rel;
								oGrid2.loadData();
                            }
                        }); */

						//$('#btnChooseContactors').click(function () {
							//fnAddEEE();
							//fnCombineCustomer('0314d83b-b003-45cc-8c3d-7148bca65acf','0424389d-3f11-4cba-bcd8-dcd9cb560ab3');
							//return false;
                        //});
                    },
                    yes: function (index, layero) {
						saChooseContactorList = saContactorList;
						if(saAddContactorList.length >0){
							let i = saContactorList.length + 1;
							$.each(saAddContactorList, function (idx, data) {
								data.RowIndex = i + idx;
								saContactorList.push(data);
							});
						}
						
						oGrid2.loadData();
						layer.close(index);
                    }
                });
            },
			/**
            * 目的 新增聯絡人
            * @param
            */
            fnCreateContactor = function () {
				//saContactorList = [];
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "新增聯絡人", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.01,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_CreateContactor', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<form id="form_CreateContactor" class="form-horizontal">\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><b class="t-red">*</b><span data-i18n="">稱呼</span>：</label>\
								<div class="col-sm-3">\
									<select class="form-control w100p" id="Call" name="Call" placeholderid="" placeholder="">\
										<option value="1">Mr.</option>\
										<option value="2">Miss.</option>\
									</select>\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><b class="t-red">*</b><span data-i18n="">姓名</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="ContactorName" id="ContactorName"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">暱稱</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="NickName" id="NickName"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">生日</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control date-picker w100p" name="Birthday" id="Birthday" maxlength="10">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">婚姻狀況</span>：</label>\
								<div class="col-sm-3">\
									<select class="form-control w100p" id="MaritalStatus" name="MaritalStatus" placeholderid="" placeholder="">\
										<option value="1">未婚</option>\
										<option value="2">已婚</option>\
									</select>\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">私人行動電話</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="PersonalMobilePhone" id="PersonalMobilePhone"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">私人郵箱</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="PersonalEmail" id="PersonalEmail"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">LINE</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="LINE" id="LINE"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">WECHAT</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="WECHAT" id="WECHAT"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">個性</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="Personality" id="Personality"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">喜好</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="Preferences" id="Preferences"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">私人地址</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="PersonalAddress" id="PersonalAddress"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="common.Memo">備註</span>：</label>\
								<div class="col-sm-8">\
									<textarea name="Memo" id="Memo" class="form-control" rows="6" cols="20"></textarea>\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">直屬上司</span>：</label>\
								<div class="col-sm-3  text-left">\
									<select class="form-control w100p" data-type="select2" id="ImmediateSupervisor" name="ImmediateSupervisor" data-msg="" datamsg="請選擇直屬上司"></select>\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">職稱</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="JobTitle" id="JobTitle"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">部門</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="Department" id="Department"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">電子郵件1</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="Email1" id="Email1"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">電子郵件2</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="Email2" id="Email2"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">電話1</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="Telephone1" id="Telephone1"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">分機1</span>：</label>\
								<div class="col-sm-1">\
									<input type="text" class="form-control w100p" name="Ext1" id="Ext1"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">電話2</span>：</label>\
								<div class="col-sm-3">\
									<input type="text" class="form-control w100p" name="Telephone2" id="Telephone2"  placeholderid="" placeholder="">\
								</div>\
								<label class="col-sm-2 control-label"><span data-i18n="">分機2</span>：</label>\
								<div class="col-sm-1">\
									<input type="text" class="form-control w100p" name="Ext2" id="Ext2"  placeholderid="" placeholder="">\
								</div>\
							</div>\
							<div class="pop-box row w100p">\
								<label class="col-sm-2 control-label"><span data-i18n="">選擇我們的原因</span>：</label>\
								<div class="col-sm-8">\
									<input type="text" class="form-control w100p" name="ChoseReason" id="ChoseReason"  placeholderid="" placeholder="">\
								</div>\
							</div><div class="pop-box row w100p"></div>\
							</form>',
                    success: function (layero, index) {
						$("#Birthday").datepicker({
							changeYear: true,
							changeMonth: true,
							altFormat: 'yyyy/MM/dd'
						});
						if(sChooseCustomerId != ""){
							g_api.ConnectLite("Contactors_Upd", "GetImmediateSupervisor", {
								Guid: '',
								CustomerId: sChooseCustomerId
							}, function (res) {
								if (res.RESULT) {
									let saState = res.DATA.rel;
									
									if (saState.length > 0) {
										$('#ImmediateSupervisor').html(createOptions(saState, 'id', 'text', false)).select2();
									}
								}
							});
						} else {
							$('#ImmediateSupervisor').html(createOptions([], 'id', 'text', false)).select2();
						}
                    },
                    yes: function (index, layero) {
						let data = getFormSerialize($("#form_CreateContactor"))
						
						let bCheck = true;
						
						if(sChooseCustomerId != ""){
							data.CustomerId = sChooseCustomerId;
							g_api.ConnectLite('Customers_Upd', 'checkContactorName', data, function (res) {
								if (!res.RESULT) {
									showMsg(res.MSG, 'error');
									//return false;
								} else {
									$.each(saAddContactorList, function (idx, Adddata) {
										if (Adddata.ContactorName == data.ContactorName) {
											showMsg('已有重複姓名之聯絡人，請重新輸入', 'error');
											bCheck = false;
										}
									});
									
									if(bCheck){
										data.RowIndex = saContactorList.length + 1;
										saAddContactorList.push(data);
										saContactorList.push(data);
										oGrid2.loadData();
										layer.close(index);
									}
								}
							}, function () {
								showMsg('檢查失敗', 'error');
								//return false;
							});
							
						} else {
							$.each(saAddContactorList, function (idx, Adddata) {
								if (Adddata.ContactorName == data.ContactorName) {
									showMsg('已有重複姓名之聯絡人，請重新輸入', 'error');
									bCheck = false;
								}
							});
							
							if(bCheck){
								data.RowIndex = saContactorList.length + 1;
								saAddContactorList.push(data);
								saContactorList.push(data);
								oGrid2.loadData();
								layer.close(index);
							}
						}
                    }
                });
            },
			/**
            * 目的 檔案匯入
            * @param
            */
            fnAddListFile = function () {
				layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "檔案匯入", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['800px', '300px'],//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddListFile', //设定一个id，防止重复弹出
                    offset: '20px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style>\
							<div class="pop-box w100p row">\
								<label class="col-sm-2  col-sm-offset-2 control-label" for="input-Default">\
									<span data-i18n="">名單來源</span><span>：</span>\
								</label>\
								<div class="col-sm-4 text-left">\
									<select class="form-control w100p" data-type="select" id="ListSourceA" name="ListSourceA"></select>\
								</div>\
								<div class="col-sm-2">\
									<button type="button" data-i18n="" id="btnDownloadTemplate" class="btn-custom w100p orange">名單表格下載</button>\
								</div>\
							</div>\
							<div class="pop-box w100p">\
								<label class="control-label" for="input-Default">\
									<span data-i18n="">已選擇檔案名稱</span><span>：</span>\
									<span style="color:blue" id="spnFileName"></span>\
								</label>\
							</div>\
							<div class="pop-box w100p">\
								<div class="">\
									<button type="button" data-i18n="" id="btnChooseFileA" class="btn-custom w15p orange">選擇檔案</button>\
								</div>\
							</div>',
                    success: function (layero, index) {
						$('#ListSourceA').html(sListSourceHtml.replace('<option value="ImportFromDB" title="資料庫匯入">資料庫匯入</option>','')).select2();
						/* g_api.ConnectLite('Customers_Upd', 'GetListSource', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                                $('#ListSourceA').html(sOptions).select2();
                            }
                        }); */
						$('#btnChooseFileA').click(function () {
							fnImportExhibitionList();
							return false;
						});
						$('#btnDownloadTemplate').click(function () {
							fnDownloadTemplate();
							return false;
						});
                    },
					yes: function (index, layero) {
						if($("#ListSourceA").val() == ""){
							showMsg('請選擇名單來源');
							return false;
						} else {
							var sValue = $('#importfile').val();
							if (sValue.indexOf('.xls') > -1 || sValue.indexOf('.xlsx') > -1) {
								var sFileId = guid(),
									sFileName = sValue;
								$.ajaxFileUpload({
									url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
									secureuri: false,
									fileElementId: 'importfile',
									success: function (data, status) {
										g_api.ConnectLite(sProgramId, 'ImportExhibitionList', {
											FileId: sFileId,
											FileName: sFileName,
											SN: sDataId
										}, function (res) {
											if (res.RESULT) {
												//oGrid1.loadData();
												fnAddListFileConfirm($("#ListSourceA").val());
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
						}
						
						//fnAddListDBConfirm();
                    }
                });
            },
			/**
            * 匯入展覽名單
            */
            fnImportExhibitionList = function () {
                $('#importfile').val('').off('change').on('change', function () {
					$("#spnFileName").text(this.value.split('/').pop().split('\\').pop());
                }).click();
            },
			/**
            * 目的 檔案匯入的資料比對與選擇新增方式
            * @param
            */
            fnAddListFileConfirm = function (_ListSource) {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "新增名單", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.50,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddListConfirm', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    //btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.glyphicon{color:orange!important;}</style><div class="pop-box w100p">\
								<div class="col-sm-2 col-sm-offset-3 text-right">\
									<i class="glyphicon glyphicon-exclamation-sign " title="加入不重複的公司及不重複的連絡人"></i>\
								</div>\
								<div class="col-sm-2  text-right">\
									<i class="glyphicon glyphicon-exclamation-sign " title="更新重複公司的公司及連絡人資訊"></i>\
								</div>\
								<div class="col-sm-2 text-right">\
									<i class="glyphicon glyphicon-exclamation-sign " title="加入不重複的公司並更新重複公司的公司及連絡人資訊"></i>\
							</div>\
							<div class="pop-box w100p">\
								<div class="col-sm-2 col-sm-offset-3">\
									<button type="button" data-i18n="" id="btnAddListFileInsert" class="btn-custom w100p orange" title="加入不重複的公司及不重複的連絡人">新增名單</button>\
								</div>\
								<div class="col-sm-2">\
									<button type="button" data-i18n="" id="btnAddListFileUpdate" class="btn-custom w100p orange" title="更新重複公司的公司及連絡人資訊">更新現有名單</button>\
								</div>\
								<div class="col-sm-2">\
									<button type="button" data-i18n="" id="btnAddListFileInsertUpdate" class="btn-custom w100p orange" title="加入不重複的公司並更新重複公司的公司及連絡人資訊">新增並更新現有名單</button>\
								</div>\
							</div>\
						<div class="pop-box col-sm-12">\
							<div id="jsGridDataCompare"></div>\
						</div>',
                    success: function (layero, index) {
						if(intListCount == 0){
							$("#btnAddListFileUpdate").prop("disabled", true);
							$("#btnAddListFileInsertUpdate").prop("disabled", true);
						}else{
							$("#btnAddListFileUpdate").prop("disabled", false);
							$("#btnAddListFileInsertUpdate").prop("disabled", false);
						}
						
						$("#jsGridDataCompare").jsGrid({
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
							//pageSize: parent.SysSet.GridRecords || 10,
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
								/* parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid); */
							},
							fields: [
								{
									name: "RowIndex", title: 'common.RowNumber', type: "text", width: 30, align: "center", sorting: false
								},
								{
									name: "CustomerCName", title: 'Customers_Upd.CustomerCName', width: 200
								},
								{
									name: "CustomerEName", title: 'Customers_Upd.CustomerEName', width: 200
								},
								{
									name: "Telephone", title: 'common.Telephone', width: 150
								},
								{
									name: "FAX", title: 'common.FAX', width: 150
								},
								{
									name: "Address", title: 'common.Address', width: 200
								}
							],
							controller: {
								loadData: function (args) {
									return fnGetGridData4(sDataId);
								},
								insertItem: function (args) {
								},
								updateItem: function (args) {
								},
								deleteItem: function (args) {
								}
							}
						});
						
						$('#btnAddListFileInsert').click(function () {
							fnGoAddListFileConfirm(index,"insert",_ListSource);
							return false;
						});
						$('#btnAddListFileUpdate').click(function () {
							fnGoAddListFileConfirm(index,"update",_ListSource);
							return false;
						});
						$('#btnAddListFileInsertUpdate').click(function () {
							fnGoAddListFileConfirm(index,"insertupdate",_ListSource);
							return false;
						});
                    }
                });
            },
			/**
            * 目的 寫入新增名單-檔案匯入
            * @param
            */
			fnGoAddListFileConfirm = function (_index, _type, _ListSource) {
				g_api.ConnectLite('Exhibition_Upd', 'AddUpdateListFile', {Type:_type,SN:sDataId,ListSource:_ListSource}
				, function (res) {
					if (res.DATA.rel) {
						sSN = '';
						oGrid.loadData();
						showMsg("匯入成功", 'success');
						layer.close(_index);
						layer.close(_index-1);
					}
					else {
						showMsg("匯入失敗" + '<br>' + res.MSG , 'error');
					}
				}
				, function () {
					showMsg("匯入失敗", 'error');
				});
            },
			/**
            * 目的 名單表格下載
            * @param
            */
			fnDownloadTemplate = function () {
                return  g_api.ConnectLite(sProgramId, 'DownloadTemplate', '', function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        DownLoadFile(oRes.FilePath, oRes.FileName);
                    }
                });
            },
			/**
            * 目的 資料庫匯入
            * @param
            */
            fnAddListDB = function () {
				layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "資料庫匯入", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddListDB', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style>\
							<div class="pop-box row w100p">\
								<div class="col-sm-1 col-sm-offset-5">\
									<button type="button" data-i18n="" id="btnAddListDBQuery" class="btn-custom w100p orange">查詢</button>\
								</div>\
								<div class="col-sm-1">\
									<button type="button" data-i18n="" id="btnAddListDBClear" class="btn-custom w100p orange">清除</button>\
								</div>\
							</div>\
							<div class="pop-box row w100p form-AddListDB">\
								<label class="col-sm-1 control-label" for="input-Default">\
									<span data-i18n="Exhibition_Upd.ExhibitionCode">專案代號</span><span>：</span>\
								</label>\
								<div class="col-sm-2">\
									<input type="text" maxlength="50" id="ExhibitionCodeA" name="ExhibitionCodeA" class="form-control w100p" placeholderid="Exhibition_Qry.Instruction_ExhibitionCode">\
								</div>\
								<label class="col-sm-1 control-label" for="input-Default">\
									<span data-i18n="Exhibition_Upd.Exhibitioname_TW">展覽名稱</span><span>：</span>\
								</label>\
								<div class="col-sm-2">\
									<input type="text" maxlength="50" id="ExhibitionameA" name="ExhibitionameA" class="form-control w100p" placeholderid="Exhibition_Qry.Instruction_Exhibitioname_TW">\
								</div>\
								<label class="col-sm-1 control-label" for="input-Default">\
									<span data-i18n="Exhibition_Upd.Industry">產業別</span><span>：</span>\
								</label>\
								<div class="col-sm-2">\
									<select class="form-control w100p" data-type="select2" id="IndustryA" name="IndustryA"></select>\
								</div>\
								<label class="col-sm-1 control-label" for="input-Default">\
									<span data-i18n="Exhibition_Upd.State">國家</span><span>：</span>\
								</label>\
								<div class="col-sm-2">\
									<select class="form-control w100p" data-type="select2" id="StateA" name="StateA"></select>\
								</div>\
							</div>\
							<div class="pop-box col-sm-12">\
								<div id="jsGridChooseExhibition"></div>\
							</div>',
                    success: function (layero, index) {
						sSN = '';
						
						if (saState.length > 0) {
							$('#StateA').html(createOptions(saState, 'id', 'text', true));
						}
						if(sIndustryHtml.length > 0){
							$("#IndustryA").html(sIndustryHtml);
						}
						$("#jsGridChooseExhibition").jsGrid({
							width: "100%",
							height: "auto",
							autoload: true,
							filtering: false,
							pageLoading: true,
							inserting: false,
							editing: false,
							sorting: true,
							paging: true,
							pageIndex: 1,
							pageSize: 10,
							pageButtonCount: parent.SysSet.GridPages || 15,
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
								/* parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid); */
							},
							fields: [
								{
									name: "RowIndex", title: 'common.RowNumber', width: 20, align: "center",
									itemTemplate: function (value, item) {
										return $("<input>", {
											type: 'checkbox', click: function (e) {
												e.stopPropagation();
												if (this.checked) {
													$("#jsGridChooseExhibition").find('[type=checkbox]').each(function () {
														this.checked = false;
													});
													this.checked = true;
													sSN = item.SN;
													//saExhibitionList = [];
													//saExhibitionList.push(item);
												}
											}
										});
									}
								},
								{
									name: "ExhibitionCode", title: 'Exhibition_Upd.ExhibitionCode', type: "text", width: 60
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
								}
							],
							controller: {
								loadData: function (args) {
									return fnQueryExhibition(args.pageIndex);
								},
								insertItem: function (args) {
								},
								updateItem: function (args) {
								},
								deleteItem: function (args) {
								}
							},
							onInit: function (args) {
								oGrid3 = args.grid;
							}
						});
						$('#btnAddListDBQuery').click(function () {
							fnQueryExhibition(args.pageIndex);
							oGrid3.loadData();
							return false;
						});
						$('#btnAddListDBClear').click(function () {
							$("#ExhibitionCodeA").val('');
							$("#ExhibitionameA").val('');
							$("#StateA").val('');
							$("#IndustryA").val('');
							return false;
						});
						$('.form-AddListDB input').keypress(function (e) {
							if (e.which == 13) {
								fnQueryExhibition(args.pageIndex);
								oGrid3.loadData();
							}
						});
                    },
					yes: function (index, layero) {
						fnAddListDBConfirm();
                    }
                });
            },
			/**
            * 目的 資料庫匯入-展覽資料
            * @param
            */
			fnQueryExhibition = function (_pageIndex) {
				return g_api.ConnectLite('Exhibition_Qry', 'QueryPage', 
					{
						pageIndex:_pageIndex,
						pageSize:10,
						sortField: 'CreateDate',
						sortOrder: 'desc',
						Excel: false,
						ExhibitionCode: $("#ExhibitionCodeA").val(),
						Exhibitioname: $("#ExhibitionameA").val(),
						State: $("#StateA").val(),
						IsShowWebSite: 'YN',
						Effective : 'Y',
						IsTransfer : 'Y',
						Industry: $("#IndustryA").val(),
					}, 
					function (res) {
						if (res.RESULT) {
							var oRes = res.DATA.rel;
						}
					}
				);
            },
			/**
            * 目的 資料庫匯入的資料比對與選擇新增方式
            * @param
            */
            fnAddListDBConfirm = function () {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "新增名單", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.50,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_AddListConfirm', //设定一个id，防止重复弹出
                    offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
                    //btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.glyphicon{color:orange!important;}</style><div class="pop-box w100p">\
								<div class="col-sm-2 col-sm-offset-5 text-right">\
									<i class="glyphicon glyphicon-exclamation-sign " title="加入不重複的公司及不重複的連絡人"></i>\
								</div>\
							</div>\
							<div class="pop-box w100p">\
								<div class="col-sm-2 col-sm-offset-5">\
									<button type="button" data-i18n="" id="btnAddListDBInsert" class="btn-custom w100p orange" title="加入不重複的公司及不重複的連絡人">新增名單</button>\
								</div>\
							</div>\
						<div class="pop-box col-sm-12">\
							<div id="jsGridDataCompare"></div>\
						</div>',
                    success: function (layero, index) {
						/* if(intListCount == 0){
							$("#btnAddListDBUpdate").prop("disabled", true);
							$("#btnAddListDBInsertUpdate").prop("disabled", true);
						} else {
							$("#btnAddListDBUpdate").prop("disabled", false);
							$("#btnAddListDBInsertUpdate").prop("disabled", false);
						} */
						$("#jsGridDataCompare").jsGrid({
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
							//pageSize: parent.SysSet.GridRecords || 10,
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
								/* parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid); */
							},
							fields: [
								{
									name: "RowIndex", title: 'common.RowNumber', type: "text", width: 30, align: "center", sorting: false
								},
								{
									name: "CustomerCName", title: 'Customers_Upd.CustomerCName', width: 200
								},
								{
									name: "CustomerEName", title: 'Customers_Upd.CustomerEName', width: 200
								},
								{
									name: "Telephone", title: 'common.Telephone', width: 150
								},
								{
									name: "FAX", title: 'common.FAX', width: 150
								},
								{
									name: "Address", title: 'common.Address', width: 200
								}
							],
							controller: {
								loadData: function (args) {
									return fnGetGridData3(sSN);
								},
								insertItem: function (args) {
								},
								updateItem: function (args) {
								},
								deleteItem: function (args) {
								}
							}
						});
						
						$('#btnAddListDBInsert').click(function () {
							fnGoAddListDBConfirm(index,"insert",sSN);
							return false;
						});
						/* $('#btnAddListDBUpdate').click(function () {
							fnGoAddListDBConfirm(index,"update",sSN);
							return false;
						});
						$('#btnAddListDBInsertUpdate').click(function () {
							fnGoAddListDBConfirm(index,"insertupdate",sSN);
							return false;
						}); */
                    }
                });
            },
			/**
            * 目的 寫入新增名單-資料庫匯入
            * @param
            */
			fnGoAddListDBConfirm = function (_index, _type, _sSN) {
				g_api.ConnectLite('Exhibition_Upd', 'AddUpdateList', {Type:_type,SN:sDataId,ChooseSN:_sSN}
				, function (res) {
					if (res.DATA.rel) {
						sSN = '';
						oGrid.loadData();
						showMsg("匯入成功", 'success');
						layer.close(_index);
						layer.close(_index-1);
					}
					else {
						showMsg("匯入失敗", 'error');
					}
				}
				, function () {
					showMsg("匯入失敗", 'error');
				});
            },
			/**
            * 目的 合併客戶
            * @param
            */
            fnCombineCustomer = function (aguid1,aguid2) {
				g_api.ConnectLite('Customers_Upd', 'CheckCombineCustomer', {guid1:aguid1,guid2:aguid2}
				, function (res) {
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
										guid1: aguid1,
										guid2 : aguid2
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
										var combinedata = getFormSerialize($(iframe.find('#form_main')));
										combinedata.Type = res.DATA.rel;
										combinedata.guid1 = aguid1;
										combinedata.guid2 = aguid2;
										
										g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
										, function (res) {
											if (res.DATA.rel) {
												saCustomerList = [];
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
									if (bRefresh) {
										bRefresh = false;
										oGrid.loadData();
									}
								}
							});
						} else if(res.DATA.rel == 2){
							layer.confirm("正式客戶合併非正式客戶，確定要合併？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
								var combinedata = {};
								combinedata.Type = res.DATA.rel;
								combinedata.guid1 = aguid1;
								combinedata.guid2 = aguid2;
								
								g_api.ConnectLite('Customers_Upd', 'CombineCustomer', combinedata
								, function (res) {
									if (res.DATA.rel) {
										saCustomerList = [];
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
				
				
                
            },
			/**
            * 目的 從名單移除
            * @param
            */
            fnRemoveFromList = function (guids) {
				layer.confirm("確定要從名單移除？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
					 g_api.ConnectLite(sProgramId, 'RemoveFromList', {
						SN: sDataId,
						Guid: guids
					}, function (res) {
						if (res.DATA.rel) {
							showMsg("移除成功", 'success');
						}
						else {
							showMsg("移除失敗", 'error');
						}
						saCustomerList = [];
						oGrid.loadData();
					}, function () {
						showMsg("移除失敗", 'error');
					});

					layer.close(index);
				});
                
            },
			/**
            * 目的 匯入帳單系統
            * @param
            */
            fnImportExhibitors = function (_strCustomerGuid) {
				layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "匯入帳單系統", // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: ['400px', '350px'],//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_ImportExhibitors', //设定一个id，防止重复弹出
                    //offset: '50px',//右下角弹出
                    anim: 0,//彈出動畫
                    //btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style>\
							<div class="pop-box">\
								<input id="radio_0" type="radio" name="rdbChoose01" value="1" checked="checked" />\
								<label for="radio_0" >出口作業</label>\
								<input id="radio_1" type="radio" name="rdbChoose01" value="2" />\
								<label for="radio_1" >展覽服務部</label></div>\
							<div class="pop-box" id="div1">\
								<button type="button" data-i18n="" id="btnAddNewCase" class="btn-custom w50p orange">建立新案件-出口</button></p>\
								<hr>\
								<div><select class= "form-control" id="ExhibitionExportGuid" name="ExhibitionExportGuid"></select></div></p>\
								<button type="button" data-i18n="" id="btnImportCase" class="btn-custom w50p orange">加入原有案件-出口</button>\
							</div>\
							<div class="pop-box" id="div2">\
								<button type="button" data-i18n="" id="btnAddNewCaseTG" class="btn-custom w50p orange">建立新案件-展覽服務部</button></p>\
								<hr>\
								<div><select class= "form-control" id="ExhibitionExportGuidTG" name="ExhibitionExportGuidTG" style="width:100%"></select></div></p>\
								<button type="button" data-i18n="" id="btnImportCase" class="btn-custom w50p orange">加入原有案件-展覽服務部</button>\
							</div>',
                    success: function (layero, index) {
						$("#div2").hide();
						g_api.ConnectLite(Service.sys, 'GetExhibitionExportlist', {Type: 'TE'}, function (res) {
                            if (res.RESULT) {
                                var saList2 = res.DATA.rel;
                                var sOptions2 = createOptions(saList2, 'id', 'text');
                                $('#ExhibitionExportGuid').html(sOptions2).select2();
                            }
                        });
						g_api.ConnectLite(Service.sys, 'GetExhibitionExportlist', {Type: 'TG'}, function (res) {
                            if (res.RESULT) {
                                var saList2 = res.DATA.rel;
                                var sOptions2 = createOptions(saList2, 'id', 'text');
                                $('#ExhibitionExportGuidTG').html(sOptions2).select2();
                            }
                        });
						
						$('[name=rdbChoose01]').change(function () {
							if($('[name=rdbChoose01]:checked').val() == "1") {
								$("#div1").show();
								$("#div2").hide();
							} else {
								$("#div2").show();
								$("#div1").hide();
							}
                        });
						$('#btnAddNewCase').click(function () {
							parent.openPageTab('ExhibitionExport_Upd', '?Action=Add&GoTab=2&ExhibitionNO=' + sDataId +'&NewCustomers=' + _strCustomerGuid);
							layer.close(index);
                        });
						$('#btnAddNewCaseTG').click(function () {
							parent.openPageTab('OtherExhibitionTG_Upd', '?Action=Add&GoTab=2&ExhibitionNO=' + sDataId +'&NewCustomers=' + _strCustomerGuid);
							layer.close(index);
                        });
						$('#btnImportCase').click(function () {
							if ($('#ExhibitionExportGuid').val() == '') {
								showMsg("請選擇案件");
								return false;
							} else {
								if(parent.OrgID == "TE"){
									parent.openPageTab('ExhibitionExport_Upd', '?Action=Upd&GoTab=2&ExportBillNO=' + $('#ExhibitionExportGuid').val()+'&ExhibitionNO=' + sDataId +'&NewCustomers=' + _strCustomerGuid);
								} else if(parent.OrgID == "TG") {
									parent.openPageTab('OtherExhibitionTG_Upd', '?Action=Upd&GoTab=2&Guid=' + $('#ExhibitionExportGuid').val()+'&ExhibitionNO=' + sDataId +'&NewCustomers=' + _strCustomerGuid);
								}
							layer.close(index);
							}
							
                        });
                    },
                    end: function () {
                        if (bRefresh) {
                            bRefresh = false;
                            oGrid.loadData();
                        }
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
                    $('#CostRulesIdTG,#State,#ExhibitionAddress,#ExhibitionDate,#file_hidden').prop('required', true);
                    $('#notte').show();
					$('.showwebsiteTG').show();
					$('.showwebsite').show();
                    $('.simp-box').hide();
                } else if (parent.OrgID === 'TE'){
					$('#CostRulesIdTE,#SeaReceiveingDate,#SeaClosingDate,#AirReceiveingDate,#AirClosingDate,#Undertaker,#Telephone,#Email,#file_hidden').prop('required', true);
					$('#notte').show();
					$('.showTE').show();
					$('.showwebsiteTE').show();
					$('.showwebsite').show();
				} else {
					$('#notte').hide();
                    $('.costrules').hide();
                }
                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'Toolbar_Transfer',
                        value: 'common.Toolbar_Transfer'// ╠common.Toolbar_Transfer⇒拋轉╣
                    }];
                }
				
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true//,
                    /* tabClick: function (el) {
                        switch (el.id) {
                            case 'litab4':
                                if (!$(el).data('action')) {
                                    fnGetGridData().done(function () {
                                        oGrid.loadData();
                                        if (saGridData.length > 0 && (parent.UserInfo.roles.indexOf('Admin') > -1 || parent.UserInfo.roles.indexOf('Manager') > -1)) {
                                            $('.export').show();
                                            $('#Export_CusList').on('click', function () {
                                                fnGetGridData('export');
                                            });
                                        }
                                    });
                                }
                                break;
                            case 'litab3':
                                if (!$(el).data('action')) {
                                    oGrid1.loadData();
                                }
                                break;
                        }
                        $(el).data('action', true);
                    } */
                });

                $.whenArray([
                    fnGet(),
                    setStateDrop(),
                    setExhibitionAddressDrop(),
                    setCostRulesDrop(),
					setListSourceDrop(),
                    fnSetArgDrop([
                        {
                            OrgID: 'TE',
                            ArgClassID: 'ExhibClass',
                            //Select: $('#Industry'),
                            ShowId: true,
							CallBack: function (data) {
								sIndustryHtml = createOptions(data, 'id', 'text', true);
								$('#Industry').html(sIndustryHtml);
								$('#Industry2').html(sIndustryHtml);
                                //$('#Industry').html(createOptions(data, 'id', 'text'))[0].remove(0);
                            }
                        },
						{
							OrgID: 'TE',
							ArgClassID: 'BlackListReason',
							CallBack: function (data) {
								sBlackListReasonHtml = createOptions(data, 'id', 'text');
								//$('#BlackListReason').html(createOptions(data, 'id', 'text', true))
							}
						}
                    ]),
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
									
									let sCName = $('#ResponsiblePerson option:selected').text().split('-')[1];
									let sEName = $('#ResponsiblePerson option:selected').text().split('-')[0].split('.')[0];
									sEName = sEName[0].toUpperCase() + sEName.slice(1);
									$('#Undertaker').val(sCName + "(" + sEName + ")");
                                }
                            }
                        }
                    ])
                ])
                    .done(function (res) {
                        if (res && res[0].RESULT === 1) {
                            var oRes = res[0].DATA.rel,
                                sDateRange = '';
                            oCurData = oRes;
                            oCurData.LogoFileId = oCurData.LogoFileId || guid();
                            setFormVal(oForm, oCurData);
                            if (oCurData.ExhibitionDateStart) {
                                sDateRange = newDate(oCurData.ExhibitionDateStart, 'date', true) + ' ~ ' + newDate(oCurData.ExhibitionDateEnd, 'date', true);
                            }
                            if (sTab) {
                                $('#litab3 a').click();
                            }
                            if (oRes.IsShowWebSite === 'N') {
								if (parent.OrgID === 'TG') {
									$('.showwebsiteTG').slideUp();
								} else{
									$('.showwebsiteTE').slideUp();
								}
								$('.showwebsite').slideUp();
                            }
							if (oRes.IsShowWebSiteAppoint === 'N' && parent.OrgID != 'TG') {
								$('.showwebsiteAppointTE').slideUp();
                            }
                            $('#State').val(oRes.State).trigger('change');
                            $('#ExhibitionDate').val(sDateRange);
                            $('#ExhibitionAddress_CN').text(oCurData.ExhibitionAddress_CN);
                            $('#ExhibitionAddress_EN').text(oCurData.ExhibitionAddress_EN);
							
							if (oCurData.SeaReceiveingDate) {
								if(oCurData.SeaReceiveingDate == '1900-01-01T00:00:00'){
									$('#SeaReceiveingDate').val("");
								} else {
									$('#SeaReceiveingDate').val(newDate(oCurData.SeaReceiveingDate, 'date', true));
								}
                            }
							if (oCurData.SeaClosingDate) {
								if(oCurData.SeaClosingDate == '1900-01-01T00:00:00'){
									$('#SeaClosingDate').val("");
								} else {
									$('#SeaClosingDate').val(newDate(oCurData.SeaClosingDate, 'date', true));
								}
                            }
							if (oCurData.AirReceiveingDate) {
								if(oCurData.AirReceiveingDate == '1900-01-01T00:00:00'){
									$('#AirReceiveingDate').val("");
								} else {
									$('#AirReceiveingDate').val(newDate(oCurData.AirReceiveingDate, 'date', true));
								}
                            }
							if (oCurData.AirClosingDate) {
								if(oCurData.AirClosingDate == '1900-01-01T00:00:00'){
									$('#AirClosingDate').val("");
								} else {
									$('#AirClosingDate').val(newDate(oCurData.AirClosingDate, 'date', true));
								}
                            }
							
                            setNameById().done(function () {
                                getPageVal();//緩存頁面值，用於清除
                            });
                            fnGetUploadFiles(oCurData.LogoFileId, fnUpload);
                        }
                        select2Init();
                        $('[name=IsShowWebSite]').click(function () {
                            if (this.value === 'N') {
								if (parent.OrgID === 'TG') {
									$('#ExhibitionDate,#CostRulesIdTG,#ExhibitionAddress,#file_hidden').removeAttr('required');
									$('.showwebsiteTG').slideUp();
								} else{
									$('#SeaReceiveingDate,#SeaClosingDate,#AirReceiveingDate,#AirClosingDate,#Undertaker,#Telephone,#Email,#file_hidden').removeAttr('required');
									$('.showwebsiteTE').slideUp();
								}
								$('.showwebsite').slideUp();
                            }
                            else {
								if (parent.OrgID === 'TG') {
									$('#ExhibitionDate,#CostRulesIdTG,#ExhibitionAddress,#file_hidden').attr('required', true);
									$('.showwebsiteTG').slideDown();
								} else{
									$('#SeaReceiveingDate,#SeaClosingDate,#AirReceiveingDate,#AirClosingDate,#Undertaker,#Telephone,#Email,#file_hidden').attr('required', true);
									$('.showwebsiteTE').slideDown();
								}
								$('.showwebsite').slideDown();
                            }
                        });
						$('[name=IsShowWebSiteAppoint]').click(function () {
                            if (this.value === 'N') {
								$('#CostRulesIdTE').removeAttr('required');
								$('.showwebsiteAppointTE').slideUp();
                            }
                            else {
								$('#CostRulesIdTE').attr('required', true);
								$('.showwebsiteAppointTE').slideDown();
                            }
                        });

                        //是否顯示於網站，值為'N'收起選項與移除required
                        if (oCurData.IsShowWebSite === 'N') {
                            $('#sub_box1').slideUp();
                            $('[name=IsShowWebSite][value="N"]').click();
                        }
						
						if (oCurData.IsShowWebSiteAppoint === 'N') {
                            $('[name=IsShowWebSiteAppoint][value="N"]').click();
                        }
                    });

                oValidator = $("#form_main").validate({
                    ignore: ''
                });

                $('#ExhibitionDate').dateRangePicker(
                    {
                        language: 'zh-TW',
                        separator: ' ~ ',
                        format: 'YYYY/MM/DD',
                        autoClose: true
                    }).bind('datepicker-change', function (e, r) {
                        try {
							$('#ExhibitionDateStart2').val(newDate(r.date1, 'date'));
							$('#ExhibitionDateEnd2').val(newDate(r.date2, 'date'));
							
                            var dShelfTime_Home = r.date2.dateAdd('d', -15),
                                dShelfTime_Abroad = new Date(newDate(r.date2, 'date')).dateAdd('d', -15);
                            $('#ShelfTime_Home').val(newDate(dShelfTime_Home, 'date'));
                            $('#ShelfTime_Abroad').val(newDate(dShelfTime_Abroad, 'date'));
							//alert(newDate(r.date2, 'date'));
							
                            //$('#ExhibitionDateEnd2').val(r.date2);
							
                        } catch (e) { }
                    });

                $('#Exhibitioname_TW').on('blur', function () {
                    $('#Exhibitioname_CN').val(simplized(this.value));
                });
                $('#Exhibitioname_CN').on('blur', function () {
                    $('#Exhibitioname_TW').val(traditionalized(this.value));
                });
				$('#ExhibitioShotName_TW').on('change', function () {
                    $('#ExhibitioShotName_TW2').val($('#ExhibitioShotName_TW').val());
                });
				$('#ResponsiblePerson').on('change', function () {
                    $('#ResponsiblePerson2').val($('#ResponsiblePerson').val());
					let sCName = this.options[this.selectedIndex].text.split('-')[1];
					let sEName = this.options[this.selectedIndex].text.split('-')[0].split('.')[0];
					sEName = sEName[0].toUpperCase() + sEName.slice(1);
					$('#Undertaker').val(sCName + "(" + sEName + ")");
                });
				$('#Industry').on('change', function () {
                    $('#Industry2').val($('#Industry').val());
                });
				$('#State').on('change', function () {
                    $('#State2').val($('#State').val());
                });
				//$("#Industry2").val(oRes.Industry);
				//$("#State2").val(oRes.State);
				
                var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 150;
                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: true,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    rowClick: function (args) {
                        if (navigator.userAgent.match(/mobile/i)) {
                            goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                        }
                    },
                    rowDoubleClick: function (args) {
                        parent.openPageTab('Callout_Upd', '?Action=Upd&SN=' + sDataId + '&guid=' + args.item.guid);
                    },
                    fields: [
						{
                            width: 30, sorting: false, align: "center",
                            headerTemplate: function () {
                                return [$("<input>", {
                                    id: 'SelectAll',
                                    type: 'checkbox', click: function () {
                                        if (this.checked) {
                                            $("#jsGrid").find('[type=checkbox]').each(function () {
                                                this.checked = true;
                                            });
                                            saCustomerList = clone(saGridData);
                                        }
                                        else {
                                            $("#jsGrid").find('[type=checkbox]').each(function () {
                                                this.checked = false;
                                            });
                                            saCustomerList = [];
                                        }
                                    }
                                })];//╠common.SelectAll⇒全選╣
                            },
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
                                            $('#jsGrid').find('#SelectAll')[0].checked = false;
                                        }
                                    }
                                });
                            }
                        },
                        {
                            name: "TransportRequire", title: '運輸需求', width: 60, align: "center", itemTemplate: function (val, item) {
								if(val == "有需求"){
									return $('<span />', { text: val }).css('color', 'green');
								} else if(val == "無需求"){
									return $('<span />', { text: val }).css('color', 'red');
								} else {
									return val;
								}
							}
                        },
                        {
                            name: "TransportationMode", title: '運輸方式', width: 50, align: "center"
                        },
                        {
                            name: "VolumeForecasting", title: '貨量', width: 50, align: "center"
                        },
                        {
                            name: "NumberOfBooths", title: '攤位數', width: 50, align: "center"
                        },
                        {
                            name: "CustomerCName", title: 'Customers_Upd.CustomerCName', width: 150,
                        },
                        {
                            name: "CustomerEName", title: 'Customers_Upd.CustomerEName', width: 200,
                        },
                        {
                            name: "ContactorName", title: 'common.Contactor', width: 100, align: "center"
                        },
                        
                        {
                            name: "ListSourceName", title: '名單來源', width: 100, align: "center", itemTemplate: function (val, item) {
								if(item.ListSource == "SelfCome"){
									return "自來";
								} else if(item.ListSource == "ImportFromDB"){
									return "資料庫匯入";
                                } else {
									return val;
								}
							}
                        },
						{
							name: "ModifyDate", title: 'common.ModifyDate', type: "text", align: "center", width: 100, itemTemplate: function (val, item) {
								return newDate(val);
							}
						},
						{
                            name: "Memo", title: 'common.Memo', width: 190, align: "left"
                        },
						{
                            name: "IsDeal", title: '是否<br>成交', width: 45, align: "Center",
							itemTemplate: function (val, item) {
								return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(val === 'Y' ? $('<span />', { text: "已成交" }).css('color', 'blue') : "未成交");
							}
                        },
						{
                            name: "IsImporter", title: '是否為<br>進口商', width: 45, align: "Center",
							itemTemplate: function (val, item) {
								return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(val === 'Y' ? $('<span />', { text: "是" }).css('color', 'red') : "否");
							}
                        },
                        {// ╠common.IsFormal⇒資料狀態╣
                            name: "IsFormal",
                            title: 'common.IsFormal',
                            width: 90,
                            align: 'center',
                            itemTemplate: function (val, item) {
                                var saAction = [];
                                if (item.IsFormal == "N") {
                                    saAction.push($('<a/>', {
                                        html: i18next.t('common.TransferToFormal'),// ╠common.TransferToFormal⇒轉為正式客戶╣
                                        class: 'link',
                                        click: function () {
                                            fnTransferToFormal(item);
                                            return false;
                                        }
                                    }));
                                    saAction.push('<br>', $('<a/>', {
										html: i18next.t('common.CorrespondFormalCus'),// ╠common.CorrespondFormalCus⇒對應到正式客戶╣
										class: 'link',
										style: 'color:green !important',
										click: function () {
											fnCorrespondFormalCus(item);
										}
									}));
                                }
                                else {
                                    saAction.push($('<span />', { text: i18next.t('common.HasFormal') }).css('color', 'green'));
                                }
                                return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetGridData(args);
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
                    height: iHeight,
                    autoload: false,
                    filtering: true,
                    pageLoading: true,
                    inserting: true,
                    editing: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: parent.SysSet.GridRecords || 10,
                    pageButtonCount: parent.SysSet.GridPages || 15,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    rowClick: function (args) {
                        //if (navigator.userAgent.match(/mobile/i)) {
                        //    goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                        //}
                    },
                    rowDoubleClick: function (args) {
                        //parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                    },
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {// ╠Exhibition_Upd.ExhibitionArea⇒展區╣
                            name: "ExhibitionArea", title: 'Exhibition_Upd.ExhibitionArea', type: 'text', width: 100
                        },
                        {
                            name: "MuseumMumber", title: 'ExhibitionImport_Upd.HallMuseumMumber', type: 'text', width: 80
                        },
                        {
                            name: "CustomerCName", title: 'Customers_Upd.CustomerCName', type: 'text', width: 140,
                            validate: [
                                {
                                    validator: 'required',
                                    message: i18next.t('Customers_Upd.CustomerCName_required')// ╠Customers_Upd.CustomerCName_required⇒請輸入公司中文名稱╣
                                },
                                {
                                    validator: function (value, item) {
                                        var bRetn = true;
                                        CallAjax(ComFn.W_Com, ComFn.GetCount, {
                                            Params: {
                                                importcustomers: {
                                                    CustomerCName: value,
                                                    OrgID: parent.top.OrgID,
                                                    ExhibitionNO: sDataId,
                                                    guid: '<>' + item.guid
                                                }
                                            }
                                        }, function (rq) {
                                            if (rq.d > 0) {
                                                bRetn = false;
                                            }
                                        }, null, true, false);
                                        return bRetn;
                                    },
                                    message: i18next.t("message.CustomerCNameExist")  // ╠message.CustomerCNameExist⇒該廠商名稱已存在╣
                                }]
                        },
                        {
                            name: "CustomerEName", title: 'Customers_Upd.CustomerEName', type: 'text', width: 140
                        },
                        {
                            name: "UniCode", title: 'Customers_Upd.UniCode', type: 'text', width: 100,
                            validate: [
                                {
                                    validator: function (_val) {
                                        return _val === '' ? true : _val.length === 8;
                                    },
                                    message: i18next.t('message.UniCodeLength')
                                }
                            ]
                        },
                        {
                            name: "Contactor", title: 'common.Contactor', type: 'text', width: 120,
                            validate: { validator: 'required', message: i18next.t('common.Contactor_required') }// ╠common.Contactor_required⇒請輸入聯絡人╣
                        },
                        {
                            name: "Telephone", title: 'common.Telephone', type: 'text', width: 100, align: "center",
                            validate: { validator: 'required', message: i18next.t('common.Telephone_required') }// ╠common.Telephone_required⇒請輸入聯絡電話╣
                        },
                        {
                            name: "Email", title: 'common.Email', type: 'text', width: 130,
                            validate: {
                                validator: function (_val) {
                                    return _val === '' ? true : isEmail(_val);
                                },
                                message: i18next.t('message.IncorrectEmail'),// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                            }
                        },
                        {
                            name: "Address", title: 'common.Address', type: 'text', width: 100
                        },
                        {
                            name: "Memo", title: 'common.Memo', type: 'text', width: 100
                        },
                        {// ╠common.IsAppoint⇒預約狀態╣
                            name: "IsAppoint", title: 'common.IsAppoint', width: 60, align: "center",
                            itemTemplate: function (val, item) {
                                return val === 'Y' ? $('<span />', { text: i18next.t('common.HasAppoint') }).css('color', 'green') : $('<span />', { text: i18next.t('common.NotAppoint') }).css('color', 'red');// ╠common.HasAppoint⇒已預約╣ ╠common.NotAppoint⇒未預約╣
                            }
                        },
                        {// ╠common.IsFormal⇒資料狀態╣
                            name: "guid",
                            title: 'common.IsFormal',
                            width: 140,
                            align: 'center',
                            itemTemplate: function (val, item) {
                                var saAction = [];
                                if (!item.IsFormal) {
                                    saAction.push($('<a/>', {
                                        html: i18next.t('common.TransferToFormal'),// ╠common.TransferToFormal⇒轉為正式客戶╣
                                        class: 'link',
                                        click: function () {
                                            fnTransferToFormal(item);
                                            return false;
                                        }
                                    }));
                                    if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                                        saAction.push('   ', $('<a/>', {
                                            html: i18next.t('common.CorrespondFormalCus'),// ╠common.CorrespondFormalCus⇒對應到正式客戶╣
                                            class: 'link',
                                            style: 'color:green !important',
                                            click: function () {
                                                fnCorrespondFormalCus(item);
                                            }
                                        }));
                                    }
                                }
                                else {
                                    saAction.push($('<span />', { text: i18next.t('common.HasFormal') }).css('color', 'green'));
                                }
                                return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
                            }
                        },
                        {
                            type: "control",
                            width: 100,
                            headerTemplate: function () {
                                var saBtn = [];
                                if (sAction === 'Upd') {
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
                                }
                                return saBtn;
                            },
                            deleteButton: false
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetGridData1(args);
                        },
                        insertItem: function (args) {
                            fnAddCustomers(args);
                        },
                        updateItem: function (args) {
                            fnUpdCustomers(args);
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid1 = args.grid;
                    }
                });
            };

        init();
    },
    /**
     * 客戶轉為正式資料后動作
     * @param   {String}afterid 客戶id
     */
    fnReFresh = function (afterid) {
        bRefresh = true;
        $('iframe').attr('src', '../Crm/Customers_Upd.html?Action=Upd&Flag=Fit&guid=' + afterid);
    };

require(['base', 'select2', 'jsgrid', 'daterangepicker', 'convetlng', 'filer', 'ajaxfile', 'util'], fnPageInit, 'daterangepicker');