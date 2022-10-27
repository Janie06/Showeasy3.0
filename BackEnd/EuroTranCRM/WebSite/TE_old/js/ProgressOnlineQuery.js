var sRefNumber = getUrlParam('RefNumber');

$(function () {
    'use strict';

    var sRandomCode = '',
        sIp = '',
        sIPInfo = '',
        oCurData = {},
        sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
        materialForm = function () {
            return $('.material-field').focus(function () {
                $('.errormsg').text('').hide();
                return $(this).closest('.form-group-material').addClass('focused has-value');
            }).focusout(function () {
                $('.errormsg').text('').hide();
                return $(this).closest('.form-group-material').removeClass('focused');
            }).blur(function () {
                if (!this.value) {
                    $(this).closest('.form-group-material').removeClass('has-value');
                }
                return $(this).closest('.form-group-material').removeClass('focused');
            });
        },
        /*
        * 目的：生成一个随机数
        * @param： min（int）最小值
        * @param： max（int）最大值
        * @return： 無
        * 作者：John
        *********************************************/
        randomNum = function (min, max) {
            return Math.floor(Math.random() * (max - min) + min);
        },
        /*
        * 目的：生成一个随机色
        * @param： min（int）最小值
        * @param： max（int）最大值
        * @return： 無
        * 作者：John
        *********************************************/
        randomColor = function (min, max) {
            var r = randomNum(min, max);
            var g = randomNum(min, max);
            var b = randomNum(min, max);
            return "rgb(" + r + "," + g + "," + b + ")";
        },
        /*
        * 目的：绘制验证码图片
        * @param： min（int）最小值
        * @param： max（int）最大值
        * @return： 無
        * 作者：John
        *********************************************/
        fnDrawPic = function () {
            var canvas = document.getElementById("canvas");
            var width = canvas.width;
            var height = canvas.height;
            var ctx = canvas.getContext('2d');
            ctx.textBaseline = 'bottom';

            /**绘制背景色**/
            ctx.fillStyle = randomColor(180, 240); //颜色若太深可能导致看不清
            ctx.fillRect(0, 0, width, height);
            /**绘制文字**/
            //var str = 'ABCEFGHJKLMNPQRSTWXY123456789';
            var str = '123456789';
            sRandomCode = '';
            for (var i = 0; i < 4; i++) {
                var txt = str[randomNum(0, str.length)];
                sRandomCode += txt;
                ctx.fillStyle = randomColor(50, 60);  //随机生成字体颜色
                ctx.font = randomNum(35, 40) + 'px SimHei'; //随机生成字体大小
                var x = 10 + i * 25;
                var y = randomNum(40, 40);
                var deg = randomNum(-45, 45);
                //修改坐标原点和旋转角度
                ctx.translate(x, y);
                ctx.rotate(deg * Math.PI / 270);
                ctx.fillText(txt, 0, 0);
                //恢复坐标原点和旋转角度
                ctx.rotate(-deg * Math.PI / 270);
                ctx.translate(-x, -y);
            }
            /**绘制干扰线**/
            //for (var i = 0; i < 8; i++) {
            //    ctx.strokeStyle = randomColor(40, 180);
            //    ctx.beginPath();
            //    ctx.moveTo(randomNum(0, width), randomNum(0, height));
            //    ctx.lineTo(randomNum(0, width), randomNum(0, height));
            //    ctx.stroke();
            //}

            /**绘制干扰点**/
            for (var i = 0; i < 100; i++) {
                ctx.fillStyle = randomColor(0, 255);
                ctx.beginPath();
                ctx.arc(randomNum(0, width), randomNum(0, height), 1, 0, 2 * Math.PI);
                ctx.fill();
            }
        },
        /*
        * 目的：设置名称
        * @return： 無
        * 作者：John
        *********************************************/
        fnSetName = function () {
            var oSupplierName = $('#SupplierName'),
                sTranName = !oCurData.SupplierName ? oCurData.SupplierEName : oCurData.SupplierName;
            if (sLang == 'zh-TW') {
                oSupplierName.text(sTranName);
            }
            else if (sLang == 'zh') {
                oSupplierName.text(sTranName);
            }
        },
        /**
         * 獲取資料
         * @param： sQueryNum{String}查詢號碼
         * @return 無
         * 起始作者：John
         * 起始日期：2017/01/05
         * 最新修改人：John
         * 最新修日期：2017/01/05
         */
        fnGetData = function (sQueryNum) {
            g_api.ConnectLite(Service.apite, 'GetTrackingProgress', {
                QueryNum: sQueryNum,
                IP: sIp,
                IPInfo: sIPInfo
            }, function (res) {
                if (res.RESULT) {
                    fnReleseData(res.DATA.rel, sQueryNum);
                }
            }, null, null, i18next.t('message.Querying'));
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
                            if (name == 'IMPORT' || name.indexOf('RETURN') > -1) {
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
                                    type: 'button', class: 'btn btn-success signaturefile', value: '取得簽收證明', 'data-i18n': 'common.ObtainProofofDelivery', 'data-path': path, id: 'btn-' + btntype
                                });
                            if (!o.complete || !path) {
                                oBtn.attr('disabled', true);
                            }
                            return oBtn[0].outerHTML;
                        },
                        setInfo: function (val, type) {
                            var o = $.parseJSON(val),
                                saLi = fnReleaseObj(o),
                                saInfoHtml = [];

                            $.each(saLi, function (idx, li) {
                                if (li.Checked) {
                                    saInfoHtml.push('<tr><td>' + (li.Date || '').substr(0, 10) + '</td><td style="text-align:center" data-i18n="' + type + '.' + li.keyName + '"></td><td>' + (li.Remark || '') + '</td></tr>');
                                }
                            });
                            return saInfoHtml.reverse().join('');
                        },
                        setFlow: function (val, type) {
                            var o = $.parseJSON(val),
                                saLi = fnReleaseObj(o),
                                sFlowHtml = $('<div>'),
                                bactive = false;

                            $.each(saLi, function (idx, li) {
                                if (li.Date !== undefined) {
                                    var sChecked = li.Checked,
                                        oLi = $('<li>', { class: 'step step-inactive', html: '<a href="#" data-i18n="' + type + '.' + li.keyName + '"></a>' }),
                                        oDiv = $('<div hidden>');

                                    if (sChecked) {
                                        oLi.addClass('step-complete');
                                        oDiv.addClass('div-complete');
                                    }
                                    else {
                                        oLi.addClass('step-incomplete');
                                        oDiv.addClass('div-incomplete');
                                    }

                                    if (!sChecked && !bactive) {
                                        sFlowHtml.find('li:last').removeClass('step-inactive').addClass('step-active');
                                        bactive = true;
                                    }
                                    sFlowHtml.append([oLi, oDiv]);
                                }
                            });
                            sFlowHtml.find('div').last().remove();
                            if (o.complete) {
                                sFlowHtml.find('li').last().addClass('step-all-complete');
                            }
                            return sFlowHtml.html();
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

                sHtmlInfo = $('#temp_info').render(oCurData);
                $('.shadow').hide();
                $('.panel-info').html(sHtmlInfo);
                $('hr').last().remove();
                if (oCurData.Flows.length == 1) {
                    $('.tab-pane').css({ 'border-bottom': '0' });
                }
                if ((sType == '01' || sType == '03' || sType == '04') && oCurData.Flows.length == 1) {
                    $('.cusbuttons :input[id=btn-IMPORT]').remove().clone(true).prependTo('.toolbar');
                }
                else if (sType == '01' && oCurData.Flows.length > 1) {
                    $('.cusbuttons :input[id^=btn-RETURN]:last').remove().clone(true).prependTo('.toolbar');
                }
                $(".sidebar-1,.btnquery-next").click(function (e) {
                    $('.shadow').show();
                    $('.toolbar').hide();
                    $('#queryNum,#randomNum').val('');
                    $('.panel-info').html('');
                });
                $('.toolbar').show();
                $(".btnprint").click(function (e) {
                    //window.print();
                    $(".panel-info").jqprint({ operaSupport: false });
                });
                if ($(".signaturefile").length > 0) {
                    $('.signaturefile').click(function (e) {
                        var sPath = $(this).attr('data-path'),
                            sFileName = $('#queryNum').val() + '_' + this.id.replace('btn-', '');
                        showWaiting(i18next.t('common.Downloading'));
                        DownLoadFile(sPath, sFileName);
                    });
                }
                transLang();
                fnSetName();
            }
            else {
                $('.errormsg').text(i18next.t('common.NoDataFound')).show();
            }
            fnDrawPic();
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
            if (!g_db.SupportLocalStorage()) {
                if (sLang == 'en') {
                    alert('The current browser does not support local storage. Please turn off private browsing settings');
                }
                else if (sLang == 'zh') {
                    alert('当前浏览器不支持本地储存，请关闭无痕浏览模式');
                }
                else {
                    alert('當前瀏覽器不支持本地儲存，請關閉私密瀏覽設定');
                }
                $('body').html('');
                return;
            }
            if (sRefNumber) {
                $('#queryNum').val(sRefNumber);
                $('.form-group-material').eq(0).addClass('has-value');
            }

            materialForm();

            $.ajax({
                type: 'get',
                jsonpCallback: "ipCallback",//callback的function名称
                url: 'https://www.taobao.com/help/getip.php',
                dataType: 'jsonp',
                success: function (res) {
                    sIp = res.ip;
                    g_api.ConnectLite(Service.apiappcom, 'GetIPInfo', {
                        ip: sIp
                    }, function (res1) {
                        if (res1.RESULT) {
                            var oIpInfo = JSON.parse(res1.DATA.rel);
                            if (oIpInfo.code == 0) {
                                sIPInfo = Tostr(oIpInfo.data);
                            }
                        }
                    });
                }
            });

            $('[data-value=zh-TW]').parent().hide();
            $("#gotop").gotop({ content: 980, bottom: 60, margin: "none", position: "right", scrollTop: 100, duration: 700 });

            fnDrawPic();
            $("#changeImg").click(function (e) {
                e.preventDefault();
                fnDrawPic();
            });

            $(".btnquery").click(function () {
                $('.errormsg').text('').hide();
                var sQueryNum = $('#queryNum').val(),
                    sRandomNum = $('#randomNum').val(),
                    sMsg = '';

                if (!sQueryNum) {
                    sMsg = i18next.t('message.QueryNO_required');  //請輸入查詢號碼;
                }
                else if (!sRandomNum) {
                    sMsg = i18next.t('message.RandomNO_required'); //請輸入驗證碼;
                }
                else if (sRandomCode !== sRandomNum) {
                    sMsg = i18next.t('message.RandomNO_Incorrect'); // 驗證碼不正確;
                }
                if (sMsg) {
                    $('.errormsg').text(sMsg).show();
                    return false;
                }
                var sWaitting = i18next.t('message.Waitting'); // 查詢中...;
                fnGetData(sQueryNum)
            });

            $(document).on('keydown', function (e) {
                if (e.keyCode === 13) {
                    $(".btnquery").click();
                }
            });
            setLang(sLang);
        };

    init();
});