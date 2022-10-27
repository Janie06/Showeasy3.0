var sRefNumber = getUrlParam('RefNumber');

$(function () {
    'use strict';

    var sRandomCode = '',
        sIp = '',
        sIPInfo = '',
        oCurData = {},
        sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        /*
        * 目的：设置名称
        * @return： 無
        * 作者：John
        *********************************************/
        fnSetName = function () {
            if (sLang === 'zh') {
                var oSupplierName = $('#SupplierName');
                oSupplierName.text(simplized(oSupplierName.text()));
            }
        },
        /*
        * 目的：初始化查詢區塊
        * @return： 無
        * 作者：John
        *********************************************/
        fnInitQueryBox = function () {
            var elQueryBox = $('#temp_query').render();
            $('#right').html(elQueryBox);

            $('#form_onlinetrack').validate({
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
                    trackingno: {
                        required: { 'zh-TW': '請輸入查詢號碼', 'zh': '请输入查询号码', 'en': 'Please enter a query number' }[sLang]
                    },
                    validcode: {
                        required: { 'zh-TW': '請輸入驗證碼', 'zh': '请输入验证码', 'en': 'Please enter the verification code' }[sLang]
                    }
                }
            });

            $(".btnquery").click(function () {
                fnGetData()
            });

            $('#change_validcode,.change_validcode').on('click', function () {
                var url = $('#imgvalidcode').data('url') || $('#imgvalidcode').attr('src');
                $('#imgvalidcode').attr('src', url + '&' + Math.random()).data('url', url);
            });
        },
        /**
         * 獲取資料
         * @return {Boolean} 是否停止
         */
        fnGetData = function () {
            var sTrackingNo = $('#trackingno').val(),
                sValidCode = $('#validcode').val();

            if (!$('#form_onlinetrack').valid()) {
                return false;
            }
            g_api.ConnectLite(Service.apite, 'GetTrackingProgress', {
                flag: 'cap4',
                QueryNum: sTrackingNo,
                ValidCode: sValidCode,
                IP: sIp,
                IPInfo: sIPInfo
            }, function (res) {
                if (res.RESULT) {
                    fnReleseData(res.DATA.rel, sTrackingNo);
                }
                else {
                    $('#change_validcode').click();
                    showMsg(res.MSG, 'error'); //提示錯誤
                }
            }, null, null, { 'zh-TW': '資料查詢中...', 'zh': '资料查询中...', 'en': 'Data query...' }[sLang]);
        },
        fnReleseData = function (data, num) {
            if (JSON.stringify(data) !== "{}") {
                oCurData = data;
                var sType = num.substr(2, 2),
                    sHtmlInfo = '',
                    fnReleaseObj = function (o) {
                        var saLi = [];
                        for (var key in o) {
                            if (typeof o[key] === 'object') {
                                var nobj = o[key];
                                nobj.keyName = key;
                                saLi.push(nobj);
                            }
                        }
                        saLi = saLi.sort(function (a, b) {
                            return new Date(a.Date === '' ? new Date() : a.Date) > new Date(b.Date === '' ? new Date() : b.Date);
                        });
                        return saLi;
                    },
                    myHelpers = {
                        checkFlowName: function (name) {
                            var bRtn = false;
                            if (name === 'IMPORT' || name.indexOf('RETURN') > -1) {
                                bRtn = true;
                            }
                            return bRtn;
                        },
                        setJson: function (val, name) {
                            var o = $.parseJSON(val);
                            return getJsonVal(o, name) || '';
                        },
                        setBtn: function (val, btntype, path) {
                            var o = $.parseJSON(val),
                                oBtn = $('<input>', {
                                    type: 'button', class: 'btn btnStyle signaturefile', value: '取得簽收證明', 'data-i18n': 'common.ObtainProofofDelivery', 'data-path': path, id: 'btn-' + btntype
                                });
                            if (!o.complete || !path) {
                                oBtn.attr('disabled', true);
                            }
                            return oBtn[0].outerHTML;
                        },
                        setInfo: function (val, type) {
                            var o = $.parseJSON(val),
                                saLi = fnReleaseObj(o),
                                saInfoHtml = [],
                                iLastIndex = 0;
                            $.each(saLi, function (idx, li) {
                                if (li.Checked) {
                                    iLastIndex = idx;
                                    saInfoHtml.push('<div class="tr templ_class' + iLastIndex + '">\
                                                        <div class= "td" >' + (li.Date || '').substr(0, 10) + '</div >\
                                                        <div class="td center" data-i18n="' + type + '.' + li.keyName + '"></div>\
                                                        <div class="td center">' + (li.keyName === "InTransit1" ? `ETA ${li.ETA}: ${li.ETADate}` : (li.Remark || '')) + '</div>\
                                                     </div >');
                                }
                            });
                            return saInfoHtml.reverse().join('').replace('templ_class' + iLastIndex, 'blue bold');
                        },
                        setFlow: function (val, type) {
                            var o = $.parseJSON(val),
                                saLi = fnReleaseObj(o),
                                elDiv = $('<div>'),
                                iActive = -1;

                            $.each(saLi, function (idx, li) {
                                if (li.Date !== undefined) {
                                    var sChecked = li.Checked,
                                        elLi = "";

                                    if (sLang === "en") {
                                        elLi = $('<li>', { class: '', html: '<p data-i18n="' + type + '.' + li.keyName + '"></p><div></div><span class="entab"></span>' });
                                    } else {
                                        elLi = $('<li>', { class: '', html: '<p data-i18n="' + type + '.' + li.keyName + '"></p><div></div><span></span>' });
                                    }

                                    if (o.complete && saLi.length - 1 === idx) {
                                        elLi = $('<li>', { class: 'bluePro', html: '<p><b data-i18n="' + type + '.' + li.keyName + '"></b><img src="' + (sLang === 'zh-TW' ? '' : '../') + 'images/tick.svg" width="18" height="18"></p><div></div><span></span>' });
                                    }

                                    if (sChecked) {
                                        elLi.addClass('active');
                                        //elLi.find('.print-img').attr('src', '' + (sLang === 'zh-TW' ? '' : '../') + 'images/progress_print.png');
                                    }

                                    if (!sChecked && iActive === -1) {
                                        iActive = idx;
                                    }
                                    elDiv.append(elLi);
                                }
                            });

                            if (iActive > 0) {
                                elDiv.find('li').eq(iActive - 1).addClass('bluePro');
                            }
                            return elDiv.html() + '<p class="clear"></p>';
                        },
                        setBillLadNOSub: function (val) {
                            return val.replaceAll('/', '<br>').replaceAll(',', '<br>').replaceAll('，', '<br>').replaceAll(';', '<br>').replaceAll('；', '<br>').replace('|', '<br>').replace('|', '<br>').replace('|', '<br>').replace('|', '<br>');
                        },
                        setSupplierName: function (cnname, enname) {
                            var sName = cnname;
                            if (!cnname) {
                                sName = enname || '';
                            }
                            return sName;
                        }
                    };
                $.views.helpers(myHelpers);

                sHtmlInfo = $('#temp_queryinfo').render(oCurData);
                $('#right').html(sHtmlInfo);

                $(".sidebar-1,.btnquery-next").click(function (e) {
                    fnInitQueryBox();
                    $('#change_validcode').click();
                });
                $(".btnprint").click(function (e) {
                    //window.print();
                    $('#right').jqprint({ operaSupport: false });
                });
                if ($(".signaturefile").length > 0) {
                    $('.signaturefile').click(function (e) {
                        var sPath = $(this).attr('data-path'),
                            sFileName = num + '_' + this.id.replace('btn-', '');
                        DownLoadFile(sPath, sFileName);
                    });
                }
                transLang();
                fnSetName();
            }
        },
        /*
         * 初始化 function
         * @param 無
         * @return  無
         * 起始作者：John
         * 起始日期：2016/05/21
         * 最新修改人：John
         * 最新修日期：2016/11/03
         */
        init = function () {
            fnInitQueryBox();

            if (sRefNumber) {
                $('#trackingno').val(sRefNumber);
            }

            $.ajax({
                type: 'get',
                //jsonpCallback: "ipCallback",//callback的function名称
                //url: 'https://www.taobao.com/help/getip.php',
                //dataType: 'jsonp',
                url: 'https://api.ipify.org?format=json&callback=?',
                dataType: 'json',
                success: function (res) {
                    sIp = res.ip;
                    g_api.ConnectLite(Service.apiappcom, 'GetIPInfo', {
                        ip: sIp
                    }, function (res1) {
                        if (res1.RESULT) {
                            var oIpInfo = JSON.parse(res1.DATA.rel);
                            if (oIpInfo.code === 0) {
                                sIPInfo = Tostr(oIpInfo.data);
                            }
                        }
                    });
                }
            });

            if ($(window).width() < 800) {
                $(".target").scroll(function () {
                    if ($(".target").scrollLeft() > 140) {
                        //alert("dsfsdf");
                        $(this).parent().removeClass("tWarpper");
                    } else {
                        $(this).parent().addClass("tWarpper");
                    }
                });
            }

            $(document).on('keydown', function (e) {
                if (e.keyCode === 13) {
                    $(".btnquery").click();
                }
            });
            setLang(sLang);
        };

    init();
});