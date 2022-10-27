/**
* 函數名稱：
* 目的：彈出帶一個按鈕的提示窗口並執行方法
* @param {String} msg     ：提示訊息
* @param {String} title   ：提示窗口的標題success....
* @param {String} Position：提示窗口的位置
* @param {String} type :提示方式success、 Info、Warning、Error四種
* @param {String} msg              :提示框中顯示的訊息
* @param {String} title            :提示框顯示的其他html內容
* @param {String} showDuration     :显示时间
* @param {String} hideDuration     :隐藏时间
* @param {String} timeOut          :超时
* @param {String} extendedTimeOut  :延长超时
* @param {String} showEasing       :
* @param {String} hideEasing       :
* @param {String} showMethod       :顯示的方式
* @param {String} hideMethod       :隱藏的方式
* @param {String} addClear         :是否添加清除
* @param {String} closeButton      :是否添加closeButton
* @param {String} debug            :
* @param {String} newestOnTop      :
* @param {String} progressBar      :是否添加進度顯示
* @param {String} positionClass    :位置
* @param {String} preventDuplicates:是否防止重复
* @param {Function} func           :執行方法
*/
function getMsgBox(type, msg, title, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, ResetButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk, fnCl) {
    var title = title || i18next.t("common.Tips") || '<span data-i18n="common.Tips"></span>';
    toastr.options = {
        closeButton: closeButton,
        ResetButton: ResetButton,
        debug: debug,
        newestOnTop: newestOnTop,
        progressBar: progressBar,
        positionClass: positionClass || 'toast-top-center',
        preventDuplicates: preventDuplicates,
        onclick: null
    };
    if (addBehaviorOnToastClick) {
        toastr.options.onclick = BehaviorFunc;
    }
    if (showDuration) {
        toastr.options.showDuration = showDuration;
    }

    if (toastr.options.ResetButton && toastr.options.closeButton) {
        msg = '<div>' + msg + '</div><div style="float:right"><button type="button" id="okBtn" class="btn btn-success" data-i18n="common.Confirm">確認</button><button type="button" id="surpriseBtn" class="btn btn-primary" style="margin: 0 8px 0 8px" data-i18n="common.Cancel">取消</button></div>';
    }
    else if (!toastr.options.ResetButton && toastr.options.closeButton) {//只存在確認按鈕
        msg += '<div style="float:right"><button type="button" id="okBtn" class="btn btn-info" data-i18n="common.Confirm">確認</button></div>';
    }
    if (hideDuration) {
        toastr.options.hideDuration = hideDuration;
    }

    if (timeOut) {
        toastr.options.timeOut = timeOut;
        //setTimeout(BehaviorFunc, timeOut)
    }

    if (extendedTimeOut) {
        toastr.options.extendedTimeOut = extendedTimeOut;
    }

    if (showEasing) {
        toastr.options.showEasing = showEasing;
    }

    if (hideEasing) {
        toastr.options.hideEasing = hideEasing;
    }

    if (showMethod) {
        toastr.options.showMethod = showMethod;
    }

    if (hideMethod) {
        toastr.options.hideMethod = hideMethod;
    }
    if (addClear) {//是否清空之前樣式
        toastr.options.tapToDismiss = false;
    }

    var $toast = toastr[type](msg, title);

    if (($toast) && $toast.find('#okBtn').length) {
        $toast.delegate('#okBtn', 'click',
            function () {
                toastr.clear();
                $(".toast-close-button").click();
                if (fnOk) {//如果方法存在就執行
                    var func = fnOk;
                    func();
                }
                return true;
            }

        );

        transLang($('#okBtn'));
    }
    if (($toast) && $toast.find('#surpriseBtn').length) {
        $toast.delegate('#surpriseBtn', 'click',
            function () {
                toastr.clear();
                $(".toast-close-button").click();
                if (fnCl) {//如果方法存在就執行
                    var func = fnCl;
                    func();
                }
                return false;
            }

        );
    }
}

/**
* 函數名稱：
* 目的：彈出提示窗口並執行方法
* @param {String} msg     ：提示訊息
* @param {String} type    ：提示窗口的標題success....
* @param {Function} func  ：執行方法
* @param {String} position：提示窗口的位置
*/
function msgAndGo(msg, type, func, position) {
    var title, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk, fnCl;

    showDuration = "300";
    hideDuration = "1000";
    timeOut = '0';
    extendedTimeOut = '0';
    showEasing = "swing";
    hideEasing = "linear";
    showMethod = "fadeIn";
    hideMethod = "fadeOut";
    addClear = true;
    closeButton = true;
    debug = false;
    newestOnTop = true;
    progressBar = false;
    positionClass = position;
    preventDuplicates = false;
    addBehaviorOnToastClick = true;
    BehaviorFunc = func;
    fnOk = null;
    fnCl = null;
    ResetButton = false;
    type = type || 'success';
    getMsgBox(type, msg, title, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, ResetButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk, fnCl);
}
var
    /**
    * 函數名稱：
    * 目的：顯示提示訊息，替代ALERT功能
    * @param {String} msg     ：提示訊息
    * @param {String} type    ：提示窗口的標題success....
    * @param {String} position：提示窗口的位置
    * @param {String} title   ：標題
    */
    showMsg = function (msg, type, position, title) {
        var showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk;

        showDuration = "300";
        hideDuration = "1000";
        timeOut = "3000";
        extendedTimeOut = "1000";
        showEasing = "swing";
        hideEasing = "linear";
        showMethod = "fadeIn";
        hideMethod = "fadeOut";
        addClear = false;
        closeButton = false;
        debug = false;
        newestOnTop = true;
        progressBar = false;
        positionClass = position;
        preventDuplicates = true;
        addBehaviorOnToastClick = true;
        BehaviorFunc = null;
        fnOk = null;
        fnCl = null;
        ResetButton = false;
        type = type || 'info';
        getMsgBox(type, msg, title, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, ResetButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk, fnCl);
    },
    /**
    * 函數名稱：
    * 目的：顯示提示訊息，替代ALERT功能
    * @param {String} msg     ：提示訊息
    * @param {String} url     ：轉向地址
    * @param {String} param   ：參數
    * @param {String} type    ：提示窗口的標題success....
    * @param {String} position：提示窗口的位置
    */
    showMsgAndGo = function (msg, url, param, type, position) {
        if (url != "") {//如果有提供跳轉畫面的url就執行跳轉頁面的動作
            msgAndGo(msg, type, function () { parent.openPageTab(url, param) }, position);
        }
    },
    /**
    * 函數名稱：
    * 目的：顯示提示訊息，替代ALERT功能
    * @param {String} msg     ：提示訊息
    * @param {String} param   ：參數
    * @param {String} type    ：提示窗口的標題success....
    * @param {String} position：提示窗口的位置
    */
    showTips = function (msg, type, position, title) {
        var showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk;

        showDuration = "300";
        hideDuration = "1000";
        timeOut = '0';
        extendedTimeOut = '0';
        showEasing = "swing";
        hideEasing = "swing";
        showMethod = "fadeIn";
        hideMethod = "fadeOut";
        addClear = true;
        closeButton = false;
        debug = false;
        newestOnTop = false;
        progressBar = false;
        positionClass = 'toast-edit-center';
        preventDuplicates = false;
        addBehaviorOnToastClick = false;
        BehaviorFunc = null;
        fnOk = null;
        fnCl = null;
        ResetButton = false;
        type = type || 'info';
        getMsgBox(type, msg, title, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, closeButton, ResetButton, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc, fnOk, fnCl);
    };