/*global $, alert, g_cus, g_de, g_api, g_db, g_ul,  btoa, console, i18n */

var i18next = ("undefined" === typeof i18next) ? parent.top.i18next : i18next,
    g_db = {
        /**
         * Check the capability
         * @private
         * @method SupportLocalStorage
         * @return {Object} description
         */
        SupportLocalStorage: function () {
            'use strict';
            return typeof (localStorage) !== "undefined";
        },

        /**
         * Insert data
         * @private
         * @method SetItem
         * @param {Object} sKey
         * @param {Object} sValue
         * @return {Object} description
         */
        SetItem: function (sKey, sValue) {
            'use strict';
            var bRes = false;

            if (this.SupportLocalStorage()) {
                localStorage.setItem(sKey, sValue);
                bRes = true;
            }
            return bRes;
        },

        /**
         * Fetch data
         * @private
         * @method GetItem
         * @param {Object} sKey
         * @return {Object} description
         */
        GetItem: function (sKey) {
            'use strict';
            var sRes = null;

            if (this.SupportLocalStorage()) {
                sRes = localStorage.getItem(sKey);
            }

            return sRes;
        },

        /**
         * Remove data
         * @private
         * @method RemoveItem
         * @param {Object} sKey
         * @return {Object} description
         */
        RemoveItem: function (sKey) {
            'use strict';
            var bRes = false;

            if (this.SupportLocalStorage()) {
                localStorage.removeItem(sKey);
                bRes = true;
            }

            return bRes;
        },

        /**
         * Description for GetDic
         * @private
         * @method GetDic
         * @return {Object} description
         */
        GetDic: function (sKey) {
            'use strict';
            var dicRes = null,
                vTemp;

            if (this.SupportLocalStorage()) {
                vTemp = localStorage.getItem(sKey);
                if (null !== vTemp) {
                    dicRes = JSON.parse(vTemp);
                }
            }

            return dicRes;
        },

        /**
         * Description for SetDic
         * @private
         * @method SetDic
         * @return {Object} description
         */
        SetDic: function (sKey, dicValue) {
            'use strict';
            var bRes = false;

            if (this.SupportLocalStorage()) {
                localStorage.setItem(sKey, JSON.stringify(dicValue));
                bRes = true;
            }
            return bRes;
        }
    };

var g_gd = {
    webapilonginurl: "/api/Service/GetLogin",
    webapiurl: "/api/Cmd/GetData",
    projectname: "Eurotran",
    projectver: "Origtek",
    relpath: "",
    debugmode: window.location.host === '192.168.1.105',
    debugcolor: "#732C6B",
    IsEDU: g_db.GetItem("isedu") === "true"
};

var g_ul = {
    /**
     * Get token from db
     * @returns {String} Token in localStorage
     */
    GetToken: function () {
        'use strict';
        return g_db.GetItem("token");
    },

    /**
     * Set token to db
     * @param   {String} sTokenValue api token
     */
    SetToken: function (sTokenValue) {
        'use strict';
        g_db.SetItem("token", sTokenValue);
    },

    /**
     * Set signature to db
     * @param   {String} sSignatureValue api signature
     */
    GetSignature: function () {
        'use strict';
        return g_db.GetItem("signature");
    },

    /**
     * Set signature to db
     * @param   {String} sSignatureValue api signature
     */
    SetSignature: function (sSignatureValue) {
        'use strict';
        g_db.SetItem("signature", sSignatureValue);
    },

    /**
     * Set language
     * @param   {String} language method
     */
    SetLang: function (sLang) {
        'use strict';
        g_db.SetItem("lang", sLang);
    },

    /**
     * Get language
     * @returns {String} language in localStorage
     */
    GetLang: function () {
        'use strict';
        return g_db.GetItem("lang");
    },

    /**
     * Set login method
     * @param {String} login method
     */
    SetLoginMethod: function (sLoginMethod) {
        'use strict';
        g_db.SetItem("LoginMethod", sLoginMethod);
    },

    /**
     * Get login method
     * @returns {String} login method in localStorage
     */
    GetLoginMethod: function () {
        'use strict';
        return g_db.GetItem("LoginMethod");
    },

    /**
     * Check is edu environment
     * @returns {String} login method in localStorage
     */
    IsEDU: function () {
        'use strict';
        return g_db.GetItem("isedu");
    },

    /**
     * Get trace stack
     */
    TraceStackDump: function () {
        var dicTraceStackData = {},
            dicTraceStackDataLevel = {},
            ldicTraceStackDataList = [],
            curArguments = null,
            iLevel = 0,
            iIdx = 0,
            iLim = 0,
            oPara = null,
            bFindButtonEvent = false;

        curArguments = arguments;

        do {
            dicTraceStackDataLevel = {};
            dicTraceStackDataLevel.name = curArguments.callee.name;
            dicTraceStackDataLevel.parameters = [];
            iLim = curArguments.length;

            if (iLim > 0) {
                oPara = curArguments["0"];

                if (typeof (oPara) === "object" && oPara.hasOwnProperty("currentTarget")) {
                    bFindButtonEvent = true;
                    dicTraceStackDataLevel.buttonclick = oPara.currentTarget.id;
                }
                else {
                    for (iIdx = 0; iIdx < iLim; iIdx++) {
                        oPara = curArguments[iIdx.toString()];
                        dicTraceStackDataLevel.parameters.push(curArguments[iIdx.toString()]);
                    }
                }
            }

            if (iLevel > 0) {
                ldicTraceStackDataList.push($.extend({}, dicTraceStackDataLevel));
            }

            if (curArguments.callee.caller === null) {
                break;
            }

            curArguments = curArguments.callee.caller.arguments;
            iLevel = iLevel + 1;
        }
        while (bFindButtonEvent === false);

        dicTraceStackData.stack = ldicTraceStackDataList;

        return dicTraceStackData;
    }
};

var g_api = {
    ConnectLite: function (i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn, i_sShwd) {
        window.IsWaiting = i_sShwd;
        return this.ConnectLiteWithoutToken(i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn);
    },
    ConnectService: function (i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn, i_sShwd) {
        window.IsWaiting = i_sShwd;
        return this.ConnectWebLiteWithoutToken(i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn);
    },

    ConnectLiteWithoutToken: function (i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn) {
        var dicData = {},
            dicParameters = {},
            token = g_ul.GetToken(),
            lang = g_ul.GetLang(),
            signature = g_ul.GetSignature();
        dicParameters.ORIGID = g_db.GetItem('orgid');
        dicParameters.USERID = g_db.GetItem('userid');
        dicParameters.MODULE = i_sModuleName;
        dicParameters.TYPE = i_sFuncName;
        dicParameters.PROJECT = g_gd.projectname;
        dicParameters.PROJECTVER = g_gd.projectver;
        dicParameters.TRACEDUMP = null; // g_ul.TraceStackDump();

        i_dicData = i_dicData || {};
        if (g_db.GetItem('dblockDict') !== null) {
            i_dicData.dblockDict = g_db.GetItem('dblockDict');
        }
        dicParameters.DATA = i_dicData;

        if (lang !== null) {
            dicParameters.LANG = lang;
        }

        if (token !== null) {
            dicParameters.TOKEN = token;
        }

        if (signature !== null) {
            dicParameters.SIGNATURE = signature;
        }

        dicParameters.CUSTOMDATA = {};

        if (window.sProgramId) {
            dicParameters.CUSTOMDATA.program_id = sProgramId;
        }

        dicParameters.CUSTOMDATA.module_id = "webapp";

        dicData.url = i_dicData.hasOwnProperty("url") ? i_dicData.url : g_gd.webapiurl;
        dicData.successfunc = i_sSuccessFunc;
        dicData.dicparameters = dicParameters;

        dicData.failfunc = ("function" === typeof (i_FailFunc)) ? i_FailFunc : function (jqXHR, textStatus, errorThrown) {
            alert("ConnectLite Fail jqXHR:" + jqXHR + " textStatus:" + textStatus + " errorThrown:" + errorThrown);
        };

        dicData.useasync = ("boolean" === typeof (i_bAsyn)) ? i_bAsyn : true;
        return this.AjaxPost(dicData);
    },

    //w.CallAjax = function (url, fnname, data, sucfn, failfn, wait, async, alwaysfn) {
    ConnectWebLiteWithoutToken: function (i_sUrl, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn) {
        var dicData = {},
            dicParameters = {},
            token = g_ul.GetToken(),
            lang = g_ul.GetLang(),
            signature = g_ul.GetSignature();
        dicParameters.ORIGID = g_db.GetItem('orgid');
        dicParameters.USERID = g_db.GetItem('userid');
        dicParameters.MODULE = '';
        dicParameters.TYPE = i_sFuncName;
        dicParameters.PROJECT = g_gd.projectname;
        dicParameters.PROJECTVER = g_gd.projectver;
        dicParameters.TRACEDUMP = null; // g_ul.TraceStackDump();

        if (g_db.GetItem('dblockDict') !== null) {
            i_dicData.dblockDict = g_db.GetItem('dblockDict');
        }
        dicParameters.DATA = i_dicData;

        if (lang !== null) {
            dicParameters.LANG = lang;
        }

        if (token !== null) {
            dicParameters.TOKEN = token;
        }

        if (signature !== null) {
            dicParameters.SIGNATURE = signature;
        }

        dicParameters.CUSTOMDATA = {};

        if (window.sProgramId) {
            dicParameters.CUSTOMDATA.program_id = sProgramId;
        }

        dicParameters.CUSTOMDATA.module_id = "webapp";

        dicData.url = getWebServiceUrl(i_sUrl, i_sFuncName);
        dicData.successfunc = i_sSuccessFunc;
        dicData.dicparameters = dicParameters;

        dicData.failfunc = ("function" === typeof (i_FailFunc)) ? i_FailFunc : function (jqXHR, textStatus, errorThrown) {
            alert("ConnectLite Fail jqXHR:" + jqXHR + " textStatus:" + textStatus + " errorThrown:" + errorThrown);
        };

        dicData.useasync = ("boolean" === typeof (i_bAsyn)) ? i_bAsyn : true;
        return this.AjaxPost(dicData);
    },

    AjaxPost: function (i_dicData) {
        'use strict';
        var defaultOption = {
            useasync: true,
            successfunc: null,
            failfunc: null,
            alwaysfunc: null,
            url: null,
            dicparameters: null
        },
            runOption = $.extend(defaultOption, i_dicData),
            runSuccess = function (res) {
                if (res.RESULT === -1) { // ╠message.TokenVerifyFailed⇒您的身份認證已經過期，請重新登入╣ ╠common.Tips⇒提示╣
                    layer.alert(i18next.t("message.TokenVerifyFailed"), { icon: 0, title: i18next.t("common.Tips") }, function (index) {
                        window.top.location.href = '/Page/login.html';
                    });
                }
                else {
                    if (runOption.successfunc) {
                        runOption.successfunc(res);
                    }
                }
            };

        return $.ajax({
            type: 'POST',
            url: runOption.url,
            data: "=" + btoa2(encodeURIComponent(JSON.stringify(runOption.dicparameters))),
            success: runSuccess,
            error: runOption.failfunc,
            beforeSend: function (xhr) {
                var token = g_ul.GetToken(),
                    timestamp = $.now(),
                    nonce = rndnum();
                xhr.setRequestHeader("orgid", runOption.dicparameters.ORIGID);
                xhr.setRequestHeader("userid", runOption.dicparameters.USERID);
                xhr.setRequestHeader("token", token);
                xhr.setRequestHeader("timestamp", timestamp);
                xhr.setRequestHeader("nonce", nonce);
            },
            async: true !== runOption.useasync ? false : true
        }).always(runOption.alwaysfunc);
    }
};

(function ($, w, d) {
    /**
     * 取得host
     */
    w.gServerUrl = w.location.origin || gethost();

    /**
     * 定義系統所有HTML模版
     */
    w.ComTmp = {
        PageTitle: '/Page/Pop/PageTitle.html',
    };

    /**
     * 定義系統所有公用 Service.fnction
     */
    w.ComFn = {
        W_Com: 'comw',
        W_Web: 'web',
        GetUserList: 'GetUserList',
        GetArguments: 'GetArguments',
        GetSysSet: 'GetSysSet',
        GetSerial: 'GetSerialNumber',
        GetUpdateOrder: 'UpdateOrderByValue',
        SendMail: 'SendMail',
        GetExcel: 'CreateExcel',
        GetList: 'QueryList',
        GetOne: 'QueryOne',
        GetPage: 'QueryPage',
        GetPagePrc: 'QueryPageByPrc',
        GetAdd: 'Add',
        GetUpd: 'Update',
        GetDel: 'Delete',
        GetTran: 'UpdateTran',
        GetCount: 'QueryCount',
        CheckInvoiceNum: 'CheckInvoiceNumber'
    };

    /**
     * 定義系統所有公用 Service
     */
    w.Service = {
        cotrl: '/Controller.ashx',
        comw: 'ComWebService',
        web: 'WebService',
        com: 'Common',
        opm: 'OpmCom',
        eip: 'EipCom',
        sys: 'SysCom',
        auth: 'Authorize'
    };

    /**
     * 定義頁面離開是否需要儲存
     */
    w.bRequestStorage = false;
    w.bLeavePage = false;

    /**
     * 定義查詢頁面默認顯示第幾頁
     */
    w.QueryPageidx = 1;

    /**
     * Ajax是否等待
     */
    w.IsWaiting = null;

    /**
     * For display javascript exception on UI
     */
    w.onerror = function (message, source, lineno, colno, error) {
        console.log(source + " line:" + lineno + " colno:" + colno + " " + message);
        if (parent.top.SysSet && parent.top.SysSet.IsOpenMail === 'Y') {
            g_api.ConnectLite('Log', 'ErrorMessage', {
                ErrorSource: source,
                Errorlineno: lineno,
                Errorcolno: colno,
                ErrorMessage: message
            }, function (res) {
            });
        }
    };

    /**
     * 翻譯語系
     * @param {HTMLElement} dom  翻譯回調函數
     */
    w.transLang = function (dom) {
        i18next = ("undefined" === typeof i18next) ? parent.top.i18next : i18next;

        var oHandleData = dom === undefined ? $('[data-i18n]') : dom.find('[data-i18n]'),
            oHandlePlaceholder = dom === undefined ? $('[placeholderid]') : dom.find('[placeholderid]');
        oHandleData.each(function (idx, el) {
            var i18key = $(el).attr('data-i18n');
            if (i18key) {
                var sLan = i18next.t(i18key);
                if (el.nodeName == 'INPUT' && el.type == 'button') {
                    $(el).val(sLan);
                }
                else {
                    $(el).html(sLan);
                }
            }
        });
        oHandlePlaceholder.each(function (idx, el) {
            var i18key = $(el).attr("placeholderid");
            if (i18key) {
                var sLan = i18next.t(i18key);
                if (sLan !== i18key) {
                    $(el).attr("placeholder", sLan);
                }
            }
        });
    };

    /**
     * 翻譯語系
     * @param {HTMLElement} dom 要翻譯的html標籤
     */
    w.refreshLang = function (dom) {
        if (dom && dom.length === 0) {
            return false;
        }

        transLang(dom);
    };

    /**
     * 設定多於系
     * @param {String} lng 語種
     * @param {String} dom 要翻譯的html標籤
     * @param {Function} callback 回調函數
     */
    w.setLang = function (lng, dom, callback) {
        if (!lng) return;

        g_ul.SetLang(lng);

        i18next = ("undefined" == typeof i18next) ? parent.top.i18next : i18next;

        $.getJSON(gServerUrl + "/Scripts/lang/" + (parent.top.OrgID || 'TE') + "/" + lng + ".json?v=20180801", function (json) {
            var oResources = {};

            oResources[lng] = {
                translation: json
            };

            i18next.init({
                lng: lng,
                resources: oResources,
                useLocalStorage: false,               //是否将语言包存储在localstorage
                //ns: { namespaces: ['trans'], defaultNs: 'trans' },             //加载的语言包
                localStorageExpirationTime: 86400000        // 有效周期，单位ms。默认1
            }, function (err, t) {
                transLang(dom);
                if (typeof callback === 'function') {
                    callback(t);
                }
            });
        });
    };

    /**
     * getLanguagePack
     */
    w.getLanguagePack = function (sMsgKey) {
        var sLang = g_ul.GetLang() || 'en';
        return i18next.getResourceBundle(sLang, 'translation').sMsgKey || sMsgKey;
    };

    /**
     * 獲取WebService路勁
     * @param {String} type  Service類型
     * @param {String} fnname function 名稱
     */
    w.getWebServiceUrl = function (type, fnname) {
        var sUrl = '';

        switch (type) {
            case 'aspx':
                sUrl = fnname;
                break;
            case 'cotrl':
                sUrl = '/Controller.ashx?' + fnname;
                break;
            default:
                sUrl = '/WS/' + Service[type] + '.asmx/' + fnname;
                break;
        }
        return sUrl;
    };
    /**
     * 呼叫Ajax
     * @param  {String} urlaspx 地址
     * @param  {String} fnname 方法名稱
     * @param  {Object} data 參數
     * @param  {Function} callback 成功回調函數
     * @param  {Function} failfn 失敗回調函數
     * @param  {String} IsWait 是否waitting（默認false）
     * @param  {Boolean} IsAsync 是否同步（默認非同步）
     * @return {Object} Ajax對象
     */
    w.CallAjax = function (url, fnname, data, sucfn, failfn, wait, async, alwaysfn) {
        w.IsWaiting = (wait === undefined || wait === true) ? true : (wait === false) ? null : wait;
        return $.ajax({
            type: 'POST',
            async: async == undefined ? true : false,
            url: getWebServiceUrl(url, fnname),
            data: JSON.stringify(data),     //傳送區域參數
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (res) {
                if (res.d === '-1' || res.d === -1) {// ╠message.TokenVerifyFailed⇒您的身份認證已經過期，請重新登入╣ ╠common.Tips⇒提示╣
                    layer.alert(i18next.t("message.TokenVerifyFailed"), { icon: 0, title: i18next.t("common.Tips") }, function (index) {
                        w.top.location.href = '/Page/login.html';
                    });
                }
                else {
                    if (sucfn) {
                        sucfn(res);
                    }
                }
            },
            beforeSend: function (xhr) {
                var orgid = g_db.GetItem('orgid'),
                    userid = g_db.GetItem('userid'),
                    token = g_ul.GetToken();
                xhr.setRequestHeader("orgid", orgid);
                xhr.setRequestHeader("userid", userid);
                xhr.setRequestHeader("token", token);
            },
            error: failfn || function (e1, e2, e3) { },
            global: wait === false ? false : true
        }).always(alwaysfn);
    };

    /**
     * 呼叫Ajax
     * @param  {String} urlaspx 地址
     * @param  {String} fnname 方法名稱
     * @param  {Object} data 參數
     * @param  {Function} callback 成功回調函數
     * @param  {Function} failfn 失敗回調函數
     * @param  {String} IsWait 是否waitting（默認false）
     * @param  {Boolean} IsAsync 是否同步（默認非同步）
     * @return {Object} Ajax對象
     */
    w.CallAjaxCross = function (urlaspx, fnname, data, callback, failfn, IsWait, IsAsync) {
        if (IsWait) IsWaiting = IsWait; else IsWaiting = null;
        return $.ajax({
            type: 'POST',
            async: IsAsync == undefined ? true : false,
            jsonpCallback: "Callback",//callback的function名称
            url: getWebServiceUrl(urlaspx, fnname),
            data: data,     //傳送區域參數
            dataType: 'jsonp',
            success: callback || function () { },
            beforeSend: function (xhr) {
                var orgid = g_db.GetItem('orgid'),
                    userid = g_db.GetItem('userid'),
                    token = g_ul.GetToken();
                xhr.setRequestHeader("orgid", orgid);
                xhr.setRequestHeader("userid", userid);
                xhr.setRequestHeader("token", token);
            },
            error: failfn || function (a, b, c) {
                console.log(c);
            },
            global: (IsWait == undefined || IsWait == null) ? false : true
        });
    };

    /**
     * 開啟Waiting視窗
     * @param  {String} msg 提示文字
     */
    w.showWaiting = function (msg) {
        $.blockUI({
            message: $('<div id="Divshowwaiting"><img src="/images/ajax-loader.gif">' + (msg || 'Waiting...') + '</div>'),
            css: {
                'font-size': '36px',
                border: '0px',
                'border-radius': '10px',
                'background-color': '#FFF',
                padding: '15px 15px',
                opacity: .5,
                color: 'orange',
                cursor: 'wait',
                'z-index': 1000000001
            },
            baseZ: 1000000000
        });
        w.setTimeout($.unblockUI, 60000);//預設開啟60秒後關閉
    };

    /**
     * 關閉Waiting視窗
     * @param  {Number} iSleep 延遲時間，單位為毫秒
     */
    w.closeWaiting = function (iSleep) {
        $(function () {
            if (iSleep == undefined) {
                iSleep = 100;
            }
            setTimeout($.unblockUI, iSleep);
        });
    };

    /**
     * 從物件陣列中移除屬性為objPropery，值為objValue元素的物件
     * @param  {Array} arrPerson 陣列物件
     * @param  {String} objPropery 物件的屬性
     * @param  {String} objPropery 對象的值
     * @return {Array} 過濾後陣列
     */
    w.Jsonremove = function (arrPerson, objPropery, objValue) {
        return $.grep(arrPerson, function (cur, i) {
            return cur[objPropery] != objValue;
        });
    };

    /**
     * 從物件陣列中獲取屬性為objPropery，值為objValue元素的物件
     * @param  {Array} arrPerson 陣列物件
     * @param  {String} objPropery 物件的屬性
     * @param  {String} objPropery 對象的值
     * @return {Array} 過濾後陣列
     */
    w.Jsonget = function (arrPerson, objPropery, objValue) {
        return $.grep(arrPerson, function (cur, i) {
            return cur[objPropery] == objValue;
        });
    };

    /**
     * 將json轉換字串
     * @param  {Object} json json物件
     * @return {String} json字串
     */
    w.Tostr = function (json) {
        return JSON.stringify(json);
    };
    /**
     * 下載文件
     * @param  {String} path 文件路徑（相對路勁）
     */
    w.DownLoadFile = function (path, filename) {
        var sUrl = gServerUrl + "/Controller.ashx";
        sUrl += '?action=downfile&path=' + path;
        if (filename) {
            sUrl += '&filename=' + filename;
        }
        w.location.href = sUrl;
        closeWaiting();
    };
    /**
     * 修改文件
     * @param  {Object} file 文件信息
     * @param  {HTMLElement} el 文件對象
     */
    w.EditFile = function (file, el) {
        layer.open({
            type: 1,
            title: i18next.t("common.EditFile"),// ╠common.EditFile⇒編輯文件信息╣
            shade: 0.75,
            maxmin: true, //开启最大化最小化按钮
            area: ['500px', '350px'],
            content: '<div class="pop-box">\
                       <div class="input-group input-append">\
                          <input type="text" maxlength="30" id="FileName" name="FileName" class="form-control w100p">\
                          <span class="input-group-addon add-on">\
                          </span>\
                       </div><br/>\
                       <div class="input-group w100p">\
                          <input type="text" maxlength="250" id="Link" name="Link" class="form-control w100p" placeholder="URL">\
                       </div><br/>\
                       <div class="input-group">\
                          <textarea name="FileDescription" id="FileDescription" class="form-control  w100p" rows="5" cols="500"></textarea>\
                       </div>\
                      </div>',
            btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
            success: function (layero, index) {
                $('.pop-box .input-group-addon').text('.' + file.subname);
                $('#FileName').val(file.filename);
                $('#Link').val(file.link);
                uniformInit(layero);//表單美化
            },
            yes: function (index, layero) {
                var sFileName = $('#FileName').val(),
                    data = {
                        FileName: sFileName,
                        Link: $('#Link').val(),
                        Description: $('#FileDescription').val()
                    };
                if (!data.FileName) {
                    showMsg(i18next.t("message.FileName_Required")); // ╠common.message⇒文件名稱不能為空╣
                    return false;
                }
                data.FileName += '.' + file.subname;
                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        files: {
                            values: data,
                            keys: { FileID: file.fileid }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        file.filename = sFileName;
                        file.link = data.Link;
                        file.description = data.Description;
                        var img_title = el.find('.jFiler-item-title>b'),
                            img_name = el.find('.file-name li:first'),
                            img_description = el.find('.jFiler-item-description span');
                        if (img_title.length > 0) {
                            img_title.attr('title', data.FileName).text(data.FileName);
                        }
                        if (img_name.length > 0) {
                            img_name.text(data.FileName);
                        }
                        if (img_description.length > 0) {
                            img_description.text(data.Description);
                        }
                        layer.close(index);
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error'); //╠message.Modify_Failed⇒修改失敗╣
                    }
                });
            }
        });
    };

    /**
     * 刪除文件
     * @param  {String} fileid 文件id
     * @param  {String} idtype id類型
     */
    w.DelFile = function (fileid, idtype, bAlert) {
        bAlert = (bAlert === undefined || bAlert === null) ? true : bAlert;
        if (fileid && $.trim(fileid)) {
            return g_api.ConnectLite(Service.com, 'DelFile',
                {
                    FileID: fileid,
                    IDType: idtype || ''
                },
                function (res) {
                    if (res.RESULT) {
                        if (bAlert) {
                            if (res.DATA.rel) {
                                showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                            }
                            else {
                                showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                            }
                        }
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                });
        }
        else {
            return $.Deferred().resolve().promise();
        }
    };

    /**
     * 刪除代辦
     * @param  {String} sourseid 代辦id
     */
    w.DelTask = function (sourseid) {
        return CallAjax(ComFn.W_Com, ComFn.GetDel, {
            Params: {
                task: { SourceID: sourseid, OrgID: parent.top.OrgID }
            }
        });
    };
    /**
     * 用於表單序列化去除禁用欄位
     * @param {HTMLElement} dom 父層物件
     */
    w.reFreshInput = function (dom) {
        dom.find('[disabled]').each(function () {
            $(this).attr('hasdisable', 1).removeAttr('disabled');
        });
    };
    /**
     * 用於表單序列化恢復禁用欄位
     * @param {HTMLElement} dom 父層物件
     */
    w.reSetInput = function (dom) {
        dom.find('[hasdisable]').each(function () {
            $(this).removeAttr('hasdisable').prop('disabled', true);
        });
    };
    /**
     * 用於表單序列化恢復禁用欄位
     * @param  {HTMLElement} dom 要禁用的物件標籤
     * @param  {HTMLElement} notdom 不要禁用的物件標籤
     */
    w.disableInput = function (dom, notdom, bdisabled) {
        bdisabled = (bdisabled === undefined || bdisabled === null) ? true : bdisabled;
        dom = dom.find(':input,select-one,select,checkbox,radio,textarea');
        if (notdom) {
            dom = dom.not(notdom);
        }
        dom.each(function () {
            $(this).prop('disabled', bdisabled);
        });
    };
    /**
     * 序列化form表單
     * @param  {HTMLElement} form 表單form物件
     * @param  {String} type 傳回類型
     * @return {Object Or String} 表單資料
     */
    w.getFormSerialize = function (form, type) {
        var formdata = {};
        reFreshInput(form);
        if (type) {
            formdata = form.serializeJSON();
        }
        else {
            formdata = form.serializeObject();
        }
        reSetInput(form);
        return formdata;
    };

    /**
     * 參數添加修改人/時間，創建人/時間
     * @param  {Object} data 表單資料
     * @param  {String} type 是否是修改
     * @return {Object} data 表單資料
     */
    w.packParams = function (data, type) {
        data.ModifyUser = parent.top.UserInfo.MemberID;
        data.ModifyDate = new Date().formate('yyyy/MM/dd HH:mm');
        if (!type) {
            data.CreateUser = data.ModifyUser;
            data.CreateDate = data.ModifyDate;
        }
        return data;
    };

    /**
     * 移除null項目
     * @param  {Object} data 表單資料
     * @return {Object} data 表單資料
     */
    w.removeNull = function (data) {
        var dataNew = {};
        if (data instanceof (Object)) {
            for (var o in data) {
                if (data[o] !== null) {
                    dataNew[o] = data[o];
                }
            }
        }
        return dataNew;
    };
    /**
     * 取得Url參數
     * @param  {String} name 取得部分的名稱 例如輸入"Action"，就能取到"Add"之類參數
     * @return {String}參數值
     */
    w.getUrlParam = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //構造一個含有目標參數的正則表達式對象
        var r = w.location.search.substr(1).match(reg);  //匹配目標參數
        if (r != null) return unescape(r[2]); return null; //返回參數值
    };

    /**
     * 對網址、路徑進行編碼
     * @param  {String} url 要編碼的url或路徑
     * @return {String} 編碼後的url或路徑
     */
    w.encodeURL = function (url) {
        return encodeURIComponent(url).replace(/\'/g, "%27").replace(/\!/g, "%21").replace(/\(/g, "%28").replace(/\)/g, "%29");
    };

    /**
     * 對網址、路徑進行解碼
     * @param  {String} url 要解碼的url或路徑
     * @return {String} 解碼後的url或路徑
     */
    w.decodeURL = function (url) {
        return decodeURIComponent(url);
    };
    /**
     * 透過代號或名稱快速查詢人員
     * @param {String} set 要移出数据的jquery對象
     * @param {String} get 要移入数据的jquery對象
     */
    w.optionListMove = function (set, get) {
        var size = set.find("option").size();
        var selsize = set.find("option:selected").size();
        if (size > 0 && selsize > 0) {
            set.find("option:selected").each(function () {
                $(this).prependTo(get);
            });
        }
    };
    /**
     * select options 排序
     * @param {String} set 要排序的select
     * @param {String} ordertype 正序還是倒序
     */
    w.optionListOrder = function (set, ordertype) {
        var size = set.find("option").size(),
            select = set.find("option:selected"),
            selsize = select.size();
        if (size > 0 && selsize > 0) {
            var firstsel = set.find("option:selected:first"),
                lastsel = set.find("option:selected:last")
            if (ordertype) {
                if (firstsel.prev().length > 0) {
                    firstsel.prev().before(select);
                }
            }
            else {
                if (lastsel.next().length > 0) {
                    lastsel.next().after(select);
                }
            }
        }
    };
    /**
     * select options 排序
     * @param {String} set 要排序的select
     * @param {String} ordertype 正序還是倒序
     */
    w.optionListSearch = function (set, get, input) {
        //給左邊的每一項添加title效果
        $('option', set).attr('title', function () {
            return this.innerHTML;
        });
        //給右邊的每一項添加title效果
        $('option', get).attr('title', function () {
            return this.innerHTML;
        });
        var tempValue = set.html(),      //儲存初始listbox值
            searchtxt = '';
        $.each(set.find('option'), function () {
            if ($(this).val() + $(this).text() != 'undefined') {
                searchtxt += $(this).val() + '|' + $(this).text() + ',';
            }
        });

        //給搜尋按鈕註冊事件
        input.off('keyup').on('keyup', function () {
            var sWord = this.value;
            set.empty();
            if (sWord != "") {
                $.each(searchtxt.split(','), function (key, val) {
                    if (val.toLowerCase().indexOf(sWord.toLowerCase()) >= 0) {
                        var setValue = val.split('|')[0];
                        var setText = val.split('|')[1];
                        set.append('<option value="' + setValue + '">' + setText + '</option>')
                    }
                });
            }
            else {
                set.html(tempValue); //搜尋欄位為空返還初始值(全部人員)
            }

            //移除右邊存在的值
            if (get.html() != '') {
                $.each(get.find('option'), function () {
                    set.find('option[value=' + $(this).val().replace(".", "\\.") + ']').remove();
                });
            }
        });
    };

    /**
     * 時間格式化處理
     * @param  {Date} date 日期時間
     * @param  {Boolean} type 格式（日期 || 日期+時間）
     * @return {Boolean} 是否可傳回空
     */
    w.newDate = function (date, type, empty) {
        var r = '';
        if (date) {
            if (typeof date == 'string') {
                r = date.replace('T', ' ').replaceAll('-', '/');
                if (r.indexOf(".") > -1) {
                    r = r.slice(0, r.indexOf("."));
                }
            }
            else {
                r = new Date(date);
            }
            r = new Date(r);
        }
        else {
            if (!empty) {
                r = new Date();
            }
        }
        return r === '' ? '' : !type ? r.formate("yyyy/MM/dd HH:mm") : r.formate("yyyy/MM/dd");
    };

    /**
     * 獲取html模版
     * @param  {String} sUrl 模版路徑
     * @param  {Boolean} bAsync 是否同步
     */
    w.getHtmlTmp = function (sUrl, bAsync) {
        return $.ajax({
            async: true,
            url: sUrl,
            success: function (html) {
                if (typeof callback === 'function') {
                    callback(html);
                }
            }
        });
    };

    /**
     * 產生guid
     * @param  {Number} len 指定长度,比如guid(8, 16) // "098F4D35"
     * @param  {Number} radix 基数
     * @return {String} guid
     */
    w.guid = function (len, radix) {
        var buf = new Uint16Array(8),
            cryptObj = w.crypto || w.msCrypto, // For IE11
            s4 = function (num) {
                var ret = num.toString(16);
                while (ret.length < 4) {
                    ret = '0' + ret;
                }
                return ret;
            };
        cryptObj.getRandomValues(buf);

        return s4(buf[0]) + s4(buf[1]) + '-' + s4(buf[2]) + '-' + s4(buf[3]) + '-' +
            s4(buf[4]) + '-' + s4(buf[5]) + s4(buf[6]) + s4(buf[7]);
    };

    /**
     * 產生隨機數
     * @param  {Number} len 指定长度,比如random(8)
     * @return {String} rnd 亂數碼
     */
    rndnum = function (len) {
        var rnd = "";
        len = len || 10;
        for (var i = 0; i < len; i++)
            rnd += Math.floor(Math.random() * 10);
        return rnd;
    };

    /**
     * 添加提示Tips
     * @param  {HTMLElement} handle dom物件
     */
    w.addTips = function (handle) {
        handle = (handle !== undefined) ? handle : $('[title]');
        handle.each(function () {
            var oTips = $(this);
            if (oTips.attr('title') && oTips.attr('tooltips') !== 'Y') {
                oTips.attr('tooltips', 'Y').jBox('Tooltip');
            }
        });
    };

    /**
     * 停止冒泡行为时
     * @param {HTMLElement} e 事件对象
     */
    w.stopBubble = function (e) {
        //如果提供了事件对象，则这是一个非IE浏览器
        if (e && e.stopPropagation)
            //因此它支持W3C的stopPropagation()方法
            e.stopPropagation();
        else
            //否则，我们需要使用IE的方式来取消事件冒泡
            w.event.cancelBubble = true;
    };

    /**
     * 阻止默认行为时
     * @param  {HTMLElement} e 事件对象
     */
    w.stopDefault = function (e) {
        //阻止默认浏览器动作(W3C)
        if (e && e.preventDefault)
            e.preventDefault();
        //IE中阻止函数器默认动作的方式
        else
            w.event.returnValue = false;
        return false;
    };
    /**
     * 獲取權限
     * @param  {String} programid 程式id
     * @param  {Function}  回調函數
     * @return {Object} ajax 物件
     */
    w.getAuthority = function (programid, callback, cus, opt) {
        var sTopMod = getTopMod(programid),
            sAction = getUrlParam('Action') === null ? 'add' : getUrlParam('Action').toLowerCase();

        return g_api.ConnectLite('Authorize', 'GetAuthorize',
            {
                ProgramID: programid,
                TopModuleID: sTopMod
            },
            function (res) {
                if (res.RESULT) {
                    var saAuthorize = res.DATA.rel,
                        saBtn = [],
                        oHasBtn = {},
                        oLastBtn = null,
                        initToolbar = function () {//等待 ToolBar（wedget）初始化ok后在執行，否則會報錯
                            $('#Toolbar').ToolBar({
                                btns: saBtn,
                                fncallback: callback || function () { }
                            });
                            transLang($('#Toolbar'));

                        },
                        delayInitToolbar = function () {
                            //initToolbar();
                            if ($.fn.ToolBar) {
                                initToolbar();
                            }
                            else {
                                delayInitToolbar();
                            }
                        };
                    $.each(saAuthorize, function (idx, roleright) {
                        if (roleright.AllowRight) {
                            var saRights = roleright.AllowRight.split('|');
                            $.each(saRights, function (e, btnright) {
                                var sBtn = $.trim(btnright);
                                if (oHasBtn[sBtn.toLowerCase()] === undefined) {
                                    oHasBtn[sBtn.toLowerCase()] = sBtn;
                                }
                            });
                        }
                    });
                    if (!oHasBtn['upd']) {
                        delete oHasBtn.save;
                        delete oHasBtn.readd;
                    }

                    if (sAction === 'upd') {
                        delete oHasBtn.readd;
                    }

                    if (sAction === 'add') {
                        delete oHasBtn.del;
                    }
                    delete oHasBtn.upd;
                    delete oHasBtn.view;

                    for (var btnkey in oHasBtn) {
                        var oBtn = {};
                        oBtn.key = oHasBtn[btnkey];

                        if (btnkey === 'leave') {
                            oLastBtn = oBtn;
                            oLastBtn.hotkey = 'ctrl + l';
                        }
                        else {
                            switch (btnkey) {
                                case 'qry':
                                    oBtn.hotkey = 'enter';
                                    break;
                                case 'add':
                                    oBtn.hotkey = 'ctrl + i';
                                    break;
                                case 'readd':
                                    oBtn.hotkey = 'ctrl + r';
                                    break;
                                case 'save':
                                    oBtn.hotkey = 'ctrl + s';
                                    break;
                                case 'del':
                                    oBtn.hotkey = 'ctrl + d';
                                    break;
                                case 'clear':
                                    oBtn.hotkey = 'ctrl + q';
                                    break;
                            }
                            saBtn.push(oBtn);
                        }
                    }

                    if (cus) {
                        saBtn.push.apply(saBtn, cus);
                    }
                    if (oLastBtn) {
                        saBtn.push(oLastBtn);
                    }

                    delayInitToolbar();
                    let SpecialCse = 'InvoiceApplyForCustomer_View, InvoiceApplyForPersonal_View, BillChangeApply_View';

                    //button Ready 再執行
                    if (opt.PrgId.indexOf('_Upd') > -1 || SpecialCse.indexOf(opt.PrgId) > -1) {
                        if (sAction === 'Upd'.toLocaleLowerCase()) {//判斷當前頁面是否有人在操作
                            parent.top.msgs.server.checkEdit(opt.PrgId, sCheckId);
                        }
                        $('#form_main').find(':input,select').not('[data-type=select2]').change(function () {
                            if (!$(this).attr('data-trigger')) {
                                bRequestStorage = true;
                            }
                        });
                        setTimeout(function () {
                            $('#form_main').find('[data-type=select2]').change(function () {
                                if (!$(this).attr('data-trigger')) {
                                    bRequestStorage = true;
                                }
                            });
                        }, 3000);
                    }
                    else {
                        if (opt.PrgId.indexOf('_Qry') > -1) {
                            parent.top.msgs.server.removeEditPrg(opt.PrgId.replace('_Qry', '_Upd'));//防止重複點擊首頁菜單導致之前編輯資料狀態無法移除
                        }
                        var oQueryBtn = $('#Toolbar_Qry');
                        if (oQueryBtn.length > 0) {
                            $('select').on('change', function (e, arg) {
                                //select2下拉單改變自動查詢
                                setTimeout(function () {
                                    if (arg !== 'clear') {//清除動作（賦值）不查詢
                                        $('#Toolbar_Qry').click();
                                    }
                                }, 10);
                            });
                            $(':input[type="radio"],:input[type="checkbox"]').on('click', function (e, arg) {
                                //radio值改變自動查詢
                                if (arg !== 'clear') {//清除動作（賦值）不查詢
                                    $('#Toolbar_Qry').click();
                                }
                            });
                        }
                    }
                }
            });
    };

    /**
     * createPageTitle
     * @param  {String} programid 程式id
     * @param  {HTMLElement} handle 標有標籤
     * @return {Object} ajax 物件
     */
    w.createPageTitle = function (programid, handle) {
        if (!programid) { return; }
        handle = handle || $('.page-title');
        var saPrgList = g_db.GetItem('programList'),
            sModTree = "",
            saModTree = [],
            setProgramPath = function (sModuleID) {
                var oParent = getParentMod(sModuleID);
                if (oParent.ModuleID) {
                    saModTree.unshift('<div class="ng-scope layout-row"> <a class="md-button" href="#"><span class="ng-binding ng-scope" data-i18n=common.' + oParent.ModuleID + '></span></a> <i class="fa fa-angle-right" aria-hidden="true"></i> </div>');
                }
                if (oParent.ParentID) {
                    setProgramPath(oParent.ParentID);
                }
            };
        if (saPrgList == null || saPrgList === '') { return; }
        var saProgramList = $.parseJSON(saPrgList);

        var saProgram = $.grep(saProgramList, function (item) { return item.ModuleID === programid; });

        if (saProgram.length === 0) { return; }

        var oProgram = saProgram[0];

        if (!oProgram.ParentID) { return; }

        setProgramPath(oProgram.ParentID);

        saModTree.push('<div class="ng-scope layout-row"> <a class="md-button" href="#"><span class="ng-binding ng-scope" data-i18n=common.' + oProgram.ModuleID + '></span></a> </div>');

        sModTree = saModTree.join('');     //串起父層路徑名稱

        return $.get(ComTmp.PageTitle).done(function (tmpl) {
            $.templates({ tmpl: tmpl });
            handle.html($.render.tmpl({ ProgramName: "common." + oProgram.ModuleID, showTable: parent.top.SysSet.TbShowOrHide, MainTableName: oProgram.MainTableName, ModTree: sModTree }));
            transLang(handle);

            if (navigator.userAgent.match(/mobile/i)) {
                $('.ismobile').hide();
            }
        });
    };

    /**
     * 產生下拉選單（公用）
     * @param  {Object} list datalist
     * @param  {String} id 顯示的id名稱
     * @param  {String} name 顯示的name名稱
     * @param  {Boolean} showid 是否顯示id
     * @param  {String} cusattr 客制化添加屬性
     * @param  {Boolean} isreapet 是否允許重複
     * @return {String} option html
     */
    w.createOptions = function (list, id, name, showid, cusattr, isreapet) {
        isreapet == isreapet || true;
        list = list || [];
        var Options = [],
            originAry = [];
        if (typeof list === 'number') {
            var intNum = list;
            while (list > 0) {
                var svalue = intNum - list + 1;
                svalue = $.trim(svalue);
                Options.push($('<option />', {
                    value: svalue,
                    title: svalue,
                    html: svalue
                }));
                list--;
            }
        } else {
            Options = [$('<option />', { value: '', html: '請選擇...' })];
            var intCount = list.length;
            if (intCount > 0) {
                $.each(list, function (idx, obj) {
                    if (isreapet !== false || (originAry.indexOf($.trim(obj[id])) < 0 && isreapet === false)) {
                        var option = $('<option />', {
                            value: $.trim(obj[id]),
                            title: $.trim(obj[name]),
                            html: (showid ? $.trim(obj[id]) + '-' : '') + $.trim(obj[name])
                        });
                        if (cusattr) {
                            option.attr(cusattr, obj[cusattr]);
                        }
                        Options.push(option);
                    }
                    originAry.push($.trim(obj[id]));
                });
            }
        }
        return $('<div />').append(Options).html();
    };

    /**
     * Radios（公用）
     * @param  {Object} list datalist
     * @param  {String} id 顯示的id名稱
     * @param  {String} name 顯示的name名稱
     * @param  {String} feilid 欄位ID
     * @param  {String} _id 客制化添加屬性
     * @param  {Boolean} showid 是否顯示id
     * @param  {Boolean} triggerattr 頁面加載後觸發change事件是否觸發提醒
     * @return {String} sHtml Radios HTML
     */
    w.createRadios = function (list, id, name, feilid, _id, showid, triggerattr) {
        list = list || [];
        _id = _id || '';
        triggerattr = triggerattr === false ? true : undefined;
        var sHtml = '',
            intCount = list.length;
        if (intCount > 0) {
            $.each(list, function (idx, obj) {
                var sId = feilid + '_' + _id + '_' + idx,
                    inputradio = $('<input />', {
                        type: 'radio',
                        id: sId,
                        name: feilid,
                        'data-trigger': triggerattr,
                        value: $.trim(obj[id])
                    }).attr('val', $.trim(obj[id]));
                sHtml += '<label for="' + sId + '">' + inputradio[0].outerHTML + ((showid ? $.trim(obj[id]) + '-' : '') + $.trim(obj[name])) + "</label>";
            });
        }
        return sHtml;
    };

    /**
     * 產生CheckList（公用）
     * @param  {Object} list datalist
     * @param  {String} id 顯示的id名稱
     * @param  {String} name 顯示的name名稱
     * @param  {String} pmid 屬性id
     * @param  {String} _id id後邊擴展
     * @return {String} sHtml CheckList HTML
     */
    w.createCheckList = function (list, id, name, pmid, _id) {
        list = list || [];
        var sHtml = '',
            intCount = list.length;
        if (intCount > 0) {
            $.each(list, function (idx, obj) {
                var inputradio = $('<input />', {
                    type: 'checkbox',
                    'id': id + (_id === undefined ? '' : _id) + '_' + idx,
                    name: pmid + '[]',
                    value: $.trim(obj[id]),
                    'class': 'input-mini'
                });
                sHtml += "<label for='" + id + (_id === undefined ? '' : _id) + '_' + idx + "' style='" + (intCount == idx + 1 ? '' : 'float:left;') + "padding-left: 10px'>" + inputradio[0].outerHTML + $.trim(obj[name]) + "</label>";
            });
        }
        return sHtml;
    };

    /**
     * 自適應
     */
    w.onresize = function () {
        var sHeight = $('body').height();

        if ($('#main-wrapper').length) {
            $('#main-wrapper').css('min-height', (sHeight - 88) + 'px');
        }
        setContentHeight();
    };
    w.fnframesize = function (down) {
        var pTar = null;
        if (d.getElementById) {
            pTar = d.getElementById(down);
        }
        else {
            eval('pTar = ' + down + ';');
        }
        if (pTar && !w.opera) {
            pTar.style.display = "block"
            if (pTar.contentDocument && pTar.contentDocument.body.offsetHeight) {
                //ns6 syntax
                var iBody = $(d.body).outerHeight(true);
                pTar.height = iBody - 125;
                pTar.width = pTar.contentDocument.body.scrollWidth + 20;
            }
            else if (pTar.Document && pTar.Document.body.scrollHeight) {
                pTar.height = pTar.Document.body.scrollHeight;
                pTar.width = pTar.Document.body.scrollWidth;
            }
        }
    };
    w.setContentHeight = function () {
        var fnIframeResize = function (h) {
            var aryIframe = d.getElementsByTagName("iframe");
            if (aryIframe == null) return;
            for (var i = 0; i < aryIframe.length; i++) {
                var Iframe = aryIframe[i];
                if (Iframe.id.indexOf('ueditor_') === -1) {
                    Iframe.height = h;
                }
            }
        },
            content = $('.page-inner'),
            pageContentHeight = $(d.body).outerHeight(true) - 110;

        fnIframeResize(pageContentHeight);
    };

    /**
     * 取得自定義json值
     * @param  {Object} json json對象
     * @param  {Object} name json鍵值
     * @return {String} sLast 最終要獲取的對應的鍵值對的值
     */
    w.getJsonVal = function (json, name) {
        var sLast = json,
            saName = name.split('_');
        for (var i = 0; i < saName.length; i++) {
            if (!sLast[saName[i]]) {
                sLast = '';
                break;
            }
            sLast = sLast[saName[i]];
        }
        return sLast;
    };

    /**
     * 緩存查詢條件
     * @param  {HTMLElement}form 表單物件
     */
    w.cacheQueryCondition = function (form) {
        var oForm = form || $('#form_main'),
            sPrgId = w.sProgramId || getProgramId() || '',
            oPm = {};
        w.bToFirstPage = false;

        if (sPrgId) {
            if (!parent[sPrgId + '_query']) {
                parent[sPrgId + '_query'] = {};
            }
            if (typeof oForm === 'number') {
                parent[sPrgId + '_query'].pageidx = oForm;
            }
            else {
                var oQueryPm_Old = clone(parent[sPrgId + '_query']);
                oPm = getFormSerialize(oForm);
                for (var key in oPm) {
                    parent[sPrgId + '_query'][key] = oPm[key];
                }
                for (var key in oQueryPm_Old) {
                    if (key !== 'pageidx' && parent[sPrgId + '_query'][key] !== oQueryPm_Old[key]) {
                        w.bToFirstPage = true;
                        break;
                    }
                }
            }
        }
    };

    /**
     * 設定表單值
     * @param  {HTMLElement} form 表單
     * @param  {Object} json json對象
     */
    w.setFormVal = function (form, json) {
        form.find('[name]').each(function () {
            var sId = this.id,
                sName = (this.name) ? this.name.replace('[]', '') : '',
                sType = this.type,
                sValue = json[sName] || getJsonVal(json, sId) || '';

            if (sValue) {
                switch (sType) {
                    case 'text':
                    case 'email':
                    case 'url':
                    case 'number':
                    case 'range':
                    case 'date':
                    case 'search':
                    case 'color':
                    case 'textarea':
                    case 'select-one':
                    case 'select':
                    case 'hidden':
                        var sDataType = $(this).attr("data-type");
                        if (sDataType && sDataType == 'pop') {
                            var sVal_Name = json[sName + 'Name'] || sValue;
                            $(this).attr("data-value", sValue);
                            if (sVal_Name) $(this).val(sVal_Name);
                        }
                        else {
                            if ($(this).hasClass('date-picker')) {
                                sValue = newDate(sValue, 'date');
                            }
                            else if ($(this).hasClass('date-picker') || $(this).hasClass('datetime-picker') || $(this).hasClass('date')) {
                                sValue = newDate(sValue);
                            }
                            if (sValue) $(this).val(sValue);
                            $(this).data('old', sValue);
                            if (sDataType && sDataType == 'int' && sValue) {
                                $(this).attr('data-value', sValue.toString().replace(/[^\d.]/g, ''));
                            }
                        }
                        if (sDataType === 'select2') {
                            $(this).trigger("change", 'setval');
                        }
                        break;
                    case 'checkbox':
                        if (typeof sValue == 'object') {
                            if (sValue.indexOf(this.value) > -1) {
                                this.checked = sValue;
                            }
                        }
                        else {
                            this.checked = typeof sValue == 'string' ? sValue == this.value : sValue;
                        }
                        $.uniform && $.uniform.update($(this).prop("checked", this.checked));
                        break;
                    case 'radio':
                        this.checked = this.value === sValue;
                        $.uniform && $.uniform.update($(this).prop("checked", this.checked));
                        break;
                }
                if ((sId == 'ModifyUser' || sId == 'CreateUser') && 'select-one'.indexOf(this.type) === -1) {
                    $(this).text(sValue);
                }
                if ((sId == 'ModifyUserName' || sId == 'CreateUserName') && 'select-one'.indexOf(this.type) === -1) {
                    $(this).text(sValue);
                }
                else if (sId == 'ModifyDate' || sId == 'CreateDate') {
                    $(this).text(newDate(sValue));
                }
            }
        });
    };
    /**
     * 去掉字符串中所有空格
     * @param  {String} str 要處理的字串
     * @param  {String} is_global (包括中间空格,需要设置第2个参数为:g)
     * @return  {String} result 處理後的字串
     */
    w.Trim = function (str, is_global) {
        var result;
        result = str.replace(/(^\s+)|(\s+$)/g, "");
        if (is_global.toLowerCase() == "g") {
            result = result.replace(/\s/g, "");
        }
        return result;
    };

    /**
     * 通過id獲取name
     * @return {Object} ajax 物件
     */
    w.setNameById = function () {
        var saRequests = [];
        $('[data-source]').each(function () {
            var that = this,
                sValue = $(that).attr('data-value') || $(that).data('value') || that.value || $(that).text(),
                saSource = $(this).data('source').split('.'),
                oParam = {},
                oEnty = {};
            if (saSource.length > 2) {
                oEnty[saSource[1]] = sValue;
                oParam[saSource[0]] = oEnty;
                oParam.OrgID = parent.top.OrgID;
                if (sValue) {
                    saRequests.push(CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: oParam
                    }, function (res) {
                        if (res.d) {
                            var oInfo = $.parseJSON(res.d);
                            if (that.type === 'text') {
                                $(that).val(oInfo[saSource[2]] || oInfo[saSource[3]]);
                            }
                            else {
                                $(that).text(oInfo[saSource[2]] || oInfo[saSource[3]]);
                            }
                        }
                    }));
                }
            }
        });
        return $.whenArray(saRequests);
    };
    /**
     * 只能輸入數字
     * @param  {HTMLElement} $input 實例化物件
     * @param  {Number} decimal 幾位小數
     * @param  {Boolean} minus 是否支持負數
     */
    w.moneyInput = function ($input, decimal, minus) {
        if ($input.length > 0) {
            var oNum = new FormatNumber();
            $input.each(function () {
                var sValue = this.value,
                    sNewStr = '';
                for (var i = 0; i < sValue.length; i++) {
                    if (!isNaN(sValue[i])) {
                        sNewStr += sValue[i];
                    }
                }
                this.value == sNewStr;
                oNum.init({ trigger: $(this), decimal: decimal || 0, minus: minus || false });
            });
        }
    };

    //當前頁面所有Control的初始值
    w.aryCurrentPageValueTmp = new Array();
    /**
     * 儲存當前頁面的值
     * @param:  {String} sMainId 要清除區塊的父層Id
     */
    w.getPageVal = function (sMainId) {
        aryCurrentPageValueTmp = [];

        //判斷傳入的樣式是否不存在或者等於空的情況
        var oHandle = (sMainId !== undefined) ? $('#' + sMainId) : ($('#searchbar').length > 0 ? $('#searchbar') : $('.page-inner'));

        //儲存畫面值
        oHandle.find(':input', 'textarea', 'select').each(function () {
            var ctl = {};     //實例化對象

            ctl.ID = this.id;
            ctl.Type = this.type;

            switch (this.type) {
                case 'text':
                case 'email':
                case 'url':
                case 'number':
                case 'range':
                case 'date':
                case 'search':
                case 'color':
                case 'password':
                case 'hidden':
                case 'textarea':
                    ctl.Value = $(this).val();
                    break;
                case 'checkbox':
                case 'radio':
                    ctl.Checked = this.checked;
                    break;
                case 'select-multiple':
                case 'select-one':
                case 'select':
                    ctl.Value = $(this).val() || '';
                    ctl.Html = $(this).html();
                    break;
            }
            aryCurrentPageValueTmp.push(ctl);
        });
    };
    /**
     * 清除畫面值
     * @param {String} flag 是否為查詢清空（查詢頁面須全部清空，不保留原值）
     */
    w.clearPageVal = function () {
        var oPageDataTmp = aryCurrentPageValueTmp,
            oQueryBtn = $('#Toolbar_Qry');
        for (var i = 0; i < oPageDataTmp.length; i++) {
            var ctl = oPageDataTmp[i],
                curInput = $("#" + ctl.ID),
                ctlParent = curInput.parent();
            try {
                switch (ctl.Type) {
                    case 'text':
                    case 'email':
                    case 'url':
                    case 'number':
                    case 'range':
                    case 'date':
                    case 'search':
                    case 'color':
                    case 'password':
                    case 'hidden':
                    case 'textarea':
                        curInput.val(ctl.Value);
                        break;
                    case 'checkbox':
                    case 'radio':
                        curInput[0].checked = ctl.Checked;
                        $.uniform && $.uniform.update(curInput.prop("checked", curInput[0].checked));
                        break;
                    case 'select-multiple':
                    case 'select-one':
                    case 'select':
                        if (ctl.Html) {
                            curInput.html(ctl.Html)
                        }
                        curInput.val(ctl.Value);
                        if (curInput.attr('data-type') === 'select2') {
                            curInput.trigger('change', 'clear');
                        }
                        break;
                }
            } catch (e) {
                alert(e);
            }
        }

        if (oQueryBtn.length > 0) {
            $('#Toolbar_Qry').click();
        }
    };

    /**
     * 監聽css文件是否加在完畢
     * @param:  {Function} fn 回調函數
     * @param:  {Object} link css標籤
     */
    w.cssReady = function (fn, link) {
        var t = d.createStyleSheet,
            r = t ? 'rules' : 'cssRules',
            s = t ? 'styleSheet' : 'sheet',
            l = d.getElementsByTagName('link');
        // passed link or last link node
        link || (link = l[l.length - 1]);
        function check() {
            try {
                return link && link[s] && link[s][r] && link[s][r][0];
            } catch (e) {
                return false;
            }
        }
        (function poll() {
            check() && setTimeout(fn, 0) || setTimeout(poll, 100);
        })();
    };
    /**
     * 克隆对象
     * @param: {Object} obj 被轉換對象
     * @return：{Object} o 新對象
     */
    w.clone = function (obj) {
        var o, i, j, k;
        if (typeof (obj) != "object" || obj === null) return obj;
        if (obj instanceof (Array)) {
            o = [];
            i = 0; j = obj.length;
            for (; i < j; i++) {
                if (typeof (obj[i]) == "object" && obj[i] != null) {
                    o[i] = arguments.callee(obj[i]);
                }
                else {
                    o[i] = obj[i];
                }
            }
        }
        else {
            o = {};
            for (i in obj) {
                if (typeof (obj[i]) == "object" && obj[i] != null) {
                    o[i] = arguments.callee(obj[i]);
                }
                else {
                    o[i] = obj[i];
                }
            }
        }

        return o;
    };
    /**
     * 公用初始函數
     * @param: {Object} opt 程式ID
     * @return：{Object} ajax 物件
     */
    w.commonInit = function (opt) {
        if (navigator.userAgent.match(/mobile/i)) {
            $('.ismobile').hide();
        }

        if ($("#tabs").length > 0) {
            $("#tabs").tabs().find('li').on('click', function () {
                var that = this;
                $('#tabs>ul>li').removeClass('active');
                $(this).addClass('active');
                if (opt.tabClick) {
                    opt.tabClick(that);
                }
            });
        }

        setTimeout(function () {
            if ($.datepicker !== undefined) {
                $.datepicker.regional['zh-TW'] = {
                    dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
                    dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"],
                    monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
                    monthNamesShort: ["01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"],
                    prevText: "上月",
                    nextText: "次月",
                    weekHeader: "週",
                    showMonthAfterYear: true, // True if the year select precedes month, false for month then year//設置是否在面板的头部年份后面显示月份
                    dateFormat: "yy/mm/dd"
                };
                $.datepicker.setDefaults($.datepicker.regional["zh-TW"]);
            }

            //註冊日曆控件（不含時間）
            if ($('.date-picker').length > 0) {
                $('.date-picker').datepicker({
                    changeYear: true,
                    changeMonth: true,
                    altFormat: 'yyyy/MM/dd',
                    onSelect: function (r, e) {
                        if (typeof opt.onSelect === 'function') {
                            opt.onSelect(r, e);
                        }
                    },
                    afterInject: function (r, e) { }
                });
            }

            //註冊日曆控件（含時間）
            if ($('.datetime-picker').length > 0) {
                $('.datetime-picker').each(function () {
                    var iHour = ($(this).attr('hour') || 9) * 1,
                        iMinute = ($(this).attr('minute') || 0) * 1,
                        iStepMinute = ($(this).attr('stepminute') || 15) * 1;
                    $(this).datetimepicker({
                        changeYear: true,
                        changeMonth: true,
                        altFormat: 'yyyy/MM/dd',
                        timeFormat: 'HH:mm',
                        //minDateTime: new Date(),
                        //maxDateTime: new Date().dateAdd('d', 5),
                        //defaultValue: newDate(null, true) + ' 08:00',
                        hour: iHour,
                        minute: iMinute,
                        stepMinute: iStepMinute,
                        hourGrid: 6,
                        minuteGrid: 15,
                        onSelect: function (r, e) {
                            if (typeof opt.onSelect === 'function') {
                                opt.onSelect(r, e);
                            }
                        },
                        afterInject: function (r, e) { }
                    });
                });
            }

            //註冊日曆控件（時間）
            if ($('.time-picker').length > 0) {
                $('.time-picker').each(function () {
                    var iHour = ($(this).attr('hour') || 9) * 1,
                        iMinute = ($(this).attr('minute') || 0) * 1;
                    $(this).timepicker({
                        timeFormat: 'HH:mm',
                        hour: iHour,
                        minute: iMinute,
                        minuteGrid: 30,
                        stepMinute: 30,
                        onSelect: function (r, e) {
                            if (typeof opt.onSelect == 'function') {
                                opt.onSelect(r, e);
                            }
                        }
                    });
                });
            }
        }, 1000);

        //註冊顏色控件
        if ($('.color-picker').length > 0) {
            $('.color-picker').each(function () {
                var that = this;
                $(that).spectrum({
                    color: "#000000",
                    //flat: true,
                    showInput: true,
                    className: "full-spectrum",
                    showInitial: true,
                    showPalette: true,
                    showSelectionPalette: true,
                    maxPaletteSize: 10,
                    preferredFormat: "hex",
                    hide: function (color) {
                        $(that).val(color.toHexString());
                    },
                    palette: [
                        ["rgb(0, 0, 0)", "rgb(67, 67, 67)", "rgb(102, 102, 102)",
                            "rgb(204, 204, 204)", "rgb(217, 217, 217)", "rgb(255, 255, 255)",
                            "rgb(152, 0, 0)", "rgb(255, 0, 0)", "rgb(255, 153, 0)", "rgb(255, 255, 0)", "rgb(0, 255, 0)"],
                        ["rgb(230, 184, 175)", "rgb(244, 204, 204)", "rgb(252, 229, 205)", "rgb(255, 242, 204)", "rgb(217, 234, 211)",
                            "rgb(208, 224, 227)", "rgb(201, 218, 248)", "rgb(207, 226, 243)", "rgb(217, 210, 233)", "rgb(234, 209, 220)",
                            "rgb(221, 126, 107)", "rgb(234, 153, 153)", "rgb(249, 203, 156)", "rgb(255, 229, 153)", "rgb(182, 215, 168)",
                            "rgb(162, 196, 201)", "rgb(164, 194, 244)", "rgb(159, 197, 232)", "rgb(180, 167, 214)", "rgb(213, 166, 189)",
                            "rgb(204, 65, 37)", "rgb(224, 102, 102)", "rgb(246, 178, 107)", "rgb(255, 217, 102)", "rgb(147, 196, 125)",
                            "rgb(118, 165, 175)", "rgb(109, 158, 235)", "rgb(111, 168, 220)", "rgb(142, 124, 195)", "rgb(194, 123, 160)",
                            "rgb(166, 28, 0)", "rgb(204, 0, 0)", "rgb(230, 145, 56)", "rgb(241, 194, 50)", "rgb(106, 168, 79)",
                            "rgb(69, 129, 142)", "rgb(60, 120, 216)", "rgb(61, 133, 198)", "rgb(103, 78, 167)", "rgb(166, 77, 121)",
                            "rgb(91, 15, 0)", "rgb(102, 0, 0)", "rgb(120, 63, 4)", "rgb(127, 96, 0)", "rgb(39, 78, 19)",
                            "rgb(12, 52, 61)", "rgb(28, 69, 135)", "rgb(7, 55, 99)", "rgb(32, 18, 77)", "rgb(76, 17, 48)",
                            "rgb(0, 255, 255)", "rgb(74, 134, 232)", "rgb(0, 0, 255)", "rgb(153, 0, 255)", "rgb(255, 0, 255)"]
                    ]
                });
            });
        }

        //if ($('textarea').length > 0) {
        //    $('textarea').each(function () {
        //        autoTextarea(this);//文本框textarea根据输入内容自适应高度
        //    });
        //}

        //keyTab();   //註冊按Tab鍵下移
        keyInput();   //註冊欄位管控

        select2Init();//初始化select2
        uniformInit();//表單美化

        if (opt.GoTop) {
            goTop();
        }

        if (opt.PrgId) {
            var sLang = g_ul.GetLang() || 'zh-TW';
            setLang(sLang);//翻譯多語系
            getPageVal(); //緩存頁面值，用於清除
            createPageTitle(opt.PrgId).done(function () {//創建Header
                if (opt.SearchBar) {
                    var iSearchBox_h = $('#searchbar').height(),
                        oSlideUpDown = $('<i>', {
                            class: 'fa fa-arrow-up slide-box',
                            click: function () {
                                if ($(this).hasClass('fa-arrow-up')) {
                                    $(this).removeClass('fa-arrow-up').addClass('fa-arrow-down');
                                    oSearchBox.slideUp();
                                    oGrid.height = (oGrid.dfheight.replace('px', '') * 1 + iSearchBox_h) + 'px';
                                }
                                else {
                                    $(this).removeClass('fa-arrow-down').addClass('fa-arrow-up');
                                    oSearchBox.slideDown();
                                    oGrid.height = oGrid.dfheight;
                                }
                                $("#jsGrid").jsGrid("refresh");
                                //調整Grid slimscrollDIV高度，保證和實際高度一致
                                var oGridBox = $('.jsgrid-grid-body.slimscroll');
                                oGridBox.parent().css('height', oGridBox.css('height'));
                            }
                        }),
                        oSlideDiv = $('<div>', { class: 'col-sm-12 up-down-go' }).append(oSlideUpDown),
                        oSearchBox = $('#searchbar').after(oSlideDiv);
                }
            });

            reSetQueryPm(opt.PrgId);   //恢復之前查詢條件

            //加載按鈕權限
            return getAuthority(opt.PrgId, opt.ButtonHandler, opt.Buttons, opt);
        }
    };

    /**
     * select2特殊化處理
     * @param  {Object} $d select2控制項
     */
    w.select2Init = function ($d) {
        var select2 = $d === undefined ? $('select[data-type=select2]') : $d.find('select[data-type=select2]');
        //註冊客制化選單
        if (select2.length > 0) {
            select2.each(function () {
                if ($(this).find('option').length > 0 && !$(this).attr('data-hasselect2')) {
                    $(this).select2().attr('data-hasselect2', true);
                    $(this).next().after($(this));
                }
            });
        }
    };

    /**
     * select2特殊化處理
     * @param  {Object} $d 表單控制項
     */
    w.uniformInit = function ($d) {
        //var checkBox = $("input[type=checkbox]:not(.switchery), input[type=radio]:not(.no-uniform)");
        var checkBox = $("input[type=radio]:not(.no-uniform)");
        if ($d) {
            checkBox = $d.find("input[type=radio]:not(.no-uniform)");
        }
        if (checkBox.length > 0) {
            checkBox.each(function () {
                $(this).uniform();
            });
        };
    };

    /**
     * 小數後‘n’位
     * @param  {HTMLElement} e
     * @param  {Object} that 控制項
     * @param  {Number} len 小數點後位數
     */
    w.keyIntp = function (e, that, len) {
        if (e.keyCode != 8 && e.keyCode != 37 && e.keyCode != 39 && e.keyCode != 46) {
            var reg = {
                1: /^(\-)*(\d+)\.(\d).*$/,
                2: /^(\-)*(\d+)\.(\d\d).*$/,
                3: /^(\-)*(\d+)\.(\d\d\d).*$/
            };
            that.value = that.value.replace(/[^\d.-]/g, '');  //清除“数字”和“.”以外的字符
            that.value = that.value.replace(/\.{2,}/g, '.'); //只保留第一个. 清除多余的
            that.value = that.value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");

            that.value = that.value.replace(reg[len], '$1$2.$3');//只能输入n个小数
            if (that.value.indexOf(".") < 0 && that.value !== "" && that.value !== "-") {//以上已经过滤，此处控制的是如果没有小数点，首位不能为类似于 01、02的金额&&不是“-”
                if (that.value.indexOf("-") === 0) {//如果是負數
                    that.value = 0 - parseFloat(that.value.replace('-', ''));
                }
                else {
                    that.value = parseFloat(that.value);
                }
                //that.value = fMoney(that.value, 2);
            }
        }
    };

    /**
     * 離開事件
     */
    w.pageLeave = function () {
        var ToLeave = function () {
            parent.top.openPageTab(sQueryPrgId);
            parent.top.msgs.server.removeEditPrg(sProgramId);
        };
        //當被lock住，不儲存任何資料，直接離開。
        if (parent.bLockDataForm0430 !== undefined)
            ToLeave();

        if (bRequestStorage) {
            layer.confirm(i18next.t('message.HasDataTosave'), {//╠message.HasDataTosave⇒尚有資料未儲存，是否要儲存？╣
                icon: 3,
                title: i18next.t('common.Tips'),// ╠message.Tips⇒提示╣
                btn: [i18next.t('common.Yes'), i18next.t('common.No')] // ╠message.Yes⇒是╣ ╠common.No⇒否╣
            }, function (index) {
                layer.close(index);
                bLeavePage = true;
                $('#Toolbar_Save').click();
            }, function () {
                ToLeave();
            });
            return false;
        }
        ToLeave();
    };

    /**
     * 目的：Html定位到某個位置
     * @param  {Object} goto 要定位的html jquery對象
     * @param  {Number} h 誤差值
     */
    w.goToJys = function (goto, h) {
        $("html,body").animate({ scrollTop: goto.offset().top + (h || 0) }, 500);//定位到...
    };

    /**
     * 目的：設置滾動條
     */
    w.slimScroll = function (h, c) {
        if ($.fn.slimScroll) {
            var option = {
                allowPageScroll: true,
                color: '#ee7624',
                opacity: 1
            };
            if (h) {
                option.height = h;
            }
            if (c) {
                option.reduce = c;
            }
            $('.slimscroll').slimscroll(option);
        }
    };

    /**
     * 目的：回頂端
     */
    w.goTop = function () {
        var oTop = $('<div>', {
            class: 'gotop',
            html: '<img src="../../images/gotop_1.png" />', click: function () {
                return $("body,html").animate({ scrollTop: 0 }, 120), !1;
            }
        });
        $('body').append(oTop.hide());//添加置頂控件
        $(d).on('scroll', function () {
            var h = ($(d).height(), $(this).scrollTop()),
                toolbarH = -45,
                toolbarCss = {};
            h > 0 ? oTop.fadeIn() : oTop.fadeOut();
            if (h > 35) {
                toolbarH = h - 80;
                $('#Toolbar').addClass('toolbar-float').removeClass('toolbar-fix');
            }
            else {
                $('#Toolbar').removeClass('toolbar-float').addClass('toolbar-fix');
            }
            $('#Toolbar').css('margin-top', toolbarH + 'px');
        });
    };

    /**
     * 目的：獲取當前瀏覽器
     */
    w.getExplorer = function () {
        var sExplorerName = '',
            explorer = w.navigator.userAgent;
        //ie
        if (explorer.indexOf("MSIE") >= 0) {
            sExplorerName = 'ie';
        }
        //firefox
        else if (explorer.indexOf("Firefox") >= 0) {
            sExplorerName = 'firefox';
        }
        //Chrome
        else if (explorer.indexOf("Chrome") >= 0) {
            sExplorerName = 'chrome';
        }
        //Opera
        else if (explorer.indexOf("Opera") >= 0) {
            sExplorerName = 'opera';
        }
        //Safari
        else if (explorer.indexOf("Safari") >= 0) {
            sExplorerName = 'safari';
        }
    };

    /**
     * 目的：異動模式點擊資料行彈出編輯按鈕
     * @param  {String} prgid 程式id
     * @param  {String} params 參數
     */
    w.goToEdit = function (prgid, params) {
        parent.top.layer.open({
            type: 1,
            title: false,
            area: ['100px', '70px'],//寬度
            shade: 0.75,//遮罩
            shadeClose: true,//╠common.Edit⇒編輯╣
            content: '<div class="pop-box">\
                         <button type="button" data-i18n="common.Edit" id="RowEdit" class="btn-custom green">編輯</button>\
                      </div>',
            success: function (layero, idx) {
                layero.find('#RowEdit').click(function () {
                    parent.top.openPageTab(prgid, params);
                    parent.top.layer.close(idx);
                });
            }
        });
    };
    /**
     * 目的:回覆查詢條件
     */
    w.reSetQueryPm = function (programid) {
        var oQueryPm = parent[programid + '_query'];
        if (oQueryPm) {
            setFormVal($('#form_main'), oQueryPm);
            if (oQueryPm.pageidx) {
                w.QueryPageidx = oQueryPm.pageidx;
            }
        }
    };
    /**
     * 金額控件文本實例化
     * @param  {Number} s需要转换的金额;
     * @param  {Number} n 保留几位小数；
     * @param  {String} istw 幣別；
     */
    w.fMoney = function (s, n, istw) {
        var p = n > 0 && n <= 20 ? n : 2;
        if (istw && istw === 'NTD') {
            s = Math.round(s);
        }
        s = parseFloat(((s || 0) + "").replace(/[^\d\.-]/g, "")).toFloat(p) + "";
        var l = s.split(".")[0].split("").reverse(),
            r = s.split(".")[1] || Array(n + 1).join(0),
            t = "";
        for (i = 0; i < l.length; i++) {
            t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? "," : "");
        }
        return t.split("").reverse().join("") + (n === 0 ? '' : "." + r);
    };
    /**
     * 金額控件
     * @param  {Number} s 需要转换的金额;
     * @return {String} istw 浮動數字
     */
    w.fnRound = function (s, istw) {
        let RoundingPoint = 0;
        if (istw && istw === 'NTD') {
            RoundingPoint = 0;
        }
        else {
            RoundingPoint = 2;
        }
        let Result = parseFloat((s).toFixed(RoundingPoint));

        return Result;
    };

    /**
     * 目的:input文本處理
     */
    w.keyInput = function () {
        //只能輸入數字
        if ($('[data-keyint]').length > 0) {
            $('[data-keyint]').on('keyup blur', function (e) {
                this.value = this.value.replace(/\D/g, '');
            });
        }
        //只能输入英文
        if ($('[data-keyeng]').length > 0) {
            $('[data-keyeng]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^a-zA-Z]/g, '');
            });
        }
        //只能输入中文
        if ($('[data-keyeng]').length > 0) {
            $('[data-keyeng]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^\u4E00-\u9FA5]/g, '');
            });
        }
        //只能輸入數字和“-”，（手機/電話）
        if ($('[data-keytelno]').length > 0) {
            $('[data-keytelno]').on('keyup blur', function (e) {
                if (e.keyCode != 8 && e.keyCode != 37 && e.keyCode != 39 && e.keyCode != 46) {
                    this.value = this.value.replace(/[^0-9\-\+\#\ ]/g, '');
                }
            });
        }
        //只能輸入數字和“-,+,#”
        if ($('[data-keyintg]').length > 0) {
            $('[data-keyintg]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^0-9\-\+\#]/g, '');
            });
        }
        //只能輸入數字和“.”,小數點後保留一位有效數字
        if ($('[data-keyintp1]').length > 0) {
            $('[data-keyintp1]').on('keyup blur', function (e) {
                keyIntp(e, this, 1);
            });
        }
        //只能輸入數字和“.”,小數點後保留兩位有效數字
        if ($('[data-keyintp2]').length > 0) {
            $('[data-keyintp2]').on('keyup blur', function (e) {
                keyIntp(e, this, 2);
            });
        }
        //只能輸入數字和“.”,小數點後保留三位有效數字
        if ($('[data-keyintp3]').length > 0) {
            $('[data-keyintp3]').on('keyup blur', function (e) {
                keyIntp(e, this, 3);
            });
        }
        //只允许输入英文
        if ($('[data-keyeng]').length > 0) {
            $('[data-keyeng]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^\a-\z\A-\Z]/g, '');
            });
        }
        //只能输入英文字母和数字,不能输入中文
        if ($('[data-keyinteng]').length > 0) {
            $('[data-keyinteng]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^0-9\a-\z\A-\Z\_]/g, '');
            });
        }
        //只能输入字母和汉字
        if ($('[data-keycneng]').length > 0) {
            $('[data-keycneng]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[/d]/g, '');
            });
        }
        //帳號輸入規則
        if ($('[data-keymemberid]').length > 0) {
            $('[data-keymemberid]').on('keyup blur', function (e) {
                this.value = this.value.replace(/[^\w\.\/]/ig, '');
            });
        }
        //限制inout輸入長度
        if ($('[_maxlength]').length > 0) {
            $('[_maxlength]').each(function () {
                var iPromiseLen = $(this).attr('_maxlength');
                if (iPromiseLen) {
                    $(this).on('input propertychange', function () {
                        var sVal = this.value,
                            sVal_New = '',
                            iLen = 0,
                            i = 0;
                        for (; i < sVal.length; i++) {
                            if ((sVal.charCodeAt(i) & 0xff00) != 0) {
                                iLen++;
                            }
                            iLen++;
                            if (iLen > iPromiseLen * 1) {
                                this.value = sVal_New;
                                break;
                            }
                            sVal_New += sVal[i];
                        }
                    });
                }
            });
        }
    };

    /**
     * 文本框根据输入内容自适应高度
     * @param {HTMLElement} elem   输入框元素
     * @param {Number} extra 设置光标与输入框保持的距离(默认0)
     * @param {Number} maxHeight 设置最大高度(可选)
     */
    w.autoTextarea = function (elem, extra, maxHeight) {
        extra = extra || 0;
        var isFirefox = !!d.getBoxObjectFor || 'mozInnerScreenX' in w,
            isOpera = !!w.opera && !!w.opera.toString().indexOf('Opera'),
            addEvent = function (type, callback) {
                elem.addEventListener ?
                    elem.addEventListener(type, callback, false) :
                    elem.attachEvent('on' + type, callback);
            },
            getStyle = elem.currentStyle ? function (name) {
                var val = elem.currentStyle[name];

                if (name === 'height' && val.search(/px/i) !== 1) {
                    var rect = elem.getBoundingClientRect();
                    return rect.bottom - rect.top -
                        parseFloat(getStyle('paddingTop')) -
                        parseFloat(getStyle('paddingBottom')) + 'px';
                };

                return val;
            } : function (name) {
                return getComputedStyle(elem, null)[name];
            },
            minHeight = parseFloat(getStyle('height'));

        elem.style.resize = 'none';

        var change = function () {
            var scrollTop, height,
                padding = 0,
                style = elem.style;

            if (elem._length === elem.value.length) return;
            elem._length = elem.value.length;

            if (!isFirefox && !isOpera) {
                padding = parseInt(getStyle('paddingTop')) + parseInt(getStyle('paddingBottom'));
            };
            scrollTop = d.body.scrollTop || d.documentElement.scrollTop;

            elem.style.height = minHeight + 'px';
            if (elem.scrollHeight > minHeight) {
                if (maxHeight && elem.scrollHeight > maxHeight) {
                    height = maxHeight - padding;
                    style.overflowY = 'auto';
                } else {
                    height = elem.scrollHeight - padding;
                    style.overflowY = 'hidden';
                };
                style.height = height + extra + 'px';
                scrollTop += parseInt(style.height) - elem.currHeight;
                d.body.scrollTop = scrollTop;
                d.documentElement.scrollTop = scrollTop;
                elem.currHeight = parseInt(style.height);
            };
        };

        addEvent('propertychange', change);
        addEvent('input', change);
        addEvent('focus', change);
        change();
    };

    /**
     * 驗證郵箱格式
     * @param  {String} email 输入的值
     * @return：{Boolean}
     */
    w.isEmail = function (email) {
        if (/^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/.test(email)) {
            return true;
        }
        else {
            return false;
        }
    };

    /**
     * input改變後立即更新當前資料
     * @param  {HTMLElement} inputs 输入框元素
     * @param  {Object} data 當前資料
     */
    w.inputChange = function (inputs, data) {
        inputs.on('change', function () {
            var that = this,
                sId = $(that).attr('data-id');
            data[sId] = $(that).val();
        });
    };

    if (!w.browser) {
        var userAgent = navigator.userAgent.toLowerCase(), uaMatch;
        w.browser = {}

        /**
         * 判断是否为ie
         */
        function isIE() {
            return ("ActiveXObject" in w);
        }
        /**
         * 判断是否为谷歌浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/chrome\/([\d.]+)/);
            if (uaMatch != null) {
                w.browser['name'] = 'chrome';
                w.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为火狐浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/firefox\/([\d.]+)/);
            if (uaMatch != null) {
                w.browser['name'] = 'firefox';
                w.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为opera浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/opera.([\d.]+)/);
            if (uaMatch != null) {
                w.browser['name'] = 'opera';
                w.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为Safari浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/safari\/([\d.]+)/);
            if (uaMatch != null) {
                w.browser['name'] = 'safari';
                w.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 最后判断是否为IE
         */
        if (!uaMatch) {
            if (userAgent.match(/msie ([\d.]+)/) != null) {
                uaMatch = userAgent.match(/msie ([\d.]+)/);
                w.browser['name'] = 'ie';
                w.browser['version'] = uaMatch[1];
            } else {
                /**
                 * IE10
                 */
                if (isIE() && !!d.attachEvent && (function () { "use strict"; return !this; }())) {
                    w.browser['name'] = 'ie';
                    w.browser['version'] = '10';
                }
                /**
                 * IE11
                 */
                if (isIE() && !d.attachEvent) {
                    w.browser['name'] = 'ie';
                    w.browser['version'] = '11';
                }
            }
        }

        /**
         * 注册判断方法
         */
        if (!$.isIE) {
            $.extend({
                isIE: function () {
                    return (w.browser.name == 'ie');
                }
            });
        }
        if (!$.isChrome) {
            $.extend({
                isChrome: function () {
                    return (w.browser.name == 'chrome');
                }
            });
        }
        if (!$.isFirefox) {
            $.extend({
                isFirefox: function () {
                    return (w.browser.name == 'firefox');
                }
            });
        }
        if (!$.isOpera) {
            $.extend({
                isOpera: function () {
                    return (w.browser.name == 'opera');
                }
            });
        }
        if (!$.isSafari) {
            $.extend({
                isSafari: function () {
                    return (w.browser.name == 'safari');
                }
            });
        }
    }

    /**
     * 取得host
     * @return {String} host Url
     */
    function gethost() {
        var g_ServerUrl = location.origin + '/';
        if (!w.location.origin) {
            g_ServerUrl = w.location.protocol + "//" + w.location.hostname + (w.location.port ? ':' + w.location.port : '');
        }
        return g_ServerUrl;
    }

    /**
     * 取得頂層模組
     * @param  {String} programId 程式id
     * @return {String} 頂層模組ID
     */
    function getTopMod(programId) {
        var sTopMod = '',
            saProgramList = g_db.GetDic('programList') || [],
            oProgram = Enumerable.From(saProgramList).Where(function (item) { return item.ModuleID === programId; }).First(),
            getParent = function (modid) {
                var oMod = getParentMod(modid);
                if (oMod.ParentID) {
                    getParent(oMod.ParentID);
                }
                else {
                    sTopMod = oMod.ModuleID
                }
            };
        getParent(oProgram.ParentID);
        return sTopMod;
    }

    /**
     *取得程式所在模組的父層模組
     * @param  {String} modid 模組id
     * @return {Object} oParent 模組對象
     */
    function getParentMod(modid) {
        var saProgramList = g_db.GetDic('programList') || [];
        var oParent = Enumerable.From(saProgramList).Where(function (item) { return (item.ModuleID == modid && item.FilePath == '#'); }).First();
        return oParent;
    }

    /**
     * 动态加载js文件的程序
     * @param  {String} filename css or js 路徑
     * @param  {String} filetype 類型js or css
     */
    function loadjscssfile(filename, filetype) {
        if (filetype == "js") {
            var script = d.createElement('script');
            script.setAttribute("type", "text/javascript");
            script.setAttribute("src", filename);
        } else if (filetype == "css") {
            var script = d.createElement('link');
            script.setAttribute("rel", "stylesheet");
            script.setAttribute("type", "text/css");
            script.setAttribute("href", filename);
        }
        if (typeof script !== "undefined") {
            $("head").append(script);
        }
    }

    function onStart(e) {
        showWaiting(typeof IsWaiting === 'string' ? IsWaiting : undefined);
    }
    function onStop(e) {
        closeWaiting();
        setTimeout(function () { IsWaiting = null; }, 3000);
    }

    $(d).ajaxStart(onStart).ajaxStop(onStop);//.ajaxSuccess(onSuccess);
})(jQuery, window, document);

/**
 * 日期添加屬性
 * @param  {String} type y:年;q:季度;m:月;w:星期;d:天;h:小時;n:分;s:秒;ms:毫秒;
 * @param  {Number} filetype 添加的數值；
 * @return {Date} r 新的時間
 */
Date.prototype.dateAdd = function (type, num) {
    var r = this,
        k = { y: 'FullYear', q: 'Month', m: 'Month', w: 'Date', d: 'Date', h: 'Hours', n: 'Minutes', s: 'Seconds', ms: 'MilliSeconds' },
        n = { q: 3, w: 7 };
    eval('r.set' + k[type] + '(r.get' + k[type] + '()+' + ((n[type] || 1) * num) + ')');
    return r;
}

/**
 * 計算兩個日期的天數
 * @param  {Object} date 第二個日期;
 * @return {Number} 時間差
 */
Date.prototype.diff = function (date) {
    return (this.getTime() - date.getTime()) / (24 * 60 * 60 * 1000);
}

/**
 * 对Date的扩展，将 Date 转化为指定格式的String
 * 月(M)、日(d)、12小时(h)、24小时(H)、分(m)、秒(s)、周(E)、季度(q) 可以用 1-2 个占位符
 * 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)
 * eg:
 * (new Date()).formate("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423
 * (new Date()).formate("yyyy-MM-dd E HH:mm:ss") ==> 2009-03-10 二 20:09:04
 * (new Date()).formate("yyyy-MM-dd EE hh:mm:ss") ==> 2009-03-10 周二 08:09:04
 * (new Date()).formate("yyyy-MM-dd EEE hh:mm:ss") ==> 2009-03-10 星期二 08:09:04
 * (new Date()).formate("yyyy-M-d h:m:s.S") ==> 2006-7-2 8:9:4.18
 */
Date.prototype.formate = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1, //月份
        "d+": this.getDate(), //日
        "h+": this.getHours() % 12 == 0 ? 12 : this.getHours() % 12, //小时
        "H+": this.getHours(), //小时
        "m+": this.getMinutes(), //分
        "s+": this.getSeconds(), //秒
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度
        "S": this.getMilliseconds() //毫秒
    };
    var week = {
        "0": "\u65e5",
        "1": "\u4e00",
        "2": "\u4e8c",
        "3": "\u4e09",
        "4": "\u56db",
        "5": "\u4e94",
        "6": "\u516d"
    };
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    if (/(E+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, ((RegExp.$1.length > 1) ? (RegExp.$1.length > 2 ? "\u661f\u671f" : "\u5468") : "") + week[this.getDay() + ""]);
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return fmt;
}
/**
 * 註冊全替換
 * @param  {String} s1 字串1
 * @param  {String} s2 字串2
 */
String.prototype.replaceAll = function (s1, s2) {
    return this.replace(new RegExp(s1, "gm"), s2);
}
/**
 * 註冊金額添加三位一撇
 */
String.prototype.toMoney = Number.prototype.toMoney = function () {
    return this.toString().replace(/\d+?(?=(?:\d{3})+$)/g, function (s) {
        return s + ',';
    });
}
/**
 * 数字四舍五入（保留n位小数）
 */
String.prototype.toFloat = Number.prototype.toFloat = function (n) {
    n = n ? parseInt(n) : 0;
    var number = this;
    if (n <= 0) return Math.round(number);
    number = Math.round(number * Math.pow(10, n)) / Math.pow(10, n);
    return number;
}
/**
 * 百分数转小数
 */
String.prototype.toPoint = function () {
    return this.replace("%", "") / 100;
}
/**
 * 小数转化为分数
 */
String.prototype.toPercent = Number.prototype.toPercent = function () {
    var percent = Number(this * 100).toFixed(1);
    percent += "%";
    return percent;
}
/**
 * 刪除陣列內包含(undefined, null, 0, false, NaN and '')的資料
 */
Array.prototype.clear = function () {
    var newArray = [];
    for (var i = 0; i < this.length; i++) {
        if (this[i]) {
            newArray.push(this[i]);
        }
    }
    return newArray;
}
/**
 * 在數組指定位置添加元素
 * @param  {Number} index 位置
 * @param  {Object} item 要添加的元素；
 */
Array.prototype.insert = function (index, item) {
    this.splice(index, 0, item);
};
/**
 * 刪除陣列內指定元素
 */
Array.prototype.remove = function (val) {
    var index = this.indexOf(val);
    if (index > -1) {
        this.splice(index, 1);
    }
};
/**
 * 方法陣列內等待
 */
jQuery.whenArray = function (array) {
    return jQuery.when.apply(this, array);
};