$(function () {
    'use strict';

    var sCusCommentsEmail = 'john.yuan@origtek-mail.com',
        sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        eForm = $("#footer-form"),
        /*
        * 目的 表單驗證初始化
        */
        fnSetForm_Rule = function () {
            eForm.validate({
                errorPlacement: function (error, element) {
                    error.css({
                        "border": "none",
                        "color": "red"
                    });
                    var wrapper = $("<p></p>");
                    error.appendTo(wrapper)
                    $("#messageBox").append(wrapper);
                },
                highlight: function (element, errorClass) {
                    $(element).css({
                        "border": "1px solid red"
                    });
                },
                unhighlight: function (element, errorClass) {
                    $(element).css({
                        "border": ""
                    });
                },
                rules: {
                    uname: {
                        required: true
                    },
                    uemail: {
                        email: true,
                        required: true
                    },
                    utel: {
                        required: true
                    },
                    umailcontent: {
                        required: true
                    },
                    validcode: {
                        required: true
                    }
                },
                messages: {
                    uname: {
                        required: "請輸入聯絡人"
                    },
                    uemail: {
                        email: "電子信箱格式錯誤",
                        required: "請輸入電子信箱"
                    },
                    utel: {
                        required: "請輸入聯絡電話"
                    },
                    umailcontent: {
                        required: "請輸入內容"
                    },
                    validcode: {
                        required: "請輸入驗證碼"
                    }
                }
            });
        },
        /*
        * 目的 寄送郵件
        */
        fnSendMail = function () {
            if (!eForm.valid()) {
                return false;
            }
            var formdata = getFormSerialize(eForm);
            formdata.flag = 'cap2';
            formdata.type = 'C';

            return g_api.ConnectLite(Service.apiwebcom, 'SendMail', formdata, function (res) {
                if (res.RESULT) {
                    if (res.DATA.rel) {
                        showMsg(i18next.t("message.CusCommentsEmailSuccess"), 'success'); // 郵件已經寄出，我們將儘快為您處理。
                        setTimeout(function () {
                            location.reload();
                        }, 3000);
                    }
                    else {
                        showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // 郵件寄送失敗
                        $('#change_pic').click();
                    }
                }
                else {
                    showMsg(res.MSG, 'error'); // 驗證碼錯誤
                    $('#change_pic').click();
                }
            }, function () {
                showMsg(i18next.t("message.SendEmail_Failed"), 'error'); // 郵件寄送失敗
            }, null, i18next.t('message.Dataprocessing'));
        },
        init = function () {
            fnSetForm_Rule();

            $("#sendmail").on("click", function () {
                fnSendMail();
                return false;
            });

            $('#change_pic').on('click', function () {
                $('#imgBtnCaptcha').attr('src', $('#imgBtnCaptcha').attr('src') + '?' + Math.random());
            });
            setLang(sLang);
        };

    init();
});