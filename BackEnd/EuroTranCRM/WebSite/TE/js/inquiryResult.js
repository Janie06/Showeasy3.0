$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        sAppointNO = getUrlParam('AppointNO'),
        sExpo = g_db.GetDic('Expo'),
        saOrderInfo = g_db.GetDic('OrderInfo'),
        oContactInfo = g_db.GetDic('ContactInfo'),
        sTotal = g_db.GetDic('Total'),
        /*
        * 目的 獲取預約明細
        */
        fnGetAppointInfo = function (confbtn) {
            return g_api.ConnectLite(Service.apite, 'GetAppointInfo', {
                AppointNO: sAppointNO
            }, function (res) {
                if (res.RESULT) {
                    var oAppointInfo = res.DATA.rel,
                        oRuleInfo = res.DATA.rule;
                    if (oAppointInfo) {
                        oAppointInfo.ExpoName = oRuleInfo.ExpoName;
						
						if(oAppointInfo.PaymentWay === "1"){
							oAppointInfo.PaymentWayDesc = "匯款";
						} else if (oAppointInfo.PaymentWay === "2"){
							oAppointInfo.PaymentWayDesc = "現場付現";
						}
						
                        var sHtml = $('#temp_contact').render(oAppointInfo);
                        $('#contactInfo').append(sHtml);
                        $('#AppointUser').text(oAppointInfo.AppointUser);
                    }
                    if (oAppointInfo.PackingInfo) {
                        oAppointInfo.PackingInfo = JSON.parse(oAppointInfo.PackingInfo);
                        var sHtml = $('#temp_service').render(oAppointInfo.PackingInfo);
                        $('#Servicebox').append(sHtml);
                        $('#Total').text(fMoney(oAppointInfo.Total || 0, 0, 'NTD'));
                        $('#ServiceInstruction').html(bEn ? oRuleInfo.Info.ServiceInstruction_EN || '' : oRuleInfo.Info.ServiceInstruction || '');
                    }
                }
            });
        },
        init = function () {
            var myHelpers = {
                setDate: function (val) {
                    return new Date(val).formate('yyyy:MM:dd HH:mm');
                },
                setMoney: function (val, flag) {
                    return (flag ? 'NT$' : '') + fMoney(val || 0, 0, 'NTD');
                },
                setExpoType: function (val) {
                    var oExpoType = {
                        'zh-TW': { '01': '裸機', '02': '木箱', '03': '散貨', '04': '打板', '05': '其他' },
                        'en': { '01': 'Unwrapped', '02': 'Wooden Crate', '03': 'Bulk Cargo', '04': 'Pallet', '05': 'Other' }
                    };
                    return val ? oExpoType[sLang][val] : '';
                },
                setService: function (ExpoStack, ExpoSplit, ExpoPack, ExpoFeed, ExpoStorage, ExpoDays) {
                    var oService = {
                        'zh-TW': ['堆高機服務', '拆箱(含空箱收送與儲存)', '裝箱', '空箱收送', '空箱儲存', '天'],
                        'en': ['Forklift', 'Unpacking (including empty crate transport & storage)', 'Packing', 'Empty Crate Transport', 'Empty Crate Storage', 'Days']
                    },
                        saText = [];
                    if (ExpoStack) {
                        saText.push(oService[sLang][0]);
                    }
                    if (ExpoSplit) {
                        saText.push(oService[sLang][1]);
                    }
                    if (ExpoPack) {
                        saText.push(oService[sLang][2]);
                    }
                    if (ExpoFeed) {
                        saText.push(oService[sLang][3]);
                    }
                    if (ExpoStorage) {
                        saText.push(oService[sLang][4] + ExpoDays + oService[sLang][5]);
                    }
                    return saText.join('，');
                }
            };
            $.views.helpers(myHelpers);

            if (sAppointNO) {
                fnGetAppointInfo();
                $('.print').click(function () {
                    $("#Result").jqprint({ operaSupport: false });
                });
            }
        };

    init();
});