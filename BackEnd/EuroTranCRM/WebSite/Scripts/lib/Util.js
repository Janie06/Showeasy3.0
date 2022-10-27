/*global $, alert, g_cus, g_de, g_api, g_db, g_ul,  btoa, console, i18n */

var g_db = {
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
    orgid: "TE",
    userid: "EUROTRAN",
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
     * Check is edu environment
     * @returns {String} login method in localStorage
     */
    IsEDU: function () {
        'use strict';
        return g_db.GetItem("isedu");
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
        dicParameters.ORIGID = g_gd.orgid;
        dicParameters.USERID = g_gd.userid;
        dicParameters.MODULE = i_sModuleName;
        dicParameters.TYPE = i_sFuncName;
        dicParameters.PROJECT = g_gd.projectname;
        dicParameters.PROJECTVER = g_gd.projectver;
        dicParameters.TRACEDUMP = null;

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

        dicParameters.CUSTOMDATA.module_id = "WebSite";

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
        dicParameters.ORIGID = g_gd.orgid;
        dicParameters.USERID = g_gd.userid;
        dicParameters.MODULE = '';
        dicParameters.TYPE = i_sFuncName;
        dicParameters.PROJECT = g_gd.projectname;
        dicParameters.PROJECTVER = g_gd.projectver;
        dicParameters.TRACEDUMP = null;

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

        dicParameters.CUSTOMDATA.module_id = "WebSite";

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
                if (res.RESULT === -1) {
                    //layer.alert(i18next.t("message.TokenVerifyFailed"), { icon: 0, title: i18next.t("common.Tips") }, function (index) {
                    //    window.top.location.href = '/Page/login.html';
                    //});
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
/**
 * 如果頁面js需要動態加載請在配置在這裡
 *
 *
 *
 *
 * window.UEDITOR_HOME_URL = "/xxxx/xxxx/";
 */
(function ($, w, d) {
    /******************取得host***************************/
    w.gWebUrl = window.location.origin || gethost();
    //w.gServerUrl = 'https://www.eurotran.com:9001';
    //w.gServerUrl = 'https://www.origtek.com:8106';
    w.gServerUrl = 'http://localhost:80';

    if ('www.eurotran.com.tw|www.g-yi.com.cn'.indexOf(window.location.hostname) > -1) {
        w.gServerUrl = 'https://www.eurotran.com:9001';
    }
    else if ('www.origtek.com'.indexOf(window.location.hostname) > -1) {
        w.gServerUrl = 'https://www.origtek.com:8106';
    }
    else if ('192.168.1.105'.indexOf(window.location.hostname) > -1) {
        w.gServerUrl = 'https://192.168.1.105:9001';
    }
    if (window.location.pathname.indexOf('/TG/') > -1) {
        g_gd.orgid = 'TG';
    }
    else if (window.location.pathname.indexOf('/SG/') > -1) {
        g_gd.orgid = 'SG';
    }

    /**
     * 定義系統所有公用 Service.fnction
    */
    w.ComFn = {
        GetOrgInfo: 'GetOrgInfo',
        GetSysSet: 'GetSysSet',
        GetArguments: 'GetArguments',
        GetNewsCount: 'GetNewsCount',
        GetNewsPage: 'GetNewsPage',
        GetExhibitionPage: 'GetExhibitionPage',
        GetNewsInfo: 'GetNewsInfo',
        GetFileList: 'GetUploadFiles',
		GetExhibitionAppoint: 'GetExhibitionAppoint'
    };

    /**
     * 定義系統所有公用 Service
    */
    w.Service = {
        apiappcom: 'Common',
        apite: 'TEAPI',
        apitg: 'TGAPI',
        apiwebcom: 'Com'
    };

    /**
     * For display javascript exception on UI
    */
    w.onerror = function (message, source, lineno, colno, error) {
        console.log(source + " line:" + lineno + " colno:" + colno + " " + message);
        if (parent.SysSet && parent.SysSet.IsOpenMail === 'Y') {
            g_api.ConnectLite('Log', 'ErrorMessage', {
                ErrorSource: source,
                Errorlineno: lineno,
                Errorcolno: colno,
                ErrorMessage: message
            }, function (res) {
                if (res.RESULT) { }
            });
        }
    };

    /**
    * 依據組織信息設定website
    * @param {Function} callback  回調函數
    * @param {Boolean} isreget 是否查詢DB
    */
    w.runByOrgInfo = function (callback, isreget) {
        isreget = isreget || false;
        var org = g_db.GetDic('OrgInfo');
        if (!org || isreget) {
            g_api.ConnectLite(Service.apiwebcom, ComFn.GetOrgInfo, {}, function (res) {
                if (res.RESULT) {
                    org = res.DATA.rel;
                    g_db.SetDic('OrgInfo', org);
                    if (typeof callback === 'function') { callback(org); }
                }
            });
        }
        else {
            if (typeof callback === 'function') { callback(org); }
        }
    };

    /**
     * Ajax是否等待
    */
    w.IsWaiting = null;

    /**
    * 翻譯語系
    * @param {HTMLElement} dom  翻譯回調函數
    */
    w.transLang = function (dom) {
        i18next = ("undefined" == typeof i18next) ? parent.i18next : i18next;

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
     * 設定多於系
     * @param {String} lng 語種
     * @param {String} dom 要翻譯的html標籤
     * @param {Function} callback 回調函數
     */
    w.setLang = function (lng, dom, callback) {
        if (!lng) return;

        g_ul.SetLang(lng);

        i18next = ("undefined" == typeof i18next) ? parent.i18next : i18next;

        $.getJSON(gServerUrl + "/Scripts/lang/" + (g_gd.orgid || 'TE') + "/" + lng + ".json?v=" + new Date().getTime().toString(), function (json) {
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
        }).error(function () {
            g_api.ConnectLite('Language', 'CreateLangJson', {}, function (res) {
                if (res.RESULT) {
                    setLang(lng, dom, callback);
                }
            });
        });
    };

    /**
    * 開啟Waiting視窗
    * @param  {String} msg 提示文字
    */
    w.showWaiting = function (msg) {
        $.blockUI({
            message: $('<div id="Divshowwaiting"><img src="/images/ajax_loading2.gif">' + (msg || 'Waiting...') + '</div>'),
            css: {
                'font-size': (navigator.userAgent.match(/mobile/i) ? '20px' : '36px'),
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
        window.location.href = sUrl;
        closeWaiting();
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
    * 取得Url參數
    * @param  {String} name 取得部分的名稱 例如輸入"Action"，就能取到"Add"之類參數
    * @return {String}參數值
    */
    w.getUrlParam = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //構造一個含有目標參數的正則表達式對象
        var r = window.location.search.substr(1).match(reg);  //匹配目標參數
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
    * 產生guid
    * @param  {Number} len 指定长度,比如guid(8, 16) // "098F4D35"
    * @param  {Number} radix 基数
    * @return {String} guid
    */
    w.guid = function (len, radix) {
        var buf = new Uint16Array(8),
            cryptObj = window.crypto || window.msCrypto, // For IE11
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
            window.event.cancelBubble = true;
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
            window.event.returnValue = false;
        return false;
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
    w.createRadios = function (list, id, name, _id, showid) {
        list = list || [];
        var strHtml = '',
            intCount = list.length;
        if (intCount > 0) {
            $.each(list, function (idx, obj) {
                var inputradio = $('<input />', {
                    type: 'radio',
                    id: _id + '_' + idx,
                    name: _id,
                    value: $.trim(obj[id])
                }).attr('val', $.trim(obj[id]));
                strHtml += '<label for="' + _id + '_' + idx + '">' + inputradio[0].outerHTML + ((showid ? $.trim(obj[id]) + '-' : '') + $.trim(obj[name])) + "</label>";
            });
        }
        return strHtml;
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
        var strHtml = '',
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
                strHtml += "<label for='" + id + (_id === undefined ? '' : _id) + '_' + idx + "' style='" + (intCount == idx + 1 ? '' : 'float:left;') + "padding-left: 10px'>" + inputradio[0].outerHTML + $.trim(obj[name]) + "</label>";
            });
        }
        return strHtml;
    };

    /**
    * 取得自定義json值
    * @param  {Object} json json對象
    * @param  {Object} name json鍵值
    * @return {String} sLast 最終要獲取的對應的鍵值對的值
    */
    w.getJsonVal = function (json, name) {
        var oLast = json,
            saName = name.split('_');
        for (var i = 0; i < saName.length; i++) {
            if (!oLast[saName[i]]) {
                oLast = '';
                break;
            }
            oLast = oLast[saName[i]];
        }
        return oLast;
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
                            $(this).trigger("change");
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
                        break;
                    case 'radio':
                        this.checked = this.value === sValue;
                        break;
                }
                if ((sId == 'ModifyUser' || sId == 'CreateUser') && 'select-one'.indexOf(this.type) === -1) {
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
            }
        }
    };

    /**
    * 目的：抓去網站設定
    * @param {Function} callback 回調函數
    * @param {Object} settype 設定類別
    * @param {String} lang 語系
    * @param {String} parentid 父層ID
    * @param {Boolean} haschild 是否只抓去父層
    * @param {Boolean} single 單筆
    */
    w.fnGetWebSiteSetting = function (callback, settype, lang, parentid, haschild, single) {
        var oQueryPm = { SetType: settype, LangId: lang };
        if (parentid) { oQueryPm.ParentId = parentid; }
        if (haschild) { oQueryPm.HasChild = haschild; }
        if (single) { oQueryPm.Single = single; }
        return g_api.ConnectLite(Service.apiwebcom, 'GetWebSiteSetting', oQueryPm, function (res) {
            if (res.RESULT) {
                if (typeof callback === 'function') {
                    callback(res.DATA.rel);
                }
            }
        });
    };

    /**
    * 目的：抓去網站設定（分頁）
    * @param {Object} args 參數
    */
    w.fnGetWebSiteSettingPage = function (args) {
        return g_api.ConnectLite(Service.apiwebcom, 'GetWebSiteSettingPage', args, function (res) {
            if (res.RESULT) {
                if (typeof args.CallBack === 'function') {
                    args.CallBack(res.DATA.rel);
                }
            }
        });
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
    * 目的：回頂端
    */
    w.goTop = function () {
        var oTop = $('<div>', {
            class: 'gotop',
            html: '<img src="../images/gotop_1.png" />', click: function () {
                return $("body,html").animate({ scrollTop: 0 }, 120), !1;
            }
        });
        $('body').append(oTop.hide());//添加置頂控件
        $(window).on('scroll', function () {
            var h = ($(d).height(), $(this).scrollTop()),
                toolbarH = -40,
                toolbarCss = {};
            h > 0 ? oTop.show() : oTop.hide();
            if (h > 40) {
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
            explorer = window.navigator.userAgent;
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
        s = parseFloat(((s || 0) + "").replace(/[^\d\.-]/g, "")).toFixed(p) + "";
        var l = s.split(".")[0].split("").reverse(),
            r = s.split(".")[1] || Array(n + 1).join(0),
            t = "";
        for (i = 0; i < l.length; i++) {
            t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? "," : "");
        }
        return t.split("").reverse().join("") + (n === 0 ? '' : "." + r);
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
    /********************************************
    * 函數名稱：showMsg
    * 目的：顯示提示訊息，替代ALERT功能
    * 作者：John
    * 時間：2015/06/12
    * 參數說明：
    *********************************************/
    /**
     * 消息提示
     * @param  {String} msg 提示文字
     * @param  {String} type 提示類型
     */
    w.showMsg = function (msg, type) {
        var showDuration = "300",
            hideDuration = "1000",
            timeOut = "3000",
            extendedTimeOut = "1000",
            showEasing = "swing",
            hideEasing = "linear",
            showMethod = "fadeIn",
            hideMethod = "fadeOut",
            addClear = false,
            debug = false,
            newestOnTop = true,
            progressBar = false,
            positionClass = 'toast-edit-center',
            preventDuplicates = true,
            addBehaviorOnToastClick = true,
            BehaviorFunc = null,
            type = type || 'info';
        getMsgBox(type, msg, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc);
    };

    /**
    * 目的：彈出帶一個按鈕的提示窗口並執行方法
    * 參數說明：
    * msg     ：提示訊息
    * title   ：提示窗口的標題success....
    * Position：提示窗口的位置
    * type :提示方式success、 Info、Warning、Error四種
    * msg              :提示框中顯示的訊息
    * title            :提示框顯示的其他html內容
    * showDuration     :显示时间
    * hideDuration     :隐藏时间
    * timeOut          :超时
    * extendedTimeOut  :延长超时
    * showEasing       :
    * hideEasing       :
    * showMethod       :顯示的方式
    * hideMethod       :隱藏的方式
    * addClear         :是否添加清除
    * debug            :
    * newestOnTop      :
    * progressBar      :是否添加進度顯示
    * positionClass    :位置
    * preventDuplicates:是否防止重复
    * func             :執行方法
    *********************************************/
    function getMsgBox(type, msg, showDuration, hideDuration, timeOut, extendedTimeOut, showEasing, hideEasing, showMethod, hideMethod, addClear, debug, newestOnTop, progressBar, positionClass, preventDuplicates, addBehaviorOnToastClick, BehaviorFunc) {
        var title = i18next.t("message.Tips") || '<span data-i18n="message.Tips"></span>';
        toastr.options = {
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
    }

    /**
    * 取得host
    * @return {String} host Url
     */
    function gethost() {
        var g_ServerUrl = location.origin + '/';
        if (!window.location.origin) {
            gWebUrl = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
        }
        return g_ServerUrl;
    }

    if (!w.browser) {
        var userAgent = navigator.userAgent.toLowerCase(), uaMatch;
        window.browser = {}

        /**
         * 判断是否为ie
         */
        function isIE() {
            return ("ActiveXObject" in window);
        }
        /**
         * 判断是否为谷歌浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/chrome\/([\d.]+)/);
            if (uaMatch != null) {
                window.browser['name'] = 'chrome';
                window.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为火狐浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/firefox\/([\d.]+)/);
            if (uaMatch != null) {
                window.browser['name'] = 'firefox';
                window.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为opera浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/opera.([\d.]+)/);
            if (uaMatch != null) {
                window.browser['name'] = 'opera';
                window.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 判断是否为Safari浏览器
         */
        if (!uaMatch) {
            uaMatch = userAgent.match(/safari\/([\d.]+)/);
            if (uaMatch != null) {
                window.browser['name'] = 'safari';
                window.browser['version'] = uaMatch[1];
            }
        }
        /**
         * 最后判断是否为IE
         */
        if (!uaMatch) {
            if (userAgent.match(/msie ([\d.]+)/) != null) {
                uaMatch = userAgent.match(/msie ([\d.]+)/);
                window.browser['name'] = 'ie';
                window.browser['version'] = uaMatch[1];
            } else {
                /**
                 * IE10
                 */
                if (isIE() && !!d.attachEvent && (function () { "use strict"; return !this; }())) {
                    window.browser['name'] = 'ie';
                    window.browser['version'] = '10';
                }
                /**
                 * IE11
                 */
                if (isIE() && !d.attachEvent) {
                    window.browser['name'] = 'ie';
                    window.browser['version'] = '11';
                }
            }
        }

        /**
         * 注册判断方法
         */
        if (!$.isIE) {
            $.extend({
                isIE: function () {
                    return (window.browser.name == 'ie');
                }
            });
        }
        if (!$.isChrome) {
            $.extend({
                isChrome: function () {
                    return (window.browser.name == 'chrome');
                }
            });
        }
        if (!$.isFirefox) {
            $.extend({
                isFirefox: function () {
                    return (window.browser.name == 'firefox');
                }
            });
        }
        if (!$.isOpera) {
            $.extend({
                isOpera: function () {
                    return (window.browser.name == 'opera');
                }
            });
        }
        if (!$.isSafari) {
            $.extend({
                isSafari: function () {
                    return (window.browser.name == 'safari');
                }
            });
        }
    }
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
* @param  {Date} date 日期
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
* 百分数转小数
*/
String.prototype.toPoint = function () {
    return this.replace("%", "") / 100;
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
jQuery.whenArray = function (array) {
    return jQuery.when.apply(this, array);
};