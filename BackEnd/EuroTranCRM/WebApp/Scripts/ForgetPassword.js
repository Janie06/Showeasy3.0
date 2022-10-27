$(function () {
    'use strict';

    var $Validator = null,
        iValidatTime = 60,
        /**
         * 設置組織下拉單
             * @return {Object} Ajax 物件
         */
        fnSetOrgIDDrop = function () {
            return CallAjax(ComFn.W_Web, 'GetOrgs', {}, function (res) {
                var saList = $.parseJSON(res.d);
                var sOptionHtml = createOptions(saList.DATA, 'OrgID', 'OrgName', true);
                $('#OrgID').html(sOptionHtml).find('option:first').remove();
            });
        },
        fnRefreshTime = function () {
            var testTime = function () {
            };
            if (iValidatTime > 0) {
                setTimeout(function () {
                    fnRefreshTime();
                }, 1000);
                iValidatTime--;
                $('#basic-addon2').attr('disabled', true).text(iValidatTime + 'S后失效').css('color', 'black');
            }
            else {
                iValidatTime = 60;
                $('#basic-addon2').removeAttr('disabled').text('重新產生驗證碼');
            }
        };

    fnSetOrgIDDrop().done(function () {
        //表單欄位驗證
        $Validator = $("#formforgetpassword").validate({
            rules: {
                txtUserId: { required: true },
                txtVerificationCode: { required: true },
                txtNewPassword: { required: true },
                txtCheckPassword: { required: true, equalTo: "#txtNewPassword" }
            },
            messages: {
                txtUserId: { required: '請輸入帳號/郵箱' },
                txtVerificationCode: { required: '請輸入驗證碼' },
                txtNewPassword: { required: '請輸入新密碼' },
                txtCheckPassword: { required: '請輸入確認新密碼', equalTo: "兩次密碼輸入不相符" }
            }
        });

        $('[name="btnVerificationCode"]').click(function (e) {
            if ($(this).attr('disabled')) {
                return;
            }
            if (!$("#txtUserId").valid()) {
                $Validator.focusInvalid();
                return false;
            }
            g_api.ConnectLite(Service.auth, 'CheckMember', {
                url: g_gd.webapilonginurl,
                OrgID: $('#OrgID').val(),
                UserID: $('#txtUserId').val()
            }, function (res) {
                if (res.RESULT) {
                    $('.newpwd').show();
                    $('#btnSent').removeAttr('disabled');
                    showMsg('驗證碼已成功寄送，請到郵箱收取', 'success');
                    fnRefreshTime();
                }
                else {
                    if (res.MSG === "1") {
                        showMsg('沒有此會員帳號，請確認輸入是否正確。', 'error');
                    }
                    else if (res.MSG === "2") {
                        showMsg('產生驗證碼失敗', 'error');
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }
            });
        });

        $('#btnSent').click(function (e) {
            if (!$("#formforgetpassword").valid()) {
                $Validator.focusInvalid();
                return false;
            }

            g_api.ConnectLite(Service.auth, 'ReSetPassword', {
                url: g_gd.webapilonginurl,
                OrgID: $('#OrgID').val(),
                UserID: $('#txtUserId').val(),
                VerificationCode: $('#txtVerificationCode').val(),
                NewPsw: $('#txtNewPassword').val()
            }, function (res) {
                if (res.RESULT) {
                    showMsg('新密碼更新成功', 'success');
                    setTimeout(function () {
                        window.location.href = '/Page/Login.html';
                    }, 1500);
                }
                else {
                    if (res.MSG === "0") {
                        showMsg('驗證碼錯誤', 'error');
                    }
                    else if (res.MSG === "1") {
                        showMsg('輸入的帳號有誤', 'error');
                    }
                    else if (res.MSG === "2") {
                        showMsg('驗證碼已失效，請重新取得驗證碼', 'error');
                    }
                    else if (res.MSG === "3") {
                        showMsg('新密碼更新失敗', 'error');
                    }
                    else if (res.MSG === "4") {
                        showMsg('驗證碼錯誤或驗證碼已失效', 'error');
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }
            }, function () {
                showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
            });
        });
    });
});