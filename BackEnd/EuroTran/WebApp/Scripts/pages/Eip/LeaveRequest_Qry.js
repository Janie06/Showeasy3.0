'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'MemberID',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param  {Object} args 查詢參數
             */
             fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;
                oQueryPm.Roles = parent.UserInfo.roles;
                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
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
                    case "Toolbar_Exp":

                        break;
                    case "Toolbar_Imp":
                        //fnImport();
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

                var saDate = [],
                    iThisYear = new Date().getFullYear(),
                    iBaseYearCount = 20;
                while (iBaseYearCount >= 0) {
                    var iCurYear = iThisYear - iBaseYearCount;
                    saDate.push({ id: iCurYear, text: iCurYear });
                    iBaseYearCount--;
                }
                iBaseYearCount += 2;
                while (iBaseYearCount <= 20) {
                    var iCurYear = iThisYear + iBaseYearCount;
                    saDate.push({ id: iCurYear, text: iCurYear });
                    iBaseYearCount++;
                }
                $('#Date').html(createOptions(saDate, 'id', 'text'));

                fnSetUserDrop([{
                    Select: $('#UserID'),
                    ShowId: true,
                    CallBack: function (data) {
                        var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87,
                            saFields = [
                                { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                                {
                                    name: "MemberID", title: 'common.Account', align: "left", type: "text", width: 70
                                },
                                {
                                    name: "WenZhongAcount", title: 'MembersMaintain_Upd.WenZhongAcount', align: "left", type: "text", width: 70
                                },
                                {
                                    name: "MemberName", title: 'common.EmployeeName', align: "left", type: "text", width: 80
                                },
                                {// ╠common.Seniority⇒請假別╣
                                    name: "HolidayCategoryName", title: 'common.HolidayCategory', align: "center", type: "text", width: 50
                                },
                                {// ╠common.EnableDate⇒啟動日期╣
                                    name: "EnableDate", title: 'common.EnableDate', type: "text", align: "center", width: 100,
                                    itemTemplate: function (val, item) {
                                        return newDate(val, true);
                                    }
                                },
                                {// ╠common.ExpirationDate⇒失效日期╣
                                    name: "ExpirationDate", title: 'common.ExpirationDate', type: "text", align: "center", width: 100,
                                    itemTemplate: function (val, item) {
                                        return newDate(val, true);
                                    }
                                },
                                {// ╠common.PaymentHours⇒給付時數╣
                                    name: "PaymentHours", title: 'common.PaymentHours', align: "center", type: "text", width: 50
                                },
                                {// ╠common.UsedHours⇒已用時數╣
                                    name: "UsedHours", title: 'common.UsedHours', align: "center", type: "text", width: 50
                                },
                                {// ╠common.RemainHours⇒剩餘時數╣
                                    name: "RemainHours", title: 'common.RemainHours', align: "center", type: "text", width: 50
                                },
                                {
                                    name: "Memo", title: 'common.Memo', type: "text", width: 150
                                }
                            ];
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
                            fields: saFields,
                            rowDoubleClick: function (args) {
                                parent.openPageTab(sEditPrgId, '?Action=Upd&Guid=' + args.item.guid);
                            },
                            controller: {
                                loadData: function (args) {
                                    return fnGet(args);
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                    }
                }]);

                fnSetArgDrop([
                    {
                        ArgClassID: 'LeaveType',
                        Select: $('#HolidayCategory'),
                        ShowId: true
                    }
                ]);
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'ajaxfile', 'util'], fnPageInit);