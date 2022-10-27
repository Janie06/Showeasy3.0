'use strict';
var fnPageInit = function () {
    var saArrOrderBy = [],
        canDo = new CanDo({
            /**
             * 當前程式所有ID名稱集合
             */
            idKeys: ['OrgID', 'ProgramID'],
            /**
             * 當前程式所有參數名稱集合
             */
            paramKeys: ['ProgramID'],
            /**
             * 客製化驗證規則
             * @param  {Object} pargs CanDo 對象
             */
            validRulesCus: function (pargs) {
                $.validator.addMethod("programidrule", function (value) {
                    var bRetn = true;
                    if (value) {
                        g_api.ConnectLite(pargs.ProgramId, pargs._api.getcout,
                            {
                                ProgramID: value,
                            },
                            function (res) {
                                if (res.RESULT && res.DATA.rel > 0) {
                                    bRetn = false;
                                }
                            }, null, false);
                    }
                    return bRetn;
                });
            },
            /**
             * 驗證規則
             */
            validRules: function (pargs) {
                return {
                    ignore: '',
                    onfocusout: false,
                    rules: {
                        ProgramID: { programidrule: pargs.action === 'add' ? true : false },
                    },
                    messages: {
                        ProgramID: { programidrule: i18next.t("message.Data_Repeat") }// ╠message.Data_Repeat⇒此筆資料已建檔╣
                    }
                };
            },
            /**
             * 處理新增資料參數
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前表單資料
             */
            getInsertParams: function (pargs, data) {
                data.ImgPath = '';
                data.ShowTop = data.ShowTop || false;
                data.GroupTag = data.ProgramID.split('_')[0];
                data.AllowRight = fnGetRights();
                data.updorder = [];
                $.each(saArrOrderBy, function (idx, order) {
                    var saOrders = order.split(';');
                    data.updorder.push({
                        OldOrderByValue: saOrders[1],
                        NewOrderByValue: saOrders[2],
                        ModuleID: saOrders[0]
                    });
                });
                return data;
            },
            /**
             * 處理修改資料參數
             * @param  {Object} pargs CanDo 對象
             * @param  {Object} data 當前表單資料
             */
            getUpdateParams: function (pargs, data) {
                data = pargs.options.getInsertParams(pargs, data);
                return data;
            },
            /**
             * 頁面初始化
             * @param  {Object} pargs CanDo 對象
             */
            pageInit: function (pargs) {
                if (pargs.action === 'upd') {
                    $('#ProgramID').prop('disabled', true);
                    pargs._getOne().done(function (res) {
                        var saActins = res.DATA.actions,
                            saLeft = [],
                            saRight = [];
                        if (pargs.data.AllowRight) {
                            $.each(saActins, function (idx, item) {
                                if (('|' + $.trim(pargs.data.AllowRight || '') + '|').indexOf('|' + item.ArgumentID + '|') > -1) {
                                    saRight.push(item);
                                }
                                else {
                                    saLeft.push(item);
                                }
                            });

                            $('#lstLeft').html(createOptions(saLeft, 'ArgumentID', 'ArgumentValue'))[0].remove(0);
                            $('#lstRight').html(createOptions(saRight, 'ArgumentID', 'ArgumentValue'))[0].remove(0);
                        }
                        else {
                            $('#lstLeft').html(createOptions(saActins, 'ArgumentID', 'ArgumentValue'))[0].remove(0);
                        }
                        $('#BackgroundCSS').spectrum("set", pargs.data.BackgroundCSS);
                        fnJsPost();
                    });
                }
                else {
                    fnSetArgDrop([
                        {
                            ArgClassID: '99999',
                            CallBack: function (data) {
                                $('#lstLeft').html(createOptions(data, 'id', 'text'))[0].remove(0);
                            }
                        }
                    ]);
                }

                $('[name=Effective]').click(function () {
                    if (this.value == 'N') {
                        $('[name=ShowInList][value=N]').click();
                        $('[name=ShowInHome][value=N]').click();
                    }
                });

                $('[name=ShowInList],[name=ShowInHome]').click(function () {
                    if (this.value == 'Y' && $('[name=Effective]:checked').val() == 'N') {
                        showMsg(i18next.t('ProgramMaintain_Upd.PrgIsDisable'));//程式已停用,不能顯示於選單!
                        $('[name=ShowInList][value=N]').click();
                        $('[name=ShowInHome][value=N]').click();
                    }
                });

                $('#btnToRight').click(function () {
                    optionListMove($('#lstLeft'), $('#lstRight'));
                });
                $('#btnToLeft').click(function () {
                    optionListMove($('#lstRight'), $('#lstLeft'));
                });
                $('#btnData').click(function () {
                    var oConfig = {
                        Id: 'ModuleList',
                        Get: fnGetPopData,
                        SearchFields: [
                            { id: "Pop_ModuleID", type: 'text', i18nkey: 'common.ModuleID' },
                            { id: "Pop_ModuleName", type: 'text', i18nkey: 'common.ModuleName' }
                        ],
                        Fields: [
                            { name: "RowIndex", title: 'common.RowNumber', sorting: false, align: 'center', width: 50 },
                            { name: "ModuleID", title: 'common.ModuleID', width: 200 },
                            { name: "ModuleName", title: 'common.ModuleName', width: 200 }
                        ],
                        Callback: function (data) {
                            var saId = [],
                                saName = [];
                            $.each(data, function (idx, item) {
                                saId.push(item.ModuleID);
                                saName.push(item.ModuleName);
                            });
                            $('#ModuleID').val(saId.join(','));
                            fnJsPost();
                        }
                    };
                    oPenPopm(oConfig);
                });
            }
        }),
        /**
         * 獲取出口帳單資料
         * @param {Object}  args 查詢條件參數
         * @return {Object} Ajax 物件
         */
        fnGetPopData = function (args) {
            args = args || {};
            args.ModuleID = $('#Pop_ModuleID').val();
            args.ModuleName = $('#Pop_ModuleName').val();
            args.sortField = args.sortField || 'OrderByValue';
            args.sortOrder = args.sortOrder || 'asc';
            args.pageIndex = args.pageIndex || 1;
            args.pageSize = args.pageSize || 10;

            return g_api.ConnectLite('ModuleMaintain_Qry', canDo._api.getpage, args,
                function (res) {
                    if (res.RESULT) {
                        var iOrderByCount = res.DATA.rel;
                        if (canDo.action === 'add') {
                            iOrderByCount++;
                        }
                        $('#OrderByValue').html(createOptions(iOrderByCount));
                        if (canDo.action === 'add') {
                            $('#OrderByValue').val(iOrderByCount);
                        }
                    }
                });
        },
        /**
         * 獲取已選權限
         * @return {Array} 可用權限
         */
        fnGetRights = function () {
            var saRights = [];
            $('#lstRight option').each(function () {
                saRights.push(this.value);
            });
            return saRights.join('|');
        },
        /**
        * 目的：記錄舊的排序值
        */
        fnSetOrderBy = function (obj) {
            var saNewArrOrderBy = [];
            if (saArrOrderBy.length > 0) {
                for (var IntCount = 0; IntCount < saArrOrderBy.length; IntCount++) {
                    var ArrOrder_S = saArrOrderBy[IntCount].split(';');
                    if (ArrOrder_S[0] == obj.id) {
                        saNewArrOrderBy.push(ArrOrder_S[0] + ';' + ArrOrder_S[1] + ';' + $(obj).val());
                    }
                    else {
                        saNewArrOrderBy.push(saArrOrderBy[IntCount]);
                    }
                }
                saArrOrderBy = saNewArrOrderBy;
            }
        },
        /**
        * 加載模組資料
        */
        fnJsPost = function () {
            g_api.ConnectLite(canDo.ProgramId, 'GetModuleInfo', {
                OrgID: parent.OrgID,
                AllModuleID: $('#ModuleID').val(),
                ProgramID: $('#ProgramID').val()
            }, function (res) {
                if (res.RESULT) {
                    var JsonArray = res.DATA.rel,
                        $ul = $('.ModuleList');
                    $ul.html('');//先清空資料
                    saArrOrderBy = [];
                    var $lititle = $('<li />').html('<div class="col-sm-12"><div class="col-sm-8"><span>模組名稱</span> </div><div class="col-sm-4"><span>程式排序</span></div></div>');
                    $lititle.appendTo($ul);
                    for (var i = 0; i < JsonArray.length; i++) {
                        var $li = $('<li />'),
                            $div = $('<div class="col-sm-12" />'),
                            $div_L = $('<div class="col-sm-8" style="padding-top: 8px;" />'),
                            $div_R = $('<div class="col-sm-4" />'),
                            $span_L = $('<span />'),
                            $span_R = $('<span />'),
                            $select = $('<select />', { change: function () { fnSetOrderBy(this); } });
                        $span_L.css({ 'color': '#267eb1' }).html(JsonArray[i].ModuleName).appendTo($div_L);
                        $select.attr({ 'id': JsonArray[i].ModuleID, 'class': 'form-control w70' }).appendTo($div_R);
                        $div_L.appendTo($div);
                        $div_R.appendTo($div);
                        $div.appendTo($li);
                        if (canDo.action == 'add') {
                            var AllCount = parseInt(JsonArray[i].PrgCount) + 2;
                            for (IntCount = 1; IntCount < AllCount; IntCount++) {
                                var $option = $('<option />');
                                if (IntCount == AllCount - 1) {
                                    //選中
                                    $option.attr({ 'value': IntCount, 'selected': 'selected' }).html(IntCount).appendTo($select);
                                }
                                else {
                                    $option.attr('value', IntCount).html(IntCount).appendTo($select);
                                }
                            }
                            $li.appendTo($ul);
                            //記錄排序值Mod|OldOrderBy|NewOrderBy|
                            saArrOrderBy.push(JsonArray[i].ModuleID + ';' + (AllCount - 1).toString() + ';' + (AllCount - 1).toString());
                        }
                        else {
                            //獲取舊資料OrderBy
                            var JsonOrderBy = JsonArray[i].OrderByValue,
                                OrderBy = JsonOrderBy != null ? JsonOrderBy : '',//新增則取最大修改取OrderBy
                                selectCount = parseInt(JsonArray[i].PrgCount),
                                AllCount = 0,
                                IntCount;
                            OrderBy == '' ? (AllCount = selectCount + 2, OrderBy = selectCount + 1) : (AllCount = selectCount + 1, OrderBy = OrderBy);//新增+2，修改+1
                            for (IntCount = 1; IntCount < AllCount; IntCount++) {
                                var $option = $('<option />');
                                if (IntCount == OrderBy) {
                                    //選中
                                    $option.attr({ 'value': IntCount, 'selected': 'selected' }).html(IntCount).appendTo($select);
                                }
                                else {
                                    $option.attr('value', IntCount).html(IntCount).appendTo($select);
                                }
                            }
                            $li.appendTo($ul);
                            //記錄排序值Mod|OldOrderBy|NewOrderBy|
                            saArrOrderBy.push(JsonArray[i].ModuleID + ';' + OrderBy + ';' + OrderBy);
                        }
                    }
                }
            });
        };
};

require(['base', 'jsgrid', 'spectrum', 'cando'], fnPageInit);