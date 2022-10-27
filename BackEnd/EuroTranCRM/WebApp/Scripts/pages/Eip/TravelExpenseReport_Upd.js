'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sViewPrgId = sProgramId.replace('_Upd', '_View'),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CheckOrder: [], TravelFeeItems: [] },
            oForm = $('#form_main'),
            oGrid = null,
            oValidator = null,
            sOptionHtml_Users = '',
            sOptionHtml_Currency = '',
            oAddItem = {},
            saUsers = [],

            /**
             * 獲取資料
             * @return  {Object} ajax物件
             */
            fnGet = function () {
                if (sDataId) {
                    return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel,
                                    sDateRange = '';
                                oCurData = oRes;
                                oCurData.TravelFeeItems = $.parseJSON(oCurData.TravelFeeItems);
                                oCurData.TravelFeeInfo = $.parseJSON(oCurData.TravelFeeInfo);
                                oCurData.CheckOrder = $.parseJSON(oCurData.CheckOrder);
                                setFormVal(oForm, oRes);
                                if (oCurData.TravelDateStart) {
                                    sDateRange = newDate(oCurData.TravelDateStart, 'date', true) + ' ~ ' + newDate(oCurData.TravelDateEnd, 'date', true);
                                }
                                $('#TravelDate').val(sDateRange);
                                $('.Applicant').text(oCurData.ApplicantName + '(' + oCurData.Applicant + ')  ' + oCurData.DeptName);
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
                    $('.Applicant').text(parent.UserInfo.MemberName + '(' + parent.UserInfo.MemberID + ')  ' + parent.UserInfo.DepartmentName);
                    $('#Applicant').val(parent.UserInfo.MemberID);
                    oCurData.TravelFeeItems = [];
                    oCurData.CheckOrder = [];
                    oCurData.Guid = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 設定展覽下拉選單，客制過
             * @param {Object} drop 當前控件
             * @return {Object} Ajax 物件
             */
            fnSetEpoDropCus = function (drop) {
                return g_api.ConnectLite('Exhibition_Upd', 'GetExhibitions',
                    {
                        SN: drop.SN || ''
                    },
                    function (res) {
                        if (res.RESULT) {
                            var saRes = res.DATA.rel;
                            if (drop.Select) {
                                drop.Select.html(createOptions(saRes, drop.IdName || 'SN', drop.TextName || 'ExhibitioFullName', drop.ShowId || false, "ExhibitioShotName_TW"));
                                if (drop.DefultVal) {
                                    drop.Select.val(drop.DefultVal);
                                }
                                if (drop.Select2) {
                                    drop.Select.each(function () {
                                        $(this).select2();
                                        $(this).next().after($(this));
                                    });
                                }
                            }
                            if (drop.CallBack && typeof drop.CallBack === 'function') {
                                drop.CallBack(saRes);
                            }
                        }
                    });
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
                data.SignedNumber = 'SerialNumber|' + parent.UserInfo.OrgID + '|TER|MinYear|3|' + parent.UserInfo.ServiceCode + '|' + parent.UserInfo.ServiceCode;
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.TravelFeeItems = JSON.stringify(oCurData.TravelFeeItems);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.TravelFeeInfo.Total = data.TravelFeeInfo.Total || 0;
                data.TravelFeeInfo.Sum = data.TravelFeeInfo.Sum || 0;
                data.TravelFeeInfo = JSON.stringify(data.TravelFeeInfo);
                data.Status = 'A';
                data.IsHandled = 'N';
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.ExhibitionName = $('#ExhibitionNO option:selected').attr('exhibitioshotname_tw');
                if (!data.TravelDate) {
                    delete data.TravelDateStart;
                    delete data.TravelDateEnd;
                }
                else {
                    data.TravelDateStart = $.trim(data.TravelDate.split('~')[0]);
                    data.TravelDateEnd = $.trim(data.TravelDate.split('~')[1]);
                }
                delete data.TravelDate;

                g_api.ConnectLite(sProgramId, 'Insert', data, function (res) {
                    if (res.DATA.rel) {
                        fnReloadFeeItem(res.DATA.rel);
                        bRequestStorage = false;
                        if (flag === 'add') {
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
             * @return  {Object} ajax物件
             */
            fnUpd = function (balert) {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.CheckFlows = fnCheckFlows(oCurData, false, true, saUsers);
                data.HandleFlows = fnHandleFlows(oCurData, saUsers);
                data.TravelFeeItems = JSON.stringify(oCurData.TravelFeeItems);
                data.CheckOrder = JSON.stringify(oCurData.CheckOrder);
                data.TravelFeeInfo = JSON.stringify(data.TravelFeeInfo);
                data.Status = oCurData.Status;
                data.Inspectors = '';
                data.Reminders = '';
                data.Flows_Lock = oCurData.Flows_Lock;
                data.Handle_Lock = oCurData.Handle_Lock;
                data.ExhibitionName = $('#ExhibitionNO option:selected').attr('exhibitioshotname_tw');
                data.Guid = oCurData.Guid;
                if (!data.TravelDate) {
                    delete data.TravelDateStart;
                    delete data.TravelDateEnd;
                }
                else {
                    data.TravelDateStart = $.trim(data.TravelDate.split('~')[0]);
                    data.TravelDateEnd = $.trim(data.TravelDate.split('~')[1]);
                }
                delete data.TravelDate;

                return g_api.ConnectLite(sProgramId, 'Update', data, function (res) {
                    if (res.DATA.rel) {
                        fnReloadFeeItem(res.DATA.rel);
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
                        travelexpense: {
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
                option.folder = 'TravelExpenseReport';
                option.type = 'list';
                option.parentid = oCurData.Guid;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 重新計算資料
             */
            fnReloadFeeItem = function (Data) {
                oCurData.TravelFeeItems = $.parseJSON(Data.TravelFeeItems);
                oCurData.TravelFeeInfo = $.parseJSON(Data.TravelFeeInfo);
                $("#jsGrid1").jsGrid("loadData");
                fnSumFeeItems();
            },

            /**
             * 提交簽呈
             */
            fnSubmitPetition = function () {
                g_api.ConnectLite(sProgramId, 'TravelExpenseReportToAudit', {
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
             * 計算費用項目
             * @param   {Object}item 按鈕物件對象
             * @param   {Object}input 當前dom元素
             */
            fnSumFeeItem = function (item, input) {
                var iAll = (item.ExchangeRate || 1) * ((item.Amount1 || 0) * 1 + (item.Amount2 || 0) * 1 + (item.Amount3 || 0) * 1);
                $(input).parents('tr').find('.total').val(fMoney(iAll, 2, item.Currency));
            },
            /**
             * 總計所有費用項目
             */
            fnSumFeeItems = function () {
                var iAll = 0, sAll = '';
                $.each(oCurData.TravelFeeItems, function (idx, item) {
                    iAll += (item.ExchangeRate || 1) * ((item.Amount1 || 0) * 1 + (item.Amount2 || 0) * 1 + (item.Amount3 || 0) * 1);
                });
                sAll = fMoney(iAll, 0, 'NTD');
                $('#TravelFeeInfo_Total').val(sAll).attr('data-value', sAll.replaceAll(',', '')).change();
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             * @return  {Object}是否停止
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

                $('#TravelDate').dateRangePicker(
                    {
                        language: 'zh-TW',
                        separator: ' ~ ',
                        format: 'YYYY/MM/DD',
                        autoClose: true
                    });

                $.whenArray([
                    fnSetDeptDrop($('#Handle_DeptID')),
                    fnSetFlowDrop({
                        Flow_Type: parent.SysSet.Eip_004,
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
                    fnSetEpoDropCus({
                        Select: $('#ExhibitionNO'),
                        IdName: 'ExhibitionCode',
                        TextName: 'ExhibitioFullName',
                        Select2: true
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
                            ArgClassID: 'Currency',
                            CallBack: function (data) {
                                sOptionHtml_Currency = createOptions(data, 'id', 'id');
                            }
                        }
                    ])])
                    .done(function () {
                        fnGet().done(function () {
                            moneyInput($('[data-type="int"]'), 2, true);
                        });

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
                            rowClick: function (args) {},
                            fields: [
                                {
                                    name: "Date", title: 'Date', width: 100, type: "text", validate: { validator: 'required', message: i18next.t('common.Date_required') },// ╠common.Date_required⇒請輸入日期╣
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
                                {
                                    name: "Particulars", title: 'PARTICULARS', width: 150, type: "text",
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
                                    name: "Currency", title: 'common.Financial_Currency', width: 70, type: "text", validate: { validator: 'required', message: i18next.t('common.Currency_required') },// ╠common.Currency_required⇒請選擇幣別╣
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control",
                                            html: sOptionHtml_Currency
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<select />', {
                                            class: "form-control",
                                            html: sOptionHtml_Currency
                                        }).val(item.Currency);
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {// ╠common.TravelExpenseFee1⇒日支額╣
                                    name: "Amount1", title: 'common.TravelExpenseFee1', width: 110, type: "text", align: "right", validate: { validator: 'required', message: i18next.t('common.TravelExpenseFee1_required') },// ╠common.TravelExpenseFee1_required⇒請輸入日支額╣
                                    itemTemplate: function (val, item) {
                                        return fMoney(item.Amount1, 2, item.Currency);
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            change: function () {
                                                oAddItem.Amount1 = $(this).attr('data-value');
                                                fnSumFeeItem(oAddItem, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.attr('data-value');
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            value: item.Amount1,
                                            change: function () {
                                                item.Amount1 = $(this).attr('data-value');
                                                fnSumFeeItem(item, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.attr('data-value');
                                    }
                                },
                                {// ╠common.TravelExpenseFee2⇒住宿費╣
                                    name: "Amount2", title: 'common.TravelExpenseFee2', width: 110, type: "text", align: "right", validate: { validator: 'required', message: i18next.t('common.TravelExpenseFee2_required') },// ╠common.TravelExpenseFee2_required⇒請輸入住宿費╣
                                    itemTemplate: function (val, item) {
                                        return fMoney(item.Amount2, 2, item.Currency);
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            change: function () {
                                                oAddItem.Amount2 = $(this).attr('data-value');
                                                fnSumFeeItem(oAddItem, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.attr('data-value');
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            value: item.Amount2,
                                            change: function () {
                                                item.Amount2 = $(this).attr('data-value');
                                                fnSumFeeItem(item, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.attr('data-value');
                                    }
                                },
                                {// ╠common.TravelExpenseFee3⇒其他╣
                                    name: "Amount3", title: 'common.TravelExpenseFee3', width: 110, type: "text", align: "right", validate: { validator: 'required', message: i18next.t('common.TravelExpenseFee3_required') },// ╠common.TravelExpenseFee3_required⇒請輸入其他╣
                                    itemTemplate: function (val, item) {
                                        return fMoney(item.Amount3, 2, item.Currency);
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p left wright amount",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            change: function () {
                                                oAddItem.Amount3 = $(this).attr('data-value');
                                                fnSumFeeItem(oAddItem, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.attr('data-value');
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            value: item.Amount3,
                                            change: function () {
                                                item.Amount3 = $(this).attr('data-value');
                                                fnSumFeeItem(item, this);
                                            }
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.attr('data-value');
                                    }
                                },
                                {// ╠common.ExchangeRate⇒匯率╣
                                    name: "ExchangeRate", title: 'common.ExchangeRate', width: 100, type: "text", align: "center",
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            change: function () {
                                                oAddItem.ExchangeRate = this.value;
                                                fnSumFeeItem(oAddItem, this);
                                            }
                                        });
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        return this.insertControl.val();
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control",
                                            value: val,
                                            change: function () {
                                                item.ExchangeRate = this.value;
                                                fnSumFeeItem(item, this);
                                            }
                                        });
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        return this.editControl.val();
                                    }
                                },
                                {
                                    name: "Total", title: 'TWD', width: 110, type: "text", align: "right",
                                    itemTemplate: function (val, item) {
                                        return fMoney(val, 2, item.Currency);
                                    },
                                    insertTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p total",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            disabled: 'disabled',
                                            value: val
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.insertControl = oControl;
                                    },
                                    insertValue: function () {
                                        let AmountStr = this.insertControl.val().replaceAll(',', '');
                                        let PositiveInt = Math.round(AmountStr);
                                        this.insertControl.val(PositiveInt);
                                        this.insertControl[0].dataset.value = PositiveInt;
                                        return this.insertControl.val().replaceAll(',', '');
                                    },
                                    editTemplate: function (val, item) {
                                        var oControl = $('<input />', {
                                            class: "form-control w100p total",
                                            'data-type': 'int',
                                            'data-name': 'int',
                                            disabled: 'disabled',
                                            value: val
                                        });
                                        moneyInput(oControl, 2, true);
                                        return this.editControl = oControl;
                                    },
                                    editValue: function () {
                                        let AmountStr = this.editControl.val().replaceAll(',', '');
                                        let PositiveInt = Math.round(AmountStr);
                                        this.editControl.val(PositiveInt);
                                        this.editControl[0].dataset.value = PositiveInt;
                                        return this.editControl.val().replaceAll(',', '');
                                    }
                                },
                                {
                                    type: "control", width: 50
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return {
                                        data: oCurData.TravelFeeItems,
                                        itemsCount: oCurData.TravelFeeItems.length //data.length
                                    };
                                },
                                insertItem: function (args) {
                                    args.guid = guid();
                                    args.Index = oCurData.TravelFeeItems.length + 1;
                                    oCurData.TravelFeeItems.push(args);
                                    oAddItem = {};
                                },
                                updateItem: function (args) {
                                },
                                deleteItem: function (args) {
                                    var saNewTravelFeeItems = [];
                                    $.each(oCurData.TravelFeeItems, function (idx, _data) {
                                        if (_data.guid !== args.guid) {
                                            saNewTravelFeeItems.push(_data);
                                        }
                                    });
                                    $.each(saNewTravelFeeItems, function (idx, _data) {
                                        _data.Index = idx + 1;
                                    });
                                    oCurData.TravelFeeItems = saNewTravelFeeItems;
                                }
                            },
                            onDataLoaded: function (args) {
                            },
                            onItemInserted: function (args) {
                                fnSumFeeItems();
                            },
                            onItemUpdated: function (args) {
                                fnSumFeeItems();
                            },
                            onItemDeleted: function (args) {
                                fnSumFeeItems();
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
                $('#TravelFeeInfo_Total,#TravelFeeInfo_CompanyAdvance').on('change', function () {
                    var iTotal = 0, iAdvance = 0, iSum = 0, sSum = '';
                    if (this.id === 'TravelFeeInfo_Total') {
                        iTotal = $(this).attr('data-value');
                        iAdvance = $('#TravelFeeInfo_CompanyAdvance').attr('data-value');
                    }
                    else {
                        iTotal = $('#TravelFeeInfo_Total').attr('data-value');
                        iAdvance = $(this).attr('data-value') || $(this).val();
                    }
                    iSum = iTotal * 1 - (!iAdvance ? 0 : iAdvance) * 1;
                    sSum = fMoney(iSum, 0, 'NTD');
                    $('#TravelFeeInfo_CompanyCope,#TravelFeeInfo_Sum').val(sSum).attr('data-value', sSum.replaceAll(',', ''));
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
                                    sIcon += '<br><img src="../../images/flow_arrow.gif" style="vertical-align:top;">';
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
                                                else if (iOrder + 1 === _data.Order) {
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
                                                else if (iOrder - 1 === _data.Order) {
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

require(['base', 'select2', 'jsgrid', 'daterangepicker', 'formatnumber', 'filer', 'common_eip', 'util'], fnPageInit, 'daterangepicker');