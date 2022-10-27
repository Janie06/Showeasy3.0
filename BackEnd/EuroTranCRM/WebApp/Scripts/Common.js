'use strict';

var
    /**
    * 設定部門主管下拉選單
    * @param {Object} handle 當前控件
    * @param {String} deptid 部門id
    * @return {Object} Ajax 物件
    */
    fnSetDeptDrop = function (handle, deptid) {
        var oParm = {};
        oParm.OrgID = parent.OrgID;
        oParm.DeptID = deptid || '';

        return g_api.ConnectLite(Service.sys, 'GetDepartmentList', oParm,
            function (res) {
                if (res.RESULT) {
                    var saData = res.DATA.rel;
                    handle.html(createOptions(saData, 'DepartmentID', 'AccountNameSort', true));
                }
            });
    },
    /**
     * 設定部門主管下拉選單
     * @param {Object} handle 當前控件
     * @param {String} deptid 部門id
     * @return {Object} Ajax 物件
     */
    fnSetDeptDropWithLimtedDeptId = function (handle) {
        var oParm = {};
        oParm.OrgID = parent.OrgID;
        oParm.DeptID = '';
        return g_api.ConnectLite(Service.sys, 'GetDepartmentListNoVoid', oParm,
            function (res) {
                if (res.RESULT) {
                    //debugger;
                    var saData = res.DATA.rel;
                    var DepId = parent.UserInfo.DepartmentID;
                    var AdminUser = parent.UserInfo.roles.toLowerCase().indexOf('admin') > -1;
                    if (!AdminUser) {
                        saData = saData.filter(c => c.ParentDepartmentID === DepId || c.DepartmentID === DepId || c.DepartmentID1 === DepId);
                    }
                    handle.html(createOptions(saData, 'DepartmentID', 'AccountNameSort', true));
                    if (!AdminUser) {
                        $(handle.selector + ' option:first').remove();
                    }
                }
            });
    },
    /**
    * 設定人員下拉選單
    * @param {Object} drops 當前控件
    * @return {Object} Ajax 物件
    */
    fnSetUserDrop = function (drops) {
        var saPost = [];
        $.each(drops, function (index, drop) {
            drop.Effective = '';
            if (drop.Action && drop.Action.toLowerCase() === 'add') {
                drop.Effective = 'Y';
            }
            saPost.push(g_api.ConnectLite(Service.com, ComFn.GetUserList,
                {
                    DepartmentID: drop.DepartmentID || '',
                    MemberID: drop.MemberID || '',
                    UserIDs: drop.UserIDs || '',
                    NotUserIDs: drop.NotUserIDs || '',
                    ServiceCode: drop.ServiceCode || '',
                    Effective: drop.Effective || ''
                },
                function (res) {
                    if (res.RESULT) {
                        var saRes = res.DATA.rel;
                        if (drop.Select) {
                            drop.Select.html(createOptions(saRes, 'MemberID', 'MemberName', drop.ShowId || false));
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
                }));
        });

        return $.whenArray(saPost);
    },
    /**
    * 設定下拉選單(參數設定)
    * @param {Array} drops 下拉配置
    * @return {Object} Ajax 物件
    */
    fnSetArgDrop = function (drops) {
        var saPost = [];
        $.each(drops, function (index, drop) {
            if (drop.ArgClassID) {
                saPost.push(g_api.ConnectLite(Service.com, ComFn.GetArguments,
                    {
                        ArgClassID: drop.ArgClassID,
                        ParentID: drop.ParentID || '',
                        ArgIDs: drop.Ids || '',
                        LevelOfArgument: drop.Level || -1,
                        OrgID: drop.OrgID || ''
                    },
                    function (res) {
                        if (res.RESULT) {
                            var saRes = res.DATA.rel;
                            if (drop.Select) {
                                drop.Select.html(createOptions(saRes, 'id', 'text', drop.ShowId || false))
                                    .on('change', function () {
                                        if (drop.OnChange && typeof drop.OnChange === 'function') {
                                            drop.OnChange(this.value);
                                        }
                                    });
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
                    }));
            }
        });

        return $.whenArray(saPost);
    },
    /**
    * 設定展覽下拉選單
    * @param {Object} drop 當前控件
    * @return {Object} Ajax 物件
    */
    fnSetEpoDrop = function (drop) {
        return g_api.ConnectLite('Exhibition_Upd', 'GetExhibitions',
            {
                SN: drop.SN || ''
            },
            function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    if (drop.Select) {
                        drop.Select.html(createOptions(saRes, drop.IdName || 'SN', drop.TextName || 'ExhibitioFullName', drop.ShowId || false));
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
    /*
     * 設定展覽選單(ExhibitionCode)
     */
    fnSetEpoDropWithExhibitionCode = function (drop) {
        return g_api.ConnectLite('BillsReport', 'GetProjects', {}, function (res) {
            if (res.RESULT) {
                var saRes = res.DATA.rel;
                if (drop.Select) {
                    drop.Select.html(createOptions(saRes, drop.IdName || 'id', drop.TextName || 'text', drop.ShowId || false));
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
    /*
     * 設定客戶資料(customer_guid)
     */
    fnSetCustomerWithGuid = function (drop) {
        return g_api.ConnectLite(Service.sys, 'GetCustomerlist', {}, function (res) {
            if (res.RESULT) {
                var saRes = res.DATA.rel;
                if (drop.Select) {
                    drop.Select.html(createOptions(saRes, 'id', 'text', drop.ShowId || false));
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
    * 設定展覽下拉選單
    * @param {Object} o 參數
    * @return {Object} Ajax 物件
    */
    fnGetOfficeTempls = function (o) {
        return g_api.ConnectLite(Service.com, 'GetOfficeTempls',
            {
                TemplID: o.TemplID || ''
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
     * 開啟單選選單Pop
     * @param  {Object} option 配置
     */
    oPenPops = function (option) {
        var oGrid = null,
            oCurItem = null,
            sContent = '',
            sGrid = '<div class="row popsrow"><div class="shadowbox"><div id="PopsGrid"></div></div></div>',
            saRow = [];
        $.each(option.SearchFields, function (index, item) {
            var oLabel = $('<Label />', { 'class': 'col-sm-2 w20p control-label' }),
                oSpan = $('<span />', { 'data-i18n': item.i18nkey }),
                oInputDiv = $('<div />', { 'class': 'col-sm-3' });

            switch (item.type) {
                case 'text':
                    $('<input />', { 'type': 'text', 'id': item.id, 'name': item.id, 'maxlength': '50', 'class': 'form-control w100p' }).appendTo(oInputDiv);
                    break;
                case 'select':
                    $('<select />', { 'id': item.id, 'name': item.id, 'class': 'form-control w100p', 'html': item.html }).appendTo(oInputDiv);
                    break;
            }
            saRow.push(oLabel.append(oSpan, '：'));
            saRow.push(oInputDiv);
            if (saRow.length === 4 || option.SearchFields.length === index + 1) {
                var oSearchDiv = $('<div />', { 'class': 'row popsrow' });
                oSearchDiv.append(saRow);
                sContent += oSearchDiv[0].outerHTML;
                saRow = [];
            }
        });
        sContent += sGrid;

        layer.open({
            type: 1,
            title: option.Title || i18next.t('common.PopsMenu'),//╠common.PopsMenu⇒單選選單╣
            shadeClose: false,
            shade: 0.5,
            maxmin: true, //开启最大化最小化按钮
            area: ['800px', option.SearchFields.length > 2 ? '615px' : '565px'],
            content: sContent,
            success: function (layero, index) {
                $("#PopsGrid").jsGrid({
                    width: "98%",
                    height: "390px",
                    autoload: true,
                    pageLoading: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: option.PageSize || 10,
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    pageButtonCount: 10,
                    rowClick: function (args) {
                        oCurItem = args.item;
                    },
                    rowDoubleClick: function (args) {
                        oCurItem = args.item;
                        option.Callback(oCurItem);
                        layer.close(index);
                    },
                    fields: option.Fields,
                    controller: {
                        loadData: function (args) {
                            return option.Get(args);
                        }
                    },
                    onInit: function (args) {
                        oGrid = args.grid;
                    }
                });
                layero.find('.layui-layer-btn1').css({ 'border-color': '#4898d5', 'background-color': '#1E9FFF', 'color': '#fff' });
                if (typeof option.PopSuccessCallback === 'function') {
                    option.PopSuccessCallback();
                }
            },
            btn: [i18next.t('common.Toolbar_Qry'), i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Toolbar_Qry⇒查詢╣ ╠common.Confirm⇒確定╣ ╠common.Cancel⇒取消╣
            yes: function (index, layero) {
                var iNum = $('.layui-layer #PerPageNum').val();
                oGrid.pageSize = iNum === '' ? 10 : iNum;
                oGrid.openPage(1);
                oCurItem = null;
                return false;
            },
            btn2: function (index, layero) {
                if (typeof option.Callback === 'function') { option.Callback(oCurItem || {}); }
                layer.close(index);
            },
            btn3: function (index) {
                if (typeof option.CancelCallback === 'function') { option.CancelCallback(); }
            },
            cancel: function () {
                if (typeof option.CancelCallback === 'function') { option.CancelCallback(); }
            }
        });
    },

    /**
     * 開啟複選選單Pop
     * @param  {Object} option 配置
     */
    oPenPopm = function (option) {
        var oGrid = null,
            sContent = '',
            sGrid = '<div class="row popsrow"><div class="shadowbox"><div id="PopmGrid"></div></div></div>',
            saRow = [],
            saRetn = [],
            saFields = [{
                width: 50, sorting: false, align: "center",
                headerTemplate: function () {
                    return [$("<input>", {
                        id: 'SelectAll',
                        type: 'checkbox', click: function () {
                            if (this.checked) {
                                $("#PopmGrid").find('[type=checkbox]').each(function () {
                                    this.checked = true;
                                });
                                saRetn = oGrid.data;
                            }
                            else {
                                $("#PopmGrid").find('[type=checkbox]').each(function () {
                                    this.checked = false;
                                });
                                saRetn = [];
                            }
                        }
                    }), $('<label />', { for: 'SelectAll', 'data-i18n': 'common.SelectAll' })];
                },
                itemTemplate: function (value, item) {
                    return $("<input>", {
                        type: 'checkbox', click: function (e) {
                            e.stopPropagation();
                            if (this.checked) {
                                saRetn.push(item);
                            }
                            else {
                                var saNewList = [];
                                $.each(saRetn, function (idx, data) {
                                    if (item.RowIndex !== data.RowIndex) {
                                        saNewList.push(data);
                                    }
                                });
                                saRetn = saNewList;
                                $('#PopmGrid').find('#SelectAll')[0].checked = false;
                            }
                        }
                    });
                }
            }];
        $.each(option.SearchFields, function (index, item) {
            var oLabel = $('<Label />', { 'class': 'col-sm-2 w20p control-label' }),
                oSpan = $('<span />', { 'data-i18n': item.i18nkey }),
                oInputDiv = $('<div />', { 'class': 'col-sm-3' });

            switch (item.type) {
                case 'text':
                    $('<input />', { 'type': 'text', 'id': item.id, 'name': item.id, 'maxlength': '50', 'class': 'form-control w100p' }).appendTo(oInputDiv);
                    break;
                case 'select':
                    $('<select />', { 'id': item.id, 'name': item.id, 'class': 'form-control w100p', 'html': item.html }).appendTo(oInputDiv);
                    break;
            }
            saRow.push(oLabel.append(oSpan, '：'));
            saRow.push(oInputDiv);
            if (saRow.length === 4 || option.SearchFields.length === index + 1) {
                var oSearchDiv = $('<div />', { 'class': 'row popsrow' });
                oSearchDiv.append(saRow);
                sContent += oSearchDiv[0].outerHTML;
                saRow = [];
            }
        });
        sContent += sGrid;
        saFields.push.apply(saFields, option.Fields);

        layer.open({
            type: 1,
            title: option.Title || i18next.t('common.PopmMenu'),//╠common.PopmMenu⇒複選選單╣
            shadeClose: false,
            shade: 0.5,
            maxmin: true, //开启最大化最小化按钮
            area: [option.Width || '800px', option.SearchFields.length > 2 ? '615px' : '565px'],
            content: (option.ContentPlush || '') + sContent,
            success: function (layero, index) {
                $("#PopmGrid").jsGrid({
                    width: "98%",
                    height: "400px",
                    autoload: true,
                    pageLoading: true,
                    sorting: true,
                    paging: true,
                    pageIndex: 1,
                    pageSize: option.PageSize || 10,
                    pagePrevText: "<",
                    pageNextText: ">",
                    pageFirstText: "<<",
                    pageLastText: ">>",
                    pageButtonCount: 10,
                    rowClick: function (args) {
                        $(args.event.currentTarget).find('[type=checkbox]').click();
                    },
                    fields: saFields,
                    controller: {
                        loadData: function (args) {
                            return option.Get(args);
                        }
                    },
                    onInit: function (args) {
                        oGrid = args.grid;
                    }
                });
                layero.find('.layui-layer-btn1').css({ 'border-color': '#4898d5', 'background-color': '#1E9FFF', 'color': '#fff' });
                if (typeof option.PopSuccessCallback === 'function') {
                    option.PopSuccessCallback();
                }
            },
            btn: [i18next.t('common.Toolbar_Qry'), i18next.t('common.Confirm'), i18next.t('common.Cancel')],//╠common.Toolbar_Qry⇒查詢╣ ╠common.Confirm⇒確定╣ ╠common.Cancel⇒取消╣
            yes: function (index, layero) {
                var iNum = $('.layui-layer #PerPageNum').val();
                oGrid.pageSize = iNum === '' ? 10 : iNum;
                oGrid.openPage(1);
                saRetn = [];
                $('#PopmGrid').find('#SelectAll')[0].checked = false;
                return false;
            },
            btn2: function (index, layero) {
                if (typeof option.Callback === 'function') { option.Callback(saRetn); }
                layer.close(index);
            },
            btn3: function (index) {
                if (typeof option.CancelCallback === 'function') { option.CancelCallback(); }
            },
            cancel: function () {
                if (typeof option.CancelCallback === 'function') { option.CancelCallback(); }
            }
        });
    };