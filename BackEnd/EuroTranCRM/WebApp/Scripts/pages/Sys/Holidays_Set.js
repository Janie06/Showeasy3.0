'use strict';
var sProgramId = getProgramId(),
    fnPageInit = function () {
        var calendars = [],
            saHolidays = [],
            sCurrentYear = moment().format('YYYY'),
            canDo = new CanDo({
                /**
                 * 客製化按鈕
                 * @param  {Object} pargs CanDo 對象
                 * @return  {Object} ajax物件
                 */
                cusBtns: function (pargs) {
                    var saCusBtns = [{
                        id: 'PreviousYear',
                        value: 'common.Toolbar_PreviousYear',// ╠common.Toolbar_PreviousYear⇒上一年╣
                        action: function (pargs) {
                            $.each(calendars, function (i, calendar) {
                                calendar.clndr.previousYear();
                                if (i === 0) {
                                    sCurrentYear = calendar.clndr.month.format('YYYY');
                                }
                            });
                            fnSetBoxClick();
                            pargs.getOne(pargs);
                        }
                    },
                    {
                        id: 'NextYear',
                        value: 'common.Toolbar_NextYear',// ╠common.Toolbar_NextYear⇒下一年╣
                        action: function (pargs) {
                            $.each(calendars, function (i, calendar) {
                                calendar.clndr.nextYear();
                                if (i === 0) {
                                    sCurrentYear = calendar.clndr.month.format('YYYY');
                                }
                            });
                            fnSetBoxClick();
                            pargs.getOne(pargs);
                        }
                    }];
                    return saCusBtns;
                },
                /**
                 * 假日設定（單筆查詢）
                 * @param  {Object} pargs cando 對象
                 * @return  {Object} ajax物件
                 */
                getOne: function (pargs) {
                    return g_api.ConnectLite(pargs.ProgramId, pargs._api.getone,
                        { Year: sCurrentYear },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                saHolidays = [];
                                if (oRes) {
                                    pargs.action = 'upd';
                                    saHolidays = $.parseJSON(oRes.Holidays);
                                }
                                else {
                                    pargs.action = 'add';
                                }
                                fnSetHolidays();
                            }
                        });
                },
                /**
                 * 假日設定（新增）
                 * @param  {Object} pargs cando 對象
                 * @return  {Object} ajax物件
                 */
                getInsert: function (pargs) {
                    if (saHolidays.length === 0) {
                        showMsg(i18next.t("message.FirstAddHolidays")); // 請先選擇假日
                        return false;
                    }
                    var data = { Year: sCurrentYear };
                    data.Holidays = JSON.stringify(saHolidays);

                    return g_api.ConnectLite(pargs.ProgramId, pargs._api.insert, data,
                        function (res) {
                            if (res.RESULT) {
                                pargs.action = 'upd';
                                showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                            }
                            else {
                                showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                        });
                },
                /**
                 * 假日設定（修改）
                 * @param  {Object} pargs cando 對象
                 * @return  {Object} ajax物件
                 */
                getUpdate: function (pargs) {
                    if (saHolidays.length === 0) {
                        showMsg(i18next.t("message.FirstAddHolidays")); // 請先選擇假日
                        return false;
                    }
                    var data = { Year: sCurrentYear };
                    data.Holidays = JSON.stringify(saHolidays);

                    return g_api.ConnectLite(pargs.ProgramId, pargs._api.update, data,
                        function (res) {
                            if (res.RESULT) {
                                showMsg(i18next.t("message.SetUp_Success"), 'success'); // ╠message.SetUp_Success⇒設置成功╣
                            }
                            else {
                                showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.SetUp_Failed"), 'error'); // ╠message.SetUp_Failed⇒設置失敗╣
                        });
                },
                /**
                 * 頁面初始化
                 * @param  {Object} pargs CanDo 對象
                 */
                pageInit: function (pargs) {
                    pargs.getOne(pargs).done(function () {
                        fnSetBoxClick();
                    });

                    $('.cal1').each(function (indx) {
                        var that = this,
                            calendar = {},
                            iMonth = indx + 1;

                        calendar.clndr = $(that).clndr({
                            template: $('#clndr_template').html(),
                            startWithMonth: sCurrentYear + '-' + (iMonth < 10 ? '0' + iMonth : iMonth) + '-01',
                            events: [],
                            multiDayEvents: {
                                singleDay: 'date',
                                endDate: 'endDate',
                                startDate: 'startDate'
                            },
                            showAdjacentMonths: true,
                            adjacentDaysChangeMonth: false,
                            daysOfTheWeek: ['日', '一', '二', '三', '四', '五', '六'],
                            forceSixRows: true
                        });
                        calendars.push(calendar);
                    });

                    $('#weekbox :checkbox').click(function () {
                        var sVal = this.value;
                        if (this.checked) {
                            $('.calendar-dow-' + sVal).each(function () {
                                if (!$(this).hasClass('holiday')) {
                                    $(this).click();
                                }
                            });
                        }
                        else {
                            $('.calendar-dow-' + sVal).each(function () {
                                if ($(this).hasClass('holiday')) {
                                    $(this).click();
                                }
                            });
                        }
                    });
                }
            }),
            fnCacheHilodays = function () {
                saHolidays = [];
                $('.holiday').not('.last-month,.next-month').each(function () {
                    var sDate = $(this).attr('data-value');
                    saHolidays.push(sDate);
                });
            },
            /**
             * 設定日期假日點擊事件
             */
            fnSetBoxClick = function () {
                $('#currentyear').html(sCurrentYear);
                $('#weekbox :checkbox').each(function () {
                    this.checked = false;
                });

                $('.cal1 td').not('.last-month,.next-month').click(function () {
                    var that = this;
                    if ($(that).hasClass('holiday')) {
                        $(that).removeClass('holiday');
                    }
                    else {
                        $(that).addClass('holiday');
                    }
                    fnCacheHilodays();
                });
            },
            /**
             * 設定日期假日
             */
            fnSetHolidays = function () {
                $.each(saHolidays, function (i, date) {
                    $('.calendar-day-' + date).not('.last-month,.next-month').addClass('holiday');
                });
            };
    };

require(['base', 'clndr', 'cando'], fnPageInit, 'clndr');