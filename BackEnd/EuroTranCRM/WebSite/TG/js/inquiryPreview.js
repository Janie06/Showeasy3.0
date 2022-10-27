$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        sExpo = g_db.GetDic('Expo'),
        saOrderInfo = g_db.GetDic('OrderInfo'),
        oContactInfo = g_db.GetDic('ContactInfo'),
        sTotal = g_db.GetDic('Total'),
        /**
        * 目的 獲取展覽報價規則
        */
        fnGetServiceInstruction = function () {
            if (sExpo) {
                return g_api.ConnectLite(Service.apiwebcom, 'GetExhibitionRules', {
                    Id: sExpo
                }, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        $('#ServiceInstruction').html(bEn ? oRes.ServiceInstruction_EN || '' : oRes.ServiceInstruction || '');
                    }
                });
            }
            else {
                $('#CostInstruction').html('');
                return $.Deferred().resolve().promise();
            }
        },
        /*
        * 目的 寄送郵件
        */
        fnAppoint = function (confbtn) {
            var data = oContactInfo;
            data.ExhibitionNO = sExpo;
            data.PackingInfo = JSON.stringify(saOrderInfo || '[]');
            data.Total = sTotal;
            $(confbtn).val(bEn ? 'Appointment processing...' : '預約處理中...').prop('disabled', true);
            return g_api.ConnectLite(Service.apitg, 'Appoint', data, function (res) {
                if (res.RESULT) {
                    if (res.DATA.rel) {
                        $.fancybox.close();
                        g_db.RemoveItem('Expo');
                        g_db.RemoveItem('ExpoName');
                        g_db.RemoveItem('OrderInfo');
                        g_db.RemoveItem('ContactInfo');
                        g_db.RemoveItem('Total');
                        window.location.href = window.location.origin + '/TG/page/inquiryResult' + (bEn ? '_en' : '') + '.html?AppointNO=' + res.DATA.AppointNO;
                    }
                    else {
                        showMsg(bEn ? 'Make an appointment to failure' : '預約失敗', 'error'); // 預約失敗
                        $(confbtn).val(bEn ? 'Determine the transfer' : '確定傳送').prop('disabled', false);
                    }
                }
                else {
                    showMsg(bEn ? 'Make an appointment to failure' : '預約失敗', 'error'); // 預約失敗
                    $(confbtn).val(bEn ? 'Determine the transfer' : '確定傳送').prop('disabled', false);
                }
            }, function () {
                showMsg(bEn ? 'Make an appointment to failure' : '預約失敗', 'error'); // 預約失敗
                $(confbtn).val(bEn ? 'Determine the transfer' : '確定傳送').prop('disabled', false);
            });
        },
        init = function () {
            var myHelpers = {
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

            if (oContactInfo) {
                var sHtml = $('#temp_contact').render(oContactInfo);
                $('#contactInfo').append(sHtml);
            }
            //if (saOrderInfo) {
            //    var sHtml = $('#temp_service').render(saOrderInfo);
            //    $('#Servicebox').append(sHtml);
            //    $('#Total').text(fMoney(sTotal || 0, 0, 'NTD'));
            //    fnGetServiceInstruction();
            //    $(".confappoint").click(function () {
            //        $.fancybox.open($('.conf-content'), { zoomOpacity: false });
            //    });
            //    $("#comfirmBtn").click(function () {
            //        if (!saOrderInfo) {
            //            alert(bEn ? 'The webpage data has been invalid, please fill in the data again!!' : '網頁數據已失效，請重新填寫資料！！');
            //        }
            //        else {
            //            fnAppoint(this);
            //        }
            //    });
            //}
            //else {
            //    window.location.href = window.location.origin + '/TG/page/inquiry' + (bEn ? '_en' : '') + '.html';
            //}
        };

    init();
});