'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var setStateDrop = function () {
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
        };
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'ExhibitionCode',
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
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['300px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                             <button type="button" data-i18n="common.BasicInformation" id="Exhibition_BasicInformation" class="btn-custom green">基本資料</button>\
                             <button type="button" data-i18n="common.WenzhongPrjFile" id="Exhibition_WenzhongPrjFile" class="btn-custom green">文中專案檔</button>\
                         </div>',//╠common.BasicInformation⇒基本資料╣╠common.WenzhongPrjFile⇒文中專案檔╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            fnGet({
                                Excel: true,
                                ExcelType: sToExcelType,
                                Index: idx
                            });
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
             * 初始化 function
             */
            init = function () {
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });
                var saFields = [
                    {
                        name: "RowIndex", title: 'common.RowNumber', type: "text", width: 50, align: "center", sorting: false
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
                    }];

                if (parent.OrgID === 'TG') {
                    $('.box-notte').show();
                    saFields.push({
                        name: "AreaName", title: 'Exhibition_Upd.Area', type: "text", width: 150
                    },
                        {
                            name: "StateName", title: 'Exhibition_Upd.State', type: "text", width: 150
                        },
                        {
                            name: "IsShowWebSite", title: 'Exhibition_Upd.IsShowWebSite', type: "text", align: "center", width: 100,
                            itemTemplate: function (val, item) {
                                return val === 'Y' ? i18next.t('common.Yes') : i18next.t('common.No');
                            }
                        });
                }
                else {
                    saFields.push({
                        name: "CreateUserName", title: 'common.CreateUser', type: "text", width: 70
                    },
                        {
                            name: "CreateDate", title: 'common.CreateDate', type: "text", align: "center", width: 100, itemTemplate: function (val, item) {
                                return newDate(val);
                            }
                        },
                        {
                            name: "ModifyDate", title: 'common.ModifyDate', type: "text", align: "center", width: 100, itemTemplate: function (val, item) {
                                return newDate(val);
                            }
                        });
                }
                saFields.push(
                    {
                        name: "Effective", title: 'common.Status', type: "text", width: 50, align: "center", itemTemplate: function (val, item) {
                            return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');// ╠common.Effective⇒有效╣ ╠common.Invalid⇒無效╣
                        }
                    },
                    {
                        name: "IsTransfer", title: 'common.Transfer_Status', type: "text", width: 50, align: "center", itemTemplate: function (val, item) {
                            return val === 'Y' ? i18next.t('common.Transfer_Yes') : i18next.t('common.Transfer_No');// ╠common.Transfer_Yes⇒已拋轉╣ ╠common.Transfer_No⇒未拋轉╣
                        }
                    });

                $.whenArray([
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            Select2: true,
                            ShowId: true
                        }
                    ]),
                    setStateDrop()])
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
                            if (navigator.userAgent.match(/mobile/i)) {
                                goToEdit(sEditPrgId, '?Action=Upd&SN=' + args.item.SN);
                            }
                        },
                        rowDoubleClick: function (args) {
                            parent.openPageTab(sEditPrgId, '?Action=Upd&SN=' + args.item.SN);
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

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);