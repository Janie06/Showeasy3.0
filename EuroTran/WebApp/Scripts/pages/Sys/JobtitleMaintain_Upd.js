'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'JobtitleID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['JobtitleID'],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            if (pargs.action === 'upd') {
                $('#JobtitleID').prop('disabled', true);
                pargs._getOne();
            }
        }
    });
};

require(['base', 'cando'], fnPageInit);