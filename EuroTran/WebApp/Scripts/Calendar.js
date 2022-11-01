'use strict';
var sProgramId = getProgramId(),
    isAdmin = parent.UserInfo.roles.toLowerCase().indexOf('admin') > -1,
    AddedTip = false,
    oCalendar = null,
    fnPageInit = function() {
        var saCalType = [],
            saOpenMent = ['P', 'G', 'D', 'C'],
            /**
             * 表單驗證
             * @param {Object}iframe  form表單
             */
            fnValidate = function(iframe) {
                iframe.find('[name=OpenMent]').click(function() {
                    var sVal = this.value;
                    if (sVal === 'G') {
                        iframe.find('#trGroupMembers').show();
                    } else {
                        iframe.find('#trGroupMembers').hide();
                        iframe.find('#GroupMembers').val('').data('value', '');
                    }
                });
                $.validator.addMethod("compardatetime", function(value, element, parms) {
                    if (new Date(value) <= new Date(iframe.find('#StartDate').val())) {
                        return false;
                    }
                    return true;
                });
                $.validator.addMethod("groupmember", function(value, element, parms) {
                    if (iframe.find('[name=OpenMent]:checked').val() === 'G' && !$(element).data('value')) {
                        return false;
                    }
                    return true;
                });
                return iframe.find("#form_calendar").validate({
                    showErrors: function() {
                        this.defaultShowErrors();
                        transLang(iframe);
                    }
                }); // //表單欄位驗證
            },
            /**
             * 修改資料
             * @param {Object} event 事件
             * @param {Object} newevent 最新的事件
             */
            fnUpdData = function(event, newevent) {
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        calendar: {
                            values: newevent,
                            keys: { NO: event.id }
                        }
                    }
                }, function(res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                    } else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error'); //╠message.Modify_Failed⇒修改失敗╣
                    }

                    oCalendar.fullCalendar('refetchEvents');
                    if (parent.Outklook) {
                        outlookAPI(outlook.Calendar_Upd, { NO: event.id })
                    }
                }, function() {
                    showMsg(i18next.t("message.Modify_Failed"), 'error'); //╠message.Modify_Failed⇒修改失敗╣
                });
            },
            /**
             * pop視窗
             * @param {Object} iframe 表單
             */
            fnRegisterEvent = function(iframe, action) {
                iframe.find('.add-on').click(function() {
                    fnGetSetUserGroups({
                        Action: action,
                        Callback: function(data) {
                            if (data.length > 0) {
                                var saId = [],
                                    saName = [];
                                $.each(data, function(idx, item) {
                                    saId.push(item.UserID);
                                    saName.push(item.UserName);
                                });
                                iframe.find('#GroupMembers').data('value', saId.join(',')).val(saName.join(','));
                            }
                        }
                    });
                });
            },
            /**
             * pop資料新增
             * @param {Object} start 開始時間
             * @param {Object} end 結束時間
             */
            fnAdd = function(start, end) {
                var oValidator = null;
                layer.open({
                    type: 2,
                    title: '新增行程',
                    shade: 0.75,
                    maxmin: true, //开启最大化最小化按钮
                    area: ['800px', '580px'],
                    content: '/Page/Pop/CalanderPop.html',
                    success: function(layero, index) {
                        var iframe = $('iframe').contents();
                        fnSetArgDrop([{
                            ArgClassID: 'CalType',
                            Select: iframe.find('#CalType'),
                            ShowId: true,
                            CallBack: function(data) {
                                var bAllDay = true;
                                iframe.find('#CurrentDate').val(start);
                                iframe.find('#AllDay')[0].checked = bAllDay = typeof start._i === 'string';
                                iframe.find('#StartDate').val(bAllDay ? start._d.formate('yyyy/MM/dd 00:00') : newDate(start));
                                iframe.find('#EndDate').val(bAllDay ? end._d.formate('yyyy/MM/dd 00:00') : newDate(end));
                                oValidator = fnValidate(iframe);
                                fnRegisterEvent(iframe, 'add');
                            }
                        }]);
                    },
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')], //╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    yes: function(index, layero) {
                        var iframe = $('iframe').contents();
                        iframe.find('#GroupMembers').removeAttr('readonly');
                        if (!iframe.find("#form_calendar").valid()) {
                            oValidator.focusInvalid();
                            iframe.find('#GroupMembers').attr('readonly', 'readonly');
                            return false;
                        }

                        var data = getFormSerialize(iframe.find('#form_calendar'));
                        data = packParams(data);
                        data.OrgID = parent.OrgID;
                        data.UserID = parent.UserID;
                        data.Color = parent.UserInfo.CalColor;
                        data.AllDay = data.AllDay || '0';

                        g_api.ConnectLite(sProgramId, ComFn.GetAdd, data,
                            function(res) {
                                if (res.RESULT) {
                                    var sNo = res.DATA.rel;
                                    showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                                    oCalendar.fullCalendar('refetchEvents');
                                    if (parent.Outklook) {
                                        outlookAPI(outlook.Calendar_Add, {
                                            NO: sNo,
                                            ResponseRequested: true
                                        });
                                    }
                                } else {
                                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                                }
                            });
                        layer.close(index);
                    }
                });
            },
            /**
             * 資料修改
             * @param {Object} event 事件
             */
            fnUpd = function(event) {
                var oValidator = null,
                    bEdit = (event.UserID === parent.UserID && event.Memo !== 'outlook'),
                    saBtns = [],
                    layerOption = {
                        type: 2,
                        title: '修改日程',
                        shade: 0.75,
                        maxmin: true, //开启最大化最小化按钮
                        area: ['800px', '580px'],
                        content: '/Page/Pop/CalanderPop.html',
                        success: function(layero, index) {
                            var iframe = $('iframe').contents();
                            fnSetArgDrop([{
                                ArgClassID: 'CalType',
                                Select: iframe.find('#CalType'),
                                ShowId: true,
                                CallBack: function(data) {
                                    oValidator = fnValidate(iframe);
                                    iframe.find('#CalType').val(event.CalType);
                                    iframe.find('#Title').val(event.title);
                                    iframe.find('#Description').val(event.content);
                                    iframe.find('#GroupMembers').data('value', event.GroupMembers);
                                    iframe.find('[name=OpenMent][value=' + event.OpenMent + ']').click();
                                    iframe.find('[name=Importment][value=' + event.Importment + ']').click();
                                    iframe.find('#AllDay')[0].checked = event.AllDay;
                                    iframe.find('#CurrentDate').val(newDate(event.start, 1));
                                    iframe.find('#StartDate').val(newDate(event.StartDate));
                                    iframe.find('#EndDate').val(newDate(event.EndDate));
                                    if (event.GroupMembers) {
                                        fnSetUserDrop([{
                                            UserIDs: event.GroupMembers,
                                            CallBack: function(data) {
                                                var saList = data,
                                                    saName = [];
                                                $.each(saList, function(idx, item) {
                                                    saName.push(item.MemberName);
                                                });
                                                iframe.find('#GroupMembers').val(saName);
                                            }
                                        }]);
                                    }
                                    if (bEdit) {
                                        fnRegisterEvent(iframe, 'upd');
                                    } else {
                                        disableInput(iframe);
                                    }
                                }
                            }]);
                            layero.find('.layui-layer-btn1').css({ 'border-color': 'red', 'background-color': 'red', 'color': '#fff' });
                        }
                    };

                if (bEdit) {
                    saBtns = [i18next.t('common.Confirm'), i18next.t('common.Toolbar_Del'), i18next.t('common.Cancel')]; //╠common.Confirm⇒確定╣ ╠common.Toolbar_Del⇒刪除╣ ╠common.Cancel⇒取消╣
                    layerOption.yes = function(index, layero) {
                        var iframe = $('iframe').contents();
                        iframe.find('#GroupMembers').removeAttr('readonly');
                        if (!iframe.find("#form_calendar").valid()) {
                            oValidator.focusInvalid();
                            iframe.find('#GroupMembers').attr('readonly', 'readonly');
                            return false;
                        }

                        var data = getFormSerialize(iframe.find('#form_calendar'));
                        data = packParams(data, 'upd');
                        data.UserID = parent.UserInfo.MemberID;
                        data.Description = data.Description || '';
                        data.Color = parent.UserInfo.CalColor;
                        data.AllDay = data.AllDay || '0';
                        fnUpdData(event, data);
                        layer.close(index);
                    };
                    layerOption.btn2 = function() {
                        CallAjax(ComFn.W_Com, ComFn.GetDel, {
                            Params: {
                                calendar: {
                                    NO: event.id
                                }
                            }
                        }, function(res) {
                            if (res.d > 0) {
                                showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                                oCalendar.fullCalendar('removeEvents', event.id);
                                if (parent.Outklook) {
                                    outlookAPI(outlook.Calendar_Del, {
                                        OutlookEventId: event.OutlookEventId
                                    });
                                }
                            } else {
                                showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                            }
                        });
                    };
                } else {
                    saBtns = [i18next.t('common.Cancel')];
                }
                layerOption.btn = saBtns;
                layer.open(layerOption);
            },

            /**
             * 選擇分組成員
             * @param  {Object} option 配置
             */
            fnGetSetUserGroups = function(option) {
                option = option || {};
                var oGrid = null,
                    saItems = [],
                    saProfileGets = [],
                    fnGetSetlectOptions = function(el) {
                        var saSetlectOptions = [];
                        el.find('#lstRight option').each(function() {
                            var sId = this.value,
                                oItems = {},
                                sEmail = $(this).attr('data-email');
                            if (sEmail) {
                                oItems = {
                                    UserID: sId,
                                    UserName: $(this).text(),
                                    OutlookEmail: sEmail
                                };
                            } else {
                                oItems = $.map(saItems, function(item) {
                                    if (sId === item.MemberID) {
                                        return {
                                            UserID: item.MemberID,
                                            UserName: item.MemberName,
                                            OutlookEmail: item.OutlookAccount
                                        };
                                    }
                                })[0];
                            }
                            saSetlectOptions.push(oItems);
                        });

                        return saSetlectOptions;
                    },
                    fnSetProfileDrop = function(handle) {
                        return CallAjax(ComFn.W_Com, ComFn.GetList, {
                            Type: '',
                            Params: {
                                profiles: {
                                    ProfileType: 'UserGroups',
                                    OrgID: parent.OrgID,
                                    UserID: parent.UserID
                                },
                                sort: { SN: 'asc' }
                            }
                        }, function(res) {
                            if (res.d) {
                                saProfileGets = JSON.parse(res.d);
                                handle.html(createOptions(saProfileGets, 'SN', 'ProfileName'));
                            }
                        });
                    },
                    fnAddProfile = function(layero) {
                        var oAddPm = {};
                        oAddPm.OrgID = parent.OrgID;
                        oAddPm.UserID = parent.UserID;
                        oAddPm.ProfileType = 'UserGroups';
                        oAddPm.ProfileName = layero.find('#ProfileName').val();
                        oAddPm.ProfileSet = fnGetSetlectOptions(layero);
                        oAddPm.ProfileSet = JSON.stringify(oAddPm.ProfileSet);
                        oAddPm = packParams(oAddPm);

                        if (!oAddPm.ProfileName) {
                            showMsg(i18next.t("message.ProfileFeesClassName_Required")); //╠message.ProfileFeesClassName_Required⇒請填寫個人化費用類別名稱╣
                            return false;
                        }

                        CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                            Params: {
                                profiles: oAddPm
                            }
                        }, function(res) {
                            if (res.d > 0) {
                                fnSetProfileDrop(layero.find('#ProfileClass')).done(function() {
                                    layero.find('#ProfileClass option').each(function() {
                                        if ($(this).text() === oAddPm.ProfileName) {
                                            layero.find('#ProfileClass').val(this.value);
                                            return false;
                                        }
                                    });
                                });
                                showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                            } else {
                                showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                            }
                        }, function() {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        });
                    },
                    fnUpdProfile = function(layero) {
                        var oUpdPm = {},
                            sId = layero.find('#ProfileClass').val();
                        oUpdPm.ProfileName = layero.find('#ProfileName').val();
                        oUpdPm.ProfileSet = [];
                        oUpdPm.ProfileSet = fnGetSetlectOptions(layero);
                        oUpdPm.ProfileSet = JSON.stringify(oUpdPm.ProfileSet);
                        oUpdPm = packParams(oUpdPm, 'upd');

                        if (!oUpdPm.ProfileName) {
                            showMsg(i18next.t("message.ProfileFeesClassName_Required")); // 請填寫個人化費用類別名稱
                            return false;
                        }

                        CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                            Params: {
                                profiles: {
                                    values: oUpdPm,
                                    keys: { SN: sId }
                                }
                            }
                        }, function(res) {
                            if (res.d > 0) {
                                fnSetProfileDrop(layero.find('#ProfileClass')).done(function() {
                                    layero.find('#ProfileClass').val(sId);
                                });
                                showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                            } else {
                                showMsg(i18next.t("message.Modify_Failed"), 'error'); //╠message.Modify_Failed⇒修改失敗╣
                            }
                        }, function() {
                            showMsg(i18next.t("message.Modify_Failed"), 'error'); //╠message.Modify_Failed⇒修改失敗╣
                        });
                    },
                    fnDelProfile = function(layero) {
                        var sId = layero.find('#ProfileClass').val();

                        if (!sId) {
                            showMsg(i18next.t("message.DeleteItem_Required")); //╠message.DeleteItem_Required⇒請選擇要刪除的項目╣
                            return false;
                        }
                        CallAjax(ComFn.W_Com, ComFn.GetDel, {
                            Params: {
                                profiles: {
                                    SN: sId
                                }
                            }
                        }, function(res) {
                            if (res.d > 0) {
                                fnSetProfileDrop(layero.find('#ProfileClass'));
                                layero.find('#ProfileName').val('');
                                layero.find('#lstRight').html('');
                                layero.find('#lstLeft').html(createOptions(saItems, 'MemberID', 'MemberName', true));
                                layero.find('#lstLeft').find('option:first').remove();
                                showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                            } else {
                                showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                            }
                        }, function() {
                            showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                        });
                    };

                getHtmlTmp('/Page/Pop/UserGroups.html').done(function(html) {
                    layer.open({
                        type: 1,
                        title: i18next.t('common.ProfileFees'), // ╠common.ProfileFees⇒個人化費用項目╣
                        shadeClose: false,
                        shade: 0.1,
                        maxmin: true, //开启最大化最小化按钮
                        area: ['600px', '590px'],
                        content: html,
                        success: function(layero, index) {
                            var elIistLeft = layero.find('#lstLeft'),
                                elListRight = layero.find('#lstRight');
                            fnSetProfileDrop(layero.find('#ProfileClass'));
                            fnSetUserDrop([{
                                Select: elIistLeft,
                                ShowId: true,
                                Action: option.Action, //傳入action來判斷是新增或是修改
                                CallBack: function(data) {
                                    saItems = data;
                                }
                            }]).done(function() {
                                elIistLeft.find('option:first').remove();
                                optionListSearch(elIistLeft, elListRight, layero.find('#ProfileFilter'));
                            });
                            layero.find('#ProfileClass').on('change', function() {
                                var sProfile = this.value,
                                    saProfileSet = [];
                                if (sProfile) {
                                    var oProfileGet = $.grep(saProfileGets, function(e) { return e.SN.toString() == sProfile; })[0];
                                    saProfileSet = JSON.parse(oProfileGet.ProfileSet || '[]');
                                    layero.find('#ProfileName').val(oProfileGet.ProfileName);
                                } else {
                                    layero.find('#ProfileName').val('');
                                }
                                elListRight.html('');
                                elIistLeft.html(createOptions(saItems, 'MemberID', 'MemberName', true)).find('option:first').remove();
                                elIistLeft.find('option').each(function() {
                                    var _option = this,
                                        sId = $(_option).val();
                                    $.each(saProfileSet, function(index, item) {
                                        if (sId === item.UserID) {
                                            $(_option).appendTo(elListRight);
                                            item.IsMove = true;
                                            return false;
                                        }
                                    });
                                });
                                $.each(saProfileSet, function(index, item) {
                                    if (!item.IsMove) {
                                        $('<option/>', {
                                            value: item.UserID,
                                            text: item.UserName,
                                            'data-email': item.OutlookEmail,
                                            class: 'outer'
                                        }).appendTo(elListRight);
                                    }
                                });
                            });
                            layero.find('#AddOutGroupUser').on('click', function() {
                                var sOutUserName = $('#OutUserName').val(),
                                    sOutEmail = $('#OutEmail').val(),
                                    sGuid = guid();
                                if (!sOutUserName || !OutEmail) {
                                    showMsg(i18next.t("message.OutUserNameAndOutEmail_Required")); // ╠message.OutUserNameAndOutEmail_Required⇒請填寫姓名和郵箱地址╣
                                    return false;
                                }
                                if (!isEmail(sOutEmail)) {
                                    showMsg(i18next.t("message.IncorrectEmail")); // ╠message.IncorrectEmail⇒郵箱格式不正確╣
                                    return false;
                                }
                                $('<option/>', {
                                    value: sGuid,
                                    text: sOutUserName,
                                    'data-email': sOutEmail,
                                    class: 'outer'
                                }).appendTo(elListRight);
                                var oAddPm = {
                                    Guid: sGuid,
                                    UserName: sOutUserName,
                                    Email: sOutEmail
                                };
                                oAddPm = packParams(oAddPm);
                                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                                    Params: {
                                        outerusers: oAddPm
                                    }
                                });
                            });
                            layero.find('.cusclass-add').on('click', function() {
                                fnAddProfile(layero);
                            });
                            layero.find('.cusclass-upd').on('click', function() {
                                fnUpdProfile(layero);
                            });
                            layero.find('.cusclass-del').on('click', function() {
                                fnDelProfile(layero);
                            });
                            layero.find('#btnToRight').on('click', function() {
                                optionListMove(elIistLeft, elListRight);
                            });
                            layero.find('#btnToLeft').on('click', function() {
                                optionListMove(elListRight, elIistLeft);
                            });
                            layero.find('#btnToUp').on('click', function() {
                                optionListOrder(elListRight, true);
                            });
                            layero.find('#btnToDown').on('click', function() {
                                optionListOrder(elListRight, false);
                            });
                            transLang(layero);
                        },
                        btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')], //╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                        yes: function(index, layero) {
                            var saRetn = fnGetSetlectOptions(layero);
                            if (typeof option.Callback === 'function') option.Callback(saRetn);
                            layer.close(index);
                        },
                        cancel: function() {
                            if (typeof option.CancelCallback === 'function') option.CancelCallback();
                        }
                    });
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object} inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function(inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        break;
                    case "Toolbar_Leave":

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del":

                        break;
                    case "Toolbar_Exp":

                        break;
                    case "Toolbar_SynChronousOutlook":
                        {
                            var index = layer.load(0, { time: 10 * 1000 }); ////同步開啟loading，并且设定最长等待10秒
                            outlookAPI(outlook.SynChronous, { flag: "once", memo: index });
                        }
                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            saCusBtns = [];

        if (parent.Outklook) {
            saCusBtns.push({
                id: 'Toolbar_SynChronousOutlook',
                value: 'common.SynChronousOutlook' // ╠common.SynChronousOutlook⇒同步Outlook╣
            });
        }

        commonInit({
            PrgId: sProgramId,
            ButtonHandler: fnButtonHandler,
            Buttons: saCusBtns,
            GoTop: true
        });
        fnSetArgDrop([{
            ArgClassID: 'CalType',
            CallBack: function(data) {
                var saHeaderShow = [],
                    oCustomButtons = {
                        addevent: {
                            text: '新增日程',
                            click: function() {
                                fnAdd(new Date(), new Date().dateAdd('h', 1));
                            }
                        }
                    },
                    elCalType = $('<div class="fc-button-group" />');
                $.grep(data, function(item, index) {
                    var sBtnId = 'cusbtn_' + item.id,
                        sCalType = '<label for="CalType_' + index + '">\
                                    <input id="CalType_' + index + '" name="CalType[]" type="checkbox" value="' + item.id + '" checked="checked"><span class="openment-text">' + item.text + '</span>\
                                </label>';

                    saHeaderShow.push(sBtnId);
                    oCustomButtons[sBtnId] = {
                        text: item.text,
                        click: function(el) {
                            var sId = $(this).attr('data-id').replace('cusbtn_', '');
                            if ($(this).hasClass('selected')) {
                                $(this).removeClass('selected');
                                saCalType.remove(sId);
                            } else {
                                $(this).addClass('selected');
                                saCalType.push(sId);
                            }
                            oCalendar.fullCalendar('refetchEvents');
                        }
                    };
                    saCalType.push(item.id);
                    elCalType.append(sCalType);
                });

                oCalendar = $('#calendar').fullCalendar({
                    locale: 'zh-tw',
                    timezone: 'local',
                    schedulerLicenseKey: 'GPL-My-Project-Is-Open-Source',
                    theme: true,
                    weekends: true, // will hide Saturdays and Sundays
                    firstDay: 0,
                    isRTL: false,
                    defaultView: 'month',
                    defaultDate: new Date().formate("yyyy-MM-dd"),
                    navLinks: true, // can click iDay/week names to navigate views
                    editable: true,
                    droppable: true, // this allows things to be dropped onto the calendar(允許將事件拖動到日曆上)
                    eventLimit: true, // allow "more" link when too many events（允許出現更多圖標）
                    selectable: true,
                    selectHelper: true,
                    weekNumbers: true,
                    weekNumbersWithinDays: true,
                    weekNumberCalculation: 'ISO',
                    nowIndicator: true,
                    displayEventTime: true,
                    fixedWeekCount: false,
                    header: {
                        left: 'prev,next today addevent ', //saHeaderShow.join(' ')
                        center: 'title',
                        right: 'month1,month2,month,agendaWeek,agendaDay,listMonth'
                    },
                    buttonIcons: {
                        prev: 'left-single-arrow',
                        next: 'right-single-arrow',
                        prevYear: 'left-double-arrow',
                        nextYear: 'right-double-arrow'
                    },
                    themeButtonIcons: {
                        prev: 'circle-triangle-w',
                        next: 'circle-triangle-e',
                        prevYear: 'seek-prev',
                        nextYear: 'seek-next'
                    },
                    businessHours: [ // specify an array instead
                        {
                            dow: [1, 2, 3, 4, 5], // Monday, Tuesday, Wednesday
                            start: '09:00', // 8am
                            end: '18:30' // 6pm
                        }
                    ],
                    customButtons: oCustomButtons,
                    select: function(start, end) {
                        fnAdd(start, end);
                    },
                    eventClick: function (calEvent) {
                        if (!!calEvent.show && calEvent.show) {
                            fnUpd(calEvent);
                        }
                    },
                    eventDrop: function(event) {
                        var data = {};
                        data.StartDate = newDate(event.start);
                        data.EndDate = newDate(event.end);
                        fnUpdData(event, data);
                    },
                    eventResize: function(event) {
                        var data = {};
                        data.StartDate = newDate(event.start);
                        data.EndDate = newDate(event.end);
                        fnUpdData(event, data);
                    },
                    eventMouseover: function (event, jsEvent, view) {
                        addTips($('[tooltips]'));
                    },
                    events: function(start, end, timezone, callback) {
                        g_api.ConnectLite(sProgramId, 'GetList', {
                                StartDate: newDate(start),
                                EndDate: newDate(end),
                                CalType: saCalType.join(','),
                                OpenMent: saOpenMent.join(',')
                            },
                            function(res) {
                                if (res.RESULT) {
                                    var saEvents = res.DATA.rel;
                                    $.each(saEvents, function (idx, item) {
                                        if (item.CalType === '07') {
                                            var Owner =  item.CreateUser === parent.UserID;
                                            if (Owner || isAdmin)
                                                item.show = true;
                                            else {
                                                item.show = false;
                                                item.Description = "";
                                            }
                                        }
                                        else {
                                            item.show = true;
                                        }
                                       
                                    });
                                    callback(saEvents);
                                }
                            });
                    },
                    /**
                     * 處理資料（轉換成日曆資料格式）
                     */
                    eventDataTransform: function(event) {
                        event.id = event.NO;
                        event.title = event.Title;
                        event.content = event.Description;
                        event.allDay = event.AllDay;
                        if (event.StartDate) event.start = event.StartDate;
                        if (event.EndDate) event.end = event.EndDate;
                        event.color = event.Importment === 'H' ? 'red' : event.Color;
                        event.className = 'calendar-event';
                        return event;
                    }
                });

                var elOpenMent = '<div class="fc-button-group" >\
                                <label for="OpenMent_0">\
                                    <input id="OpenMent_0" name="OpenMent[]" type="checkbox" value="P" checked="checked"><span class="openment-text" data-i18n="CalanderPop.Individual">個人</span>\
                                </label>\
                                <label for="OpenMent_1">\
                                    <input id="OpenMent_1" name="OpenMent[]" type="checkbox" value="G" checked="checked"><span class="openment-text" data-i18n="CalanderPop.Group">群組</span>\
                                </label>\
                                <label for="OpenMent_3">\
                                    <input id="OpenMent_3" name="OpenMent[]" type="checkbox" value="C" checked="checked"><span class="openment-text" data-i18n="CalanderPop.Company">公司</span>\
                                </label>\
                              </div>';
                //<label for="OpenMent_2">\
                //    <input id="OpenMent_2" name="OpenMent[]" type="checkbox" value="D" checked="checked"><span class="openment-text" data-i18n="CalanderPop.Department">部門</span>\
                //</label>\

                $('.fc-toolbar .fc-left').append(elCalType[0].outerHTML);
                $('.fc-toolbar .fc-right').prepend(elOpenMent);

                $('[name="CalType[]"]').click(function() {
                    saCalType = [];
                    $('[name="CalType[]"]').each(function() {
                        if (this.checked) {
                            saCalType.push(this.value);
                        }
                    });
                    oCalendar.fullCalendar('refetchEvents');
                });
                $('[name="OpenMent[]"]').click(function() {
                    saOpenMent = [];
                    $('[name="OpenMent[]"]').each(function() {
                        if (this.checked) {
                            saOpenMent.push(this.value);
                        }
                    });
                    oCalendar.fullCalendar('refetchEvents');
                });
            }
        }]);
    },
    closeTips = function(index) {
        layer.close(index); //同步完成后關閉loading
        oCalendar.fullCalendar('refetchEvents');
    };

require(['base', 'jsgrid', 'jbox', 'util'], fnPageInit);