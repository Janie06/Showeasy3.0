'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var oForm = $('#form_main'),
            oTimeOut = null,
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: '',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @param {Object} args 查詢參數
             * @return {Object} Ajax 物件
             */
            fnGetPro = function (args) {
                var oQueryPm = getFormSerialize(oForm);
                $.extend(oQueryPm, oBaseQueryPm, args);
                oBaseQueryPm.pageIndex = oQueryPm.pageIndex;

                return g_api.ConnectLite(sProgramId, 'QueryPage', oQueryPm, function (res) {
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
                    title: i18next.t('common.DownLoadDocuments'), // ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['200px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                            <button type="button" data-i18n="common.BasicInformation" id="OtherBusiness_BasicInformation" class="btn-custom green">基本資料</button>\
                         </div>',
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
                    ])
                ])
                    .done(function () {
                        reSetQueryPm(sProgramId);
                        var sCode = parent.UserInfo.ServiceCode;
                        /* if (sCode && parent.SysSet.IMCode.indexOf(sCode) > -1 && parent.UserInfo.roles.indexOf('CDD') === -1) {
                            $('#ResponsiblePerson').val(parent.UserInfo.MemberID);
                        } */

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
                                        goToEdit(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                    }
                                }
                                else {
                                    showMsg(i18next.t("message.NoPermission")); // ╠message.NoPermission⇒沒有權限╣
                                }
                            },
                            rowDoubleClick: function (args) {
                                if ((args.item.IsVoid === 'Y' && parent.UserInfo.IsManager) || args.item.IsVoid === 'N') {
                                    parent.openPageTab(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                                else {
                                    showMsg(i18next.t("message.NoPermission")); // ╠message.NoPermission⇒沒有權限╣
                                }
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
                                    name: "RowIndex", title: 'common.RowNumber', type: "text", width: 50, align: "center", sorting: false
                                },
                                {
                                    name: "ExhibitionName", title: 'common.ExhibitionName', type: "text", width: 150
                                },
                                {// ╠common.DeclarationNO⇒報單號碼╣
                                    name: "ImportDeclarationNO", title: 'common.DeclarationNO', type: "text", width: 100
                                },
                                {
                                    name: "AgentName", title: 'ExhibitionImport_Upd.Agent', type: "text", width: 150
                                },
                                {
                                    name: "ResponsiblePersonName", title: 'common.ResponsiblePerson', type: "text", width: 100
                                },
                                {
                                    type: "control", title: 'common.Bill_Status', itemTemplate: function (val, item) {// ╠common.Bill_Status⇒帳單狀態╣
                                        var iTips = 0,
                                            sTipsHtml = '<div class="layui-layer-btn-c">' + i18next.t("common.Bill_Status") + '</div>',// ╠common.Bill_Status⇒帳單狀態╣
                                            saBills = $.parseJSON(!(item.Bills) ? '[]' : item.Bills),
                                            bOK = true,
                                            oOption = {
                                                btnAlign: 'c',
                                                time: 600000 //一個小時（如果不配置，默认是3秒）
                                            },
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
                                        if (saBills.length > 0) {
                                            sTipsHtml += '<ul class="bill-status">';
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
                                                sTipsHtml += "<li><a class='gopagetab' onclick='parent.openPageTab(\"" + sEditPrgId + "\",\"?Action=Upd&GoTab=3&Guid=" + item.Guid + "&BillNO=" + bill.BillNO + "\")' ><div>" + (bill.SupplierName || bill.SupplierEName || '') + '（' + bill.BillNO + "）</div><div>" + sStatusName + "</div></a></li>";
                                            });
                                            sTipsHtml += '</ul>';
                                            oOption.area = ['550px'];
                                            if (saBills.length > 15) {
                                                oOption.area = ['550px', '500px'];
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