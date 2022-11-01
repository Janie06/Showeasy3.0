'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CreateDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param   args{Object}  查詢條件參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = {},
                    oQuery = getFormSerialize(oForm),
                    sSearchWords = '%' + oQuery.SearchWords + '%',
                    sQueryTimeStart = oQuery.QueryTimeStart,
                    sQueryTimeEnd = oQuery.QueryTimeEnd;

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return CallAjax(ComFn.W_Com, ComFn.GetPage, {
                    Params: {
                        Entity: 'websitemaillog',
                        OrderFields: oQueryPm.sortField || 'CreateDate',
                        OrderType: oQueryPm.sortOrder || 'desc',
                        PageIndex: oQueryPm.pageIndex,
                        PageSize: oQueryPm.pageSize,
                        _OR_: [{ Ucomp: sSearchWords },
                        { Uname: sSearchWords },
                        { Uemail: sSearchWords },
                        { Utel: sSearchWords },
                        { Title: sSearchWords },
                        { Content: sSearchWords }],
                        _AND_: [{ key: 'CreateDate', name: 'QueryTimeStart', value: sQueryTimeStart === '' ? '' : '>>=' + sQueryTimeStart }
                            , { key: 'CreateDate', name: 'QueryTimeEnd', value: sQueryTimeEnd === '' ? '' : '<<=' + sQueryTimeEnd + ' 23:59:59' }],
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
                        editing: false,
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
                        },
                        rowDoubleClick: function (args) {
                        },
                        fields: [
                            { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                            {
                                name: "Ucomp", title: 'common.CompanyName', type: "text", width: 200// ╠common.CompanyName⇒公司名稱╣
                            },
                            {
                                name: "Uname", title: 'common.Contactor', type: "text", width: 100
                            },
                            {
                                name: "Uemail", title: 'common.Email', type: "text", width: 150
                            },
                            {
                                name: "Utel", title: 'common.Telephone', type: "text", width: 150
                            },
                            {
                                name: "Title", title: 'common.Purpose', type: "text", width: 180// ╠common.Purpose⇒主旨╣
                            },
                            {// ╠common.Emailbodyhtml⇒郵件主體內容╣
                                name: "Content", title: 'common.Emailbodyhtml', type: "text", width: 200,
                                itemTemplate: function (val, item) {
                                    return val.length > 66 ? val.substr(0, 66) + '...' : val;
                                }
                            },
                            {
                                name: "CreateDate", title: 'common.CreateDate', type: "text", align: "center", width: 100,
                                itemTemplate: function (val, item) {
                                    return newDate(val);
                                }
                            }
                        ],
                        controller: {
                            loadData: function (args) {
                                return fnGet(args);
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

require(['base', 'jsgrid', 'util'], fnPageInit);