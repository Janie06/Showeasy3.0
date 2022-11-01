'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'MemberID',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'MemberID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "MemberID", title: 'common.Account', width: 100 },
            { name: "MemberName", title: 'common.FullName', width: 120 },
            { name: "DepartmentName", title: 'common.Department', width: 150 },
            { name: "JobtitleName", title: 'common.JobtitleName', width: 150 },
            { name: "ImmediateSupervisorName", title: 'MembersMaintain_Upd.ImmediateSupervisor', width: 100 },
            { name: "ContectExt", title: 'common.Telephone', width: 100 },
            { name: "EmergencyContect", title: 'MembersMaintain_Upd.EmergencyContect', width: 120 },
            { name: "ArriveDate", title: 'MembersMaintain_Upd.ArriveDate', width: 100, align: 'center', itemTemplate: function (val) { return newDate(val, 'date', true) } },
            { name: "LeaveDate", title: 'MembersMaintain_Upd.LeaveDate', width: 100, align: 'center', itemTemplate: function (val) { return newDate(val, 'date', true) } },
            {
                name: "Effective", title: 'common.Status', width: 100, align: 'center', itemTemplate: function (val) {
                    return val === 'Y' ? i18next.t('common.Effective') : i18next.t('common.Invalid');
                }
            }
        ],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            $.whenArray([fnSetDeptDrop($('#DepartmentID')), fnSetJobTitleDrop()]).done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    }),
        /**
        * 設置職稱下拉單
         * @return {Object} Ajax 物件
        */
        fnSetJobTitleDrop = function () {
            return g_api.ConnectLite(canDo.EditPrgId, 'GetJobTitleDrop', {},
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        $('#JobTitle').html(createOptions(saList, 'JobtitleID', 'JobtitleName', true));
                    }
                });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);