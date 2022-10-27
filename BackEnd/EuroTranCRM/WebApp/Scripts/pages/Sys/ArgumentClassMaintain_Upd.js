'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ArgumentClassID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['ArgumentClassID'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("argumentclassidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            ArgumentClassID: value
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
                    ArgumentClassID: { argumentclassidrule: pargs.action === 'add' ? true : false },
                },
                messages: {
                    ArgumentClassID: { argumentclassidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
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
                $('#ArgumentClassID').prop('disabled', true);
                postArray.push(pargs._getOne());
            }
            postArray.push(fnSetOrderByValueDrop());

            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    var oRes = res[0].DATA.rel;
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
        };
};

require(['base', 'cando'], fnPageInit);