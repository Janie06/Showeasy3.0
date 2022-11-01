'use strict';
var fnPageInit = function () {
    var oAuditFlag = {
        'N': 'common.NotAudit',
        'Y': 'common.Audited',
        'P': 'common.InAudit',
        'A': 'common.AuditAgain',
        'Z': 'common.ApplyforUpdateing',
        'Q': 'common.NotPass'
    },
        setStateDrop = function () {
            return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                OrgID: 'TE',
                ArgClassID: 'Area',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    let saState = res.DATA.rel;
                    if (saState.length > 0) {
                        $('#State').append(createOptions(saState, 'id', 'text', true)).select2();
                    }
                }
            });
        },
        canDo = new CanDo({
            sortField: 'CreateDate',
            sortOrder: 'desc',
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['OrgID', 'guid'],
            /**
             * Grid欄位設置（可以是 function）
             */
            gridFields: [
                {
                    name: "RowIndex", title: 'common.RowNumber', type: "text", width: 50, align: "center", sorting: false
                },
                {
                    name: "CustomerCName", title: 'Customers_Upd.CustomerCName', type: "text", width: 200
                },
                {
                    name: "CustomerEName", title: 'Customers_Upd.CustomerEName', type: "text", width: 200
                },
                {
                    name: "CustomerShotCName", title: 'Customers_Upd.CustomerShotCName', type: "text", width: 100
                },
                {
                    name: "CustomerNO", title: 'Customers_Upd.CustomerNO', type: "text", align: "center", width: 80
                },
                {
                    name: "UniCode", title: 'Customers_Upd.UniCode', type: "text", align: "center", width: 80
                },
                {
                    name: "Contactors", title: 'common.Contactor', type: "text", width: 120, itemTemplate: function (val, item) {
                        var saContactors = JSON.parse(item.Contactors || '[]'),
                            sContactors = '';
                        if (saContactors.length > 0) {
                            sContactors = Enumerable.From(saContactors).Select("$.FullName").ToArray().join('，');
                        }
                        return sContactors;
                    }
                },
                {
                    name: "Address", title: 'common.Address', type: "text", width: 230
                },
                {
                    name: "CreateUserName", title: 'common.CreateUser', type: "text", width: 70
                },
                {
                    name: "CreateDate", title: 'common.CreateDate', type: "text", width: 120, itemTemplate: function (val, item) {
                        return newDate(val);
                    }
                },
                {
                    name: "IsAudit", title: 'common.Audit_Status', type: "text", width: 80, align: "center", itemTemplate: function (val, item) {
                        return i18next.t(oAuditFlag[val]);
                    }
                },
                {
                    name: "Effective", title: 'common.Status', type: "text", width: 50, align: "center", itemTemplate: function (val, item) {
                        return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');// ╠common.Effective⇒有效╣ ╠common.Invalid⇒無效╣
                    }
                }
            ],
            /**
             * 打開要匯出的pop選擇匯出類別
             */
            getExcel: function (pargs) {
                layer.open({
                    type: 1,
                    title: i18next.t('common.DownLoadDocuments'),// ╠common.DownLoadDocuments⇒下載文檔╣
                    area: ['200px', '280px'],//寬度
                    shade: 0.75,//遮罩
                    shadeClose: true,
                    btn: [i18next.t('common.Cancel')],// ╠common.Cancel⇒取消╣
                    content: '\
                             <div class="pop-box">\
                               <p><button type="button" data-i18n="common.BasicInformation" id="Cus_BasicInformation" class="btn-custom w100p  green">基本資料</button></p>\
                               <p><button type="button" data-i18n="common.Cus_Email" id="Cus_Email" class="btn-custom w100p green">名稱+Email</button></p>\
                               <p><button type="button" data-i18n="common.WenzhongCusFile" id="Cus_WenzhongCusFile" class="btn-custom w100p green">文中客供商檔</button></p>\
                             </div>',//╠common.BasicInformation⇒基本資料╣╠common.Cus_Email⇒名稱+Email╣╠common.WenzhongCusFile⇒文中客供商檔╣
                    success: function (layero, idx) {
                        $('.pop-box :button').click(function () {
                            var sToExcelType = this.id;
                            canDo.getPage({
                                Excel: true,
                                ExcelType: sToExcelType,
                                Index: idx
                            });
                        });
                        canDo._transLang(layero);
                    }
                });
            },
            /**
             * 頁面初始化
             * @param  {Object} pargs CanDo 對象
             */
            pageInit: function (pargs) {

                var ss = canDo;
                $.whenArray([
                    setStateDrop(),
                    fnSetUserDrop([
                        {
                            Select: $('#CreateUser'),
                            ShowId: true,
                            Select2: true
                        }
                    ]),
                    fnSetArgDrop([
                        {
                            ArgClassID: 'TranType',
                            CallBack: function (data) {
                                var sOptions = '<label for="TransactionType_7"><input type="radio" id="TransactionType_7" name="TransactionType" value="A,B,C,D,E,F" checked="checked">全部</label>' + createRadios(data, 'id', 'text', 'TransactionType')
                                $('#transactiontype').html(sOptions).find('[name="TransactionType"]').click(function () {
                                    $('#Toolbar_Qry').trigger('click');
                                });
                                pargs._uniformInit($('#transactiontype'));
                            }
                        }
                    ]),
                ])
                    .done(function () {
                        pargs._reSetQueryPm();
                        pargs._initGrid();
                    });
            }
        });
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);