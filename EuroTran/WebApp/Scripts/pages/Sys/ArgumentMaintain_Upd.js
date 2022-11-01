'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'ArgumentClassID', 'ArgumentID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['ArgumentClassID', 'ArgumentID'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("argumentidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            ArgumentClassID: $('#ArgumentClassID').val(),
                            ArgumentID: value
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
        validRules: {
            onfocusout: false,
            rules: {
                ArgumentID: { argumentidrule: true },
            },
            messages: {
                ArgumentID: { argumentidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
            }
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            if (pargs.action === 'upd') {
                $('#ArgumentClassID,#ArgumentID').prop('disabled', true);
                postArray.push(pargs._getOne());
            }
            postArray.push(fnSetArgumentClassIDDrop(), fnSetOrderByValueDrop());

            //加載報關類別,加載報價頁簽,加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    var oRes = res[0].DATA.rel;
                    pargs._setFormVal(oRes);
                }
                fnSetParentArgumentDrop().done(function () {
                    pargs._getPageVal();//緩存頁面值，用於清除
                });
            });
            $('#ArgumentValue').on('blur', function () {
                $('#ArgumentValue_CN').val(simplized(this.value));
            });
        }
    }),
        /**
         * 設定參數類別下拉選單
         * @return {Object} Ajax 物件
         */
        fnSetArgumentClassIDDrop = function () {
            return g_api.ConnectLite(canDo.QueryPrgId, canDo._api.getlist, {},
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $('#ArgumentClassID').html(createOptions(saList, 'ArgumentClassID', 'ArgumentClassName', true))
                            .on('change', function () {
                                fnSetOrderByValueDrop();
                            });
                    }
                });
        },
        /**
         * 設定報關類別下拉選單
         * @return {Object} Ajax 物件
         */
        fnSetOrderByValueDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.getcout,
                {
                    ArgumentClassID: $('#ArgumentClassID').val()
                },
                function (res) {
                    if (res.RESULT) {
                        var iOrderByCount = res.DATA.rel;
                        if (canDo.action === 'add') {
                            iOrderByCount++;
                        }
                        $('#OrderByValue').html(createOptions(iOrderByCount));
                        if (canDo.action === 'add') {
                            $('#OrderByValue').val(iOrderByCount);
                        }
                    }
                });
        },
        /**
         * 獲取父層下拉資料
         * @return {Object} Ajax 物件
         */
        fnSetParentArgumentDrop = function () {
            var sArgumentClassID = $('#ArgumentClassID').val();
            if (sArgumentClassID) {
                return fnSetArgDrop([
                    {
                        ArgClassID: sArgumentClassID,
                        Select: $('#ParentArgument'),
                        ShowId: true,
                        Select2: true,
                        CallBack: function (data) {
                            if (canDo.data.ParentArgument) {
                                $('#ParentArgument').val(canDo.data.ParentArgument).trigger('change');
                            }
                        }
                    }
                ]);
            }
            else {
                return $.Deferred().resolve().promise();
            }
        };
};

require(['base', 'select2', 'jsgrid', 'convetlng', 'cando'], fnPageInit);