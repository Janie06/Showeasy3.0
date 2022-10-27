'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    sAction = getUrlParam('Action') || 'Add',
    sDataId = getUrlParam('SN'),
    sTab = getUrlParam('Tab'),
    sCheckId = sDataId,
    bRefresh = false,
    fnPageInit = function () {
        var oGrid = null,
            oForm = $('#form_main'),
            oGrid1 = null,
            oValidator = null,
            oCurData = {},
            saGridData = [],
            saState = [],
            saHalls = [],
            oBaseQueryPm = {
                pageIndex: 1,
                pageSize: parent.SysSet.GridRecords || 10,
                sortField: 'CustomerCName',
                sortOrder: 'asc'
            },
            /**
             * 獲取資料
             * @return {Object} Ajax 物件
             */
            fnGet = function () {
                if (sDataId) {
                    $('#litab3').show();
                    return g_api.ConnectLite(sQueryPrgId, ComFn.GetOne,
                        {
                            Guid: sDataId
                        },
                        function (res) {
                            if (res.RESULT) {
                                var oRes = res.DATA.rel;
                                oCurData = oRes;
                                $('.TransferResult').show();
                                if (oRes.IsTransfer === 'N') {
                                    $('#TransferResult').text(i18next.t("common.Transfer_No")).css('color', 'red');// ╠common.Transfer_No⇒未拋轉╣
                                }
                                else {// ╠common.Transfer_No⇒已拋轉╣  ╠common.TransferDate⇒最後拋轉時間╣
                                    $('#TransferResult').text(i18next.t("common.Transfer_Yes") + '（' + i18next.t("common.TransferDate") + '：' + newDate(oRes.LastTransfer_Time) + '）').css('color', 'green');
                                }
                            }
                        });
                }
                else {
                    $('#litab3').hide();
                    oCurData.LogoFileId = guid();
                    fnUpload();
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 新增資料
             * @param   {String} sFlag  新增或儲存後新增
             */
            fnAdd = function (sFlag) {
                var data = getFormSerialize(oForm);
                data = packParams(data);
                data.OrgID = parent.OrgID;
                data.LogoFileId = oCurData.LogoFileId;
                data.IsTransfer = 'N';
                //data.ExhibitionCode = 'SerialNumber|' + parent.UserInfo.OrgID + '|' + (parent.UserInfo.OrgID === 'TE' ? '' : parent.UserInfo.OrgID) + '|MinYear|3||';

                if (!data.ExhibitionDate) {
                    delete data.ExhibitionDateStart;
                    delete data.ExhibitionDateEnd;
                }
                else {
                    data.ExhibitionDateStart = $.trim(data.ExhibitionDate.split('~')[0]);
                    data.ExhibitionDateEnd = $.trim(data.ExhibitionDate.split('~')[1]);
                }
                delete data.ExhibitionDate;
                delete data.file_hidden;

                if (!data.ShelfTime_Home) {
                    delete data.ShelfTime_Home;
                }
                if (!data.ShelfTime_Abroad) {
                    delete data.ShelfTime_Abroad;
                }

                g_api.ConnectLite(Service.com, ComFn.GetSerial, {
                    Type: parent.UserInfo.OrgID === 'TE' ? '' : parent.UserInfo.OrgID,
                    Flag: 'MinYear',
                    Len: 3,
                    Str: '',
                    AddType: '',
                    PlusType: ''
                }, function (res) {
                    if (res.RESULT) {
                        data.ExhibitionCode = res.DATA.rel;
                        CallAjax(ComFn.W_Com, ComFn.GetAdd, {
                            Params: { exhibition: data }
                        }, function (res1) {
                            if (res1.d > 0) {
                                bRequestStorage = false;
                                if (sFlag === 'add') {
                                    CallAjax(ComFn.W_Com, ComFn.GetOne, {
                                        Type: '',
                                        Params: {
                                            exhibition: {
                                                ExhibitionCode: data.ExhibitionCode
                                            },
                                        }
                                    }, function (res2) {
                                        var oRes = $.parseJSON(res2.d);
                                        showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Upd&SN=' + oRes.SN); // ╠message.Save_Success⇒新增成功╣
                                    });
                                }
                                else {
                                    showMsgAndGo(i18next.t("message.Save_Success"), sProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
                                }
                                if (data.ExhibitionDateStart && data.ExhibitionDateEnd) {
                                    //如果展覽時間不為空的話就同步更新至行事曆和outlook中
                                    fnSynChronousCalendar(data);
                                }
                            }
                            else {
                                showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                            }
                        }, function () {
                            showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                        });
                    }
                    else {
                        showMsg(i18next.t('message.CreateBill_Failed') + '<br>' + res.MSG, 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                    }
                }, function () {
                    showMsg(i18next.t('message.CreateBill_Failed'), 'error'); // ╠message.CreateBill_Failed⇒帳單新增失敗╣
                });
            },
            /**
             * 檢查名稱
             */
            fnCheckNameThenAction = function (Type) {
                return g_api.ConnectLite(sProgramId, 'CheckExhibitionName', {
                    Type: Type,
                    SN: getUrlParam('SN') ,
                    Exhibitioname_TW: $('#Exhibitioname_TW').val() ? $('#Exhibitioname_TW').val():  "",
                    ExhibitioShotName_TW: $('#ExhibitioShotName_TW').val() ? $('#ExhibitioShotName_TW').val() : ""
                }, function (res) {
                    if (!res.MSG) {
                        switch (Type) {
                            case "add":
                            case "readd":
                                fnAdd(Type);
                                break;
                            case "upd":
                                fnUpd();
                                break;
                            default:
                                break;
                        }
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg("未知錯誤，請聯絡資訊人員", 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                });
            },


            /**
             * 修改資料
             */
            fnUpd = function () {
                var data = getFormSerialize(oForm);
                data = packParams(data, 'upd');
                data.LogoFileId = oCurData.LogoFileId;

                if (!data.ExhibitionDate) {
                    delete data.ExhibitionDateStart;
                    delete data.ExhibitionDateEnd;
                }
                else {
                    data.ExhibitionDateStart = $.trim(data.ExhibitionDate.split('~')[0]);
                    data.ExhibitionDateEnd = $.trim(data.ExhibitionDate.split('~')[1]);
                }
                delete data.ExhibitionDate;
                delete data.file_hidden;

                if (!data.ShelfTime_Home) {
                    delete data.ShelfTime_Home;
                }
                if (!data.ShelfTime_Abroad) {
                    delete data.ShelfTime_Abroad;
                }
                if (!data.ExhibitionCode) {
                    data.ExhibitionCode = 'SerialNumber|' + parent.UserInfo.OrgID + '||MinYear|3||';
                }

                CallAjax(ComFn.W_Com, ComFn.GetUpd, {
                    Params: {
                        exhibition: {
                            values: data,
                            keys: { SN: sDataId }
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        bRequestStorage = false;
                        showMsg(i18next.t("message.Modify_Success"), 'success'); //╠message.Modify_Success⇒修改成功╣
                        if (window.bLeavePage) {
                            setTimeout(function () {
                                pageLeave();
                            }, 1000);
                        }
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
                        exhibition: {
                            SN: sDataId
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
             * 設定國家下拉選單
             */
            setStateDrop = function () {
                return g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'Area',
                    ParentID: '',
                    LevelOfArgument: 1
                }, function (res) {
                    if (res.RESULT) {
                        saState = res.DATA.rel;
                        if (saState.length > 0) {
                            $('#State').html(createOptions(saState, 'id', 'text', true)).change(function () {
                                setExhibitionAddressDrop();
                            });
                            select2Init($('#State').parent());
                            if (parent.OrgID === 'TG' && sAction === 'Add') {
                                $('#State').val('TWN');
                                setExhibitionAddressDrop();
                            }
                        }
                        else {
                            $('#State').html(createOptions([]));
                        }
                    }
                });
            },
            /**
             * 設定展覽地點下拉選單
             */
            setExhibitionAddressDrop = function () {
                var sState = $('#State').val() || '';
                g_api.ConnectLite(Service.com, ComFn.GetArguments, {
                    ArgClassID: 'Area',
                    ParentID: sState,
                    LevelOfArgument: 2
                }, function (res) {
                    if (res.RESULT) {
                        saHalls = res.DATA.rel;
                        if (saHalls.length > 0) {
                            $('#ExhibitionAddress').html(createOptions(saHalls, 'id', 'text', true)).change(function () {
                                var sExhibitionAddressId = this.value;
                                if (sExhibitionAddressId) {
                                    var oState = Enumerable.From(saHalls).Where(function (e) { return e.id === sExhibitionAddressId; }).First();
                                    $('#ExhibitionAddress_CN').text(oState.text_cn);
                                    $('#ExhibitionAddress_EN').text(oState.text_en);
                                }
                                else {
                                    $('#ExhibitionAddress_CN').text('');
                                    $('#ExhibitionAddress_EN').text('');
                                }
                            });
                            if (oCurData.ExhibitionAddress) {
                                $('#ExhibitionAddress').val(oCurData.ExhibitionAddress);
                                if (!$('#ExhibitionAddress').val()) {
                                    $('#ExhibitionAddress_CN').text('');
                                    $('#ExhibitionAddress_EN').text('');
                                }
                            }
                        }
                        else {
                            $('#ExhibitionAddress').html(createOptions([]));
                        }
                    }
                });
            },
            /**
             * 設定費用規則下拉選單
             */
            setCostRulesDrop = function () {
                return CallAjax(ComFn.W_Com, ComFn.GetList, {
                    Type: '', Params: {
                        exhibitionrules: {
                            OrgID: parent.OrgID
                        }
                    }
                }, function (res) {
                    var saData = JSON.parse(res.d);
                    if (saData) {
                        $('#CostRulesId').html(createOptions(saData, 'Guid', 'Title'));
                    }
                });
            },
            /**
             * 上傳附件
             * @param {Array}files 要綁定的html物件
             */
            fnUpload = function (files) {
                var option = {};
                option.input = $('#fileInput');
                option.theme = 'dragdropbox';
                option.folder = 'Exhibition';
                option.type = 'one';
                option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
                option.limit = 1;
                option.parentid = oCurData.LogoFileId;
                if (files) {
                    option.files = files;
                }
                fnUploadRegister(option);
            },
            /**
             * 拋轉文中
             */
            fnTransfer = function () {
                g_api.ConnectLite('Exhibition', 'Transfer', {
                    SN: oCurData.SN
                }, function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.Transfer_Success"), 'success'); // ╠message.Transfer_Success⇒拋轉完成╣
                        parent.msgs.server.pushTransfer(parent.OrgID, parent.UserID, oCurData.ExhibitionCode, 3);
                        fnGet();
                    }
                    else {
                        showMsg(i18next.t('message.Transfer_Failed') + '<br>' + res.MSG, 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Transfer_Failed"), 'error');// ╠message.Transfer_Failed⇒拋轉失敗╣
                });
            },
            /**
             * 抓去參加展覽列表資料
             * @param  {String} flag 是否匯出
             * @return {Object} ajax物件
             */
            fnGetGridData = function (flag) {
                if (sDataId) {
                    return g_api.ConnectLite(sProgramId, 'GetCustomers', {
                        SN: sDataId,
                        Flag: flag
                    }, function (res) {
                        if (res.RESULT) {
                            if (flag === 'export') {
                                DownLoadFile(res.DATA.rel, oCurData.Exhibitioname_TW);
                            }
                            else {
                                saGridData = res.DATA.rel;
                            }
                        }
                    });
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 抓去參加展覽列表資料
             * @param  {String} flag 是否匯出
             * @return {Object} ajax物件
             */
            fnGetGridData1 = function (args) {
                var oQueryPm = {};
                $.extend(oQueryPm, oBaseQueryPm, args);

                if (sDataId) {
                    oQueryPm.SN = sDataId;
                    return g_api.ConnectLite(sProgramId, 'GetImportCustomers', oQueryPm);
                }
                else {
                    return $.Deferred().resolve().promise();
                }
            },
            /**
             * 刪除單筆列表資料
             * @param  {String} id 廠商id
             * @return {Object} ajax 物件
             */
            fnDelGrid1Data = function (id) {
                CallAjax(ComFn.W_Com, ComFn.GetDel, {
                    Params: {
                        importcustomers: {
                            guid: id
                        }
                    }
                }, function (res) {
                    if (res.d > 0) {
                        showMsg(i18next.t("message.Delete_Success"), 'success'); // ╠message.Delete_Success⇒刪除成功╣
                        oGrid1.loadData();
                    }
                    else {
                        showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Delete_Failed"), 'error'); // ╠message.Delete_Failed⇒刪除失敗╣
                });
            },
            /**
             * 轉正式資料
             * @param  {Object} item 單筆廠商資料
             * @return {Object} ajax 物件
             */
            fnTransferToFormal = function (item) {
                layer.open({
                    type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t('common.CustomerTransferToFormal'),// ╠common.CustomerTransferToFormal⇒匯入廠商轉正╣
                    area: ['1200px', '600px'],//寬度
                    shade: 0.75,//遮罩
                    maxmin: true, //开启最大化最小化按钮
                    id: 'layer_TransferToFormal', //设定一个id，防止重复弹出
                    anim: 0,//彈出動畫
                    btnAlign: 'c',//按鈕位置
                    content: '../Crm/Customers_Upd.html?Action=Add&From=Appoint&Flag=Pop',
                    success: function (layero, index) {
                        //layer.full(index); //弹出即全屏
                        var iframe = layero.find('iframe').contents();
                        iframe.find('#CustomerCName').val(item.CustomerCName);
                        iframe.find('#CustomerEName').val(item.CustomerEName);
                        iframe.find('#UniCode').val(item.UniCode);
                        iframe.find('#Telephone').val(item.Telephone);
                        iframe.find('#Email').val(item.Email);
                        iframe.find('#Address').val(item.Address);
                        iframe.find('#InvoiceAddress').val(item.InvoiceAddress);
                        iframe.find('body').attr('PopId', item.guid);
                        setTimeout(function () {
                            iframe.find('.plustemplkey ').trigger('click');
                            var _input = iframe.find('#table_box').find(':input');
                            _input.eq(0).val(item.Contactor);
                            _input.eq(2).val(item.Telephone);
                            _input.eq(5).val(item.Email);
                            setTimeout(function () {
                                _input.eq(0).trigger('click');
                                _input.eq(2).trigger('click');
                                _input.eq(5).trigger('click');
                            }, 500);
                        }, 1000);
                    },
                    end: function () {
                        if (bRefresh) {
                            bRefresh = false;
                            oGrid1.loadData();
                        }
                    }
                });
            },
            /**
            * 匯入廠商資料
            */
            fnImportCusList = function () {
                $('#importfile').val('').off('change').on('change', function () {
                    if (this.value.indexOf('.xls') > -1 || this.value.indexOf('.xlsx') > -1) {
                        var sFileId = guid(),
                            sFileName = this.value;
                        $.ajaxFileUpload({
                            url: '/Controller.ashx?action=importfile&FileId=' + sFileId,
                            secureuri: false,
                            fileElementId: 'importfile',
                            success: function (data, status) {
                                g_api.ConnectLite(sProgramId, 'ImportCustomers', {
                                    FileId: sFileId,
                                    FileName: sFileName,
                                    SN: sDataId
                                }, function (res) {
                                    if (res.RESULT) {
                                        oGrid1.loadData();
                                    }
                                    else {
                                        showMsg(i18next.t('message.ProgressError') + '<br>' + res.MSG, 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                    }
                                }, function () {
                                    showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                                });
                            },
                            error: function (data, status, e) {
                                showMsg(i18next.t("message.ProgressError"), 'error'); // ╠message.ProgressError⇒資料處理異常╣
                            }
                        });
                        bRequestStorage = true;
                    }
                    else {
                        showMsg(i18next.t("message.FileTypeError"), 'error'); // ╠message.FileTypeError⇒文件格式錯誤╣
                    }
                }).click();
            },
            /**
             * 匯入廠商資料匯出
             */
            fnExportCusList = function () {
                g_api.ConnectLite(sProgramId, 'ExportCustomers', {
                    SN: sDataId
                }, function (res) {
                    if (res.RESULT) {
                        DownLoadFile(res.DATA.rel);
                    }
                });
            },
            /**
             * 新增匯入客戶
             * @param   {Object}data 序列化資料
             */
            fnAddCustomers = function (data) {
                data.ExhibitionNO = sDataId;
                g_api.ConnectLite(sProgramId, 'InsertImportCustomers', data, function (res) {
                    if (res.RESULT) {
                        oGrid1.loadData();
                        showMsg(i18next.t("message.Save_Success"), 'success'); // ╠message.Save_Success⇒新增成功╣
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg(i18next.t("message.Save_Failed"), 'error'); // ╠message.Save_Failed⇒新增失敗╣
                });
            },
            /**
             * 修改資料
             * @param   {Object}data 序列化資料
             */
            fnUpdCustomers = function (data) {
                data = packParams(data, 'upd');
                delete data.Exhibitioname_TW;
                delete data.Exhibitioname_EN;
                data = removeNull(data);

                g_api.ConnectLite(sProgramId, 'UpdImportCustomers', data, function (res) {
                    if (res.RESULT) {
                        bRequestStorage = false;
                        showMsg(i18next.t("message.Modify_Success"), 'success'); // ╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(res.MSG, 'error');
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error'); // ╠message.Modify_Failed⇒修改失敗╣
                });
            },
            /**
            * 目的 對應正式客戶
            * @param  {String}item 預約單資料
            */
            fnCorrespondFormalCus = function (item) {
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t("common.CorrespondFormalCus"), // ╠common.CorrespondImpCus⇒對應正式客戶╣
                    area: '640px;',//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_Correspond', //设定一个id，防止重复弹出
                    offset: '100px',//右下角弹出
                    anim: 0,//彈出動畫
                    btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
                    btnAlign: 'c',//按鈕位置
                    content: '<style>.select2-container{z-index: 39891015;}</style><div class="form-group">\
                                 <div class="col-sm-12">\
                                     <select class= "form-control w95p" id="CustomerId" name="CustomerId"></select>\
                                 </div>\
                              </div >',
                    success: function (layero, index) {
                        g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
                            if (res.RESULT) {
                                var saList = res.DATA.rel;
                                var sOptions = createOptions(saList, 'id', 'text');
                                $('#CustomerId').html(sOptions).select2();
                            }
                        });
                        transLang(layero);
                    },
                    yes: function (index, layero) {
                        var sCustomerId = $('#CustomerId').val();
                        if (!sCustomerId) {
                            showMsg(i18next.t('message.SelectFormalCus'));//╠message.SelectFormalCus⇒請選擇對應的客戶╣
                            return false;
                        }
                        return g_api.ConnectLite('Exhibition_Upd', 'UpdateCustomerTag', {//匯入費用項目
                            PrevId: item.guid,
                            AfterId: sCustomerId
                        }, function (res) {
                            if (res.DATA.rel) {
                                oGrid1.loadData();
                                showMsg(i18next.t("message.Correspond_Success"), 'success'); //╠message.Correspond_Success⇒對應成功╣
                                layer.close(index);
                            }
                            else {
                                showMsg(i18next.t("message.Correspond_Failed"), 'error');//╠message.Correspond_Failed⇒對應失敗╣
                            }
                        });
                    }
                });
            },
            /**
             * 同步新更新行事曆和outlook
             * @param   {Object}_data 序列化資料
             */
            fnSynChronousCalendar = function (_data) {
                var data = {};
                data.CalType = '04';//展覽
                data.Title = _data.ExhibitioShotName_TW;
                data.Description = _data.Exhibitioname_TW + '（' + _data.Exhibitioname_EN + '）    ' + _data.Memo;
                data.StartDate = _data.ExhibitionDateStart;
                data.EndDate = _data.ExhibitionDateEnd;
                data.Color = parent.UserInfo.CalColor;
                data.RelationId = _data.ExhibitionCode;
                data.GroupMembers = '';
                data.Importment = 'M';
                data.OpenMent = 'C';
                data.AllDay = '0';

                g_api.ConnectLite('Calendar', ComFn.GetAdd, data,
                    function (res) {
                        if (res.RESULT) {
                            var sNo = res.DATA.rel;
                            if (parent.Outklook) {
                                outlookAPI(outlook.Calendar_Add, {
                                    NO: sNo,
                                    ResponseRequested: false
                                });
                            }
                        }
                    });
            },
            /**
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":
                        $('#file_hidden').val($('li.jFiler-item').length || '');
                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return;
                        }
                        

                        if (sAction === 'Add') {
                            fnCheckNameThenAction('add');//fnAdd('add');
                        }
                        else {
                            fnCheckNameThenAction('upd');//fnUpd();
                        }

                        break;
                    case "Toolbar_ReAdd":

                        $('#file_hidden').val($('li.jFiler-item').length || '');
                        if (!$("#form_main").valid()) {
                            oValidator.focusInvalid();
                            return;
                        }
                        fnCheckNameThenAction('readd'); // fnAdd('readd');

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
                    case "Toolbar_Imp":

                        break;
                    case "Toolbar_Transfer":

                        fnTransfer();

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
                var saCusBtns = null;
                $('#State,#Industry,#ExhibitionDate').prop('required', true);
                $('.ShowNames').hide();
                if (parent.OrgID === 'TG') {
                    $('#CostRulesId,#State,#ExhibitionAddress,#ExhibitionDate,#file_hidden').prop('required', true);
                    $('#notte').show();
                    $('.simp-box').hide();
                }
                else {
                    $('.costrules').hide();
                }
                if (sAction === 'Upd') {
                    saCusBtns = [{
                        id: 'Toolbar_Transfer',
                        value: 'common.Toolbar_Transfer'// ╠common.Toolbar_Transfer⇒拋轉╣
                    }];
                }

                commonInit({
                    PrgId: sProgramId,
                    ButtonHandler: fnButtonHandler,
                    Buttons: saCusBtns,
                    GoTop: true,
                    tabClick: function (el) {
                        switch (el.id) {
                            case 'litab2':
                                if (!$(el).data('action')) {
                                    fnGetGridData().done(function () {
                                        oGrid.loadData();
                                        if (saGridData.length > 0 && (parent.UserInfo.roles.indexOf('Admin') > -1 || parent.UserInfo.roles.indexOf('Manager') > -1)) {
                                            $('.export').show();
                                            $('#Export_CusList').on('click', function () {
                                                fnGetGridData('export');
                                            });
                                        }
                                    });
                                }
                                break;
                            case 'litab3':
                                if (!$(el).data('action')) {
                                    oGrid1.loadData();
                                }
                                break;
                        }
                        $(el).data('action', true);
                    }
                });

                $.whenArray([
                    fnGet(),
                    setStateDrop(),
                    setExhibitionAddressDrop(),
                    setCostRulesDrop(),
                    fnSetArgDrop([
                        {
                            OrgID: 'TE',
                            ArgClassID: 'ExhibClass',
                            Select: $('#Industry'),
                            ShowId: true
                        }
                    ])
                ])
                    .done(function (res) {
                        if (res && res[0].RESULT === 1) {
                            var oRes = res[0].DATA.rel,
                                sDateRange = '';
                            oCurData = oRes;
                            oCurData.LogoFileId = oCurData.LogoFileId || guid();
                            setFormVal(oForm, oCurData);
                            if (oCurData.ExhibitionDateStart) {
                                sDateRange = newDate(oCurData.ExhibitionDateStart, 'date', true) + ' ~ ' + newDate(oCurData.ExhibitionDateEnd, 'date', true);
                            }
                            if (sTab) {
                                $('#litab3 a').click();
                            }
                            if (oRes.IsShowWebSite === 'N') {
                                $('.showwebsite').slideUp();
                            }
                            $('#State').val(oRes.State).trigger('change');
                            $('#ExhibitionDate').val(sDateRange);
                            $('#ExhibitionAddress_CN').text(oCurData.ExhibitionAddress_CN);
                            $('#ExhibitionAddress_EN').text(oCurData.ExhibitionAddress_EN);
                            setNameById().done(function () {
                                getPageVal();//緩存頁面值，用於清除
                            });
                            fnGetUploadFiles(oCurData.LogoFileId, fnUpload);
                        }
                        select2Init();
                        $('[name=IsShowWebSite]').click(function () {
                            if (this.value === 'N') {
                                $('#ExhibitionDate,#CostRulesId,#ExhibitionAddress,#file_hidden').removeAttr('required');
                                $('.showwebsite').slideUp();
                            }
                            else {
                                $('#ExhibitionDate,#CostRulesId,#ExhibitionAddress,#file_hidden,#CostRulesId').attr('required', true);
                                $('.showwebsite').slideDown();
                            }
                        });

                        //是否顯示於網站，值為'N'收起選項與移除required
                        if (oCurData.IsShowWebSite === 'N') {
                            $('#sub_box1').slideUp();
                            $('[name=IsShowWebSite][value="N"]').click();
                        }
                    });

                oValidator = $("#form_main").validate({
                    ignore: ''
                });

                $('#ExhibitionDate').dateRangePicker(
                    {
                        language: 'zh-TW',
                        separator: ' ~ ',
                        format: 'YYYY/MM/DD',
                        autoClose: true
                    }).bind('datepicker-change', function (e, r) {
                        try {
                            var dShelfTime_Home = r.date2.dateAdd('d', -15),
                                dShelfTime_Abroad = new Date(newDate(r.date2, 'date')).dateAdd('d', -15);
                            $('#ShelfTime_Home').val(newDate(dShelfTime_Home, 'date'));
                            $('#ShelfTime_Abroad').val(newDate(dShelfTime_Abroad, 'date'));
                        } catch (e) { }
                    });

                $('#Exhibitioname_TW').on('blur', function () {
                    $('#Exhibitioname_CN').val(simplized(this.value));
                });
                $('#Exhibitioname_CN').on('blur', function () {
                    $('#Exhibitioname_TW').val(traditionalized(this.value));
                });
                var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 150;
                $("#jsGrid").jsGrid({
                    width: "100%",
                    height: "auto",
                    autoload: true,
                    filtering: false,
                    pageLoading: true,
                    inserting: false,
                    editing: false,
                    sorting: false,
                    paging: false,
                    pageIndex: 1,
                    pageSize: parent.SysSet.GridRecords || 10,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    rowClick: function (args) {
                        if (navigator.userAgent.match(/mobile/i)) {
                            goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                        }
                    },
                    rowDoubleClick: function (args) {
                        parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                    },
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {
                            name: "AgentCName", title: 'ExhibitionImport_Upd.Agent', width: 200
                        },
                        {
                            name: "CustomerCName", title: 'Customers_Upd.CustomerCName', width: 200
                        },
                        {
                            name: "CustomerEName", title: 'Customers_Upd.CustomerEName', width: 200
                        },
                        {
                            name: "ContactorName", title: 'common.Contactor', width: 150, align: "center"
                        },
                        {
                            name: "Telephone", title: 'common.Telephone', width: 150
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return {
                                data: saGridData,
                                itemsCount: saGridData.length //data.length
                            };
                        },
                        insertItem: function (args) {
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
                $("#jsGrid1").jsGrid({
                    width: "100%",
                    height: iHeight,
                    autoload: false,
                    filtering: true,
                    pageLoading: true,
                    inserting: true,
                    editing: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: parent.SysSet.GridRecords || 10,
                    pageButtonCount: parent.SysSet.GridPages || 15,
                    confirmDeleting: true,
                    deleteConfirm: i18next.t('message.ConfirmToDelete'),// ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    rowClick: function (args) {
                        //if (navigator.userAgent.match(/mobile/i)) {
                        //    goToEdit('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                        //}
                    },
                    rowDoubleClick: function (args) {
                        //parent.openPageTab('Customers_Upd', '?Action=Upd&guid=' + args.item.guid);
                    },
                    fields: [
                        {
                            name: "RowIndex", title: 'common.RowNumber', width: 50, align: "center"
                        },
                        {// ╠Exhibition_Upd.ExhibitionArea⇒展區╣
                            name: "ExhibitionArea", title: 'Exhibition_Upd.ExhibitionArea', type: 'text', width: 100
                        },
                        {
                            name: "MuseumMumber", title: 'ExhibitionImport_Upd.HallMuseumMumber', type: 'text', width: 80
                        },
                        {
                            name: "CustomerCName", title: 'Customers_Upd.CustomerCName', type: 'text', width: 140,
                            validate: [
                                {
                                    validator: 'required',
                                    message: i18next.t('Customers_Upd.CustomerCName_required')// ╠Customers_Upd.CustomerCName_required⇒請輸入公司中文名稱╣
                                },
                                {
                                    validator: function (value, item) {
                                        var bRetn = true;
                                        CallAjax(ComFn.W_Com, ComFn.GetCount, {
                                            Params: {
                                                importcustomers: {
                                                    CustomerCName: value,
                                                    OrgID: parent.top.OrgID,
                                                    ExhibitionNO: sDataId,
                                                    guid: '<>' + item.guid
                                                }
                                            }
                                        }, function (rq) {
                                            if (rq.d > 0) {
                                                bRetn = false;
                                            }
                                        }, null, true, false);
                                        return bRetn;
                                    },
                                    message: i18next.t("message.CustomerCNameExist")  // ╠message.CustomerCNameExist⇒該廠商名稱已存在╣
                                }]
                        },
                        {
                            name: "CustomerEName", title: 'Customers_Upd.CustomerEName', type: 'text', width: 140
                        },
                        {
                            name: "UniCode", title: 'Customers_Upd.UniCode', type: 'text', width: 100,
                            validate: [
                                {
                                    validator: function (_val) {
                                        return _val === '' ? true : _val.length === 8;
                                    },
                                    message: i18next.t('message.UniCodeLength')
                                }
                            ]
                        },
                        {
                            name: "Contactor", title: 'common.Contactor', type: 'text', width: 120,
                            validate: { validator: 'required', message: i18next.t('common.Contactor_required') }// ╠common.Contactor_required⇒請輸入聯絡人╣
                        },
                        {
                            name: "Telephone", title: 'common.Telephone', type: 'text', width: 100, align: "center",
                            validate: { validator: 'required', message: i18next.t('common.Telephone_required') }// ╠common.Telephone_required⇒請輸入聯絡電話╣
                        },
                        {
                            name: "Email", title: 'common.Email', type: 'text', width: 130,
                            validate: {
                                validator: function (_val) {
                                    return _val === '' ? true : isEmail(_val);
                                },
                                message: i18next.t('message.IncorrectEmail'),// ╠message.IncorrectEmail⇒郵箱格式不正確╣
                            }
                        },
                        {
                            name: "Address", title: 'common.Address', type: 'text', width: 100
                        },
                        {
                            name: "Memo", title: 'common.Memo', type: 'text', width: 100
                        },
                        {// ╠common.IsAppoint⇒預約狀態╣
                            name: "IsAppoint", title: 'common.IsAppoint', width: 60, align: "center",
                            itemTemplate: function (val, item) {
                                return val === 'Y' ? $('<span />', { text: i18next.t('common.HasAppoint') }).css('color', 'green') : $('<span />', { text: i18next.t('common.NotAppoint') }).css('color', 'red');// ╠common.HasAppoint⇒已預約╣ ╠common.NotAppoint⇒未預約╣
                            }
                        },
                        {// ╠common.IsFormal⇒資料狀態╣
                            name: "guid",
                            title: 'common.IsFormal',
                            width: 140,
                            align: 'center',
                            itemTemplate: function (val, item) {
                                var saAction = [];
                                if (!item.IsFormal) {
                                    saAction.push($('<a/>', {
                                        html: i18next.t('common.TransferToFormal'),// ╠common.TransferToFormal⇒轉為正式客戶╣
                                        class: 'link',
                                        click: function () {
                                            fnTransferToFormal(item);
                                            return false;
                                        }
                                    }));
                                    if (parent.UserInfo.roles.indexOf('Admin') > -1) {
                                        saAction.push('   ', $('<a/>', {
                                            html: i18next.t('common.CorrespondFormalCus'),// ╠common.CorrespondFormalCus⇒對應到正式客戶╣
                                            class: 'link',
                                            style: 'color:green !important',
                                            click: function () {
                                                fnCorrespondFormalCus(item);
                                            }
                                        }));
                                    }
                                }
                                else {
                                    saAction.push($('<span />', { text: i18next.t('common.HasFormal') }).css('color', 'green'));
                                }
                                return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(saAction);
                            }
                        },
                        {
                            type: "control",
                            width: 100,
                            headerTemplate: function () {
                                var saBtn = [];
                                if (sAction === 'Upd') {
                                    saBtn.push($('<button />', {
                                        type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Imp'), html: '<i class="glyphicon glyphicon-import"></i>', click: function () {
                                            fnImportCusList();
                                        }
                                    }));
                                    saBtn.push($('<button />', {
                                        type: 'button', 'class': 'btn-custom blue', title: i18next.t('common.Toolbar_Exp'), html: '<i class="glyphicon glyphicon-export"></i>', click: function () {
                                            fnExportCusList();
                                        }
                                    }));
                                }
                                return saBtn;
                            },
                            deleteButton: false
                        }
                    ],
                    controller: {
                        loadData: function (args) {
                            return fnGetGridData1(args);
                        },
                        insertItem: function (args) {
                            fnAddCustomers(args);
                        },
                        updateItem: function (args) {
                            fnUpdCustomers(args);
                        },
                        deleteItem: function (args) {
                        }
                    },
                    onInit: function (args) {
                        oGrid1 = args.grid;
                    }
                });
            };

        init();
    },
    /**
     * 客戶轉為正式資料后動作
     * @param   {String}afterid 客戶id
     */
    fnReFresh = function (afterid) {
        bRefresh = true;
        $('iframe').attr('src', '../Crm/Customers_Upd.html?Action=Upd&Flag=Fit&guid=' + afterid);
    };

require(['base', 'select2', 'jsgrid', 'daterangepicker', 'convetlng', 'filer', 'ajaxfile', 'util'], fnPageInit, 'daterangepicker');