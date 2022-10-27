'use strict';
var fnPageInit = function () {
    var sAction = getUrlParam('Action') || 'Add',
        canDo = new CanDo({
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['OrgID', 'Guid'],
            /**
             * 當前程式所有參數名稱集合
             */
            //paramKeys: ['ArgumentClassID', 'ArgumentID'],
            /**
             * 新增資料
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前新增的資料
             * @param {String} flag 新增 or 儲存后新增
             */
            getInsertBack: function (pargs, data, flag) {
                if (flag == 'add') {
                    showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Upd&Guid=' + data.guid); // ╠message.Save_Success⇒新增成功╣
                }
                else {
                    showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                }
            },
            /**
             * 修改資料
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前修改的資料
             */
            getUpdateBack: function (pargs, data) {
                if (typeof pargs.data.Contactors === 'string') {
                    pargs.data.Contactors = $.parseJSON(pargs.data.Contactors || '[]');
                }
                showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                if (window.bLeavePage) {
                    setTimeout(function () {
                        pargs._pageLeave();
                    }, 1000);
                }
                else {
                    pargs._setFormVal(data);
                    //location.reload();
                }
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
                    //$('#ArgumentClassID,#ArgumentID').prop('disabled', true);
                    postArray.push(pargs._getOne());
                }
                postArray.push(
                    fnSetUserDrop([{
                        Select: $('#MemberID'),
                        ShowId: true,
                        Action: sAction,
                        //Select2: true,
                    }]),
                    fnSetArgDrop([{
                        ArgClassID: 'LeaveType',
                        Select: $('#Leave'),
                        ShowId: true,
                        CallBack: function (data) {
                            data = Enumerable.From(data).Where(function (val) { return val.Correlation.trim() !== ''; }).ToArray();
                            $('#Leave').html(createOptions(data, 'id', 'text', true))
                                .on('change', function () {
                                    if ($('#Leave').OnChange && typeof $('#Leave').OnChange === 'function') {
                                        $('#Leave').OnChange(this.value);
                                    }
                                });
                        }

                    }])
                );

                //加載報關類別,加載報價頁簽,加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
                $.whenArray(postArray).done(function (res) {
                    $('#Leave').find("[value='09']").remove();
                    if (pargs.action === 'upd' && res[0].RESULT) {
                        var oRes = res[0].DATA.rel;
                        oRes.CreateUserName = oRes.CreateUser;
                        oRes.ModifyUserName = oRes.ModifyUser;
                        pargs._setFormVal(oRes);
                        $('#UsedHours').val(oRes.UsedHours)
                        $('#RemainHours').val(oRes.RemainHours)
                        $('#PaymentHours').attr('readonly',true);
                        $('.UpdLeaveHoursArea').show();
                    }
                    else {
                        $('.UpdLeaveHoursArea').hide();
                    }
                });
            },

        });
};
require(['base', 'select2', 'jsgrid', 'filer', 'timepicker', 'common_eip', 'cando'], fnPageInit, 'timepicker');

