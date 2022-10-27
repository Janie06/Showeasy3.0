'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'LoginTime',
        sortOrder: 'desc',
        inserting: true,
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['NO'],
        onItemEditing: function (args) {
            if (args.item.IsVerify === 'Y') {
                args.cancel = true;
            }
        },
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: function (pargs) {
            return [
                { name: "RowIndex", title: 'common.RowNumber', editing: false, align: "center", inserting: false, type: "text", width: 50, sorting: false },
                {
                    name: "UserID", title: 'common.UserId', editing: true, align: "left", type: "text", width: 80,
                    validate: { validator: 'required', message: i18next.t('common.AuthID_required') }// ╠common.AuthID_required⇒請輸入授權ID╣
                },
                {
                    name: "UserName", title: 'common.UserName', editing: true, align: "left", type: "text", width: 130,
                    validate: { validator: 'required', message: i18next.t('common.AuthName_required') }// ╠common.AuthName_required⇒請輸入授權名稱╣
                },
                { name: "Token", title: 'common.Token', editing: false, inserting: false, align: "left", type: "text", width: 400 },//╠common.Token⇒Token╣
                { name: "LoginIp", title: 'common.LoginIp', type: "text", editing: false, inserting: false, align: "center", width: 100 },
                {
                    name: "IsVerify", title: 'common.IsVerify', type: "text", editing: false, inserting: false, align: "center", width: 80,
                    itemTemplate: function (val, item) {
                        return val === 'Y' ? i18next.t('common.Yes') : i18next.t('common.No');
                    }
                },
                {
                    type: "control", width: 200, align: 'center',
                    itemTemplate: function (val, item) {
                        var oDom = [];
                        if (item.IsVerify === 'N') {
                            oDom.push($('<a/>', {
                                html: i18next.t('common.ReSetTokenSignature'),//╠common.ReSetTokenSignature⇒重新產生Token和簽名╣
                                class: 'a-url',
                                click: function () {
                                    fnReSetToken(item);
                                    return false;
                                }
                            }), $('<a/>', {
                                html: i18next.t('common.Toolbar_Del'),//╠common.Toolbar_Del⇒刪除╣
                                class: 'a-url',
                                click: function () {
                                    pargs.gridDelete(item).done(function () {
                                        pargs.getPage({});
                                    });
                                    return false;
                                }
                            }));
                        }
                        return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(oDom);
                    }
                }
            ];
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            pargs._reSetQueryPm();
            pargs._initGrid();
        }
    }),
        /**
         * 重新產生Token和簽名
         * @param  {Object}  data 表單資料
         */
        fnReSetToken = function (data) {
            data = packParams(data, true);

            g_api.ConnectLite(canDo.ProgramId, 'ReSetToken', { NO: data.NO }, function (res) {
                if (res.RESULT) {
                    fnGet(oBaseQueryPm);
                    showMsg(i18next.t("message.Create_Success"), 'success'); //╠message.Create_Success⇒產生成功╣
                }
                else {
                    showMsg(i18next.t('message.Create_Failed') + '<br>' + res.MSG, 'error'); //╠message.Create_Failed⇒產生失敗╣
                }
            }, function () {
                showMsg(i18next.t("message.Create_Failed"), 'error');//╠message.Create_Failed⇒產生失敗╣
            });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);