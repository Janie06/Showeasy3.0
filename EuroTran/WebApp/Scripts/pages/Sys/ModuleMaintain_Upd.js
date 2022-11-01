'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ModuleID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['ModuleID'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("moduleidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            ModuleID: value
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
                    ModuleID: { moduleidrule: pargs.action === 'add' ? true : false },
                },
                messages: {
                    ModuleID: { moduleidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                }
            };
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            if (pargs.action === 'upd') {
                $('#ModuleID').prop('disabled', true);
                postArray.push(pargs._getOne());
            }
            postArray.push(fnSetOrderByValueDrop(), fnSetParentIDDrop());

            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    var oRes = res[0].DATA.rel;
                    $('#BackgroundCSS').spectrum("set", oRes.BackgroundCSS);
                    pargs._setFormVal(oRes);
                    pargs._getPageVal();//緩存頁面值，用於清除
                }
            });
        }
    }),
        /**
         * 設定報關類別下拉選單
         */
        fnSetOrderByValueDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.getcout, {},
                function (res) {
                    if (res.RESULT) {
                        var iCount = res.DATA.rel;
                        if (canDo.action === 'add') {
                            iCount++;
                        }
                        $('#OrderByValue').html(createOptions(iCount));
                        if (canDo.action === 'add') {
                            $('#OrderByValue').val(iCount);
                        }
                    }
                });
        },
        /**
         * 設定上層模組下拉選單
         * @return {Object} Ajax 物件
         */
        fnSetParentIDDrop = function () {
            return g_api.ConnectLite(canDo._service.sys, 'GetModuleList', {},
                function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel;
                        $('#ParentID').html(createOptions(saData, 'ModuleID', 'AccountNameSort', true));
                    }
                });
        };
};

require(['base', 'jsgrid', 'spectrum', 'cando'], fnPageInit);