var msgs,
    iCuryIndex = 0,
    iTipsCount = 0,
    online_Users = [],
    Ann = {
        div: $('div.notice-box'),
        ul: $('div.notice-box').find('ul'),
        li_class: 'li-list',
        active: 'li-active',
        fadeOut: 500,
        timer: null
    },
    AnnList = [],
    /**上線登陸註冊信息
     * @param {String} islogin 是否登入
     */
    fnRegister = function (islogin) {
        var orgid = UserInfo.OrgID,
            userid = UserInfo.MemberID,
            username = UserInfo.MemberName;
        //上線
        msgs.server.register(orgid, userid, username, islogin);
    },
    /**
     * 查看公告明細
     * @param {String} id 公告guid
     * @param {String} flag 是否包含關閉按鈕
     */
    fnOpenAnn = function (id, flag) {
        getHtmlTmp('/Page/Pop/AnnounceInfo.html').done(function (html) {
            var oAnnInfo = Enumerable.From(AnnList).Where(function (item) { return item.AnnouncementID === id; }).First();
            oAnnInfo.CategoryName = i18next.t('common.Announcement');// ╠common.Announcement⇒公 告╣
            oAnnInfo.CreateDate = newDate(oAnnInfo.CreateDate, 'date');
            var sHtml = $('<script type="text/x-jsrender"/>').html(html).render(oAnnInfo);
            layer.open({
                type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                title: oAnnInfo.CategoryName,
                //title: false, //不显示标题栏
                area: '640px;',//寬度
                shade: 0.75,//遮罩
                closeBtn: flag ? 0 : 1,
                shadeClose: true,
                //maxmin: true, //开启最大化最小化按钮
                id: 'layer_Announce', //设定一个id，防止重复弹出
                offset: '100px',//右下角弹出
                anim: 0,//彈出動畫
                btn: flag ? [i18next.t('common.Gotit')] : [],
                btnAlign: 'c',//按鈕位置
                content: sHtml,
                success: function (layero, index) {
                    layero.find('a').each(function () {
                        if (($(this).attr('href') || '').indexOf('net/upload/file') > -1 && (($(this).prev().attr('src') || '').indexOf('icon_jpg') > -1 || ($(this).prev().attr('src') || '').indexOf('icon_pdf') > -1)) {
                            $(this).attr('target', '_new');
                        }
                    });
                    layero.find('.layui-layer-title').css({ 'text-align': 'center', 'padding': '0 30px 0 20px', 'font-size': '20px', 'font-weight': '600' });
                    slimScroll();
                },
                yes: function (index, layero) {
                    CallAjax(ComFn.W_Com, ComFn.GetCount, {
                        Params: {
                            read: {
                                AnnouncementID: oAnnInfo.AnnouncementID,
                                CreateUser: UserInfo.MemberID
                            }
                        }
                    }, function (res) {
                        if (res.d === 0) {
                            CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                                Params: {
                                    read: {
                                        AnnouncementID: oAnnInfo.AnnouncementID,
                                        CreateUser: UserInfo.MemberID,
                                        CreateDate: newDate()
                                    }
                                }
                            });
                        }
                    });
                    setTimeout(function () { fnGetAnnouncements(); }, 60000);
                    layer.close(index);
                }
            });
        });
    },
    /**
     * 查看公告明細
     * @param {Object} tips 提示資料
     */
    fnShowTips = function (tips) {
        getHtmlTmp('/Page/Pop/AnnounceInfo.html').done(function (html) {
            var oTips = {};
            oTips.CategoryName = i18next.t('common.SystemTips');// ╠common.SystemTips⇒系統提醒╣
            oTips.CreateUserName = '';
            oTips.CreateDate = '';
            oTips.Description = '';
            var saTips = JSON.parse(tips);
            $.each(saTips, function (idx, _tips) {
                oTips.Description += '<p class="tipslink" data-from="' + _tips.SourceFrom + '" data-parm="' + _tips.Params + '"><a style="cursor: pointer;"><span style="font-size: 14px;">' + (idx + 1) + '. ' + _tips.EventName + '  </span></a><span>' + _tips.Important + '  ' + newDate(_tips.StartDate, true) + '~' + newDate(_tips.StartDate, true) + '</span></p>';
            });
            var sHtml = $('<script type="text/x-jsrender"/>').html(html).render(oTips);
            layer.open({
                type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                title: oTips.CategoryName,
                area: '640px;',//寬度
                shade: 0.75,//遮罩
                closeBtn: 1,
                shadeClose: true,
                id: 'layer_Tips', //设定一个id，防止重复弹出
                offset: '100px',//右下角弹出
                anim: 0,//彈出動畫
                btn: [i18next.t('common.Gotit')], // ╠common.Gotit⇒知道了╣
                btnAlign: 'c',//按鈕位置
                content: sHtml,
                success: function (layero, index) {
                    layero.find('.tipslink>a').click(function () {
                        var sFrom = $(this).parent().attr('data-from').replace('_Qry', '_Upd'),
                            sParm = $(this).parent().attr('data-parm');
                        openPageTab(sFrom, sParm);
                        layer.close(index);
                    });
                    layero.find('.layui-layer-title').css({ 'text-align': 'center', 'padding': '0 30px 0 20px', 'font-size': '20px', 'font-weight': '600' });
                    slimScroll();
                }
            });
        });
    },
    /**
     * @param {Object} tips 提示資料
     */
    fnAttendanceTips = function (tips) {
        getHtmlTmp('/Page/Pop/AnnounceInfo.html').done(function (html) {
            var oTips = {};
            oTips.CategoryName = i18next.t('common.EiptemTips');// ╠common.EiptemTips⇒考勤未打卡提醒╣
            oTips.CreateUserName = '';
            oTips.CreateDate = '';
            oTips.Description = '';
            var saTips = JSON.parse(tips);
            $.each(saTips, function (idx, _tips) {
                oTips.Description += '<p class="tipslink" data-id="' + _tips.NO + '" data-user="' + _tips.Owner + '"><a style="cursor: pointer;"><span style="font-size: 14px;">' + (idx + 1) + '. ' + _tips.Title + '  </span></a>&nbsp;&nbsp;<a class="a-url" data-i18n="common.CancelTips">取消提醒</a></p>';// ╠common.CancelTips⇒取消提醒╣
            });
            var sHtml = $('<script type="text/x-jsrender"/>').html(html).render(oTips);
            layer.open({
                type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                title: oTips.CategoryName,
                area: '680px;',//寬度
                shade: 0.75,//遮罩
                closeBtn: 1,
                shadeClose: true,
                id: 'layer_Tips', //设定一个id，防止重复弹出
                offset: '100px',//右下角弹出
                anim: 0,//彈出動畫
                btn: [i18next.t('common.Gotit'), i18next.t('common.CancelAllTips')], // ╠common.CancelAllTips⇒取消所有提醒╣
                btnAlign: 'c',//按鈕位置
                content: sHtml,
                success: function (layero, index) {
                    layero.find('.a-url').click(function () {
                        var oTips = $(this).parent(),
                            sTipsId = oTips.attr('data-id');
                        fnDeleteEipTips(sTipsId).done(function () {
                            oTips.remove();
                        });
                    });
                    layero.find('.layui-layer-title').css({ 'text-align': 'center', 'padding': '0 30px 0 20px', 'font-size': '20px', 'font-weight': '600' });
                    slimScroll();
                },
                yes: function (index, layero) {
                    layer.close(index);
                },
                btn2: function (index, layero) {
                    var saIds = [];
                    layero.find('.tipslink').each(function () {
                        saIds.push({
                            NO: $(this).attr('data-id')
                        });
                    });
                    fnDeleteEipTips(saIds);
                }
            });
        });
    },
    /**
     * 公告實現輪播
     */
    fnAnnouncementTimer = function () {
        if (Ann.ul.find('li').length > 1) {
            Ann.ul.animate({
                marginTop: "-5.7rem"
            }, 1000, function () {
                $(this).css({ marginTop: "0rem" }).find("li:first").appendTo(this);
            });
        }
    },
    /**
     * 彈出公告訊息
     * @param {Object} list 公告list
     */
    fnShowAlertAnnouncement = function (list) {
        if (list) {
            $.each(list, function (index, item) {
                if (!item.IsAlert) {
                    fnOpenAnn(item.AnnouncementID, true);
                    return false;
                }
            });
        }
    },
    /**
     * 顯示一般輪播公告
     * @param {Object} list 公告list
     */
    fnShowSlideAnnouncement = function (list) {
        //加載公告
        var saLi = [];
        AnnList = list;
        $.each(list, function (idx, item) {
            var sFontColor = item.FontColor || '#000',
                sTitle = item.Title;
            if (sTitle.length > 27) {
                sTitle = sTitle.substr(0, 27) + ' . . .';
            }
            saLi.push($('<li />').addClass(Ann.li_class).attr('data-guid', item.AnnouncementID).append($('<a/>',
                {
                    html: sTitle,
                    click: function () {
                        fnOpenAnn(item.AnnouncementID, false);
                    }
                }).css('color', sFontColor)));
        });
        if (saLi.length > 0) {
            Ann.ul.html('').append(saLi);
            Ann.div.show();
        }
        else {
            Ann.div.hide();
        }

        if (Ann.timer) clearInterval(Ann.timer);
        Ann.timer = setInterval(fnAnnouncementTimer, 5000);
        Ann.ul.find('li').mousemove(function () {
            clearInterval(Ann.timer);
        }).mouseout(function () {
            Ann.timer = setInterval(fnAnnouncementTimer, 5000);
        });
    },
    /**
     * 獲取當前公告信息
     */
    fnGetAnnouncements = function () {
        if (!UserInfo.MemberID) { return; }
        g_api.ConnectLite(Service.com, 'GetAnnlist', {}, function (res) {
            if (res.RESULT) {
                AnnList = res.DATA.rel;
                fnShowSlideAnnouncement(AnnList);
                fnShowAlertAnnouncement(AnnList);
            }
        });
    },

    //提醒未打卡人員
    fnShowAbsenceNotification = function (saAttendances) {
        getHtmlTmp('/Page/Pop/AnnounceInfo.html').done(function (html) {
            let btnwords = i18next.t('common.Gotit');
            if (!btnwords)
                btnwords = "知道了";
            var oTips = {};
            oTips.CategoryName = i18next.t('common.EiptemTips');// ╠common.EiptemTips⇒考勤未打卡提醒╣
            oTips.CreateUserName = '';
            oTips.CreateDate = '';
            oTips.Description = '';
            var Days = '';

            $.each(saAttendances, function (idx, _sad) {
                let CardDate = new Date(_sad.CardDate);
                Days += CardDate.formate("yyyy.MM.dd (EEE)") + '，';
            });
            oTips.Description = '<span style="font-size: 18px;"><p>您於' + Days + '</P><p>刷卡紀錄異常，請盡快辦理請假程序。</p><p>如有疑問，請向管理員詢問，謝謝！</p></span>';
            var sHtml = $('<script type="text/x-jsrender"/>').html(html).render(oTips);
            layer.open({
                type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                title: oTips.CategoryName,
                area: '680px;',//寬度,
                shade: 0.8,//遮罩
                closeBtn: 1,
                shadeClose: true,
                id: 'layer_Attendances', //设定一个id，防止重复弹出
                offset: '100px',//右下角弹出
                anim: 0,//彈出動畫
                btn: [btnwords],
                btnAlign: 'c',//按鈕位置
                content: sHtml,
                success: function (layero, index) {
                    slimScroll();
                },
                yes: function (index, layero) {
                    g_db.SetItem("AbsenceFromLastWeek", "True");
                    layer.close(index);
                },
            });
        });
    }

/**
* 獲取上週未刷卡／遲到／早退，周一僅提醒一次。
*/
fnGetAbsenceFromLastWeek = function () {
    let DayOfWeek = new Date().getDay();
    let HadNoticed = g_db.GetItem("AbsenceFromLastWeek");
    if (!UserInfo.MemberID) { return; }
    if (DayOfWeek !== 1) {
        g_db.SetItem("AbsenceFromLastWeek", "");
    }
    else if (!HadNoticed && DayOfWeek === 1) {
        g_api.ConnectLite(Service.com, 'GetAbsenceFromLastWeek', {}, function (res) {
            if (res.RESULT) {
                let AbsenceList = res.DATA.rel;
                if (AbsenceList.length > 0)
                    fnShowAbsenceNotification(AbsenceList);
                else
                    g_db.SetItem("AbsenceFromLastWeek", "True");
            }
        });
    }
},

    /**
     * 獲取所有提示
     */
    fnGetTips = function () {
        CallAjax(ComFn.W_Com, ComFn.GetList, {
            Type: '',
            Params: {
                tips: {
                    IsRead: 'N',
                    Owner: window.UserID,
                    OrgID: window.OrgID
                },
                sort: { CreateDate: 'desc' }
            }
        }, function (res) {
            if (res.d) {
                var saList = $.parseJSON(res.d),
                    myHelpers = {
                        dtformate: function (val) {
                            return newDate(val);
                        },
                        setIcon: function (type) {
                            var sIcon = 'icon-bell';
                            if (type === 'fa-check') {
                                sIcon = 'fa fa-check';
                            }
                            else if (type === 'fa-times') {
                                sIcon = 'fa fa-times';
                            }
                            return sIcon;
                        },
                        setBgcolor: function (type) {
                            var sBgcolor = 'success';
                            if (type === 'fa-check') {
                                sBgcolor = 'green';
                            }
                            else if (type === 'fa-times') {
                                sBgcolor = 'red';
                            }
                            return sBgcolor;
                        }
                    };
                $.views.helpers(myHelpers);
                iTipsCount = saList.length;
                if (iTipsCount > 0) {
                    var sHtml = $('#temp_tips').render(saList);//angularJS
                    $('.tips-count').text(iTipsCount > 100 ? '99+' : iTipsCount);
                    $('.tipscount').text(iTipsCount);
                    $('#alltips').html(sHtml).find('.tips-item').on('click', function () {
                        var saUrl = $(this).attr('data-url').split('|'),
                            sPrgId = saUrl[0],
                            sParam = saUrl[1];
                        parent.openPageTab(sPrgId, sParam);
                        fnRemoveTips(false, $(this));
                    });

                    $('.stop-prevent').off('click').on('click', function (e) {
                        var that = this;
                        if ($(that).hasClass('tips-delete')) {
                            fnRemoveTips(false, $(that).parents('.tips-item'));
                        }
                        else if ($(that).hasClass('tips-clearall')) {
                            fnRemoveTips(true);
                        }
                        return false;
                    });
                    $('.tips-has,.tips-count,.tipscount').show();
                    $('.tips-empty').hide();
                }
                else {
                    $('.tips-empty').show();
                    $('.tips-has,.tips-count,.tipscount').hide();
                }
            }
        });
    },
    /**
     * 移除頁面提醒
     * @param  {Boolean} clearall 是否清空全部
     * @param  {HTMLElement} otips 當前資料物件
     */
    fnRemoveTips = function (clearall, otips) {
        var fnSet = function () {
            $('.tips-count').text(iTipsCount > 100 ? '99+' : iTipsCount);
            $('.tipscount').text(iTipsCount);
            if (iTipsCount) {
                $('.tips-has,.tips-count,.tipscount').show();
                $('.tips-empty').hide();
            }
            else {
                $('.tips-empty').show();
                $('.tips-has,.tips-count,.tipscount').hide();
            }
        };
        if (clearall) {
            var saIds = [];
            $('#alltips .tips-item').each(function () {
                saIds.push({
                    NO: $(this).attr('data-id')
                });
            });
            fnDeleteTips(saIds).done(function (res) {
                if (res.d > 0) {
                    iTipsCount = 0;
                    $('#alltips').html('');
                    fnSet();
                }
            });
        }
        else {
            fnDeleteTips(otips.attr('data-id')).done(function (res) {
                if (res.d > 0) {
                    iTipsCount--;
                    otips.remove();
                    fnSet();
                }
            });
        }
    },
    /**
     * 刪除提醒資料
     * @param  {HTMLElement} tips 要刪除的消息ID
     * @return  {Function} ajax物件
     */
    fnDeleteTips = function (tips) {
        return CallAjax(ComFn.W_Com, ComFn.GetDel, {
            Params: {
                tips: typeof tips === 'string' ? { NO: tips } : tips
            }
        });
    },
    /**
     * 刪除EIP提醒
     * @param  {HTMLElement} tips 要刪除的消息ID
     * @return  {Function} ajax物件
     */
    fnDeleteEipTips = function (tips) {
        return CallAjax(ComFn.W_Com, ComFn.GetDel, {
            Params: {
                clocktips: typeof tips === 'string' ? { NO: tips } : tips
            }
        });
    },
    /**
     * 重組數組
     * @param  {Array} tipsusers 人員列表
     * @return {Array} 人員組合ID
     */
    fnReleaseUsers = function (tipsusers) {
        var saUsers = [];
        if (tipsusers && tipsusers.length > 0) {
            $.each(tipsusers, function (idx, user) {
                if (user) {
                    saUsers.push(window.OrgID + user);
                }
            });
        }
        return saUsers;
    },
    /**
     * 刪除提醒資料
     * @param  {Array} data 提示資料
     * @param  {Array} tipsusers 人員列表
     * @return {Object} ajax
     */
    fnAddTips = function (data, tipsusers) {
        return CallAjax(ComFn.W_Com, ComFn.GetAdd, {
            Params: {
                tips: data
            }
        }, function (res) {
            if (res.d > 0) {
                if (tipsusers) {
                    var sTipsUsers = fnReleaseUsers(tipsusers);
                    parent.msgs.server.pushTips(sTipsUsers);
                }
            }
        });
    },
    /**
    * 取得樹狀圖清單資料
    * @param  {String} sModid 模組ID
    * @return {String} 當前模組下所有程式清單html字串
    */
    getListMenu = function (sModid) {
        var saProgramList = g_db.GetDic('programList') || [],
            sectionData = new Array(2),
            Menuli = '<ul class="nav nav-pills">',                                 //左邊清單列表
            Listli = '<ul>',                                //右邊清單列表
            subStyle = '';                              //設定模組樣式Class

        //set Menu
        $.each(saProgramList, function (i, program) {
            if (program.ParentID === '' && program.ModuleID === sModid && program.ShowInList.toLowerCase() === 'y') {
                Menuli += " <li id=\"" + program.ModuleID + "\" class='menu-layer-one onmenu active' onclick=\"setTreeMenu('" + program.Module + "'); setMenuStyle('" + program.ModuleID + "');return false;\"> ";
                Menuli += " <a href='#' data-toggle='tab' class='menu-layer-one' data-i18n=common." + program.ModuleID + "></a></li> ";
            }                                                                                 //功能清單中的管理模組頁簽標題轉換
            else if (program.ParentID === '' && program.ModuleID !== sModid && program.ShowInList.toLowerCase() === 'y') {
                Menuli += " <li id=\"" + program.ModuleID + "\" class='menu-layer-one drophide' onclick=\"setTreeMenu('" + program.ModuleID + "'); setMenuStyle('" + program.ModuleID + "');return false;\"> ";
                Menuli += " <a href='#' data-toggle='tab' class='menu-layer-one' data-i18n=common." + program.ModuleID + "></a></li> ";
            }                                                                                 //功能清單中的進出口頁簽標題轉換
        });

        //setList
        $.each(saProgramList, function (e, program) {
            if (program.ParentID === sModid && program.ShowInList.toLowerCase() === 'y') {
                var subModule = '<ul>';
                var moduleid = program.ModuleID;
                $.each(saProgramList, function (i, program1) {
                    if (program1.ParentID === moduleid && program1.ShowInList.toLowerCase() === 'y') {
                        var sub2Module = "<ul>";
                        var sub2moduleid = program1.ModuleID;
                        $.each(saProgramList, function (m, program2) {
                            if (program2.ParentID === sub2moduleid && program2.ShowInList.toLowerCase() === 'y') {
                                var sub3Module = "<ul>";
                                var sub3moduleid = program2.ModuleID;
                                $.each(saProgramList, function (m, program3) {
                                    if (program3.ParentID === sub3moduleid && program3.ShowInList.toLowerCase() === 'y') {
                                        if (program3.FilePath === '#') {
                                            sub3Module += " <li id=\"" + program3.ModuleID + "\"  class='layer-four folderType'>";
                                            sub3Module += " <a>" + program3.ModuleName + "</a></li> ";
                                        }
                                        else {
                                            sub3Module += " <li id=\"" + program3.ModuleID + "\"  class='layer-four ProgramType' onclick=\"if('" + program3.FilePath + "'!='#')  parent.openPageTab('" + program3.ModuleID + "');if('" + program3.FilePath + "'=='#'){setTreeMenu('" + program3.ModuleID + "');}\"> ";
                                            sub3Module += " <a href='#' data-i18n=common." + program3.ModuleID + "></a></li> ";
                                        }//功能清單中系統管理子層(系統架構、參數管理、組織結構)的底層標題
                                    }
                                });
                                sub3Module += '</ul>';
                                if (program2.FilePath === '#') {
                                    sub2Module += " <li id=\"" + program2.ModuleID + "\"  class='layer-three folderType'>";
                                    sub2Module += " <a data-i18n=common." + program2.ModuleID + "></a>" + sub3Module + "</li> ";
                                }//功能清單中的頁簽標題轉換
                                else {
                                    sub2Module += " <li id=\"" + program2.ModuleID + "\"  class='layer-three ProgramType' onclick=\"if('" + program2.FilePath + "'!='#')  parent.openPageTab('" + program2.ModuleID + "');if('" + program2.FilePath + "'=='#'){setTreeMenu('" + program2.ModuleID + "');}\"> ";
                                    sub2Module += " <a href='#' data-i18n=common." + program2.ModuleID + "></a>" + sub3Module + "</li> ";
                                }//功能清單中系統管理下層的標題
                            }
                        });
                        sub2Module += '</ul>';

                        if (program1.FilePath === '#') {
                            subModule += " <li id=\"" + program1.ModuleID + "\"  class='layer-two folderType noborder'> ";
                            subModule += " <a data-i18n=common." + program1.ModuleID + "></a>" + sub2Module + "</li>";
                        }//功能清單中系統管理的子層標題轉換
                        else {
                            subModule += " <li id=\"" + program1.ModuleID + "\"  class='layer-two ProgramType' onclick=\"if('" + program1.FilePath + "'!='#')  parent.openPageTab('" + program1.ModuleID + "');if('" + program1.FilePath + "'=='#'){setTreeMenu('" + program1.ModuleID + "');}\"> ";
                            subModule += " <a href='#' data-i18n=common." + program1.ModuleID + "></a>" + sub2Module + "</li>";
                        }//功能清單中我的工作下層的標題
                    }
                });
                subModule += '</ul>';

                if (program.FilePath === '#') {
                    Listli += " <br class='clear'><div class='map-menu-list'><li id=\"" + program.ModuleID + "\"  class='layer-one folderType' > ";
                    Listli += " <a data-i18n=common." + program.ModuleID + "></a>" + subModule + "</li></div>";
                }//功能清單中我的工作及系統管理的轉換
                else {
                    Listli += " <br class='clear'><div class='map-menu-list'><li id=\"" + program.ModuleID + "\"  class='layer-one ProgramType' onclick=\"if('" + program.FilePath + "'!='#')  parent.openPageTab('" + program.ModuleID + "');if('" + program.FilePath + "'=='#'){setTreeMenu('" + program.ModuleID + "');}\"> ";
                    Listli += " <a href='#' data-i18n=common." + program.ModuleID + "></a>" + subModule + "</li></div>";
                }//進出口頁面父層
            }
        });
        Menuli += '</ul>';
        Listli += '</ul>';

        sectionData[0] = Menuli;
        sectionData[1] = Listli;
        return sectionData;
    },
    /**
    * 設定左邊清單點選樣式
    * @param  {String} sModid 模組ID
    */
    setMenuStyle = function (sModid) {
        //先透過foreach迴圈清除所有的onmenu樣式
        $('#sectionMenu ul > li').each(function () {
            $(this).removeClass('onmenu');
        });
        //設定當前點選到的樣式
        $('#sectionMenu').find('#' + sModid).addClass('onmenu');
    },
    /**
    * 模組清單設定
    * @param  {String} sModid 模組ID
    */
    setTreeMenu = function (sModid) {
        var oMenu = getListMenu(sModid);
        $('#sectionMenu').html(oMenu[0]);
        $('#sectionList').html(oMenu[1]).find('.folderType').each(function () {
            var oNext = $(this).next();
            if (oNext.length > 0 && [0].tagName !== 'BR' && oNext[0].className !== 'clear') {
                $(this).after('<br class="clear">');
            }
        });

        refreshLang();
    };

/**
* 開啟畫面
* @param  {String} programId 當前程式ID
* @param  {String} parameters 參數
* @param  {String} title 當前程式名稱
* @return  {Boolean} 是否停止
*/
function openPageTab(programId, parameters, title) {
    var noAuthPrgs = ['NotView', 'Index', 'Test_Calendar'];// ╠common.Index⇒首頁╣

    if (!g_db.GetDic('programList') && noAuthPrgs.indexOf(programId) === -1) window.location.href = '/Page/Login.html';

    var sTitle = "",          //頁簽標題
        sUrl = "",            //網址
        sGrouptag = "default",   //頁簽屬性，相同屬性者共用同一個頁簽
        sOperation = "",
        sTabTemplate = "<li aria-controls='#{controls}' id='li_" + programId + "'><a href='#{href}' role='tab' data-toggle='tab' aria-expanded='true' id='#{id}'><i class='fa fa-times-circle' aria-hidden='true'></i><span data-i18n='{i18nkey}'></span><i class='fa fa-refresh' aria-hidden='true'></i></a></li>",  //頁簽預設內容
        sPara = parameters === undefined ? '' : parameters;

    //抓取各頁面的相關參數
    switch (programId) {
        case "Calendar_Test"://首頁
        case "Index"://首頁
        case "Profile"://個人主頁
        case "NotView"://沒有檢視權限
            if (programId !== 'Profile') {
                sTabTemplate = "<li aria-controls='#{controls}' id='li_" + programId + "'><a href='#{href}' data-i18n='{i18nkey}' role='tab' data-toggle='tab' aria-expanded='true' id='#{id}'></a> </li>";  //沒有刪除和刷新的頁簽
            }
            sUrl = '/Page/' + programId + '.html';
            sTitle = programId === 'NotView' ? 'Index' : 'common.' + programId;
            sGrouptag = programId;
            sPara = ''; //清空參數，因為sUrl已經帶參數了
            break;

        default:

            var saProgramList = g_db.GetDic('programList') || [],
                oProgram = {},
                saCorrects = Enumerable.From(saProgramList).Where(function (item) { return item.ModuleID === programId; }).ToArray();
            if (saCorrects.length > 0) {
                oProgram = saCorrects[0];
            }

            sOperation = oProgram.ModuleID;
            sTitle = title === undefined ? "common." + oProgram.ModuleID : title;
            sUrl = oProgram.FilePath;               //頁簽
            sGrouptag = oProgram.grouptag !== '' ? oProgram.grouptag : sGrouptag;
            break;
    }

    if (!sUrl) {
        showMsg(i18next.t("message.Permissions")); // ╠message.Permissions⇒您沒有檢視權限，請聯繫系統管理員╣
        return false;
    }

    if (programId.indexOf('ngn_') > -1) {
        var link = document.createElement('a');
        link.target = '_blank';
        link.href = sUrl;
        link.click();
        return false;
    }

    var tabs = $("#tabs").tabs();

    var id = "tabs-" + sGrouptag;     //導航Id(程式Id)
    var li = sTabTemplate.replaceAll('#{href}', "#" + id).replaceAll('#{id}', sGrouptag).replaceAll('{i18nkey}', sTitle).replaceAll('#{controls}', id);

    //判斷內容IFrame是否存在，如果不存在就添加
    if (window.ShowMode === 'M') {
        if ($("li[aria-controls=" + id + ']').length === 0) {
            $("#tabsList").append(li); //添加頁簽導航部分
        }
        else {
            var licontrols = $("li[aria-controls=" + id + ']');
            licontrols.attr({ 'id': 'li_' + programId });
            licontrols.find('a span').attr('data-i18n', 'common.' + programId);
        }
    }
    else {
        $("#tabsList").find('li').slice(1).remove();   //如果是單頁簽模式則移除前頁簽
        if ($("li[aria-controls=" + id + ']').length === 0) {
            $("#tabsList").append(li); //添加頁簽導航部分
        }
    }

    if (tabs.find('#' + id).length === 0) {
        var sContentHtml = '<iframe src="' + sUrl + sPara + '" style="width:100%;" onload="javascript:fnframesize(this.id);" id="iframe' + id + '" name="' + (sOperation || "Index") + '" class="tabiframe" frameborder="0" border="0" cellspacing="0" allowtransparency="true" scrolling="yes" />'; //新頁簽的內容部分
        tabs.append("<div id='" + id + "' class='tab-pane fade'>" + sContentHtml + "</div>"); //添加頁簽內容部分
    }

    $('#iframe' + id).attr({ "src": sUrl + sPara, 'name': sOperation });

    tabs.tabs("refresh");   //強迫刷新頁面
    //取得目前的頁簽位置

    var iIndex = $("#tabsList").find('li').length === 0 ? 0 : $("#tabsList").find('li').length - 1;
    SetCuryLiShow(iIndex);

    $("#tabsList").find('#' + sGrouptag).click();  //最後再點選一次，確定在畫面裡面

    //Alt+Backspace鍵，刪除頁簽
    tabs.bind("keyup", function (event) {
        if (event.altKey && event.keyCode === $.ui.keyCode.BACKSPACE) {
            var PrevLi = tabs.find(".ui-tabs-active").prev().find('a'),
                LiId = tabs.find(".ui-tabs-active").remove().attr("aria-controls"),
                sPrgid = tabs.find(".ui-tabs-active")[0].id;
            $("#" + LiId).remove();
            PrevLi.click();
            if (sPrgid.indexOf('_Upd') > -1) {//移除當前操作的程式
                msgs.server.removeEditPrg(sPrgid);
            }
            if (sPrgid.indexOf('_View') > -1) {//移除當前操作的程式
                msgs.server.removeEditPrg(sPrgid);
            }
        }
    });

    //刪除頁簽
    tabs.find(".fa-times-circle").unbind('click').click(function () {
        var PrevLi = $(this).closest("li").prev().find('a'),
            LiId = $(this).closest("li").remove().attr("aria-controls"),
            sPrgid = $(this).closest("li")[0].id;
        $("#" + LiId).remove();
        PrevLi.click();
        if (sPrgid.indexOf('_Upd') > -1) {//移除當前操作的程式
            msgs.server.removeEditPrg(sPrgid.replace('li_', ''));
        }
        if (sPrgid.indexOf('_View') > -1) {//移除當前操作的程式
            msgs.server.removeEditPrg(sPrgid.replace('li_', ''));
        }
    });
    //重新整理頁簽
    tabs.find('.fa-refresh').unbind('click').click(function () {
        var LiId = $(this).closest('li').attr("aria-controls");    //找到頁面ID(請搜尋<li><a href='#{href}' id='#{id}'>)
        var getUrl = $('#iframe' + LiId).attr('src');       //取得目前的連結(src)
        $("#iframe" + LiId).attr('src', getUrl);             //重新載入連結，不等於清除功能
    });

    setContentHeight();
    transLang($('#tabsList'));
}

function setCurryIndex(intAdd) {
    var intCurryIndex = iCuryIndex;
    intCurryIndex = intCurryIndex + intAdd;
    if (intCurryIndex < 0) {
        intCurryIndex = 0;
    }
    if (intCurryIndex >= $("#tabsList").find("li").length - 2) {
        intCurryIndex = $("#tabsList").find("li").length - 2;
    }
    iCuryIndex = intCurryIndex;
    GetLiWidth();
}
function GetLiWidth() {
    $("#tabsList").find("li").each(function (n) {
        if (n <= iCuryIndex && n !== 0 && n !== $("#tabsList").find("li").length - 1) {
            $(this).hide();
            $("#divPrevNext").show();
        } else {
            $(this).show();
        }
    });
    $('#tabsList').css('width', $('#tabpanel')[0].offsetWidth - 130 + 'px');
}
function reSetLi(index, width) {
    this.index = index;
    this.width = width;
    return this;
}
function SetCuryLiShow(intLeftCount) {
    var aryWidth = new Array();
    $("#tabsList").find("li").each(function (n) {
        var li = new reSetLi(n, this.offsetWidth);
        aryWidth.push(li);
    });

    var windowsize = $(window).width() - 100;             //屏幕分辨率
    if (navigator.userAgent.match(/mobile/i)) {
        windowsize = screen.width - 40;
    }
    var _itemwidth = 0;
    var intStartIndex = 0;
    var intEndIndex = intLeftCount;
    for (var i = intLeftCount; i >= 0; i--) {
        _itemwidth += aryWidth[i].width * 1;
        if (_itemwidth > windowsize) {
            intStartIndex = i + 1;
            setCurryIndex(1);
            break;
        }
    }

    $("#tabsList").find("li").each(function (n) {
        if (n >= intStartIndex) {
            $(this).show();
            if (n > intEndIndex) {
                $("#divPrevNext").show();
            }
        } else {
            if (n !== 0 && n !== $("#tabsList").find("li").length - 1) {
                $(this).hide();
                $("#divPrevNext").show();
            }
        }
    });
    iCuryIndex = intStartIndex;
}

/**
* 消息組件初始化
*/
function MsgApp() {
    var init = function () {
        //$.connection.hub.url = gServerUrl + '/signalr';
        var connection = $.connection.hub;
        msgs = $.connection.msgHub;

        connection.logging = true;

        fnFeed();

        //hub连接开启
        connection.start().done(function () {
            fnRegister(true);
            if (window.Outklook) {
                if (!window.OutklookSync) {
                    $('.outlook-waiting').slideDown();
                    outlookAPI(outlook.SynChronous, { flag: "auto" },
                        function (res) {
                            if (res === '1') {
                                window.OutklookSync += 1;
                                g_db.SetItem('outklooksync', window.OutklookSync);
                            }
                        });
                }
            }
            else {
                if (!window.OutlookTips) {
                    setTimeout(function () {
                        // ╠common.OutlookTips⇒當前未同步登入Outlook，操作行事曆可能不會同步或更新╣
                        // ╠common.ReLoad⇒重新登入╣
                        // ╠common.Close⇒關閉╣
                        // ╠common.Or⇒或╣
                        $('.outlook-waiting').html(
                            [
                                i18next.t('common.Tips'),
                                '：',
                                i18next.t('common.OutlookTips'),
                                '   ',
                                $('<a/>', {
                                    text: i18next.t('common.ReLoad'),
                                    class: 'link',
                                    click: function () {
                                        $('.log-out').trigger('click');
                                    }
                                }),
                                i18next.t('common.Or'),
                                $('<a/>', {
                                    text: i18next.t('common.Close'),
                                    class: 'link',
                                    click: function () {
                                        $('.outlook-waiting').slideUp();
                                        window.OutlookTips += 1;
                                        g_db.SetItem('outlooktips', window.OutlookTips);
                                    }
                                })
                            ]).css({ 'font-size': '15px', 'padding-top': '5px', 'color': '#ff0000' }).slideDown();
                    }, 3660);
                }
            }
        })
            .fail(function () {
                console.log("Could not Connect!");
            });

        connection.connectionSlow(function () {
            //console.log("connectionSlow");
        });

        connection.disconnected(function () {
            setTimeout(function () {//掉線後10秒內自動重新連線
                connection.start().done(function () {
                    fnRegister(false);
                });
            }, 1000);
        });

        connection.error(function (error) {
            //console.log(error);
        });

        connection.reconnected(function () {
            //console.log("reconnected");
        });

        connection.reconnecting(function () {
            //console.log("reconnecting");
        });

        connection.starting(function () {
            //console.log("starting");
        });
        connection.stateChanged(function (state) {
            //console.log(state);
        });
    };

    init();
}

function fnFeed() {
    var init = function () {
        //后端登陸註冊调用后，产生的loginUser回调
        msgs.client.onConnected = function (connnectId, userName, onlineUsers) {
            //console.log(JSON.stringify(onlineUsers));
            online_Users = onlineUsers;

            if ($('#iframetabs-Profile').length > 0) {
                $('#iframetabs-Profile')[0].contentWindow.fnSetUserOnline();
            }
        };

        //后端断线时调用调用后，产生的loginUser回调
        msgs.client.onUserDisconnected = function (connnectId, onlineUsers, orgId, userId, userName, isLogin, tips) {
            online_Users = onlineUsers;
            if ($('#iframetabs-Profile').length > 0) {
                $('#iframetabs-Profile')[0].contentWindow.fnSetUserOnline();
            }
            if (isLogin && userId === UserInfo.MemberID) {
                // ╠message.VerifyOutTips⇒您的帳號已在別處登入╣ ╠common.Tips⇒提示╣
                layer.alert(i18next.t("message.VerifyOutTips") + '<br>' + tips, { icon: 0, closeBtn: 0, title: i18next.t("common.Tips") }, function (index) {
                    window.top.location.href = '/Page/Login.html';
                });
            }
        };

        //推送公告
        msgs.client.broadcast = function (msg) {
            fnGetAnnouncements();
        };
        //推送是否可編輯結果（true or false）
        msgs.client.checkedit = function (isedit, prgid, username) {
            if (!isedit) {
                var oCurrentFn = $('iframe[name=' + prgid + ']');
                //oCurrentFn.contents().ready
                // ╠message.NotToEdit⇒當前資料正在編輯，稍後請刷新頁面再繼續操作╣ ╠common.Operator⇒操作人╣
                oCurrentFn[0].contentWindow.showTips(i18next.t("message.NotToEdit") + '，' + i18next.t("common.Operator") + '：' + username);

                parent.bLockDataForm0430 = false;

                var CheckTablelistType = oCurrentFn.contents().find('[role=\'tablist\']').length > 0;
                //tab-content: 多個子頁籤(例如進口編輯)
                if (CheckTablelistType) {
                    oCurrentFn.contents().find(".tab-content").css('pointer-events', 'none');
                }
                //panel-body: 只有一個頁面(例如部門資料編輯)
                else {
                    oCurrentFn.contents().find(".panel-body").css('pointer-events', 'none');
                }

                //toolbar
                oCurrentFn.contents().find('#Toolbar button').not('#Toolbar_Leave').attr('disabled', true);

            }
        };
        //更新提示
        msgs.client.pushtips = function (msg) {
            fnGetTips();
        };
        //推送系統消息
        msgs.client.pushmsgs = function (msg) {
        };
        //檢核文字檔小助手是否安裝
        msgs.client.existtrasfer = function (msg, bInstall) {
            if (bInstall) {
                var sPrgId = '';
                switch (msg) {
                    case 'IM':
                        sPrgId = 'ExhibitionImport';
                        break;
                    case 'EX':
                        sPrgId = 'ExhibitionExport';
                        break;
                }
                $('#iframetabs-' + sPrgId)[0].contentWindow.fnToAccountAudit();
            }
            else {
                layer.msg(i18next.t("message.Financial_InstallTransfer")); // ╠message.Financial_InstallTransfer⇒請先安裝文字檔小助手╣
            }
        };
        //提示安裝並運行文字檔小助手
        msgs.client.transfertips = function (connectionid) {
            layer.msg(i18next.t("message.Financial_InstallTransfer")); // ╠message.Financial_InstallTransfer⇒請先安裝文字檔小助手╣
        };
        //test
        msgs.client.hello = function (msg) {
            //debugger;
        };

        // msgs.server.offline();

        //推送消息
        msgs.client.message = function (data) {
            switch (data.Type) {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:

                    break;
                case 6:

                    break;
                case 7:

                    break;
                case 'OutlookSynChronous':
                    {
                        if (data.Flag === 'auto') {
                            if (data.Message === UserInfo.OutlookAccount) {
                                var elSuccess = $('.outlook-waiting');
                                elSuccess.find('span').text(i18next.t("message.SynchronousSuccess")).css({ 'color': 'green' });
                                elSuccess.find('img').attr('src', '../images/Success.png');
                                setTimeout(function () {
                                    elSuccess.slideUp();
                                }, 2500);
                            }
                        }
                        else if (data.Flag === 'once') {
                            var iframe_Calendar = $('#iframetabs-Calendar');
                            if (iframe_Calendar.length > 0) {
                                iframe_Calendar[0].contentWindow.closeTips(data.Memo);
                            }
                        }
                    }
                    break;
            }
        };

        //接收消息
        msgs.client.receive = function (data) {
            switch (data.Type) {
                case 1://發送給自己
                    break;
                case 2://發送給連線人員

                    break;
                case 3://發送給制定人員ID
                    {
                        switch (data.Memo) {
                            case 'tips'://系統所有待辦定時提醒
                                fnShowTips(data.Content);
                                break;
                            case 'attendance'://EIP考勤提醒
                                fnAttendanceTips(data.Content);
                                break;
                        }
                    }
                    break;
                case 4://發送給制定多個人員ID

                    break;
                case 5://發送給制定群組

                    break;
            }
        };
    };

    init();
}

$(function () {
    'use strict';

    var
        /**
        * 設定系統時間
        */
        Refresh = function () {
            var Nowtime = new Date().formate("HH:mm:ss");
            ltrdate.innerHTML = new Date().formate("yyyy.MM.dd (EEE)");
            ltrtime.innerHTML = Nowtime;
            setTimeout(Refresh, 1000);
        },
        /**
        * 特殊處理，由於階層的關係需要去判斷border-right的顯示，下noborder代表不下樣式
        */
        setTreeViewCss = function () {
            $("#sectionList li:not('.layer-one')").each(function () {
                if ($(this).hasClass('folderType')) {                     //如果該li的class為folderType,在上上層的li加上 class noborder, 加在上上層的原因為folderType的li前面都會加上<br class="clear">的中斷點
                    $(this).prev().prev().addClass('noborder');
                }
            });
        },
        /**
        * 設定頁簽顯示方式
        */
        setShowMode = function () {
            if (window.ShowMode === "M") {    //判斷當前是否開啟多頁簽
                window.ShowMode = "S";
                $("#divPrevNext,#tabpanel").hide();
            } else {
                window.ShowMode = "M";
                $("#divPrevNext,#tabpanel").show();
            }

            CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                Params: {
                    members: {
                        values: { SysShowMode: window.ShowMode },
                        keys: {
                            MemberID: window.UserID,
                            OrgID: window.OrgID
                        }
                    }
                }
            }, function (res) {
                if (res.d > 0) {
                    showMsg(i18next.t("message.SetTab_Successed"), 'success'); // ╠message.Delete_Success⇒設定頁簽模式成功╣
                }
                else {
                    showMsg(i18next.t("message.SetTab_Failed"), 'error'); // ╠message.SetTab_Failed⇒設定頁簽模式失敗╣
                }
            });
        },
        /**
        * 更改語系國別
        * @param {String} lang 當前語言
        */
        fnUpdCountry = function (lang) {
            CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                Params: {
                    members: {
                        values: { Country: lang },
                        keys: {
                            MemberID: window.UserID,
                            OrgID: window.OrgID
                        }
                    }
                }
            }, function (res) { });
        },
        /**
        * 獲取組織資料
        * @return  {Function} ajax物件
        */
        fnGetOrgData = function () {
            return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                Type: '',
                Params: {
                    organization: {
                        OrgID: window.OrgID
                    }
                }
            }).done(function (res) {
                if (res.d) {
                    var oOrg = $.parseJSON(res.d);
                    window.OrgInfo = oOrg;
                    $('title').text(OrgInfo.SystemCName);
                    $('.logo-min').attr('src', '/Controller.ashx?action=getimg&folder=Organization&id=' + oOrg.LoGoId + '&orgid=' + oOrg.OrgID + '&times=' + $.now());
                }
            });
        },
        /**
        * 獲取個人資料
        * @return  {Function} ajax物件
        */
        fnGetPersonalData = function () {
            return $.whenArray([
                g_api.ConnectLite('Authorize', 'GetUserInfo'),
                g_api.ConnectLite(Service.com, ComFn.GetSysSet)])
                .done(function (res1, res2) {
                    window.UserInfo = {};
                    window.SysSet = {};
                    if (res1[0].RESULT > 0) {
                        var oUser = res1[0].DATA.rel;
                        //$('#userName').text(oUser.MemberName);
                        $('<span>', { text: oUser.MemberName }).prependTo("#spUser");
                        window.UserInfo = oUser;
                        UserInfo.OrgID = window.OrgID;
                        UserInfo.roles = UserInfo.roles || '';
                        var sMemberPic = $.trim(UserInfo.MemberPic);
                        UserInfo.MemberPic = sMemberPic === '' ? guid() : sMemberPic;
                        fnSetArgDrop([
                            {
                                ArgClassID: 'LanCountry',
                                Select: $('#countrychange'),
                                ShowId: true,
                                DefultVal: UserInfo.Country,
                                CallBack: function () {
                                    $('#countrychange')[0].remove(0);// 移除下拉選單第一個選項
                                }
                            }
                        ]);
                        fnGetAnnouncements();
                        var msgApp = new MsgApp();
                    }
                    if (res2[0].RESULT > 0) {
                        var saList = res2[0].DATA.rel;
                        $.each(saList, function (i, oSet) {
                            window.SysSet[oSet.SettingItem] = oSet.SettingValue;
                        });
                        window.SysSet.GridRecords = window.SysSet.GridRecords || 10;
                        window.SysSet.GridPages = window.SysSet.GridPages || 15;
                        window.SysSet.CustomersAuditUsers = window.SysSet.CustomersAuditUsers || '';
                        window.SysSet.BillAuditor = window.SysSet.BillAuditor || '';
                        window.SysSet.TaxRate = window.SysSet.TaxRate || '0';
                        window.SysSet.IsOpenMail = window.SysSet.IsOpenMail || 'N';
                        window.SysSet.CDDProUsers = window.SysSet.CDDProUsers || '';
                        UserInfo.IsManager = UserInfo.roles.indexOf(SysSet.Supervisor) > -1 || UserInfo.roles.indexOf('Manager') > -1;
                    }
                    fnGetHeadPic();
                });
        },
        /**
        * 獲取程式資料
        * @return  {Function} ajax物件
        */
        fnGetProgramList = function () {
            return g_api.ConnectLite(Service.sys, 'GetSysFNList', {},
                function (res) {
                    if (res.RESULT === 0) {
                        alert(res.MSG);
                    }
                    else {
                        var sPrgList = res.DATA.rel;
                        g_db.SetDic('programList', sPrgList);
                        if (sPrgList.length > 0) {
                            window.TopModID = sPrgList[0].ModuleID;
                            setTreeMenu(TopModID);     //產生功能清單
                            setTreeViewCss();
                        }
                    }
                });
        },
        /**
        * 獲取頭像
        * @return  {Function} ajax物件
        */
        fnGetHeadPic = function () {
            var callback = function (files) {
                UserInfo.Filelist = files;
                $('#imgUser').attr('src', '/Controller.ashx?action=getimg&folder=Members&id=' + UserInfo.MemberPic + '&orgid=' + UserInfo.OrgID + '&times=' + $.now());
            };
            return fnGetUploadFiles(UserInfo.MemberPic, callback);
        },
        /**
        * 上傳頭像
        * @param {Array} files 當前文件
        * @param {HTMLElement} iframe 父層表單
        */
        fnUpload = function (files, iframe) {
            var option = {};
            option.input = iframe.find('#fileInput');
            option.limit = 1;
            option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
            option.theme = 'dragdropbox';
            option.folder = 'Members';
            option.type = 'one';
            option.parentid = UserInfo.MemberPic;
            option.uploadFile = {
                url: '/Controller.ashx?action=upload&source=Members&userid=' + UserInfo.MemberID + '&orgid=' + UserInfo.OrgID + '&parentid=' + UserInfo.MemberPic,
                data: null,
                type: 'POST',
                enctype: 'multipart/form-data',
                beforeSend: function () { },
                success: function (data, el) {
                    iframe.find('.jFiler-input-dragDrop').hide();
                    var parent = el.find(".jFiler-jProgressBar").parent();
                    fnGetHeadPic();
                    el.find(".jFiler-jProgressBar").fadeOut("slow", function () {
                        $("<div class=\"jFiler-item-others text-success\"><i class=\"icon-jfi-check-circle\"></i> Success</div>").hide().appendTo(parent).fadeIn("slow");
                    });
                },
                error: function (el) {
                    var parent = el.find(".jFiler-jProgressBar").parent();
                    el.find(".jFiler-jProgressBar").fadeOut("slow", function () {
                        $("<div class=\"jFiler-item-others text-error\"><i class=\"icon-jfi-minus-circle\"></i> Error</div>").hide().appendTo(parent).fadeIn("slow");
                    });
                },
                statusCode: null,
                onProgress: null,
                onComplete: null
            };
            option.uploadFile = {
                url: '/Controller.ashx?action=upload&source=Members&userid=' + UserInfo.MemberID + '&orgid=' + UserInfo.OrgID + '&parentid=' + UserInfo.MemberPic,
                data: null,
                type: 'POST',
                enctype: 'multipart/form-data',
                beforeSend: function () { },
                success: function (data, el) {
                    iframe.find('.jFiler-input-dragDrop').hide();
                    var parent = el.find(".jFiler-jProgressBar").parent();
                    fnGetHeadPic();
                    el.find(".jFiler-jProgressBar").fadeOut("slow", function () {
                        $("<div class=\"jFiler-item-others text-success\"><i class=\"icon-jfi-check-circle\"></i> Success</div>").hide().appendTo(parent).fadeIn("slow");
                    });
                },
                error: function (el) {
                    var parent = el.find(".jFiler-jProgressBar").parent();
                    el.find(".jFiler-jProgressBar").fadeOut("slow", function () {
                        $("<div class=\"jFiler-item-others text-error\"><i class=\"icon-jfi-minus-circle\"></i> Error</div>").hide().appendTo(parent).fadeIn("slow");
                    });
                },
                statusCode: null,
                onProgress: null,
                onComplete: null
            };
            option.onRemove = function (itemEl, file) {
                DelFile(UserInfo.MemberPic, 'parent').done(function () {
                    iframe.find('.jFiler-input-dragDrop').show();
                    fnGetHeadPic();
                });
            };
            if (files) {
                option.files = files;
            }
            fnUploadRegister(option);
        },
        /**
        * 初始化
        */
        init = function () {
            if (!g_db.SupportLocalStorage()) {
                if (sLang === 'en') {
                    alert('The current browser does not support local storage. Please turn off private browsing settings');
                }
                else if (sLang === 'zh') {
                    alert('当前浏览器不支持本地储存，请关闭无痕浏览模式');
                }
                else {
                    alert('當前瀏覽器不支持本地儲存，請關閉私密瀏覽設定');
                }
                $('body').html('');
                return;
            }
            window.OrgID = g_db.GetItem('orgid');
            window.UserID = g_db.GetItem('userid');
            window.ShowMode = g_db.GetItem('mode');
            window.Outklook = g_db.GetItem('outklook') === 'true';
            window.OutklookSync = parseInt(g_db.GetItem('outklooksync') || 0);
            window.OutlookTips = parseInt(g_db.GetItem('outlooktips') || 0);

            if (!window.OrgID || !window.UserID) {
                window.location.href = '/Page/Login.html';
                return;
            }

            fnGetOrgData();
            $.whenArray([
                fnGetPersonalData(),
                fnGetProgramList()
            ]).done(function () {
                var saProgramList = g_db.GetDic('programList') || [];
                if (saProgramList.length) {
                    setLang(UserInfo.Country, undefined, function () {
                        openPageTab('Index', 'Index');
                    });
                }
                else {
                    openPageTab('NotView', 'NotView');
                    return false;
                }
                fnGetAbsenceFromLastWeek();
            });
            fnGetTips();

            // Slimscroll
            slimScroll();
            Waves.displayEffect();//波浪

            //$('#divPenlSetting').click(function () {
            //    setShowMode();
            //});

            if (window.ShowMode === "S") {    //判斷當前是否開啟多頁簽
                $("#divPrevNext,#tabpanel").hide();
            }

            $('.stop-prevent').on('click', function (e) {
                return false;
            });

            ///登陸outlook
            //$('.mail-box').on('click', function (e) {
            //    if (!UserInfo.OutLookId) {
            //        window.location.href = "/Home/Index";
            //        return false;
            //    }
            //});

            $('.log-out').on('click', function (e) {
                var fnClear = function () {
                    g_db.RemoveItem('orgid');
                    g_db.RemoveItem('userid');
                    g_db.RemoveItem('loginname');
                    g_db.RemoveItem('usertype');
                    g_db.RemoveItem('mode');
                    g_db.RemoveItem('token');
                    g_db.RemoveItem('outklook');
                };
                fnClear();
                if (window.Outklook) {
                    window.location.href = "/Login/Index?orgid=&userid=";
                }
                else {
                    window.location.href = '/Page/Login.html';
                }

                // ╠message.ToLogOut⇒是否同時退出Outlook？╣ ╠common.Tips⇒提示╣
                /*layer.confirm(i18next.t("message.ToLogOut"),
                    {
                        icon: 3,
                        title: i18next.t("common.Tips"),
                        btn: [i18next.t('common.Yes'), i18next.t('common.No')] // ╠message.Yes⇒是╣ ╠common.No⇒否╣
                    },
                    function (index) {
                        fnClear();
                        window.location.href = "/Login/Index?orgid=&userid=";
                        layer.close(index);
                    },
                    function () {
                        fnClear();
                        window.location.href = '/Page/Login.html';
                    }
                );*/
            });

            ///左移
            $("#lbtnPrev").on('click', function () {
                setCurryIndex(-1);
            });
            ///右移
            $("#lbtnNext").on('click', function () {
                setCurryIndex(1);
            });

            /**
            * 行事曆
            */
            $(".person-list>li>a").on('click', function () {
                var that = this;
                switch (that.id) {
                    case 'profile':
                        openPageTab('Profile');
                        break;
                    case 'userinfo':
                        var oValidator = null;
                        layer.open({
                            id: 'memberinfo',
                            type: 2,
                            title: i18next.t('common.PersonalDataUpdate'),//╠common.PersonalDataUpdate⇒個人資料修改╣
                            offset: '100px',//右下角弹出
                            shade: 0.75,
                            area: ['660px', '600px'],
                            content: '/Page/Pop/UpdUserInfo.html', //iframe的url,
                            btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                            success: function (layero, index) {
                                var iframe = $('iframe').contents();
                                iframe.find('#MemberID').val(UserInfo.MemberID);
                                iframe.find('#MemberName').val(UserInfo.MemberName);

                                //自定義驗證屬性
                                $.validator.addMethod("notEqualTo", function (value, element, param) {
                                    if (value === '' && iframe.find(param).val() === '') {
                                        return true;
                                    }
                                    return value !== iframe.find(param).val();
                                });
                                $.validator.addMethod("EqualToNew", function (value, element, param) {
                                    if (value === '' && iframe.find(param).val() === '') {
                                        return true;
                                    }
                                    return value === iframe.find(param).val();
                                });
                                $.validator.addMethod("New_required", function (value, element, param) {
                                    if (value === '' && iframe.find(param).val() !== '') {
                                        return false;
                                    }
                                    return true;
                                });

                                oValidator = iframe.find("#form_UpdUserInfo").validate({ //表單欄位驗證
                                    rules: {
                                        OldPsw: { New_required: "#NewPsw" },
                                        NewPsw: { notEqualTo: "#OldPsw" },
                                        CheckNewPsw: {
                                            EqualToNew: "#NewPsw"
                                        }
                                    },
                                    messages: {
                                        MemberName: i18next.t('UpdUserInfo.MemberName_required'),// ╠UpdUserInfo.MemberName_required⇒請輸入名稱╣
                                        OldPsw: {
                                            New_required: i18next.t('UpdUserInfo.OldPsw_required')// ╠UpdUserInfo.OldPsw_required⇒請輸入舊密碼╣
                                        },
                                        CalColor: i18next.t('UpdUserInfo.CalColor_required'),// ╠UpdUserInfo.CalColor_required⇒請輸入行事曆顏色╣
                                        NewPsw: {
                                            required: i18next.t('UpdUserInfo.NewPsw_required'),// ╠UpdUserInfo.NewPsw_required⇒請輸入新密碼╣
                                            notEqualTo: i18next.t('UpdUserInfo.NotEqualTo')// ╠UpdUserInfo.NotEqualTo⇒舊密碼與新密碼不可相同╣
                                        },//舊密碼與新密碼不可相同
                                        CheckNewPsw: {
                                            required: i18next.t('UpdUserInfo.CheckNewPsw_required'),// ╠UpdUserInfo.CheckNewPsw_required⇒再次輸入新密碼╣
                                            EqualToNew: i18next.t('UpdUserInfo.EqualTo') // ╠UpdUserInfo.EqualTo⇒兩次密碼輸入不相符╣
                                        }
                                    }
                                });
                                fnUpload(UserInfo.Filelist, iframe);
                                if (UserInfo.Filelist.length > 0) {
                                    iframe.find('.jFiler-input-dragDrop').hide();
                                }
                                transLang(iframe.find('#form_UpdUserInfo'));
                            },
                            yes: function (index, layero) {
                                var iframe = $('iframe').contents();
                                if (!iframe.find("#form_UpdUserInfo").valid()) {
                                    oValidator.focusInvalid();
                                    return false;
                                }
                                var data = {
                                    UserName: iframe.find('#MemberName').val(),
                                    OldPsw: iframe.find('#OldPsw').val(),
                                    NewPsw: iframe.find('#NewPsw').val(),
                                    CalColor: iframe.find('#CalColor').val(),
                                    MemberPic: UserInfo.MemberPic
                                };
                                g_api.ConnectLite(Service.auth, 'UpdataPsw', data, function (res) {
                                    if (res.RESULT) {
                                        UserInfo.CalColor = data.CalColor;
                                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                                        layer.close(index);
                                    }
                                    else {
                                        if (res.MSG === "1") {
                                            showMsg(i18next.t("message.CheckOldPassword"), 'error');// ╠message.CheckOldPassword⇒舊密碼驗證失敗╣
                                        }
                                        else if (res.MSG === "2") {
                                            showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                                        }
                                        else {
                                            showMsg(i18next.t("message.Modify_Failed") + '<br>' + res.MSG, 'error'); //╠message.Modify_Failed⇒修改失敗╣
                                        }
                                    }
                                }, function () {
                                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                                });
                            }
                        });
                        break;
                    case 'calendar':
                        openPageTab('Calendar');
                        break;
                }
            });

            /**
            * 語系國別設定
            */
            $('#countrychange').on('change', function () {
                $('#setlistset').click();

                var sLang = $(this).val();
                fnUpdCountry(sLang);
                setLang(sLang);

                $('#tabsList>li').each(function () {
                    var sId = $(this).attr('aria-controls'),
                        iframe = $('#' + sId).find('iframe').contents();
                    setLang(sLang, iframe);
                });
            });

            setTimeout(Refresh, 1000);
            //closeWaiting(3000); //最長3秒鐘停止等待

            //var sFileName = "John_Test",
            //    sInputPath = "C:\Users\Alina\Desktop\Temple\OfficeToPDF\Demo\土地建物分離估價適用版.xlsx";
            //g_api.ConnectLite('Pdf', 'ExcelToPdf', {
            //    InputPath: sInputPath,
            //    FileName: sFileName
            //}, function (res) {
            //    if (res.RESULT) {
            //        DownLoadFile(res.DATA.rel, sFileName);
            //    }
            //    else {
            //        showMsg(res.MSG, 'error');
            //    }
            //});

            //debugger;


        };

    init();
});