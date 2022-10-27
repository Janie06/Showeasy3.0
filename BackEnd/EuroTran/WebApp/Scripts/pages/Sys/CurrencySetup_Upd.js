'use strict';
let sProgramID = 'CurrencySetup_Upd';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['year', 'month', 'currency'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['year', 'month', 'currency'],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            fpLoadYears();
            fpLoadMonth();
            fpLoadCurrency().done(() => {
                if (pargs.action === 'upd') {
                    $('#year,#month,#currency').prop('disabled', true);
                    pargs._getOne();
                }
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
            $('#month').html(sOptionHtml).val(dtCurrentDate.getMonth() + 1);
        },
        /**
        * 載入幣別
        */
        fpLoadCurrency = function () {
            return g_api.ConnectLite('CurrencySetup_Qry', 'CurrencyList', {},
                function (res) {
                    if (res.RESULT) {
                        var saList = res.DATA.rel;
                        var sOptionHtml = createOptions(saList, 'ArgumentID', 'ArgumentValue', true);
                        $('#currency').html(sOptionHtml);
                    }
                });
        };
};

require(['base', 'cando'], fnPageInit);