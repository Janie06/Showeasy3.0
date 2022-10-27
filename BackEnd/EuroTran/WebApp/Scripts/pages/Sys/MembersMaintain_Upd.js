'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'MemberID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['MemberID'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("compardate", function (value, element, parms) {
                if (new Date(value) < new Date($('#ArriveDate').val())) {
                    return false;
                }
                return true;
            });
            $.validator.addMethod("emailequ", function (value) {
                var bRetn = true;
                if ($.trim(value)) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            _MemberID: pargs.data.MemberID,
                            Email: value
                        },
                        function (res) {
                            if (res.RESULT && res.DATA.rel > 0) {
                                bRetn = false;
                            }
                        }, null, false);
                }
                return bRetn;
            });
            $.validator.addMethod("outlookequ", function (value) {
                var bRetn = true;
                if ($.trim(value)) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            _MemberID: pargs.data.MemberID,
                            OutlookAccount: value
                        },
                        function (res) {
                            if (res.RESULT && res.DATA.rel > 0) {
                                bRetn = false;
                            }
                        }, null, false);
                }
                return bRetn;
            });
            $.validator.addMethod("memberidrule", function (value) {
                var bRetn = true;
                if (value) {
                    g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                        {
                            MemberID: value
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
                    Email: {
                        required: true,
                        email: true,
                        emailequ: ''
                    },
                    OutlookAccount: {
                        outlookequ: ''
                    },
                    EmergencyEMail: { email: true },
                    MemberID: { memberidrule: pargs.action === 'add' ? true : false }
                }, messages: {
                    Email: {
                        required: i18next.t("common.Email_required"),// ╠message.Email_required⇒請輸入組織郵箱╣
                        email: i18next.t("message.IncorrectEmail"),// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                        emailequ: i18next.t("message.EmailIsExist")// ╠message.EmailIsExist⇒郵箱已存在╣
                    },
                    OutlookAccount: {
                        outlookequ: i18next.t("message.OutlookAccountExist")// ╠message.OutlookAccountExist⇒Outlook帳號已存在╣
                    },
                    EmergencyEMail: {
                        email: i18next.t("message.IncorrectEmail")// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    },
                    MemberID: { memberidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                }
            };
        },
        /**
         * 處理新增資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getInsertParams: function (pargs, data) {
            data.NetworkLogin = data.NetworkLogin || false;
            data.IsAttendance = data.IsAttendance || false;
            data.Password = $.trim(parent.SysSet['DefaultPassword'] || '123456');
            data.MemberPic = pargs.data.MemberPic;
            data.roles = $('#Roles').val();
            if (!data.BirthDate) delete data.BirthDate;
            if (!data.ArriveDate) delete data.ArriveDate;
            if (!data.LeaveDate) delete data.LeaveDate;
            return data;
        },
        /**
         * 處理修改資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getUpdateParams: function (pargs, data) {
            data = pargs.options.getInsertParams(pargs, data);
            delete data.Password;
            return data;
        },
        /**
         * 新增資料
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前新增的資料
         * @param {String} flag 新增 or 儲存后新增
         */
        getInsertBack: function (pargs, data, flag) {
            var fnCallBack = function () {
                if (flag === 'add') {
                    showMsgAndGo(i18next.t("message.Insert_Success"), pargs.QueryPrgId); // ╠message.Insert_Success⇒新增成功╣
                }
                else {
                    showMsgAndGo(i18next.t("message.Insert_Success"), pargs.ProgramId, '?Action=Add'); // ╠message.Insert_Success⇒新增成功╣
                }
            };

            layer.confirm(i18next.t('message.DefaultPassword').replace('{initpswd}', parent.SysSet['DefaultPassword'] || 123456), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                if (parent.SysSet.IsOpenMail == 'Y') {
                    fnSendEmailToMember(flag);
                }
                else {
                    layer.alert(i18next.t('message.NotOpenMail'), { icon: 0 }, function () {// ╠message.NotOpenMail⇒系統沒有開放郵件發送功能，請聯絡管理員！╣
                        fnCallBack();
                    });
                }
                layer.close(index);
            }, function () {
                fnCallBack();
            });
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            postArray.push(fnSetJobtitleDrop(), fnSetRolesDrop(), fnSetDeptDrop($('#DepartmentID')),
                fnSetUserDrop([
                    {
                        Select: $('#ImmediateSupervisor'),
                        ShowId: true,
                        Select2: true,
                        Action: pargs.action
                    }
                ]),
                fnSetArgDrop([
                    {
                        ArgClassID: 'JobClass',
                        Select: $('#JobClass'),
                        ShowId: true
                    },
                    {
                        ArgClassID: 'LanCountry',
                        Select: $('#Country'),
                        ShowId: true
                    }
                ]));
            if (pargs.action === 'upd') {
                $('#MemberID').prop('disabled', true);
                postArray.push(pargs._getOne());
            }
            else {
                pargs.data.MemberPic = guid();
                fnUpload();
            }
            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    pargs._setFormVal(pargs.data);
                    pargs.data.MemberPic = $.trim(pargs.data.MemberPic) === '' ? guid() : pargs.data.MemberPic;
                    pargs.data.Roles = (pargs.data.RuleIDs || '').split(',').clear();
                    $('#Roles').val(pargs.data.Roles).trigger('change');
                    $('#CalColor').spectrum("set", pargs.data.CalColor);
                    pargs._getPageVal();//緩存頁面值，用於清除
                    fnGetUploadFiles(pargs.data.MemberPic, fnUpload);
                }
            });

            $('#LeaveDate').on('blur', function () {
                if (this.value != '') {
                    $("[name='Effective'][value='N']").click();
                }
                else {
                    $("[name='Effective'][value='Y']").click();
                }
            });
            $('#IsAttendance').on('click', function () {
                if (this.checked) {
                    $(".CardId").show();
                    $("#CardId").attr('required', true);
                }
                else {
                    $(".CardId").hide();
                    $("#CardId").removeAttr('required');
                }
            });
        }
    }),
        /**
         * 寄送初始密碼給新增人員
         * @param {String} flag 新增 or 儲存后新增
         */
        fnSendEmailToMember = function () {
            g_api.ConnectLite(canDo._service.auth, 'SendPswToNewMember', {
                UserID: $('#MemberID').val(),
                OrgID: parent.OrgID
            }, function (res) {
                if (res.RESULT) {
                    if (res.DATA.rel) {
                        if (flag == 'add') {
                            showMsgAndGo(i18next.t("message.SendEmail_Success"), canDo.QueryPrgId); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.SendEmail_Success"), canDo.ProgramId); // ╠message.Save_Success⇒新增成功╣
                        }
                    }
                    else {
                        if (flag == 'add') {
                            showMsgAndGo(i18next.t("message.SendEmail_Failed"), canDo.QueryPrgId); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.SendEmail_Failed"), canDo.ProgramId); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                        }
                    }
                }
                else {
                    if (flag == 'add') {
                        showMsgAndGo(i18next.t("message.SendEmail_Failed"), canDo.QueryPrgId); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                    }
                    else {
                        showMsgAndGo(i18next.t("message.SendEmail_Failed"), canDo.ProgramId); // ╠message.SendEmail_Failed⇒郵件寄送失敗╣
                    }
                }
            });
        },
        /**
         * 設定幣別下拉選單
         * @return {Object} Ajax 物件
         */
        fnSetJobtitleDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, 'GetJobTitleDrop', { Effective: 'Y' },
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $('#JobTitle').html(createOptions(saList, 'JobtitleID', 'JobtitleName', true));
                    }
                });
        },
        /**
         * 設定角色下拉選單
         * @return {Object} Ajax 物件
         */
        fnSetRolesDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, 'GetRolesDrop', { Effective: 'Y' },
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $('#Roles').html(createOptions(saList, 'RuleID', 'RuleName')).select2();
                    }
                });
        },
        /**
         * 上傳附件
         * @param {Array} files 上傳的文件
         */
        fnUpload = function (files) {
            var option = {};
            option.input = $('#fileInput');
            option.limit = 1;
            //option.changeInput = '<div class="jFiler-input-dragDrop"><img id="ImgMemberPic" width="150" height="150" src="../../images/noImage.jpg"></div>';
            option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
            option.theme = 'dragdropbox';
            option.folder = 'Members';
            option.type = 'one';
            option.parentid = canDo.data.MemberPic;
            if (files) {
                option.files = files;
            }
            fnUploadRegister(option);
        };
};

require(['base', 'select2', 'jsgrid', 'spectrum', 'filer', 'cando'], fnPageInit);