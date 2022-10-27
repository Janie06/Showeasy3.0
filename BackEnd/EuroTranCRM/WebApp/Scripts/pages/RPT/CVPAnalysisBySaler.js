'use strict';
var ReportService = 'CostAndProfitReport';
var sProgramId = getProgramId();
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 客製化按鈕
         * @param  {Object} pargs CanDo 對象
         */
        cusBtns: function (pargs) {
            var saCusBtns = [{
                id: 'Toolbar_PreviewReport',
                value: 'common.Toolbar_PreviewReport',// ╠common.Toolbar_PreviewReport⇒預覽報表╣
                action: function (pargs) {
                    fnReport('pdf');
                }
            },
            {
                id: 'Toolbar_DownloadReport',
                value: 'common.Toolbar_DownloadReport',// ╠common.Toolbar_DownloadReport⇒下載報表╣
                action: function (pargs) {
                    fnReport('excel');
                }
            }];
            return saCusBtns;
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            $.whenArray([fnSetDeptDropWithLimtedDeptId($('#ResponsibleDeptID')), fnSetUserDrop([
                {
                    Select: $('#ResponsiblePerson'),
                    Select2: true,
                    ShowId: true,
                    CallBack: function () { }
                }
            ])]).done(function () {

            });
            var iheight = $('body').height() - $('.page-title').height() - $('#searchbar').height() - 107;
            $('#report').css('height', iheight + 'px');
            $('.slide-box').on('click', function () {
                if ($(this).hasClass('fa-arrow-down')) {
                    $('#report').css('height', iheight + 80 + 'px');
                }
                else {
                    $('#report').css('height', iheight + 'px');
                }
            });

            $('#ResponsibleDeptID').on('change', function () {
                //fnSetHandle_PersonDrop(this.value);
            });

        }
    }),
        /**
         * 設置部門資料，連動業務
         * @param {String} deptid 部門ID
         */
        fnSetHandle_PersonDrop = function (deptid) {
            var sDepartmentID = deptid || '';
            fnSetUserDrop([
                {
                    Action: 'add',
                    DepartmentID: sDepartmentID,
                    CallBack: function (data) {
                        var saList = data;
                        $('#ResponsiblePerson').html(createOptions(saList, 'MemberID', 'MemberName', true));
                    }
                }
            ]);
        },
        /**
         * 預覽報表或下載報表
         * @param  {String} flag 預覽或下載
         */
        fnReport = function (flag) {
            var oQuery = canDo._getFormSerialize();
            oQuery.Flag = flag;
            oQuery.AdminUser = parent.UserInfo.roles.toLowerCase().indexOf('admin') > -1;
            return g_api.ConnectLite(ReportService, sProgramId, oQuery, function (res) {
                if (res.RESULT) {
                    var sPath = res.DATA.rel;
                    if (flag === 'pdf') {
                        $('#report').attr('src', gServerUrl + '/' + sPath);
                    }
                    else {
                        canDo._downLoadFile(sPath);
                    }
                }
                else {
                    showMsg("Fail", 'error');
                    console.log(res.MSG);
                }
            });
        };
};

require(['base', 'select2', 'cando'], fnPageInit);