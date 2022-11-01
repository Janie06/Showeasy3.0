'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'CreateDate',
        sortOrder: 'desc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'AnnouncementID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: "center", type: "text", width: 50, sorting: false },
            {
                name: "LogType", title: 'common.LogType', align: "left", type: "text", width: 120,
                itemTemplate: function (val, item) {
                    var oType = { leavesetchange: i18next.t("common.LeaveSetChange"), billnoupdate: i18next.t("common.BillNoUpdate") };
                    return oType[val] || '';
                }
            },
            {
                name: "CreateUser", title: 'common.Actor', type: "text", align: "center", width: 100
            },
            {
                name: "CreateDate", title: 'common.LogTime', type: "text", align: "center", width: 100,
                itemTemplate: function (val, item) {
                    return newDate(val);
                }
            },
            {// ╠common.LogInfo⇒記錄明細╣ ╠common.Info⇒詳情╣
                name: "LogInfo", title: 'common.LogInfo', type: "text", width: 500,
                itemTemplate: function (val, item) {
                    var elCotent = val || '';
                    if (elCotent.length > 266) {
                        elCotent = [elCotent.substr(0, 266), $('<a />', {
                            class: 'link', text: i18next.t("common.Info"),
                            click: function () {
                                layer.open({
                                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                                    title: i18next.t('common.LogInfo'),
                                    area: ['960px', '600px'],//寬度
                                    shade: 0.75,//遮罩
                                    shadeClose: true,
                                    maxmin: true, //开启最大化最小化按钮
                                    id: 'layer_Info', //设定一个id，防止重复弹出
                                    offset: '50px',
                                    anim: 0,//彈出動畫
                                    btn: [i18next.t('common.Cancel')],
                                    btnAlign: 'c',//按鈕位置
                                    content: '<div class="col-md-12"><code></code></div>',
                                    success: function (layero, index) {
                                        layero.find('code').html(item.LogInfo || '');
                                    }
                                });
                            }
                        })];
                    }
                    return elCotent;
                }
            }
        ],
        pageInit: function (pargs) {
            fnSetUserDrop([{
                Select: $('#Actor'),
                ShowId: true,
                Select2: true,
                CallBack: function (data) {
                    pargs._reSetQueryPm();
                    pargs._initGrid();
                }
            }]);
        }
    });
};

require(['base', 'select2', 'jsgrid', 'cando'], fnPageInit);