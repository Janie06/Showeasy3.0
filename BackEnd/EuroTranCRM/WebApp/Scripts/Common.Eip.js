'use strict';

var
    /**
    * 獲取文件list
    * @param {Object} handle  jquery dom 對象
    * @param {String} parentid 文件來源id
    * @param {String} oldid 退件文件來源id
    * @param {String} oldtext 退件文件來源id
    * @param {String} goprg 退件文件來源id
    * @return {Object} Ajax 物件 ExFeild1
    */
    fnGetFiles = function (handle, parentid, oldid, oldtext, goprg) {
        var callback = function (files) {
            var saFiles = files,
                sHtml = '';
            $.each(saFiles, function (idx, file) {
                if (file.fileid) {
                    var icon = 'default';
                    file.subname = file.subname.toLowerCase();

                    if ("doc|xls|txt|exe|mp3|mv|pdf|ppt|psd|".indexOf(file.subname) > -1) {
                        icon = file.subname;
                    }
                    else if ("png|jpg|jpeg|gif|bmp".indexOf(file.subname) > -1) {
                        icon = 'jpg';
                    }
                    else if ("7z|zip|rar".indexOf(file.subname) > -1) {
                        icon = 'rar';
                    }
                    else if ("docx|doc".indexOf(file.subname) > -1) {
                        icon = 'doc';
                    }
                    else if ("xls|xlsx".indexOf(file.subname) > -1) {
                        icon = 'xls';
                    }
                    else if ("pptx".indexOf(file.subname) > -1) {
                        icon = 'ppt';
                    }
                    sHtml += '<p style="line-height: 15px;"><img src="https://www.eurotran.com:9001/Ueditor/dialogs/attachment/fileTypeImages/icon_' + icon + '.gif"><a style="text-decoration: underline !important;" href="javascript:void(0);" title="' + file.name + '" path="' + file.path + '" subname="' + icon + '"><span style="font-size: 12px;">' + file.name + '</span></a></p>';

                }
            });
            if (oldid) {
                sHtml += '<p style="line-height: 15px;"><img src="https://www.eurotran.com:9001/Ueditor/dialogs/attachment/fileTypeImages/icon_txt.gif"><a style="text-decoration: underline !important;" href="javascript:void(0);" title="' + oldtext + '"oldid="' + oldid + '"goprg="' + goprg + '"><span style="font-size: 12px;">' + oldtext + '</span></a></p>';
            }
            handle.append(sHtml).find('a').on('click', function () {
                var sPath = $(this).attr('path'),
                    sFileName = $(this).text().split('.')[0],
                    sSubName = $(this).attr('subname'),
                    sPrg = $(this).attr('goprg'),
                    sOldid = $(this).attr('oldid');
                if (sPrg) {
                    parent.layer.open({
                        type: 2, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                        title: i18next.t('common.Info'), //不显示标题栏
                        area: ['900px', '600px'],
                        shade: false,//遮罩
                        closeBtn: 1,
                        shadeClose: true,
                        maxmin: true, //开启最大化最小化按钮
                        offset: '100px',//右下角弹出
                        anim: 0,//彈出動畫
                        content: "/Page/Eip/" + sPrg + ".html?Action=Upd&Guid=" + sOldid,
                        success: function (layero) {
                            layero.find('iframe').contents().find('#Toolbar').hide();
                        }
                    });
                }
                else {
                    if ("pdf|jpg".indexOf(sSubName) > -1) {
                        var sUrl = gServerUrl + '/' + sPath;
                        window.open(sUrl);
                    }
                    else {
                        DownLoadFile(sPath, sFileName);
                    }
                }
            });
        };
        return fnGetUploadFiles(parentid, callback);
    },
    /**
    * 打印
    * @param {Object} handle  jquery dom 對象
    */
    fnPrePrint = function (handle) {
        $(':input,select').not('[type="button"],[type="radio"]').each(function () {
            var parentDom = $(this).parent();
            if (!parentDom.attr('printele')) {
                var sOldrHTML = '',
                    sPrintrHTML = '';
                parentDom.find(':input,select').each(function () {
                    $(this).attr('value', this.value);
                    var thisHtml = this.outerHTML,
                        printHtml = $('<div>', { class: 'show-text' });
                    switch (this.type) {
                        case 'text':
                        case 'number':
                        case 'textarea':
                            printHtml.html(this.value);
                            break;
                        case 'select':
                        case 'select-one':
                            printHtml.html($(this).find('option:selected').text());
                            break;
                    }
                    sOldrHTML += thisHtml;
                    sPrintrHTML += printHtml[0].outerHTML;
                });
            }
            parentDom.attr('printele', sOldrHTML).html(sPrintrHTML);
        });

        handle.jqprint({ operaSupport: false });
        $('[printele]').each(function () {
            var oldHtml = $(this).attr('printele');
            $(this).html(oldHtml).removeAttr('printele').find(':input,select').each(function () {
                this.value = $(this).attr('value');
            });
        });
    },
    /**
    * 處理簽核流程
    * @param {Object} data 當前資料
    * @param {Boolean} flag 當前資料
    * @param {Boolean} isstrfy 當前資料
    * @param {Array} users 當前資料
    * @return {Object} 返回流程字串或json對象
    */
    fnCheckFlows = function (data, flag, isstrfy, users) {
        var Applicant = data.Applicant ? data.Applicant : data.AskTheDummy;
        var saCheckFlows = [],
            iOrder = 0;
        if (flag && data.Agent_Person) {
            iOrder++;
            var oCurUser = $.grep(users, function (cur) { return cur.MemberID === data.Agent_Person; })[0],
                oFlow = {};
            oFlow.ParentId = '0';
            oFlow.FlowId = guid();
            oFlow.Order = iOrder;
            oFlow.SignedWay = 'flow1';
            oFlow.SignedId = data.Agent_Person;
            oFlow.SignedMember = oCurUser.MemberName;
            oFlow.Department = oCurUser.DepartmentName;
            oFlow.Jobtitle = oCurUser.JobtitleName;
            oFlow.SignedDecision = '';
            oFlow.SignedOpinion = '';
            oFlow.SignedDate = '';
            saCheckFlows.push(oFlow);
        }
        var ShiftOrder = 0;
        $.each(data.CheckOrder, function (idx, order) {

            if (flag && idx === 0) {
                var saExsit = Jsonget(order.SignedMember, 'id', data.Agent_Person);
                if (saExsit.length > 0) {
                    iOrder--;
                    saCheckFlows = [];
                }
            }

            $.each(order.SignedMember, function (i, _user) {
                if (_user.id !== Applicant) {
                    let intOrder = parseInt(order.Order);
                    var oFlow = {};
                    oFlow.ParentId = order.id;
                    oFlow.FlowId = guid();
                    oFlow.Order = intOrder + iOrder + ShiftOrder;
                    oFlow.SignedWay = order.SignedWay;
                    oFlow.SignedId = _user.id;
                    oFlow.SignedMember = _user.name;
                    oFlow.Department = _user.deptname;
                    oFlow.Jobtitle = _user.jobname;
                    oFlow.SignedDecision = '';
                    oFlow.SignedOpinion = '';
                    oFlow.SignedDate = '';
                    if (order.SignedMember.length > 1 && i !== order.SignedMember.length - 1) {
                        oFlow.Line = true;
                    }
                    if (i === 0) {
                        oFlow.Icon = true;
                    }
                    saCheckFlows.push(oFlow);
                }
                else if (order.SignedWay == "flow1") {
                    --ShiftOrder;
                }
            });
        });
        return isstrfy ? JSON.stringify(saCheckFlows) : saCheckFlows;
    },
    /**
    *處理簽辦流程
    *@method fnHandleFlows
    *@param {Object} data 當前資料
    *@param {Array} users 當前資料
    *@return {Object} 返回流程字串或json對象
    */
    fnHandleFlows = function (data, users) {
        var saHandleFlows = [],
            iOrder = 1;
        if (data.Handle_Person) {
            var oCurUser = $.grep(users, function (cur) { return cur.MemberID === data.Handle_Person; })[0],
                oFlow = {};
            oFlow.FlowId = guid();
            oFlow.Order = iOrder;
            oFlow.SignedWay = 'flow1';
            oFlow.SignedId = data.Handle_Person;
            oFlow.SignedMember = oCurUser.MemberName;
            oFlow.Department = oCurUser.DepartmentName;
            oFlow.Jobtitle = oCurUser.JobtitleName;
            oFlow.SignedDecision = 'N';
            oFlow.SignedOpinion = '';
            oFlow.SignedDate = '';
            saHandleFlows.push(oFlow);
        }
        return JSON.stringify(saHandleFlows);
    },
    /**
     * 合併通知
     * @param {Array} list Grid list
     * @return {Array} list New Grid list
     */
    releaseGridList = function (list) {
        var saIndex = [],
            saList_New = [];
        list = Enumerable.From(list).OrderBy("$.Order").ToArray();
        $.each(list, function (idx, _data) {
            if (saIndex.indexOf(idx) === -1) {
                var nextdata_1 = list[idx + 1],
                    nextdata_2 = list[idx + 2],
                    bTwo = false;
                if (nextdata_1 && nextdata_1.SignedWay === _data.SignedWay && _data.SignedWay === 'flow4') {
                    _data.SignedMember = _data.SignedMember.concat(nextdata_1.SignedMember);
                    saIndex.push(idx + 1);
                    bTwo = true;
                }
                if (bTwo && nextdata_2 && nextdata_2.SignedWay === _data.SignedWay && _data.SignedWay === 'flow4') {
                    _data.SignedMember = _data.SignedMember.concat(nextdata_2.SignedMember);
                    saIndex.push(idx + 2);
                }
                saList_New.push(_data);
            }
        });
        $.each(saList_New, function (idx, _data) {
            _data.Order = idx + 1;
        });
        return saList_New;
    },
    /**
    * 開啟複選選單Pop
    * @param  {Object} option 配置
    */
    oPenUserListPop = function (option) {
        option = option || {};
        var oGrid = null,
            saUserList = [],// ╠common.Filter⇒過濾╣
            sContent = '<style>.btn-xs {padding: 1px 5px; }</style>\
                        <div class="row popsrow">\
                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.Department">部門</span>：</label>\
                             <div class="col-sm-6">\
                                 <select class="form-control" id="Department"></select>\
                             </div>\
                        </div>\
                        <div class="row popsrow">\
                             <label class="col-sm-3 control-label wright" for="input-Default"><span data-i18n="common.Filter">過濾</span>：</label>\
                             <div class="col-sm-6">\
                                 <input type="text" class="form-control w100p" id="Filter" maxlength="50">\
                             </div>\
                        </div>\
                        <div class="row popsrow">\
                            <div class="col-sm-2"></div>\
                            <div class="col-sm-8">\
                                <table style="width: 100%">\
                                    <tr>\
                                        <td class="w50p">\
                                            <select size="10" name="lstLeft" id="lstLeft" class="form-control w100p" multiple></select>\
                                        </td>\
                                        <td>\
                                            <p>\
                                                <button type="button" id="btnToRight" class="btn-custom btn-xs"><i class="fa fa-chevron-right"></i></button>\
                                            </p>\
                                            <p>\
                                                <button type="button" id="btnToLeft" class="btn-custom btn-xs"><i class="fa fa-chevron-left"></i></button>\
                                            </p>\
                                        </td>\
                                        <td class="w50p">\
                                            <select size="10" name="lstRight" id="lstRight" class="form-control w100p" multiple></select>\
                                        </td>\
                                        <td>\
                                            <p>\
                                                <button type="button" id="btnToUp" class="btn-custom btn-xs"><i class="fa fa-chevron-up"></i></button>\
                                            </p>\
                                            <p>\
                                                <button type="button" id="btnToDown" class="btn-custom btn-xs"><i class="fa fa-chevron-down"></i></button>\
                                            </p>\
                                        </td>\
                                    </tr>\
                                </table>\
                            </div>\
                            <div class="col-sm-2"></div>\
                        </div>\
                        <div class="row popsrow">\
                            <div class="col-sm-2"></div>\
                            <div id="flowtype">\
                            </div>\
                        </div>';
        layer.open({
            type: 1,
            title: i18next.t('common.SelectUsers'),//╠common.SelectUsers⇒選取人員╣
            shadeClose: false,
            shade: 0.1,
            maxmin: true, //开启最大化最小化按钮
            area: ['500px', '460px'],
            content: sContent,
            success: function (layero, index) {
                var saFlowTypes = [
                    { id: 'flow1', text: i18next.t('common.flow1') },//╠common.flow1⇒串簽╣
                    { id: 'flow2', text: i18next.t('common.flow2') },//╠common.flow2⇒會辦╣
                    { id: 'flow3', text: i18next.t('common.flow3') },//╠common.flow3⇒擇辦╣
                    { id: 'flow4', text: i18next.t('common.flow4') }];//╠common.flow4⇒通知╣
                layero.find('#flowtype').html(createRadios(saFlowTypes, 'id', 'text', 'flowtype')).find('label:first').click();
                if (option.SignedWay) {
                    layero.find('#flowtype [value=' + option.SignedWay + ']').click();
                }
                if (option.Flowtype) {
                    layero.find('#flowtype').hide();
                    layero.find('#lstLeft,#lstRight').attr('size', 12);
                }
                fnSetDeptDrop(layero.find('#Department'));

                fnSetUserDrop([{
                    Select: layero.find('#lstLeft'),
                    Action: 'add',
                    ShowId: true,
                    CallBack: function (data) {
                        saUserList = data;
                        layero.find('#lstLeft')[0].remove(0);
                        optionListSearch(layero.find('#lstLeft'), layero.find('#lstRight'), layero.find('#Filter'));
                        if (option.SignedMember) {
                            option.SignedMember = Enumerable.From(option.SignedMember).Distinct("$=>$.id").ToArray();
                            layero.find('#lstRight').html(createOptions(option.SignedMember, 'id', 'name'));
                            layero.find('#lstRight')[0].remove(0);
                            $.each(option.SignedMember, function (idx, _user) {
                                layero.find('#lstLeft').find('option[value="' + _user.id + '"]').remove();
                            });
                        }
                    }
                }]);
                layero.find('#Department').on('change', function () {
                    var sDeptId = this.value;
                    fnSetUserDrop([{
                        Select: layero.find('#lstLeft'),
                        Action: 'add',
                        DepartmentID: sDeptId,
                        CallBack: function (data) {
                            layero.find('#lstLeft')[0].remove(0);
                            optionListSearch(layero.find('#lstLeft'), layero.find('#lstRight'), layero.find('#Filter'));
                        }
                    }]);
                });
                layero.find('#btnToRight').on('click', function () {
                    optionListMove(layero.find('#lstLeft'), layero.find('#lstRight'));
                });
                layero.find('#btnToLeft').on('click', function () {
                    optionListMove(layero.find('#lstRight'), layero.find('#lstLeft'));
                });
                layero.find('#btnToUp').on('click', function () {
                    optionListOrder(layero.find('#lstRight'), true);
                });
                layero.find('#btnToDown').on('click', function () {
                    optionListOrder(layero.find('#lstRight'), false);
                });
            },
            btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Confirm⇒確定╣╠common.Cancel⇒取消╣
            yes: function (index, layero) {
                var oRetn = {},
                    saUsers = [],
                    sFlowtype = layero.find('#flowtype [name=flowtype]:checked').val();
                layero.find('#lstRight option').each(function () {
                    var userid = $(this).val(),
                        oCurUser = Enumerable.From(saUserList).Where(function (e) { return e.MemberID === userid; }).First();
                    let Name = $(this).text().split("-")[1];
                    saUsers.push({
                        id: $(this).val(),
                        name: Name,
                        deptname: oCurUser.DepartmentName,
                        jobname: oCurUser.JobtitleName
                    });
                });
                oRetn.Users = saUsers;
                oRetn.FlowType = sFlowtype || '';
                if (typeof option.Callback === 'function') option.Callback(oRetn);
                layer.close(index);
            },
            cancel: function () {
                if (typeof option.CancelCallback === 'function') option.CancelCallback();
            }
        });
    },
    /**
    * 設定簽核流程下拉選單
    * @param {Object} drop 當前控件
    * @return {Object} Ajax 物件
    */
    fnSetFlowDrop = function (drop) {
        return g_api.ConnectLite(Service.eip, 'GetFlows',
            {
                Flow_Type: drop.Flow_Type || '',
                ShareTo: drop.ShareTo || ''
            },
            function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    if (drop.Select) {
                        drop.Select.html(createOptions(saRes, 'Guid', 'Flow_Name', drop.ShowId || false));
                        if (drop.DefultVal) {
                            drop.Select.val(drop.DefultVal);
                        }
                        if (drop.Select2) {
                            drop.Select.each(function () {
                                $(this).select2();
                                $(this).next().after($(this));
                            });
                        }
                    }
                    if (drop.CallBack && typeof drop.CallBack === 'function') {
                        drop.CallBack(saRes);
                    }
                }
            });
    },
    /**
    * 獲取賬單資料
    * @param {Object} o 參數
    * @return {Object} Ajax 物件
    */
    fnGetBills = function (o) {
        return g_api.ConnectLite(Service.opm, 'GetBills',
            {
                BillNO: o.BillNO || ''
            },
            function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    if (o.CallBack && typeof o.CallBack === 'function') {
                        o.CallBack(saRes);
                    }
                }
            });
    },

    /**
     * 根據帳單狀態，移除抽單按鈕(扣除已經辦只能admin抽單)
     * @param {Object} status 帳單狀態
     * @param {Object} applicant 申請者
     */
    fnCheckReEdit = function (status, applicant) {
        let RemoveReEdit = false;
        let AdminUser = parent.UserInfo.roles.indexOf('Admin') > -1;
        switch (status) {
            case "A":
            case "B":
            case "E":
                if (applicant !== parent.UserID) {
                    RemoveReEdit = true;
                }
                break;
            case "C-O":
            case "D-O":
            case "X":
                //不能抽單狀況:已抽單、已退件、已作廢、已經辦。
                RemoveReEdit = true;
                break;
            case "H-O":
                //"已經辦"(H-O)，僅超級管理員可
                if (AdminUser) {
                    RemoveReEdit = false;
                }
                else {
                    RemoveReEdit = true;
                }
                break;
            default:
                RemoveReEdit = true;
                break;
        }

        //調整抽單鈕
        if (RemoveReEdit)
            $('#Toolbar_ReEdit').remove();
    },

    /**
     * 重新取得新的流程，再執行複製
     *@param {Object} oCurData 目前的資料
     *@param {Object} CopyFn 流程更新後執行的Function
     */
    fnRefreshFlowsThenCopy = function (oCurData, CopyFn) {
        CallAjax(ComFn.W_Com, ComFn.GetOne, {
            Type: '',
            Params: {
                checkflow: {
                    Guid: oCurData.FlowId
                }
            }
        }, function (res) {
            if (res.d) {
                //取得新的flow資料 start
                var oRes = $.parseJSON(res.d);
                // ╠message.RefreshFlowsThenCopy_Failed_FlowsNotFound⇒複製失敗，找不到原始對應流程單據╣
                if (oRes.ShareTo === null)
                    showMsg(i18next.t("message.RefreshFlowsThenCopy_Failed_FlowsNotFound"), 'error');
                else if (oRes.ShareTo.indexOf(parent.UserID) === -1)
                    // ╠message.RefreshFlowsThenCopy_Failed_FlowsNotFound⇒複製失敗，沒有使用該流程權限。╣
                    showMsg(i18next.t("message.RefreshFlowsThenCopy_Failed_FlowsAccessDenied"), 'error');
                else {
                    oRes.Flows = $.parseJSON(oRes.Flows);
                    oCurData.CheckOrder = oRes.Flows;
                    oCurData.Flows_Lock = oRes.Flows_Lock;
                    oCurData.Handle_Lock = oRes.Handle_Lock;
                    oCurData.Handle_Person = oRes.Handle_Person;
                    oCurData.Handle_DeptID = oRes.Handle_DeptID;
                    CopyFn();
                }
                //取得新的flow資料 end
            }
            // ╠message.RefreshFlowsThenCopy_Failed_SearchingFlowsError⇒複製失敗，搜尋時發生錯誤。╣
            else {
                showMsg(i18next.t("message.RefreshFlowsThenCopy_Failed_SearchingFlowsError"), 'error');
            }
        });
};