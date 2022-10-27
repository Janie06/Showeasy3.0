(function ($, w, d) {
    'use strict';
    /**
    * 上海駒驛須移除選項
    * @param {Object} input jquery 物件;
    */
    w.fnKeyUnit = function (input) {
        input.value = input.value.toUpperCase();
    };

    /**
    * 上海駒驛須移除選項
    * @param {Object} handle jquery 物件;
    */
    w.fnSGMod = function (handle) {
        var oHandle = handle || $('.shjy-box');
        if (parent.OrgID !== 'SG') {
            oHandle.remove();
        }
    };

    /**
     * 更新帳單信息
     * @param  {String} servname 服務端類名稱
     * @param  {String} xbid 當前資料id
     * @param  {String} billno 帳單號碼
     */
    w.fnUpdateBillInfo = function (servname, xbid, billno) {
        var oPm = { Guid: xbid };
        if (billno) {
            oPm.BillNO = billno;
        }
        g_api.ConnectLite(servname, 'UpdateBillInfo', oPm, function (res) {
            if (res.RESULT) {
                console.log(res);
            }
        });
    };

    /**
     * 更新帳單信息
     * @param  {String} billno 帳單號碼
     */
    w.fnDeleteBillInfo = function (billno) {
        var oPm = { BillNO: billno };
        g_api.ConnectLite('Exhibition', 'DeleteBillInfo', oPm, function (res) {
            if (res.RESULT) {
                console.log(res);
            }
        });
    };

    //取得下拉已選的數值
    w.getSelectedValues = function (strSelector) {
        let result = [];
        let collection = document.querySelectorAll(strSelector + " option");
        collection.forEach(function (x) {
            if (x.selected) {
                result.push(x.value);
            }
        });
        return result;
    };
    /**
     * 驗證是否存在的的有效的帳單資料
     * @param  {Array} bills 帳單
     * @return {Boolean} 是否有效
     */
    w.fnCheckBillEffective = function (bills) {
        var saBill_Effective = [];
        if (bills && bills.length > 0) {
            saBill_Effective = $.grep(bills, function (bill) {
                return bill.AuditVal !== '6';
            });
        }
        return saBill_Effective.length > 0;
    };
    /**
     * 驗證是否存在的的有效的帳單資料
     * @param  {Array} bills 退運帳單
     * @return {Boolean} 是否有效
     */
    w.fnCheckRtnBillEffective = function (bills) {
        var saBill_Effective = [];
        if (bills && bills.length > 0) {
            $.grep(bills, function (_bill) {
                if (_bill.Bills && _bill.Bills.length > 0) {
                    saBill_Effective.push($.grep(_bill.Bills, function (bill) {
                        return bill.AuditVal !== '6';
                    }));
                }
            });
        }
        return saBill_Effective.length > 0;
    };

    /**
    * 添加費用明細
    * @param  {Object} option 配置
    */
    w.fnStarFeeItems = function (option) {
        option = option || {};
        var oGrid = null,
            saItems = [],
            saProfileGets = [],
            fnSetProfileDrop = function (handle) {
                return CallAjax(ComFn.W_Com, ComFn.GetList, {
                    Type: '', Params: {
                        profiles: {
                            ProfileType: 'FeeClass',
                            OrgID: parent.OrgID,
                            UserID: parent.UserID
                        },
                        sort: { SN: 'asc' }
                    }
                }, function (res) {
                    if (res.d) {
                        saProfileGets = JSON.parse(res.d);
                        handle.html(createOptions(saProfileGets, 'SN', 'ProfileName'));
                    }
                });
            },
            fnAddProfile = function (layero) {
                var oAddPm = {};
                oAddPm.OrgID = parent.OrgID;
                oAddPm.UserID = parent.UserID;
                oAddPm.ProfileType = 'FeeClass';
                oAddPm.ProfileName = layero.find('#ProfileName').val();
                oAddPm.ProfileSet = [];
                layero.find('#lstRight option').each(function () {
                    oAddPm.ProfileSet.push(this.value);
                });
                oAddPm.ProfileSet = JSON.stringify(oAddPm.ProfileSet);
                oAddPm = packParams(oAddPm);

                if (!oAddPm.ProfileName) {
                    showMsg(i18next.t("message.ProfileFeesClassName_Required")); //╠message.ProfileFeesClassName_Required⇒請填寫個人化費用類別名稱╣
                    return false;
                }

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        profiles: oAddPm
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetProfileDrop(layero.find('#ProfileClass')).done(function () {
                            layero.find('#ProfileClass option').each(function () {
                                if ($(this).text() === oAddPm.ProfileName) {
                                    layero.find('#ProfileClass').val(this.value);
                                    return false;
                                }
                            });
                        });
                        showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                });
            },
            fnUpdProfile = function (layero) {
                var oUpdPm = {},
                    sId = layero.find('#ProfileClass').val();
                oUpdPm.ProfileName = layero.find('#ProfileName').val();
                oUpdPm.ProfileSet = [];
                layero.find('#lstRight option').each(function () {
                    oUpdPm.ProfileSet.push(this.value);
                });
                oUpdPm.ProfileSet = JSON.stringify(oUpdPm.ProfileSet);
                oUpdPm = packParams(oUpdPm, 'upd');

                if (!oUpdPm.ProfileName) {
                    showMsg(i18next.t("message.ProfileFeesClassName_Required")); // 請填寫個人化費用類別名稱
                    return false;
                }

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        profiles: {
                            values: oUpdPm,
                            keys: { SN: sId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetProfileDrop(layero.find('#ProfileClass')).done(function () {
                            layero.find('#ProfileClass').val(sId);
                        });
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                });
            },
            fnDelProfile = function (layero) {
                var sId = layero.find('#ProfileClass').val();

                if (!sId) {
                    showMsg(i18next.t("message.DeleteItem_Required")); //╠message.DeleteItem_Required⇒請選擇要刪除的項目╣
                    return false;
                }
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        profiles: {
                            SN: sId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        fnSetProfileDrop(layero.find('#ProfileClass'));
                        layero.find('#ProfileName').val('');
                        layero.find('#lstRight').html('');
                        layero.find('#lstLeft').html(createOptions(saItems, 'id', 'text', true));
                        layero.find('#lstLeft').find('option:first').remove();
                        showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            };

        getHtmlTmp('/Page/Pop/ProfileFees.html').done(function (html) {
            layer.open({
                type: 1,
                title: i18next.t('common.ProfileFees'),// ╠common.ProfileFees⇒個人化費用項目╣
                shadeClose: false,
                shade: 0.1,
                maxmin: true, //开启最大化最小化按钮
                area: ['600px', '500px'],
                content: html,
                success: function (layero, index) {
                    var elIistLeft = layero.find('#lstLeft'),
                        elLstRight = layero.find('#lstRight');
                    fnSetProfileDrop(layero.find('#ProfileClass'));
                    fnSetArgDrop([
                        {
                            ArgClassID: 'FeeClass',
                            Select: elIistLeft,
                            ShowId: true,
                            CallBack: function (data) {
                                saItems = data;
                            }
                        }
                    ]).done(function () {
                        elIistLeft.find('option:first').remove();
                        optionListSearch(elIistLeft, elLstRight, layero.find('#ProfileFilter'));
                    });
                    layero.find('#ProfileClass').on('change', function () {
                        var sProfile = this.value,
                            saProfileSet = [];
                        if (sProfile) {
                            var oProfileGet = $.grep(saProfileGets, function (e) { return e.SN.toString() === sProfile; })[0];
                            saProfileSet = JSON.parse(oProfileGet.ProfileSet || '[]');
                            layero.find('#ProfileName').val(oProfileGet.ProfileName);
                        }
                        else {
                            layero.find('#ProfileName').val('');
                        }
                        elLstRight.html('');
                        elIistLeft.html(createOptions(saItems, 'id', 'text', true)).find('option:first').remove();
                        layero.find('#lstLeft option').each(function () {
                            var sId = $(this).val();
                            if (saProfileSet.indexOf(sId) > -1) {
                                $(this).appendTo(elLstRight);
                            }
                        });
                    });
                    layero.find('.cusclass-add').on('click', function () {
                        fnAddProfile(layero);
                    });
                    layero.find('.cusclass-upd').on('click', function () {
                        fnUpdProfile(layero);
                    });
                    layero.find('.cusclass-del').on('click', function () {
                        fnDelProfile(layero);
                    });
                    layero.find('#btnToRight').on('click', function () {
                        optionListMove(elIistLeft, elLstRight);
                    });
                    layero.find('#btnToLeft').on('click', function () {
                        optionListMove(elLstRight, elIistLeft);
                    });
                    layero.find('#btnToUp').on('click', function () {
                        optionListOrder(elLstRight, true);
                    });
                    layero.find('#btnToDown').on('click', function () {
                        optionListOrder(elLstRight, false);
                    });
                    transLang(layero);
                },
                btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                yes: function (index, layero) {
                    var saRetn = [];
                    layero.find('#lstRight option').each(function () {
                        var sId = $(this).val();
                        saRetn.push({
                            guid: guid(),
                            FinancialCode: $(this).val(),
                            FinancialCostStatement: $(this).text(),
                            Memo: '',
                            FinancialCurrency: 'NTD',
                            FinancialUnitPrice: '0',
                            FinancialNumber: '',
                            FinancialUnit: '',
                            FinancialAmount: '0',
                            FinancialExchangeRate: '1',
                            FinancialTWAmount: '0',
                            FinancialTaxRate: '0',
                            FinancialTax: '0',
                            CreateUser: parent.UserID,
                            CreateDate: newDate()
                        });
                    });
                    if (typeof option.Callback === 'function') option.Callback(saRetn);
                    layer.close(index);
                },
                cancel: function () {
                    if (typeof option.CancelCallback === 'function') option.CancelCallback();
                }
            });
        });
    };

    /**
    * 複製費用項目
    * @param  {Object} option 配置
    */
    w.fnCopyFee = function (option) {
        option = option || {};
        var oGrid = null,
            oBilllist = null,
            saLastData = [],
            saFeeList = [],
            fnGetBillList = function (exhibitionno, billno, flag) {
                return g_api.ConnectLite(Service.opm, 'GetBillInfos',
                    {
                        ExhibitionNO: exhibitionno || '',
                        BillNO: billno || ''
                    },
                    function (res) {
                        if (res.RESULT) {
                            var saData = res.DATA.rel;
                            if (saData.length > 0) {
                                saLastData = JSON.parse(saData[0].FeeItems);
                            }
                            else {
                                saLastData = [];
                            }
                            if (flag) {
                                if (saData.length > 0) {
                                    var sBilllistHtml = '';
                                    $.each(saData, function (idx, _data) {
                                        sBilllistHtml += (idx === 0 ? '<li class="active">' : '<li>') + _data.BillNO + '</li>';
                                    });
                                    oBilllist.html(sBilllistHtml).find('li').click(function () {
                                        oBilllist.find('li').removeClass('active');
                                        var sBillNO = $(this).addClass('active').text(),
                                            sExhibitionNO = $('#ExhibitionName').val();
                                        fnGetBillList(sExhibitionNO, sBillNO, false);
                                    });
                                }
                                else {
                                    oBilllist.html('');
                                }
                            }
                            oGrid.jsGrid("loadData");
                        }
                    });
            }, //╠common.ExhibitionName⇒展覽名稱╣ ╠common.BillNO⇒帳單號碼╣
            sContent = '<style>.select2-container--open { z-index: 1000000001;}.jsgrid-header-cell{padding:0 0;}</style>\
                        <div class="row popsrow">\
                             <label class="col-sm-2 control-label wright" for="input-Default"><span data-i18n="common.ExhibitionName">展覽名稱</span>：</label>\
                             <div class="col-sm-4">\
                                 <select class="form-control" id="ExhibitionName"></select>\
                             </div>\
                             <label class="col-sm-1 control-label wright" for="input-Default"><span data-i18n="common.BillNO">帳單號碼</span>：</label>\
                             <div class="col-sm-4">\
                                 <input type="text" class="form-control w100p" id="BillNO" maxlength="50">\
                             </div>\
                        </div>\
                        <div class="row popsrow">\
                            <div class="col-sm-2">\
                                <div class="bill-box slimscroll">\
                                    <ul class="bill-list">\
                                    </ul>\
                                </div>\
                            </div>\
                            <div class="col-sm-10">\
                                <div id="jsGrid_Fees"></div>\
                            </div>\
                            <div class="col-sm-2"></div>\
                        </div>';
        layer.open({
            type: 1,
            title: i18next.t('common.CopyFeeItems'),//╠common.CopyFeeItems⇒複製費用項目╣
            shadeClose: false,
            shade: 0.1,
            maxmin: true, //开启最大化最小化按钮
            area: ['800px', '500px'],
            content: sContent,
            success: function (layero, index) {
                fnSetEpoDrop({
                    Select: $('#ExhibitionName'),
                    Select2: true
                });
                oBilllist = layero.find('.bill-list');

                oGrid = $("#jsGrid_Fees").jsGrid({
                    width: "100%",
                    height: "320px",
                    autoload: true,
                    filtering: false,
                    pageLoading: true,
                    pageIndex: 1,
                    pageSize: 10000,
                    fields: [
                        {
                            name: "OrderBy", title: '#', width: 30, align: "center"
                        },
                        {
                            width: 50, sorting: false, align: "center",
                            headerTemplate: function () {
                                return [$("<input>", {
                                    id: 'SelectAll',
                                    type: 'checkbox', click: function () {
                                        if (this.checked) {
                                            $("#jsGrid_Fees").find('[type=checkbox]').each(function () {
                                                this.checked = true;
                                            });
                                            saFeeList = clone(saLastData);
                                        }
                                        else {
                                            $("#jsGrid_Fees").find('[type=checkbox]').each(function () {
                                                this.checked = false;
                                            });
                                            saFeeList = [];
                                        }
                                    }
                                }), $('<label />', { for: 'SelectAll', 'data-i18n': 'common.SelectAll' })];//╠common.SelectAll⇒全選╣
                            },
                            itemTemplate: function (value, item) {
                                return $("<input>", {
                                    type: 'checkbox', click: function (e) {
                                        e.stopPropagation();
                                        if (this.checked) {
                                            saFeeList.push(item);
                                        }
                                        else {
                                            var saNewList = [];
                                            $.each(saFeeList, function (idx, data) {
                                                if (item.guid !== data.guid) {
                                                    saNewList.push(data);
                                                }
                                            });
                                            saFeeList = saNewList;
                                            $('#jsGrid_Fees').find('#SelectAll')[0].checked = false;
                                        }
                                    }
                                });
                            }
                        },
                        {
                            name: "FinancialCode", title: 'common.Financial_Code', width: 70, type: "text"
                        },
                        {
                            name: "FinancialCostStatement", title: 'common.Financial_CostStatement', width: 150, type: "text",
                            itemTemplate: function (val, item) {
                                return val === '' ? item.Memo : val;
                            }
                        },
                        {
                            name: "FinancialCurrency", title: 'common.Financial_Currency', width: 60, type: "text", align: "center"
                        },
                        {
                            name: "FinancialUnitPrice", title: 'common.Financial_UnitPrice', width: 80, align: "right", type: "text",
                            itemTemplate: function (val, item) {
                                return fMoney(val || 0, 2);
                            }
                        },
                        {
                            name: "FinancialNumber", title: 'common.Financial_Number', width: 80, type: "text", align: "right"
                        },
                        {
                            name: "FinancialUnit", title: 'common.Financial_Unit', width: 80, type: "text"
                        },
                        {
                            name: "FinancialAmount", title: 'common.Financial_Amount', width: 80, type: "text", align: "right",
                            itemTemplate: function (val, item) {
                                return fMoney(val || 0, 2);
                            }
                        },
                        {
                            name: "FinancialExchangeRate", title: 'common.ExchangeRate', width: 60, type: "text", align: "center"
                        },
                        {
                            name: "FinancialTWAmount", title: 'common.Financial_TWAmount', width: 80, align: "right", type: "text",
                            itemTemplate: function (val, item) {
                                return fMoney(val || 0, 2);
                            }
                        },
                        {
                            name: "FinancialTaxRate", title: 'common.Financial_TaxRate', width: 60, align: "center", type: "text"
                        },
                        {
                            name: "FinancialTax", title: 'common.Financial_Tax', width: 80, align: "right", type: "text",
                            itemTemplate: function (val, item) {
                                return fMoney(val || 0, 2);
                            }
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: saLastData,
                                itemsCount: saLastData.length //data.length
                            };
                        }
                    }
                });
                layero.find('.layui-layer-btn1').css({ 'border-color': '#4898d5', 'background-color': '#1E9FFF', 'color': '#fff' });
            },
            btn: [i18next.t('common.Toolbar_Qry'), i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Toolbar_Qry⇒查詢╣ ╠common.Confirm⇒確定╣ ╠common.Cancel⇒取消╣
            yes: function (index, layero) {
                var sExhibitionNO = layero.find('#ExhibitionName').val(),
                    sBillNO = layero.find('#BillNO ').val();
                fnGetBillList(sExhibitionNO, sBillNO, true);
            },
            btn2: function (index, layero) {
                if (typeof option.Callback === 'function') option.Callback(saFeeList);
                layer.close(index);
            }
        });
    };

    w.fnGetCurrencyByYear = function (opm) {

        return g_api.ConnectLite('CurrencySetup_Qry', 'GetCurrencyByYear', {
            year: opm.Year
        }, function (res) {
            if (res.RESULT) {
                var saList = res.DATA.rel;

                if (opm.CallBack && typeof opm.CallBack === 'function') {
                    opm.CallBack(saList);
                }
            }
        }, {}, false);
    };

    w.roundDecimal = function (val, precision) {
        return Math.round(Math.round(val * Math.pow(10, (precision || 0) + 1)) / 10) / Math.pow(10, (precision || 0));
    };


    /**
    * 檢驗單據是否有權限存取
    * @param  {Object} option 配置
    */
    w.ExhibitionBillAuthorize = function (Opm) {
        //人員:負責業務、創建人、老闆、部門主管、直屬主管
        //角色:財務組、Admin
        //BillAutherizedList 例外清單
        var BillAutherizedList = parent.SysSet.BillAutherizedList.toLowerCase().indexOf(parent.UserID) > -1;
        var AccessAuthorize = false;
        var CurrentUserID = parent.UserID;
        var ResponsibleToThisExhibition = Opm.ResponsiblePerson === CurrentUserID || Opm.CreateUser === CurrentUserID;
        if (!ResponsibleToThisExhibition && !BillAutherizedList) {
            var QueryData = {
                RuleID: 'Account,Admin',
                ResponsiblePerson: Opm.ResponsiblePerson
            };
            g_api.ConnectLite(Service.opm, 'GetExhibitionBillAuthorize', QueryData, function (res) {
                if (res.RESULT) {
                    AccessAuthorize = res.DATA.rel.indexOf(parent.UserID) > -1
                }
                else {
                    AccessAuthorize = false;
                }
            }, {}, false);
        }
        return ResponsibleToThisExhibition || AccessAuthorize || BillAutherizedList;
    };


    /**
     * 開放權限可以編輯
     */
    w.fnOpenAccountingArea = function (group, userid) {
        if (userid.indexOf('Admin') > -1 || userid.indexOf('Account') > -1 && group.length > 0) {
            group.find('input').removeAttr('disabled');
            group.find('select').removeAttr('disabled');
        }
    };


    /**
    * 計算報價費用
    */
    w.fnCalcuQuotationFee = function (OriginFeeField, CountFeeField, CurrencyID, ExchangeRate) {
        let RoundingPoint = CurrencyID === 'NTD' ? 0 : 2;
        $.each(CountFeeField, function (idx, item) {
            var OFF = OriginFeeField[idx];
            if ('TG,TE'.indexOf(parent.OrgID) > -1)
                RoundingPoint = 0;
            let prepay = 0;
            if ($(OFF).find('.prepay').val())
                prepay = roundDecimal(parseFloat($(OFF).find('.prepay').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
            let subtotal = roundDecimal(parseFloat($(OFF).find('.subtotal').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
            let taxtotal = roundDecimal(parseFloat($(OFF).find('.taxtotal').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
            let boxtotal = (subtotal + taxtotal) - prepay;
            //let boxtotal = roundDecimal(parseFloat($(OFF).find('.boxtotal').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
            $(item).find('.mprepay ').val(fMoney(prepay, RoundingPoint, ''));
            $(item).find('.msubtotal').val(fMoney(subtotal, RoundingPoint, ''));
            $(item).find('.mtaxtotal').val(fMoney(taxtotal, RoundingPoint, ''));
            $(item).find('.mboxtotal').val(fMoney(boxtotal, RoundingPoint, ''));
        });
    };

    w.fnCalcuBillsFee = function (oBillBox, OriginFeeField, CountFeeField, CurrencyID, ExchangeRate) {
        let RoundingPoint = CurrencyID === 'NTD' ? 0 : 2;
        if ('TG,TE'.indexOf(parent.OrgID) > -1) {
            RoundingPoint = 0;
        }
        let OFF = oBillBox.find(OriginFeeField);
        let iAdvance = roundDecimal(parseFloat($(OFF).find('.prepay').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
        let iSubtotal = roundDecimal(parseFloat($(OFF).find('.subtotal').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
        let iTaxtotal = roundDecimal(parseFloat($(OFF).find('.taxtotal').val().replaceAll(',', '')) * ExchangeRate, RoundingPoint);
        let iTaxSubtotal = iSubtotal + iTaxtotal;
        let iPaytotal = iTaxSubtotal - iAdvance;
        //let iTaxSubtotal = parseFloat($(OFF).find('.boxtotal').val().replaceAll(',', '')) * ExchangeRate;
        //let iPaytotal = parseFloat($(OFF).find('.paytotal').val().replaceAll(',', '')) * ExchangeRate;
        oBillBox.find(CountFeeField + ' .mprepay').val(fMoney(iAdvance, RoundingPoint, ''));
        oBillBox.find(CountFeeField + ' .msubtotal').val(fMoney(iSubtotal, RoundingPoint, ''));
        oBillBox.find(CountFeeField + ' .mtaxtotal').val(fMoney(iTaxtotal, RoundingPoint, ''));
        oBillBox.find(CountFeeField + ' .mboxtotal').val(fMoney(iTaxSubtotal, RoundingPoint, ''));
        oBillBox.find(CountFeeField + ' .mpaytotal').val(fMoney(iPaytotal, RoundingPoint, ''));
    };

    /**
     * 輸入預收事件
     */
    w.SetBillPrepayEvent = function SetBillPrepayEvent(oBillBox, bill) {
        oBillBox.find('[data-id="Advance"]').on('keyup blur', function (e) {
            //bill.AmountTaxSum紀錄外幣為主
            var ExchangeRate = (bill.ExchangeRate || 1.00);
            var iAdvance = parseFloat((this.value === '' ? '0' : this.value).replaceAll(',', ''));
            var iTotal = bill.AmountTaxSum;
            var iTotalReceivable = iTotal - iAdvance;
            oBillBox.find('.paytotal').val(fMoney(iTotalReceivable, 2, bill.Currency));
            bill.Advance = iAdvance;
            bill.TotalReceivable = iTotalReceivable;
            var iAdvance_main = iAdvance * ExchangeRate;
            var iTotal_main = iTotal * ExchangeRate;
            oBillBox.find('[data-id="mAdvance"]').val(fMoney(iAdvance_main, 0, 'NTD'));
            oBillBox.find('.mpaytotal').val(fMoney((iTotal_main - iAdvance_main), 0, 'NTD'));
        });
        oBillBox.find('[data-id="mAdvance"]').on('keyup blur', function (e) {
            var ExchangeRate = (bill.ExchangeRate || 1.00);
            var iAdvance = parseFloat((this.value === '' ? '0' : this.value).replaceAll(',', ''));
            var iTotal = parseFloat((bill.AmountTaxSum || 0) * ExchangeRate);
            oBillBox.find('.mpaytotal').val(fMoney((iTotal - iAdvance), 0, 'NTD'));
            var iAdvance_foreign = iAdvance / ExchangeRate;
            var iTotal_foreign = bill.AmountTaxSum;
            var TotalReceivable_foreign = iTotal_foreign - iAdvance_foreign;
            oBillBox.find('[data-id="Advance"]').val(fMoney(iAdvance_foreign, 2, bill.Currency));
            oBillBox.find('.paytotal').val(fMoney(TotalReceivable_foreign, 2, bill.Currency));
            bill.Advance = iAdvance_foreign;
            bill.TotalReceivable = TotalReceivable_foreign;
        });
    }

    /**
     * 檢查主幣別
     */
    w.fnCheckMainOrForeignCurrency = function (chosenCurrency) {
        var TWDAsAMainCurrency = 'TE,TG'.indexOf(parent.OrgID) > -1;
        var UsingMainCurrency = false;
        switch (TWDAsAMainCurrency) {
            case true:
                {
                    if (chosenCurrency === 'NTD') {
                        UsingMainCurrency = true;
                    }
                }
                break;
            case false:
                {
                    if (chosenCurrency === 'CNY' || chosenCurrency === 'RMB') {
                        UsingMainCurrency = true;
                    }
                }
                break;
            default:
                break;
        }
        return UsingMainCurrency;
    };

})(jQuery, window, document);