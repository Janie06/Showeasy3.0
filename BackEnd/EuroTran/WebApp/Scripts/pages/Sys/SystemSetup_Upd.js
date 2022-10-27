'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'SettingItem'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['SettingItem'],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            if (pargs.action === 'upd') {
                $('#SettingItem').prop('disabled', true);
                pargs._getOne();
            }
        }
    });
};

require(['base', 'cando'], fnPageInit);