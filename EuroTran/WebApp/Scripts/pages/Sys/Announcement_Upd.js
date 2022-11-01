'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'AnnouncementID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['AnnouncementID'],
        /**
         * 須初始化的UEEditer 的物件ID集合
         */
        ueEditorIds: ['Description'],
        /**
         * 客製化驗證規則
         * @param  {Object} pargs CanDo 對象
         */
        validRulesCus: function (pargs) {
            $.validator.addMethod("compardate", function (value, element, parms) {
                if (new Date(value) < new Date($('#StartDateTime').val())) {
                    return false;
                }
                return true;
            });
        },
        /**
         * 驗證規則
         */
        validRules: {
            ignore: ''
        },
        /**
         * 查詢當前資料
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前資料實體
         */
        getOneBack: function (pargs, data) {
            pargs._setFormVal(data);
            pargs._setUEValues(data);
            $("#FontColor").spectrum("set", data.FontColor);
        },
        /**
         * 新增資料
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前新增的資料
         * @param {String} flag 新增 or 儲存后新增
         */
        getInsertBack: function (pargs, data, flag) {
            if (flag == 'add') {
                showMsgAndGo(i18next.t("message.Save_Success"), pargs.QueryPrgId); // ╠message.Save_Success⇒新增成功╣
            }
            else {
                showMsgAndGo(i18next.t("message.Save_Success"), pargs.ProgramId, '?Action=Add'); // ╠message.Save_Success⇒新增成功╣
            }
            parent.msgs.server.broadcast(data);
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var postArray = [];

            if (pargs.action === 'upd') {
                postArray.push(pargs._getOne());
            }

            postArray.push(fnSetArgDrop([
                {
                    ArgClassID: 'Ann_Type',
                    Select: $('#Ann_Type'),
                    ShowId: true
                }
            ]));

            $.whenArray(postArray).done(function (res) {
                if (pargs.action === 'upd' && res[0].RESULT) {
                    var oRes = res[0].DATA.rel;
                    pargs._setFormVal(oRes);
                    pargs._getPageVal();//緩存頁面值，用於清除
                }
            });
        }
    });
};

require(['base', 'spectrum', 'cando'], fnPageInit);