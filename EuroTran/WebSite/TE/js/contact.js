$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        sContactType = getUrlParam('T') || '',
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
                        required: { 'zh-TW': '請輸入聯絡人姓名', 'zh': '请输入联络人姓名', 'en': 'Please enter contact name' }[sLang]
                    },
                    uemail: {
                        email: { 'zh-TW': '電子信箱格式錯誤', 'zh': '电子信箱格式错误', 'en': 'Wrong email format' }[sLang],
                        required: { 'zh-TW': '請輸入電子信箱', 'zh': '请输入电子信箱', 'en': 'Please enter your email address' }[sLang]
                    },
                    ucomp: {
                        required: { 'zh-TW': '請輸入公司名稱', 'zh': '请输入公司名称', 'en': 'Please enter the company name' }[sLang]
                    },
                    utel: {
                        required: { 'zh-TW': '請輸入聯絡電話', 'zh': '请输入联络电话', 'en': 'Please enter your contact number' }[sLang]
                    },
                    umailcontent: {
                        required: { 'zh-TW': '請輸入內容', 'zh': '请输入内容', 'en': 'Please enter the content' }[sLang]
                    },
                    validcode: {
                        required: { 'zh-TW': '請輸入驗證碼', 'zh': '请输入验证码', 'en': 'Please enter the verification code' }[sLang]
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
            formdata.flag = 'cap1';

            return g_api.ConnectLite(Service.apiwebcom, 'SendMail', formdata, function (res) {
                if (res.RESULT) {
                    if (res.DATA.rel) {
                        showMsg({ 'zh-TW': '郵件已經寄出，我們將儘快為您處理', 'zh': '邮件已经寄出，我们将尽快为您处理', 'en': 'The mail has been sent out and we will deal with it for you as soon as possible' }[sLang], 'success'); // 郵件已經寄出，我們將儘快為您處理。
                        setTimeout(function () {
                            location.reload();
                        }, 3000);
                    }
                    else {
                        showMsg({ 'zh-TW': '郵件寄送失敗', 'zh': '邮件寄送失败', 'en': 'Please enter contact name' }[sLang], 'error'); // 郵件寄送失敗
                        $('#change_validcode').click();
                    }
                }
                else {
                    showMsg(res.MSG, 'error'); // 驗證碼錯誤
                    $('#change_validcode').click();
                }
            }, function () {
                showMsg({ 'zh-TW': '郵件寄送失敗', 'zh': '邮件寄送失败', 'en': 'Mail delivery failed' }[sLang], 'error'); // 郵件寄送失敗
            }, null, { 'zh-TW': '郵件寄送中...', 'zh': '邮件寄送中...', 'en': 'In the mail...' }[sLang]);
        },
        init = function () {
            if (sContactType) { $('#type').val(sContactType); }

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