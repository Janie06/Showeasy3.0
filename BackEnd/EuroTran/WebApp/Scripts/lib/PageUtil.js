/*!
 * PageUtil.js
 * Copyright (c) 2018 CreativeDream
 * Website: xxx
 * Version: 1.0.0 (03-05-2018)
 * Requires: jQuery v1.7.1 or later
 */
var i18next = "undefined" === typeof i18next ? parent.top.i18next : i18next,
    IsWaiting = null,
    bLeavePage = false,

    g_db = {
        /**
         * Check the capability
         * @private
         * @method SupportLocalStorage
         * @return {Object} description
         */
        SupportLocalStorage: function () {
            'use strict';
            return typeof localStorage !== "undefined";
        },

        /**
         * Insert data
         * @private
         * @method SetItem
         * @param {String} sKey key鍵
         * @param {Object} sValue Value值
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
         * @param {String} sKey key鍵
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
         * @param {String} sKey key鍵
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
         * @param {String} sKey key鍵
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
         * @param {String} sKey key鍵
         * @param {Object} dicValue json對象
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
    },

    g_gd = {
        webapilonginurl: "/api/Service/GetLogin",
        webapiurl: "/api/Cmd/GetData",
        projectname: "Eurotran",
        projectver: "Origtek",
        relpath: "",
        debugmode: window.location.host === '192.168.1.105',
        debugcolor: "#732C6B",
        IsEDU: g_db.GetItem("isedu") === "true"
    },

    g_ul = {
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
         * @return {Object} 簽名
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
         * @param   {String} sLang 多語系值
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
         * @param {String} sLoginMethod method
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
         * 產生隨機數
         * @param  {Number} len 指定长度,比如random(8)
         * @return {String} rnd 亂數碼
         */
        RndNum: function (len) {
            var rnd = "";
            len = len || 10;
            for (var i = 0; i < len; i++)
                rnd += Math.floor(Math.random() * 10);
            return rnd;
        }
    },

    g_api = {
        ConnectLite: function (i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn, i_sShwd) {
            window.IsWaiting = i_sShwd;
            return this.ConnectLiteWithoutToken(i_sModuleName, i_sFuncName, i_dicData, i_sSuccessFunc, i_FailFunc, i_bAsyn);
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
            dicParameters.TRACEDUMP = null; //

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

            dicData.failfunc = "function" === typeof i_FailFunc ? i_FailFunc : function (jqXHR, textStatus, errorThrown) {
                alert("ConnectLite Fail jqXHR:" + jqXHR + " textStatus:" + textStatus + " errorThrown:" + errorThrown);
            };

            dicData.useasync = "boolean" === typeof i_bAsyn ? i_bAsyn : true;
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
                        nonce = g_ul.RndNum();
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

var CanDo = (function (w, d) {
    'use strict';
    var CanDo = function (config) {
        return new CanDo.fn._init(config);
    };

    CanDo.fn = CanDo.prototype = {
        constructor: CanDo,
        _init: function (config) {
            var cando = this,
                dfoptions = {
                    cusBtns: [],//客製化按鈕
                    goTop: true,//置頂圖標
                    searchBar: true,//是否有搜尋區塊
                    goBack: true,//是否返回查詢頁面
                    insertGo: true,//新增完后是否跳轉
                    updateGo: true,//修改完后是否跳轉
                    deleteGo: true,//刪除完后是否跳轉
                    pageSize: parent.top.SysSet.GridRecords || 0,//Grid顯示筆數
                    gridPages: parent.top.SysSet.GridPages || 15,//Grid按鈕顯示數量
                    queryPageidx: 1,//當前頁面索引
                    toFirstPage: false
                };

            $.extend(dfoptions, config);
            for (var key in cando._pageParam) {
                var val = dfoptions[key];
                if (val) {
                    cando._pageParam[key] = val;
                }
            }

            cando.options = dfoptions;
            cando.ids = {};
            cando.params = {};
            cando.UE_Editor = {};
            cando.currentPageValue = [];
            cando.validator = null;
            cando.CheckId = '';
            cando.action = cando._getAction();
            cando.ProgramId = cando._getProgramId();
            cando.QueryPrgId = cando._getQueryPrgId();
            cando.EditPrgId = cando._getEditPrgId();
            w.sProgramId = cando.ProgramId;
            cando.data = cando._data;
            cando.pageParam = cando._pageParam;
            cando.setGrid = function (grid) {
                cando.Grid = grid;
            };
            if (typeof cando.options.cusBtns === 'function') {
                cando.options.cusBtns = cando.options.cusBtns(cando);
            }

            var idKeys = cando.options.idKeys,
                paramKeys = cando.options.paramKeys,
                index,
                _key;
            if (idKeys && $.isArray(idKeys)) {
                for (index in idKeys) {
                    if ('clear,insert,remove'.indexOf(index) === -1) {
                        _key = idKeys[index];
                        cando.ids[_key] = cando._getUrlParam(_key);
                        cando.CheckId += cando.ids[_key] || '';
                    }
                }
            }
            if (paramKeys && $.isArray(paramKeys)) {
                for (index in paramKeys) {
                    if ('clear,insert,remove'.indexOf(index) === -1) {
                        _key = paramKeys[index];
                        cando.params[_key] = cando._getUrlParam(_key);
                    }
                }
            }

            //ToolBar(功能函數)
            cando.initButtonHandler = function (inst, e) {
                return cando._buttonHandler.call(cando, inst, e);
            };
            //查詢（分頁）
            cando.getPage = function (args) {
                return cando._getPage(args);
            };
            //查詢（單筆）
            cando.getOne = function (args) {
                return cando._getOne(args);
            };
            //新增
            cando.getInsert = function (p, args) {
                return cando._getInsert(args);
            };
            //修改
            cando.getUpdate = function (args) {
                return cando._getUpdate(args);
            };
            //刪除
            cando.getDelete = function (args) {
                return cando._getDelete(args);
            };
            //匯出
            cando.getExcel = function (args) {
                if (args) {
                    return cando._getPage(args);
                }
            };
            //Grid新增
            cando.gridInsert = function (args) {
                if (args) {
                    return cando._gridInsert(args);
                }
            };
            //Grid新增
            cando.gridUpdate = function (args) {
                if (args) {
                    return cando._gridUpdate(args);
                }
            };
            //Grid新增
            cando.gridDelete = function (args) {
                if (args) {
                    return cando._gridDelete(args);
                }
            };

            for (_key in cando) {
                var _key_ = _key.replace('_', ''),
                    _val = dfoptions[_key_];
                if (_val && _key !== 'constructor') {
                    cando[_key_] = _val;
                }
            }
            if (!cando.form) {
                cando.form = cando._form;
            }
            if (!cando.jsGrid) {
                cando.jsGrid = cando._jsGrid;
            }
            //pageInit(功能初始化)
            cando._pageInit().done(function () {
                var editorIds = cando.options.ueEditorIds;
                if ($.isArray(editorIds)) {
                    for (var index in editorIds) {
                        if ('clear,insert,remove'.indexOf(index) === -1) {
                            var key = editorIds[index];
                            if ($('#UE_' + key).length > 0) {
                                cando.UE_Editor[key] = UE.getEditor('UE_' + key);
                            }
                        }
                    }
                }
                if (typeof cando.pageInit === 'function') {
                    cando.pageInit.call(cando, cando);
                }
                if (typeof cando.options.validRulesCus === 'function') {
                    cando.options.validRulesCus(cando);
                }
                if (typeof cando.options.validRules === 'function') {
                    cando.options.validRules = cando.options.validRules(cando);
                }
                cando.validator = cando.form.validate(cando.options.validRules || {});
            });
        },
        _service: {
            cotrl: '/Controller.ashx',
            com: 'Common',
            opm: 'OpmCom',
            eip: 'EipCom',
            sys: 'SysCom',
            auth: 'Authorize'
        },
        _api: {
            getpage: 'QueryPage',
            getlist: 'QueryList',
            getone: 'QueryOne',
            getcout: 'QueryCout',
            insert: 'Insert',
            update: 'Update',
            delete: 'Delete',
            ginsert: 'GridInsert',
            gupdate: 'GridUpdate',
            gdelete: 'GridDelete',
            order: 'UpdateOrderByValue'
        },
        _data: {},
        _form: $('#form_main'),
        _jsGrid: $("#jsGrid"),
        _pageParam: {
            pageIndex: 1,
            pageSize: 10,
            sortField: 'OrderByValue',
            sortOrder: 'asc'
        },
        /**
         * 通過url獲取程式id
         * @param  {String} path 文件路徑
         * @return {String} 程式ID
         */
        _getProgramId: function (path) {
            var herf = path || d.location.href,
                herfs = herf.split('/');
            herfs = herfs[herfs.length - 1].split('.');
            return herfs[0] || '';
        },
        /**
         * 通過查詢程式ID獲取編輯頁面程式ID
         * @return {String} 編輯頁面程式ID
         */
        _getEditPrgId: function () {
            var prgid = this._getProgramId();
            return prgid.replace('_Qry', '_Upd');
        },
        /**
         * 通過編輯程式ID獲取查詢程式ID
         * @return {String} 編輯頁面程式ID
         */
        _getQueryPrgId: function () {
            var prgid = this._getProgramId();
            return prgid.replace('_Upd', '_Qry');
        },
        /**
         * ToolBar 按鈕事件 function
         * @param   {Object} inst 按鈕物件對象
         * @param   {Object} e 事件對象
         * @return {Boolean} 停止標記
         */
        _buttonHandler: function (inst, e) {
            var cando = this,
                id = inst.id;
            if (!$.isEmptyObject(cando.UE_Editor)) {
                for (var key in cando.UE_Editor) {
                    if ($('#' + key).length > 0) {
                        $('#' + key).val(cando.UE_Editor[key].getContentTxt());
                    }
                }
            }
            switch (id) {
                case "Toolbar_Qry":

                    if (!cando.form.valid()) {
                        cando.validator.focusInvalid();
                        return false;
                    }

                    var iNum = $('#PerPageNum').val();
                    cando.Grid && (cando.Grid.pageSize = iNum === '' ? cando.options.pageSize || 10 : iNum);
                    cando._cacheQueryCondition();
                    cando.Grid && cando.Grid.openPage(cando.options.toFirstPage ? 1 : cando.pageParam.pageIndex);

                    if (typeof cando.options.afterQuery === 'function') {
                        cando.options.afterQuery(cando);
                    }

                    break;
                case "Toolbar_Save":

                    if (!cando.form.valid()) {
                        cando.validator.focusInvalid();
                        return false;
                    }

                    if (cando.action === 'add') {
                        cando.getInsert.call(cando, cando, cando.action);
                    }
                    else {
                        cando.getUpdate.call(cando, cando);
                    }

                    break;
                case "Toolbar_ReAdd":

                    if (!cando.form.valid()) {
                        cando.validator.focusInvalid();
                        return false;
                    }
                    cando.getInsert.call(cando, cando, 'readd');

                    break;
                case "Toolbar_Clear":

                    cando._clearPageVal();

                    break;
                case "Toolbar_Leave":

                    cando._pageLeave();

                    break;

                case "Toolbar_Add":
                    parent.openPageTab(cando.EditPrgId, '?Action=Add');

                    break;
                case "Toolbar_Upd":

                    break;
                case "Toolbar_Copy":

                    break;
                case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                    layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                        cando.getDelete.call(cando);
                        layer.close(index);
                    });

                    break;
                case "Toolbar_Exp":
                    if (cando.Grid.data.length === 0) {
                        showMsg(i18next.t("message.NoDataExport"));// ╠message.NoDataExport⇒沒有資料匯出╣
                        return false;
                    }
                    cando.getExcel({ Excel: true });

                    break;
                default:
                    {
                        var actions = cando.options.cusBtns.filter(function (item) { return item.id === id; });
                        if (actions.length > 0 && typeof actions[0].action === 'function') {
                            actions[0].action(cando);
                        }
                        else {
                            alert("No handle '" + id + "'");
                        }
                    }

                    break;
            }
        },
        /**
         * 通過編輯程式ID獲取查詢程式ID
         */
        _initGrid: function () {
            var cando = this,
                iheight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
            if (typeof cando.options.gridFields === 'function') {
                cando.options.gridFields = cando.options.gridFields(cando);
            }
            cando.jsGrid.jsGrid({
                width: "100%",
                height: iheight + "px",
                autoload: true,
                pageLoading: true,
                inserting: cando.options.inserting || false,
                editing: true,
                sorting: true,
                paging: true,
                pageIndex: cando.options.toFirstPage ? 1 : cando.options.queryPageidx,
                pageSize: cando.options.pageSize,
                pageButtonCount: cando.options.gridPages,
                invalidMessage: i18next.t('common.InvalidData'),
                confirmDeleting: true,
                deleteConfirm: i18next.t('message.ConfirmToDelete'),
                pagePrevText: "<",
                pageNextText: ">",
                pageFirstText: "<<",
                pageLastText: ">>",
                fields: cando.options.gridFields,
                onItemEditing: function (args) {
                    if (typeof cando.options.onItemEditing === 'function') {
                        cando.options.onItemEditing(args);
                    }
                },
                onPageChanged: function (args) {
                    if (typeof cando.options.onPageChanged === 'function') {
                        cando.options.onPageChanged(cando, args);
                    }
                    else {
                        cando._cacheQueryCondition(args.pageIndex);
                    }
                },
                rowClick: function (args) {
                    if (typeof cando.options.rowClick === 'function') {
                        cando.options.rowClick(cando, args);
                    }
                    else {
                        if (navigator.userAgent.match(/mobile/i)) {
                            var _param = cando._getParamsStr(cando.ids, args.item);
                            cando._goToEdit(cando.EditPrgId, '?Action=Upd' + _param);
                        }
                    }
                },
                rowDoubleClick: function (args) {
                    if (typeof cando.options.rowDoubleClick === 'function') {
                        cando.options.rowDoubleClick(cando, args);
                    }
                    else {
                        var _param = cando._getParamsStr(cando.ids, args.item);
                        parent.top.openPageTab(cando.EditPrgId, '?Action=Upd' + _param);
                    }
                },
                controller: {
                    loadData: function (args) {
                        return cando.getPage(args);
                    },
                    insertItem: function (args) {
                        return cando.gridInsert(args);
                    },
                    updateItem: function (args) {
                        return cando.gridUpdate(args);
                    },
                    deleteItem: function (args) {
                        return cando.gridDelete(args);
                    }
                },
                onInit: function (args) {
                    cando.setGrid(args.grid);
                }
            });
        },
        /**
         * 基本查詢（分頁）
         * @param {String} args 參數
         * @return {Object} Ajax對象
         */
        _getPage: function (args) {
            var cando = this,
                qrParam = cando._getFormSerialize();
            $.extend(qrParam, cando.pageParam, args);
            cando.pageParam.pageIndex = qrParam.pageIndex;

            return g_api.ConnectLite(cando.ProgramId, cando._api.getpage, qrParam, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (args.Excel) {//匯出
                        cando._downLoadFile.call(cando, oRes);
                        if (layer && args.Index) {
                            layer.close(args.Index);
                        }
                    }
                }
            });
        },
        /**
         * 基本查詢（單筆）
         * @return {Object} Ajax對象
         */
        _getOne: function () {
            var cando = this,
                data = clone(cando.ids);
            if (typeof cando.options.getOneParams === 'function') {
                data = cando.options.getOneParams(cando);
            }
            return g_api.ConnectLite(cando.ProgramId, cando._api.getone, data,
                function (res) {
                    if (res.RESULT) {
                        cando.data = res.DATA.rel;
                        var jsonStrKeys = cando.options.jsonStrKeys;
                        if (jsonStrKeys && $.isArray(jsonStrKeys)) {
                            for (var index in jsonStrKeys) {
                                if ('clear,insert,remove'.indexOf(index) === -1) {
                                    var key = jsonStrKeys[index],
                                        val = cando.data[key];
                                    if (typeof val === 'string' && val !== '') {
                                        cando.data[key] = JSON.parse(val);
                                    }
                                }
                            }
                        }
                        if (typeof cando.options.getOneBack === 'function') {
                            cando.options.getOneBack(cando, cando.data);
                        }
                        else {
                            cando._setFormVal(cando.data);
                            cando._setUEValues(cando.data);
                        }
                        setTimeout(function () {
                            cando._getPageVal();//緩存頁面值，用於清除
                        }, 500);
                    }
                },
                function () {
                    showMsg(i18next.t("message.GetOne_Error"), 'error'); // ╠message.GetOne_Error⇒獲取當前資料異常╣
                });
        },
        /**
         * 基本新增
         * @param {String} flag 新增 or 儲存后新增
         * @return {Object} Ajax對象
         */
        _getInsert: function (flag) {
            var cando = this,
                data = cando._getFormSerialize();
            data = cando._getUEValues(data);

            if (typeof cando.options.getInsertParams === 'function') {
                data = cando.options.getInsertParams(cando, data);
            }

            data = $.extend(cando.data, data);

            return g_api.ConnectLite(cando.ProgramId, cando._api.insert, data, function (res) {
                if (res.RESULT) {
                    w.bRequestStorage = false;
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getInsertBack === 'function') {
                        cando.options.getInsertBack(cando, oRes, flag);
                    }
                    else {
                        if (!cando.options.goBack) {
                            var _param = cando._getParamsStr(cando.ids, oRes);
                            showMsgAndGo(i18next.t("message.Insert_Success"), cando.ProgramId, '?Action=Upd' + _param); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            if (flag === 'add') {
                                if (cando.options.insertGo) {
                                    showMsgAndGo(i18next.t("message.Insert_Success"), cando.QueryPrgId); // ╠message.Insert_Success⇒新增成功╣
                                }
                                else {
                                    showMsg(i18next.t("message.Insert_Success"), 'success'); //╠message.Insert_Success⇒新增成功╣
                                }
                            }
                            else {
                                showMsgAndGo(i18next.t("message.Insert_Success"), cando.ProgramId, '?Action=Add'); // ╠message.Insert_Success⇒新增成功╣
                            }
                        }
                    }
                }
                else {
                    showMsg(i18next.t("message.Insert_Failed") + '<br>' + res.MSG, 'error');// ╠message.Insert_Failed⇒新增失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Insert_Error"), 'error');// ╠message.Insert_Error⇒新增資料異常╣
            });
        },
        /**
         * 基本修改
         * @return {Object} Ajax對象
         */
        _getUpdate: function () {
            var cando = this,
                data = cando._getFormSerialize();
            data = cando._getUEValues(data);

            if (typeof cando.options.getUpdateParams === 'function') {
                data = cando.options.getUpdateParams(cando, data);
            }

            data = $.extend(cando.data, data, cando.ids);

            return g_api.ConnectLite(cando.ProgramId, cando._api.update, data, function (res) {
                if (res.RESULT) {
                    w.bRequestStorage = false;
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getUpdateBack === 'function') {
                        cando.options.getUpdateBack(cando, oRes);
                    }
                    else {
                        if (cando.options.updateGo) {
                            showMsgAndGo(i18next.t("message.Modify_Success"), cando.QueryPrgId); //╠message.Modify_Success⇒修改成功╣
                        }
                        else {
                            showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                            if (w.bLeavePage) {
                                setTimeout(function () {
                                    cando._pageLeave();
                                }, 1000);
                            }
                        }
                    }
                }
                else {
                    showMsg(i18next.t("message.Modify_Failed") + '<br>' + res.MSG, 'error');// ╠message.Modify_Failed⇒修改失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Modify_Error"), 'error');//╠message.Modify_Error⇒修改資料異常╣
            });
        },
        /**
         * 基本刪除
         * @param {String} args 參數
         * @return {Object} Ajax對象
         */
        _getDelete: function (args) {
            var cando = this,
                data = clone(cando.ids);
            if (typeof cando.options.getDeleteParams === 'function') {
                data = cando.options.getDeleteParams(cando);
            }
            return g_api.ConnectLite(cando.ProgramId, cando._api.delete, data, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getDeleteBack === 'function') {
                        cando.options.getDeleteBack(cando, oRes);
                    } else {
                        if (cando.options.deleteGo) {
                            showMsgAndGo(i18next.t("message.Delete_Success"), cando.QueryPrgId); // ╠message.Delete_Success⇒刪除成功╣
                        }
                        else {
                            showMsg(i18next.t("message.Delete_Success"), 'success'); //╠message.Delete_Success⇒刪除成功╣
                        }
                    }
                }
                else {
                    showMsg(i18next.t("message.Delete_Failed") + '<br>' + res.MSG, 'error');// ╠message.Delete_Failed⇒刪除失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Delete_Error"), 'error'); // ╠message.Delete_Error⇒刪除資料異常╣
            });
        },
        /**
         * Grid新增
         * @param {String} data 新增參數
         * @return {Object} Ajax對象
         */
        _gridInsert: function (data) {
            var cando = this;
            if (typeof cando.options.getGridInsertParams === 'function') {
                data = cando.options.getGridInsertParams(cando, data);
            }
            return g_api.ConnectLite(cando.ProgramId, cando._api.ginsert, data, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getGridInsertBack === 'function') {
                        cando.options.getGridInsertBack(cando, oRes);
                    }
                    else {
                        showMsg(i18next.t("message.Insert_Success"), 'success'); // ╠message.Insert_Success⇒新增成功╣
                    }
                }
                else {
                    showMsg(i18next.t("message.Insert_Failed") + '<br>' + res.MSG, 'error');// ╠message.Insert_Failed⇒新增失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Insert_Error"), 'error');// ╠message.Insert_Error⇒新增資料異常╣
            });
        },
        /**
         * Grid修改
         * @param {String} data 修改參數
         * @return {Object} Ajax對象
         */
        _gridUpdate: function (data) {
            var cando = this;

            if (typeof cando.options.getGridUpdateParams === 'function') {
                data = cando.options.getGridUpdateParams(cando, data);
            }

            return g_api.ConnectLite(cando.ProgramId, cando._api.gupdate, data, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getGridUpdateBack === 'function') {
                        cando.options.getGridUpdateBack(cando, oRes);
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                    }
                }
                else {
                    showMsg(i18next.t("message.Modify_Failed") + '<br>' + res.MSG, 'error');// ╠message.Modify_Failed⇒修改失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Modify_Error"), 'error');//╠message.Modify_Error⇒修改資料異常╣
            });
        },
        /**
         * Grid刪除
         * @param {String} data 刪除參數
         * @return {Object} Ajax對象
         */
        _gridDelete: function (data) {
            var cando = this;
            if (typeof cando.options.getGridDeleteParams === 'function') {
                data = cando.options.getGridDeleteParams(cando);
            }
            return g_api.ConnectLite(cando.ProgramId, cando._api.gdelete, data, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (typeof cando.options.getGridDeleteBack === 'function') {
                        cando.options.getGridDeleteBack(cando, oRes);
                    } else {
                        showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                    }
                }
                else {
                    showMsg(i18next.t("message.Delete_Failed") + '<br>' + res.MSG, 'error');// ╠message.Delete_Failed⇒刪除失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Delete_Error"), 'error'); // ╠message.Delete_Error⇒刪除資料異常╣
            });
        },
        /**
         * 基本匯出
         * @param {String} args 參數
         */
        _getExcel: function (args) {
        },
        /**
         * 基本查詢（分頁）
         * @param {String} args 參數
         * @return {Object} Ajax對象
         */
        _pageInit: function (args) {
            var cando = this;
            if (navigator.userAgent.match(/mobile/i)) {
                $('.ismobile').hide();
            }

            if ($("#tabs").length > 0) {
                $("#tabs").tabs().find('li').on('click', function () {
                    var that = this;
                    $('#tabs>ul>li').removeClass('active');
                    $(this).addClass('active');
                    if (typeof cando.options.tabAction === 'function') {
                        cando.options.tabAction(that, cando);
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
                            if (typeof cando.options.onSelect === 'function') {
                                cando.options.onSelect(r, e);
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
                                if (typeof cando.options.onSelect === 'function') {
                                    cando.options.onSelect(r, e);
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
                                if (typeof cando.options.onSelect === 'function') {
                                    cando.options.onSelect(r, e);
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

            cando._keyInput();   //註冊欄位管控
            cando._select2Init();//初始化select2
            cando._uniformInit();//表單美化

            if (cando.options.goTop) {
                var elTop = $('<div>', {
                    class: 'gotop',
                    html: '<img src="../../images/gotop_1.png" />', click: function () {
                        return $("body,html").animate({ scrollTop: 0 }, 120), !1;
                    }
                });
                $('body').append(elTop.hide());//添加置頂控件
                $(d).on('scroll', function () {
                    var h = ($(d).height(), $(this).scrollTop()),
                        toolbarH = -45,
                        toolbarCss = {},
                        elToolBar = cando._getToolBar();
                    h > 0 ? elTop.fadeIn() : elTop.fadeOut();
                    if (h > 35) {
                        toolbarH = h - 80;
                        elToolBar.addClass('toolbar-float').removeClass('toolbar-fix');
                    }
                    else {
                        elToolBar.removeClass('toolbar-float').addClass('toolbar-fix');
                    }
                    elToolBar.css('margin-top', toolbarH + 'px');
                });
            }

            if (cando.ProgramId) {
                var lang = g_ul.GetLang() || 'zh-TW';
                cando._setLang(lang);//翻譯多語系
                cando._getPageVal(); //緩存頁面值，用於清除
                cando._createPageTitle(); //創建Header

                if (cando.options.searchBar) { //設置顯示于隱藏搜尋區塊
                    var iSearchBox_h = $('#searchbar').height(),
                        elSlideUpDown = $('<i>', {
                            class: 'fa fa-arrow-up slide-box',
                            click: function () {
                                if ($(this).hasClass('fa-arrow-up')) {
                                    $(this).removeClass('fa-arrow-up').addClass('fa-arrow-down');
                                    elSearchBox.slideUp();
                                    !!cando.Grid && (cando.Grid.height = cando.Grid.dfheight.replace('px', '') * 1 + iSearchBox_h + 'px');
                                }
                                else {
                                    $(this).removeClass('fa-arrow-down').addClass('fa-arrow-up');
                                    elSearchBox.slideDown();
                                    !!cando.Grid && (cando.Grid.height = cando.Grid.dfheight);
                                }
                                !!cando.Grid && cando.Grid.refresh();
                                //調整Grid slimscrollDIV高度，保證和實際高度一致
                                var elGridBox = $('.jsgrid-grid-body.slimscroll');
                                elGridBox.parent().css('height', elGridBox.css('height'));
                            }
                        }),
                        elSlideDiv = $('<div>', { class: 'col-sm-12 up-down-go' }).append(elSlideUpDown),
                        elSearchBox = $('#searchbar').after(elSlideDiv);
                }

                cando._reSetQueryPm();   //恢復之前查詢條件

                //加載按鈕權限
                return cando._getAuthority(cando).done(function () {

                });
            }
        },
        /**
         * 獲取權限
         * @return {Object} Ajax對象
         */
        _getAuthority: function (outterCando) {
            var cando = this,
                topmod = cando._getTopMod();

            return g_api.ConnectLite('Authorize', 'GetAuthorize',
                {
                    ProgramID: cando.ProgramId,
                    TopModuleID: topmod
                },
                function (res) {
                    if (res.RESULT) {
                        var authorize = res.DATA.rel,
                            btns = [],
                            hasBtn = {},
                            lastBtn = null,
                            elToolBar = cando._getToolBar(),
                            initToolbar = function () {//等待 ToolBar（wedget）初始化ok后在執行，否則會報錯
                                elToolBar.ToolBar({
                                    btns: btns,
                                    fncallback: cando.initButtonHandler
                                });
                                cando._transLang(elToolBar);
                            },
                            delayInitToolbar = function () {
                                if ($.fn.ToolBar) {
                                    initToolbar();
                                }
                                else {
                                    delayInitToolbar();
                                }
                            };
                        $.each(authorize, function (idx, roleright) {
                            if (roleright.AllowRight) {
                                var saRights = roleright.AllowRight.split('|');
                                $.each(saRights, function (e, btnright) {
                                    var sBtn = $.trim(btnright);
                                    if (hasBtn[sBtn.toLowerCase()] === undefined) {
                                        hasBtn[sBtn.toLowerCase()] = sBtn;
                                    }
                                });
                            }
                        });
                        if (!hasBtn['upd']) {
                            delete hasBtn.save;
                            delete hasBtn.readd;
                        }

                        if (cando.action === 'upd') {
                            delete hasBtn.readd;
                        }

                        if (cando.action === 'add') {
                            delete hasBtn.del;
                        }
                        delete hasBtn.upd;
                        delete hasBtn.view;

                        for (var btnkey in hasBtn) {
                            var oBtn = {};
                            oBtn.key = hasBtn[btnkey];

                            if (btnkey === 'leave') {
                                lastBtn = oBtn;
                                lastBtn.hotkey = 'ctrl + l';
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
                                btns.push(oBtn);
                            }
                        }

                        if (cando.options.cusBtns.length > 0) {
                            btns.push.apply(btns, cando.options.cusBtns);
                        }
                        if (lastBtn) {
                            btns.push(lastBtn);
                        }
                        delayInitToolbar();


                        //button Ready時候
                        if (outterCando.ProgramId.indexOf('_Upd') > -1) {
                            var action = outterCando._getAction();
                            if (action === 'upd') {//判斷當前頁面是否有人在操作
                                parent.top.msgs.server.checkEdit(outterCando.ProgramId, outterCando.CheckId);
                            }
                            outterCando.form.find(':input,select').not('[data-type=select2]').change(function () {
                                if (!$(this).attr('data-trigger')) {
                                    w.bRequestStorage = true;
                                }
                            });
                            setTimeout(function () {
                                outterCando.form.find('[data-type=select2]').change(function () {
                                    if (!$(this).attr('data-trigger')) {
                                        w.bRequestStorage = true;
                                    }
                                });
                            }, 3000);
                        }
                        else {
                            if (outterCando.ProgramId.indexOf('_Qry') > -1) {
                                parent.top.msgs.server.removeEditPrg(outterCando.ProgramId.replace('_Qry', '_Upd'));//防止重複點擊首頁菜單導致之前編輯資料狀態無法移除
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
        },
        /**
         * 取得頂層模組
         * @return {String} 頂層模組
         */
        _getTopMod: function () {
            var cando = this,
                topmod = '',
                programList = g_db.GetDic('programList') || [],
                program = programList.filter(function (item) { return item.ModuleID === cando.ProgramId; })[0],
                getParent = function (modid) {
                    var oMod = cando._getParentMod(modid);
                    if (oMod.ParentID) {
                        getParent(oMod.ParentID);
                    }
                    else {
                        topmod = oMod.ModuleID;
                    }
                };
            getParent(program.ParentID);
            return topmod;
        },
        /**
         * 序列化form表單
         * @param  {HTMLElement} fm 表單form物件
         * @param  {String} type 傳回類型
         * @return {Object} fmdata 表單資料
         */
        _getFormSerialize: function (fm, type) {
            var cando = this,
                fmdata = {};
            fm = fm || cando.form;
            reFreshInput(fm);
            if (type) {
                fmdata = fm.serializeJSON();
            }
            else {
                fmdata = fm.serializeObject();
            }
            reSetInput(fm);
            return fmdata;
        },
        /**
         *取得程式所在模組的父層模組
         * @param  {String} modid 模組id
         * @return {Object} 模組對象
         */
        _getParentMod: function (modid) {
            var cando = this,
                programList = g_db.GetDic('programList') || [],
                parentProgram = programList.filter(function (item) { return item.ModuleID === modid && item.FilePath === '#'; })[0];
            return parentProgram;
        },
        /**
         * 儲存當前頁面的值
         * @param  {String} elId 要清除區塊的父層Id
         */
        _getPageVal: function (elId) {
            var cando = this,
                //判斷傳入的樣式是否不存在或者等於空的情況
                oHandle = elId !== undefined ? $('#' + elId) : cando.form;

            cando.currentPageValue = [];
            //儲存畫面值
            oHandle.find(':input', 'textarea', 'select').each(function () {
                var ctl = {};     //實例化對象

                ctl.ID = this.id;
                ctl.Type = this.type;
                if (ctl.ID && ctl.Type) {
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
                    cando.currentPageValue.push(ctl);
                }
            });
        },
        /**
         * 取得Url參數
         * @param  {String} name 取得部分的名稱 例如輸入"Action"，就能取到"Add"之類參數
         * @return {Object} url參數
         */
        _getUrlParam: function (name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"), //構造一個含有目標參數的正則表達式對象
                r = w.location.search.substr(1).match(reg);  //匹配目標參數
            if (r !== null) {
                return unescape(r[2]);//返回參數值
            } else {
                return null;
            }
        },
        /**
         * 創建程式Title
         */
        _createPageTitle: function () {
            var cando = this,
                handle = $('.page-title'),
                programList = g_db.GetItem('programList'),
                modTree = '',
                modTrees = [],
                setProgramPath = function (modid) {
                    var oParent = cando._getParentMod(modid);
                    if (oParent.ModuleID) {
                        modTrees.unshift('<div class="ng-scope layout-row"> <a class="md-button" href="#"><span class="ng-binding ng-scope" data-i18n=common.' + oParent.ModuleID + '></span></a> <i class="fa fa-angle-right" aria-hidden="true"></i> </div>');
                    }
                    if (oParent.ParentID) {
                        setProgramPath(oParent.ParentID);
                    }
                };
            if (programList === null || programList === '') { return; }
            var programLists = $.parseJSON(programList),
                curProgramList = programLists.filter(function (item) { return item.ModuleID === cando.ProgramId; });

            if (curProgramList.length === 0) { return; }

            var program = curProgramList[0];

            if (!program.ParentID) { return; }

            setProgramPath(program.ParentID);

            modTrees.push('<div class="ng-scope layout-row"> <a class="md-button" href="#"><span class="ng-binding ng-scope" data-i18n=common.' + program.ModuleID + '></span></a> </div>');

            modTree = modTrees.join('');     //串起父層路徑名稱

            $.templates({
                tmpl: '<div class="bread-crumbs layout-row ismobile" layout="row">\
                              {{: ModTree }}\
                       </div >\
                       <div class="title ismobile">\
                          <h2 data-i18n="{{:ProgramName}}"></h2>\
                          {{if showTable=="Y"}}<h5>(Use:{{:MainTableName}})</h5>{{/if}}\
                       </div>' });
            handle.html($.render.tmpl({ ProgramName: "common." + program.ModuleID, showTable: parent.top.SysSet.TbShowOrHide, MainTableName: program.MainTableName, ModTree: modTree }));
            cando._transLang(handle);

            if (navigator.userAgent.match(/mobile/i)) {
                $('.ismobile').hide();
            }
        },
        /**
         * 目的:恢復查詢條件
         */
        _reSetQueryPm: function () {
            var cando = this,
                qrParam = parent[cando.ProgramId + '_query'];
            if (qrParam) {
                cando._setFormVal(qrParam);
                if (qrParam.pageidx) {
                    cando.options.queryPageidx = qrParam.pageidx;
                }
            }
        },
        /**
         * 設定表單值
         * @param  {Object} json json對象
         */
        _setFormVal: function (json) {
            var cando = this;
            cando.form.find('[name]').each(function () {
                var id = this.id,
                    name = this.name ? this.name.replace('[]', '') : '',
                    type = this.type,
                    value = json[name] || cando._getJsonVal(json, id) || '';

                if (value) {
                    switch (type) {
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
                            var dataType = $(this).attr("data-type");
                            if (dataType && dataType === 'pop') {
                                var sVal_Name = json[name + 'Name'] || value;
                                $(this).attr("data-value", value);
                                if (sVal_Name) $(this).val(sVal_Name);
                            }
                            else {
                                if ($(this).hasClass('date-picker')) {
                                    value = newDate(value, 'date');
                                }
                                else if ($(this).hasClass('date-picker') || $(this).hasClass('datetime-picker') || $(this).hasClass('date')) {
                                    value = newDate(value);
                                }
                                if (value) $(this).val(value);
                                $(this).data('old', value);
                                if (dataType && dataType === 'int' && value) {
                                    $(this).attr('data-value', value.toString().replace(/[^\d.]/g, ''));
                                }
                            }
                            if (dataType === 'select2') {
                                $(this).trigger("change", 'setval');
                            }
                            break;
                        case 'checkbox':
                            if (typeof value === 'object') {
                                if (value.indexOf(this.value) > -1) {
                                    this.checked = value;
                                }
                            }
                            else {
                                this.checked = typeof value === 'string' ? value === this.value : value;
                            }
                            $.uniform && $.uniform.update($(this).prop("checked", this.checked));
                            break;
                        case 'radio':
                            this.checked = this.value === value;
                            $.uniform && $.uniform.update($(this).prop("checked", this.checked));
                            break;
                    }
                    if ((id === 'ModifyUserName' || id === 'CreateUserName') && 'select-one'.indexOf(this.type) === -1) {
                        $(this).text(value);
                    }
                    else if (id === 'ModifyDate' || id === 'CreateDate') {
                        $(this).text(newDate(value));
                    }
                }
            });
        },
        /**
         * 取得自定義json值
         * @param  {Object} json json對象
         * @param  {Object} name json鍵值
         * @return {String} last 最終要獲取的對應的鍵值對的值
         */
        _getJsonVal: function (json, name) {
            var last = json,
                names = name.split('_');
            for (var i = 0; i < names.length; i++) {
                if (!last[names[i]]) {
                    last = '';
                    break;
                }
                last = last[names[i]];
            }
            return last;
        },
        /**
         * 取得（url）參數（格式）字串
         * @param  {Object} keys key值json對象
         * @param  {Object} from 資料來源
         * @return {String} param 最終要獲取的參數（格式）字串
         */
        _getParamsStr: function (keys, from) {
            var param = '';
            for (var key in keys) {
                param += '&' + key + '=' + encodeURIComponent(from[key]);
            }
            return param;
        },
        /**
         * 清除畫面值
         * @param {String} flag 是否為查詢清空（查詢頁面須全部清空，不保留原值）
         */
        _clearPageVal: function () {
            var cando = this,
                pageval = cando.currentPageValue,
                elQryBtn = $('#Toolbar_Qry');
            for (var i = 0; i < pageval.length; i++) {
                var ctl = pageval[i],
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
                                curInput.html(ctl.Html);
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

            if (elQryBtn.length > 0) {
                $('#Toolbar_Qry').click();
            }
        },
        /**
         * 離開事件
         * @return {boolean} 停止標記
         */
        _pageLeave: function () {
            var cando = this,
                toLeave = function () {
                    parent.top.openPageTab(cando.QueryPrgId);
                    parent.top.msgs.server.removeEditPrg(cando.ProgramId);
                };
            //當被lock住，不儲存任何資料，直接離開。
            if (parent.bLockDataForm0430 !== undefined)
                toLeave();

            if (w.bRequestStorage) {
                layer.confirm(i18next.t('message.HasDataTosave'), {//╠message.HasDataTosave⇒尚有資料未儲存，是否要儲存？╣
                    icon: 3,
                    title: i18next.t('common.Tips'),// ╠message.Tips⇒提示╣
                    btn: [i18next.t('common.Yes'), i18next.t('common.No')] // ╠message.Yes⇒是╣ ╠common.No⇒否╣
                }, function (index) {
                    layer.close(index);
                    w.bLeavePage = true;
                    $('#Toolbar_Save').click();
                }, function () {
                    toLeave();
                });
                return false;
            }
            toLeave();
        },
        /**
         * 緩存查詢條件
         * @param  {HTMLElement}form 表單物件
         */
        _cacheQueryCondition: function (form) {
            var cando = this,
                _form = form || cando.form,
                prgid = cando.ProgramId || cando._getProgramId() || '',
                qrParam = {};
            cando.options.toFirstPage = false;

            if (prgid) {
                if (!parent[prgid + '_query']) {
                    parent[prgid + '_query'] = {};
                }
                if (typeof _form === 'number') {
                    parent[prgid + '_query'].pageidx = _form;
                }
                else {
                    var oQueryPm_Old = clone(parent[prgid + '_query']), key;
                    qrParam = cando._getFormSerialize();
                    for (key in qrParam) {
                        parent[prgid + '_query'][key] = qrParam[key];
                    }
                    for (key in oQueryPm_Old) {
                        if (key !== 'pageidx' && parent[prgid + '_query'][key] !== oQueryPm_Old[key]) {
                            cando.options.toFirstPage = true;
                            break;
                        }
                    }
                }
            }
        },
        /**
         *  獲取當前服務器地址
         * @return {String} 當前服務器地址
         */
        _getServerUrl: function () {
            return w.location.origin || this._getHost();
        },
        /**
         *  獲取當前服務器地址
         * @return {String} 當前服務器地址
         */
        _getHost: function () {
            var serverUrl = location.origin + '/';
            if (!w.location.origin) {
                serverUrl = w.location.protocol + "//" + w.location.hostname + (w.location.port ? ':' + w.location.port : '');
            }
            return serverUrl;
        },
        /**
         * Toolbar物件
         * @return {HTMLElement} New 一個Toolbar標籤
         */
        _getToolBar: function () {
            return $('#Toolbar');
        },
        /**
         * 獲取當前動作
         * @return {String} add or upd
         */
        _getAction: function () {
            var cando = this;
            return cando._getUrlParam('Action') === null ? 'add' : cando._getUrlParam('Action').toLowerCase();
        },
        /**
         * 下載文件
         * @param  {String} path 文件路徑（相對路勁）
         * @param  {String} filename 文件名稱
         */
        _downLoadFile: function (path, filename) {
            var cando = this,
                serverUrl = cando._getServerUrl(),
                url = serverUrl + "/Controller.ashx";
            url += '?action=downfile&path=' + path;
            if (filename) {
                url += '&filename=' + filename;
            }
            w.location.href = url;
            cando._closeWaiting();
        },
        /**
         * 開啟Waiting視窗
         * @param  {String} msg 提示文字
         */
        _showWaiting: function (msg) {
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
        },
        /**
         * 關閉Waiting視窗
         * @param  {Number} sleep 延遲時間，單位為毫秒
         */
        _closeWaiting: function (sleep) {
            $(function () {
                if (sleep === undefined) {
                    sleep = 100;
                }
                setTimeout($.unblockUI, sleep);
            });
        },
        /**
         * select2特殊化處理
         * @param  {Object} el select2控制項
         */
        _select2Init: function (el) {
            var select2 = el === undefined ? $('select[data-type=select2]') : el.find('select[data-type=select2]');
            //註冊客制化選單
            if (select2.length > 0) {
                select2.each(function () {
                    if ($(this).find('option').length > 0 && !$(this).attr('data-hasselect2')) {
                        $(this).select2().attr('data-hasselect2', true);
                        $(this).next().after($(this));
                    }
                });
            }
        },
        /**
         * 目的：異動模式點擊資料行彈出編輯按鈕
         * @param  {String} prgid 程式id
         * @param  {String} params 參數
         */
        _goToEdit: function (prgid, params) {
            var cando = this;
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
                    cando._transLang(layero);
                }
            });
        },
        /**
         * radio特殊化處理
         * @param  {Object} el 表單控制項
         */
        _uniformInit: function (el) {
            var radio = $("input[type=radio]:not(.no-uniform)");
            if (el) {
                radio = el.find("input[type=radio]:not(.no-uniform)");
            }
            if (radio.length > 0) {
                radio.each(function () {
                    $(this).uniform();
                });
            }
        },
        /**
         * 目的:input文本處理
         */
        _keyInput: function () {
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
                    if (e.keyCode !== 8 && e.keyCode !== 37 && e.keyCode !== 39 && e.keyCode !== 46) {
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
                                if ((sVal.charCodeAt(i) & 0xff00) !== 0) {
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
        },
        /**
         * 設定多於系
         * @param {String} lng 語種
         * @param {HTMLElement} el 要翻譯的html標籤
         * @param {Function} callback 回調函數
         */
        _setLang: function (lng, el, callback) {
            if (!lng) return;

            g_ul.SetLang(lng);

            i18next = "undefined" === typeof i18next ? parent.top.i18next : i18next;
            var cando = this,
                serverUrl = cando._getServerUrl();

            $.getJSON(serverUrl + "/Scripts/lang/" + (parent.top.OrgID || 'TE') + "/" + lng + ".json?v=" + new Date().getTime().toString(), function (json) {
                var resources = {};

                resources[lng] = {
                    translation: json
                };

                i18next.init({
                    lng: lng,
                    resources: resources,
                    useLocalStorage: false,               //是否将语言包存储在localstorage
                    //ns: { namespaces: ['trans'], defaultNs: 'trans' },             //加载的语言包
                    localStorageExpirationTime: 86400000        // 有效周期，单位ms。默认1
                }, function (err, t) {
                    cando._transLang(el);
                    if (typeof callback === 'function') {
                        callback(t);
                    }
                });
            });
        },
        /**
         * 翻譯語系
         * @param {HTMLElement} dom  翻譯回調函數
         */
        _transLang: function (dom) {
            i18next = "undefined" === typeof i18next ? parent.top.i18next : i18next;

            var i18nHandle = dom === undefined ? $('[data-i18n]') : dom.find('[data-i18n]'),
                i18nHandlePlaceholder = dom === undefined ? $('[placeholderid]') : dom.find('[placeholderid]');
            i18nHandle.each(function (idx, el) {
                var i18key = $(el).attr('data-i18n');
                if (i18key) {
                    var sLan = i18next.t(i18key);
                    if (el.nodeName === 'INPUT' && el.type === 'button') {
                        $(el).val(sLan);
                    }
                    else {
                        $(el).html(sLan);
                    }
                }
            });
            i18nHandlePlaceholder.each(function (idx, el) {
                var i18key = $(el).attr("placeholderid");
                if (i18key) {
                    var sLan = i18next.t(i18key);
                    if (sLan !== i18key) {
                        $(el).attr("placeholder", sLan);
                    }
                }
            });
        },
        /**
         * 獲取UE—Editor 值
         * @param {Object} data 當前表單資料
         * @return {Object} data 當前表單資料(含UE控件)
         */
        _getUEValues: function (data) {
            var cando = this;
            if (!$.isEmptyObject(cando.UE_Editor)) {
                for (var key in cando.UE_Editor) {
                    data[key] = cando.UE_Editor[key].getContent();
                }
            }

            return data;
        },
        /**
         * 設置UE—Editor 值
         * @param {Object} data 當前編輯資料
         */
        _setUEValues: function (data) {
            var cando = this,
                readyToSet = function (_key) {
                    cando.UE_Editor[_key].ready(function () {
                        cando.UE_Editor[_key].setContent(data[_key] || '');
                    });
                };
            if (!$.isEmptyObject(cando.UE_Editor)) {
                for (var key in cando.UE_Editor) {
                    readyToSet(key);
                }
            }
        }
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
        isreapet = isreapet || true;
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
                    if (isreapet !== false || originAry.indexOf($.trim(obj[id]) < 0 && isreapet === false)) {
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
                sHtml += "<label for='" + id + (_id === undefined ? '' : _id) + '_' + idx + "' style='" + (intCount === idx + 1 ? '' : 'float:left;') + "padding-left: 10px'>" + inputradio[0].outerHTML + $.trim(obj[name]) + "</label>";
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
            $('#main-wrapper').css('min-height', sHeight - 88 + 'px');
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
            pTar.style.display = "block";
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
            if (aryIframe === null) return;
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
     * 翻譯語系
     * @param {HTMLElement} dom 要翻譯的html標籤
     * @returns {Boolean} 停止標記
     */
    w.refreshLang = function (dom) {
        if (dom && dom.length === 0) {
            return false;
        }

        CanDo.fn._transLang(dom);
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
     * 時間格式化處理
     * @param  {Date} date 日期時間
     * @param  {Boolean} type 格式（日期 || 日期+時間）
     * @param  {Boolean} empty 是否預設空
     * @return {Boolean} 是否可傳回空
     */
    w.newDate = function (date, type, empty) {
        var r = '';
        if (date) {
            if (typeof date === 'string') {
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
     * 克隆对象
     * @param {Object} obj 被轉換對象
     * @return {Object} o 新對象
     */
    w.clone = function (obj) {
        var o, i, j, k;
        if (typeof obj !== "object" || obj === null) return obj;
        if (obj instanceof Array) {
            o = [];
            i = 0; j = obj.length;
            for (; i < j; i++) {
                if (typeof obj[i] === "object" && obj[i] !== null) {
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
                if (typeof obj[i] === "object" && obj[i] !== null) {
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
     * 用於表單序列化恢復禁用欄位
     * @param  {HTMLElement} dom 要禁用的物件標籤
     * @param  {HTMLElement} notdom 不要禁用的物件標籤
     * @param  {Boolean} bdisabled 狀態
     */
    w.disableInput = function (dom, notdom, bdisabled) {
        bdisabled = bdisabled === undefined || bdisabled === null ? true : bdisabled;
        dom = dom.find(':input,select-one,select,checkbox,radio,textarea');
        if (notdom) {
            dom = dom.not(notdom);
        }
        dom.each(function () {
            $(this).prop('disabled', bdisabled);
        });
    };
    /**
     * 獲取html模版
     * @param  {String} sUrl 模版路徑
     * @param  {Boolean} bAsync 是否同步
     * @return {Object} Ajax對象
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
        }
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

                g_api.ConnectLite(Service.com, 'EditFile', data,
                    function (res) {
                        if (res.RESULT) {
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
     * @param  {String} balert id類型
     * @return {Object} Ajax對象
     */
    w.DelFile = function (fileid, idtype, balert) {
        balert = balert === undefined || balert === null ? true : balert;
        if (fileid && $.trim(fileid)) {
            return g_api.ConnectLite(Service.com, 'DelFile',
                {
                    FileID: fileid,
                    IDType: idtype || ''
                },
                function (res) {
                    if (res.RESULT) {
                        if (balert) {
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
     * select options 排序
     * @param {String} set 要排序的select
     * @param {String} get 獲取的select
     * @param {HTMLElement} input 正序還是倒序
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
            if ($(this).val() + $(this).text() !== 'undefined') {
                searchtxt += $(this).val() + '|' + $(this).text() + ',';
            }
        });

        //給搜尋按鈕註冊事件
        input.off('keyup').on('keyup', function () {
            var sWord = this.value;
            set.empty();
            if (sWord !== "") {
                $.each(searchtxt.split(','), function (key, val) {
                    if (val.toLowerCase().indexOf(sWord.toLowerCase()) >= 0) {
                        var setValue = val.split('|')[0];
                        var setText = val.split('|')[1];
                        set.append('<option value="' + setValue + '">' + setText + '</option>');
                    }
                });
            }
            else {
                set.html(tempValue); //搜尋欄位為空返還初始值(全部人員)
            }

            //移除右邊存在的值
            if (get.html() !== '') {
                $.each(get.find('option'), function () {
                    set.find('option[value=' + $(this).val().replace(".", "\\.") + ']').remove();
                });
            }
        });
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
     * 目的：設置滾動條
     * @param {Number} h 高度
     * @param {Number} c 定位點
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
     * 下載文件
     * @param  {String} path 文件路徑（相對路勁）
     * @param  {String} filename 文件名稱
     */
    w.DownLoadFile = function (path, filename) {
        var sUrl = gServerUrl + "/Controller.ashx";
        sUrl += '?action=downfile&path=' + path;
        if (filename) {
            sUrl += '&filename=' + filename;
        }
        w.location.href = sUrl;
        CanDo.fn._closeWaiting();
    };
    /**
     * 取得host
     */
    w.gServerUrl = CanDo.fn._getServerUrl();
    /**
     * 定義系統所有公用 Service.fnction
     */
    w.ComFn = {
        W_Com: 'comw',
        GetList: 'QueryList',
        GetAdd: 'Add',
        GetUpd: 'Update',
        GetDel: 'Delete',
        GetUserList: 'GetUserList',
        GetArguments: 'GetArguments'
    };
    w.Service = CanDo.fn._service;
    w.transLang = CanDo.fn._transLang;

    function onStart(e) {
        CanDo.fn._showWaiting(typeof IsWaiting === 'string' ? IsWaiting : undefined);
    }
    function onStop(e) {
        CanDo.fn._closeWaiting();
        setTimeout(function () { IsWaiting = null; }, 3000);
    }

    $(d).ajaxStart(onStart).ajaxStop(onStop);

    CanDo.fn._init.prototype = CanDo.fn;

    return CanDo;
})(window, document);

/**
 * 日期添加屬性
 * @param  {String} type y:年;q:季度;m:月;w:星期;d:天;h:小時;n:分;s:秒;ms:毫秒;
 * @param  {Number} num 添加的數值；
 * @return {Date} r 新的時間
 */
Date.prototype.dateAdd = function (type, num) {
    var r = this,
        k = { y: 'FullYear', q: 'Month', m: 'Month', w: 'Date', d: 'Date', h: 'Hours', n: 'Minutes', s: 'Seconds', ms: 'MilliSeconds' },
        n = { q: 3, w: 7 };
    eval('r.set' + k[type] + '(r.get' + k[type] + '()+' + (n[type] || 1) * num + ')');
    return r;
};

/**
 * 計算兩個日期的天數
 * @param  {Object} date 第二個日期;
 * @return {Number} 時間差
 */
Date.prototype.diff = function (date) {
    return (this.getTime() - date.getTime()) / (24 * 60 * 60 * 1000);
};

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
 * @param  {String} fmt 格式字串;
 * @return {Date} fmt 新的時間
 */
Date.prototype.formate = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1, //月份
        "d+": this.getDate(), //日
        "h+": this.getHours() % 12 === 0 ? 12 : this.getHours() % 12, //小时
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
        fmt = fmt.replace(RegExp.$1, (RegExp.$1.length > 1 ? RegExp.$1.length > 2 ? "\u661f\u671f" : "\u5468" : "") + week[this.getDay() + ""]);
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return fmt;
};
/**
 * 註冊全替換
 * @param  {String} s1 字串1
 * @param  {String} s2 字串2
 * @return {String} 替換後的新字串
 */
String.prototype.replaceAll = function (s1, s2) {
    return this.replace(new RegExp(s1, "gm"), s2);
};
/**
 * 註冊金額添加三位一撇
 * @return {String}三位一撇字串
 */
String.prototype.toMoney = Number.prototype.toMoney = function () {
    return this.toString().replace(/\d+?(?=(?:\d{3})+$)/g, function (s) {
        return s + ',';
    });
};
/**
 * 数字四舍五入（保留n位小数）
 * @param  {Number} n 保留n位
 * @return {Number} number 數值
 */
String.prototype.toFloat = Number.prototype.toFloat = function (n) {
    n = n ? parseInt(n) : 0;
    var number = this;
    if (n <= 0) return Math.round(number);
    number = Math.round(number * Math.pow(10, n)) / Math.pow(10, n);
    return number;
};
/**
 * 百分数转小数
 * @return {Number} 百分数
 */
String.prototype.toPoint = function () {
    return this.replace("%", "") / 100;
};
/**
 * 小数转化为分数
 * @return {String} percent 百分数
 */
String.prototype.toPercent = Number.prototype.toPercent = function () {
    var percent = Number(this * 100).toFixed(1);
    percent += "%";
    return percent;
};
/**
 * 刪除陣列內包含(undefined, null, 0, false, NaN and '')的資料
 * @return {Array} 新陣列
 */
Array.prototype.clear = function () {
    var newArray = [];
    for (var i = 0; i < this.length; i++) {
        if (this[i]) {
            newArray.push(this[i]);
        }
    }
    return newArray;
};
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
 * @param  {String} val 元素
 */
Array.prototype.remove = function (val) {
    var index = this.indexOf(val);
    if (index > -1) {
        this.splice(index, 1);
    }
};
/**
 * 方法陣列內等待
 * @param {Array} array 執行方法陣列
 * @return {Object} Ajax 對象
 */
jQuery.whenArray = function (array) {
    return jQuery.when.apply(this, array);
};