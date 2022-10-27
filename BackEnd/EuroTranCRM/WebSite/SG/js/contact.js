$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh',
        bEn = sLang === 'en',
        eForm = $("#form_contact"),
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
                    error.appendTo(wrapper);
                    $(element).parent().after(wrapper);
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
                },
                messages: {
                    uname: {
                        required: bEn ? 'Please enter contact name' : "请输入联络人姓名"
                    },
                    uemail: {
                        email: bEn ? 'Wrong email format' : "电子信箱格式错误",
                        required: bEn ? 'Please enter your email address' : "请输入电子信箱"
                    },
                    ucomp: {
                        required: bEn ? 'Please enter the company name' : "请输入公司名称"
                    },
                    utel: {
                        required: bEn ? 'Please enter your contact number' : "请输入联络电话"
                    },
                    umailcontent: {
                        required: bEn ? 'Please enter the content' : "请输入内容"
                    },
                    validcode: {
                        required: bEn ? 'Please enter the verification code' : "请输入验证码"
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
            formdata.flag = 'cap3';
            formdata.type = 'C';

            return g_api.ConnectLite(Service.apiwebcom, 'SendMail', formdata, function (res) {
                if (res.RESULT) {
                    if (res.DATA.rel) {
                        showMsg(bEn ? 'The mail has been sent out and we will deal with it for you as soon as possible' : '邮件已经寄出，我们将尽快为您处理', 'success'); // 郵件已經寄出，我們將儘快為您處理。
                        setTimeout(function () {
                            location.reload();
                        }, 3000);
                    }
                    else {
                        showMsg(bEn ? 'Please enter contact name' : '邮件寄送失败', 'error'); // 郵件寄送失敗
                        $('#change_validcode').click();
                    }
                }
                else {
                    showMsg(res.MSG, 'error'); // 驗證碼錯誤
                    $('#change_validcode').click();
                }
            }, function () {
                showMsg(bEn ? 'Mail delivery failed' : '邮件寄送失败', 'error'); // 郵件寄送失敗
            }, null, bEn ? 'In the mail...' : '邮件寄送中...');
        },
        init = function () {
            $('#change_validcode,.change_validcode').on('click', function () {
                var url = $('#imgvalidcode').data('url') || $('#imgvalidcode').attr('src');
                $('#imgvalidcode').attr('src', url + '&' + Math.random()).data('url', url);
            });

            fnSetForm_Rule();

            $("#sendmail").on("click", function () {
                fnSendMail();
            });
        };

    init();
});