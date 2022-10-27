'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CreateDate,CustomerCName',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param   args{Object}  查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                oQueryPm.SearchWords = '%' + oQueryPm.SearchWords + '%';

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return CallAjax(ComFn.W_Com, ComFn.GetPage, {
                    Params: {
                        Entity: 'ovw_importcustomers',
                        OrderFields: oQueryPm.sortField || 'CreateDate',
                        OrderType: oQueryPm.sortOrder || 'desc',
                        PageIndex: oQueryPm.pageIndex,
                        PageSize: oQueryPm.pageSize,
                        ExhibitionNO: oQueryPm.ExhibitionNO,
                        _OR_: [
                            { CustomerCName: oQueryPm.SearchWords },
                            { Contactor: oQueryPm.SearchWords },
                            { Telephone: oQueryPm.SearchWords },
                            { Email: oQueryPm.SearchWords },
                            { Address: oQueryPm.SearchWords }
                        ],
                        OrgID: parent.OrgID
                    }
                });
            },
            /**
             * 資料刪除
             * @param  {Object} data  表單資料
             * @return {Object} Ajax 物件
             */
            fnDel = function (data) {
                return CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        importcustomers: {
                            guid: data.guid
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
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

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        break;
                    case "Toolbar_Exp":

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
                $.whenArray([
                    commonInit({
                        PrgId: sProgramId,
                        ButtonHandler: fnButtonHandler,
                        SearchBar: true
                    }),
                    fnSetEpoDrop({
                        Select: $('#ExhibitionNO'),
                        Select2: true
                    })
                ])
                    .done(function () {
                        var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;
                        $("#jsGrid").jsGrid({
                            width: "100%",
                            height: iHeight + "px",
                            autoload: true,
                            pageLoading: true,
                            inserting: false,
                            editing: false,
                            sorting: true,
                            paging: true,
                            pageIndex: window.bToFirstPage ? 1 : window.QueryPageidx || 1,
                            pageSize: parent.SysSet.GridRecords || 10,
                            pageButtonCount: parent.SysSet.GridPages || 15,
                            confirmDeleting: true,
                            deleteConfirm: "確定要刪除嗎？",
                            pagePrevText: "<",
                            pageNextText: ">",
                            pageFirstText: "<<",
                            pageLastText: ">>",
                            onPageChanged: function (args) {
                                cacheQueryCondition(args.pageIndex);
                            },
                            rowClick: function (args) {
                            },
                            rowDoubleClick: function (args) {
                            },
                            fields: [
                                {
                                    name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                                },
                                {
                                    name: "Exhibitioname_TW", title: 'ExhibitionExport_Upd.ExportBillName', width: 200
                                },
                                {
                                    name: "CustomerCName", type: 'text', title: 'Customers_Upd.CustomerCName', width: 200
                                },
                                {
                                    name: "Contactor", type: 'text', title: 'common.Contactor', width: 120
                                },
                                {
                                    name: "Telephone", type: 'text', title: 'common.Telephone', width: 100
                                },
                                {
                                    name: "Email", type: 'text', title: 'common.Email', width: 120
                                },
                                {
                                    name: "Address", type: 'text', title: 'common.Address', width: 200
                                },
                                {// ╠common.IsAppoint⇒預約狀態╣
                                    name: "IsAppoint", title: 'common.IsAppoint', width: 100, align: "center",
                                    itemTemplate: function (val, item) {
                                        return val == 'Y' ? $('<span />', { text: i18next.t('common.HasAppoint') }).css('color', 'green') : $('<span />', { text: i18next.t('common.NotAppoint') }).css('color', 'red');// ╠common.HasAppoint⇒已預約╣ ╠common.NotAppoint⇒未預約╣
                                    }
                                },
                                {// ╠common.IsFormal⇒資料狀態╣
                                    name: "IsFormal", title: 'common.IsFormal', width: 100, align: "center",
                                    itemTemplate: function (val, item) {
                                        return val ? $('<span />', { text: i18next.t('common.HasFormal') }).css('color', 'green') : $('<span />', { text: i18next.t('common.NotFormal') }).css('color', 'red');
                                    }
                                }
                            ],
                            controller: {
                                loadData: function (args) {
                                    return fnGet(args);
                                },
                                insertItem: function (args) {
                                },
                                updateItem: function (args) {
                                    fnUpd(args);
                                },
                                deleteItem: function (args) {
                                    return fnDel(args);
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                    });
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);