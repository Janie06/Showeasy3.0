'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'TemplID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['TemplID'],
        /**
         * 當前程式所有參數名稱集合
         */
        jsonStrKeys: ['TemplKeys'],
        /**
         * 處理新增資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getInsertParams: function (pargs, data) {
            data.FileID = pargs.data.FileID;
            data.TemplKeys = JSON.stringify(pargs.data.TemplKeys);
            return data;
        },
        /**
         * 處理修改資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getUpdateParams: function (pargs, data) {
            return pargs.options.getInsertParams(pargs, data);
        },
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("templidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            TemplID: value,
                        },
                        function (res) {
                            if (res.RESULT && res.DATA.rel > 0) {
                                bRetn = false;
                            }
                        }, null, false);
                }
                return bRetn;
            });
        },
        /**
         * 驗證規則
         */
        validRules: function (pargs) {
            return {
                onfocusout: false,
                rules: {
                    TemplID: { templidrule: pargs.action === 'add' ? true : false },
                },
                messages: {
                    TemplID: { templidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                }
            };
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            if (pargs.action === 'upd') {
                $('#TemplID').prop('disabled', true);
                pargs._getOne().done(function (res) {
                    pargs.data.FileID = pargs.data.FileID || guid();
                    fnBindKeys();
                    fnGetUploadFiles(pargs.data.FileID, fnUpload);
                });
            }
            else {
                pargs.data.FileID = guid();
                pargs.data.TemplKeys = [];
                fnUpload();
            }
            $('.plustemplkey').on('click', function () {
                var oNewKey = {};
                oNewKey.guid = guid();
                oNewKey.TemplKey = '';
                oNewKey.TemplKeyValue = '';
                oNewKey.TemplName = '';
                oNewKey.Memo = '';
                canDo.data.TemplKeys.push(oNewKey);
                fnBindKeys();
            });
        }
    }),
        /**
         * 綁定模版參數
         * @param {Array} files 上傳的文件
         */
        fnBindKeys = function () {
            var sKeysHtml = '';
            $.each(canDo.data.TemplKeys, function (idx, item) {
                sKeysHtml += '<tr data-id="' + item.guid + '">\
                              <td class="wcenter">' + (idx + 1) + '</td>\
                              <td class="wcenter"><input type="text" data-input="TemplKey" class="form-control w100p" value="' + item.TemplKey + '"></td>\
                              <td><input type="text" data-input="TemplKeyValue" class="form-control w100p" value="' + item.TemplKeyValue + '"></td>\
                              <td><input type="text" data-input="TemplName" class="form-control w100p" value="' + item.TemplName + '"></td>\
                              <td><input type="text" data-input="Memo" class="form-control w100p" value="' + item.Memo + '"></td>\
                              <td class="wcenter">\
                                 <i class="glyphicon glyphicon-trash" data-value="' + item.guid + '" title="刪除"></i>\
                              </td>\
                             </tr>';
            });
            $('#table_box').html(sKeysHtml).find('.glyphicon-trash').on('click', function () {
                var sId = $(this).attr('data-value'),
                    saNewList = [];

                $.each(canDo.data.TemplKeys, function (idx, item) {
                    if (sId !== item.guid) {
                        saNewList.push(item);
                    }
                });
                $(this).parents('tr').remove();
                canDo.data.TemplKeys = saNewList;
            });
            $('#table_box').find('[data-input]').on('change', function () {
                var sKey = $(this).attr('data-input'),
                    sId = $(this).parents('tr').attr('data-id'),
                    sVal = this.value;

                $.each(canDo.data.TemplKeys, function (idx, item) {
                    if (sId === item.guid) {
                        item[sKey] = sVal;
                        return false;
                    }
                });
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
            option.folder = 'OfficeTemplate';
            option.type = 'one';
            option.limit = 1;
            option.parentid = canDo.data.FileID;
            if (files) {
                option.files = files;
            }
            fnUploadRegister(option);
        };
};

require(['base', 'filer', 'cando'], fnPageInit);