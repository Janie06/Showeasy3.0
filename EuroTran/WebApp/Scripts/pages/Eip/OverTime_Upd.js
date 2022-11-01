'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sViewPrgId = sProgramId.replace('_Upd', '_View'),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckOrder: [], OverTimes: [] },
            oForm = $('#form_main'),
            oGrid = null,
            oValidator = null,
            sOptionHtml_Users = '',
            sOptionHtml_OverTime = '',
            sOptionHtml_PrjCode = '',
            oAddItem = {},
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
                                oCurData.OverTimes = $.parseJSON(oCurData.OverTimes);
                                oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                                setFormVal(oForm, oRes);
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
                                if (oCurData.Flows_Lock === 'Y') {
                                    $(".checkordertoolbox").hide();
                                }
                                else {
                                    $(".checkordertoolbox").show();
                                }
                                if (oCurData.Handle_Lock === 'Y') {
                                    $("#Handle_DeptID,#Handle_Person").attr('disabled', true);
                                }
                                else {
                                    $("#Handle_DeptID,#Handle_Person").removeAttr('disabled');
                                }
                                $("#jsGrid").jsGrid("loadData");
                                $("#jsGrid1").jsGrid("loadData");
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
                            }
                        });
                }
                else {
                    $('.AskTheDummy').text(parent.UserInfo.MemberName + '(' + parent.UserInfo.MemberID + ')  ' + parent.UserInfo.DepartmentName);
                    $('#AskTheDummy').val(parent.UserInfo.MemberID);
                    oCurData.OverTimes = [];
                    oCurData.CheckOrder = [];
                    oCurData.Guid = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} sFlag 新增或儲存後新增
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Guid = oCurData.Guid;
                data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|OT|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.OverTimes = JSON.stringify(oCurData.OverTimes);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Status = 'A';
                data.IsHandled = 'N';
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        overtime: data
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
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.OverTimes = JSON.stringify(oCurData.OverTimes);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;

                return CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        overtime: {
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
                        overtime: {
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
                option.folder = 'OverTime';
                option.type = 'list';
                option.parentid = oCurData.Guid;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 提交簽呈
             */
            fnSubmitPetition = function () {
                g_api.ConnectLite(sProgramId, 'OverTimeToAudit', {
                    guid: oCurData.Guid
                }, function (res) {
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t("message.ToAudit_Success"), sViewPrgId, '?Action=Upd&Guid=' + oCurData.Guid);// ╠message.ToAudit_Success⇒提交審核成功╣
                        parent.msgs.server.pushTips(parent.fnReleaseUsers(res.DATA.rel));
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
                    GoTop: true
                });
                oValidator = $("#form_main").validate();

                $.whenArray([
                    fnSetDeptDrop($('#Handle_DeptID')),
                    fnSetFlowDrop({
                        Flow_Type: parent.SysSet.Eip_007,
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
                    fnSetEpoDrop({
                        CallBack: function (data) {
                            sOptionHtml_PrjCode = createOptions(data, 'ExhibitionCode', 'ExhibitioShotName_TW');
                        }
                    }),
                    fnSetUserDrop([
                        {
                            Select: $('#Handle_Person'),
                            Select2: true,
                            ShowId: true,
                            Action: sAction,
                            CallBack: function (data) {
                                saUsers = data;
                                sOptionHtml_Users = createOptions(data, 'MemberID', 'MemberName');
                            }
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'OverTime',
                            CallBack: function (data) {
                                sOptionHtml_OverTime = createOptions(data, 'id', 'text', true);
                            }
                        }
                    ])])
                    .done(function () {
                        fnGet();

                        $("#jsGrid1").jsGrid({
                            width: "100%",
                            height: "auto",
                            autoload: true,
                            filtering: false,
                            inserting: true,
                            editing: true,
                            pageLoading: true,
                            confirmDeleting: true,
                            invalidMessage: i18next.t('common.InvalidData'),// ╠common.InvalidData⇒输入的数据无效！╣
                            deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                            pageIndex: 1,
                            pageSize: 10000,
                            fields: [
                                {
                                    name: "Index", title: '#', width: 30, align: "center"
                                },
                                {// ╠common.EmployeeCode⇒員工代號╣
                                    name: "EmployeeCode", title: 'common.EmployeeCode', width: 70, type: "text",
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p",
                                            disabled: true
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p",
                                            disabled: true,
                                            value: val
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.EmployeeName⇒員工姓名╣
                                    name: "EmployeeName", title: 'common.EmployeeName', width: 70, type: "text", validate: { validator: 'required', message: i18next.t('common.Employee_required') },// ╠common.Employee_required⇒請選擇員工╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_Users,
                                            change: function () {
                                                oControl.parent().prev().find('input').val(this.value);
                                            }
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        if (this.insertControl.val()) {
                                            return this.insertControl.find('option:selected').text();
                                        }
                                        else {
                                            return '';
                                        }
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_Users,
                                            change: function () {
                                                oControl.parent().prev().find('input').val(this.value);
                                            }
                                        });
                                        return this.editControl = oControl.val(item.EmployeeCode);
                                    },
                                    editValue: function () {
                                        if (this.editControl.val()) {
                                            return this.editControl.find('option:selected').text();
                                        }
                                        else {
                                            return '';
                                        }
                                    }
                                },
                                {// ╠common.OvertimeClass⇒加班類別╣
                                    name: "OvertimeClass", title: 'common.OvertimeClass', width: 70, type: "text", validate: { validator: 'required', message: i18next.t('common.OvertimeClass_required') },// ╠common.OvertimeClass_required⇒請選擇加班類別╣
                                    itemTemplate: function (val, item) {
                                        return item.OvertimeClassName;
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_OverTime
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        if (this.insertControl.val()) {
                                            oAddItem.OvertimeClassName = this.insertControl.find('option:selected').text();
                                        }
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_OverTime,
                                            change: function () {
                                                item.OvertimeClassName = $(this).find('option:selected').text();
                                            }
                                        });
                                        return this.editControl = oControl.val(item.OvertimeClass);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.AttendanceDate⇒出勤日期╣
                                    name: "AttendanceDate", title: 'common.AttendanceDate', width: 80, align: "center", type: "text", validate: { validator: 'required', message: i18next.t('common.AttendanceDate_required') },// ╠common.AttendanceDate_required⇒請選擇出勤日期╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker"
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker",
                                            value: val
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.StartDate⇒开始日期╣
                                    name: "StartDate", title: 'common.StartDate', width: 80, type: "text", align: "center", validate: { validator: 'required', message: i18next.t('common.StartDate_required') },// ╠common.StartDate_required⇒請選擇開始日期╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker"
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker",
                                            value: val
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.StartTime⇒开始時間╣
                                    name: "StartTime", title: 'common.StartTime', width: 60, type: "text", align: "center", validate: { validator: 'required', message: i18next.t('common.StartTime_required') },// ╠common.StartTime_required⇒請選擇開始時間╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker"
                                        });
                                        return this.insertControl = oControl.timepicker(
                                            {
                                                timeFormat: 'HH:mm',
                                                hour: 9,
                                                hourGrid: 6,
                                                minuteGrid: 30,
                                                stepMinute: 30
                                            }
                                        );
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker",
                                            value: val
                                        });
                                        return this.editControl = oControl.timepicker(
                                            {
                                                timeFormat: 'HH:mm',
                                                hour: 9,
                                                hourGrid: 6,
                                                minuteGrid: 30,
                                                stepMinute: 30
                                            }
                                        );
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.EndDate⇒結束日期╣
                                    name: "EndDate", title: 'common.EndDate', width: 80, type: "text", align: "center", validate: { validator: 'required', message: i18next.t('common.EndDate_required') },// ╠common.EndDate_required⇒請選擇結束日期╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker"
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker",
                                            value: val
                                        });
                                        oControl.datepicker({
                                            changeYear: true,
                                            changeMonth: true,
                                            altFormat: 'yyyy/MM/dd'
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.EndTime⇒結束時間╣
                                    name: "EndTime", title: 'common.EndTime', width: 60, type: "text", align: "center", validate: { validator: 'required', message: i18next.t('common.EndTime_required') },// ╠common.EndTime_required⇒請選擇結束時間╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker"
                                        });
                                        return this.insertControl = oControl.timepicker(
                                            {
                                                timeFormat: 'HH:mm',
                                                hour: 17,
                                                minute: 30,
                                                hourGrid: 6,
                                                minuteGrid: 30,
                                                stepMinute: 30
                                            }
                                        );
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p date-picker",
                                            value: val
                                        });
                                        return this.editControl = oControl.timepicker(
                                            {
                                                timeFormat: 'HH:mm',
                                                hour: 17,
                                                minute: 30,
                                                hourGrid: 6,
                                                minuteGrid: 30,
                                                stepMinute: 30
                                            }
                                        );
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.OvertimeHours⇒加班時數╣
                                    name: "OvertimeHours", title: 'common.OvertimeHours', width: 50, align: "center", type: "text", validate: { validator: 'required', message: i18next.t('common.OvertimeHours_required') }// ╠common.OvertimeHours_required⇒請輸入加班時數╣
                                },
                                {// ╠common.TakeTimeOffHours⇒補休時數╣
                                    name: "TakeTimeOffHours", title: 'common.TakeTimeOffHours', width: 50, align: "center", type: "text", validate: { validator: 'required', message: i18next.t('common.TakeTimeOffHours_required') }// ╠common.TakeTimeOffHours_required⇒請輸入補休時數╣
                                },
                                {// ╠common.PrjCode⇒專案代號╣
                                    name: "PrjCode", title: 'common.PrjCode', width: 80, type: "text",
                                    itemTemplate: function (val, item) {
                                        return item.PrjCode + '（' + item.PrjName + '）';
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_PrjCode
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '100px' });
                                        }, 1000);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        if (this.insertControl.val()) {
                                            oAddItem.PrjName = this.insertControl.find('option:selected').text();
                                        }
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control w100p",
                                            html: sOptionHtml_PrjCode,
                                            change: function () {
                                                item.PrjName = $(this).find('option:selected').text();
                                            }
                                        });
                                        setTimeout(function () {
                                            oControl.select2({ width: '100px' });
                                        }, 1000);
                                        return this.editControl = oControl.val(item.PrjCode);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.LeaveReason⇒事由說明╣
                                    name: "Reason", title: 'common.LeaveReason', width: 70, type: "text",
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<textarea rows="2" cols="20" />', {
                                            class: "form-control w100p"
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<textarea rows="2" cols="20" />', {
                                            class: "form-control w100p"
                                        });
                                        return this.editControl = oControl.val(val);
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    type: "control", width: 50
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return {
                                        data: oCurData.OverTimes,
                                        itemsCount: oCurData.OverTimes.length //data.length
                                    };
                                },
                                insertItem: function (args) {
                                    args.guid = guid();
                                    args.Index = oCurData.OverTimes.length + 1;
                                    args.OvertimeClassName = oAddItem.OvertimeClassName;
                                    args.PrjName = oAddItem.PrjName;
                                    oCurData.OverTimes.push(args);
                                },
                                updateItem: function (args) {
                                },
                                deleteItem: function (args) {
                                    var saNewOverTimes = [];
                                    $.each(oCurData.OverTimes, function (idx, _data) {
                                        if (_data.guid !== args.guid) {
                                            saNewOverTimes.push(_data);
                                        }
                                    });
                                    $.each(saNewOverTimes, function (idx, _data) {
                                        _data.Index = idx + 1;
                                    });
                                    oCurData.OverTimes = saNewOverTimes;
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
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
                                var saSignedMember = [];
                                $.each(data.Users, function (idx, user) {
                                    saSignedMember.push({
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    });
                                });
                                oFlow.id = guid();
                                oFlow.Order = oCurData.CheckOrder.length + 1;
                                oFlow.SignedWay = data.FlowType;
                                oFlow.SignedMember = saSignedMember;
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
                                                    var saSignedMember = [];
                                                    $.each(data.Users, function (idx, user) {
                                                        saSignedMember.push({
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        });
                                                    });
                                                    oFlow.id = guid();
                                                    oFlow.Order = item.Order;
                                                    oFlow.SignedWay = data.FlowType;
                                                    oFlow.SignedMember = saSignedMember;
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
                    }
                });
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'timepicker', 'filer', 'common_eip', 'util'], fnPageInit, 'timepicker');