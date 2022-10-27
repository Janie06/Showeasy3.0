'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['OrgID'],
        /**
         * 修改完后是否跳轉
         */
        updateGo: false,
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("orgidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            OrgID: value,
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
                    OrgID: { orgidrule: pargs.action === 'add' ? true : false },
                },
                messages: {
                    OrgID: { orgidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                }
            };
        },
        /**
         * 須初始化的UEEditer 的物件ID集合
         */
        ueEditorIds: ['ServiceTitle', 'ServiceTitle_CN', 'ServiceTitle_EN', 'Introduction', 'Introduction_CN', 'Introduction_EN', 'VideoDescription', 'VideoDescription_CN', 'VideoDescription_EN', 'MissionAndVision_TW', 'MissionAndVision_CN', 'MissionAndVision_EN'],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            if (pargs.action === 'upd') {
                $('#OrgID').prop('disabled', true);
                pargs._getOne().done(function (res) {
                    pargs.data.LoGoId = pargs.data.LoGoId || guid();
                    pargs.data.BackgroundImage = pargs.data.BackgroundImage || guid();
                    pargs.data.WebsiteLgoId = pargs.data.WebsiteLgoId || guid();
                    pargs.data.PicShowId = pargs.data.PicShowId || guid();
                    pargs.data.WebsiteLgoId_CN = pargs.data.WebsiteLgoId_CN || guid();
                    pargs.data.PicShowId_CN = pargs.data.PicShowId_CN || guid();
                    pargs.data.WebsiteLgoId_EN = pargs.data.WebsiteLgoId_EN || guid();
                    pargs.data.PicShowId_EN = pargs.data.PicShowId_EN || guid();

                    fnGetUploadFiles(pargs.data.LoGoId, fnUpload, 'fileInput1');
                    fnGetUploadFiles(pargs.data.BackgroundImage, fnUpload, 'fileInput2');
                    fnGetUploadFiles(pargs.data.WebsiteLgoId, fnUpload, 'fileInput3');
                    fnGetUploadFiles(pargs.data.PicShowId, fnUpload, 'fileInput4');
                    fnGetUploadFiles(pargs.data.WebsiteLgoId_EN, fnUpload, 'fileInput3_EN');
                    fnGetUploadFiles(pargs.data.PicShowId_EN, fnUpload, 'fileInput4_EN');
                    fnGetUploadFiles(pargs.data.WebsiteLgoId_CN, fnUpload, 'fileInput3_CN');
                    fnGetUploadFiles(pargs.data.PicShowId_CN, fnUpload, 'fileInput4_CN');
                });
            }
            else {
                pargs.data.LoGoId = guid();
                pargs.data.BackgroundImage = guid();
                pargs.data.WebsiteLgoId = guid();
                pargs.data.PicShowId = guid();
                pargs.data.WebsiteLgoId_CN = guid();
                pargs.data.PicShowId_CN = guid();
                pargs.data.WebsiteLgoId_EN = guid();
                pargs.data.PicShowId_EN = guid();
                fnUpload(null, pargs.data.LoGoId, 'fileInput1');
                fnUpload(null, pargs.data.BackgroundImage, 'fileInput2');
                fnUpload(null, pargs.data.WebsiteLgoId, 'fileInput3');
                fnUpload(null, pargs.data.PicShowId, 'fileInput4');
                fnUpload(null, pargs.data.WebsiteLgoId_EN, 'fileInput3_EN');
                fnUpload(null, pargs.data.PicShowId_EN, 'fileInput4_EN');
                fnUpload(null, pargs.data.WebsiteLgoId_CN, 'fileInput3_CN');
                fnUpload(null, pargs.data.PicShowId_CN, 'fileInput4_CN');
            }
        }
    }),
        /**
         * 上傳附件
         * @param {Array} files 當前文件
         * @param {String} parentid
         * @param {String} inputid file input id
         */
        fnUpload = function (files, parentid, inputid) {
            var option = {};
            switch (inputid) {
                case 'fileInput1':
                    option.limit = 1;
                    option.type = 'one';
                    option.theme = 'dragdropbox1';
                    break;
                case 'fileInput2':
                    option.limit = 1;
                    option.type = 'one';
                    option.theme = 'dragdropbox2';
                    break;
                case 'fileInput3':
                case 'fileInput3_EN':
                case 'fileInput3_CN':
                    option.limit = 1;
                    option.type = 'one';
                    option.theme = inputid;
                    break;
                case 'fileInput4':
                case 'fileInput4_EN':
                case 'fileInput4_CN':
                    option.limit = 99;
                    option.theme = 'dragdropbox4';
                    break;
            }
            option.input = $('#' + inputid);
            option.parentid = parentid;
            option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png', 'svg'];
            option.folder = 'Organization';
            if (files) {
                option.files = files;
            }
            fnUploadRegister(option);
        };
};

require(['base', 'filer', 'cando'], fnPageInit);