'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CreateDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param  {Object}  args  查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm);
            },
            /**
             * 匯出資料
             * @param {Object} args  查詢參數
             * @return {Object} Ajax 物件
             */
            fnExcel = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                oQueryPm.Excel = true;

                g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        DownLoadFile(res.DATA.rel);
                    }
                });
            },
            /**
            * 目的 對應匯入廠商
            * @param  {String}item 預約單資料
            */
            fnCorrespondImpCus = function (item) {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t("PackingOrder_Upd.CorrespondImpCus"), // ╠PackingOrder_Upd.CorrespondImpCus⇒對應匯入廠商╣
                    area: '640px;',//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_PackingInfo', //设定一个id，防止重复弹出
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
                        g_api.ConnectLite(sEditPrgId, 'SetImpCusDrop', {
                            Id: item.ExhibitionNO
                        }, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'guid', 'CustomerCName');
                                $('#CustomerId').html(sOptions).select2();
                            }
                        });
                        transLang(layero);
                    },
                    yes: function (index, layero) {
                        var sCustomerId = $('#CustomerId').val();
                        if (!sCustomerId) {
                            showMsg(i18next.t('message.SelectImpCus'));//╠message.SelectImpCus⇒請選擇對應的廠商╣
                            return false;
                        }
                        g_api.ConnectLite(sEditPrgId, 'CorrespondImpCus', {
                            Id: item.AppointNO,
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
             * 匯入到「其他」入口
            * @param  {String}item 預約單資料
             */
            fnImportOthers = function (item) {
                layer.open({
                    type: 1,
                    title: i18next.t('PackingOrder_Upd.SelectImportPrg'),// ╠PackingOrder_Upd.SelectImportPrg⇒選擇匯入入口╣
                    area: ['300px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                           <button type="button" data-i18n="PackingOrder_Upd.OtherPrg" id="OtherPrg" class="btn-custom green">「其他」</button>\
                           <button type="button" data-i18n="PackingOrder_Upd.OtherPrgTG" id="OtherPrgTG" class="btn-custom green">「其他（駒驛）」</button>\
                         </div>',//╠PackingOrder_Upd.OtherPrg⇒「其他」╣╠PackingOrder_Upd.OtherPrgTG⇒「其他（駒驛）」╣
                    success: function (layero, index) {
                        $('.pop-box :button').click(function () {
                            if (this.id === 'OtherPrg') {
                                parent.openPageTab('OtherBusiness_Upd', '?Action=Add&AppointNO=' + item.AppointNO);
                            }
                            else {
                                parent.openPageTab('OtherExhibitionTG_Upd', '?Action=Add&AppointNO=' + item.AppointNO);
                            }
                            layer.close(index);
                        });
                        transLang(layero);
                    }
                });
            },
			/**
             * 匯入到「展場服務部」入口
            * @param  {String}item 預約單資料
             */
            fnImportOthersTE = function (item) {
                layer.open({
                    type: 1,
                    title: i18next.t('PackingOrder_Upd.SelectImportPrg'),// ╠PackingOrder_Upd.SelectImportPrg⇒選擇匯入入口╣
                    area: ['300px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                           <button type="button" data-i18n="展場服務部" id="OtherPrgTG" class="btn-custom green">「展場服務部」</button>\
                         </div>',//╠PackingOrder_Upd.OtherPrg⇒「其他」╣╠PackingOrder_Upd.OtherPrgTG⇒「其他（駒驛）」╣
                    success: function (layero, index) {
                        $('.pop-box :button').click(function () {
                            parent.openPageTab('OtherExhibitionTG_Upd', '?Action=Add&AppointNO=' + item.AppointNO);
                            layer.close(index);
                        });
                        transLang(layero);
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
                        
                        var iNum = $('#PerPageNum').val();
                        oGrid.pageSize = iNum === '' ? parent.SysSet.GridRecords || 10 : iNum;
                        cacheQueryCondition();
                        oGrid.openPage(window.bToFirstPage ? 1 : oBaseQueryPm.pageIndex);

                        break;
                    case "Toolbar_Save":

                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_EntryOrder":

                        parent.openPageTab(sEditPrgId, '?Action=Add');

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_Exp":
                        if (oGrid.data.length === 0) {
                            showMsg(i18next.t("message.NoDataExport"));// ╠message.NoDataExport⇒沒有資料匯出╣
                            return false;
                        }
                        fnExcel();
                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /**
             * 頁面初始化
             */
            init = function () {
                var saCusBtns = [{
                    id: 'Toolbar_EntryOrder',
                    value: 'common.Toolbar_EntryOrder'// ╠common.Toolbar_EntryOrder⇒錄入預約單╣
                }];
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    SearchBar: true
                }).done(function () {
                    var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
					var saFields = [
						{ name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 40, sorting: false },
						{// ╠common.AppointNO⇒預約單號╣
							name: "AppointNO", title: 'common.AppointNO', type: "text", width: 120
						},
						{// ╠common.CompanyName⇒公司名稱╣
							name: "CompName", title: 'common.CompanyName', type: "text", width: 150
						}];
					
					if (parent.OrgID === 'TE'){
						saFields.push(
							{// ╠common.Unicode
								name: "Unicode", title: 'Customers_Upd.UniCode', type: "text", width: 60
							}
						);
					}
					
					saFields.push(
						{// ╠Exhibition_Upd.Exhibitioname_TW⇒展覽名稱╣
							name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 150,
							itemTemplate: function (val, item) {
								return $('<a />', {
									class: 'link',
									text: val, click: function () {
										parent.openPageTab('Exhibition_Upd', '?Action=Upd&SN=' + item.ExhibitionNO);
									}
								})
							}
						},
						{// ╠common.MuseumMumber⇒預約攤號╣
							name: "MuseumMumber", title: 'common.MuseumMumber', type: "text", width: 70
						},
						{// ╠common.AppointUser⇒預約人員╣
							name: "AppointUser", title: 'common.AppointUser', type: "text", width: 80
						},
						{// ╠common.AppointTel⇒預約電話╣
							name: "AppointTel", title: parent.OrgID === 'TE' ? '公司電話及分機' : 'common.AppointTel', type: "text", width: 80
						},
						{// ╠common.AppointEmail⇒預約Email╣
							name: "AppointEmail", title: 'common.AppointEmail', type: "text", width: 140
						},
						{// ╠ExhibitionImport_Upd.SitiContactor⇒現場聯絡人╣
							name: "Contactor", title: 'ExhibitionImport_Upd.SitiContactor', type: "text", width: 80
						},
						{// ╠ExhibitionImport_Upd.SitiTelephone⇒現場聯絡電話╣
							name: "ContactTel", title: 'ExhibitionImport_Upd.SitiTelephone', type: "text", width: 80
						},
						{// ╠ExhibitionImport_Upd.ApproachTime⇒進場時間╣
							name: "ApproachTime", title: 'ExhibitionImport_Upd.ApproachTime', type: "text", align: "center", width: 100,
							itemTemplate: function (val, item) {
								return newDate(val);
							}
						},
						{// ╠ExhibitionImport_Upd.ExitTime⇒退場時間╣
							name: "ExitTime", title: 'ExhibitionImport_Upd.ExitTime', type: "text", align: "center", width: 100,
							itemTemplate: function (val, item) {
								return newDate(val);
							}
						}
					);
					
					if (parent.OrgID === 'TE'){
						saFields.push(
							{// ╠付款方式╣
								name: "PaymentWay", title: '付款方式', type: "text", width: 50, itemTemplate: function (val, item) {
								return val === '1' ? "匯款" : "現場付現";
								}
							}
						);
					}
					
					saFields.push(
						{// ╠common.AppointDateTime⇒預約時間╣
							name: "AppointDateTime", title: 'common.AppointDateTime', type: "text", align: "center", width: 100,
							itemTemplate: function (val, item) {
								return newDate(val);
							}
						},
						{// ╠PackingOrder_Upd.CorrespondStatus⇒對應狀態╣
							name: "CustomerId", title: 'PackingOrder_Upd.CorrespondStatus', type: "text", align: "center", width: 100,
							itemTemplate: function (val, item) {
								var templ = [];
								if (val) {// ╠PackingOrder_Upd.HasCorrespond⇒已對應╣
									templ.push($('<span />', { text: i18next.t('PackingOrder_Upd.HasCorrespond') }).css('color', 'green'));
									if (item.IsFormal) {
										// ╠common.HasFormal⇒已轉正╣
										templ.push('（', $('<span />', { text: i18next.t('common.HasFormal') }).css('color', 'green'), '）');
									}
									else {
										// ╠common.NotFormal⇒未轉正╣
										templ.push('（', $('<a />', {
											class: 'link',
											text: i18next.t('common.NotFormal'), click: function () {
												parent.openPageTab('Customers_Upd', '?Action=Add&FromId=' + item.AppointNO + '&From=Appoint&Flag=Appoint');
											}
										}).css('color', 'red'), '）');
									}
								}
								else {
									templ.push($('<a/>', {
										html: i18next.t('PackingOrder_Upd.CorrespondImpCus'),// ╠PackingOrder_Upd.CorrespondImpCus⇒對應到匯入廠商╣
										class: 'link',
										click: function () {
											fnCorrespondImpCus(item);
										}
									}));
								}
								return templ;
							}
						},
						{// ╠PackingOrder_Upd.ImpOtherPrgStatus⇒匯入狀態╣
							name: "OtherId", title: 'PackingOrder_Upd.ImpOtherPrgStatus', type: "text", align: "center", width: 100,
							itemTemplate: function (val, item) {
								var templ = $('<a />', {
									class: 'link',
									text: i18next.t('PackingOrder_Upd.HasImpOtherPrg'), click: function () {
										var bTG = item.OtherIdFrom === 'OtherBusiness_Upd';
										parent.openPageTab(item.OtherIdFrom, '?Action=Upd&' + (bTG ? 'ImportBillNO' : 'Guid') + '=' + item.OtherId + '&GoTab=' + (bTG ? 2 : 3));
									}
								}).css('color', 'green');// ╠PackingOrder_Upd.HasImpOtherPrg⇒已產生賬單資料╣
								if (!val) {
									templ = $('<a/>', {
										html: parent.OrgID === 'TE' ? "匯入到「展場服務部」" : i18next.t('PackingOrder_Upd.ImpOtherPrg'),// ╠PackingOrder_Upd.ImpOtherPrg⇒匯入到「其他」入口╣
										class: 'link',
										click: function () {
											var sError = '';
											if (!item.CustomerId) {
												sError = i18next.t('message.CorrespondImpCusFirst');//╠message.CorrespondImpCusFirst⇒請先對應到匯入廠商╣
											}
											if (!item.IsFormal) {
												sError = i18next.t('message.TransferToFormalFirst');//╠message.TransferToFormalFirst⇒請先轉正該廠商╣
											}

											if (sError) {
												showMsg(sError);
												return false;
											}
											if (parent.OrgID === 'TE'){
												fnImportOthersTE(item);
											} else {
												fnImportOthers(item);
											}
										}
									});
								}
								return templ;
							}
						}
					);
					
                    $("#jsGrid").jsGrid({
                        width: "100%",
                        height: iHeight + "px",
                        autoload: true,
                        pageLoading: true,
                        inserting: false,
                        editing: false,
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
                        rowClass: function (item) {
                            var sClass = '';
                            if (item.IsKeyMode) {
                                sClass = 'key-in';
                            }
                            return sClass;
                        },
                        rowClick: function (args) {
                            if (navigator.userAgent.match(/mobile/i)) {
                                goToEdit(sEditPrgId, '?Action=Upd&AppointNO=' + args.item.AppointNO);
                            }
                        },
                        rowDoubleClick: function (args) {
                            if (args.item.IsKeyMode) {
                                parent.openPageTab(sEditPrgId, '?Action=Upd&AppointNO=' + args.item.AppointNO);
                            }
                            else {
                                var saPackingInfo = JSON.parse(args.item.PackingInfo),
                                    sContent = '<div class="row popsrow">\
                                            <div class="col-sm-12">\
                                                <div id="jsGrid_PackingInfo"></div>\
                                            </div>\
                                        </div>\
                                        <div class="row popsrow">\
                                            <div class="col-sm-12 wcenter" style="font-size: 20px;font-weight: bold;">\
                                               費用總計(新臺幣含稅)：NT$<span class="PackingTotal"></span>\
                                            </div>\
                                        </div>';

                                layer.open({
                                    type: 1,
                                    title: i18next.t('common.BookingDetails'),//╠common.BookingDetails⇒預約明細╣
                                    shadeClose: false,
                                    shade: 0.1,
                                    maxmin: true, //开启最大化最小化按钮
                                    area: ['800px', '500px'],
                                    content: sContent,
                                    success: function (layero, index) {
                                        $("#jsGrid_PackingInfo").jsGrid({
                                            width: "100%",
                                            height: "auto",
                                            autoload: true,
                                            pageLoading: true,
                                            pageIndex: 1,
                                            pageSize: 10000,
                                            fields: [
                                                { name: "Index", title: 'common.RowNumber', width: 50, align: "center" },
                                                {// ╠common.Packaging⇒包裝類型╣
                                                    name: "ExpoType", title: 'common.Packaging', width: 100, align: "center",
                                                    itemTemplate: function (val, item) {
                                                        var oExpoType = {
                                                            'zh-TW': { '01': '裸機', '02': '木箱', '03': '散貨', '04': '打板', '05': '其他' },
                                                            'en': { '01': 'Unwrapped', '02': 'Wooden Crate', '03': 'Bulk Cargo', '04': 'Pallet', '05': 'Other' }
                                                        },
                                                            bEn = 'Unwrapped,Wooden Crate,Bulk Cargo,Pallet,Other'.indexOf(item.ExpoTypeText) > -1;
                                                        return val ? oExpoType[bEn ? 'en' : 'zh-TW'][val] : '';
                                                    }
                                                },
                                                {// ╠common.Dimensions⇒尺寸╣
                                                    name: "ExpoLen", title: 'common.Dimensions', width: 100,
                                                    itemTemplate: function (val, item) {
                                                        return item.ExpoLen.toMoney() + '*' + item.ExpoWidth.toMoney() + '*' + item.ExpoHeight.toMoney();
                                                    }
                                                },
                                                {// ╠common.WeightKG⇒重量╣
                                                    name: "ExpoWeight", title: 'common.WeightKG', width: 100, align: "right",
                                                    itemTemplate: function (val, item) {
                                                        return val.toMoney();
                                                    }
                                                },
                                                {
                                                    name: "ExpoNumber", title: 'common.Number', width: 100, align: "center",
                                                    itemTemplate: function (val, item) {
                                                        return val.toMoney();
                                                    }
                                                },// ╠common.Number⇒件數╣
                                                {// ╠common.ServiceProject⇒服務項目╣
                                                    name: "ExpoStack", title: 'common.ServiceProject', width: 200,
                                                    itemTemplate: function (val, item) {
                                                        var oService = {
                                                            'zh-TW': ['堆高機服務', '拆箱(含空箱收送與儲存)', '裝箱', '空箱收送', '空箱儲存', '天'],
                                                            'en': ['Forklift', 'Unpacking (including empty crate transport & storage)', 'Packing', 'Empty Crate Transport', 'Empty Crate Storage', 'Days']
                                                        },
                                                            saText = [],
                                                            bEn = 'Unwrapped,Wooden Crate,Bulk Cargo,Pallet,Other'.indexOf(item.ExpoTypeText) > -1;
                                                        oService = oService[bEn ? 'en' : 'zh-TW'];
                                                        if (item.ExpoStack) {
                                                            saText.push(oService[0]);
                                                        }
                                                        if (item.ExpoSplit) {
                                                            saText.push(oService[1]);
                                                        }
                                                        if (item.ExpoPack) {
                                                            saText.push(oService[2]);
                                                        }
                                                        if (item.ExpoFeed) {
                                                            saText.push(oService[3]);
                                                        }
                                                        if (item.ExpoStorage) {
                                                            saText.push(oService[4] + item.ExpoDays + oService[5]);
                                                        }
                                                        return saText.join('，');
                                                    }
                                                },
                                                {// ╠common.Cost⇒費用╣
                                                    name: "SubTotal", title: 'common.Cost', width: 100, align: "right",
                                                    itemTemplate: function (val, item) {
                                                        return fMoney(val || 0, 0, 'NTD');
                                                    }
                                                }
                                            ],
                                            controller: {
                                                loadData: function (args) {
                                                    return {
                                                        data: saPackingInfo,
                                                        itemsCount: saPackingInfo.length //data.length
                                                    };
                                                }
                                            }
                                        });
                                        $(".PackingTotal").text(fMoney(args.item.Total || 0, 0, 'NTD'));
                                    },
                                    btn: [i18next.t('common.Close')],//╠common.Close⇒關閉╣
                                });
                            }
                        },
                        fields: saFields,
                        controller: {
                            loadData: function (args) {
                                return fnGet(args);
                            }
                        },
                        onInit: function (args) {
                            oGrid = args.grid;
                        }
                    });
                });
            };

        init();
    };

require(['base', 'jsgrid', 'select2', 'util'], fnPageInit);