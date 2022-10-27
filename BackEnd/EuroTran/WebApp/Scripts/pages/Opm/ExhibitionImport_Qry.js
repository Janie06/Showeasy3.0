'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: '',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param  {Object} args 查詢參數
             * @return {Object} Ajax 物件
             */
            fnGetPro = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, args.Excel ? 'GetExcel' : 'QueryPage', oQueryPm, function (res) {
                    if (args.Excel) {//匯出
                        var oRes = res.DATA.rel;
                        DownLoadFile(oRes);
                        layer.close(args.Index);
                    }
                });
            },
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            fnOpenPopToExcel = function () {
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['200px', '420px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                             <p><button type="button" data-i18n="common.BasicInformation" id="Import_BasicInformation" class="btn-custom w100p green">基本資料</button></p>\
                             <p><button type="button" data-i18n="common.BusinessTrackingSchedule" id="Import_BusinessTrackingSchedule" class="btn-custom w100p green">業務追蹤進度表</button></p>\
                             <p><button type="button" data-i18n="common.AdvanceAndRetreatWorkSheet" id="Import_AdvanceAndRetreatWorkSheet" class="btn-custom w100p green">進退場工作表</button></p>\
                             <p><button type="button" data-i18n="common.ReturnRecord" id="Import_ReturnRecord" class="btn-custom w100p green">退運記錄表</button></p>\
                             <p><button type="button" data-i18n="common.AreasList" id="Import_AreasList" class="btn-custom w100p green">退押款清冊</button></p>\
                             <p><button type="button" data-i18n="common.BondedSheet" id="Import_BondedSheet" class="btn-custom w100p green">保稅工作表</button></p>\
                         </div>',//╠common.BasicInformation⇒基本資料╣╠common.BusinessTrackingSchedule⇒業務追蹤進度表╣╠common.AdvanceAndRetreatWorkSheet⇒進退場工作表╣╠common.ReturnRecord⇒退運記錄表╣╠common.AreasList⇒退押款清冊╣╠common.BondedSheet⇒保稅工作表╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            fnGetPro({
                                Excel: true,
                                ExcelType: sToExcelType,
                                Index: idx
                            });
                        });
                        transLang(layero);
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
             * 設置Grid行樣式
             * @param {String} val 欄位值
             * @param {Object} item 欄位值
             * @return {String} 新字串
             */
            setGridRow = function (val, item) {
                var sVal = val || '';
                if (item.IsAlert === 'Y') {
                    sVal = '<span class="t-red t-bold">' + sVal + '</span>';
                }
                return sVal
            },
            /**
             * 初始化 function
             */
            init = function () {
                if (parent.OrgID !== 'TE') {
                    $('.dept-box').hide();
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    SearchBar: true
                });
                //$.timepicker.dateRange($('#ExhibitionDateStart'), $('#ExhibitionDateEnd'),
                //    {
                //        minInterval: (1000 * 60 * 60 * 24 * 1), // 1 days
                //        changeYear: true,
                //        changeMonth: true
                //    }
                //);

                $.whenArray([
                    fnSetDeptDrop($('#DepartmentID'), parent.SysSet.SearchDeptList),
                    fnSetUserDrop([
                        {
                            Select: $('#ResponsiblePerson'),
                            Select2: true,
                            ShowId: true,
                            ServiceCode: parent.SysSet.IMCode
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'DeclClass',
                            Select: $('#DeclarationClass'),
                            ShowId: true
                        }
                    ])])
                    .done(function () {
                        reSetQueryPm(sProgramId);
                        var sCode = parent.UserInfo.ServiceCode;
                        if (sCode && parent.SysSet.IMCode.indexOf(sCode) > -1 && parent.UserInfo.roles.indexOf('CDD') === -1) {
                            $('#ResponsiblePerson').val(parent.UserInfo.MemberID);
                        }

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
                            headerClick: function () {
                            },
                            onPageChanged: function (args) {
                                cacheQueryCondition(args.pageIndex);
                            },
                            rowClick: function (args) {
                                if ((args.item.IsVoid === 'Y' && parent.UserInfo.IsManager) || args.item.IsVoid === 'N') {
                                    if (navigator.userAgent.match(/mobile/i)) {
                                        goToEdit(sEditPrgId, '?Action=Upd&ImportBillNO=' + args.item.ImportBillNO);
                                    }
                                }
                                else {
                                    showMsg(i18next.t("message.NoPermission")); // ╠message.NoPermission⇒沒有權限╣
                                }
                            },
                            rowDoubleClick: function (args) {
                                if ((args.item.IsVoid === 'Y' && parent.UserInfo.IsManager) || args.item.IsVoid === 'N') {
                                    parent.openPageTab(sEditPrgId, '?Action=Upd&ImportBillNO=' + args.item.ImportBillNO);
                                }
                                else {
                                    showMsg(i18next.t("message.NoPermission")); // ╠message.NoPermission⇒沒有權限╣
                                }
                            },
                            rowMouseDown: function (args) {
                                //oTimeOut = setTimeout(function () {
                                //}, 1000);
                            },
                            rowMouseUp: function (args) {
                                //clearTimeout(oTimeOut);
                            },
                            rowMouseUut: function (args) {
                                //clearTimeout(oTimeOut);
                            },
                            rowClass: function (item) {
                                var sClass = '';
                                if (item.IsVoid === 'Y') {
                                    sClass = 'data-void';
                                }
                                return sClass;
                            },
                            fields: [
                                {
                                    name: "RowIndex", title: 'common.RowNumber', type: "text", width: 40, align: "center", sorting: false, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "RefNumber", title: 'ExhibitionImport_Upd.RefNumber', type: "text", width: 90, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "ImportBillName", title: 'ExhibitionImport_Upd.ImportBillName', type: "text", width: 140, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "SupplierCName", title: 'ExhibitionImport_Upd.Supplier', type: "text", width: 120, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "AgentName", title: 'ExhibitionImport_Upd.Agent', type: "text", width: 120, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "ResponsiblePersonName", title: 'ExhibitionImport_Upd.ResponsiblePerson', type: "text", width: 60, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "BillLadNO", title: 'ExhibitionImport_Upd.BillLadNO', type: "text", width: 120, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {
                                    name: "REF", title: 'ExhibitionImport_Upd.REF', type: "text", width: 60, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(val, item);
                                        return sVal;
                                    }
                                },
                                {// ╠common.Flow_Status⇒貨物狀態╣
                                    name: "Flow_Status", title: 'common.Flow_Status', type: "text", width: 70, itemTemplate: function (val, item) {
                                        var sValShow = '',
                                            saVal = (val || '').split('-');
                                        if (saVal.length > 1) {
                                            var oImport = { '1': '已收文件', '2': '貨物抵達', '3': '報關作業', '4': '貨物放行', '5': '轉至展館倉庫', '6': '已送達' },
                                                oReImport = { '1': '文件確認', '2': '報關作業', '3': '貨物放行', '4': '回運中', '5': '抵達目的地', '6': '已送達' };
                                            if (saVal[0] === 'Import') {
                                                sValShow = i18next.t("common.ExhibitionImport_Qry") + '-' + oImport[saVal[1]];// ╠common.ExhibitionImport_Qry⇒進口╣
                                            }
                                            else {
                                                var saReImports = $.parseJSON(item.ReImports);
                                                if (saReImports.length > 0) {
                                                    $.each(saReImports, function (idx, reimport) {
                                                        var iLen = idx + 1,
                                                            sFlowStatus = '';
                                                        if (reimport.ReImport.Sign && reimport.ReImport.Sign.Checked) {
                                                            sFlowStatus = '6';
                                                        }
                                                        else if (reimport.ReImport.ReachDestination && reimport.ReImport.ReachDestination.Checked) {
                                                            sFlowStatus = '5';
                                                        }
                                                        else if (reimport.ReImport.HuiYun && reimport.ReImport.HuiYun.Checked) {
                                                            sFlowStatus = '4';
                                                        }
                                                        else if (reimport.ReImport.ReCargoRelease && reimport.ReImport.ReCargoRelease.Checked) {
                                                            sFlowStatus = '3';
                                                        }
                                                        else if (reimport.ReImport.ReCustomsDeclaration && reimport.ReImport.ReCustomsDeclaration.Checked) {
                                                            sFlowStatus = '2';
                                                        }
                                                        else if (reimport.ReImport.FileValidation && reimport.ReImport.FileValidation.Checked) {
                                                            sFlowStatus = '1';
                                                        }
                                                        if (sFlowStatus) {
                                                            sValShow += i18next.t("common.Returns") + iLen + '-' + oReImport[sFlowStatus] + '<br/>';// ╠common.Returns⇒退運╣
                                                        }
                                                    });
                                                }
                                            }
                                        }
                                        sValShow = setGridRow(sValShow, item);
                                        return sValShow;
                                    }
                                },
                                {// ╠ExhibitionImport_Upd.ExhibitionDateStart⇒活動日期起╣
                                    name: "ExhibitionDateStart", title: 'ExhibitionImport_Upd.ExhibitionDateStart', type: "text", width: 60, itemTemplate: function (val, item) {
                                        var sVal = setGridRow(newDate(val, 'date', true), item);
                                        return sVal;
                                    }
                                },
                                {// ╠ExhibitionImport_Upd.IsSendMail⇒查詢碼寄送狀態╣
                                    name: "IsSendMail", title: 'ExhibitionImport_Upd.IsSendMail', type: "text", align: "center", width: 70, itemTemplate: function (val, item) {
                                        return val === 'Y' ? i18next.t('common.HasMail') : i18next.t('common.NotMail');// ╠common.HasMail⇒已寄送╣  ╠common.NotMail⇒未寄送╣
                                    }
                                },
                                {// ╠common.Bill_Status⇒帳單狀態╣
                                    type: "control", title: 'common.Bill_Status', itemTemplate: function (val, item) {
                                        var iTips = 0,
                                            sTipsHtml = '<div class="layui-layer-btn-c">' + i18next.t("common.Bill_Status") + '</div>',// ╠common.Bill_Status⇒帳單狀態╣
                                            saBills = $.parseJSON(!item.Bills ? '[]' : item.Bills),
                                            saReturnBills = $.parseJSON(!item.ReturnBills ? '[]' : item.ReturnBills),
                                            saReBills = [],
                                            oOption = {
                                                btnAlign: 'c',
                                                time: 600000 //一個小時（如果不配置，默认是3秒）
                                            },
                                            bOK = true,
                                            oTips = $('<span>', {
                                                'class': 'glyphicon glyphicon-info-sign',
                                                'aria-hidden': true
                                            }).on({
                                                click: function () {
                                                    oOption.btn = [i18next.t("common.Close")];// ╠common.Close⇒關閉╣
                                                    layer.msg(sTipsHtml, oOption);
                                                },
                                                mouseenter: function (event) {
                                                    delete oOption.btn;
                                                    iTips = layer.msg(sTipsHtml, oOption);
                                                },
                                                mouseleave: function (event) {
                                                    layer.close(iTips);
                                                }
                                            });
                                        if (saReturnBills.length > 0) {
                                            $.each(saReturnBills, function (idx, _return) {
                                                if (_return.Bills && _return.Bills.length > 0) {
                                                    $.each(_return.Bills, function (idx, bill) {
                                                        saReBills.push(bill);
                                                    });
                                                }
                                            });
                                        }

                                        if (saBills.length > 0 || saReBills.length > 0) {
                                            sTipsHtml += '<ul class="bill-status">';
                                            if (saBills.length > 0) {
                                                $.each(saBills, function (idx, bill) {
                                                    var sStatusName = '';
                                                    switch (bill.AuditVal) {
                                                        case '0':// ╠common.NotAudit⇒未提交審核╣
                                                            sStatusName = i18next.t("common.NotAudit");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '1':// ╠common.InAudit⇒提交審核中╣
                                                            sStatusName = i18next.t("common.InAudit");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '2':// ╠common.Audited⇒已審核╣
                                                            sStatusName = i18next.t("common.Audited");
                                                            break;
                                                        case '3':// ╠common.NotPass⇒不通過╣
                                                            sStatusName = i18next.t("common.NotPass");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '4':// ╠common.HasBeenRealized⇒已銷帳╣
                                                            sStatusName = i18next.t("common.HasBeenRealized");
                                                            break;
                                                        case '5':// ╠common.HasBeenPost⇒已過帳╣
                                                            sStatusName = i18next.t("common.HasBeenPost");
                                                            break;
                                                        case '6':// ╠common.HasVoid⇒已作廢╣
                                                            sStatusName = i18next.t("common.HasVoid");
                                                            break;
                                                        case '7':// ╠common.HasReEdit⇒抽單中╣
                                                            sStatusName = i18next.t("common.HasReEdit");
                                                            break;
                                                    }
                                                    sTipsHtml += "<li><a class='gopagetab' onclick='parent.openPageTab(\"" + sEditPrgId + "\",\"?Action=Upd&GoTab=3&ImportBillNO=" + item.ImportBillNO + "&BillNO=" + bill.BillNO + "\")' ><div>" + bill.BillNO + "</div><div>" + sStatusName + "</div></a></li>";
                                                });
                                            }
                                            if (saReBills.length > 0) {
                                                sTipsHtml += '<li>退運帳單</li>';
                                                $.each(saReBills, function (idx, bill) {
                                                    var sStatusName = '';
                                                    switch (bill.AuditVal) {
                                                        case '0':// ╠common.NotAudit⇒未提交審核╣
                                                            sStatusName = i18next.t("common.NotAudit");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '1':// ╠common.InAudit⇒提交審核中╣
                                                            sStatusName = i18next.t("common.InAudit");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '2':// ╠common.Audited⇒已審核╣
                                                            sStatusName = i18next.t("common.Audited");
                                                            break;
                                                        case '3':// ╠common.NotPass⇒不通過╣
                                                            sStatusName = i18next.t("common.NotPass");
                                                            if (bOK) { bOK = false; }
                                                            break;
                                                        case '4':// ╠common.HasBeenRealized⇒已銷帳╣
                                                            sStatusName = i18next.t("common.HasBeenRealized");
                                                            break;
                                                        case '5':// ╠common.HasBeenPost⇒已過帳╣
                                                            sStatusName = i18next.t("common.HasBeenPost");
                                                            break;
                                                        case '6':// ╠common.HasVoid⇒已作廢╣
                                                            sStatusName = i18next.t("common.HasVoid");
                                                            break;
                                                        case '7':// ╠common.HasReEdit⇒抽單中╣
                                                            sStatusName = i18next.t("common.HasReEdit");
                                                            break;
                                                    }
                                                    sTipsHtml += "<li><a class='gopagetab' onclick='parent.openPageTab(\"" + sEditPrgId + "\",\"?Action=Upd&GoTab=9&ImportBillNO=" + item.ImportBillNO + "&BillNO=" + bill.BillNO + "\")' ><div>" + bill.BillNO + "</div><div>" + sStatusName + "</div></a></li>";
                                                });
                                            }
                                            sTipsHtml += '</ul>';
                                            if (saBills.length + saReBills.length > 15) {
                                                oOption.area = ['340px', '500px'];
                                            }
                                        }
                                        else {
                                            sTipsHtml = '<div>' + i18next.t("common.NOBills") + '</div>';// ╠common.NOBills⇒還沒有帳單╣
                                            bOK = false;
                                        }
                                        if (bOK) {
                                            oTips.css('color', 'blue');
                                        }
                                        else {
                                            oTips.css('color', 'red');
                                        }
                                        return oTips;
                                    }
                                }
                                //{
                                //    name: "ExhibitionDateEnd", title: 'ExhibitionImport_Upd.ExhibitionDateEnd', type: "text", width: 70, itemTemplate: function (val, item) {
                                //        var sVal = setGridRow(newDate(val, 'date', true), item);
                                //        return sVal;
                                //    }
                                //}
                            ],
                            controller: {
                                loadData: function (args) {
                                    return fnGetPro(args);
                                }
                            },
                            onInit: function (args) {
                                oGrid = args.grid;
                            }
                        });
                        if (parent.UserInfo.roles.indexOf('Business') > -1 || parent.UserInfo.roles.indexOf('CDD') > -1) {//報關||業務
                            $('.slide-box').click();
                        }
                    });
            };
        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);