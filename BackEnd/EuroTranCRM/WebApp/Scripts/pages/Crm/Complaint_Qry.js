'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    sViewPrgId = sProgramId.replace('_Qry', '_View'),
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
                oQueryPm.ComplaintType = !oQueryPm.ComplaintType ? Enumerable.From($('[name=ComplaintType]:checkbox')).Select("x=>x.value").ToArray().join(',') : oQueryPm.ComplaintType.join(',');
                oQueryPm.DataType = !oQueryPm.DataType ? Enumerable.From($('[name=DataType]:checkbox')).Select("x=>x.value").ToArray().join(',') : oQueryPm.DataType.join(',');
                oQueryPm.Roles = parent.UserInfo.roles;
                console.log("oQueryPm:", oQueryPm);
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
             * 獲取客訴來源
             */
            setComplaintSourceDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    OrgID: 'TE',
                    ArgClassID: 'CRMComplaintSource'
                }, function (res) {
                    if (res.RESULT) {
                        let QueryData = res.DATA.rel;
                        if (QueryData.length > 0) {
                            $('#ComplaintSource').append(createOptions(QueryData, 'id', 'text', true)).select2();
                        }
                    }
                });
            },
            /**
             * 獲取組團單位
             */
            fnSetGroupUnit = function () {
                g_api.ConnectLite('Complaint_Qry', 'GetGroupUnit', {}, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                        $('#GroupUnit').html(sOptions).select2();
                    }
                });
            },
            /**
             * 獲取配合代理
             */
            fnSetCoopAgent= function () {
                g_api.ConnectLite('Complaint_Qry', 'GetCoopAgent', {}, function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        var sOptions = createOptions(saList, 'guid', 'CustomerShotCName');
                        $('#CoopAgent').html(sOptions).select2();
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
                    area: ['300px', '160px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '<div class="pop-box">\
                             <button type="button" data-i18n="common.BasicInformation" id="Complaint_BasicInformation" class="btn-custom green">基本資料</button>\
                         </div>',//╠common.BasicInformation⇒基本資料╣╠common.WenzhongPrjFile⇒文中專案檔╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            fnGet({
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
             * 頁面初始化
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
                    ]),
                    setComplaintSourceDrop(),
                    fnSetCoopAgent(),
                    fnSetGroupUnit(),
                    fnSetUserDrop([
                        {
                            Select: $('#Applicant'),
                            ShowId: true,
                            Select2: true,
                        }
                    ])
                ]).done(function () {
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
                        rowClass: function (item) {
                            if (item.Status === 'X') {
                                return 'data-void';
                            }
                        },
                        onPageChanged: function (args) {
                            cacheQueryCondition(args.pageIndex);
                        },
                        rowClick: function (args) {
                            if (navigator.userAgent.match(/mobile/i)) {
                                if ('A,C'.indexOf(args.item.DataType) > -1 && args.item.AskTheDummy === parent.UserID) {
                                    goToEdit(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                                else {
                                    goToEdit(sViewPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                                }
                            }
                        },
                        rowDoubleClick: function (args) {
                            if ('A,C'.indexOf(args.item.DataType) > -1 && args.item.CreateUser === parent.UserID) {
                                parent.openPageTab(sEditPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                            }
                            else {
                                parent.openPageTab(sViewPrgId, '?Action=Upd&Guid=' + args.item.Guid);
                            }
                        },

                        fields: [
                            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 40, sorting: false },
                            { name: "ComplaintNumber", title: '客訴編號', width: 60 },
                            {
                                name: "ComplaintTitle", title: '客訴主旨', width: 200, itemTemplate: function (val, item) {
                                    var sVal = val,
                                        oStatus = {
                                            'A': i18next.t('common.Draft'),// ╠common.Draft⇒草稿╣
                                            'B': i18next.t('common.InAudit'),// ╠common.InAudit⇒審核中╣
                                            'C-O': i18next.t('common.HasReEdited'),// ╠common.HasReEdited⇒已抽單╣
                                            'D-O': i18next.t('common.HasReturned'),// ╠common.HasReturned⇒已退件╣
                                            'H-O': i18next.t('common.Hashandle'),// ╠common.Hashandle⇒已經辦╣
                                            'E': i18next.t('common.ToHandle'),// ╠common.ToHandle⇒待經辦╣
                                            'X': i18next.t('common.HasVoid')// ╠common.HasVoid⇒已作廢╣
                                        };
                                    sVal += oStatus[item.DataType] ? '<span style="color:#DF5F09">(' + oStatus[item.DataType] + ')</span>' : '';

                                    if (item.Important > 1) {
                                        for (var i = 0; i < item.Important - 1; i++) {
                                            sVal += ' <img src="../../images/star.gif">';
                                        }
                                    }
                                    return $('<a>', { html: sVal });
                                }
                            },
                            {
                                name: "ComplaintType", title: '類型', width: 70, align: 'center', itemTemplate: function (val, item) {
                                    var sVal = '',
                                        ComplaintTypeArray = {
                                            '1': '貨損',
                                            '2': '延誤',
                                            '3': '遺失',
                                            '4': '抱怨'
                                        };
                                    sVal = ComplaintTypeArray[item.ComplaintType];
                                    return sVal;
                                } },
                            { name: "ExhibitioShotName_TW", title: '展覽簡稱', width: 200, align: 'center' },//ComplaintTypeArray
                            { name: "CustomerCName", title: '客戶名稱', width: 200, align: 'center' },
                            { name: "Complainant", title: '申訴人', width: 70, align: 'center' },
                            { name: "CreateUser", title: 'common.CreateUser', width: 70, align: 'center' },
                            {
                                name: "CreateDate", title: 'common.CreateDate', width: 90, align: 'center', itemTemplate: function (val, item) {
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
                })
                
            };

        init();
    };

require(['base', 'select2', 'jsgrid', 'util'], fnPageInit);