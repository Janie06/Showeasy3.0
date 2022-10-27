'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('FileID'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = {},
            oForm = $('#form_main'),
            oValidator = null,
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                if (sDataId) {
                    return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            websitefiles: {
                                FileID: sDataId,
                                OrgID: parent.OrgID
                            }
                        }
                    }, function (res) {
                        if (res.d) {
                            var oRes = $.parseJSON(res.d);
                            oCurData = oRes;
                            setFormVal(oForm, oRes);
                            fnGetUploadFiles(oCurData.FileID, fnUpload);
                            setNameById().done(function () {
                                getPageVal();//緩存頁面值，用於清除
                            });
                            $('#UniqueID').attr('disabled', 'disabled');
                        }
                    });
                }
                else {
                    oCurData.FileID = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} sFlag 新增或儲存後新增
             */
            fnAdd = function (sFlag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.FileID = oCurData.FileID;

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        websitefiles: data
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
                });
            },
            /**
             * 修改資料
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.FileID = oCurData.FileID;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        websitefiles: {
                            values: data,
                            keys: {
                                FileID: sDataId,
                                OrgID: parent.OrgID
                            }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        showMsgAndGo(i18next.t("message.Modify_Success"), sQueryPrgId);//╠message.Modify_Success⇒修改成功╣
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
                        websitefiles: {
                            FileID: sDataId,
                            OrgID: parent.OrgID
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
             * 上傳附件
             * @param {Array} files 上傳的文件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'WebSiteFiles';
                option.type = 'list';
                option.parentid = oCurData.FileID;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
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
                    GoTop: true
                });
                $.validator.addMethod("uniqueidrule", function (value) {
                    var bRetn = true;
                    if (value) {
                        CallAjax(ComFn.W_Com, ComFn.GetCount, {
                            Params: {
                                websitefiles: {
                                    UniqueID: value,
                                    OrgID: parent.OrgID
                                }
                            }
                        }, function (rq) {
                            if (rq.d > 0) {
                                bRetn = false;
                            }
                        }, null, true, false);
                    }
                    return bRetn;
                });
                oValidator = $("#form_main").validate({
                    onfocusout: false,
                    rules: {
                        UniqueID: { uniqueidrule: sAction === 'Add' ? true : false },
                    },
                    messages: {
                        UniqueID: { uniqueidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                    }
                });

                fnGet();
            };

        init();
    };

require(['base', 'jsgrid', 'filer', 'util'], fnPageInit);