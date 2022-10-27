'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sViewPrgId = sProgramId.replace('_Upd', '_View'),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckOrder: [] },
            oForm = $('#form_main'),
            oValidator = null,
            oGrid = null,
            sHolidays = '',
            iOneDayHours = 8,
            saUsers = [],
            /**
             * 獲取資料
             */
            fnGet = function () {
                if (sDataId) {
                    return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oCurData = oRes;
                                oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                                setFormVal(oForm, oRes);
                                $('#StartDate').val(newDate(oCurData.StartDate));
                                $('#EndDate').val(newDate(oCurData.EndDate));
                                $('.AskTheDummy').text(oCurData.AskTheDummyName + '(' + oCurData.AskTheDummy + ')  ' + oCurData.DeptName);
                                fnGetUploadFiles(oCurData.Guid, fnUpload);
                                if (oCurData.Handle_DeptID) {
                                    fnSetUserDrop([
                                        {
                                            Select: $('#Handle_Person'),
                                            DepartmentID: oCurData.Handle_DeptID,
                                            ShowId: true,
                                            Select2: true,
                                            Action: sAction,
                                            DefultVal: oCurData.Handle_Person
                                        }
                                    ]);
                                }
                                if (oCurData.Agent_DeptID) {
                                    fnSetUserDrop([
                                        {
                                            Select: $('#Agent_Person'),
                                            DepartmentID: oCurData.Agent_DeptID,
                                            ShowId: true,
                                            Select2: true,
                                            Action: sAction,
                                            DefultVal: oCurData.Agent_Person
                                        }
                                    ]);
                                }
                                if (oRes.Flows_Lock === 'Y') {
                                    $(".checkordertoolbox").hide();
                                }
                                else {
                                    $(".checkordertoolbox").show();
                                }
                                if (oRes.Handle_Lock === 'Y') {
                                    $("#Handle_DeptID,#Handle_Person").attr('disabled', true);
                                }
                                else {
                                    $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                }
                                $("#jsGrid").jsGrid("loadData");
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
                            }
                        });
                }
                else {
                    $('.AskTheDummy').text(parent.UserInfo.MemberName + '(' + parent.UserInfo.MemberID + ')  ' + parent.UserInfo.DepartmentName);
                    $('#AskTheDummy').val(parent.UserInfo.MemberID);
                    oCurData.CheckOrder = [];
                    oCurData.Guid = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} flag 新增或儲存後新增
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Guid = oCurData.Guid;
                data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|BT|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(oCurData, true, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Status = 'A';
                data.IsHandled = 'N';
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        businesstravel: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (flag == 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&Guid=' + data.Guid); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                });
            },
            /**
             * 修改資料
             * @param {Boolean} balert 是否提示
             */
            fnUpd = function (balert) {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.CheckFlows = fnCheckFlows(oCurData, true, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;

                return CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        businesstravel: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        if (!balert) {
                            bRequestStorage = false;
                            showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                            if (window.bLeavePage) {
                                setTimeout(function () {
                                    pageLeave();
                                }, 1000);
                            }
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
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        businesstravel: {
                            Guid: sDataId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        DelTask(sDataId);
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
             * 上傳附件
             * @param {Array} files 上傳的文件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'BusinessTravel';
                option.type = 'list';
                option.parentid = oCurData.Guid;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 計算時差
             */
            fnGetDateDiff = function () {
                try {
                    var sStartDate = $('#StartDate').val(),
                        sEndDate = $('#EndDate').val();
                    if (sStartDate === '' || sEndDate === '') {
                        return false;
                    }
                    var date_s = sStartDate.split(" ")[0].replaceAll('/', '-'),//開始的日期部分
                        date_e = sEndDate.split(" ")[0].replaceAll('/', '-'),//結束的日期部分
                        time_s = sStartDate.split(" ")[1],//開始的時間部分
                        time_e = sEndDate.split(" ")[1],//結束的日期部分
                        newdate_s = new Date(date_s + ' 00:00').getTime();

                    newdate_s = new Date(newdate_s + 24 * 60 * 60 * 1000);

                    var newdate_e = new Date(date_e + ' 23:59').getTime();
                    newdate_e = new Date(newdate_e - 24 * 60 * 60 * 1000);
                    var intDays = 0;
                    var intHours = 0;
                    var intDaysHours = "";
                    while (newdate_s <= newdate_e) {
                        intDaysHours = fnDaysAndHours(newdate_s);
                        intDays += parseInt(intDaysHours.split('|')[0]);
                        intHours += parseInt(intDaysHours.split('|')[1]);
                        newdate_s = new Date(newdate_s.getTime() + 24 * 60 * 60 * 1000);
                    }
                    //計算第一天和最後一天
                    if (date_s === date_e) {//若都是當天
                        intDaysHours = fnDateSE(date_s, time_s, time_e);
                        intDays += parseInt(intDaysHours.split('|')[0]);
                        intHours += (intDaysHours.split('|')[1] * 1);
                    }
                    else {
                        //第一天
                        intDaysHours = fnDateSE(date_s, time_s, "23:59");
                        intDays += parseInt(intDaysHours.split('|')[0]);
                        intHours += (intDaysHours.split('|')[1] * 1);
                        //最後一天
                        intDaysHours = fnDateSE(date_e, "00:00", time_e);
                        intDays += parseInt(intDaysHours.split('|')[0]);
                        intHours += (intDaysHours.split('|')[1] * 1);
                    }
                    var days = Math.floor(intHours / iOneDayHours);
                    intDays = intDays + days;
                    //intHours = intHours % iOneDayHours;
                    $('#TotalTime').val(intDays * iOneDayHours + intHours);
                } catch (e) { }
            },
            /**
             * 返回週幾信息
             * @param  {Date} times 開始時間
             * @param  {Date} timee 結束時間
             */
            fnDiffTimeSE = function (times, timee) {
                var minute = 1000 * 60 * 60,
                    DateS1 = new Date(times).getTime(),
                    DateE1 = new Date(timee).getTime(),
                    diffValue = DateE1 - DateS1,
                    minC = diffValue / minute;
                return minC;
            },
            /**
             * 日期和時間
             * @param  {Date} dates 當前日期
             * @param  {Date} times 開始時間
             * @param  {Date} timee 結束時間
             */
            fnDateSE = function (dates, times, timee) {
                try {
                    var intDays = 0,
                        inthour = 0,
                        intAllhour = 0,
                        aryTimeSE = [parent.SysSet.WorkTimePM, parent.SysSet.WorkTimeAM];
                    if (sHolidays.indexOf(dates) > -1) {
                        //是節假日
                        intDays = 0;
                        inthour = 0;
                    }
                    else {//非節假日
                        var IsCheckAllDay = "";
                        for (var i = 0; i < aryTimeSE.length; i++) {
                            if (aryTimeSE[i] != "" && aryTimeSE[i] !== undefined) {
                                var aryTimeS = dates + " " + aryTimeSE[i].split('~')[0];//設小
                                var aryTimeE = dates + " " + aryTimeSE[i].split('~')[1];//設大
                                if (fnDiffTimeSE((dates + " " + times), aryTimeS) >= 0 && fnDiffTimeSE(aryTimeE, (dates + " " + timee)) >= 0) {
                                    IsCheckAllDay += "Y";
                                    intAllhour += fnDiffTimeSE(aryTimeS, aryTimeE);
                                }
                                else {
                                    IsCheckAllDay += "N";
                                    //傳小>設小 AND 傳大>=設大
                                    if ((fnDiffTimeSE(aryTimeS, (dates + " " + times)) >= 0 && fnDiffTimeSE((dates + " " + timee), aryTimeE) > 0) || (fnDiffTimeSE(aryTimeS, (dates + " " + times)) > 0 && fnDiffTimeSE((dates + " " + timee), aryTimeE) >= 0)) {
                                        intAllhour += fnDiffTimeSE((dates + " " + times), (dates + " " + timee));
                                    }
                                    else if (fnDiffTimeSE(aryTimeS, (dates + " " + times)) > 0 && fnDiffTimeSE(aryTimeE, (dates + " " + timee)) > 0 && fnDiffTimeSE((dates + " " + times), aryTimeE) > 0) {
                                        intAllhour += fnDiffTimeSE((dates + " " + times), aryTimeE);
                                    }
                                    else if (fnDiffTimeSE((dates + " " + times), aryTimeS) > 0 && fnDiffTimeSE(aryTimeS, (dates + " " + timee)) > 0 && fnDiffTimeSE((dates + " " + timee), aryTimeE) > 0) {
                                        intAllhour += fnDiffTimeSE(aryTimeS, (dates + " " + timee));
                                    }
                                }
                            }
                        }
                        if (IsCheckAllDay.indexOf("N") > -1) {//非全天
                            intDays = 0;
                            inthour = intAllhour;
                            var days = Math.floor(inthour / iOneDayHours);
                            if (days > 0) {
                                //全天
                                intDays = 1;
                                inthour = 0;
                            }
                        }
                        else {
                            intDays = 1;
                            inthour = 0;
                        }
                    }
                    return intDays + "|" + inthour;
                } catch (e) { }
            },
            /**
             * 日期和小時
             * @param  {Date} curdate 當前日期
             */
            fnDaysAndHours = function (curdate) {
                try {
                    var intDays = 0,
                        inthour = 0;
                    if (sHolidays.indexOf(curdate.formate("yyyy-MM-dd")) > -1) { //是節假日
                        intDays = 0;
                        inthour = 0;
                    }
                    else {//非節假日
                        intDays = 1;
                        inthour = 0;
                    }
                    return intDays + "|" + inthour;
                } catch (e) { }
            },
            /**
             * 獲取假日信息
             */
            fnGetHolidays = function () {
                var sYear = new Date().getFullYear() + ',' + new Date().dateAdd('y', 1).getFullYear();
                CallAjax(ComFn.W_Com, ComFn.GetList, {
                    Type: '',
                    Params: {
                        holidays: {
                            _CHARINDEX_Year: sYear,
                            OrgID: parent.OrgID
                        }
                    }
                }, function (res) {
                    if (res.d) {
                        var saRes = $.parseJSON(res.d);
                        $.each(saRes, function (idx, _data) {
                            sHolidays += _data.Holidays;
                        });
                    }
                });
            },
            /**
             * 提交簽呈
             */
            fnSubmitPetition = function () {
                g_api.ConnectLite(sProgramId, 'BusinessTravelToAudit', {
                    guid: oCurData.Guid
                }, function (res) {
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t("message.ToAudit_Success"), sViewPrgId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.ToAudit_Success⇒提交審核成功╣
                        parent.msgs.server.pushTip(parent.OrgID, res.DATA.rel);
                    }
                    else {
                        showMsg(i18next.t('message.ToAudit_Failed') + '<br>' + res.MSG, 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.ToAudit_Failed'), 'error'); // ╠message.ToAudit_Failed⇒提交審核失敗╣
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

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        if (sAction === 'Add') {
                            fnAdd('add');
                        }
                        else {
                            fnUpd();
                        }

                        break;
                    case "Toolbar_ReAdd":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
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
                    case "Toolbar_Petition":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        fnUpd(true).done(function () {
                            fnSubmitPetition();
                        });

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

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
                var saCusBtns = null;

                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'Toolbar_Petition',
                        value: 'common.SubmitPetition'// ╠common.SubmitPetition⇒提交簽呈╣
                    }];
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true,
                    onSelect: function (d, el) {
                        fnGetDateDiff();
                    }
                });
                oValidator = $("#form_main").validate();

                $.whenArray([
                    fnSetDeptDrop($('#Handle_DeptID,#Agent_DeptID')),
                    fnSetFlowDrop({
                        Flow_Type: parent.SysSet.Eip_002,
                        ShareTo: parent.UserID,
                        CallBack: function (data) {
                            $.each(data, function (idx, item) {
                                var saFlows = $.parseJSON(item.Flows),
                                    saFlowsText = [],
                                    sFlowsText = '';
                                $.each(saFlows, function (idx, flow) {
                                    var sFlowType = i18next.t('common.' + flow.SignedWay);
                                    if (flow.SignedWay !== 'flow1') {
                                        saFlowsText.push(sFlowType + '(' + Enumerable.From(flow.SignedMember).ToString("，", "$.name") + ')');
                                    }
                                    else {
                                        saFlowsText.push(Enumerable.From(flow.SignedMember).ToString("，", "$.name"));
                                    }
                                });
                                sFlowsText = saFlowsText.join(' → ');
                                item.text = item.Flow_Name + ' - ' + (sFlowsText.length > 60 ? sFlowsText.substr(0, 60) + '...' : sFlowsText);
                            });
                            $('#FlowId').html(createOptions(data, 'Guid', 'text')).on('change', function () {
                                var sFlowId = this.value;
                                if (sFlowId) {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            checkflow: {
                                                Guid: sFlowId
                                            }
                                        }
                                    }, function (res) {
                                        if (res.d) {
                                            var oRes = $.parseJSON(res.d);
                                            oRes.Flows = $.parseJSON(oRes.Flows);
                                            oCurData.CheckOrder = oRes.Flows;
                                            oCurData.Flows_Lock = oRes.Flows_Lock;
                                            oCurData.Handle_Lock = oRes.Handle_Lock;
                                            $("#Handle_DeptID").val(oRes.Handle_DeptID);
                                            $("#Handle_Person").val(oRes.Handle_Person).trigger('change');
                                            if (oRes.Flows_Lock === 'Y') {
                                                $(".checkordertoolbox").hide();
                                            }
                                            else {
                                                $(".checkordertoolbox").show();
                                            }
                                            if (oRes.Handle_Lock === 'Y') {
                                                $("#Handle_DeptID,#Handle_Person").attr('disabled', true);
                                            }
                                            else {
                                                $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                            }
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    });
                                }
                                else {
                                    oCurData.CheckOrder = [];
                                    $(".checkordertoolbox").hide();
                                    $("#jsGrid").jsGrid("loadData");
                                    $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                }
                            });
                        }
                    }),
                    fnSetUserDrop([
                        {
                            Select: $('#Handle_Person,#Agent_Person'),
                            Select2: true,
                            Action: sAction,
                            CallBack: function (data) {
                                saUsers = data;
                            }
                        }
                    ]),
                    fnGetHolidays(),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'BTrlType',
                            Select: $('#HolidayCategory'),
                            ShowId: true
                        }
                    ])])
                    .done(function () {
                        fnGet();
                    });

                $('#Handle_DeptID').on('change', function () {
                    fnSetUserDrop([
                        {
                            Select: $('#Handle_Person'),
                            DepartmentID: this.value,
                            ShowId: true,
                            Select2: true,
                            Action: sAction
                        }
                    ]);
                });
                $('#Agent_DeptID').on('change', function () {
                    fnSetUserDrop([
                        {
                            Select: $('#Agent_Person'),
                            DepartmentID: this.value,
                            ShowId: true,
                            Select2: true,
                            Action: sAction
                        }
                    ]);
                });
                $('#Agent_Person').on('change', function () {
                    oCurData.Agent_Person = this.value;
                });

                $('.flowlink').on('click', function () {
                    var oOption = {};
                    oOption.SignedWay = this.id;
                    oOption.Callback = function (data) {
                        if (data.Users.length > 0) {
                            var oFlow = {};
                            if (data.FlowType === 'flow1') {
                                $.each(data.Users, function (idx, user) {
                                    oFlow = {};
                                    oFlow.id = guid();
                                    oFlow.Order = oCurData.CheckOrder.length + 1;
                                    oFlow.SignedWay = data.FlowType;
                                    oFlow.SignedMember = [{
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    }];
                                    oCurData.CheckOrder.push(oFlow);
                                });
                            }
                            else {
                                var saUsers = [];
                                $.each(data.Users, function (idx, user) {
                                    saUsers.push({
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    });
                                });
                                oFlow.id = guid();
                                oFlow.Order = oCurData.CheckOrder.length + 1;
                                oFlow.SignedWay = data.FlowType;
                                oFlow.SignedMember = saUsers;
                                oCurData.CheckOrder.push(oFlow);
                            }
                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                            $("#jsGrid").jsGrid("loadData");
                        }
                    };
                    oPenUserListPop(oOption);
                });

                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    filtering: false,
                    pageLoading: true,
                    pageIndex: 1,
                    pageSize: 10000,
                    fields: [
                        {
                            name: "Order", title: 'common.Order', width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                return val < 10 ? '0' + val : val;
                            }
                        },
                        {
                            name: "SignedWay", title: 'common.SignedWay', width: 120, align: "center",
                            itemTemplate: function (val, item) {
                                return i18next.t('common.' + val);
                            }
                        },
                        {
                            type: "Icon", width: 50, align: "center",
                            itemTemplate: function (val, item) {
                                var oIcon = {
                                    flow1: '<img src="../../images/flow_check.gif">',
                                    flow2: '<img src="../../images/flow_check.gif"><img src="../../images/flow_check.gif">',
                                    flow3: '<img src="../../images/flow_check.gif"><img src="../../images/flow_nocheck.gif">',
                                    flow4: '<img src="../../images/flow4.gif">'
                                },
                                    sIcon = oIcon[item.SignedWay];
                                if (item.Order !== oCurData.CheckOrder.length) {
                                    sIcon += '<br><img src="../../images/flow_arrow.gif" style="vertical-align:top;">'
                                }
                                return sIcon;
                            }
                        },
                        {
                            name: "SignedMember", title: 'common.SignedMember', width: 500,
                            itemTemplate: function (val, item) {
                                return Enumerable.From(val).ToString("，", "$.name");
                            }
                        },
                        {
                            type: "control", title: 'common.Action', width: 200,
                            itemTemplate: function (val, item) {
                                var oBtns = [$('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                    class: 'glyphicon glyphicon-pencil' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                    title: i18next.t('common.Edit'),// ╠common.Edit⇒編輯╣
                                    click: function () {
                                        if ($(this).hasClass('disabled')) { return false; }
                                        var oOption = {};
                                        oOption.SignedWay = item.SignedWay;
                                        oOption.SignedMember = item.SignedMember;
                                        oOption.Callback = function (data) {
                                            if (data.Users.length > 0) {
                                                var oFlow = {};
                                                if (data.FlowType === 'flow1') {
                                                    $.each(data.Users, function (idx, user) {
                                                        var oFlow = {};
                                                        oFlow.id = guid();
                                                        oFlow.Order = item.Order + idx;
                                                        oFlow.SignedWay = data.FlowType;
                                                        oFlow.SignedMember = [{
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        }];
                                                        oCurData.CheckOrder.insert(item.Order + idx, oFlow);
                                                    });
                                                }
                                                else {
                                                    var saUsers = [];
                                                    $.each(data.Users, function (idx, user) {
                                                        saUsers.push({
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        });
                                                    });
                                                    oFlow.id = guid();
                                                    oFlow.Order = item.Order;
                                                    oFlow.SignedWay = data.FlowType;
                                                    oFlow.SignedMember = saUsers;
                                                    oCurData.CheckOrder.insert(item.Order, oFlow);
                                                }
                                                var iOrder = 1;
                                                $.each(oCurData.CheckOrder, function (idx, _data) {
                                                    if (item.id !== _data.id) {
                                                        _data.Order = iOrder;
                                                        iOrder++;
                                                    }
                                                });
                                                oCurData.CheckOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id !== item.id; }).ToArray();
                                                oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                                $("#jsGrid").jsGrid("loadData");
                                            }
                                        };
                                        oPenUserListPop(oOption);
                                    }
                                })),
                                $('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                    class: 'glyphicon glyphicon-trash' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                    title: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                    click: function () {
                                        if ($(this).hasClass('disabled')) { return false; }

                                        var saNewList = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id !== item.id; }).ToArray();
                                        oCurData.CheckOrder = saNewList;
                                        $.each(oCurData.CheckOrder, function (idx, _data) {
                                            _data.Order = idx + 1;
                                        });
                                        oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                        $("#jsGrid").jsGrid("loadData");
                                    }
                                }))];

                                if (oCurData.CheckOrder.length !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-down' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                        title: i18next.t('common.Down'),// ╠common.Down⇒下移╣
                                        click: function () {
                                            if ($(this).hasClass('disabled')) { return false; }
                                            var sOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(oCurData.CheckOrder, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order++;
                                                }
                                                else if ((iOrder + 1) === _data.Order) {
                                                    _data.Order--;
                                                }
                                            });
                                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    })));
                                }
                                else {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }));
                                }

                                if (1 !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-up' + (oCurData.Flows_Lock === 'Y' ? ' disabled' : ''),
                                        title: i18next.t('common.Up'),// ╠common.Up⇒上移╣
                                        click: function () {
                                            if ($(this).hasClass('disabled')) { return false; }
                                            var sOrder = Enumerable.From(oCurData.CheckOrder).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(oCurData.CheckOrder, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order--;
                                                }
                                                else if ((iOrder - 1) === _data.Order) {
                                                    _data.Order++;
                                                }
                                            });
                                            oCurData.CheckOrder = releaseGridList(oCurData.CheckOrder);
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    })));
                                }

                                return oBtns;
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: oCurData.CheckOrder,
                                itemsCount: oCurData.CheckOrder.length //data.length
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
                        oGrid = args.grid;
                    }
                });
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'filer', 'timepicker', 'common_eip', 'util'], fnPageInit, 'timepicker');