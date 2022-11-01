'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'EventID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['EventID'],
        /**
         * 須初始化的UEEditer 的物件ID集合
         */
        ueEditorIds: ['TaskDescription', 'TaskReward'],
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
            data.EventNo = '';
            data.SourceFrom = 'Task_Qry';
            data.Params = '?Action=Upd&EventID=' + pargs.data.EventID;
            if (!data.AlertTime) delete data.AlertTime;
            if (!data.StartDate) delete data.StartDate;
            if (!data.EndDate) delete data.EndDate;
            return data;
        },
        /**
         * 處理修改資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getUpdateParams: function (pargs, data) {
            data = pargs.options.getInsertParams(pargs, data);
            var sTaskReward = pargs.UE_Editor.TaskReward.getPlainTxt();
            if ($.trim(sTaskReward)) {
                data.ReplyStatus = $('#Status option:checked').text();
                data.ReplyContent = sTaskReward;
            }
            delete data.SourceFrom;
            delete data.Params;
            return data;
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            if (pargs.action === 'upd') {
                postArray.push(pargs._getOne());
            }
            else {
                $('#StartDate').val(newDate());
                $('#EndDate').val(newDate(new Date().dateAdd('h', 1)));
                pargs.data.EventID = guid();
                pargs.UE_Editor.TaskReward.ready(function () {
                    pargs.UE_Editor.TaskReward.disable();
                });
                $('#CreateUser').text(parent.UserInfo.MemberName);
                fnUpload();
            }
            postArray.push(fnSetUserDrop([{
                Select: $('#Owner'),
                Action: pargs.action,
                Select2: true,
                ShowId: true
            }]));

            //加載報關類別,加載報價頁簽,加載運輸方式, 加載機場, 加載貨棧場, 加載倉庫
            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    var saTaskReply = res[0].DATA.taskreply;
                    $('#CreateUser').text(pargs.data.ExFeild1);
                    pargs._setFormVal(pargs.data);
                    fnGetUploadFiles(pargs.data.EventID, fnUpload);
                    if (pargs.data.EventNo) {
                        $('#Status,#StartDate,#EventName,#AlertTime').prop('disabled', true);
                    }
                    fnSetUEditor();
                    $.each(saTaskReply, function (indx, task) {
                        task.RowIndex = indx + 1;
                        task.ReplyDate = newDate(task.ReplyDate);
                    });
                    var sHtml = $("#Task_temp").render({ List: saTaskReply });
                    $('#ReplyData').html(sHtml);
                }
            });

            $('#Status').change(function () {
                var sVal = this.value;
                if (sVal == 'D' || sVal == 'O') {
                    $('#Progress').val('100');
                }
                else {
                    $('#Progress').val(oCurData.PreProgress);
                }
            });
        }
    }),
        /**
         * 設置富文本框
         */
        fnSetUEditor = function () {
            //判斷當前登入者是否為超級權限決定是否禁用富文本框
            if (parent.UserInfo.roles.indexOf(parent.SysSet.Supervisor) === -1) {
                pargs.UE_Editor.UE_TaskDescription.ready(function () {
                    if (canDo.data.CreatUser !== parent.UserID) {
                        pargs.UE_Editor.UE_TaskDescription.disable();
                    }
                });

                pargs.UE_Editor.TaskReward.ready(function () {
                    if (canDo.data.CreatUser !== parent.UserID && canDo.data.Owner !== parent.UserID) {
                        pargs.UE_Editor.TaskReward.disable();
                    }
                });
            }
        },
        /**
         * 上傳附件
         * @param {Array} files 上傳的文件
         */
        fnUpload = function (files) {
            var option = {};
            option.input = $('#fileInput');
            option.theme = 'dragdropbox';
            option.folder = 'Task';
            option.type = 'list';
            option.parentid = canDo.data.EventID;
            if (files) {
                option.files = files;
            }
            fnUploadRegister(option);
        };
};

require(['base', 'select2', 'timepicker', 'filer', 'cando'], fnPageInit, 'timepicker');