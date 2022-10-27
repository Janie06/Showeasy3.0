'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * Grid初始化
         * @param  {Object} pargs CanDo 對象
         */
        initGrid: function (pargs, data) {
            var saFields = [];
            saFields.push({ name: "RowId", title: 'common.RowNumber', align: 'center', width: 50, sorting: false });
            saFields.push({
                name: "ProgramName", title: 'common.ProgramName', width: 120, sorting: false, itemTemplate: function (val, item) {
                    return $('<a/>', {
                        html: val, click: function () {
                            $(this).parents('tr').find(':input[type="checkbox"]').click();
                        }
                    });
                }
            });

            $.each(data, function (indx, item) {
                saFields.push({
                    name: item.id, title: 'common.Toolbar_' + item.id, width: 70, sorting: false, align: "center",
                    itemTemplate: function (val, item) {
                        if (val != undefined) {
                            var sAllRight = $.trim(item.AllowRight);
                            return $("<input>", {
                                type: 'checkbox',
                                value: val,
                                disabled: item.fix === 'Y' ? true : false,
                                checked: sAllRight.indexOf(val) > -1, click: function (e) {
                                    e.stopPropagation();
                                    var sRight = $.trim(item.AllowRight),
                                        saRight = sRight === '' ? [] : sRight.split('|'),
                                        saLastRight = [];
                                    if (this.checked) {
                                        saRight.push(this.value);
                                    }
                                    else {
                                        if (saRight.indexOf(this.value) > -1)
                                            saRight.splice($.inArray(this.value, saRight), 1);
                                    }
                                    $.each(saRight, function (idx, right) {
                                        if (('|' + $.trim(item.CanAllowRight) + '|').indexOf('|' + right + '|') > -1) {
                                            saLastRight.push(right);
                                        }
                                    });
                                    item.AllowRight = saLastRight.join('|');
                                }
                            });
                        }
                        else {
                            return "";
                        }
                    }
                });
            });
            var iHeight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 117;
            pargs.jsGrid.jsGrid({
                width: "100%",
                height: iHeight + "px",
                autoload: false,
                pageLoading: true,
                inserting: false,
                editing: false,
                sorting: false,
                paging: false,
                invalidMessage: '输入的数据无效！',
                confirmDeleting: true,
                deleteConfirm: "確定要刪除嗎？",
                pagePrevText: "<",
                pageNextText: ">",
                pageFirstText: "<<",
                pageLastText: ">>",
                fields: saFields,
                controller: {
                    loadData: function (args) {
                        return pargs.getPage(pargs, args);
                    },
                },
                onInit: function (args) {
                    pargs.setGrid(args.grid);
                }
            });
        },
        /**
         * 客製化按鈕
         * @param  {Object} pargs CanDo 對象
         */
        cusBtns: function (pargs) {
            var saCusBtns = [{
                id: 'CheckAll',
                value: 'common.Toolbar_CheckAll',// ╠common.Toolbar_CheckAll⇒全選╣
                /**
                 * 業務提交審核
                 */
                action: function (pargs) {
                    pargs.jsGrid.find('[type=checkbox]').each(function () {
                        if (!this.checked) {
                            $(this).click();
                        }
                    });
                }
            }, {
                id: 'CancelAll',
                value: 'common.Toolbar_CancelAll',// ╠common.Toolbar_CancelAll⇒全部取消╣
                /**
                 * 主管審核
                 */
                action: function (pargs) {
                    pargs.jsGrid.find('[type=checkbox]').each(function () {
                        if (this.checked) {
                            $(this).click();
                        }
                    });
                }
            }];
            return saCusBtns;
        },
        /**
         * 系統權限（分頁查詢）
         */
        getPage: function (pargs) {
            var sAuthantedtype = $('[name=authantedtype]:checked').val(),
                sModuleID = $('#ModuleID').val() || [],
                sSubSystem = $('#Subsystem').val(),//子系統
                sRuleID = '';

            switch (sAuthantedtype) {
                case 'Role'://選擇角色查詢
                    sRuleID = $('#RuleID').val();
                    break;
                case 'Dept': //選擇部門查詢
                    sRuleID = $('#DepartmentID').val();
                    break;
                case 'Member'://選擇人員查詢
                    sRuleID = $('#MemberID').val();
                    break;
            }

            var oParm = {
                Type: sAuthantedtype,
                OrgID: parent.OrgID,
                RuleID: sRuleID,
                ChildSystem: sSubSystem,
                ModuleID: sModuleID.join(',')
            };

            return g_api.ConnectLite(pargs.ProgramId, 'GetAuthorizeBy_', oParm);
        },
        /**
         * 系統權限（修改）
         */
        getInsert: function (pargs) {
            var data = pargs.Grid.data,
                oParm = {},
                sAuthantedtype = $('[name=authantedtype]:checked').val(),
                sModuleID = $('#ModuleID').val() || [],
                sSubSystem = $('#Subsystem').val(),//子系統
                sRuleID = '';

            switch (sAuthantedtype) {
                case 'Role'://選擇角色查詢
                    sRuleID = $('#RuleID').val();
                    break;
                case 'Dept': //選擇部門查詢
                    sRuleID = $('#DepartmentID').val();
                    break;
                case 'Member'://選擇人員查詢
                    sRuleID = $('#MemberID').val();
                    break;
            }

            oParm = {
                Type: sAuthantedtype,
                OrgID: parent.OrgID,
                RuleID: sRuleID,
                ModuleID: sModuleID.join(','),
                ChildSystem: sSubSystem
            };
            oParm.add = [];
            $.each(data, function (i, item) {
                if (item.RuleID && item.ProgramID && item.AllowRight) {
                    var oAddDic = {
                        OrgID: parent.OrgID,
                        RuleID: sRuleID,
                        ProgramID: item.ProgramID,
                        AllowRight: item.AllowRight,
                        TopModuleID: sSubSystem,
                        Memo: item.Memo
                    };
                    oAddDic = packParams(oAddDic);
                    oParm.add.push(oAddDic);
                }
            });

            return g_api.ConnectLite(pargs.ProgramId, 'UpdateAuthorize', oParm,
                function (res) {
                    if (res.RESULT) {
                        showMsg(i18next.t("message.Modify_Success"));//╠message.Modify_Success⇒修改成功╣
                    }
                    else {
                        showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                    }
                }, function () {
                    showMsg(i18next.t("message.Modify_Failed"), 'error');//╠message.Modify_Failed⇒修改失敗╣
                });
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            $.whenArray([
                fnSubSystemDrop(),
                fnSetModuleDrop(),
                fnRuleIDDrop(),
                fnSetDepartmentList(),
                fnSetUserDrop([{
                    Select: $('#MemberID'),
                    ShowId: true
                }])
            ])
                .done(function () {
                    pargs._reSetQueryPm();
                    $('[name=authantedtype]:checked').click();

                    fnSetArgDrop([
                        {
                            ArgClassID: '99999',
                            CallBack: function (data) {
                                pargs.initGrid(pargs, data);
                            }
                        }
                    ])
                });
        }
    }),
        /**
         * 設置部門資料
         */
        fnSetDepartmentList = function () {
            $('.boxdept,.boxmember').hide(); //部門,人員
            $('[name=authantedtype]').click(function () {
                if (this.value == 'Role') {
                    $('.boxdept,.boxmember').hide(); //部門,人員
                    $('.boxrule').show(); //角色
                }
                else if (this.value == 'Dept') {
                    $('.boxrule,.boxmember').hide(); //角色,人員
                    $('.boxdept').show(); //部門
                }
                else {
                    $('.boxrule,.boxdept').hide(); //角色,部門
                    $('.boxmember').show(); //人員
                }
            });

            fnSetDeptDrop($('#DepartmentID'));
        },
        /**
        * 設置子系統下拉單
        * @return {Object} Ajax 物件
        */
        fnSubSystemDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, 'GetModulelist', { ParentID: true },
                function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel;
                        var sOptionHtml = createOptions(saData, 'ModuleID', 'ModuleName', true);
                        $('#Subsystem').html(sOptionHtml).find('option').first().remove();
                    }
                });
        },
        /**
        * 設置子系統下拉單
        * @return {Object} Ajax 物件
        */
        fnSetModuleDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, 'GetModulelist', { ParentID: false },
                function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel;
                        var sOptionHtml = createOptions(saData, 'ModuleID', 'ModuleName', true);
                        $('#ModuleID').html(sOptionHtml).select2()[0].remove(0);
                    }
                });
        },
        /**
        * 設置角色名稱下拉單
        * @return {Object} Ajax 物件
        */
        fnRuleIDDrop = function () {
            return g_api.ConnectLite(canDo.ProgramId, 'GetRules', {},
                function (res) {
                    if (res.RESULT) {
                        var saData = res.DATA.rel;
                        var sOptionHtml = createOptions(saData, 'RuleID', 'RuleName', true);
                        $('#RuleID').html(sOptionHtml);
                    }
                });
        };
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);