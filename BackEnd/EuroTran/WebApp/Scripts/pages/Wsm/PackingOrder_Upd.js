'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('AppointNO'),
    sCheckId = sDataId,
    fnPageInit = function () {
        var oCurData = {},
            oForm = $('#form_main'),
            oCostRules = {},
            oGrid = null,
            oValidator = null,
            sExhibitionNO = '',
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                if (sDataId) {
                    return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                        Type: '',
                        Params: {
                            packingorder: {
                                AppointNO: sDataId
                            }
                        }
                    }, function (res) {
                        if (res.d) {
                            var oRes = $.parseJSON(res.d);
                            oCurData = oRes;
                            oCurData.Total = oCurData.Total || 0;
                            oCurData.PackingInfo = $.parseJSON(oCurData.PackingInfo || '[]');
                            setFormVal(oForm, oRes);
                            fnSetImpCusDrop(oCurData.ExhibitionNO, oCurData.CustomerId);
                            setNameById().done(function () {
                                getPageVal();//緩存頁面值，用於清除
                            });
                            $('#AppointNO').attr('disabled', 'disabled');
                            moneyInput($('#Total'), 0);
                        }
                    });
                }
                else {
                    oCurData.Total = 0;
                    oCurData.PackingInfo = [];
                    fnSetImpCusDrop();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param {String} flag 新增或儲存後新增
             * @return {Boolean} 停止標記
             */
            fnAdd = function (flag) {
                var data = getFormSerialize(oForm),
                    sDateStr = new Date().formate("yyyyMMdd");
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.IsKeyMode = true;
                data.Total = oCurData.Total;
                data.CompName = $('#CustomerId option:selected').text();
                data.AppointNO = 'SerialNumber|' + parent.UserInfo.OrgID + '|A' + sExhibitionNO + '|Empty|3|' + sDateStr + '|';
                if (oCurData.PackingInfo.length === 0) {
                    showMsg(i18next.t("message.PackingInfo_required")); // ╠message.PackingInfo_required⇒請添加服務明細╣
                    return false;
                }
                data.PackingInfo = JSON.stringify(oCurData.PackingInfo);

                CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                    Params: {
                        packingorder: data
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        if (flag === 'add') {
                            showMsgAndGo(i18next.t("message.Save_Success"), sQueryPrgId); // ╠message.Save_Success⇒新增成功╣
                        }
                        else {
                            showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                        }
                        parent.msgs.server.broadcast(data);
                    }
                    else {
                        showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                    }
                });
            },
            /**
             * 修改資料
             * @return {Boolean} 停止標記
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);

                data = packParams(data, 'upd');
                data.Total = oCurData.Total;
                data.CompName = $('#CustomerId option:selected').text();
                if (oCurData.PackingInfo.length === 0) {
                    showMsg(i18next.t("message.PackingInfo_required")); // ╠message.PackingInfo_required⇒請添加服務明細╣
                    return false;
                }
                data.PackingInfo = JSON.stringify(oCurData.PackingInfo);

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        packingorder: {
                            values: data,
                            keys: {
                                AppointNO: sDataId,
                                OrgID: parent.OrgID
                            }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        showMsgAndGo(i18next.t("message.Modify_Success"), sQueryPrgId);//╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                });
            },
            /**
             * 資料刪除
             */
            fnDel = function () {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        packingorder: {
                            AppointNO: sDataId
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsgAndGo(i18next.t("message.Delete_Success"), sQueryPrgId); // ╠message.Delete_Success⇒刪除成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
             * 開啟新增Pop
             * @param {String} flag 新增或修改
             * @param {Object} curitem 當前編輯資料
             */
            fnOpenPackingInfo = function (flag, curitem) {
                getHtmlTmp('/Page/Pop/PackingInfo.html').done(function (html) {
                    layer.open({
                        type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                        title: i18next.t("PackingOrder_Upd.PackingInfo"), // ╠PackingOrder_Upd.PackingInfo⇒服務明細╣
                        area: '640px;',//寬度
                        shade: 0.75,//遮罩
                        closeBtn: 1,
                        maxmin: true, //开启最大化最小化按钮
                        id: 'layer_PackingInfo', //设定一个id，防止重复弹出
                        offset: '10px',//右下角弹出
                        anim: 0,//彈出動畫
                        btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                        btnAlign: 'c',//按鈕位置
                        content: html,
                        success: function (layero, index) {
                            var fnSetCheckbox = function (el) {
                                var ul = $('#form_PackingInfo'),
                                    bCheck = false;
                                if (el.id === 'ExpoSplit' || el.id === 'ExpoPack') {
                                    ul.find('.packing').each(function () {
                                        if (this.checked) {
                                            bCheck = true;
                                            return false;
                                        }
                                    });
                                    if (bCheck) {
                                        ul.find('.single-packing,#ExpoDays').prop('disabled', true);
                                        ul.find('.textStyle').val('');
                                    }
                                    else {
                                        ul.find('.single-packing,#ExpoDays').prop('disabled', false);
                                    }
                                }
                                else if (el.id === 'ExpoFeed' || el.id === 'ExpoStorage') {
                                    ul.find('.single-packing').each(function () {
                                        if (this.checked) {
                                            bCheck = true;
                                            return false;
                                        }
                                    });
                                    if (bCheck) {
                                        ul.find('.packing').prop('disabled', true);
                                        /*** 2019/01/22 Yang Leo 點選「空箱收送」時，已包含「空箱存放」，直接鎖住「空箱存放」欄位 和 點選「空箱存放」時，直接鎖住「空箱收送」欄位 Star ***/
                                        if (oCostRules.IsMerge === 'Y') {
                                            if (el.id.indexOf('ExpoFeed') > -1) {
                                                ul.find("[id^='ExpoStorage'],.textStyle").prop('disabled', true);
                                            } else {
                                                ul.find("[id^='ExpoFeed']").prop('disabled', true);
                                            }
                                        }
                                        /*** 2019/01/22 Yang Leo 點選「空箱收送」時，已包含「空箱存放」，直接鎖住「空箱存放」欄位 和 點選「空箱存放」時，直接鎖住「空箱收送」欄位 End ***/
                                    }
                                    else {
                                        ul.find('.packing').prop('disabled', false);
                                        /*** 2019/01/22 Yang Leo 取消點選「空箱收送」時，已包含「空箱存放」，直接解鎖「空箱存放」欄位 和 取消點選「空箱存放」時，直接解鎖「空箱收送」欄位 Star ***/
                                        if (oCostRules.IsMerge === 'Y') {
                                            if (el.id.indexOf('ExpoFeed') > -1) {
                                                ul.find("[id^='ExpoStorage'],.textStyle").prop('disabled', false);
                                            } else {
                                                ul.find("[id^='ExpoFeed']").prop('disabled', false);
                                            }
                                        }
                                        /*** 2019/01/22 Yang Leo 取消點選「空箱收送」時，已包含「空箱存放」，直接解鎖「空箱存放」欄位 和 取消點選「空箱存放」時，直接解鎖「空箱收送」欄位 End ***/
                                    }
                                }
                            };
                            if (curitem) {
                                setFormVal($('#form_PackingInfo'), curitem);
                                var checkBox = layero.find('[type="checkbox"]:not(:first):checked');
                                if (checkBox.length > 0) {
                                    fnSetCheckbox(checkBox[0]);
                                }
                            }
                            moneyInput($('[data-name="int"]'), 0);
                            layero.find('[type="checkbox"]').click(function () {
                                fnSetCheckbox(this);
                            });

                            transLang(layero.find('#form_PackingInfo'));
                        },
                        yes: function (index, layero) {
                            var data = getFormSerialize($('#form_PackingInfo')),
                                oExpoType = {
                                    '01': '裸機',
                                    '02': '木箱',
                                    '03': '散貨',
                                    '04': '打板',
                                    '05': '其他'
                                },
                                sError = '';
                            if (flag === 'add') {
                                data.Guid = guid();
                                data.Index = oCurData.PackingInfo.length + 1;
                            }
                            else {
                                data.Guid = curitem.Guid;
                                data.Index = curitem.Index;
                            }
                            data.ExpoLen = fnPackNum(data.ExpoLen);
                            data.ExpoWidth = fnPackNum(data.ExpoWidth);
                            data.ExpoHeight = fnPackNum(data.ExpoHeight);
                            data.TotalCBM = data.ExpoLen * data.ExpoWidth * data.ExpoHeight / 1000000;//CBM
                            data.ExpoWeight = fnPackNum(data.ExpoWeight);//重量
                            data.ExpoWeightTon = data.ExpoWeight / 1000;//噸
                            data.ExpoNumber = fnPackNum(data.ExpoNumber);//件數
                            data.SubTotal = 0;//小計
                            data.ExpoDays = fnPackNum(data.ExpoDays);//天數
                            data.SubText = [];//
                            data.ExpoStack = data.ExpoStack || false;
                            data.ExpoSplit = data.ExpoSplit || false;
                            data.ExpoPack = data.ExpoPack || false;
                            data.ExpoFeed = data.ExpoFeed || false;
                            data.ExpoStorage = data.ExpoStorage || false;
                            data.ExpoTypeText = oExpoType[data.ExpoType];

                            if (data.ExpoNumber > 0) {//件數
                                data.SubText.push(data.ExpoNumber.toString().toMoney() + '件');
                                if (data.TotalCBM > 0) {//CBM
                                    var iTotalCBM = data.TotalCBM * data.ExpoNumber;
                                    if (iTotalCBM < 1) {
                                        iTotalCBM = 1;
                                    }
                                    data.SubText.push(iTotalCBM.toFloat(2).toString().toMoney() + 'CBM');
                                }
                                if (data.ExpoWeight > 0) {//KG
                                    var iExpoWeight = data.ExpoWeight * data.ExpoNumber;
                                    if (iExpoWeight < 1) {
                                        iExpoWeight = 1;
                                    }
                                    data.SubText.push(iExpoWeight.toFloat(2).toString().toMoney() + 'KG');
                                }
                            }

                            if (data.ExpoStorage && data.TotalCBM > 0 && data.ExpoNumber > 0 && data.ExpoDays > 0) {//儲存
                                data.SubTotal += data.TotalCBM * data.ExpoNumber * data.ExpoDays * fnPackNum(oCostRules.StoragePrice);
                            }
                            if (data.ExpoFeed && data.TotalCBM > 0 && data.ExpoNumber > 0) {// 空箱收送
                                data.SubTotal += data.TotalCBM * data.ExpoNumber * fnPackNum(oCostRules.FeedingPrice);
                            }
                            if (data.ExpoPack && data.TotalCBM > 0 && data.ExpoNumber > 0) {//裝箱
                                data.SubTotal += (data.TotalCBM < 1 ? 1 : data.TotalCBM) * data.ExpoNumber * fnPackNum(oCostRules.PackingPrice);
                            }
                            if (data.ExpoSplit && data.TotalCBM > 0 && data.ExpoNumber > 0) {//拆箱
                                data.SubTotal += (data.TotalCBM < 1 ? 1 : data.TotalCBM) * data.ExpoNumber * fnPackNum(oCostRules.PackingPrice);
                            }
                            if (data.ExpoStack && data.ExpoWeightTon > 0 && data.ExpoNumber > 0) {//堆高
                                /*** 2019/01/22 Yang Leo 台北駒驛「線上預約」堆高機價格計算方式調整「材積重」及「重量」擇一個重的去計價 Star ***/
                                //尺寸：200x200x200cm(換算材積重 = 1,336KGS)
                                //公式 = 長  寬  高 / 1000000 * 167
                                //重量：900KGS
                                //目前系統公式直接以重量900KGS計價 -> $920 / 件
                                //明年改為以材積重1,336KGS去計價 -> $1,700 / 件
                                if ((data.TotalCBM * 167) > data.ExpoWeight) { //公式=長*寬*高/1000000*167
                                    data.ExpoWeightTon = (data.TotalCBM * 167) / 1000;
                                }
                                /*** 2019/01/22 Yang Leo 台北駒驛「線上預約」堆高機價格計算方式調整「材積重」及「重量」擇一個重的去計價 End ***/
                                var rule = fnGetCurRule(data.ExpoWeightTon);
                                if (rule.PricingMode === 'T') {
                                    data.SubTotal += data.ExpoWeightTon * data.ExpoNumber * fnPackNum(rule.Price);
                                }
                                else {
                                    data.SubTotal += data.ExpoNumber * fnPackNum(rule.Price);
                                }
                            }

                            if (data.ExpoStack || data.ExpoSplit || data.ExpoPack || data.ExpoFeed || data.ExpoStorage) {
                                if (data.ExpoNumber === 0) {
                                    sError += '[件數]不能為空<br/>';
                                }
                                if (data.ExpoStack) {
                                    if (data.ExpoWeightTon === 0) {
                                        sError += '[堆高機]服務重量不能為空<br/>';
                                    }
                                    if (data.TotalCBM === 0) {
                                        sError += '[堆高機]服務尺寸不能為空<br/>';
                                    }
                                }
                                if (data.ExpoSplit) {
                                    if (data.TotalCBM === 0) {
                                        sError += '[裝箱]服務尺寸不能為空<br/>';
                                    }
                                }
                                if (data.ExpoPack) {
                                    if (data.TotalCBM === 0) {
                                        sError += '[拆箱(含空箱收送與儲存)]服務尺寸不能為空<br/>';
                                    }
                                }
                                if (data.ExpoFeed) {
                                    if (data.TotalCBM === 0) {
                                        sError += '[空箱收送]服務尺寸不能為空 <br />';
                                    }
                                }
                                if (data.ExpoStorage) {
                                    if (data.TotalCBM === 0) {
                                        sError += '[空箱存放 / 存放]服務尺寸不能為空 <br />';
                                    }
                                    if (data.ExpoDays === 0) {
                                        sError += '[空箱存放 / 存放]服務天數不能為空 <br /> ';
                                    }
                                }
                            }
                            else {
                                sError += '請至少選擇一種服務<br/>';
                            }
                            if (sError) {
                                showMsg(sError);
                                return false;
                            }

                            if (flag === 'add') {
                                oCurData.PackingInfo.push(data);
                            }
                            else {
                                $.grep(oCurData.PackingInfo, function (item) {
                                    if (item.Guid === curitem.Guid) {
                                        $.extend(item, data);
                                    }
                                });
                            }
                            fnTotalSum();
                            oGrid.loadData();
                            layer.close(index);
                        }
                    });
                });
            },
            /**
            * 目的 轉換數字
            * @param  {String}str 當前input值
             * @return {Number} 數字
            */
            fnPackNum = function (str) {
                return ((str || '') === '' ? '0' : str) * 1;
            },
            /**
            * 目的 轉換數字
            * @param  {String}str 當前input值
            */
            fnTotalSum = function () {
                var iTotal = 0;
                $.grep(oCurData.PackingInfo, function (item) {
                    iTotal += item.SubTotal * 1;
                });
                oCurData.Total = iTotal.toFloat().toMoney();
                $('#Total').val(oCurData.Total);
            },
            /**
            * 目的 獲取當前適應費用規則
            * @param  {Number}weight 當前重量
            * @return {Object} 當前規則對象
            */
            fnGetCurRule = function (weight) {
                var oRule = {};
                $.each(oCostRules.CostRules, function (index, item) {
                    var rule_min = fnPackNum(item.Weight_Min),
                        rule_max = fnPackNum(item.Weight_Max);
                    if (weight >= rule_min && weight < rule_max || (weight === 30 && rule_max === 30)) {
                        oRule = item;
                        return false;
                    }
                });
                return oRule;
            },
            /**
            * 目的 設置匯入廠商下拉單
            * @param  {String}id 展覽ID
            * @param  {String}val 客戶值
            * @return {Object} Ajax對象
            */
            fnSetImpCusDrop = function (id, val) {
                if (id) {
                    return g_api.ConnectLite(sProgramId, 'SetImpCusDrop', {
                        Id: id
                    }, function (res) {
                        if (res.RESULT) {
                            var saList = res.DATA.rel;
                            var sOptions = createOptions(saList, 'guid', 'CustomerCName');
                            $('#CustomerId').html(sOptions).on('change', function () {
                                g_api.ConnectLite(sProgramId, 'GetImpCusData', {
                                    Id: this.value
                                }, function (res) {
                                    if (res.RESULT) {
                                        var oRes = res.DATA.rel || {};
                                        $('#MuseumMumber').val(oRes.MuseumMumber || '');
                                        $('#AppointUser').val(oRes.Contactor || '');
                                        $('#AppointTel').val(oRes.Telephone || '');
                                        $('#AppointEmail').val(oRes.Email || '');
                                        $('#Contactor').val(oRes.Contactor || '');
                                        $('#ContactTel').val(oRes.Telephone || '');
                                    }
                                });
                            }).select2();
                            if (val) {
                                $('#CustomerId').val(val).trigger('change');
                            }
                        }
                    });
                }
                else {
                    $('#CustomerId').html(createOptions([]));
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * ToolBar 按鈕事件 function
             * @param {Object}inst 按鈕物件對象
             * @param {Object} e 事件對象
            * @return {Boolean} 停止標識
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }

                        if (sAction === 'Add') {
                            fnAdd('add');
                        }
                        else {
                            fnUpd();
                        }

                        break;
                    case "Toolbar_ReAdd":

                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return false;
                        }
                        fnAdd('readd');

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        pageLeave();

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

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
                    GoTop: true
                });

                oValidator = $("#form_main").validate({
                    onfocusout: false,
                    rules: {
                        AppointEmail: { email: true }
                    },
                    messages: {
                        AppointEmail: { email: i18next.t("message.IncorrectEmail") }// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                    }
                });

                $.whenArray([
                    fnSetEpoDrop({
                        Select: $('#ExhibitionNO'),
                        Select2: true,
                        CallBack: function () {
                        }
                    })
                ])
                    .done(function () {
                        fnGet().done(function () {
                            $("#jsGrid").jsGrid({
                                width: "100%",
                                height: "auto",
                                autoload: true,
                                filtering: false,
                                inserting: true,
                                editing: false,
                                pageLoading: true,
                                confirmDeleting: true,
                                invalidMessage: i18next.t('common.InvalidData'),// ╠common.InvalidData⇒输入的数据无效！╣
                                deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                                pageIndex: 1,
                                pageSize: 10000,
                                fields: [
                                    { name: "Index", title: 'common.RowNumber', width: 50, align: "center" },
                                    {// ╠common.Packaging⇒包裝類型╣
                                        name: "ExpoType", title: 'common.Packaging', width: 100, align: "center",
                                        itemTemplate: function (val, item) {
                                            var oExpoType = {
                                                'zh-TW': { '01': '裸機', '02': '木箱', '03': '散貨', '04': '打板', '05': '其他' },
                                                'en': { '01': 'Unwrapped', '02': 'Wooden Crate', '03': 'Bulk Cargo', '04': 'Pallet', '05': 'Other' }
                                            },
                                                bEn = 'Unwrapped,Wooden Crate,Bulk Cargo,Pallet,Other'.indexOf(item.ExpoTypeText) > -1;
                                            return val ? oExpoType[bEn ? 'en' : 'zh-TW'][val] : '';
                                        }
                                    },
                                    {// ╠common.Dimensions⇒尺寸╣
                                        name: "ExpoLen", title: 'common.Dimensions', width: 100,
                                        itemTemplate: function (val, item) {
                                            return item.ExpoLen.toMoney() + '*' + item.ExpoWidth.toMoney() + '*' + item.ExpoHeight.toMoney();
                                        }
                                    },
                                    {// ╠common.WeightKG⇒重量╣
                                        name: "ExpoWeight", title: 'common.WeightKG', width: 100, align: "right",
                                        itemTemplate: function (val, item) {
                                            return val.toMoney();
                                        }
                                    },
                                    {
                                        name: "ExpoNumber", title: 'common.Number', width: 100, align: "center",
                                        itemTemplate: function (val, item) {
                                            return val.toMoney();
                                        }
                                    },// ╠common.Number⇒件數╣
                                    {// ╠common.ServiceProject⇒服務項目╣
                                        name: "ExpoStack", title: 'common.ServiceProject', width: 200,
                                        itemTemplate: function (val, item) {
                                            var oService = {
                                                'zh-TW': ['堆高機服務', '拆箱(含空箱收送與儲存)', '裝箱', '空箱收送', '空箱儲存', '天'],
                                                'en': ['Forklift', 'Unpacking (including empty crate transport & storage)', 'Packing', 'Empty Crate Transport', 'Empty Crate Storage', 'Days']
                                            },
                                                saText = [],
                                                bEn = 'Unwrapped,Wooden Crate,Bulk Cargo,Pallet,Other'.indexOf(item.ExpoTypeText) > -1;
                                            oService = oService[bEn ? 'en' : 'zh-TW'];
                                            if (item.ExpoStack) {
                                                saText.push(oService[0]);
                                            }
                                            if (item.ExpoSplit) {
                                                saText.push(oService[1]);
                                            }
                                            if (item.ExpoPack) {
                                                saText.push(oService[2]);
                                            }
                                            if (item.ExpoFeed) {
                                                saText.push(oService[3]);
                                            }
                                            if (item.ExpoStorage) {
                                                saText.push(oService[4] + item.ExpoDays + oService[5]);
                                            }
                                            return saText.join('，');
                                        }
                                    },
                                    {// ╠common.Cost⇒費用╣
                                        name: "SubTotal", title: 'common.Cost', width: 100, align: "right",
                                        itemTemplate: function (val, item) {
                                            return fMoney(val || 0, 0, 'NTD');
                                        }
                                    },
                                    {
                                        type: "control",
                                        modeSwitchButton: false,
                                        editButton: false,
                                        width: 100,
                                        headerTemplate: function () {
                                            return i18next.t('common.Action');
                                        },
                                        itemTemplate: function (val, item) {
                                            var saAction = [$('<a/>', {
                                                html: i18next.t('common.Toolbar_Upd'),// ╠common.Toolbar_Upd⇒修改╣
                                                class: 'a-url',
                                                click: function () {
                                                    fnOpenPackingInfo('upd', item);
                                                    return false;
                                                }
                                            }), $('<a/>', {
                                                html: i18next.t('common.Toolbar_Del'),// ╠common.Toolbar_Del⇒刪除╣
                                                class: 'a-url delete',
                                                click: function () {
                                                    // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣
                                                    layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                                                        oCurData.PackingInfo = Jsonremove(oCurData.PackingInfo, 'Guid', item.Guid);
                                                        $.each(oCurData.PackingInfo, function (idx, _item) {
                                                            _item.Index = idx + 1;
                                                        });
                                                        fnTotalSum();
                                                        oGrid.loadData();
                                                        layer.close(index);
                                                    });
                                                }
                                            })];
                                            return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
                                        }
                                    }
                                ],
                                controller: {
                                    loadData: function (args) {
                                        return {
                                            data: oCurData.PackingInfo,
                                            itemsCount: oCurData.PackingInfo.length //data.length
                                        };
                                    },
                                    insertItem: function (args) {
                                        if (oCostRules.CostRules) {
                                            fnOpenPackingInfo('add');
                                        }
                                        else {
                                            showMsg(i18next.t("PackingOrder_Upd.ExhibitionNO_required")); // ╠PackingOrder_Upd.ExhibitionNO_required⇒請選擇展覽名稱╣
                                        }
                                    },
                                    updateItem: function (args) {
                                    },
                                    deleteItem: function (args) {
                                    }
                                },
                                onInit: function (args) {
                                    oGrid = args.grid;
                                }
                            });
                        });
                    });

                $('#ExhibitionNO').on('change', function () {
                    fnSetImpCusDrop(this.value);
                    return g_api.ConnectLite(sProgramId, 'GetExhibitionRules', {
                        Id: this.value
                    }, function (res) {
                        if (res.RESULT) {
                            oCostRules = res.DATA.rel;
                            oCostRules.CostRules = $.parseJSON(oCostRules.CostRules);
                            sExhibitionNO = oCostRules.ExhibitionCode;
                        }
                    });
                });
            };

        init();
    };

require(['base', 'timepicker', 'jsgrid', 'select2', 'formatnumber', 'util'], fnPageInit, 'timepicker');