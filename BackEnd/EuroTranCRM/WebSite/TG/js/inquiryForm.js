$(function () {
    'use strict';

    var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        bEn = sLang === 'en',
        eForm = $("#form_inquiry"),
        saOrderInfo = g_db.GetDic('OrderInfo'),
        oContactInfo = g_db.GetDic('ContactInfo'),
        sTotal = g_db.GetDic('Total'),
        sExpoName = g_db.GetDic('ExpoName'),
        /*
        * 目的 表單驗證初始化
        */
        fnSetForm_Rule = function () {
            eForm.validate({
                errorPlacement: function (error, element) {
                    error.css({
                        "border": "none",
                        "color": "red"
                    });
                    var wrapper = $("<p></p>");
                    error.appendTo(wrapper);
                    $(element).parent().after(wrapper);
                },
                highlight: function (element, errorClass) {
                    $(element).css({
                        "border": "1px solid red"
                    });
                },
                unhighlight: function (element, errorClass) {
                    $(element).css({
                        "border": ""
                    });
                },
                rules: {
                },
                messages: {
                    CompName: {
                        required: bEn ? 'Please enter the company name' : '請輸入公司名稱'
                    },
                    MuseumMumber: {
                        required: bEn ? 'Please enter the booth number' : '請輸入攤位號碼'
                    },
                    AppointUser: {
                        required: bEn ? 'Please enter the name of the reservation holder' : '請輸入預約者姓名'
                    },
                    AppointTel: {
                        required: bEn ? 'Please enter the reservation number' : '請輸入預約者電話'
                    },
                    AppointEmail: {
                        email: bEn ? "The subscriber's Email format is wrong" : '請輸入公司名稱',
                        required: bEn ? 'Please enter the subscriber Email' : '請輸入公司名稱'
                    },
                    Contactor: {
                        required: bEn ? 'Please enter the field contact person' : '請輸入現場聯絡人'
                    },
                    ContactTel: {
                        required: bEn ? 'Please enter the field contact phone' : '請輸入現場聯絡手機'
                    },
                    ApproachTime: {
                        required: bEn ? 'Please choose the entry time' : '請選擇進場時間'
                    },
                    ExitTime: {
                        required: bEn ? 'Please choose the exit time' : '請選擇退場時間'
                    },
                    validcode: {
                        required: bEn ? 'Please enter the verification code' : '請輸入驗證碼'
                    }
                }
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
                        'zh-TW': ['堆高機服務', '拆箱', '裝箱', '空箱收送與儲存(展覽期間)'],
                        'en': ['Forklift', 'Unpacking', 'Packing', 'Empty Crate Transport And StorageEmpty Crate Transport and Storage During the Exhibition']
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
                    return saText.join('，');
                }
            };
            $.views.helpers(myHelpers);

            fnSetForm_Rule();

            if (bEn) {
                $.datepicker.setDefaults($.datepicker.regional[""]);
            }
            $(".datepicker").datepicker({
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd'
            });

            //$('#change_validcode,.change_validcode').on('click', function () {
            //    var url = $('#imgvalidcode').data('url') || $('#imgvalidcode').attr('src');
            //    $('#imgvalidcode').attr('src', url + '&' + Math.random()).data('url', url);
            //});

            if (oContactInfo) {
                setFormVal($('#form_inquiry'), oContactInfo);
            }
            //if (saOrderInfo) {
            //    var sHtml = $('#temp_service').render(saOrderInfo);
            //    $('#Servicebox').append(sHtml);
            //    $('#Total').text(fMoney(sTotal || 0, 0, 'NTD'));
            //    $(".confappoint").click(function () {
            //        if (!eForm.valid()) {
            //            return false;
            //        }
            //        var formdata = getFormSerialize(eForm);
            //        formdata.ExpoName = sExpoName;
            //        g_db.SetDic('ContactInfo', formdata);
            //        window.location.href = window.location.origin + '/TG/page/inquiryPreview' + (bEn ? '_en' : '') + '.html';
            //    });
            //}
            //else {
            //    window.location.href = window.location.origin + '/TG/page/inquiry' + (bEn ? '_en' : '') + '.html';
            //}
        };

    init();
});