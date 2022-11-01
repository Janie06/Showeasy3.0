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

                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (args.Excel) {//匯出
                            DownLoadFile(oRes);
                        }
                    }
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object} inst 按鈕物件對象
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

                fnSetUserDrop([
                    {
                        Select: $('#AskTheDummy'),
                        ShowId: true,
                        Select2: true,
                        CallBack: function () {
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
                                    if (item.Status === 'X') {
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
                                    if ('A,C'.indexOf(args.item.Status) > -1 && args.item.AskTheDummy === parent.UserID) {
                                        parent.openPageTab(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                    }
                                    else {
                                        parent.openPageTab(sViewPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                    }
                                },

                                fields: [
                                    { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 40, sorting: false },
                                    { name: "SignedNumber", title: 'common.SignedNumber', width: 60 },
                                    {
                                        name: "KeyNote", title: 'common.KeyNote', width: 200, itemTemplate: function (val, item) {
                                            var sVal = val,
                                                oStatus = {
                                                    'A': i18next.t('common.Draft'),// ╠common.Draft⇒草稿╣
                                                    'B': i18next.t('common.InAudit'),// ╠common.InAudit⇒審核中╣
                                                    'C-O': i18next.t('common.HasReEdited'),// ╠common.HasReEdited⇒已抽單╣
                                                    'D-O': i18next.t('common.HasReturned'),// ╠common.HasReturned⇒已退件╣
                                                    'H-O': i18next.t('common.Hashandle'),// ╠common.Hashandle⇒已經辦╣
                                                    'E': i18next.t('common.ToHandle'),// ╠common.ToHandle⇒待經辦╣
                                                    'X': i18next.t('common.HasVoid')// ╠common.HasVoid⇒已作廢╣
                                                };
                                            sVal += oStatus[item.Status] ? '<span style="color:#DF5F09">(' + oStatus[item.Status] + ')</span>' : '';

                                            if (item.Important > 1) {
                                                for (var i = 0; i < item.Important - 1; i++) {
                                                    sVal += ' <img src="../../images/star.gif">';
                                                }
                                            }
                                            return $('<a>', { html: sVal });
                                        }
                                    },
                                    { name: "AskTheDummyName", title: 'common.Applicant', width: 70, align: 'center' },
                                    {
                                        name: "FillBrushDate", title: 'common.FillBrushDate', width: 90, align: 'center', itemTemplate: function (val, item) {
                                            return newDate(val, true);
                                        }
                                    },
                                    {
                                        name: "FillBrushType", title: 'common.FillBrushType', width: 90, align: 'center', itemTemplate: function (val, item) {
                                            // ╠common.AllDay⇒全天╣ ╠common.Morning⇒上午╣ ╠common.Afternoon⇒下午╣
                                            return val === 'O' ? i18next.t('common.AllDay') : (val === 'A' ? i18next.t('common.Morning') : i18next.t('common.Afternoon'));
                                        }
                                    },
                                    {
                                        name: "CheckFlows", title: 'common.Progress', width: 50, itemTemplate: function (val, item) {
                                            var saCheckFlows = $.parseJSON(val),
                                                iProgress = 0;
                                            var saNewList = Enumerable.From(saCheckFlows).GroupBy("$.Order").ToArray();
                                            $.each(saNewList, function (idx, _data) {
                                                var sSignedWay = _data.source[0].SignedWay,
                                                    iCount = Enumerable.From(_data.source).Where(function (e) { return e.SignedDecision !== ''; }).Count();
                                                if ((iCount === _data.source.length && sSignedWay !== 'flow3') || (iCount > 0 && sSignedWay === 'flow3')) {
                                                    iProgress = idx + 1;
                                                }
                                                else {
                                                    return false;
                                                }
                                            });
                                            return $('<a>', { html: iProgress + '/' + saNewList.length });
                                        }
                                    },
                                    {
                                        name: "CheckFlows", title: 'common.CurAuditor', width: 100, itemTemplate: function (val, item) {
                                            var saCheckFlows = $.parseJSON(val),
                                                sCurAuditor = '';
                                            var saNewList = Enumerable.From(saCheckFlows).GroupBy("$.Order").ToArray();
                                            $.each(saNewList, function (idx, _data) {
                                                var sSignedWay = _data.source[0].SignedWay,
                                                    sFlowType = i18next.t('common.' + sSignedWay),
                                                    iCount = Enumerable.From(_data.source).Where(function (e) { return e.SignedDecision !== ''; }).Count();
                                                if (('flow1,flow3'.indexOf(sSignedWay) > -1 && iCount === 0) || (sSignedWay === 'flow2' && iCount !== _data.source.length)) {
                                                    sCurAuditor = Enumerable.From(_data.source).ToString("，", "$.SignedMember");
                                                    if (sSignedWay !== 'flow1') {
                                                        sCurAuditor = sFlowType + '(' + sCurAuditor + ')';
                                                    }
                                                    return false;
                                                }
                                            });
                                            if (sCurAuditor === '') {
                                                var saCheckFlows = $.parseJSON(item.HandleFlows);
                                                if (saCheckFlows[0].SignedDecision !== 'Y') {
                                                    sCurAuditor = saCheckFlows[0].SignedMember;
                                                }
                                            }
                                            return $('<a>', { html: sCurAuditor.length > 11 ? sCurAuditor.substr(0, 11) + '...' : sCurAuditor, title: sCurAuditor });
                                        }
                                    },
                                    {
                                        name: "CreateDate", title: 'common.CreateDate', width: 90, align: 'center', itemTemplate: function (val, item) {
                                            return newDate(val);
                                        }
                                    }
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
                        }
                    }
                ]);
            };

        init();
    };
require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);