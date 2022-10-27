'use strict';
var sProgramId = getProgramId(),
    sEditPrgId = getEditPrgId(),
    oGrid = null,
    fnPageInit = function () {
        var sOptionHtml_ExhibitionName = '';
        var setStateDrop = function () {
            return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                OrgID: 'TE',
                ArgClassID: 'Area',
                LevelOfArgument: 1
            }, function (res) {
                if (res.RESULT) {
                    let saState = res.DATA.rel;
                    if (saState.length > 0) {
                        $('#State').html(createOptions(saState, 'id', 'text', false));
                    }
                }
            });
        },
        fnSetExpDrop = function () {
            return g_api.ConnectLite('BusinessOpportunity_Qry', 'GetExp', {}, function (res) {
                if (res.RESULT) {
                    var data = res.DATA.rel;
                    var dataCount = data.length;
                    var optionScript = '<option value="">請選擇...</option>';
                    for (var i = 0; i < dataCount; i++) {
                        optionScript += "<option value='" + data[i]['ExhibitionCode'] + "' title='" + data[i]['Exhibitioname_TW'] + "'>";
                        optionScript += data[i]['Exhibitioname_TW'] += "</option>";
                    }
                    $('#ExhibitionCode').html(optionScript).select2();
                    select2Init($('#ExhibitionCode').parent());
                }
            })
        },
        /**
        * 創建年份下拉選項
        */
        fnSetYearDrop = function () {
            let objNowYear = new Date();
            let sOption = '';
            let sYear = '';
            let sNowYear = objNowYear.getFullYear();
            for (let i = 0; i < 5; i++) {
                sYear = (parseInt(sNowYear) + i).toString();
                sOption += "<option value='" + sYear + "'>" + sYear + "</option>";
            }
            $("#Year").html(sOption);
        },
        fnButtonHandler = function (inst, e) {
            var sId = inst.id;
            switch (sId) {
                case "Toolbar_Qry":


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

                    break;
                case "Toolbar_Upd":

                    break;
                case "Toolbar_Copy":

                    break;
                case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                    break;
                case "Toolbar_Exp":

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
            PrgId: "BusinessOpportunity_Upd",
            ButtonHandler: fnButtonHandler,
            SearchBar: true
        });
                 
        $.whenArray([
            fnSetExpDrop(),
            fnSetEpoDrop({
                CallBack: function (data) {
                    sOptionHtml_ExhibitionName = createOptions(data, 'SN', 'ExhibitioShotName_TW')
                }
            }),
            fnSetYearDrop()
        ]).done(function ()
        {
                    
        })
        var Effective = $('input[name=Effective]:checked').val();
        var Year;
        var Date;
        var ExhibitionShotName;
        var ExhibitionName;
        var ExhibitionName_EN;
        var State;
        var Industry;
        var CustomerName;
        var Department;
        var Contactor;
        var JobTitle;
        var Email1;
        var Email2;
        var Telephone1;
        var Telephone2; 

        $('input[name=Status]').click(function () {
            var Effective = $('input[name=Effective]:checked').val();
        });

        $('#Date').dateRangePicker({
            language: 'zh-TW',
            separator: ' ~ ',
            format: 'YYYY/MM/DD',
            autoClose: true
        });

        $(document).on('click', '#BtnExhibitionCode', function (res) {
            layer.open({
                type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                title: "對應展覽",//i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                area: '640px;',//寬度
                shade: 0.75,//遮罩
                //maxmin: true, //开启最大化最小化按钮
                id: 'layer_ExhibitionCode', //设定一个id，防止重复弹出
                offset: '10px',//右下角弹出
                anim: 0,//彈出動畫
                btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                btnAlign: ['c'],//按鈕位置
                content: '<style>.select2-container{z-index: 39891015;}</style><div class="pop-box">\
                                <select data-type="select2" name="ExhibitionCode" id="ExhibitionCode" class="form-control w95p" datamsg="請輸入對應展覽"></select>\
                            </div>',
                success: function (layero, index) {
                    $('#ExhibitionCode').html(sOptionHtml_ExhibitionName).select2();
                },
                yes: function (index, layero) {
                    var confirmString = "確定對應該展覽";
                    layer.confirm(confirmString, { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                        var ExhibitionCode = $('#ExhibitionCode').val();
                        $('#BtnExhibitionCode').val(ExhibitionCode);
                        if (ExhibitionCode == "") {
                            $('[name=Effective][value="2"]').click();
                            $('[name=Effective][value="2"]').click();
                            $("#ExhibitionShotName").attr('disabled', false);
                            $("#ExhibitionName").attr('disabled', false);
                            $("#ExhibitionName_EN").attr('disabled', false);
                            $("#State").attr('disabled', false);
                            $("#Industry").attr('disabled', false);
                            $("#Year").val(Year);
                            $("#Date").val(Date);
                            $("#ExhibitionShotName").val(ExhibitionShotName);
                            $("#ExhibitionName").val(ExhibitionName);
                            $("#ExhibitionName_EN").val(ExhibitionName_EN);
                            $("#State").val(State);
                            $("#Industry").val(Industry);
                            $("#CustomerName").val(CustomerName);
                            $("#Department").val(Department);
                            $("#Contactor").val(Contactor);
                            $("#JobTitle").val(JobTitle);
                            $("#Email1").val(Email1);
                            $("#Email2").val(Email2);
                            $("#Telephone1").val(Telephone1);
                            $("#Telephone2").val(Telephone2);
                        } else {
							$('[name=Effective][value="1"]').attr('disabled', false);
                            $('[name=Effective][value="1"]').click();
                            $('[name=Effective][value="1"]').click();
                            g_api.ConnectLite('BusinessOpportunity_Qry', 'QueryExhibition', {
                                ExhibitionCode: ExhibitionCode
                            }, function (res) {
                                if (res.DATA.rel.length != 0) {
                                    var resData = res.DATA.rel;
                                    var DateStart = resData[0]['ExhibitionDateStart'];
                                    var DateEnd = resData[0]['ExhibitionDateEnd'];
                                    var sDateStart = '';
                                    var sDateEnd = '';
                                    var DateStr = '';
                                    var YearStr = '';
                                    if (resData['ExhibitionDateStart'] == '' || resData['ExhibitionDateStart'] == null) {
                                        try {
                                            sDateStart = DateStart.split('T')[0].replaceAll('-', '/');
                                            sDateEnd = DateEnd.split('T')[0].replaceAll('-', '/');
                                            DateStr = sDateStart + '' + " ~ " + sDateEnd;
                                            YearStr = sDateStart.split('/')[0];
                                        }
                                        catch (e) {
                                        }
                                    }
                                    Year = $("#Year").val();
                                    Date = $("#Date").val();
                                    ExhibitionShotName = $("#ExhibitionShotName").val();
                                    ExhibitionName = $("#ExhibitionName").val();
                                    ExhibitionName_EN = $("#ExhibitionName_EN").val();
                                    State = $("#State").val();
                                    Industry = $("#Industry").val();
                                    CustomerName = $("#CustomerName").val();
                                    Department = $("#Department").val();
                                    Contactor = $("#Contactor").val();
                                    JobTitle = $("#JobTitle").val();
                                    Email1 = $("#Email1").val();
                                    Email2 = $("#Email2").val();
                                    Telephone1 = $("#Telephone1").val();
                                    Telephone2 = $("#Telephone2").val();
                                    $("#ExhibitionShotName").val(resData[0]['ExhibitioShotName_TW']);
                                    $("#ExhibitionName").val(resData[0]['Exhibitioname_TW']);
                                    $("#ExhibitionName_EN").val(resData[0]['Exhibitioname_EN']);
                                    $("#State").val(resData[0]['State']);
                                    $("#Industry").val(resData[0]['Industry']);
                                    $('#Year').val(YearStr);
                                    $('#Date').val(DateStr);
                                    

                                    $("#ExhibitionShotName").attr('disabled', true);
                                    $("#ExhibitionName").attr('disabled', true);
                                    $("#ExhibitionName_EN").attr('disabled', true);
                                    $("#State").attr('disabled', true);
                                    $("#Industry").attr('disabled', true);
									$("#Year").attr('disabled', true);
                                    $("#Date").attr('disabled', true);
									$("#BtnExhibitionCode").attr('disabled', true);
									$("#CreateExhibition").attr('disabled', true);
                                } else {
                                    showMsg("查無展覽，請確認展覽是否存在", 'error');
                                }

                            }, function () {

                            })
                        }
                        layer.close(index);
                        layer.close(index - 1);
                    });
                },
                end: function () {
                }
            });
            });
        };
        $(document).on('click', '#CreateExhibition', function () {
            parent.top.openPageTab('Exhibition_Upd', '?Action=Add');
        })
        $('[name=Effective][value="2"]').click(function () {
            $('#BtnExhibitionCode').attr('disabled', false);
            $('#CreateExhibition').attr('disabled', false);
            $("#ExhibitionShotName").attr('disabled', false);
            $("#ExhibitionName").attr('disabled', false);
            $("#ExhibitionName_EN").attr('disabled', false);
            $("#State").attr('disabled', false);
            $("#Industry").attr('disabled', false);
            $("#Year").attr('disabled', false);
            $("#Date").attr('disabled', false);
            $('#ExhibitionCode').val('');
			$('[name=Effective][value="1"]').attr('disabled', true);
			$('#BtnExhibitionCode').val("");
        })
    init();
    };
require(['base', 'select2', 'daterangepicker', 'jsgrid', 'util'], fnPageInit, 'daterangepicker');