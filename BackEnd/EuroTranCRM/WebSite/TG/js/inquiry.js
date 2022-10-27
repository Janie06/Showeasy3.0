$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        iPageIndex = 1,
        iPageCount = 100000,
        oCostRules = {},
        sExpo = g_db.GetDic('Expo') || getUrlParam('SN'),
        iTotal = g_db.GetDic('Total') || 0,
        saOrderInfo = g_db.GetDic('OrderInfo'),
        eForm = $('#form_inquiry'),
        sState = getUrlParam('State'),
        sYear = getUrlParam('Year'),
        sMonth = getUrlParam('Month'),
        FeeCalculateRules = {},
        /**
        * 目的 設定展覽地區下拉單
        */
        fnGetArguments = function () {
            g_api.ConnectLite(Service.apiappcom, ComFn.GetArguments, {
                ArgClassID: 'Area',
                OrderBy: 'id',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    $('#area').html(createOptions(saRes, 'id', bEn ? 'text_en' : 'text', true));
                    if (sState) {
                        $('#area').val(sState);
                    }
                    else {
                        $('#area').val('TWN');
                    }

                    $('#area').select2({
                        placeholder: bEn ? 'Select Area' : '請選擇地區'
                    });
                    if (sYear) {
                        $('#year').val(sYear);
                    }
                    if (sMonth) {
                        var iMonth = sMonth * 1,
                            sVal = '';
                        switch (iMonth) {
                            case 1:
                            case 2:
                            case 3:
                                sVal = '01-03';
                                break;
                            case 4:
                            case 5:
                            case 6:
                                sVal = '04-06';
                                break;
                            case 7:
                            case 8:
                            case 9:
                                sVal = '07-09';
                                break;
                            case 10:
                            case 11:
                            case 12:
                                sVal = '10-12';
                                break;
                        }
                        $('#month').val(sVal);
                    }
                }
            });
        },
        /**
        * 目的 設置日期下拉單
        */
        fnSetDate = function () {
            var saYears = [],
                year_cur = new Date().getFullYear();
            saYears.push({ id: year_cur, text: year_cur });
            year_cur++;
            saYears.push({ id: year_cur, text: year_cur });
            $('#year').html(createOptions(saYears, 'id', 'text')).find('option:first').text(bEn ? 'Year' : '選擇年份');
        },
        /**
        * 目的 設定展覽資訊下拉單
        * @return {Object} Ajax 物件
        */
        fnGetExhibitionsTop = function () {
            var sState = $('#area').val(),
                sYear = $('#year').val(),
                sMonth = $('#month').val(),
                sDateStart = '',
                sDateEnd = '';
            if (sYear) {
                sDateStart = sYear.toString();
                sDateEnd = sYear.toString();
                if (sMonth) {
                    var saMonth = sMonth.split('-'),
                        sMonth_s = saMonth[0],
                        sMonth_e = saMonth[1];
                    sDateStart += '/' + sMonth_s + '/01';
                    sDateEnd += '/' + sMonth_e + '/' + ('03,12'.indexOf(sMonth_e) > -1 ? '31' : '30');
                }
                else {
                    sDateStart += '/01/01';
                    sDateEnd += '/12/31';
                }
            }
            else {
                sDateStart = newDate(null, true);
            }

            return g_api.ConnectLite(Service.apiwebcom, ComFn.GetExhibitionPage, {
                pageIndex: iPageIndex,
                pageSize: iPageCount,
                IsShowWebSim: "Y",
                KeyWords: '',
                Area: sState,
                DateStart: sDateStart,
                DateEnd: sDateEnd
            }, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    $('#expo').html(createOptions(oRes.DataList, 'SN', bEn ? 'Exhibitioname_EN' : 'Exhibitioname_TW'));
                    if (sExpo) {
                        $('#expo').val(sExpo);
                    }
                    $('#expo').select2({
                        placeholder: bEn ? 'Select Exhibition' : "請選擇展覽"
                    });
                }
            });
        },
        /**
        * 目的 獲取展覽報價規則
        * @return {Object} Ajax 物件
        */
        fnGetExhibitionRules = function () {
            var sId = $('#expo').val();
            if (sId) {
                return g_api.ConnectLite(Service.apiwebcom, 'GetExhibitionRules', {
                    Id: sId
                }, function (res) {
                    if (res.RESULT) {
                        let MinCBMText = {
                            "zh-TW": { "A": "(以全部貨量計min)", "S": "(以單件計min)" },
                            "zh": { "A": "(以全部貨量計min)", "S": "(以單件計min)" },
                            "en": { "A": "(per shipment)", "S": "(per piece)" },
                        };

                        oCostRules = res.DATA.rel;
                        oCostRules.CostRules = $.parseJSON(oCostRules.CostRules);
                        oCostRules.HasFeedingRequiredMinCBM = !oCostRules.FeedingRequiredMinCBM ? false : true;
                        oCostRules.FeedingMinCBMModeText = MinCBMText[sLang][oCostRules.FeedingMinMode] || "";
                        //if (oCostRules.FeedingMinMode) {
                        //    let UseMode = oCostRules.PackingMinMode;
                        //    oCostRules.FeedingMinCBMModeText = MinCBMText[g_db.GetItem("lang")][oCostRules.FeedingMinMode];
                        //}
                        oCostRules.HasPackingRequiredMinCBM = !oCostRules.PackingRequiredMinCBM ? false : true;
                        oCostRules.PackingMinCBMModeText = MinCBMText[sLang][oCostRules.PackingMinMode] || "";
                        //if (oCostRules.PackingMinMode) {
                        //    let UseMode = oCostRules.PackingMinMode;
                        //    oCostRules.PackingMinCBMModeText = MinCBMText[g_db.GetItem("lang")][UseMode];
                        //}
                        FeeCalculateRules = {
                            "Packing": {
                                "MinCBMMode": oCostRules.PackingMinMode,
                                "MinCBMAmount": oCostRules.PackingRequiredMinCBM,
                                "Price": oCostRules.PackingPrice,
                            },
                            "Feeding": {
                                "MinCBMMode": oCostRules.FeedingMinMode,
                                "MinCBMAmount": oCostRules.FeedingRequiredMinCBM,
                                "Price": oCostRules.FeedingPrice,
                            },
                        };
                        var sHtml = $('#temp_costdoc').render(oCostRules);
                        $('#CostInstruction').html(sHtml);
                        $('#inquiryNo dd:not(:last)').remove();//清空現有預估報價
                        $('.download li a').on('click', function () {
                            var sName = $(this).attr('filename'),
                                sPath = $(this).attr('filepath');
                            DownLoadFile(sPath, sName);
                        });
                    }
                });
            }
            else {
                $('#CostInstruction').html('');
                return $.Deferred().resolve().promise();
            }
        },

        /**
         * 目的 取得有效CBM
         */
        fnGetApproveCBMFee = function (Rule, Type, OriginalCBM, OriginalNum) {
            var TotalRoundedCBMs = Round((OriginalCBM * OriginalNum));
            switch (Rule.MinCBMMode) {
                case "A":
                    {
                        let TotalRows = $('#inquiryNo dd').not(':last').length;
                        var AllDatas = getFormSerialize(eForm);
                        var AllApproveCBMs = 0;
                        for (let i = 0; i < TotalRows; i++) {
                            var PackingInfo = AllDatas["PackingInfo" + i.toString()];
                            var ApproveCBM = PackingInfo[Type]
                                ? fnGetCMB(PackingInfo["ExpoLen"], PackingInfo["ExpoWidth"], PackingInfo["ExpoHeight"]) * fnPackNum(PackingInfo["ExpoNumber"])
                                : 0;
                            AllApproveCBMs += ApproveCBM;
                        }
                        AllApproveCBMs = AllApproveCBMs.toFloat(2);
                        var ApproveCBM = Rule.MinCBMAmount && Rule.MinCBMAmount > AllApproveCBMs
                            ? Rule.MinCBMAmount * (TotalRoundedCBMs / AllApproveCBMs)
                            : TotalRoundedCBMs;
                        //修正其他每個資料
                        return ApproveCBM * Rule.Price;
                    }
                case "S":
                    {
                        let ApproveCBM = Rule.MinCBMAmount && Rule.MinCBMAmount > OriginalCBM
                            ? Number(Rule.MinCBMAmount) * OriginalNum
                            : TotalRoundedCBMs;
                        return ApproveCBM * Rule.Price;
                    }
                default:
                    return 0;
            }

            function Round(data) {
                return (data).toFloat(2).toString().toMoney();
            }

        },
        /**
         * 目的 以全部貨量計min
         */
        fnMinFeeAllQty = function () {
            //不足Min CBM
            //超過1CBM
        },


        /**
        * 目的 預估費用明細區塊
        */
        fnPackInquiry = function () {
            var iIndex = $('#inquiryNo dd').length - 1,
                sHtml = $('#temp_costinfo').render({ Index: iIndex });
            $('#inquiryNo dd:last').before(sHtml);
            fnBindEvent();
        },
        /**
        * 目的 小計計算
        * @param  {HTMLelement}input 當前區塊資料
        */
        fnSumSub = function (input) {
            //if (input.className.indexOf('inputnumber') > -1) {
            //    input.value = input.value.toMoney();
            //}
            var sBoxId = $(input).parents('dd').attr('id'),
                data = getFormSerialize(eForm),
                curData = data[sBoxId];
            curData = fnPackItem(curData);
            input.AnyError = CheckLimited(curData);
            $('#' + sBoxId).find('.subtext').text(curData.SubText.join(' / '));
            $('#' + sBoxId).find('.subtotal').text('NT$' + fMoney(curData.SubTotal, 0, 'NTD'));
            $('[name="' + sBoxId + '[SubTotal]"]').val(curData.SubTotal);

            let TotalRows = $('#inquiryNo dd').not(':last').length;
            let SubTotal = 0;
            for (let i = 0; i < TotalRows; i++) {
                let CurrentPacking = "PackingInfo" + i.toString();
                if (sBoxId != CurrentPacking) {
                    fnPackItem(data[CurrentPacking]);
                    $('#' + CurrentPacking + " .subtotal").text(SetMoney(data[CurrentPacking].SubTotal, true));
                }
                SubTotal += data[CurrentPacking].SubTotal;
            }
            $('.totaltext').text(SetMoney(SubTotal, true));

            function SetMoney(val, flag) {
                return (flag ? 'NT$' : '') + fMoney(val || 0, 0, 'NTD');
            }
        },
        fnGetCMB = function (ExpoLen, ExpoWidth, ExpoHeight) {
            return fnPackNum(ExpoLen) * fnPackNum(ExpoWidth) * fnPackNum(ExpoHeight) / 1000000;//CBM
        },

        CheckLimited = function (curdata) {
            let AnyError = false;
            if (curdata['ExpoWeight'] > -1) {
                var MaxWeight = oCostRules.CostRules
                    .map(c => parseFloat(c.Weight_Max) || 0)
                    .reduce((x, y) => {
                        return Math.max(x, y);
                    }) * 1000;
                if (curdata['ExpoWeight'] * 1 > MaxWeight) {
                    AnyError = true;
                }
            }

            if (curdata['ExpoHeight'] * 1 >= 300) {
                AnyError = true;
            }

            if (curdata['ExpoLen'] * 1 > 999) {
                AnyError = true;
            }

            if (curdata['ExpoWidth'] * 1 > 999) {
                AnyError = true;
            }

            if (curdata['ExpoNumber'] * 1 > 99) {
                AnyError = true;
            }
            return AnyError;
        },
        /**
        * 目的 處理當前費用項目
        * @param  {Object} curdata 當前input值
        * @return {Object} 當前服務項目計算對象
        */
        fnPackItem = function (curdata) {
            curdata.ExpoLen = curdata.ExpoLen === '' ? '0' : curdata.ExpoLen;
            curdata.ExpoWidth = curdata.ExpoWidth === '' ? '0' : curdata.ExpoWidth;
            curdata.ExpoHeight = curdata.ExpoHeight === '' ? '0' : curdata.ExpoHeight;
            curdata.TotalCBM = fnGetCMB(curdata.ExpoLen, curdata.ExpoWidth, curdata.ExpoHeight);
            //fnPackNum(curdata.ExpoLen) * fnPackNum(curdata.ExpoWidth) * fnPackNum(curdata.ExpoHeight) / 1000000;//CBM
            curdata.ExpoWeight = fnPackNum(curdata.ExpoWeight);//重量
            curdata.ExpoWeightTon = curdata.ExpoWeight / 1000;//噸
            curdata.ExpoNumber = fnPackNum(curdata.ExpoNumber);//件數
            curdata.SubTotal = 0;//小計
            //curdata.ExpoDays = fnPackNum(curdata.ExpoDays);//天數
            curdata.SubText = [];//
            curdata.ExpoStack = curdata.ExpoStack || false; //推高機項目
            curdata.ExpoSplit = curdata.ExpoSplit || false;//拆箱項目
            curdata.ExpoPack = curdata.ExpoPack || false;//裝箱項目
            curdata.ExpoFeed = curdata.ExpoFeed || false;//空箱收送
            curdata.ExpoStorage = curdata.ExpoStorage || false;//[20190613 已移除]

            var oExpoType = {
                '01': bEn ? 'Unwrapped' : '裸機',
                '02': bEn ? 'Wooden Crate' : '木箱',
                '03': bEn ? 'Bulk Cargo' : '散貨',
                '04': bEn ? 'Pallet' : '打板',
                '05': bEn ? 'Other' : '其他'
            };
            curdata.ExpoTypeText = oExpoType[curdata.ExpoType];//

            if (curdata.ExpoNumber > 0) {//件數
                curdata.SubText.push(curdata.ExpoNumber.toString().toMoney() + (bEn ? 'package' : '件'));
                if (curdata.TotalCBM > 0) {//CBM
                    var iTotalCBM = curdata.TotalCBM * curdata.ExpoNumber;
                    curdata.SubText.push(iTotalCBM.toFloat(2).toString().toMoney() + 'CBM');
                }
                if (curdata.ExpoWeight > 0) {//KG
                    var iExpoWeight = curdata.ExpoWeight * curdata.ExpoNumber;
                    if (iExpoWeight < 1) {
                        iExpoWeight = 1;
                    }
                    curdata.SubText.push(iExpoWeight.toFloat(2).toString().toMoney() + 'KG');
                }
            }

            let HasCBMAndExpoNumber = curdata.TotalCBM > 0 && curdata.ExpoNumber > 0;
            let TotalCBMs = curdata.TotalCBM * curdata.ExpoNumber;
            if (curdata.ExpoSplit && HasCBMAndExpoNumber) {//拆箱
                //Packing Feeding
                let ApproveCBMFee = fnGetApproveCBMFee(FeeCalculateRules.Packing, "ExpoSplit", curdata.TotalCBM, curdata.ExpoNumber);
                curdata.SubTotal += ApproveCBMFee;
            }
            if (curdata.ExpoPack && HasCBMAndExpoNumber) {//裝箱
                let ApproveCBMFee = fnGetApproveCBMFee(FeeCalculateRules.Packing, "ExpoPack", curdata.TotalCBM, curdata.ExpoNumber);
                curdata.SubTotal += ApproveCBMFee;
            }
            if (curdata.ExpoFeed && HasCBMAndExpoNumber) {// 空箱收送與儲存(展覽期間)，等同裝箱計價規則
                let ApproveCBMFee = fnGetApproveCBMFee(FeeCalculateRules.Feeding, "ExpoFeed", curdata.TotalCBM, curdata.ExpoNumber);
                curdata.SubTotal += ApproveCBMFee;
            }

            if (curdata.ExpoStack && curdata.ExpoWeightTon > 0 && curdata.ExpoNumber > 0) {//堆高
                /*** 2019/01/22 Yang Leo 台北駒驛「線上預約」堆高機價格計算方式調整「材積重」及「重量」擇一個重的去計價 Star ***/
                //尺寸：200x200x200cm(換算材積重 = 1,336KGS)
                //公式 = 長  寬  高 / 1000000 * 167
                //重量：900KGS
                //目前系統公式直接以重量900KGS計價 -> $920 / 件
                //明年改為以材積重1,336KGS去計價 -> $1,700 / 件
                if (curdata.TotalCBM * 167 > curdata.ExpoWeight) { //公式=長*寬*高/1000000*167
                    curdata.ExpoWeightTon = curdata.TotalCBM * 167 / 1000;
                }
                /*** 2019/01/22 Yang Leo 台北駒驛「線上預約」堆高機價格計算方式調整「材積重」及「重量」擇一個重的去計價 End ***/
                var rule = fnGetCurRule(curdata.ExpoWeightTon);
                switch (rule.PricingMode) {
                    case 'T':
                        {
                            curdata.SubTotal += curdata.ExpoWeightTon * curdata.ExpoNumber * fnPackNum(rule.Price);
                        }
                        break;
                    case 'N':
                        {
                            curdata.SubTotal += curdata.ExpoNumber * fnPackNum(rule.Price);
                        }
                        break;
                    default:
                        {
                            //debugger;
                            //showMsg(bEn ? 'Special specifications, please call +886-2-2785-7900.' : '貨物內有特殊規格，敬請來電詢價02-2785-7900');
                        }
                        break;
                }
            }
            return curdata;
        },
        /**
        * 目的 綁定事件
        */
        fnBindEvent = function () {
            $('#inquiryNo dd').find('.delete').off('click').on('click', function () {
                $(this).parent().remove();
                $('#inquiryNo dd').not(':last').each(function (index) {
                    var that = this;
                    if (that.id) {
                        var sNewId = 'PackingInfo' + index;
                        $(this).find('[name]').each(function () {
                            var name = $(this).attr('name');
                            if (name.indexOf(that.id) > -1) {
                                $(this).attr('name', name.replace(that.id, sNewId))
                            }
                        });
                        that.id = sNewId;
                    }

                });
                var firstInput = $('#inquiryNo').find(':input[type="text"]:first');
                if (firstInput.length > 0) {
                    firstInput.change();
                }
                else {
                    $('[name="Total"]').val('0');
                    $('.totaltext').text('0');
                }
            });


            $('#inquiryNo').find(':input').off('change keypress keyup').on('change keypress keyup', function (e) {
                var _input = this;
                if (!$('#expo').val()) {
                    showMsg(bEn ? 'Please select exhibition' : '請選擇展覽'); // 請選擇展覽
                    return false;
                }
                if (_input.type === 'select-one') {
                    return false;
                }
                if (_input.type === 'checkbox') {
                    fnDisabledPack(_input);//拆箱和裝箱服務不能再單獨選擇收送或儲存
                }
                if (e.type === 'keypress') {
                    if (!String.fromCharCode(e.keyCode).match(/[0-9\.]/)) {//防止輸入“e”
                        return false;
                    }
                }
                else {
                    fnSumSub(_input);
                    if (_input.AnyError) {
                        showMsg(bEn ? 'Special specifications, please call +886-2-2785-7900.' : '貨物內有特殊規格，敬請來電詢價02-2785-7900');
                        $('#goappoint').addClass("isDisabled");
                    }
                    else {
                        $('#goappoint').removeClass("isDisabled");

                    }
                }
            });
            fnsubtotalval();
            $('#inquiryNo dd').each(function () {
                var boxchecked = $(this).find(':input[type="checkbox"]:not(:first):checked');
                if (boxchecked.length > 0) {
                    fnDisabledPack(boxchecked[0]);
                }
            });
            moneyInput($('.inputnumber'), 0);
        },
        /**
        * 目的 禁用服務
        * @param  {HTMLElement} el 當前input值
        */
        fnsubtotalval = function () {
            var iTotal = 0;
            $('.subtotalval').each(function () {
                iTotal += this.value * 1;
            });

            $('[name="Total"]').val(iTotal);

            iTotal = fMoney(iTotal, 0, 'NTD');
            $('.totaltext').text(iTotal);
        },
        /**
        * 目的 禁用服務
        * @param  {HTMLElement} el 當前input值
        */
        fnDisabledPack = function (el) {
            //var ul = $(el).parents('ul'),
            //    bCheck = false;
            //if (el.id.indexOf('ExpoSplit') > -1 || el.id.indexOf('ExpoPack') > -1) {
            //    ul.find('.packing').each(function () {
            //        if (this.checked) {
            //            bCheck = true;
            //            return false;
            //        }
            //    });
            //    if (bCheck) {
            //        ul.find('.single-packing,.textStyle').prop('disabled', true);
            //        ul.find('.textStyle').val('');
            //    }
            //    else {
            //        ul.find('.single-packing,.textStyle').prop('disabled', false);
            //    }
            //}
            //else if (el.id.indexOf('ExpoFeed') > -1 || el.id.indexOf('ExpoStorage') > -1) {
            //    ul.find('.single-packing').each(function () {
            //        if (this.checked) {
            //            bCheck = true;
            //            return false;
            //        }
            //    });
            //    if (bCheck) {
            //        ul.find('.packing').prop('disabled', true);
            //        /*** 2019/01/22 Yang Leo 點選「空箱收送」時，已包含「空箱存放」，直接鎖住「空箱存放」欄位 和 點選「空箱存放」時，直接鎖住「空箱收送」欄位 Star ***/

            //        if (oCostRules.IsMerge === 'Y') {
            //            if (el.id.indexOf('ExpoFeed') > -1) {
            //                ul.find("[id^='ExpoStorage'],.textStyle").prop('disabled', true);
            //                ul.find(".textStyle").val('');
            //            } else {
            //                ul.find("[id^='ExpoFeed']").prop('disabled', true);
            //            }
            //        }
            //        /*** 2019/01/22 Yang Leo 點選「空箱收送」時，已包含「空箱存放」，直接鎖住「空箱存放」欄位 和 點選「空箱存放」時，直接鎖住「空箱收送」欄位 End ***/
            //    }
            //    else {
            //        ul.find('.packing').prop('disabled', false);
            //        /*** 2019/01/22 Yang Leo 取消點選「空箱收送」時，已包含「空箱存放」，直接解鎖「空箱存放」欄位 和 取消點選「空箱存放」時，直接解鎖「空箱收送」欄位 Star ***/
            //        if (oCostRules.IsMerge === 'Y') {
            //            if (el.id.indexOf('ExpoFeed') > -1) {
            //                ul.find("[id^='ExpoStorage'],.textStyle").prop('disabled', false);
            //            } else {
            //                ul.find("[id^='ExpoFeed']").prop('disabled', false);
            //            }
            //        }
            //        /*** 2019/01/22 Yang Leo 取消點選「空箱收送」時，已包含「空箱存放」，直接解鎖「空箱存放」欄位 和 取消點選「空箱存放」時，直接解鎖「空箱收送」欄位 End ***/
            //    }
            //}
        },
        /**
        * 目的 轉換數字
        * @param  {String}str 當前input值
        * @return {Number} 轉化後數字
        */
        fnPackNum = function (str) {
            return ((str || '') === '' ? '0' : str) * 1;
        },
        /**
        * 目的 獲取當前適應費用規則
        * @param  {Number}weight 當前重量
        * @return {Object} 當前要抓取的規則對象
        */
        fnGetCurRule = function (weight) {
            var oRule = {};
            $.each(oCostRules.CostRules, function (index, item) {
                let rule_min = fnPackNum(item.Weight_Min);
                let rule_max = fnPackNum(item.Weight_Max);
                let RoundedWeight = Math.ceil(weight * 10) / 10;
                if (RoundedWeight >= rule_min && RoundedWeight <= rule_max || (weight === 30 && rule_max === 30)) {
                    oRule = item;
                    return false;
                }
            });
            return oRule;
        },
        /**
        * 目的 Go預約
        * @return {Boolean} 停止符
        */
        fnGoAppoint = function () {
            $('#inquiryNo dd').each(function () {
                var boxchecked = $(this).find(':input[type="checkbox"]:not(:first):checked');
                if (boxchecked.length > 0) {
                    fnDisabledPack(boxchecked[0]);
                }
            });
            var sExpo = $('#expo').val(),
                data = getFormSerialize(eForm),
                oOrderInfo = {},
                sError = '';
            oOrderInfo.ExhibitionNO = sExpo;
            oOrderInfo.PackingInfo = [];
            //oOrderInfo.Total = data.Total;

            if (!oOrderInfo.ExhibitionNO) {
                sError += bEn ? 'Please select exhibition<br/>' : '請選擇展覽<br/>'; // 請選擇展覽
            }
            oOrderInfo.Total = 0
            $('#inquiryNo dd:not(:last)').each(function (index) {
                
                var item = data['PackingInfo' + index],
                    iNum = index + 1;
                if (item) {
                    item = fnPackItem(item);
                    oOrderInfo.Total += item.SubTotal;
                    item.Index = index;
                    oOrderInfo.PackingInfo.push(item);

                    if (item.ExpoStack || item.ExpoSplit || item.ExpoPack || item.ExpoFeed || item.ExpoStorage) {
                        if (item.ExpoNumber === 0) {
                            sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Number of pieces] cannot be empty<br/>' : '：[件數]不能為空<br/>'); // 件數不能為空
                        }
                        if (item.ExpoStack) {
                            if (item.ExpoWeightTon === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Forklift] service weight cannot be empty<br/>' : '：[堆高機]服務重量不能為空<br/>'); // [堆高機服務]重量不能為空
                            }
                            if (item.TotalCBM === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Forklift] service dimensions cannot be empty<br/>' : '：[堆高機]服務尺寸不能為空<br/>'); // [堆高機]服務尺寸不能為空
                            }
                        }
                        if (item.ExpoSplit) {
                            if (item.TotalCBM === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Packing] service dimensions cannot be empty<br/>' : '：[裝箱]服務尺寸不能為空<br/>'); // [裝箱]服務尺寸不能為空
                            }
                        }
                        if (item.ExpoPack) {
                            if (item.TotalCBM === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Unpacking (including empty crate transport & storage)] service dimensions shall not be empty<br/>' : '：[拆箱(含空箱收送與儲存)]服務尺寸不能為空<br/>'); // [拆箱(含空箱收送與儲存)]服務尺寸不能為空
                            }
                        }
                        if (item.ExpoFeed) {
                            if (item.TotalCBM === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Empty Crate Transport] service dimensions cannot be empty<br/>' : '：[空箱收送]服務尺寸不能為空<br/>'); // 空箱收送服務尺寸不能為空
                            }
                        }
                        if (item.ExpoStorage) {
                            if (item.TotalCBM === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[empty storage/storage] service dimensions must not be empty<br/>' : '[空箱存放 / 存放]服務尺寸不能為空<br/>'); //[空箱存放 / 存放]服務尺寸不能為空
                            }
                            if (item.ExpoDays === 0) {
                                sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：[Empty Crate Storage] service days cannot be empty<br/>' : '：[空箱存放 / 存放]服務天數不能為空<br/>'); //[空箱存放 / 存放]服務天數不能為空
                            }
                        }
                    }
                    else {
                        sError += (bEn ? 'Project' : '項目') + iNum + (bEn ? '：Please choose at least one service<br/>' : '：請至少選擇一種服務<br/>'); // 請至少選擇一種服務
                    }
                }
            });

            if (oOrderInfo.PackingInfo.length === 0) {
                sError += bEn ? 'Please fill in the exhibition estimated quotation information<br/>' : '請填寫展覽預估報價資訊<br/>'; // 請填寫展覽預估報價資訊
            }
            if (sError) {
                showMsg(sError);
                return false;
            }
            else {
                g_db.SetDic('Expo', sExpo);
                g_db.SetDic('ExpoName', $('#expo option:selected').text());
                g_db.SetDic('OrderInfo', oOrderInfo.PackingInfo);
                g_db.SetDic('Total', oOrderInfo.Total);
                window.location.href = window.location.origin + '/TG/page/inquiryForm' + (bEn ? '_en' : '') + '.html';
            }
        },
        init = function () {
            var myHelpers = {
                getFileName: function (val) {
                    return val ? val.split('.')[0] : '';
                },
                setMoney: function (val, flag) {
                    return (flag ? 'NT$' : '') + fMoney(val || 0, 0, 'NTD');
                },
                setCheck: function (val) {
                    return val ? 'checked="checked"' : '';
                },
                setSubtext: function (val) {
                    return val ? val.join(' / ') : '';
                },
                setWeight: function (val) {
                    return (val || 0) * 1000;
                }
            };
            $.views.helpers(myHelpers);

            $.whenArray([fnSetDate(), fnGetArguments(), fnGetExhibitionsTop()]).done(function () {
                fnGetExhibitionRules().done(function () {
                    if (saOrderInfo) {
                        if (typeof saOrderInfo === 'string') {
                            saOrderInfo = JSON.parse(saOrderInfo);
                        }
                        var sHtml = $('#temp_costinfo').render(saOrderInfo);
                        $('#inquiryNo dd:last').before(sHtml);
                        $('[name="Total"]').val(iTotal);
                        $('.totaltext').text(fMoney(iTotal, 0, 'NTD'));
                        $.each(saOrderInfo, function (index, item) {
                            $('[name="PackingInfo' + index + '[ExpoType]"]').val(item.ExpoType);
                        });
                        fnBindEvent();
                    }
                    else {
                        fnPackInquiry();//在新預設一項
                    }
                });
            });
            $('#inquiryIntro').fancybox({
                src: 'inquiryIntro' + (bEn ? '_en' : '') + '.html',
                type: 'iframe',
                smallBtn: true,
                iframe: {
                    css: {
                        width: '500px'
                    }
                }
            });

            $('.expo-search').on('change', function () {
                $('.totaltext').text("0");
                fnGetExhibitionsTop().done(function () {
                    fnGetExhibitionRules();
                });
            });
            $('.add').on('click', function () {
                fnPackInquiry();
            });
            $('#goappoint').on('click', function () {
                fnGoAppoint();
            });
            $('#expo').on('change', function () {
                fnGetExhibitionRules().done(function () {
                    fnPackInquiry();//在新預設一項
                    $('#inquiryNo').find(':input[type="text"]:first').change();
                });
            });
        };

    init();
});