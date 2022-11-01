'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('Guid'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = { CostRules: [] },
            oForm = $('#form_main'),
            oGrid = null,
            oValidator = null,
            oEditor_CostInstruction = null,
            oEditor_ServiceInstruction = null,
            oEditor_CostInstruction_EN = null,
            oEditor_ServiceInstruction_EN = null,
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                if (sDataId) {
                    return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            exhibitionrules: {
                                Guid: sDataId,
                                OrgID: parent.OrgID
                            }
                        }
                    }, function (res) {
                    });
                }
                else {
                    oCurData.Guid = guid();
                    oCurData.FileId_EN = guid();
                    fnUpload(null, oCurData.Guid, 'fileInput');
                    fnUpload(null, oCurData.FileId_EN, 'fileInput_EN');
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param  {String}  sFlag 新增或儲存後新增
             */
            fnAdd = function (sFlag) {
                var data = getFormSerialize(oForm);
                data.StoragePrice = 0; //因應變更寫法，預設0  By Mark
                data.IsMerge = 'N'; //因應變更寫法，預設N  By Mark
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.Guid = oCurData.Guid;
                data.FileId_EN = oCurData.FileId_EN;
                data.CostInstruction = oEditor_CostInstruction.getContent();
                data.ServiceInstruction = oEditor_ServiceInstruction.getContent();
                data.CostInstruction_EN = oEditor_CostInstruction_EN.getContent();
                data.ServiceInstruction_EN = oEditor_ServiceInstruction_EN.getContent();
                delete data.editorValue;
                data.CostRules = JSON.stringify(oCurData.CostRules);
                data.PackingPrice = data.PackingPrice === '' ? 0 : data.PackingPrice;
                data.FeedingPrice = data.FeedingPrice === '' ? 0 : data.FeedingPrice;
                data.StoragePrice = data.StoragePrice === '' ? 0 : data.StoragePrice;

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        exhibitionrules: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (sFlag === 'add') {
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
                data.CostInstruction = oEditor_CostInstruction.getContent();
                data.ServiceInstruction = oEditor_ServiceInstruction.getContent();
                data.CostInstruction_EN = oEditor_CostInstruction_EN.getContent();
                data.ServiceInstruction_EN = oEditor_ServiceInstruction_EN.getContent();
                delete data.editorValue;
                data.CostRules = JSON.stringify(oCurData.CostRules);
                data.FileId_EN = oCurData.FileId_EN;

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        exhibitionrules: {
                            values: data,
                            keys: {
                                Guid: sDataId,
                                OrgID: parent.OrgID
                            }
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
                        exhibitionrules: {
                            Guid: sDataId,
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
             * @param {Array} files 要綁定的資料
             * @param {Sring} parentid 要綁定的資料
             * @param {Sring} flag 要綁定的資料
             */
            fnUpload = function (files, parentid, flag) {
                var option = {};
                option.input = $('#' + flag);
                option.theme = 'dragdropbox';
                option.folder = 'WebSiteFiles';
                option.type = 'list';
                option.parentid = parentid;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * Grid客戶化計價模式控件
             * @param {Sring}flag 要綁定的資料標記
             * @return {HTMLElement} DIV 物件
             */
            fnCreatePriceModeInput = function (flag) {
                var $div = $('<div>'),
                    data = [{ id: 'N', text: i18next.t('common.ByNumber') }, { id: 'T', text: i18next.t('common.ByWeight') }];//╠common.ByNumber⇒按件數計價╣ ╠common.ByWeight⇒按重量計價╣
                $div.html(createRadios(data, 'id', 'text', '~PricingMode' + flag, flag, false, false));
                return $div;
            },
            /**
             * ToolBar 按鈕事件 function
             * @param  {Object}inst 按鈕物件對象
             * @param  {Object} e 事件對象
             * @return {Boolean} 是否停止
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":
                        $('#CostInstruction').val(oEditor_CostInstruction.getContentTxt());
                        $('#ServiceInstruction').val(oEditor_ServiceInstruction.getContentTxt());
                        $('#CostInstruction_EN').val(oEditor_CostInstruction_EN.getContentTxt());
                        $('#ServiceInstruction_EN').val(oEditor_ServiceInstruction_EN.getContentTxt());
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
                        $('#CostInstruction').val(oEditor_CostInstruction.getContentTxt());
                        $('#ServiceInstruction').val(oEditor_ServiceInstruction.getContentTxt());
                        $('#CostInstruction_EN').val(oEditor_CostInstruction_EN.getContentTxt());
                        $('#ServiceInstruction_EN').val(oEditor_ServiceInstruction_EN.getContentTxt());
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
                oValidator = $("#form_main").validate({
                    ignore: ''
                });
                oEditor_CostInstruction = UE.getEditor('EU_CostInstruction');
                oEditor_ServiceInstruction = UE.getEditor('EU_ServiceInstruction');
                oEditor_CostInstruction_EN = UE.getEditor('EU_CostInstruction_EN');
                oEditor_ServiceInstruction_EN = UE.getEditor('EU_ServiceInstruction_EN');

                $.whenArray([
                    fnGet(),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'Currency',
                            LevelOfArgument: 0,
                            ShowId: true,
                            CallBack: function (data) {
                                $('#Currency').html(createOptions(data, 'id', 'id'))[0].remove(0);
                            }
                        }
                    ])
                ]).done(function (res1) {
                    if (res1 && res1[0].d) {
                        var oRes = $.parseJSON(res1[0].d);
                        oCurData = oRes;
                        oCurData.CostRules = $.parseJSON(oCurData.CostRules);
                        if (!oCurData.FileId_EN) {
                            oCurData.FileId_EN = guid();
                        }
                        setFormVal(oForm, oRes);
                        oEditor_CostInstruction.ready(function () {
                            oEditor_CostInstruction.setContent(oRes.CostInstruction);
                        });
                        oEditor_ServiceInstruction.ready(function () {
                            oEditor_ServiceInstruction.setContent(oRes.ServiceInstruction);
                        });
                        oEditor_CostInstruction_EN.ready(function () {
                            oEditor_CostInstruction_EN.setContent(oRes.CostInstruction_EN || '');
                        });
                        oEditor_ServiceInstruction_EN.ready(function () {
                            oEditor_ServiceInstruction_EN.setContent(oRes.ServiceInstruction_EN || '');
                        });
                        $("#jsGrid").jsGrid("loadData");
                        fnGetUploadFiles(oCurData.Guid, fnUpload, 'fileInput');
                        fnGetUploadFiles(oCurData.FileId_EN, fnUpload, 'fileInput_EN');
                        setNameById().done(function () {
                            getPageVal();//緩存頁面值，用於清除
                        });
                        $('#UniqueID').attr('disabled', 'disabled');
                    }
                    $(':input[data-type="money"]').each(function () {
                        moneyInput($(this), 2);
                    });
                });

                $("#jsGrid").jsGrid({
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
                            name: "Index", title: '#', width: 50, align: 'center'
                        },
                        {// ╠common.Weight_Min⇒重量（小）╣
                            name: "Weight_Min", title: 'common.Weight_Min', width: 120, align: 'center', type: "text", validate: { validator: 'required', message: i18next.t('common.Weight_Min_required') }//╠common.Weight_Min_required⇒請輸入重量（小）╣
                        },
                        {// ╠common.Weight_Max⇒重量（大）╣
                            name: "Weight_Max", title: 'common.Weight_Max', width: 120, align: 'center', type: "text", validate: { validator: 'required', message: i18next.t('common.Weight_Max_required') }//╠common.Weight_Max_required⇒請輸入重量（大）╣
                        },
                        {// ╠common.Price⇒價格╣
                            name: "Price", title: 'common.Price', width: 120, align: 'right', type: "text", validate: { validator: 'required', message: i18next.t('common.Price_required') },//╠common.Price_required⇒請輸入價格╣
                            itemTemplate: function (val, item) {
                                return fMoney(val, 2, item.Currency);
                            },
                            insertTemplate: function (val, item) {
                                var oControl = $('<input type="text" class="form-control w100p" data-type="money" data-name="int" />');
                                moneyInput(oControl, 2);
                                return this.insertControl = oControl;
                            },
                            insertValue: function () {
                                return this.insertControl.attr('data-value');
                            },
                            editTemplate: function (val, item) {
                                var oControl = $('<input type="text" class="form-control w100p" data-type="money" data-name="int" />').val(val);
                                moneyInput(oControl, 2);
                                return this.editControl = oControl;
                            },
                            editValue: function () {
                                return this.editControl.attr('data-value');
                            }
                        },
                        {// ╠common.PricingMode⇒計價模式╣
                            name: "PricingMode", title: 'common.PricingMode', align: 'center', width: 180, type: "text",
                            itemTemplate: function (val, item) {
                                var oControl = fnCreatePriceModeInput(item.Index);
                                oControl.find(':input[value="' + val + '"]').click();
                                uniformInit(oControl);
                                return oControl;
                            },
                            insertTemplate: function (val, item) {
                                var oControl = fnCreatePriceModeInput('add');
                                setTimeout(function () {
                                    oControl.find('label:first').click();
                                    uniformInit(oControl);
                                }, 100);
                                return this.insertControl = oControl;
                            },
                            insertValue: function () {
                                return this.insertControl.find(':input:checked').val();
                            },
                            editTemplate: function (val, item) {
                                var oControl = fnCreatePriceModeInput('edit');
                                oControl.find(':input[value="' + val + '"]').click();
                                uniformInit(oControl);
                                return this.editControl = oControl;
                            },
                            editValue: function () {
                                return this.editControl.find(':input:checked').val();
                            }
                        },
                        {
                            name: "Memo", title: 'common.Memo', width: 200, type: "textarea"
                        },
                        {
                            type: "control", width: 50
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: oCurData.CostRules,
                                itemsCount: oCurData.CostRules.length //data.length
                            };
                        },
                        insertItem: function (args) {
                            args.guid = guid();
                            args.Index = oCurData.CostRules.length + 1;
                            oCurData.CostRules.push(args);
                        },
                        updateItem: function (args) {
                        },
                        deleteItem: function (args) {
                            oCurData.CostRules = Jsonremove(oCurData.CostRules, 'guid', args.guid);
                            $.each(oCurData.CostRules, function (idx, _data) {
                                _data.Index = idx + 1;
                            });
                        }
                    },
                    onInit: function (args) {
                        oGrid = args.grid;
                    }
                });
            };

        init();
    };

require(['base', 'jsgrid', 'formatnumber', 'filer', 'util'], fnPageInit);