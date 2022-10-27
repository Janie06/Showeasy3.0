'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'UserID',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param {Object}  args 查詢條件參數
             */
            fnGetPro = function (args) {
                var oQueryPm = {},
                    oQuery = getFormSerialize(oForm),
                    sUserID = oQuery.UserID,
                    sDate = oQuery.Date;

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return CallAjax(ComFn.W_Com, ComFn.GetPagePrc, {
                    Type: 'wenzhong_getlist',
                    Params: {
                        querysort: oQueryPm.sortField + ' ' + oQueryPm.sortOrder,
                        pageindex: oQueryPm.pageIndex,
                        pagesize: oQueryPm.pageSize,
                        UserID: sUserID,
                        Date: sDate,
                        LeaveDate: '',
                        OrgID: parent.OrgID
                    }
                });
            },
            /**
            * 匯入費用項目
            */
            fnImport = function () {
                $('#importfile').val('').off('change').on('change', function () {
                    if (this.value.indexOf('.xls') > -1 || this.value.indexOf('.xlsx') > -1) {
                        var sFileId = guid(),
                            sFileName = this.value;
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
                            secureuri: false,
                            fileElementId: 'importfile',
                            success: function (data, status) {
                                g_api.ConnectLite(sProgramId, 'GetImport', {//匯入費用項目
                                    FileId: sFileId,
                                    FileName: sFileName
                                }, function (res) {
                                    if (res.RESULT) {
                                        $('#Toolbar_Qry').click();
                                    }
                                    else {
                                        showMsg(i18next.t('message.ProgressError') + '<br>' + res.MSG, 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                    }
                                }, function () {
                                    showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                });
                            },
                            error: function (data, status, e) {
                                showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                            }
                        });
                        bRequestStorage = true;
                    }
                    else {
                        showMsg(i18next.t("message.FileTypeError"), 'error'); // ╠message.FileTypeError⇒文件格式錯誤╣
                    }
                }).click();
            },
            /**
             * 資料刪除
             * @param {String} id pk
             */
            fnDel = function (id) {
                g_api.ConnectLite(sProgramId, 'GetDel', {//匯入費用項目
                    Guid: id
                }, function (res) {
                    if (res.RESULT) {
                        if (res.DATA.rel) {
                            $('#Toolbar_Qry').trigger('click');
                            showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                        }
                        else {
                            showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed") + '<br>' + res.MSG, 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
            * 修改文中設定
            */
            fnUpd = function (data) {//╠common.OriginalHours⇒原本時數╣╠common.Hours⇒小時╣╠common.UpdLeaveHours⇒變更時數╣
                var sContent = '\
                        <div class="row popsrow">\
                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.OriginalHours">原本時數</span>：</label>\
                             <label class="col-sm-8 show-text"><span id="OriginalHours"></span><span data-i18n="common.Hours">小時</span></label>\
                        </div>\
                        <div class="row popsrow">\
                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.UpdLeaveHours">變更時數</span>：</label>\
                             <div class="col-sm-8">\
                                 <input type="text" class="form-control w100p" id="UpdLeaveHours" maxlength="10">\
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
                    area: ['400px', '300px'],
                    content: sContent,
                    success: function (layero, index) {
                        layero.find('#OriginalHours').text(fMoney(data.PaymentHours * 1, 2));
                        layero.find('#UpdLeaveHours').on('keyup blur', function (e) {
                            keyIntp(e, this, 1);
                        });
                    },
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    yes: function (index, layero) {
                        var sUpdLeaveHours = layero.find('#UpdLeaveHours').val(),
                            sMemo = layero.find('#Memo').val(),
                            iTotal = 0;
                        iTotal = data.PaymentHours * 1 + sUpdLeaveHours * 1;
                        if (!sUpdLeaveHours) {
                            showMsg(i18next.t("message.UpdLeaveHours_required")); // 請輸入變更時數
                            return false;
                        }
                        else if (iTotal < 0) {
                            showMsg(i18next.t("message.PaymentHoursWarnning")); // 可用時數不可小於0
                            return false;
                        }
                        g_api.ConnectLite(sProgramId, 'UpdLeaveHours', {
                            Guid: data.Guid,
                            UpdLeaveHours: sUpdLeaveHours,
                            Memo: sMemo
                        }, function (res) {
                            if (res.RESULT) {
                                if (res.DATA.rel) {
                                    $('#Toolbar_Qry').trigger('click');
                                    showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                                }
                                else {
                                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                                }
                            }
                            else {
                                showMsg(i18next.t("message.Modify_Failed") + '<br>' + res.MSG, 'error'); //╠message.Modify_Failed⇒修改失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                        });

                        layer.close(index);
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
                        fnImport();
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

                var saDate = [],
                    iThisYear = new Date().getFullYear(),
                    iBaseYearCount = 20;
                while (iBaseYearCount >= 0) {
                    var iCurYear = iThisYear - iBaseYearCount;
                    saDate.push({ id: iCurYear, text: iCurYear });
                    iBaseYearCount--;
                }
                iBaseYearCount += 2;
                while (iBaseYearCount <= 20) {
                    var iCurYear = iThisYear + iBaseYearCount;
                    saDate.push({ id: iCurYear, text: iCurYear });
                    iBaseYearCount++;
                }
                $('#Date').html(createOptions(saDate, 'id', 'text'));

                fnSetUserDrop([{
                    Select: $('#UserID'),
                    ShowId: true,
                    Select2: true,
                    CallBack: function (data) {
                        var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87,
                            saFields = [
                                { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                                {
                                    name: "UserID", title: 'common.Account', align: "left", type: "text", width: 70
                                },
                                {
                                    name: "WenZhongAcount", title: 'MembersMaintain_Upd.WenZhongAcount', align: "left", type: "text", width: 70
                                },
                                {
                                    name: "UserName", title: 'common.EmployeeName', align: "left", type: "text", width: 80
                                },
                                {// ╠common.Seniority⇒年資╣
                                    name: "Seniority", title: 'common.Seniority', align: "center", type: "text", width: 50
                                },
                                {// ╠common.EnableDate⇒啟動日期╣
                                    name: "EnableDate", title: 'common.EnableDate', type: "text", align: "center", width: 100,
                                    itemTemplate: function (val, item) {
                                        return newDate(val, true);
                                    }
                                },
                                {// ╠common.ExpirationDate⇒失效日期╣
                                    name: "ExpirationDate", title: 'common.ExpirationDate', type: "text", align: "center", width: 100,
                                    itemTemplate: function (val, item) {
                                        return newDate(val, true);
                                    }
                                },
                                {// ╠common.PaymentHours⇒給付時數╣
                                    name: "PaymentHours", title: 'common.PaymentHours', align: "center", type: "text", width: 50
                                },
                                {// ╠common.UsedHours⇒已用時數╣
                                    name: "UsedHours", title: 'common.UsedHours', align: "center", type: "text", width: 50
                                },
                                {// ╠common.RemainHours⇒剩餘時數╣
                                    name: "RemainHours", title: 'common.RemainHours', align: "center", type: "text", width: 50
                                },
                                {
                                    name: "Memo", title: 'common.Memo', type: "text", width: 150
                                }
                            ];
                        if (parent.UserInfo.roles.indexOf('EipManager') > -1 || parent.UserInfo.roles.indexOf('Admin') > -1) {
                            saFields.push({
                                title: 'common.Action', width: 50, align: 'center',
                                itemTemplate: function (val, item) {
                                    var saAction = [$('<a/>', {
                                        html: i18next.t('common.Toolbar_Upd'),// ╠common.Toolbar_Upd⇒修改╣
                                        class: 'a-url',
                                        click: function () {
                                            fnUpd(item);
                                            return false;
                                        }
                                    })];
                                    if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                                        saAction.push($('<a/>', {
                                            html: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                            class: 'a-url delete',
                                            click: function () {
                                                // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
                                                layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                                    fnDel(item.Guid);
                                                    layer.close(index);
                                                });
                                            }
                                        }));
                                    }
                                    return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
                                }
                            });
                        }
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
                            fields: saFields,
                            controller: {
                                loadData: function (args) {
                                    return fnGetPro(args);
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                    }
                }]);
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'ajaxfile', 'util'], fnPageInit);