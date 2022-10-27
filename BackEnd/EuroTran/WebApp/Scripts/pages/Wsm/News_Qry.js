'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'News_LanguageType,OrderByValue',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param {Object}  args 查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return  g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (args.Excel) {//匯出
                            DownLoadFile(oRes);
                        }
                    }
                });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":
                        
                        var iNum = $('#PerPageNum').val();
                        oGrid.pageSize = iNum === '' ? parent.SysSet.GridRecords || 10 : iNum;
                        cacheQueryCondition();
                        oGrid.openPage(window.bToFirstPage ? 1 : oBaseQueryPm.pageIndex);

                        break;
                    case "Toolbar_Save":

                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        break;

                    case "Toolbar_Add":

                        parent.openPageTab(sEditPrgId, '?Action=Add');
                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /**
             * 頁面初始化
             */
            init = function () {
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });

                fnSetArgDrop([
                    {
                        ArgClassID: 'LanCountry',
                        Select: $('#News_LanguageType'),
                        ShowId: true
                    },
                    {
                        ArgClassID: 'News_Class',
                        Select: $('#News_Type'),
                        ShowId: true
                    }
                ])
                    .done(function () {
                        reSetQueryPm(sProgramId);
                        var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
                        $("#jsGrid").jsGrid({
                            width: "100%",
                            height: iHeight + "px",
                            autoload: true,
                            pageLoading: true,
                            inserting: false,
                            editing: true,
                            sorting: true,
                            paging: true,
                            pageIndex: window.bToFirstPage ? 1 : window.QueryPageidx || 1,
                            pageSize: parent.SysSet.GridRecords || 10,
                            pageButtonCount: parent.SysSet.GridPages || 15,
                            pagePrevText: "<",
                            pageNextText: ">",
                            pageFirstText: "<<",
                            pageLastText: ">>",
                            onPageChanged: function (args) {
                                cacheQueryCondition(args.pageIndex);
                            },
                            rowClick: function (args) {
                                if (navigator.userAgent.match(/mobile/i)) {
                                    goToEdit(sEditPrgId, '?Action=Upd&SN=' + args.item.SN);
                                }
                            },
                            rowDoubleClick: function (args) {
                                parent.openPageTab(sEditPrgId, '?Action=Upd&SN=' + args.item.SN);
                            },
                            fields: [
                                { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
                                {
                                    name: "News_Type", title: 'News_Upd.News_Type', width: 100, itemTemplate: function (val, item) {
                                        return item.News_TypeName;
                                    }
                                },
                                {
                                    name: "News_LanguageType", title: 'News_Upd.News_LanguageType', width: 100, itemTemplate: function (val, item) {
                                        return item.News_LanguageTypeName;
                                    }
                                },
                                { name: "News_Title", title: 'News_Upd.News_Title', width: 300 },
                                {// ╠common.ReleaseDate⇒發佈日期╣
                                    name: "CreateDate", title: 'common.ReleaseDate', width: 100, align: 'center', itemTemplate: function (val, item) {
                                        return newDate(val, 'date');
                                    }
                                },
                                {
                                    name: "News_StartDete", title: 'News_Upd.News_StartDete', width: 100, align: 'center', itemTemplate: function (val, item) {
                                        return newDate(val, 'date');
                                    }
                                },
                                {
                                    name: "News_EndDete", title: 'News_Upd.News_EndDete', width: 100, align: 'center', itemTemplate: function (val, item) {
                                        return newDate(val, 'date', true);
                                    }
                                },
                                {
                                    name: "News_Show", title: 'common.Status', width: 100, align: 'center', itemTemplate: function (val, item) {
                                        return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');// ╠common.Effective⇒有效╣ ╠common.Invalid⇒無效╣
                                    }
                                },
                                {
                                    name: "OrderByValue", title: 'common.OrderByValue', type: "select", width: 100,
                                    itemTemplate: function (val, item) {
                                        return this._createSelect = $("<select>", {
                                            class: 'w70',
                                            html: createOptions(item.OrderCount),
                                            change: function () {
                                                var sOldValue = val,
                                                    sNewValue = this.value;
                                                g_api.ConnectLite(sProgramId, ComFn.GetUpdateOrder, {
                                                    Id: item.SN,
                                                    OldOrderByValue: sOldValue,
                                                    NewOrderByValue: sNewValue
                                                }, function (res) {
                                                    if (res.RESULT) {
                                                        showMsg(i18next.t('message.Update_Success'), 'success');// ╠message.Update_Success⇒更新成功╣
                                                        oGrid.openPage(window.bToFirstPage ? 1 : oBaseQueryPm.pageIndex);
                                                    }
                                                    else {
                                                        showMsg(i18next.t('message.Update_Failed') + '<br>' + res.MSG, 'error'); // ╠message.Update_Failed⇒更新失敗╣
                                                    }
                                                });
                                            }
                                        }).val(val);
                                    }
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return fnGet(args);
                                },
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                    });
            };

        init();
    };

require(['base', 'jsgrid', 'util'], fnPageInit);