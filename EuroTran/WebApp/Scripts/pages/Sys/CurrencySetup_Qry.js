'use strict';
let sProgramID = 'CurrencySetup_Qry';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'year',
        sortOrder: 'asc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['year', 'month', 'currency'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            { name: "year", title: 'CurrencySetup_Qry.year', align: 'center', width: 200 },
            { name: "month", title: 'CurrencySetup_Qry.month', align: 'center', width: 200 },
            { name: "currency", title: 'CurrencySetup_Qry.currency', align: 'center', width: 200 },
            { name: "exchange_rate", title: 'CurrencySetup_Qry.exchange_rate', align: 'center', width: 200 }
        ],

        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            fpLoadYears();
            fpLoadMonth();
            fpLoadCurrency().done(() => {
                pargs._reSetQueryPm();
                pargs._initGrid();
            })
        }
    }),
        /**
        * 載入年份
        */
        fpLoadYears = function () {
            let dtCurrentDate = new Date();
            let ibaseYear = dtCurrentDate.getFullYear();
            let saYearList = [];
            //抓取當年度加減20年
            for (let y = ibaseYear - 20; y <= ibaseYear + 20; y++) {
                saYearList.push({
                    year: y,
                    value: y
                });
            }

            var sOptionHtml = createOptions(saYearList, 'year', 'value', false);
            $('#year').html(sOptionHtml).val(dtCurrentDate.getFullYear());
        },
        /**
         * 載入月份
         */
        fpLoadMonth = function () {
            let dtCurrentDate = new Date();
            let saMonthList = [];
            //抓取當年度加減20年
            for (let m = 1; m <= 12; m++) {
                saMonthList.push({
                    month: m,
                    value: m
                });
            }

            var sOptionHtml = createOptions(saMonthList, 'month', 'value', false);
            $('#month').html(sOptionHtml).val('');
        },
        /**
         * 載入幣別
         */
        fpLoadCurrency = function () {
            return g_api.ConnectLite(sProgramID, 'CurrencyList', {},
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        var sOptionHtml = createOptions(saList, 'ArgumentID', 'ArgumentValue', true);
                        $('#currency').html(sOptionHtml);
                    }
                });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);