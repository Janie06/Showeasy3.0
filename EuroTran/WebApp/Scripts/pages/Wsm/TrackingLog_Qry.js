'use strict';
var sProgramId = getProgramId(),
    oGrid = null,
    fnPageInit = function () {
        var canDo = new CanDo({
            sortField: 'QueryTime',
            sortOrder: 'desc',
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['NO'],
            /**
             * Grid欄位設置（可以是 function）
             */
            gridFields: function () {
                var saFeilds = [
                    { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
                    {
                        name: "QueryNumber", title: 'common.QueryNumber', type: "text", align: "center", width: 100
                    },
                    {
                        name: "Exhibitioname_TW", title: 'Exhibition_Upd.Exhibitioname_TW', type: "text", width: 200
                    },
                    {
                        name: "Exhibitioname_EN", title: 'ExhibitionImport_Upd.ImportBillEName', type: "text", width: 200
                    },
                    {
                        name: "AgentName", title: 'ExhibitionImport_Upd.Agent', type: "text", width: 200
                    },
                    {
                        name: "CustomerName", title: 'ExhibitionImport_Upd.Supplier', type: "text", width: 200
                    },
                    {
                        name: "QueryIp", title: 'IP', type: "text", width: 120, align: "center"
                    },
                    {// ╠common.IPAddress⇒IP地址信息╣
                        name: "IPInfo", title: 'common.IPAddress', type: "text", width: 150,
                        itemTemplate: function (val, item) {
                            if (val) {
                                var oIPInfo = $.parseJSON(val);
                                return traditionalized((oIPInfo.country || '') + ' ' + (oIPInfo.area || '') + ' ' + (oIPInfo.region || '') + ' ' + (oIPInfo.city || ''));
                            }
                            else {
                                return '';
                            }
                        }
                    },
                    {
                        name: "QueryTime", title: 'common.QueryTime', type: "text", align: "center", width: 100,
                        itemTemplate: function (val, item) {
                            return newDate(val);
                        }
                    }
                ];
                if (parent.UserInfo.roles.indexOf(parent.SysSet.Supervisor) > -1) {
                    saFeilds.push({ type: "control", title: 'common.Action', editButton: false });
                }
                return saFeilds;
            },
            /**
             * 查詢后事件
             * @param  {Object} pargs CanDo 對象
             */
            rowClick: function (pargs, args) {
                if (navigator.userAgent.match(/mobile/i)) {
                    var sEditPrgId = 'ExhibitionImport_Upd',
                        sActionId = 'ImportBillNO';
                    if (args.item.QueryNumber.indexOf('CTEE') > -1) {
                        sEditPrgId = 'ExhibitionExport_Upd';
                        sActionId = 'ExportBillNO';
                    }
                    pargs._goToEdit(sEditPrgId, '?Action=Upd&' + sActionId + '=' + args.item.ParentId);
                }
            },
            /**
             * 查詢后事件
             * @param  {Object} pargs CanDo 對象
             */
            rowDoubleClick: function (pargs, args) {
                var sEditPrgId = 'ExhibitionImport_Upd',
                    sActionId = 'ImportBillNO';
                if (args.item.QueryNumber.indexOf('CTEE') > -1) {
                    sEditPrgId = 'ExhibitionExport_Upd';
                    sActionId = 'ExportBillNO';
                }
                parent.openPageTab(sEditPrgId, '?Action=Upd&' + sActionId + '=' + args.item.ParentId);
            },
            /**
             * 查詢后事件
             * @param  {Object} pargs CanDo 對象
             */
            afterQuery: function (pargs) {
                if ($('#QueryNumber').val()) {
                    $('.countinfo').show();
                    fnGetGroupInfo();
                }
                else {
                    $('.countinfo').hide();
                    $('.list-unstyled,.counter').html('');
                }
            },
            /**
             * 頁面初始化
             * @param  {Object} pargs CanDo 對象
             */
            pageInit: function (pargs) {
                fnSetDeptDrop($('#DepartmentID'), parent.SysSet.SearchDeptList).done(function () {
                    pargs._reSetQueryPm();
                    pargs._initGrid();
                });
            }
        }),
            /**
             * 獲取分佈資料
             * @param {Object}  args 查詢條件參數
             */
            fnGetGroupInfo = function () {
                var oQueryPm = canDo._getFormSerialize();

                g_api.ConnectLite(canDo.ProgramId, 'GetGroupInfo', oQueryPm, function (res) {
                    if (res.RESULT) {
                        var sGroupInfo = '',
                            iTotal = 0;
                        if (res.DATA.rel.length > 0) {
                            $.each(res.DATA.rel, function (idx, info) {
                                var oIPInfo = $.parseJSON(info.IPInfo || '{}'),
                                    sCounry = traditionalized((oIPInfo.country || '') + ' ' + (oIPInfo.area || '') + ' ' + (oIPInfo.region || '') + ' ' + (oIPInfo.city || ''));
                                sGroupInfo += '<li>\
                                              <div class="server-load">\
                                                  <div class="server-stat">\
                                                      <p>' + info.QueryIp + '</p>\
                                                  </div>\
                                                  <div class="server-stat">\
                                                      <p>' + sCounry + '</p>\
                                                  </div>\
                                                  <div class="server-stat">\
                                                      <p>' + info.Count + '</p>\
                                                  </div>\
                                              </div>\
                                          </li>';
                                iTotal += info.Count;
                            });
                        }
                        $('.list-unstyled').html(sGroupInfo);
                        $('.counter').html(iTotal.toString().toMoney());
                    }
                });
            };
    };

require(['base', 'jsgrid', 'convetlng', 'cando'], fnPageInit);