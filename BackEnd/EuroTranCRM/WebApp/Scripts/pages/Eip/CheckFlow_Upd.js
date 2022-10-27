'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = {},
            oForm = $('#form_main'),
            oValidator = null,
            oGrid = null,
            saGridData = [],
            /**
             * 獲取分享權限
             */
            fnGetRightOption = function () {
                var saRight = [];
                $('#lstRight option').each(function () {
                    saRight.push(this.value);
                });
                return saRight.join(',');
            },
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
                                saGridData = $.parseJSON(oCurData.Flows);
                                setFormVal(oForm, oRes);
                                fnSetHandle_PersonDrop(oCurData.Handle_DeptID, 1);
                                fnSetHandle_PersonDrop('', 2, false);
                                $("#jsGrid").jsGrid("loadData");
                                setNameById().done(function () {
                                    getPageVal();//緩存頁面值，用於清除
                                });
                            }
                        });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} sFlag 新增或儲存後新增
             */
            fnAdd = function (sFlag) {
                var data = getFormSerialize(oForm),
                    oAddPm = {};
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.ShareTo = fnGetRightOption();
                data.Flows = JSON.stringify(saGridData);
                data.Flows_Lock = data.Flows_Lock || 'N';
                data.Handle_Lock = data.Handle_Lock || 'N';
                delete data.lstLeft;
                delete data.lstRight;

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        checkflow: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (sFlag == 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sQueryPrgId); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                        }
                        parent.msgs.server.broadcast(data);
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
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.ShareTo = fnGetRightOption();
                data.Flows = JSON.stringify(saGridData);
                data.Flows_Lock = data.Flows_Lock || 'N';
                data.Handle_Lock = data.Handle_Lock || 'N';
                delete data.lstLeft;
                delete data.lstRight;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        checkflow: {
                            values: data,
                            keys: { Guid: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        showMsgAndGo(i18next.t("message.Modify_Success"), sQueryPrgId); //╠message.Modify_Success⇒修改成功╣
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
                        checkflow: {
                            Guid: sDataId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
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
             * 設置公告類別下拉單
             * @param {String} deptid 部門ID
             * @param {String} flag 經辦單位或者共享對象
             */
            fnSetHandle_PersonDrop = function (deptid, flag, bchange) {
                var sDepartmentID = deptid || '';
                fnSetUserDrop([
                    {
                        ShowId: true,
                        Action: sAction,
                        DepartmentID: sDepartmentID,
                        CallBack: function (data) {
                            var saList = data;
                            if (flag === 1) {
                                $('#Handle_Person').html(createOptions(saList, 'MemberID', 'MemberName', true));
                                if (oCurData.Handle_Person) {
                                    $('#Handle_Person').val(oCurData.Handle_Person);
                                }
                            }
                            else {
                                var saLeft = [],
                                    saRight = [];
                                if (oCurData.ShareTo) {
                                    $.each(saList, function (idx, data) {
                                        if (oCurData.ShareTo.indexOf(data.MemberID) > -1) {
                                            saRight.push(data);
                                        }
                                        else {
                                            saLeft.push(data);
                                        }
                                    });
                                }
                                else {
                                    saLeft = saList;
                                }
                                $('#lstLeft').html(createOptions(saLeft, 'MemberID', 'MemberName', true)).find('option:first').remove();
                                if (!bchange) {
                                    $('#lstRight').html(createOptions(saRight, 'MemberID', 'MemberName', true)).find('option:first').remove();
                                }
                            }
                        }
                    }
                ]);
            },
            /**
             * 獲取匯入的資料
             */
            fnGetPopData_ImportData = function (args) {
                args = args || {};
                args.Flow_Type = $('#Pop_Flow_Type').val();
                args.Flow_Name = $('#Pop_Flow_Name').val();

                return g_api.ConnectLite(sQueryPrgId, ComFn.GetPage, args);
            },
            /**
             * 匯入資料
             */
            fnImport = function () {
                var oConfig = {
                    Get: fnGetPopData_ImportData,
                    SearchFields: [
                        { id: "Pop_Flow_Type", type: 'select', i18nkey: 'CheckFlow_Upd.Flow_Type', html: $('#Flow_Type').html() },
                        { id: "Pop_Flow_Name", type: 'text', i18nkey: 'CheckFlow_Upd.Flow_Name' }
                    ],
                    Fields: [
                        { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 40 },
                        { name: "Flow_TypeName", title: 'CheckFlow_Upd.Flow_Type', width: 120 },
                        { name: "Flow_Name", title: 'CheckFlow_Upd.Flow_Name', width: 150 }
                    ],
                    Callback: function (item) {
                        oCurData = item;
                        saGridData = $.parseJSON(oCurData.Flows);
                        setFormVal(oForm, oCurData);
                        fnSetHandle_PersonDrop(oCurData.Handle_DeptID, 1);
                        fnSetHandle_PersonDrop('', 2, false);
                        $("#jsGrid").jsGrid("loadData");
                    }
                };
                oPenPops(oConfig);
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

                        if (sAction == 'Add') {
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
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

                        break;
                    case "Toolbar_Imp":

                        fnImport();

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
                var saCusBtns = [];
                if (sAction === 'Add') {
                    saCusBtns = [{
                        id: 'Toolbar_Imp',
                        value: 'common.Toolbar_Imp'// ╠common.Toolbar_Imp⇒匯入╣
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
                    fnSetDeptDrop($('#Handle_DeptID,#ShareType')),
                    fnSetUserDrop([
                        {
                            ShowId: true,
                            Select: $('#Handle_Person'),
                            Select2: true,
                            Action: sAction,
                            CallBack: function (data) {
                                $('#lstLeft').html(createOptions(data, 'MemberID', 'MemberName', true))[0].remove(0);
                            }
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'Flow_Type',
                            Select: $('#Flow_Type'),
                            ShowId: true
                        }
                    ])])
                    .done(function () {
                        fnGet();
                    });
                $('#Handle_DeptID').on('change', function () {
                    fnSetHandle_PersonDrop(this.value, 1);
                });
                $('#Agent_Person').on('change', function () {
                    oCurData.Agent_Person = this.value;
                });
                $('#ShareType').on('change', function () {
                    fnSetHandle_PersonDrop(this.value, 2, true);
                });

                $('#btnToRight').click(function () {
                    optionListMove($('#lstLeft'), $('#lstRight'));
                    oCurData.ShareTo = fnGetRightOption();
                });
                $('#btnToLeft').click(function () {
                    optionListMove($('#lstRight'), $('#lstLeft'));
                    oCurData.ShareTo = fnGetRightOption();
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
                                    oFlow.Order = saGridData.length + 1;
                                    oFlow.SignedWay = data.FlowType;
                                    oFlow.SignedMember = [{
                                        id: user.id,
                                        name: user.name,
                                        deptname: user.deptname,
                                        jobname: user.jobname
                                    }];
                                    saGridData.push(oFlow);
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
                                oFlow.Order = saGridData.length + 1;
                                oFlow.SignedWay = data.FlowType;
                                oFlow.SignedMember = saUsers;
                                saGridData.push(oFlow);
                            }
                            saGridData = releaseGridList(saGridData);
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
                                if (item.Order !== saGridData.length) {
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
                                    class: 'glyphicon glyphicon-pencil',
                                    title: i18next.t('common.Edit'),// ╠common.Edit⇒編輯╣
                                    click: function () {
                                        var oOption = {};
                                        oOption.SignedWay = item.SignedWay;
                                        oOption.SignedMember = item.SignedMember;
                                        oOption.Callback = function (data) {
                                            if (data.Users.length > 0) {
                                                var oFlow = {};
                                                if (data.FlowType === 'flow1') {
                                                    $.each(data.Users, function (idx, user) {
                                                        oFlow = {};
                                                        oFlow.id = guid();
                                                        oFlow.Order = item.Order + idx;
                                                        oFlow.SignedWay = data.FlowType;
                                                        oFlow.SignedMember = [{
                                                            id: user.id,
                                                            name: user.name,
                                                            deptname: user.deptname,
                                                            jobname: user.jobname
                                                        }];
                                                        saGridData.insert(item.Order + idx, oFlow);
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
                                                    saGridData.insert(item.Order, oFlow);
                                                }
                                                var iOrder = 1;
                                                $.each(saGridData, function (idx, _data) {
                                                    if (item.id !== _data.id) {
                                                        _data.Order = iOrder;
                                                        iOrder++;
                                                    }
                                                });
                                                saGridData = Enumerable.From(saGridData).Where(function (e) { return e.id !== item.id; }).ToArray();
                                                saGridData = releaseGridList(saGridData);
                                                $("#jsGrid").jsGrid("loadData");
                                            }
                                        };
                                        oPenUserListPop(oOption);
                                    }
                                })),
                                $('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                    class: 'glyphicon glyphicon-trash',
                                    title: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                    click: function () {
                                        var saNewList = Enumerable.From(saGridData).Where(function (e) { return e.id !== item.id; }).ToArray();
                                        saGridData = saNewList;
                                        $.each(saGridData, function (idx, _data) {
                                            _data.Order = idx + 1;
                                        });
                                        saGridData = releaseGridList(saGridData);
                                        $("#jsGrid").jsGrid("loadData");
                                    }
                                }))];

                                if (saGridData.length !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-down',
                                        title: i18next.t('common.Down'),// ╠common.Down⇒下移╣
                                        click: function () {
                                            var sOrder = Enumerable.From(saGridData).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(saGridData, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order++;
                                                }
                                                else if ((iOrder + 1) === _data.Order) {
                                                    _data.Order--;
                                                }
                                            });
                                            saGridData = releaseGridList(saGridData);
                                            $("#jsGrid").jsGrid("loadData");
                                        }
                                    })));
                                }
                                else {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }));
                                }

                                if (1 !== item.Order) {
                                    oBtns.push($('<div>', { class: 'fa-item col-sm-3' }).append($('<i>', {
                                        class: 'glyphicon glyphicon-arrow-up',
                                        title: i18next.t('common.Up'),// ╠common.Up⇒上移╣
                                        click: function () {
                                            var sOrder = Enumerable.From(saGridData).Where(function (e) { return e.id === item.id; }).ToString('', '$.Order'),
                                                iOrder = sOrder * 1;
                                            $.each(saGridData, function (idx, _data) {
                                                if (iOrder === _data.Order) {
                                                    _data.Order--;
                                                }
                                                else if ((iOrder - 1) === _data.Order) {
                                                    _data.Order++;
                                                }
                                            });
                                            saGridData = releaseGridList(saGridData);
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
                                data: saGridData,
                                itemsCount: saGridData.length //data.length
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

require(['base', 'select2', 'jsgrid', 'common_eip', 'util'], fnPageInit);