'use strict';
var sProgramId = getProgramId(),
    fnPageInit = function () {
        var oLeaveSet = null,
            oChangeRecords = null,
            oGrid = null,
            oGrid1 = null,
            oGrid2 = null,
            oGrid3 = null,
            oSet = null,
            sHtmlCategory = '',
            sCurrentUserId = parent.UserID,
            sCurrentYear = new Date().formate('yyyy'),
            oBaseQueryPm = {
                sortField: 'CreateDate',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param  {Object} args 查詢參數
             * @return  {Object} ajax物件
             */
            fnGetLeaveList = function (args) {
                var oQueryPm = {};

                $.extend(oQueryPm, oBaseQueryPm, args);
                oQueryPm.AskTheDummy = sCurrentUserId;
                oQueryPm.Status = 'B,H-O,E';
                oQueryPm.LeaveDateStart = sCurrentYear + '/01/01';
                oQueryPm.LeaveDateEnd = sCurrentYear + '/12/31';

                return g_api.ConnectLite('Leave_Qry', ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        $('.currentyear').text(sCurrentYear);
                    }
                });
            },
            /**
             * 獲取請假設定
             * @return  {Object} ajax物件
             */
            fnGetLeaveSet = function () {
                var limited = g_api.ConnectLite('LeaveRequest_Qry', 'GetAllLeaveRequest', {
                    UserID: sCurrentUserId,
                    OrgID: parent.OrgID,
                    CurrentYear: sCurrentYear
                });

                return $.whenArray([CallAjax(ComFn.W_Com, ComFn.GetPagePrc, {
                    Type: 'wenzhong_getlist',
                    Params: {
                        querysort: '',
                        pageindex: 1,
                        pagesize: 100,
                        UserID: sCurrentUserId,
                        Date: sCurrentYear,
                        LeaveDate: '',
                        OrgID: parent.OrgID
                    }
                }),
                CallAjax(ComFn.W_Com, ComFn.GetOne, {
                    Type: '',
                    Params: {
                        leaveset: {
                            OrgID: parent.OrgID,
                            UserID: sCurrentUserId,
                            TYear: sCurrentYear
                        }
                    }
                }),
                limited
                ]).done(function (res1, res2, res3) {
                    var OtherLeave = [];
                    if (res2[1] === 'success') {
                        oSet = $.parseJSON(res2[0].d);
                        var saSet = $.parseJSON(oSet.SetInfo),
                            oTXJ = {};
                        saSet = saSet || [];
                        
                        if (oSet.SetInfo) {
                            $('#InitLeaveSet').hide();
                        }
                        else {
                            if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                                $('#InitLeaveSet').show();
                            }
                            else {
                                $('#InitLeaveSet').hide();
                            }
                        }

                        if (res1[1] === 'success') {
                            oTXJ = $.parseJSON(res1[0].d === '' ? '[]' : res1[0].d);
                        }
                        if (res3[1] === 'success') {
                            OtherLeave = res3[0].DATA.rel;
                        }

                        $.each(saSet, function (idx, set) {
                            var iPaymentHours = 0,
                                iUsedHours = 0,
                                iRemainHours = 0,
                                saMemo = [];
                            if (set.Id === '09') {
                                $.each(oTXJ.DataList, function (idx, txj) {
                                    var _iRemainHours = (txj.RemainHours || '0') * 1;
                                    iPaymentHours += txj.PaymentHours * 1;
                                    iUsedHours += txj.UsedHours * 1;
                                    iRemainHours += _iRemainHours;
                                    saMemo.push(newDate(txj.EnableDate, true) + '~' + newDate(txj.ExpirationDate, true) + '：' + txj.PaymentHours + '/' + txj.RemainHours + '(' + i18next.t('common.Hours') + ')');
                                });
                                set.PaymentHours = iPaymentHours;
                                set.UsedHours = iUsedHours;
                                set.RemainHours = iRemainHours;
                                set.Memo = saMemo.join(' + ');
                            }
                            else {
                                let MatchtedLeaveRequests = Enumerable.From(OtherLeave).Where(function (item) { return item.Leave === set.Id; }).ToArray();
                                if (MatchtedLeaveRequests.length > 0) {
                                    $.each(MatchtedLeaveRequests, function (idx, mlr) {
                                        console.log([ 'mlr', mlr]);
                                        iPaymentHours += mlr.PaymentHours * 1;
                                        iUsedHours += mlr.UsedHours * 1;
                                        iRemainHours += mlr.RemainHours * 1;
                                        saMemo.push(newDate(mlr.EnableDate, true) + '~' + newDate(mlr.ExpirationDate, true) + '：' + mlr.PaymentHours + '/' + mlr.RemainHours + '(hr)');
                                    });
                                    if (set.PaymentHours) {
                                        set.PaymentHours = (parseFloat(set.PaymentHours) + parseFloat(iPaymentHours)).toString();
                                    }
                                    set.UsedHours = (parseFloat(set.UsedHours ||  '0') + parseFloat(iUsedHours)).toString();
                                    set.RemainHours = (parseFloat(set.RemainHours || '0') + parseFloat(iRemainHours)).toString();
                                    set.Memo = saMemo.join(' + ');
                                }
                            }
                        });

                        fnGetChangeRecords();

                        oLeaveSet = {
                            data: saSet,
                            itemsCount: saSet.length
                        };
                        oGrid1.loadData({ IsFilter: true });
                    }
                });
            },
            /**
             * 獲取請假設定變更記錄
             * @return  {Object} ajax物件
             */
            fnGetChangeRecords = function () {

                var param = $.extend({}, { UserId: sCurrentUserId, SouseId: oSet.Guid, LogType: 'leavesetchange,WenZhongChange' });
                g_api.ConnectLite(sProgramId, "GetChangLog", param, function (res) {
                    if (res.RESULT) {
                        var saChangeRecords = $.parseJSON(res.DATA.rel);

                        oChangeRecords = {
                            data: saChangeRecords,
                            itemsCount: saChangeRecords.length
                        };
                        oGrid2.loadData({ IsFilter: true });
                    }
                });
            },
            /**
            * 設置人員菜單
             * @return  {Object} ajax物件
            */
            fnSetMemberMenu = function () {
                return g_api.ConnectLite(Service.sys, 'GetAllMembersByUserId', {},
                    function (res) {
                        if (res.RESULT) {
                            var saUsers = res.DATA.rel,
                                saTop = [],
                                saTreeList = [],
                                saPass = [],
                                fnGetParent = function (list, parentid) {
                                    var saParent = Enumerable.From(list).Where(function (item) { return item.ID === parentid; }).ToArray();
                                    return saParent.length > 0 ? saParent[0] : {};
                                };

                            $.each(saUsers, function (idx, tb) {
                                var oParent = fnGetParent(saUsers, tb.ParentID);
                                if (tb.level === '1' && !oParent.ID && saTop.indexOf(tb.ParentID) === -1) {
                                    saTop.push(tb.ParentID);
                                }
                            });
                            $.each(saTop, function (idx, topid) {
                                if (saPass.indexOf(topid) === -1) {
                                    var saList = new TreeMenu(saUsers).init(topid, 'ParentID', 'ID', 'Name');
                                    $.each(saList, function (idx, _list) {
                                        saTreeList.push(_list);
                                    });
                                    saPass.push(topid);
                                }
                            });
                            var fnCallBack = function (data) {
                                sCurrentUserId = data.id;
                                oBaseQueryPm.IsFilter = undefined;
                                oGrid.openPage(1);
                                fnGetLeaveSet();
                            };
                            $('#tree_menu').tree({
                                data: saTreeList,
                                autoOpen: false
                            });
                            bindTreeEvent(fnCallBack);
                            var node_Dept = $('#tree_menu').tree('getNodeById', parent.UserInfo.DepartmentID),
                                node_Member = $('#tree_menu').tree('getNodeById', parent.UserID);
                            $('#tree_menu').tree('openNode', node_Dept);
                            $(node_Member.element).addClass('jqtree-selected');
                        }
                    });
            },
            /**
             * @param {Object} item 要變更的設定
             */
            fnUpdateLeaveSet = function (item) {
                var oUpdPm = {},
                    oLogInfoAdd = {
                        OrgID: parent.OrgID,
                        SouseId: oSet.Guid,
                        LogType: 'leavesetchange',
                        CreateUser: parent.UserID,
                        CreateDate: newDate()
                    },
                    oLogInfo = {
                        ChangeRecord: item.Name + '：' + (item.PaymentHoursOld === '' ? i18next.t('common.NoLimit') : item.PaymentHoursOld + i18next.t('common.Hours')) + ' => ' + item.PaymentHours + i18next.t('common.Hours'),
                        ChangeUserName: parent.UserInfo.MemberName,
                        Memo: item.Memo
                    },
                    oLeaveSetUpd = {};
                oLogInfoAdd.LogInfo = JSON.stringify(oLogInfo);

                oLeaveSetUpd.SetInfo = JSON.stringify(oLeaveSet.data);

                oUpdPm.upd = {
                    leaveset: {
                        values: oLeaveSetUpd,
                        keys: {
                            Guid: oSet.Guid
                        }
                    }
                };

                oUpdPm.add = {};
                oUpdPm.add.loginfo = oLogInfoAdd;

                CallAjax(ComFn.W_Com, ComFn.GetTran, {
                    Params: oUpdPm
                }, function (res) {
                    if (res.d > 0) {
                        fnGetLeaveSet();
                        showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                    }
                    else {
                        showMsg(i18next.t("message.SetUp_Failed") + '<br>' + res.MSG, 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                });
            },
            /**
             * @param {Object} item 要變更的特休假信息
             */
            fnUpdateLeaveTX = function (item) {
                var param = $.extend({}, { UserId: sCurrentUserId, SouseId: oSet.Guid }, item);
                g_api.ConnectLite(sProgramId, 'UpdateLeaveTX', param, function (res) {
                    if (res.RESULT) {
                        fnGetLeaveSet();
                        showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                    }
                    else {
                        showMsg(i18next.t("message.SetUp_Failed") + '<br>' + res.MSG, 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                });
            },
            /**
             * 初始化差勤設定
             */
            fnInitLeaveSet = function () {
                g_api.ConnectLite(sProgramId, 'InitLeaveSet', {
                    CurrentYear: sCurrentYear
                }, function (res) {
                    if (res.RESULT) {
                        fnGetLeaveSet();
                        showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                    }
                    else {
                        showMsg(i18next.t("message.SetUp_Failed") + '<br>' + res.MSG, 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.SetUp_Error"), 'error'); // ╠message.SetUp_Error⇒設置異常╣
                });
            },
            /**
             * 抓取請假規則設定
             * @return {Object} ajax
             */
            fnGetLeaveSetting = function () {
                return g_api.ConnectLite(sProgramId, 'GetLeaveSetting', {});
            },
            /**
             * 修改請假規則設定
             * @param {Object} args 參數
             * @return {Function} Ajax
             */
            fnUpdateLeaveSetting = function (args) {
                return g_api.ConnectLite(sProgramId, 'UpdateLeaveSetting', args, function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                    }
                    else {
                        showMsg(i18next.t("message.SetUp_Failed") + '<br>' + res.MSG, 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.SetUp_Error"), 'error');//╠message.SetUp_Error⇒修改資料異常╣
                });
            },
            /**
             * 是否小數控件
             * @param {Sring}flag 要綁定的資料標記
             * @return {HTMLElement} DIV 物件
             */
            fnCreateRadiosInput = function (flag) {
                var $div = $('<div>'),
                    data = [{ id: 'Y', text: i18next.t('common.Yes') }, { id: 'N', text: i18next.t('common.No') }];
                $div.html(createRadios(data, 'id', 'text', '~DecimalFraction' + flag, flag, false, false));
                return $div;
            },
            /**
             * @param   {Object} inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        oBaseQueryPm.IsFilter = undefined;
                        oGrid.openPage(1);

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
                    case "Toolbar_Exp":

                        break;
                    case "Toolbar_Imp":

                        break;
                    case "PreviousYear":
                        sCurrentYear = (sCurrentYear * 1 - 1).toString();
                        oBaseQueryPm.IsFilter = undefined;
                        oGrid.openPage(1);
                        oGrid1.openPage(1);
                        break;
                    case "NextYear":
                        sCurrentYear = (sCurrentYear * 1 + 1).toString();
                        oBaseQueryPm.IsFilter = undefined;
                        oGrid.openPage(1);
                        oGrid1.openPage(1);
                        break;
                    case "InitLeaveSet":
                        if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                            fnInitLeaveSet();
                        }
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
                    SearchBar: true,
                    GoTop: true,
                    Buttons: [{
                        id: 'PreviousYear',
                        value: 'common.Toolbar_PreviousYear'// ╠common.Toolbar_PreviousYear⇒上一年╣
                    },
                    {
                        id: 'NextYear',
                        value: 'common.Toolbar_NextYear'// ╠common.Toolbar_NextYear⇒下一年╣
                    },
                    {
                        id: 'InitLeaveSet',
                        value: 'common.InitLeaveSet'// ╠common.InitLeaveSet⇒初始化差勤設定╣
                    }]
                });

                fnSetArgDrop([
                    {
                        ArgClassID: 'LeaveType',
                        CallBack: function (data) {
                            sHtmlCategory = createOptions(data, 'id', 'text', true);
                        }
                    }
                ]);
                fnSetMemberMenu();

                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: 1000,
                    fields: [
                        { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 40, sorting: false },
                        { name: "HolidayCategoryName", title: 'common.HolidayCategory', width: 60 },
                        {
                            name: "KeyNote", title: 'common.KeyNote', width: 200, itemTemplate: function (val, item) {
                                var sVal = val;

                                if (item.Important > 1) {
                                    for (var i = 0; i < item.Important - 1; i++) {
                                        sVal += ' <img src="../../images/star.gif">';
                                    }
                                }
                                return $('<a>', { html: sVal });
                            }
                        },
                        { name: "LeaveReason", title: 'common.LeaveReason', width: 150, align: 'center' },
                        { name: "TotalTime", title: 'common.TotalTime', align: 'center', width: 50 },
                        { name: "Agent_PersonName", title: 'common.AgentPerson', width: 100, align: 'center' },
                        {
                            name: "StartDate", title: 'common.StartDate', width: 100, align: 'center', itemTemplate: function (val, item) {
                                return newDate(val);
                            }
                        },
                        {
                            name: "EndDate", title: 'common.EndDate', width: 100, align: 'center', itemTemplate: function (val, item) {
                                return newDate(val);
                            }
                        },
                        {
                            name: "Status", title: 'common.Status', width: 100, align: 'center', itemTemplate: function (val, item) {
                                var oStatus = {
                                    'B': i18next.t('common.InAudit'),// ╠common.InAudit⇒審核中╣
                                    'E': i18next.t('common.ToHandle'),// ╠common.ToHandle⇒待經辦╣
                                    'H-O': i18next.t('common.HasCompleted')// ╠common.HasCompleted⇒已完成╣
                                };
                                return oStatus[item.Status] ? '<span style="color:#DF5F09">' + oStatus[item.Status] + '</span>' : '';
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetLeaveList(args);
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
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: true,
                    pageIndex: 1,
                    pageSize: 1000,
                    fields: [
                        {
                            name: "Name", title: 'common.HolidayCategory', width: 70, itemTemplate: function (val, item) {
                                return item.Id + '-' + item.Name;
                            }
                        },
                        {// ╠common.AvailableHours⇒可用時數╣ = 剩餘時數 + 已用時數 
                            name: "PaymentHours", title: 'common.AvailableHours', width: 50, align: 'center',
                            itemTemplate: function (val, item) {
                                var oControl = $('<a>', {
                                    html: fMoney(val * 1, 2),
                                    style: 'color: #337ab7;text-decoration: underline !important',
                                    click: function () {
                                        var isAdmin = parent.UserInfo.roles.indexOf('Admin') > -1;
                                        var isEipManger = parent.UserInfo.roles.indexOf('EipManager') > -1;
                                        if (!isAdmin && !isEipManger) {
                                            return false;
                                        }
                                        if (item.Id == "09" && !isAdmin) {
                                            return false;
                                        }

                                        //╠common.HolidayCategory⇒假別╣╠common.OriginalHours⇒原本時數╣╠common.Hours⇒小時╣╠common.AddLeaveHours⇒新增時數╣╠common.Memo⇒備註╣
                                        var sContent = '\
                                        <div class="row popsrow">\
                                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.HolidayCategory">假別</span>：</label>\
                                             <div class="col-sm-8">\
                                                 <select class="form-control" id="HolidayCategory" disabled></select>\
                                             </div>\
                                        </div>\
                                        <div class="row popsrow">\
                                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.OriginalHours">原本時數</span>：</label>\
                                             <label class="col-sm-8 show-text"><span id="OriginalHours"></span><span data-i18n="common.Hours">小時</span></label>\
                                        </div>\
                                        <div class="row popsrow">\
                                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.AddLeaveHours">新增時數</span>：</label>\
                                             <div class="col-sm-8">\
                                                 <input type="text" class="form-control w100p" id="AddLeaveHours" maxlength="10">\
                                             </div>\
                                        </div>\
                                        <div class="row popsrow">\
                                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.Memo">備註</span>：</label>\
                                             <div class="col-sm-8">\
                                                 <textarea id="Memo" class="form-control" rows="5" cols="20"></textarea>\
                                             </div>\
                                        </div>';
                                        layer.open({
                                            type: 1,
                                            title: i18next.t('common.ChangeLeaveHours'),// ╠common.ChangeLeaveHours⇒變更可用假數╣
                                            shadeClose: false,
                                            shade: 0.1,
                                            maxmin: true, //开启最大化最小化按钮
                                            area: ['400px', '350px'],
                                            content: sContent,
                                            success: function (layero, index) {
                                                layero.find('#HolidayCategory').html(sHtmlCategory).val(item.Id).find('option:first').remove();
                                                layero.find('#OriginalHours').text(fMoney(item.PaymentHours * 1, 2));
                                                layero.find('#AddLeaveHours').on('keyup blur', function (e) {
                                                    keyIntp(e, this, 1);
                                                });
                                            },
                                            btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                                            yes: function (index, layero) {
                                                var sHolidayCategory = layero.find('#HolidayCategory').val(),
                                                    sAddLeaveHours = layero.find('#AddLeaveHours').val(),
                                                    sMemo = layero.find('#Memo').val(),
                                                    iTotal = 0;
                                                iTotal = item.PaymentHours * 1 + sAddLeaveHours * 1;
                                                if (sAddLeaveHours === '' || sAddLeaveHours === 0) {
                                                    showMsg(i18next.t("message.PaymentHoursTips")); // ╠message.PaymentHoursTips⇒修改時數不可以是空或0╣
                                                    return false;
                                                }
                                                if (iTotal < 0) {
                                                    showMsg(i18next.t("message.PaymentHoursWarnning")); // ╠message.PaymentHoursWarnning⇒可用時數不可小於0╣
                                                    return false;
                                                }
                                                if (item.Id === '09' && sAddLeaveHours * 1 > 0) {
                                                    showMsg(i18next.t("message.PaymentHoursTXTips")); // ╠message.PaymentHoursTXTips⇒特休假時數不可以增加╣
                                                    return false;
                                                }
                                                item.PaymentHoursOld = item.PaymentHours;
                                                item.PaymentHours = iTotal;
                                                item.RemainHours = item.PaymentHours * 1 - item.UsedHours * 1;
                                                item.Memo = sMemo;
                                                if (item.Id === '09') {
                                                    //showMsg(i18next.t("message.TXJSetWarnning")); // ╠message.TXJSetWarnning⇒特休假不可以設定╣
                                                    //return false;
                                                    item.AddLeaveHours = sAddLeaveHours;
                                                    fnUpdateLeaveTX(item);
                                                }
                                                else {
                                                    fnUpdateLeaveSet(item);
                                                }
                                                layer.close(index);
                                            }
                                        });
                                    }
                                });
                                if (val === '') {
                                    oControl.html(i18next.t('common.NoLimit'));// ╠common.NoLimit⇒不限╣
                                }
                                return fMoney(val * 1, 2);
                                //return oControl;
                            }
                        },
                        {// ╠common.UsedHours⇒已用時數╣
                            name: "UsedHours", title: 'common.UsedHours', width: 50, align: 'center',
                            itemTemplate: function (val, item) {
                                return fMoney(val * 1, 2);
                            }
                        },
                        {// ╠common.RemainHours⇒剩餘時數╣
                            name: "RemainHours", title: 'common.RemainHours', width: 50, align: 'center',
                            itemTemplate: function (val, item) {
                                return fMoney(val * 1, 2);
                            }
                        },
                        { name: "Memo", title: 'common.Memo', width: 100 }
                    ],
                    controller: {
                        loadData: function (args) {
                            if (args.IsFilter === undefined) {
                                fnGetLeaveSet();
                            }
                            else {
                                return oLeaveSet;
                            }
                        }
                    },
                    onInit: function (args) {
                        oGrid1 = args.grid;
                    }
                });

                $("#jsGrid2").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: true,
                    pageIndex: 1,
                    pageSize: 1000,
                    fields: [
                        {// ╠common.ChangeRecord⇒變更記錄╣
                            name: "ChangeRecord", title: 'common.ChangeRecord', width: 100,
                            itemTemplate: function (val, item) {
                                var oLogInfo = $.parseJSON(item.LogInfo);
                                return oLogInfo.ChangeRecord;
                            }
                        },
                        {
                            name: "Memo", title: 'common.Memo', width: 100,
                            itemTemplate: function (val, item) {
                                var oLogInfo = $.parseJSON(item.LogInfo);
                                return oLogInfo.Memo;
                            }
                        },
                        {// ╠common.ChangeUser⇒變更者╣
                            name: "ChangeUser", title: 'common.ChangeUser', width: 50, align: 'center',
                            itemTemplate: function (val, item) {
                                var oLogInfo = $.parseJSON(item.LogInfo);
                                return oLogInfo.ChangeUserName;
                            }
                        },
                        {// ╠common.ChangeTime⇒	變更時間╣
                            name: "CreateDate", title: 'common.ChangeTime', width: 80, align: 'center',
                            itemTemplate: function (val, item) {
                                return newDate(val);
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            if (args.IsFilter === undefined) {
                                fnGetChangeRecords();
                            }
                            else {
                                return oChangeRecords;
                            }
                        }
                    },
                    onInit: function (args) {
                        oGrid2 = args.grid;
                    }
                });

                if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                    $("#litab2").show()
                    $("#jsGrid3").jsGrid({
                        width: "100%",
                        height: "auto",
                        autoload: true,
                        filtering: false,
                        inserting: false,
                        editing: true,
                        pageLoading: true,
                        invalidMessage: i18next.t('common.InvalidData'),// ╠common.InvalidData⇒输入的数据无效！╣
                        pageIndex: 1,
                        pageSize: 10000,
                        rowClick: function (args) {
                        },
                        fields: [
                            {
                                name: "ArgumentValue", title: 'common.HolidayCategory', width: 100, type: "text", align: "center", editing: false
                            },
                            {
                                name: "Memo", title: 'common.HolidayCategoryExplanation', width: 150, type: "text"// ╠common.HolidayCategoryExplanation⇒假別說明╣
                            },
                            {// ╠common.ExpenditureHours⇒可預支時數(h)╣
                                name: "Correlation", title: 'common.ExpenditureHours', width: 110, type: "text", align: "center",
                                editTemplate: function (val, item) {
                                    var oControl = $('<input />', {
                                        class: "form-control",
                                        'data-type': 'int',
                                        'data-name': 'int',
                                        value: item.Correlation
                                    });
                                    moneyInput(oControl, 0, true);
                                    return this.editControl = oControl;
                                },
                                editValue: function () {
                                    return this.editControl.attr('data-value') || '';
                                }
                            },
                            {// ╠common.MinimumLeaveHours⇒最小請假時數(h)╣
                                name: "ExFeild1", title: 'common.MinimumLeaveHours', width: 110, type: "text", align: "center",
                                editTemplate: function (val, item) {
                                    var oControl = $('<input />', {
                                        class: "form-control",
                                        'data-type': 'int',
                                        'data-name': 'int',
                                        value: item.ExFeild1
                                    });
                                    moneyInput(oControl, 2, true);
                                    return this.editControl = oControl;
                                },
                                editValue: function () {
                                    return this.editControl.attr('data-value') || '';
                                }
                            },
                            {// ╠common.LeaveInterval⇒每次請假間隔(h)╣
                                name: "ExFeild2", title: 'common.LeaveInterval', width: 110, type: "text", align: "center",
                                editTemplate: function (val, item) {
                                    var oControl = $('<input />', {
                                        class: "form-control",
                                        'data-type': 'int',
                                        'data-name': 'int',
                                        value: item.ExFeild2
                                    });
                                    moneyInput(oControl, 2, true);
                                    return this.editControl = oControl;
                                },
                                editValue: function () {
                                    return this.editControl.attr('data-value') || '';
                                }
                            },
                            {// ╠common.LeaveHoursPerMonth⇒每月請假最大時數╣
                                name: "ExFeild3", title: 'common.LeaveHoursPerMonth', width: 110, type: "text", align: "center",
                                editTemplate: function (val, item) {
                                    var oControl = $('<input />', {
                                        class: "form-control",
                                        'data-type': 'int',
                                        'data-name': 'int',
                                        value: item.ExFeild3
                                    });
                                    moneyInput(oControl, 2, true);
                                    return this.editControl = oControl;
                                },
                                editValue: function () {
                                    return this.editControl.attr('data-value') || '';
                                }
                            },
                            {// ╠common.DecimalFraction⇒是否小數╣
                                name: "ExFeild4", title: 'common.DecimalFraction', align: 'center', width: 100, type: "text",
                                itemTemplate: function (val, item) {
                                    return val === 'Y' ? i18next.t('common.Yes') : i18next.t('common.No');
                                },
                                editTemplate: function (val, item) {
                                    var oControl = fnCreateRadiosInput('edit');
                                    oControl.find(':input[value="' + (val || 'N') + '"]').click();
                                    uniformInit(oControl);
                                    return this.editControl = oControl;
                                },
                                editValue: function () {
                                    return this.editControl.find(':input:checked').val();
                                }
                            },
                            {
                                title: 'common.Action', type: "control", width: 50, deleteButton: false, editButton: parent.UserInfo.roles.indexOf('EipView') === -1
                            }
                        ],
                        controller: {
                            loadData: function (args) {
                                return fnGetLeaveSetting();
                            },
                            updateItem: function (args) {
                                fnUpdateLeaveSetting(args);
                            }
                        },
                        onInit: function (args) {
                            oGrid3 = args.grid;
                        }
                    });
                }
                else {
                    $("#litab2").hide()
                }
                
            };

        init();
    };

require(['base', 'jsgrid', 'jqtree', 'formatnumber', 'util'], fnPageInit);