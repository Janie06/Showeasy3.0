'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var saLangTypes = [],
            saLangCountries = [],
            canDo = new CanDo({
                sortField: 'LangId',
                sortOrder: 'asc',
                inserting: true,
                /**
                 * 當前程式所有ID名稱集合
                 */
                idKeys: ['OrgID', 'ArgumentClassID', 'ArgumentID'],
                /**
                 * Grid欄位設置（可以是 function）
                 */
                gridFields: function () {
                    return [
                        { name: "RowIndex", title: 'common.RowNumber', editing: false, inserting: false, type: "text", width: 50, align: "center", sorting: false },
                        { name: "Type", title: 'Language_Set.LanguageType', editing: false, align: "left", type: "select", items: saLangTypes, valueField: "id", textField: "name", width: 200 },
                        { name: "Country", title: 'Language_Set.Language', editing: false, align: "left", type: "select", items: saLangCountries, valueField: "id", textField: "label", width: 100 },
                        { name: "LangId", title: 'Language_Set.languageId_required', editing: false, type: "text", width: 200, validate: { validator: 'required', message: i18next.t("Language_Set.languageId_required") } },// ╠Language_Set.languageId_required⇒語系ID不能為空╣
                        { name: "LangName", title: 'Language_Set.LanguageName', type: "text", width: 200 },
                        { name: "Memo", title: 'common.Memo', type: "text", width: 200 },
                        { type: "control" }
                    ];
                },
                /**
                 * 客製化按鈕
                 * @param  {Object} pargs CanDo 對象
                 */
                cusBtns: function (pargs) {
                    var saCusBtns = [{
                        id: 'CreateLangFile',
                        value: 'common.Toolbar_CreateLangFile',// ╠common.Toolbar_CreateLangFile⇒產生檔案╣
                        action: function (_pargs) {
                            fnCreateLangFile();
                        }
                    },
                    {
                        id: 'CopyLanguage',
                        value: 'common.Toolbar_CopyLanguage',// ╠common.Toolbar_CopyLanguage⇒語系複製╣
                        action: function (_pargs) {
                            fnCopyLanguageFile();
                        }
                    },
                    {
                        id: 'InitializeLanguage',
                        value: 'common.Toolbar_InitializeLanguage',// ╠common.Toolbar_InitializeLanguage⇒語系初始化╣
                        action: function (_pargs) {
                            fnInitializeLanguage();
                        }
                    }];
                    return saCusBtns;
                },
                /**
                 * 頁面初始化
                 * @param  {Object} pargs CanDo 對象
                 */
                pageInit: function (pargs) {
                    $.when(fnSetLanguageTypeDrop(),
                        fnSetArgDrop([
                            {
                                ArgClassID: 'LanCountry',
                                Select: $('#Country'),
                                ShowId: true,
                                CallBack: function (data) {
                                    saLangCountries = data;
                                }
                            }
                        ])).done(function () {
                            pargs._reSetQueryPm();
                            pargs._initGrid();
                        });
                }
            }),
            /**
             * 設置語言類別下拉單
             * @return {Object} Ajax 物件
             */
            fnSetLanguageTypeDrop = function () {
                return g_api.ConnectLite(canDo.ProgramId, 'GetSysHtmlPath', {
                    filepath: ''
                }, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $.each(saList, function (idx, data) {
                            data.id = getProgramId(data.id);
                            data.name = data.name.split("/").slice(-1)[0].match(/^[a-zA-Z0-9_]*/g);
                            //data.name = data.name.split("/").slice(-1)[0].match(/^[a-zA-Z0-9_]*/g);
                        });
                        saList.unshift({ id: 'other', name: '其他' });
                        saList.unshift({ id: 'message', name: '提示消息' });
                        saList.unshift({ id: 'common', name: '公用' });
                        saLangTypes = saList;

                        var sOptionHtml = createOptions(saList, 'id', 'name');
                        $('#LanguageType').html(sOptionHtml);
                    }
                    else {
                        showMsg(res.MSG, 'error'); //更新失敗
                    }
                });
            },
            /**
             * 初始化多語系
             */
            fnInitializeLanguage = function () {
                g_api.ConnectLite('Language', 'InitializeLanguage', {}, function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.Initialize_Success"), 'success'); // ╠message.Initialize_Success⇒初始化成功╣
                    }
                    else {
                        showMsg(i18next.t('message.Initialize_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Initialize_Failed⇒初始化失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Initialize_Failed"), 'error');// ╠message.Initialize_Failed⇒初始化失敗╣
                });
            },
            /**
             * 更新語系檔案
             */
            fnCreateLangFile = function () {
                g_api.ConnectLite('Language', 'CreateLangJson', {}, function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.Createlanguage_Success"), 'success'); // ╠message.Createlanguage_Success⇒語系檔產生成功╣
                    }
                    else {
                        showMsg(i18next.t('message.Createlanguage_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Createlanguage_Failed⇒語系檔產生失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Createlanguage_Failed"), 'error'); // ╠message.Createlanguage_Failed⇒語系檔產生失敗╣
                });
            },
            /**
             * 複製語系
             */
            fnCopyLanguageFile = function () {
                var oValidator = null;
                layer.open({
                    type: 2,
                    title: i18next.t('common.CopyLanguage'),// ╠common.CopyLanguage⇒語言複製╣
                    shade: 0.75,
                    maxmin: true, //开启最大化最小化按钮
                    area: ['600px', '240px'],
                    content: '/Page/Pop/CopyLanguage.html',
                    success: function (layero, index) {
                        var iframe = $('iframe').contents(),
                            sOptions = $('#Country').html();
                        iframe.find('#lang_from').html(sOptions);
                        iframe.find('#lang_to').html(sOptions);

                        $.validator.addMethod("comparlang_to", function (value, element, parms) {
                            if (value === iframe.find(parms).val()) {
                                return false;
                            }
                            return true;
                        });
                        oValidator = iframe.find("#CopyLanguage_form").validate({ //表單欄位驗證
                            rules: {
                                lang_from: 'required',
                                lang_to: { required: true, comparlang_to: '#lang_from' }
                            },
                            messages: {
                                lang_from: i18next.t('CopyLanguage.LangFrom_required'),// ╠CopyLanguage.LangFrom_required⇒請選擇來源語系╣
                                lang_to: { required: i18next.t('CopyLanguage.LangTo_required'), comparlang_to: i18next.t('CopyLanguage.LangTo_equalTo') }//╠CopyLanguage.LangTo_required⇒請選擇目的語系╣ ╠CopyLanguage.LangTo_equalTo⇒目的語系不能與來源語系一致╣
                            }
                        });
                        transLang(layero);
                    },
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    yes: function (index, layero) {
                        var iframe = $('iframe').contents();
                        if (!iframe.find("#CopyLanguage_form").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        var sLang_from = iframe.find('#lang_from').val(),
                            sLang_to = iframe.find('#lang_to').val(),
                            data = { LangFrom: sLang_from, LangTo: sLang_to, UserId: parent.UserInfo.MemberID };

                        g_api.ConnectLite(canDo.ProgramId, 'CopyLanguage', data, function (res) {
                            if (res.RESULT) {
                                showMsg(i18next.t("message.CopyLanguage_Success")); //╠message.CopyLanguage_Success⇒語系複製成功╣
                                layer.close(index);
                            }
                            else {
                                if (res.MSG == "0") {
                                    showMsg(i18next.t("message.CopyLanguage_Failed"), 'error'); //╠message.CopyLanguage_Failed⇒語言複製失敗╣
                                }
                                else if (res.MSG == "1") {
                                    showMsg(i18next.t("message.Lang_From_NotFond"), 'error'); //╠message.Lang_From_NotFond⇒來源語系為空╣
                                }
                                else {
                                    showMsg(res.MSG, 'error');
                                }
                            }
                        }, function () {
                            showMsg(i18next.t("message.CopyLanguage_Failed"), 'error');//╠message.CopyLanguage_Failed⇒語言複製失敗╣
                        });
                    }
                });
            };
    };

require(['base', 'jsgrid', 'cando'], fnPageInit);