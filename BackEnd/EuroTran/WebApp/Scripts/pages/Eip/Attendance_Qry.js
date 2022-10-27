'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            saLeave = [],
            saBusTrip = [],
            saOvertime = [],
            saAttendanceDiff = [],
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CardDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param {Object}  args  查詢條件參數
             * @return {Object} Ajax返回物件
             */
            fnGet = function (args) {
                var oQueryPm = {},
                    oQuery = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;
                $.extend(oQueryPm, oQuery);

                return $.whenArray([
                    g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm),
                    g_api.ConnectLite(sProgramId, 'GetLeave',
                        {
                            UserIDs: oQuery.UserIDs,
                            DateStart: oQueryPm.DateStart,
                            DateEnd: oQueryPm.DateEnd
                        },
                        function (res) {
                            if (res.RESULT) {
                                saLeave = res.DATA.rel;
                            }
                        }),
                    g_api.ConnectLite(sProgramId, 'GetBusTrip',
                        {
                            UserIDs: oQuery.UserIDs,
                            DateStart: oQueryPm.DateStart,
                            DateEnd: oQueryPm.DateEnd
                        },
                        function (res) {
                            if (res.RESULT) {
                                saBusTrip = res.DATA.rel;
                            }
                        }),
                    //g_api.ConnectLite(sProgramId, 'GetOvertime', {
                    //    UserIDs: oQuery.UserIDs,
                    //    DateStart: oQueryPm.DateStart,
                    //    DateEnd: oQueryPm.DateEnd
                    //}, function (res) {
                    //    if (res.RESULT) {
                    //        saOvertime = res.DATA.rel;
                    //    }
                    //}),
                    g_api.ConnectLite(sProgramId, 'GetAttendanceDiff',
                        {
                            UserIDs: oQuery.UserIDs,
                            DateStart: oQueryPm.DateStart,
                            DateEnd: oQueryPm.DateEnd
                        }
                        , function (res) {
                            if (res.RESULT) {
                                saAttendanceDiff = res.DATA.rel;
                            }
                        })
                ]);
            },
            /**
             * 處理請假，加班和出差資料的考情關係
             * @param   {Object} date 當前考勤資料日期
             * @param   {String} userid 人員id
             * @param   {Object} flag 請假出差標記
             * @return {Boolean} 是否停止
             */
            fnGetCorrect = function (date, userid, flag) {
                var oA = $('<a>', { class: 'a-url' }),
                    sPrgId = '',
                    oCorrect = {},
                    oStatus = {
                        'H-O': i18next.t('common.HasCompleted'),// ╠common.HasCompleted⇒已完成╣
                        'B': i18next.t('common.InAudit'),// ╠common.InAudit⇒審核中╣
                        'E': i18next.t('common.ToHandle'),// ╠common.ToHandle⇒待經辦╣
                    };
                if (flag === 1) {
                    sPrgId = 'Leave_View';
                    $.each(saLeave, function (idx, item) {
                        var rDate = new Date(newDate(date, true));
                        if (userid === item.AskTheDummy && rDate >= new Date(newDate(item.StartDate, true)) && rDate <= new Date(newDate(item.EndDate, true))) {
                            oCorrect = item;
                            return false;
                        }
                    });
                }
                else if (flag === 2) {
                    sPrgId = 'BusinessTravel_View';
                    $.each(saBusTrip, function (idx, item) {
                        var rDate = new Date(newDate(date, true));
                        if (userid === item.AskTheDummy && rDate >= new Date(newDate(item.StartDate, true)) && rDate <= new Date(newDate(item.EndDate, true))) {
                            oCorrect = item;
                            return false;
                        }
                    });
                }
                else if (flag === 3) {
                    sPrgId = 'OverTime_View';
                    $.each(saOvertime, function (idx, item) {
                        var rDate = new Date(newDate(date, true));
                        if (userid === item.AskTheDummy && rDate >= new Date(newDate(item.StartDate, true)) && rDate <= new Date(newDate(item.EndDate, true))) {
                            oCorrect = item;
                            return false;
                        }
                    });
                }
                else if (flag === 4) {
                    sPrgId = 'AttendanceDiff_View';
                    $.each(saAttendanceDiff, function (idx, item) {
                        var rDate = newDate(date, true);
                        if (userid === item.AskTheDummy && rDate === newDate(item.FillBrushDate, true)) {
                            oCorrect = item;
                            return false;
                        }
                    });
                }
                oA.html(oStatus[oCorrect.Status]).click(function () {
                    parent.openPageTab(sPrgId, '?Action=Upd&Guid=' + oCorrect.Guid);
                });
                return oA;
            },
            /**
             * 匯出資料
             */
            fnExcel = function () {
                var oQuery = getFormSerialize(oForm);

                g_api.ConnectLite(sProgramId, 'GetExcel', {
                    UserIDs: oQuery.UserIDs,
                    DateStart: oQuery.DateStart,
                    DateEnd: oQuery.DateEnd
                }, function (res) {
                    if (res.RESULT) {
                        DownLoadFile(res.DATA.rel);
                    }
                });
            },
            /**
             * 同步打卡資料
             * @param {Boolean} isreset 是否重置
             * @return {Boolean} 是否停止
             */
            fnTransferEip = function (isreset) {
                var sDateStart = $('#DateStart').val(),
                    sDateEnd = $('#DateEnd').val();
                if (sDateStart === '' || sDateEnd === '') {
                    showMsg(i18next.t("message.SynchronousDateInterval_Required")); //╠message.SynchronousDateInterval_Required⇒請選擇要同步的時間區間╣
                    return false;
                }

                g_api.ConnectLite(sProgramId, 'TransferEip', {
                    DateStart: sDateStart,
                    DateEnd: sDateEnd,
                    IsReSet: isreset
                }, function (res) {
                    if (res.RESULT) {
                        oGrid.loadData();
                        showMsg(i18next.t("message.SynchronousSuccess"), 'success'); //╠message.SynchronousSuccess⇒同步成功╣
                    }
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object} inst 按鈕物件對象
             * @param   {Object} e 事件對象
             * @return {Boolean} 是否停止
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

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_TransferEip":

                        fnTransferEip(true);

                        break;
                    case "Toolbar_ReSetSynchronous":

                        fnTransferEip(true);

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
             * init 初始化
             */
            init = function () {
                var saCusBtns = [];
                saCusBtns.push({
                    id: 'Toolbar_TransferEip',
                    value: 'common.Synchronous'// ╠common.Synchronous⇒同步╣
                });
                //if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                //    saCusBtns.push({
                //        id: 'Toolbar_ReSetSynchronous',
                //        value: 'common.ReSetSynchronous'// ╠common.ReSetSynchronous⇒同步重置╣
                //    });
                //}

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    SearchBar: true
                });

                if (!(parent.UserInfo.roles.indexOf('Admin') > -1 || parent.UserInfo.roles.indexOf('EipManager') > -1 || parent.UserInfo.roles.indexOf('EipView') > -1)) {
                    $('.eipmg').remove();
                    $('.noteipmg').show();
                }
                else {
                    $('.noteipmg').remove();
                }

                $('.add-on').click(function () {
                    var oOption = {};
                    oOption.Flowtype = true;
                    oOption.Callback = function (data) {
                        var saSelectedUser = [],
                            saSearchUserIds = [];
                        if (data.Users.length > 0) {
                            $.each(data.Users, function (idx, user) {
                                saSelectedUser.push(user.name);
                                saSearchUserIds.push(user.id);
                            });
                        }
                        $('#UserName').val(saSelectedUser.join(','));
                        $('#UserIDs').val(saSearchUserIds.join(','));
                    };
                    oPenUserListPop(oOption);
                });

                reSetQueryPm(sProgramId);
                var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
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
                    fields: [
                        { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                        {
                            name: "OrgID", title: 'Organization_Upd.OrgID', align: "center", type: "text", width: 80
                        },
                        {
                            name: "CardUserName", title: 'common.FullName', align: "left", type: "text", width: 100
                        },
                        {// ╠common.Date⇒日期╣
                            name: "CardDate", title: 'common.Date', align: "center", type: "text", width: 100,
                            itemTemplate: function (val, item) {
                                return newDate(val, true);
                            }
                        },
                        {// ╠common.TimeA⇒上班/刷卡╣
                            name: "TimeA", title: 'common.TimeA', type: "text", align: "center", width: 120,
                            itemTemplate: function (val, item) {
                                var sText = '-----';
                                if (item.TimeA) {
                                    sText = $.grep(item.TimeA.split(':'), function (e, i) { return i !== 2; }).join(':') + ' / ' + (item.StatusA ? '<span class="t-red">' + item.SignIn.substr(0, 5) + '</span>' : item.SignIn.substr(0, 5));
                                }
                                return sText;
                            }
                        },
                        {// ╠common.TimeP⇒下班/刷卡╣
                            name: "TimeP", title: 'common.TimeP', type: "text", align: "center", width: 120,
                            itemTemplate: function (val, item) {
                                var sText = '-----';
                                if (item.TimeP) {
                                    if (item.SignOut) {
                                        sText = item.TimeP + ' / ' + (item.StatusP ? '<span class="t-red">' + item.SignOut.substr(0, 5) + '</span>' : item.SignOut.substr(0, 5));
                                    }
                                }
                                return sText;
                            }
                        },
                        {// ╠common.WorkHours⇒時數╣
                            name: "Hours", title: 'common.WorkHours', align: "center", type: "text", width: 50
                        },
                        {// ╠common.LackHours⇒欠勤╣
                            name: "Hours", title: 'common.LackHours', align: "center", type: "text", width: 50,
                            itemTemplate: function (val, item) {
                                var sText = '--';
                                if (item.Hours === '0' || !!Number(item.Hours)) {
                                    let MaxLimitedHours = 9.00;
                                    let ActualHours = Number(item.Hours);
                                    let LackHour = parseFloat((MaxLimitedHours - ActualHours).toFixed(2));;
                                    sText = Math.ceil(LackHour);
                                }
                                return sText;
                            }
                        },
                        {
                            name: "Memo", title: 'common.Memo', type: "text", width: 100
                        },
                        {// ╠common.AskForLeave⇒請假╣
                            name: "CardDate", title: 'common.AskForLeave', align: "center", type: "text", width: 80, sorting: false,
                            itemTemplate: function (val, item) {
                                return fnGetCorrect(val, item.UserID, 1);
                            }
                        },
                        {// ╠common.OnBusinessTrip⇒出差╣
                            name: "CardDate", title: 'common.OnBusinessTrip', align: "center", type: "text", width: 80, sorting: false,
                            itemTemplate: function (val, item) {
                                return fnGetCorrect(val, item.UserID, 2);
                            }
                        },
                        //{// ╠common.WorkOvertime⇒加班╣
                        //    name: "CardDate", title: 'common.WorkOvertime', align: "center", type: "text", width: 100, sorting: false,
                        //    itemTemplate: function (val, item) {
                        //        return fnGetCorrect(val, item.UserID, 3);
                        //    }
                        //},
                        {// ╠common.AttendanceDiff⇒差勤異常╣
                            name: "CardDate", title: 'common.AttendanceDiff', align: "center", type: "text", width: 80, sorting: false,
                            itemTemplate: function (val, item) {
                                return fnGetCorrect(val, item.UserID, 4);
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
            };

        init();
    };
require(['base', 'select2', 'jsgrid', 'filer', 'common_eip', 'util'], fnPageInit);