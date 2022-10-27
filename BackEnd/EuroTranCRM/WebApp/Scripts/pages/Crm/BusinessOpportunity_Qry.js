'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    sViewPrgId = sProgramId.replace('_Qry', '_View'),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'ModifyDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;
                oQueryPm.Roles = parent.UserInfo.roles;

                return g_api.ConnectLite(sProgramId, 'QueryPage', oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (args.Excel) {//匯出
                            DownLoadFile(oRes);
                        }
                    }
                });
            },
            fnAdd = function (data) {
                data = packParams(data);
                g_api.ConnectLite('BusinessOpportunity_Qry', 'Insert', data,
                    function (res) {
                        if (res.RESULT == '1') {
							showMsg(i18next.t("message.Save_Success"), 'success');
                        } else {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        }

                    },
                    function (res) {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                )
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnModifyBusinessOpportunity = function (data) {
                var formData = {};
                layer.open({
                    type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "編輯潛在商機",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                    area: ['70%', '90%'],//寬度
                    shade: 0.75,//遮罩
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_CombineContactor', //设定一个id，防止重复弹出
					offset: '10px',//右下角弹出
                    anim: 0,//彈出動畫
					btn: ['作廢','儲存', i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: ['c'],//按鈕位置
                    content: '../CRM/BusinessOpportunity_Upd.html',
                    success: function (layero, index) {
                        $('.layui-layer-btn0').css('border', '1px solid #dedede');
                        $('.layui-layer-btn0').css('background-color', '#FFF');
                        $('.layui-layer-btn0').css('color', '#333');
                        $('.layui-layer-btn1').css('background-color', '#1E9FFF');
                        $('.layui-layer-btn1').css('color', '#FFF');
                        
                        g_api.ConnectLite('BusinessOpportunity_Qry', 'QueryOne',
                            {
                                SN: data.SN
                            }, function (res) {
                                if (res.RESULT) {
                                    var oRes = res.DATA.rel;
                                    var iframe = layero.find('iframe').contents();
                                    var DateStart = oRes['DateStart'];
                                    var DateEnd = oRes['DateEnd'];
                                    var sDateStart = '';
                                    var sDateEnd = '';
                                    var DateStr = '';
                                    try {
                                        sDateStart = DateStart.split('T')[0].replaceAll('-', '/');
                                        sDateEnd = DateEnd.split('T')[0].replaceAll('-', '/');
                                        DateStr = sDateStart + '' + " ~ " + sDateEnd;
                                    } catch (e) {
                                        DateStr = null;
                                    }
                                    iframe.find("#State").html($("#State").html());
                                    iframe.find("#Industry").html($("#Industry").html());
                                    if (oRes['ExhibitionNO'] != "") {
                                        iframe.find("#BtnExhibitionCode").attr({ disabled: 'disabled' });
                                        iframe.find('#CreateExhibition').attr({ disabled: 'disabled' });
                                        iframe.find("#ExhibitionShotName").attr({ disabled: 'disabled' });
                                        iframe.find("#ExhibitionName").attr({ disabled: 'disabled' });
                                        iframe.find("#ExhibitionName_EN").attr({ disabled: 'disabled' });
                                        iframe.find("#State").attr({ disabled: 'disabled' });
                                        iframe.find("#Industry").attr({ disabled: 'disabled' });
										iframe.find("#Year").attr({ disabled: 'disabled' });
                                        iframe.find("#Date").attr({ disabled: 'disabled' });
                                    }
                                    if (oRes['Effective'] === '1') {
                                        iframe.find('[name = Effective][value = "1"]').click();
                                        $('.layui-layer-btn0').hide();
                                    }else if (oRes['Effective'] === '2')
                                    {   
                                        iframe.find('[name = Effective][value = "2"]').click();
                                        iframe.find("#BtnExhibitionCode").attr({ disabled: false });
                                        iframe.find('#CreateExhibition').attr({ disabled: false });
                                        iframe.find("#ExhibitionShotName").attr({ disabled: false });
                                        iframe.find("#ExhibitionName").attr({ disabled: false });
                                        iframe.find("#ExhibitionName_EN").attr({ disabled: false });
                                        iframe.find("#State").attr({ disabled: false });
                                        iframe.find("#Industry").attr({ disabled: false });
                                        iframe.find("#Year").attr({ disabled: false });
                                        iframe.find("#Date").attr({ disabled: false });
                                        $('.layui-layer-btn0').css('position', 'absolute');
                                        $('.layui-layer-btn0').css('left', '5%');
                                    } else {
                                        var EffectiveStatusHtml = 
                                               '<input id="Effective_1" type="radio" name="Effective" value="1" />\
                                                <label for="Effective_1" data-i18n="已處理">已處理</label>\
                                                <input id="Effective_2" type="radio" name="Effective" value="2" />\
                                                <label for="Effective_2" data-i18n="未處理">未處理</label>\
                                                <input id="Effective_0" type="radio" name="Effective" value="0" />\
                                                <label for="Effective_0" data-i18n="已作廢">已作廢</label>';
                                        iframe.find('#EffectiveStatusDiv').html(EffectiveStatusHtml);
                                        iframe.find('[name = Effective][value = "0"]').click();
										iframe.find('[name = Effective][value = "1"]').attr({ disabled: 'disabled' });
										iframe.find('[name = Effective]').click(function () {
											if($(this).val() == "2" || $(this).val() == "0"){
												iframe.find('#BtnExhibitionCode').attr('disabled', false);
												iframe.find('#CreateExhibition').attr('disabled', false);
												iframe.find("#ExhibitionShotName").attr('disabled', false);
												iframe.find("#ExhibitionName").attr('disabled', false);
												iframe.find("#ExhibitionName_EN").attr('disabled', false);
												iframe.find("#State").attr('disabled', false);
												iframe.find("#Industry").attr('disabled', false);
												iframe.find("#Year").attr('disabled', false);
												iframe.find("#Date").attr('disabled', false);
												iframe.find('#ExhibitionCode').val('');
												iframe.find('[name=Effective][value="1"]').attr('disabled', true);
												}
										})
                                        $('.layui-layer-btn0').hide();
                                        iframe.find("#BtnExhibitionCode").attr({ disabled: false });
                                        iframe.find('#CreateExhibition').attr({ disabled: false });
                                        iframe.find("#ExhibitionShotName").attr({ disabled: false });
                                        iframe.find("#ExhibitionName").attr({ disabled: false });
                                        iframe.find("#ExhibitionName_EN").attr({ disabled: false });
                                        iframe.find("#State").attr({ disabled: false });
                                        iframe.find("#Industry").attr({ disabled: false });
                                        iframe.find("#Year").attr({ disabled: false });
                                        iframe.find("#Date").attr({ disabled: false });
										iframe.find('#BtnExhibitionCode').val("");
                                    }
                                    iframe.find("#BtnExhibitionCode").val(oRes['ExhibitionNO'])
                                    iframe.find("#Year").val(oRes['Year']);
                                    iframe.find("#Date").val(DateStr);
                                    iframe.find("#ExhibitionShotName").val(oRes['ExhibitionShotName']);
                                    iframe.find("#ExhibitionName").val(oRes['ExhibitionName']);
                                    iframe.find("#ExhibitionName_EN").val(oRes['ExhibitionName_EN']);
                                    iframe.find("#State").val(oRes['State']);
                                    iframe.find("#Industry").val(oRes['Industry']);
                                    iframe.find("#CustomerName").val(oRes['CustomerName']);
                                    iframe.find("#Contactor").val(oRes['Contactor']);
                                    iframe.find("#Department").val(oRes['Department']);
                                    iframe.find("#JobTitle").val(oRes['JobTitle']);
                                    iframe.find("#Email1").val(oRes['Email1']);
                                    iframe.find("#Email2").val(oRes['Email2']);
                                    iframe.find("#Telephone1").val(oRes['Telephone1']);
                                    iframe.find("#Telephone2").val(oRes['Telephone2']);
                                    iframe.find("#labelCreateUser").html("CreateName");
                                    iframe.find("#labelCreateUser").html(oRes['CreateUser']);
                                    iframe.find("#labelModifyUser").html(oRes['ModifyUser']);
                                    iframe.find("#labelCreateDate").html(oRes['CreateDate']);
                                    iframe.find("#labelModifyDate").html(oRes['ModifyDate']);
                                }
                            }
                        );
                        
                    },
                    yes: function (index, layero) {
                        layer.confirm("確定要作廢？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            var iframe = layero.find('iframe').contents();
                            formData = getFormSerialize($(iframe.find('#form_main')));
                            formData.ExhibitionCode = iframe.find('#BtnExhibitionCode').val();
                            var voidFlag = iframe.find('input[name=Effective]:checked').val();
                            if (voidFlag === "0") {
                                showMsg("作廢失敗，此單已作廢", 'error');
                                layer.close(index);
                                return;
                            }
                            g_api.ConnectLite('BusinessOpportunity_Qry', 'UpdateByGuid', {
                                    SN: data.SN,
                                }, function (res) {
                                    showMsg("作廢成功", 'success');
                                }, function (res) {
                                    showMsg("作廢失敗", 'error');
                            });
                            
                            layer.close(index);
                            layer.close(index-1);
                        });
                    },
                    btn2: function (index, layero) {
                        var iframe = layero.find('iframe').contents();
                        var formData = getFormSerialize($(iframe.find('#form_main')));
                        
                        try {
                            formData.DateStart = $.trim(formData.Date.split('~')[0]);
                            formData.DateEnd = $.trim(formData.Date.split('~')[1]);
                        } catch (e) {
                            formData.DateStart = null;
                            formData.DateEnd = null;
                        }
                        formData.ExhibitionNO = iframe.find('#BtnExhibitionCode').val();
                        formData.SN = data.SN;
                        layer.confirm("確定要儲存？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            if (formData.CustomerName == "" || formData.CustomerName == null) {
                                showMsg("請輸入公司名稱", 'info');
                                layer.close(index);
                                return;
                            }
                            if (formData.Contactor == "" || formData.Contactor == null) {
                                showMsg("請輸入聯絡人", 'info');
                                layer.close(index);
                                return;
                            }
                            g_api.ConnectLite('BusinessOpportunity_Qry', 'Update', formData,
                                function (res) {
                                    if (res.RESULT == '1') {
										showMsg(i18next.t("儲存成功"), 'success');
                                        //showMsgAndGo(i18next.t("儲存成功"), sProgramId, '?Action=Upd&SN=0'); // ╠message.Save_Success⇒新增成功╣
                                    } else {
                                        showMsg(i18next.t(""), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                                    }

                                },
                                function (res) {
                                    showMsg(i18next.t("儲存失敗"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                                }
                            )
                            layer.close(index);
                            layer.close(index - 1);

                        });
						return false;
                    },
                    end: function () {
                        oGrid.loadData();
                    }
                });
            },
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
			fnAddBusinessOpportunity = function (_guid1,_guid2, title) {
                layer.open({
                    type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: "建立潛在商機",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                    area: ['70%', '90%'],//寬度
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
                        iframe.find('[name = Effective][value = "2"]').click();
                        iframe.find("#State").html($("#State").html());
                        iframe.find("#Industry").html($("#Industry").html());
                    },
					yes: function (index, layero) {
                        layer.confirm("確定要儲存？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            var iframe = layero.find('iframe').contents();
                            var formData = getFormSerialize($(iframe.find('#form_main')));
                            if (formData.CustomerName == "" || formData.CustomerName == null) {
                                showMsg("請輸入公司名稱",'info');
                                layer.close(index);
                                return;
                            }
                            if (formData.Contactor == "" || formData.Contactor == null) {
                                showMsg("請輸入聯絡人", 'info');
                                layer.close(index);
                                return;
                            }
                            try {
                                formData.DateStart = $.trim(formData.Date.split('~')[0]);
                                formData.DateEnd = $.trim(formData.Date.split('~')[1]);
                            } catch (e) {
                                formData.DateStart = null;
                                formData.DateEnd = null;
                            }
                            formData.ExhibitionCode = iframe.find('#BtnExhibitionCode').val();
                            fnAdd(formData);
                            layer.close(index);
                            layer.close(index - 1);

                        });
                    },
                    end: function () {
                        oGrid.loadData();
                    }
                });
            },
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
                        
						fnAddBusinessOpportunity();
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
                        fnGet({ Excel: true });

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
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });
                let saIndustry;
                $.whenArray([
                    setStateDrop(),
                    fnSetArgDrop([
                        {
                            OrgID: 'TE',
                            ArgClassID: 'ExhibClass',
                            Select: $('#Industry'),
                            Select2: true,
                            ShowId: true
                        }
                    ]),
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            ShowId: true,
                            Select2: true
                        }
                    ]),
                    g_api.ConnectLite(Service.com, ComFn.GetArguments,
                        {
                            ArgClassID: "ExhibClass",
                            OrgID: 'TE'
                        },
                        function (res) {
                            if (res.RESULT) {
                                var saRes = res.DATA.rel;
                                saIndustry = saRes;
                            }
                        })
                ]).done(function () {
                    parent.open
                    reSetQueryPm(sProgramId);
                    var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
                    $("#jsGrid").jsGrid({
                        width: "100%",
                        height: iHeight + "px",
                        autoload: true,
                        pageLoading: true,
                        inserting: false,
                        editing: true,
                        sorting: true,
                        paging: true,
                        pageIndex: window.bToFirstPage ? 1 : window.QueryPageidx || 1,
                        pageSize: parent.SysSet.GridRecords || 10,
                        pageButtonCount: parent.SysSet.GridPages || 15,
                        pagePrevText: "<",
                        pageNextText: ">",
                        pageFirstText: "<<",
                        pageLastText: ">>",
                        rowClass: function (item) {
                            if (item.Effective === '0') {
                                return 'data-void';
                            }
                        },
                        onPageChanged: function (args) {
                            cacheQueryCondition(args.pageIndex);
                        },
                        rowClick: function (args) {
                            if (navigator.userAgent.match(/mobile/i)) {
                                if ('A,C'.indexOf(args.item.Status) > -1 && args.item.AskTheDummy === parent.UserID) {
                                    goToEdit(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                                else {
                                    goToEdit(sViewPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                            }
                        },
                        rowDoubleClick: function (args) {
                            if ('A,C'.indexOf(args.item.Status) > -1 && args.item.Applicant === parent.UserID) {
                                fnModifyBusinessOpportunity(args.item);
                            }
                            else {
                                fnModifyBusinessOpportunity(args.item);
                            }
                        },

                        fields: [
                            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 40, sorting: false },
                            { name: "Year", title: '展覽年分', align: 'center', width: 60 },
                            {
                                name: "Date", title: '展覽月分', width: 60, align: 'center', itemTemplate: function (val, item) {
                                    var DateStart = new Date(item['DateStart']);
                                    var DateEnd = new Date(item['DateEnd']);
                                    var DateStartVal = parseInt(DateStart.getMonth()) + 1;
                                    var DateStr = ''
                                    if (DateStart.getMonth() != DateEnd.getMonth()) {
                                        DateStr = DateStartVal;
                                    } else {
                                        DateStr = DateStartVal;
                                    }
                                    if (item['DateStart'] == null || item['DateEnd'] == null) {
                                        DateStr = '';
                                    }
                                        
                                    return DateStr;
                                }
                            },
                            { name: "ExhibitionName", title: '展覽名稱', width: 150, align: 'center' },
                            { name: "ExhibitionName_EN", title: '展覽英文名稱', width: 150, align: 'center' },
                            { name: "State", title: '國家', width: 60, align: 'center' },
                            {
                                name: "Industry", title: '產業別', width: 60, align: 'center', itemTemplate: function (val, item) {
                                    for (let idx in saIndustry) {
                                        var sId = saIndustry[idx]['id'] || '';
                                        var sVal = val || '';
                                        if (sId.trim() == sVal) {
                                            return saIndustry[idx]['text'];
                                        }
                                    }
                                    return "";
                                }
                            },
                            { name: "CustomerName", title: '公司名稱', width: 60, align: 'center' },
                            { name: "Contactor", title: '聯絡人', width: 60, align: 'center' },
                            { name: "CreateUser", title: 'common.CreateUser', width: 70, align: 'center' },
                            {
                                name: "CreateDate", title: 'common.CreateDate', width: 90, align: 'center', itemTemplate: function (val, item) {
                                    return newDate(val);
                                }
                            },
                            {
                                name: "ModifyDate", title: 'common.ModifyDate', width: 90, align: 'center', itemTemplate: function (val, item) {
                                    return newDate(val);
                                }
                            },
                            {
                                name: "Effective", title: '狀態', width: 70, align: 'center', itemTemplate: function (val, item) {
                                    if (val == "1") {
                                        return "已處理";
                                    } else if (val == '2') {
                                        return "未處裡";
                                    } else {
                                        return "已作廢";
                                    }
                                }
                            },
                        ],
                        controller: {
                            loadData: function (args) {
                                return fnGet(args);
                            },
                        },
                        onInit: function (args) {
                            oGrid = args.grid;
                        }
                    });
                })
            };
        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);