'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('SN'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = {},
            oForm = $('#form_main'),
            oValidator = null,
            oEditor = null,
            /**
             * 獲取資料
             * @return {Object} ajax 對象
             */
            fnGet = function () {
                if (sDataId) {
                    return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            news: {
                                SN: sDataId
                            }
                        }
                    }, function (res) {
                        if (res.d) {
                            var oRes = $.parseJSON(res.d);
                            oCurData = oRes;
                            if (!oCurData.News_Pic) {
                                oCurData.News_Pic = guid();
                            }
                            if (!oCurData.PicShowId) {
                                oCurData.PicShowId = guid();
                            }

                            //判斷非TG時是否需要生成展會花絮
                            if (oCurData.News_Type == "02" && parent.OrgID != "TG") {
                                $('.picshowid').show();
                            }

                            oEditor.ready(function () {
                                oEditor.setContent(oRes.News_Content);
                            });
                            setFormVal(oForm, oRes);
                            fnSetOrderByValueDrop(oRes.News_LanguageType).done(function () {
                                $('#OrderByValue').val(oRes.OrderByValue);
                            });
                            fnGetUploadFiles(oCurData.News_Pic, fnUpload, $('#fileInput'));
                            fnGetUploadFiles(oCurData.PicShowId, fnUpload, $('#fileInput_show'));
                            setNameById().done(function () {
                                getPageVal(); //緩存頁面值，用於清除
                            });
                        }
                    });
                }
                else {
                    oCurData.News_Pic = guid();
                    oCurData.PicShowId = guid();
                    fnUpload(null, oCurData.News_Pic, $('#fileInput'));
                    fnUpload(null, oCurData.PicShowId, $('#fileInput_show'));
                    return fnSetOrderByValueDrop();
                }
            },
            /**
             * 新增資料
             * @param  {String} sFlag 儲存 or 儲存后新增
             * @return {Object} ajax 對象
             */
            fnAdd = function (sFlag) {
                var data = getFormSerialize(oForm);
                data.OrgID = parent.OrgID;
                data.News_Pic = oCurData.News_Pic;
                data.PicShowId = oCurData.PicShowId;
                data.News_Content = oEditor.getContent();
                data.NewsContent = oEditor.getContentTxt();

                return g_api.ConnectLite(sProgramId, ComFn.GetAdd, data, function (res) {
                    if (res.RESULT) {
                        bRequestStorage = false;
                        if (sFlag === 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sQueryPrgId); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed") + '<br>' + res.MSG, 'error');// ╠message.Save_Failed⇒新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error');// ╠message.Save_Failed⇒新增失敗╣
                });
            },
            /**
             * 修改資料
             * @return {Object} ajax 對象
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);

                data.News_Content = oEditor.getContent();
                data.NewsContent = oEditor.getContentTxt();
                data.SN = sDataId;
                data.OrgID = parent.OrgID;
                data.News_Pic = oCurData.News_Pic;
                data.PicShowId = oCurData.PicShowId;

                return g_api.ConnectLite(sProgramId, ComFn.GetUpd, data, function (res) {
                    if (res.RESULT) {
                        bRequestStorage = false;
                        showMsgAndGo(i18next.t("message.Modify_Success"), sQueryPrgId); //╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed") + '<br>' + res.MSG, 'error');// ╠message.Modify_Failed⇒修改失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                });
            },
            /**
             * 資料刪除
             * @return {Object} ajax 對象
             */
            fnDel = function () {
                return g_api.ConnectLite(sProgramId, ComFn.GetDel, { Id: sDataId }, function (res) {
                    if (res.RESULT) {
                        showMsgAndGo(i18next.t("message.Delete_Success"), sQueryPrgId); // ╠message.Delete_Success⇒刪除成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed") + '<br>' + res.MSG, 'error');// ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
             * 設定排序下拉選單
             * @param  {String} parentid 父層id
             * @return {Object} ajax 對象
             */
            fnSetOrderByValueDrop = function (parentid) {
                var oParams = {
                    news: {
                        News_Show: 'Y',
                        OrgID: parent.OrgID
                    }
                };
                if (parentid) {
                    oParams.news.News_LanguageType = parentid;
                }
                return CallAjax(ComFn.W_Com, ComFn.GetCount, {
                    Params: oParams
                }, function (res) {
                    var iCount = res.d;
                    if (sAction === 'Add') {
                        iCount++;
                    }
                    $('#OrderByValue').html(createOptions(iCount));
                    if (sAction === 'Add') {
                        $('#OrderByValue').val(1);
                    }
                });
            },
            /**
             * 上傳附件
             * @param {Array} files 上傳的文件
             * @param {String} parentid 父層id
             * @param {String} finput file input id
             */
            fnUpload = function (files, parentid, finput) {
                var option = {},
                    sFilesType = finput.attr('data-file');
                option.input = finput;
                option.limit = 1;
                option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
                option.folder = 'News';
                option.type = 'one';
                option.maxSize = 1;
                option.theme = 'dragdropbox' + parentid;
                option.parentid = parentid;
                if (files) {
                    option.files = files;
                }
                if (sFilesType === 'show') {
                    option.limit = 99;
                    delete option.type;
                }
                fnUploadRegister(option);
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             * @return {Boolean} 是否停止
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

                if (parent.OrgID === 'SG') {
                    $('.picshowid').show();
                };

                $.validator.addMethod("compardate", function (value, element, parms) {
                    if (new Date(value) < new Date($('#News_StartDete').val())) {
                        return false;
                    }
                    return true;
                });
                oValidator = $("#form_main").validate();

                oEditor = UE.getEditor('News_Content');

                $('#News_LanguageType').change(function () {
                    fnSetOrderByValueDrop(this.value);
                });

                //TE新增新類別展會花絮顯示條件 2019/04/15 Yang
                $('#News_Type').change(function () {
                    if (parent.OrgID === 'TE') {
                        $(this).val() == '02' ? $('.picshowid').show(): $('.picshowid').hide() ; 
                    }
                })

                fnSetArgDrop([
                    {
                        ArgClassID: 'LanCountry',
                        Select: $('#News_LanguageType'),
                        ShowId: true
                    },
                    {
                        ArgClassID: 'News_Class',
                        Select: $('#News_Type'),
                        ShowId: true
                    }
                ])
                    .done(function () {
                        fnGet();
                    });
            };

        init();
    };

require(['base', 'jsgrid', 'filer', 'util'], fnPageInit);