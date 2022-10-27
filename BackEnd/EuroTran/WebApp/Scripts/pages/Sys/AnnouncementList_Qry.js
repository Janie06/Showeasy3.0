'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        sortField: 'GoTop DESC,GoTop_Time DESC,CreateDate',
        sortOrder: 'desc',
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'AnnouncementID'],
        /**
         * Grid欄位設置（可以是 function）
         */
        gridFields: [
            { name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 50, sorting: false },
            {
                name: "Ann_Type", title: 'Announcement_Upd.Ann_Type', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return $('#Ann_Type option[value=' + val + ']').text().split('-')[1];
                }
            },
            { name: "Title", title: 'Announcement_Upd.Title', width: 300 },
            {
                name: "StartDateTime", title: 'Announcement_Upd.StartDateTime', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return newDate(val, 'date');
                }
            },
            {
                name: "EndDateTime", title: 'Announcement_Upd.EndDateTime', width: 120, align: 'center', itemTemplate: function (val, item) {
                    return newDate(val, 'date');
                }
            },
            {// ╠common.GoTop⇒置頂╣
                name: "GoTop", title: 'common.GoTop', width: 100, align: 'center', itemTemplate: function (val, item) {
                    var oCheckBox = $('<input>', {
                        type: 'checkbox',
                        checked: val,
                        click: function () {
                            var oPm = { AnnouncementID: item.AnnouncementID };
                            if (this.checked) {
                                oPm.GoTop = true;
                            }
                            else {
                                oPm.GoTop = false;
                            }
                            g_api.ConnectLite(canDo.ProgramId, 'UpdateGoTop', oPm, function (res) {
                                if (res.RESULT) {
                                    canDo.Grid.openPage(canDo.bToFirstPage ? 1 : canDo.pageIndex);
                                }
                            });
                        }
                    });
                    return oCheckBox;
                }
            },
            {
                title: 'common.Action', width: 100, align: 'center',
                itemTemplate: function (val, item) {
                    var oDom = $('<a/>', {
                        html: i18next.t('common.See'),// ╠common.See⇒查看╣
                        class: 'a-url',
                        click: function () {
                            fnOpenAnn(item.AnnouncementID);
                            return false;
                        }
                    });
                    return $('<div>', { 'style': 'width:100%;text-align: center;' }).append(oDom);
                }
            }
        ],
        /**
         * 當前程式所有ID名稱集合
         */
        rowDoubleClick: function (pargs, args) {
            fnOpenAnn(args.item.AnnouncementID);
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            $.when(fnSetArgDrop([
                {
                    ArgClassID: 'Ann_Type',
                    Select: $('#Ann_Type'),
                    ShowId: true
                }
            ])).done(function () {
                pargs._reSetQueryPm();
                pargs._initGrid();
            });
        }
    }),
        /**
         * 查看公告明細
         * @param {String} id 公告guid
         */
        fnOpenAnn = function (id) {
            getHtmlTmp('/Page/Pop/AnnounceInfo.html').done(function (html) {
                var oAnnInfo = canDo.Grid.data.filter(function (item) { return item.AnnouncementID === id; })[0];
                oAnnInfo.CategoryName = '公&nbsp;&nbsp;告';
                oAnnInfo.CreateUserName = oAnnInfo.CreateUser;
                oAnnInfo.CreateDate = newDate(oAnnInfo.CreateDate, 'date');
                var sHtml = $('<script type="text/x-jsrender"/>').html(html).render(oAnnInfo);
                layer.open({
                    type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                    title: i18next.t('common.Announcement'),
                    area: '640px;',//寬度
                    shade: 0.75,//遮罩
                    closeBtn: 1,
                    shadeClose: true,
                    //maxmin: true, //开启最大化最小化按钮
                    id: 'layer_Announce', //设定一个id，防止重复弹出
                    offset: '12px',
                    anim: 0,//彈出動畫
                    btn: [],
                    btnAlign: 'c',//按鈕位置
                    content: sHtml,
                    success: function (layero, index) {
                        layero.find('a').each(function () {
                            if (($(this).attr('href') || '').indexOf('net/upload/file') > -1 && (($(this).prev().attr('src') || '').indexOf('icon_jpg') > -1 || ($(this).prev().attr('src') || '').indexOf('icon_pdf') > -1)) {
                                $(this).attr('target', '_new');
                            }
                        });
                        layero.find('.layui-layer-title').css({ 'text-align': 'center', 'padding': '0 30px 0 20px', 'font-size': '20px', 'font-weight': '600' });
                        slimScroll();
                    }
                });
            });
        };
};

require(['base', 'jsgrid', 'cando'], fnPageInit);