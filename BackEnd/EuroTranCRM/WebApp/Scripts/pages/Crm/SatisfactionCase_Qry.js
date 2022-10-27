'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var saContactorList = [],
			oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'SN',
                sortOrder: 'desc'
            },
            /**
             * 獲取資料
             * @param  {Object} args 查詢參數
             * @return {Object} Ajax 物件
             */
            fnGet = function (args) {
                var oQueryPm = getFormSerialize(oForm);

                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;
				
                return g_api.ConnectLite(sProgramId, ComFn.GetPage, oQueryPm, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (args.Excel) {//匯出
                            DownLoadFile(oRes);
                            layer.close(args.Index);
                        }
                    }
                });
            },
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.confirm("確定要匯出嗎？", { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                    fnGet({
                        Excel: true,
                        Index: index
                    });
                })
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

                        fnSave('add');

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
                         if (oGrid.data.length === 0) {
                            showMsg(i18next.t("message.NoDataExport"));// ╠message.NoDataExport⇒沒有資料匯出╣
                            return false;
                        }
                        fnOpenPopToExcel(); 
                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
			
            /**
             * 初始化 function
             */
            init = function () {
                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });
                $.whenArray([
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            Select2: true,
                            ShowId: true
                        }
                    ])
				])
                .done(function () {
                    reSetQueryPm(sProgramId);
                    var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 87;

                    $("#jsGrid").jsGrid({
                        width: "100%",
                        height: iHeight + "px",
                        autoload: true,
                        pageLoading: true,
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
							{
								name: "RowIndex", title: 'common.RowNumber', type: "text", width: 30, align: "center", sorting: false
							},
							{
								name: "CaseName", title: '滿意度案件名稱', type: "text", width: 60
							},
							{
								name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 180
							},
							{
								name: "ExhibitionDateStart", title: 'Exhibition_Upd.ExhibitionDateRange', type: "text", align: "center", width: 150, itemTemplate: function (val, item) {
									var sDateRange = newDate(item.ExhibitionDateStart, 'date', true) + '~' + newDate(item.ExhibitionDateEnd, 'date', true);
									return sDateRange === '~' ? '' : sDateRange;
								}
							},
							{
								name: "CreateUser", title: 'common.CreateUser', type: "text", width: 70
							},
							{
								name: "CreateDate", title: 'common.CreateDate', type: "text", align: "center", width: 100, itemTemplate: function (val, item) {
									if(val != null){	
										return newDate(val);
									}
								}
							},
							{
								name: "ModifyDate", title: 'common.ModifyDate', type: "text", align: "center", width: 100, itemTemplate: function (val, item) {
									if(val != null){	
										return newDate(val);
									}
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

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);