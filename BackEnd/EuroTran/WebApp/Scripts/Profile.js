'use strict';
var sProgramId = getProgramId(),
    allMembers = [],
    /**
     * 目的：設置公司成員list
     * @param：{Array} list 人員資料
     * @param：{String} flag 是否在線
     * @return：{String} sHtml 人員列表html
     */
    fnSetMemberList = function (list, flag) {
        var sHtml = '';
        $.each(list, function (i, item) {
            var html = '<div class="team-member">'
                + '<div id="team-' + item.MemberID.replace('.', '') + '" class="online ' + (flag ? 'on' : 'off') + '"></div>'
                + '<img src="/Controller.ashx?action=getimg&folder=Members&id=' + item.MemberPic + '&orgid=' + parent.UserInfo.OrgID + '" width="40" height="40">'
                + '<span class="member-name">' + item.MemberID + '（' + item.MemberName + '）</span>'
                + '</div>';
            sHtml += html;
        });
        return sHtml;
    },
    /**
     * 目的：設置在線成員
     */
    fnSetUserOnline = function () {
        var allOnlineUsers = parent.online_Users,
            onlines = [],
            offlines = [],
            sHtml = '';
        $.grep(allMembers, function (user) {
            var current = $.grep(allOnlineUsers, function (item) { return item.UserId === user.MemberID; });
            if (current.length > 0) {
                onlines.push(user);
            }
            else {
                offlines.push(user);
            }
        });
        sHtml += fnSetMemberList(onlines, true);
        sHtml += fnSetMemberList(offlines, false);
        $(".team").html(sHtml);
    },
    fnPageInit = function () {
        var oGrid1 = null,
            oGrid2 = null,
            sBackgroundImg = parent.OrgID + parent.UserID,
            sBackgroundImgFileId = '',
            Ann_imer = null,
            /*
             * 獲取背景圖片
             * @return：{Object} ajax 對象
             */
            fnGetBackgroundImg = function () {
                return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                    Type: '',
                    Params: {
                        files: {
                            ParentID: sBackgroundImg,
                            OrgID: parent.OrgID
                        }
                    }
                }, function (res) {
                    if (res.d) {
                        var oData = $.parseJSON(res.d);
                        $('.profile-cover').css({ 'background': "url('../../" + (oData.FilePath || '').replace(/\\/g, "\/") + "')" });
                        sBackgroundImgFileId = oData.FileID;
                    }
                });
            },
            /*
            * 設定背景
            */
            fnBgimgfile = function () {
                $('#bgimgfile').val('').off('change').on('change', function () {
                    var sLowerName = this.value.toLowerCase();
                    if (sLowerName.indexOf('.jpg') > -1 || sLowerName.indexOf('.jpeg') > -1 || sLowerName.indexOf('.png') > -1 || sLowerName.indexOf('.gif') > -1 || sLowerName.indexOf('.bmp') > -1) {
                        var sFileName = this.value;
                        DelFile(sBackgroundImgFileId, '', false).done(function () {
                            $.ajaxFileUpload({
                                url: '/Controller.ashx?action=upload&source=Members&userid=' + parent.UserID + '&orgid=' + parent.OrgID + '&parentid=' + sBackgroundImg,
                                secureuri: false,
                                fileElementId: 'bgimgfile',
                                success: function (el, status, data) {
                                    var oData = $.parseJSON(data.responseText)[0];
                                    sBackgroundImgFileId = oData.FileID;
                                    $('.profile-cover').css({ 'background': "url('../../" + oData.FilePath.replace(/\\/g, "\/") + "')" });
                                },
                                error: function (data, status, e) {
                                    showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                }
                            });
                        });
                    }
                    else {
                        showMsg(i18next.t("message.FileTypeError"), 'error'); // ╠message.FileTypeError⇒文件格式錯誤╣
                    }
                }).click();
            },
            /**
             * 目的：設置個人資料明細
             */
            fnSetUserInfo = function () {
                $('#user_img').attr('src', '/Controller.ashx?action=getimg&folder=Members&id=' + parent.UserInfo.MemberPic + '&orgid=' + parent.UserInfo.OrgID);
                $('.user_name').text(parent.UserInfo.MemberName);
                $('.user_jobtitle').text(parent.UserInfo.JobtitleName);
                $('.user_address').text(parent.UserInfo.Address);
                $('.user_mail').text(parent.UserInfo.Email);
                fnGetBackgroundImg();
                $('.profile-cover').on('click', function () {
                    fnBgimgfile();
                });
                $('#user_img').on('click', function (e) {
                    e.stopPropagation();
                });
            },
            /**
             * 公告實現輪播
             */
            fnAnnouncementTimer = function () {
                if ($('.announcement ul').find('li').length > 1) {
                    var iH = $('.announcement ul li:first').height();
                    $('.announcement ul').animate({
                        marginTop: -iH - 13
                    }, 1000, function () {
                        $(this).css({ marginTop: -8 }).find("li:first,hr:first").appendTo(this);
                    });
                }
            },
            /**
             * 目的：加載公告
             */
            fnGetAnnouncement = function () {
                if (!parent.UserInfo.MemberID) { return; }
                g_api.ConnectLite(Service.com, 'GetAnnlist', {}, function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel,
                            sHtml = '';
                        if (saData.length > 0) {
                            $.each(saData, function (idx, item) {
                                sHtml += '<li><a class="a-url" id="' + item.AnnouncementID + '" style="color:' + (item.FontColor || '#000') + '">' + item.Title + '</a></li><hr>';
                            });
                            $('.announcement ul').html(sHtml).find('a').on('click', function () {
                                var sId = this.id;
                                //parent.openPageTab('Announcement_Upd', '?Action=Upd&AnnouncementID=' + sId);
                                parent.fnOpenAnn(sId, false);;
                            });
                            $('.announcement-get-more a').on('click', function () {
                                parent.openPageTab('AnnouncementList_Qry');
                            });
                            var h1 = $('.announcement').height();
                            var h2 = $('.announcement ul').height();

                            if (h2 > h1 + 5) {
                                Ann_imer = setInterval(fnAnnouncementTimer, 3000);
                                $('.announcement ul li').mousemove(function () {
                                    clearInterval(Ann_imer);
                                }).mouseout(function () {
                                    Ann_imer = setInterval(fnAnnouncementTimer, 3000);
                                });
                            }
                        }
                        else {
                            $('.announcement-get-more').hide();
                        }
                    }
                });
            },
            /**
             * 目的：加載請假資訊
             */
            fnGetLeaves = function () {
                g_api.ConnectLite(Service.eip, 'GetLeavelist', {}, function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel,
                            sHtml_Today = '',
                            sHtml_Tomorrow = '',
                            rToday = new Date(newDate(null, true)),
                            rTomorrow = new Date(newDate(null, true)).dateAdd('d', 1),
                            rAfterTomorrow = new Date(newDate(null, true)).dateAdd('d', 2),
                            saToday = $.grep(saData, function (cur) {
                                return new Date(cur.Info.StartDate) <= rTomorrow && new Date(cur.Info.EndDate) >= rToday;
                            }),
                            saTomorrow = $.grep(saData, function (cur) {
                                return new Date(cur.Info.StartDate) <= rAfterTomorrow && new Date(cur.Info.EndDate) >= rTomorrow;
                            });
                        sHtml_Today = $('#temp_leave').render(saToday);
                        sHtml_Tomorrow = $('#temp_leave').render(saTomorrow);
                        $('.today').html(sHtml_Today);
                        $('.tomorrow').html(sHtml_Tomorrow);
                        $('#TodayLeavesCount').html($('<a/>', { html: '(' + saToday.length + ')' }));
                        $('#TomorrowLeavesCount').html($('<a/>', { html: '(' + saTomorrow.length + ')' }));
                        if (parent.UserInfo.roles.toLowerCase().indexOf('admin') > -1) {
                            $('span.a-url').on('click', function () {
                                var sId = $(this).attr('data-id'),
                                    sUserid = $(this).attr('data-userid'),
                                    sStatus = $(this).attr('data-status');
                                if ('A,C'.indexOf(sStatus) > -1 && sUserid === parent.UserID) {
                                    parent.openPageTab('Leave_Upd', '?Action=Upd&Guid=' + sId);
                                }
                                else {
                                    parent.openPageTab('Leave_View', '?Action=Upd&Guid=' + sId);
                                }
                            });
                        }
                    }
                });
            },
            /**
             * 目的：加載待處理事項資料(非EIP)
             * @param {Object} args grid參數
             * @return {Function}  ajax 物件
             */
            fnGetTasks_BILL = function (args) {
                var oQueryPm = {};
                $.extend(oQueryPm, args);
                oQueryPm.IsEIP = false;
                oQueryPm.pageIndex = oQueryPm.pageIndex || 1;
                oQueryPm.pageSize = oQueryPm.pageSize || 10;
                oQueryPm.sortField = oQueryPm.sortField || 'CreateDate';
                oQueryPm.sortOrder = oQueryPm.sortOrder || 'desc';

                return g_api.ConnectLite('Task_Qry', ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        $('#TaskListCount').html($('<a/>', { html: '(' + oRes.Total + ')' }));
                    }
                });
            },
            /**
             * 目的：加載待處理事項資料(EIP)
             * @param {Object} args grid參數
             * @return {Function}  ajax 物件
             */
            fnGetTasks_EIP = function (args) {
                var oQueryPm = {};
                $.extend(oQueryPm, args);
                oQueryPm.IsEIP = true;
                oQueryPm.pageIndex = oQueryPm.pageIndex || 1;
                oQueryPm.pageSize = oQueryPm.pageSize || 10;
                oQueryPm.sortField = oQueryPm.sortField || 'CreateDate';
                oQueryPm.sortOrder = oQueryPm.sortOrder || 'desc';

                return g_api.ConnectLite('Task_Qry', ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        $('#EipListCount').html($('<a/>', { html: '(' + oRes.Total + ')' }));
                    }
                });
            },
            /**
             * 目的：初始化代辦Grid
             */
            fnInitGrid = function () {
                $("#jsGrid_Bill").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    pageLoading: true,
                    inserting: false,
                    editing: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: 10,
                    pageButtonCount: parent.SysSet.GridPages || 15,
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    fields: [
                        { name: "EventNo", title: 'common.EventNo', align: 'center', width: 80 },// ╠common.EventNo⇒事件編號╣
                        { name: "SourceFromName", title: 'common.SourceFromName', width: 70 },
                        {
                            name: "EventName", title: 'common.Purpose', width: 250, itemTemplate: function (value, item) {
                                var oGoPage = $('<a>', {
                                    class: 'a-url',
                                    html: value,
                                    click: function () {
                                        var sEditPrgId = item.SourceFrom.replace('_Qry', '_Upd');
                                        if (navigator.userAgent.match(/mobile/i)) {
                                            goToEdit(sEditPrgId, item.Params);
                                        }
                                        else {
                                            parent.openPageTab(sEditPrgId, item.Params);
                                        }
                                    }
                                });
                                return [oGoPage, item.EIP_Status === 'E' ? '<span style="color:#DF5F09">(' + i18next.t('common.ToHandle') + ')</span>' : ''];//╠common.ToHandle⇒待經辦╣
                            }
                        },
                        { name: "CreateUserName", title: 'common.Publisher', width: 70 },//╠common.Publisher⇒發佈人╣
                        {
                            name: "Status", title: 'common.Status', width: 60, itemTemplate: function (value) {
                                var oStatus = { U: '未審核', G: '審核中', D: '已審核（待確認）', O: '已審核' };
                                return oStatus[value] || '未審核';
                            }
                        },
                        {//╠common.PublishDate⇒發佈時間╣
                            name: "CreateDate", title: 'common.PublishDate', align: 'center', width: 110, itemTemplate: function (value) {
                                return newDate(value);
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetTasks_BILL(args);
                        },
                    },
                    onDataLoaded: function (args) {
                        setTimeout(function () {
                            var oPage = $('#PerPageNum');
                            oPage.after(oPage.val()).remove();
                        }, 100);
                        oGrid1 = args.grid;
                    }
                });
                $("#jsGrid_Eip").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    pageLoading: true,
                    inserting: false,
                    editing: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: 10,
                    pageButtonCount: parent.SysSet.GridPages || 15,
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    fields: [
                        { name: "EventNo", title: 'common.EventNo', align: 'center', width: 80 },
                        { name: "SourceFromName", title: 'common.SourceFromName', width: 70 },
                        {
                            name: "EventName", title: 'common.Purpose', width: 250, itemTemplate: function (value, item) {// ╠common.Purpose⇒主旨╣
                                var oGoPage = $('<a>', {
                                    class: 'a-url',
                                    html: value,
                                    click: function () {
                                        var sEditPrgId = item.SourceFrom.replace('_Qry', '_Upd');
                                        if (navigator.userAgent.match(/mobile/i)) {
                                            goToEdit(sEditPrgId, item.Params);
                                        }
                                        else {
                                            parent.openPageTab(sEditPrgId, item.Params);
                                        }
                                    }
                                });
                                return [oGoPage, item.EIP_Status === 'E' ? '<span style="color:#DF5F09">(' + i18next.t('common.ToHandle') + ')</span>' : ''];//╠common.ToHandle⇒待經辦╣
                            }
                        },
                        { name: "CreateUserName", title: 'common.Publisher', width: 70 },//╠common.Publisher⇒發文者╣
                        { name: "ProgressShow", title: 'common.Progress', align: 'center', width: 50 },//╠common.Progress⇒進度╣
                        {
                            name: "Status", title: 'common.Status', width: 60, itemTemplate: function (value) {//╠common.Status⇒狀態╣
                                var oStatus = { U: '未審核', G: '審核中', D: '已審核（待確認）', O: '已審核' };
                                return oStatus[value] || '未審核';
                            }
                        },
                        {
                            name: "CreateDate", title: 'common.PublishDate', align: 'center', width: 110, itemTemplate: function (value) {
                                return newDate(value);
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetTasks_EIP(args);
                        },
                    },
                    onDataLoaded: function (args) {
                        setTimeout(function () {
                            var oPage = $('#PerPageNum');
                            oPage.after(oPage.val()).remove();
                        }, 100);
                        oGrid2 = args.grid;
                    }
                });
            },
            /**
             * 目的：頁面初始化
             */
            init = function () {
                var myHelpers = {
                    setDateRange: function (date1, date2) {
                        var r1 = newDate(date1, true),
                            r2 = newDate(date2, true);
                        return r1 + '~' + r2;
                    },
                    setFilePath: function (val, orgid) {
                        return '/Controller.ashx?action=getimg&folder=Members&id=' + val + '&orgid=' + orgid;
                    },
                    setLeaveTitle: function (info) {
                        return $('<span/>', {
                            class: 'a-url',
                            html: info.KeyNote,
                            'data-id': info.Guid,
                            'data-userid': info.AskTheDummy,
                            'data-status': info.Status
                        })[0].outerHTML;
                    }
                };
                $.views.helpers(myHelpers);

                fnSetUserInfo(); //設置個人資料明細
                fnInitGrid()//初始化代辦Grid
                fnSetUserDrop([
                    {
                        NotUserIDs: parent.UserID,
                        Action: 'add',
                        CallBack: function (data) {
                            allMembers = data;
                            fnSetUserOnline();
                            slimScroll();
                        }
                    }
                ]);//加載公司成員
                fnGetAnnouncement();//加載公告
                fnGetLeaves();//加載請假資訊
                goTop();//置頂控件

                onresize();

                $('.panel-reload').on('click', function () {
                    switch (this.id) {
                        case 'eip_box':
                            {
                                //debugger;
                                oGrid2.pageIndex = 1;
                                oGrid2.loadData();
                            }
                            break;
                        case 'bill_box':
                            {
                                oGrid1.pageIndex = 1;
                                oGrid1.loadData();
                            }
                            break;
                        case 'ann_box':
                            fnGetAnnouncement();//加載公告
                            break;
                        case 'leave_box1':
                        case 'leave_box2':
                            fnGetLeaves();//加載請假資訊
                            break;
                    }
                });
            };

        init();
    };

require(['base', 'jsgrid', 'ajaxfile', 'util'], fnPageInit);