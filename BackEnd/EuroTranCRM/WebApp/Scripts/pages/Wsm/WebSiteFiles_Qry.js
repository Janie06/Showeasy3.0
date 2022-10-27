'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oBaseQueryPm = {
            pageIndex: 1,
            pageSize: parent.SysSet.GridRecords || 10,
            sortField: 'ModifyDate',
            sortOrder: 'desc'
        },
            /**
             * 獲取資料
             */
            fnGet = function (args) {
                var oQueryPm = {},
                    sTitle = '%' + $('#Title').val() + '%';//模版ID

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return CallAjax(ComFn.W_Com, ComFn.GetPage, {
                    Params: {
                        Entity: 'websitefiles',
                        OrderFields: oQueryPm.sortField,
                        OrderType: oQueryPm.sortOrder,
                        PageIndex: oQueryPm.pageIndex,
                        PageSize: oQueryPm.pageSize,
                        Title: sTitle,
                        OrgID: parent.OrgID
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
                }).done(function () {
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
                                goToEdit(sEditPrgId, '?Action=Upd&FileID=' + args.item.FileID);
                            }
                        },
                        rowDoubleClick: function (args) {
                            parent.openPageTab(sEditPrgId, '?Action=Upd&FileID=' + args.item.FileID);
                        },

                        fields: [
                            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
                            { name: "UniqueID", title: 'common.UniqueID', width: 150 },
                            { name: "Title", title: 'common.FilesTitle', width: 200 },
                            { name: "Description", title: 'common.FilesDescription', width: 300 },
                            { name: "Memo", title: 'common.Memo', width: 300 },
                            { name: "ModifyUser", title: 'common.ModifyUser', width: 100 },
                            {
                                name: "ModifyDate", title: 'common.ModifyDate', width: 150, align: 'center', itemTemplate: function (val, item) {
                                    return newDate(val);
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