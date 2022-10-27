$(function () {
    'use strict';

    var sIP = '',
        oValidator = null,
        /**
         * 設置組織下拉單
         * @return {Object} ajax物件
         */
        fnSetOrgIDDrop = function () {
            return CallAjax(ComFn.W_Web, 'GetOrgs', {}, function (res) {
                var res = $.parseJSON(res.d);
                if (res.RESULT === 0) {
                    alert(res.MSG);
                }
                else {
                    var sOptionHtml = createOptions(res.DATA, 'OrgID', 'OrgName', true);
                    $('#OrgID').html(sOptionHtml).find('option:first').remove();
                }
            });
        },
        /**
         * 登陸
         * @param {String} flag  驗證提示
         */
        fnLogin = function (flag) {
            var oData = {};
            g_api.ConnectLite(Service.auth, 'Login', {
                url: g_gd.webapilonginurl,
                OrgID: $('#OrgID').val(),
                UserID: $('#UserId').val(),
                Pwd: $('#Password').val(),
                Outklook: $('#Outklook')[0].checked,
                IP: sIP,
                Relogin: flag || false
            }, function (res) {
                if (res.RESULT) {
                    var oAuth = res.DATA.rel;
                    g_db.SetItem('orgid', oAuth.orgid);
                    g_db.SetItem('userid', oAuth.userid);
                    g_db.SetItem('loginname', oAuth.loginname);
                    g_db.SetItem('usertype', oAuth.usertype);
                    g_db.SetItem('mode', oAuth.mode);
                    g_db.SetItem('outklook', oAuth.outklook);
                    g_db.SetItem('outklooksync', 0);
                    g_db.SetItem('outlooktips', 0);
                    g_ul.SetToken(oAuth.token);
                    if (oAuth.outklook) {
                        window.location.href = "/Login/Index?orgid=" + oAuth.orgid + "&userid=" + oAuth.userid;
                    }
                    else {
                        window.location.href = '/Page/MainPage.html';
                    }
                }
                else {
                    if (res.MSG.indexOf('Tips：') > -1) {
                        layer.confirm(res.MSG,
                            {
                                icon: 3,
                                title: '提示',
                                btn: ['是', '否']
                            },
                            function (index) {
                                fnLogin(true);
                                layer.close(index);
                            }
                        );
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }
            }, null, true, '登入中...');
        },
        /**
         * 初始化
         */
        init = function () {
            $.ajax({
                type: 'get',
                //jsonpCallback: "ipCallback",//callback的function名称
                //url: 'https://www.taobao.com/help/getip.php',
                url: 'https://api.ipify.org?format=json&callback=?',
                dataType: 'json',
                success: function (res) {
                    sIP = res.ip;
                    closeWaiting();
                }
            });
            oValidator = $("#formlogin").validate({ //表單欄位驗證
                rules: {
                    OrgID: 'required',
                    UserId: 'required',
                    Password: 'required'
                },
                messages: {
                    OrgID: '請選擇組織代號',
                    UserId: '請輸入帳號',
                    Password: '請輸入密碼'
                }
            });
            fnSetOrgIDDrop().done(function () {
                $('#btnLogin').click(function (e) {
                    if (!$("#formlogin").valid()) {
                        oValidator.focusInvalid();
                        return false;
                    }
                    fnLogin();
                }).focus();

                $(document).on('keydown', function (e) {
                    if (e.keyCode === 13) {
                        $("#btnLogin").click();
                    }
                });
            });
            var sHeight = $('body').height();
            $('#main-wrapper').css('min-height', sHeight - 350 + 'px');
        };
    init();
});