var outlook = {
    Test: "Test",
    SynChronous: "SynChronous",
    GetUserEmail: "GetUserEmail",
    ContactFolders: "ContactFolders",
    Contacts: "Contacts",
    Inbox_Qry: "InboxQry",
    Inbox_Add: "InboxAdd",
    Inbox_Upd: "InboxUpd",
    Inbox_Del: "InboxDel",
    Calendar_Qry: "CalendarQry",
    Calendar_Add: "CalendarAdd",
    Calendar_Upd: "CalendarUpd",
    Calendar_Del: "CalendarDel",

    CalendarInfo: "CalendarInfo"
},
    outlookAPI = function (action, param, calllback, isasync) {
        if (parent.top.SysSet.OutlookRun !== 'Y' && 'CalendarAdd,CalendarUpd,CalendarDel'.indexOf(action) > -1) {
            showMsg(i18next.t("message.CheckToOpenOutlookRun")); //╠message.CheckToOpenOutlookRun⇒同步更新Outlook失敗<br/>請聯絡管理員開啟同步Outlook設定項╣
            return false;
        }
        $.ajax({
            type: 'POST',
            url: '/Login/' + action + (!isasync ? 'Async' : ''),
            data: { data: JSON.stringify(param || {}) },
            success: function (res) {
                if (typeof calllback === 'function') { calllback(res); }
            },
            error: function (xhr, status, error) {
            }
        });
    };