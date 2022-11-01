'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'DepartmentID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['DepartmentID'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("departmentidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            DepartmentID: value,
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
                    DepartmentID: { departmentidrule: pargs.action === 'Add' ? true : false },
                },
                messages: {
                    DepartmentID: { departmentidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                }
            };
        },
        /**
         * 處理新增資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getInsertParams: function (pargs, data) {
            data.NameOfLevel = '';
            data.LevelOfDepartment = !(data.ParentDepartmentID) ? 0 : data.ParentDepartmentID.split('-').length;
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
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            if (pargs.action === 'upd') {
                $('#DepartmentID').prop('disabled', true);
                postArray.push(pargs._getOne());
            }
            postArray.push(fnSetOrderByValueDrop(), fnSetOrderByValueDrop(), fnSetDeptDrop($('#ParentDepartmentID')), fnSetUserDrop([
                {
                    Select: $('#ChiefOfDepartmentID'),
                    ShowId: true,
                    Select2: true,
                    Action: pargs.action
                }
            ]));

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

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);