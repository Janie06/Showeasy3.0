'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'ModifyDate',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm);
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

                $.when(fnSetArgDrop([
                    {
                        ArgClassID: 'Flow_Type',
                        Select: $('#Flow_Type'),
                        ShowId: true
                    }
                ]))
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
                                    goToEdit(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                            },
                            rowDoubleClick: function (args) {
                                parent.openPageTab(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                            },

                            fields: [
                                { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
                                {
                                    name: "Flow_TypeName", title: 'CheckFlow_Upd.Flow_Type', width: 100
                                },
                                { name: "Flow_Name", title: 'CheckFlow_Upd.Flow_Name', width: 250 },
                                {
                                    name: "Flows", title: 'CheckFlow_Upd.Flows', width: 400, itemTemplate: function (val, item) {
                                        var saFlows = $.parseJSON(item.Flows),
                                            saFlowsText = [],
                                            sFlowsText = '';
                                        $.each(saFlows, function (idx, flow) {
                                            var sFlowType = i18next.t('common.' + flow.SignedWay);
                                            if (flow.SignedWay !== 'flow1') {
                                                saFlowsText.push(sFlowType + '(' + Enumerable.From(flow.SignedMember).ToString("，", "$.name") + ')');
                                            }
                                            else {
                                                saFlowsText.push(Enumerable.From(flow.SignedMember).ToString("，", "$.name"));
                                            }
                                        });
                                        sFlowsText = saFlowsText.join(' → ');
                                        return $('<a>', { html: sFlowsText.length > 90 ? sFlowsText.substr(0, 90) + '...' : sFlowsText, title: sFlowsText });
                                    }
                                },
                                {
                                    name: "Handle_PersonName", title: 'common.Handle_Person', width: 100, align: 'center'
                                },
                                {
                                    name: "ModifyUserName", title: 'common.ModifyUser', width: 100, align: 'center'
                                },
                                {
                                    name: "ModifyDate", title: 'common.ModifyDate', width: 120, align: 'center', itemTemplate: function (val, item) {
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

require(['base', 'jsgrid', 'common_eip', 'util'], fnPageInit);