'use strict';

var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 修改完后是否跳轉
         */
        updateGo: false,
        /**
         * 須初始化的UEEditer 的物件ID集合
         */
        ueEditorIds: ['Introduction', 'Introduction_CN', 'Introduction_EN', 'MissionAndVision_TW', 'MissionAndVision_CN', 'MissionAndVision_EN', 'MissionAndVision_EN', 'ServiceTitle', 'ServiceTitle_CN', 'ServiceTitle_EN', 'VideoDescription', 'VideoDescription_CN', 'VideoDescription_EN', 'SettingInfo'],
        /**
         * 查詢當前資料
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前資料實體
         */
        getOneBack: function (pargs, data) {
            pargs._setFormVal(data);
            pargs._setUEValues(data);
            fnGetUploadFiles(data.WebsiteLgoId, fnUpload, $('#WebsiteLogoId'));
            fnGetUploadFiles(data.PicShowId, fnUpload, $('#PicShowId'));
            fnGetUploadFiles(data.PicShowId_CN, fnUpload, $('#PicShowId_CN'));
            fnGetUploadFiles(data.PicShowId_EN, fnUpload, $('#PicShowId_EN'));
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            pargs.action = 'update';
            pargs._getOne();
            fnGridInit('Carousel', 'TW');//初始化輪播(繁中)
            fnGridInit('ServiceItems', 'TW');//初始化服務項目(繁中)
            fnGridInit('ClassicCase', 'TW');//初始化經典案例(繁中)
            fnGridInit('Video', 'TW');//初始化品牌影音(繁中)
            fnGridInit('Downloads', 'TW');//文檔下載(繁中)
            fnGridInit('ServiceBase', 'TW');//初始化全球服務據點(繁中)

            fnGridInit('Carousel', 'CN');//初始化輪播(簡中)
            fnGridInit('ServiceItems', 'CN');//初始化服務項目(簡中)
            fnGridInit('ClassicCase', 'CN');//初始化經典案例(簡中)
            fnGridInit('Video', 'CN');//初始化品牌影音(簡中)
            fnGridInit('Downloads', 'CN');//文檔下載(簡中)
            fnGridInit('ServiceBase', 'CN');//初始化全球服務據點(簡中)

            fnGridInit('Carousel', 'EN');//初始化輪播(英文)
            fnGridInit('ServiceItems', 'EN');//初始化服務項目(英文)
            fnGridInit('ClassicCase', 'EN');//初始化經典案例(英文)
            fnGridInit('Video', 'EN');//初始化品牌影音(英文)
            fnGridInit('Downloads', 'EN');//文檔下載(英文)
            fnGridInit('ServiceBase', 'EN');//初始化全球服務據點(英文)
        }
    }),
        oGrid = {},
        oOption = {},
        oBaseQueryPm = {
            pageIndex: 1,
            pageSize: parent.SysSet.GridRecords || 10,
            sortField: 'ParentId,OrderByValue',
            sortOrder: 'asc'
        },
        oLang = { 'TW': 'zh-TW', 'CN': 'zh', 'EN': 'en' },
        /**
         * 新增輪播資訊
         * @param {Object} args 參數
         * @return {Function} Ajax
         */
        fnSetInsert = function (args) {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.ginsert, args, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                }
            });
        },
        /**
         * 修改輪播資訊
         * @param {Object} args 參數
         * @return {Function} Ajax
         */
        fnSetUpdate = function (args) {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.gupdate, args, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                }
            });
        },
        /**
         * 刪除輪播資訊
         * @param {Object} args 參數
         * @return {Function} Ajax
         */
        fnSetDelete = function (args) {
            return g_api.ConnectLite(canDo.ProgramId, canDo._api.gdelete, args, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                }
            });
        },
        /**
         * 抓去輪播資訊
         * @param {Object} args 參數
         * @param {String} settype 設定類別
         * @param {String} lang 語系
         * @return {Object} ajax
         */
        fnGetSetting = function (args, settype, lang) {
            var sKey = settype + '_' + lang,
                oQueryPm = {
                    SetType: settype, LangId: oLang[lang]
                };

            $.extend(oQueryPm, oBaseQueryPm, args);

            return g_api.ConnectLite(canDo.ProgramId, canDo._api.getpage, oQueryPm, function (res) {
                if (res.RESULT) {
                    var oRes = res.DATA.rel;
                    if (settype === 'ClassicCase') {
                        var saParentId = $.grep(oRes.DataList, function (item) {
                            if (!item.ParentId) {
                                return item;
                            }
                        });
                        oOption[sKey].ParentIdHtml = createOptions(saParentId, 'Guid', 'Title');
                    }
                }
            });
        },
        /**
         * Grid客戶化計價模式控件
         * @param {String}flag 標記
         * @return {HTMLElement} html 物件
         */
        fnCreateActiveInput = function (flag) {
            var elDiv = $('<div>'),
                data = [{ id: true, text: i18next.t('common.Effective') }, { id: false, text: i18next.t('common.Invalid') }];
            elDiv.html(createRadios(data, 'id', 'text', '~Active' + flag, flag, false, false));
            return elDiv;
        },
        /**
         * 初始化輪播
         * @param {String} settype 設定類別
         * @param {String} lang 語系
         */
        fnGridInit = function (settype, lang) {
            var sKey = settype + '_' + lang,
                saFields = [{ name: "RowIndex", title: 'common.RowNumber', align: 'center', width: 40, sorting: false }];

            if ('ClassicCase'.indexOf(settype) > -1) {
                saFields.push({
                    name: "ParentId", title: 'common.ParentId', width: 120, type: 'select',// ╠common.ParentId⇒父層╣
                    itemTemplate: function (val, item) {
                        var oControl = $('<select />', {
                            class: "form-control w100p",
                            html: !oOption[sKey] ? '' : oOption[sKey].ParentIdHtml
                        }).val(val);
                        return oControl;
                    },
                    insertTemplate: function (val, item) {
                        var oControl = $('<select />', {
                            class: "form-control w100p",
                            html: !oOption[sKey] ? '' : oOption[sKey].ParentIdHtml
                        });
                        setTimeout(function () { oControl.html(!oOption[sKey] ? '' : oOption[sKey].ParentIdHtml); }, 2000);
                        return this.insertControl = oControl;
                    },
                    insertValue: function () {
                        return this.insertControl.val();
                    },
                    editTemplate: function (val, item) {
                        var oControl = $('<select />', {
                            class: "form-control w100p",
                            html: !oOption[sKey] ? '' : oOption[sKey].ParentIdHtml
                        });
                        return this.editControl = oControl.val(val);
                    },
                    editValue: function () {
                        return this.editControl.val();
                    }
                });
            }
            if ('Carousel'.indexOf(settype) === -1) {
                saFields.push({
                    name: "Title", title: { 'ServiceItems': 'common.ServiceItemsName' }[settype] || 'common.Title', width: 120, type: 'text', validate: [
                        {// ╠common.ServiceItemsName⇒服務項目名稱╣
                            validator: 'required',// ╠common.ServiceItemsName_required⇒請輸入服務項目名稱╣  ╠common.Title_required⇒請輸入標題╣
                            message: { 'ServiceItems': 'common.ServiceItemsName_required' }[settype] || i18next.t('common.Title_required')
                        },
                        {
                            validator: 'maxLength',
                            message: "Field value is too long",
                            param: 200
                        }]
                });
            }

            if ('ServiceItems,ClassicCase,ServiceBase'.indexOf(settype) > -1) {
                saFields.push({
                    name: "IconId", title: { 'ServiceBase': 'common.StrongholdIconPic' }[settype] || 'common.IconPic', width: 120,
                    itemTemplate: function (val, item) {//╠common.IconPic⇒圖標╣//╠common.StrongholdIconPic⇒據點圖標╣
                        var oFileDiv = $('<div />', { class: 'file-Info' }),
                            fileInput = $('<input />', { type: 'file', class: 'displayNone', name: 'files[]', 'multiple': 'multiple' });
                        fnGetUploadFiles(val, fnUpload, fileInput);
                        return oFileDiv.append(fileInput);
                    }
                });
            }
            if ('ClassicCase'.indexOf(settype) > -1) {
                saFields.push({
                    name: "SubIconId", title: 'common.SubIcon', width: 120,
                    itemTemplate: function (val, item) {//╠common.SubIcon⇒圖標2╣
                        var oFileDiv = $('<div />', { class: 'file-Info' }),
                            fileInput = $('<input />', { type: 'file', class: 'displayNone', name: 'files[]', 'multiple': 'multiple' });
                        fnGetUploadFiles(val, fnUpload, fileInput);
                        return oFileDiv.append(fileInput);
                    }
                });
            }
            if ('Carousel,ClassicCase,Downloads'.indexOf(settype) > -1) {
                saFields.push({
                    name: "CoverId", title: { 'Carousel': 'common.CarouselPic', 'Downloads': 'common.DownLoadDocuments' }[settype] || 'common.CoverPic', width: 120, align: 'center',
                    itemTemplate: function (val, item) {//╠common.CarouselPic⇒輪播圖片╣
                        var oFileDiv = $('<div />', { class: 'file-Info' }),
                            fileInput = $('<input />', { type: 'file', class: 'displayNone', name: 'files[]', 'multiple': 'multiple', 'data-file': settype });
                        fnGetUploadFiles(val, fnUpload, fileInput);
                        return oFileDiv.append(fileInput);
                    }
                });
            }

            if ('ServiceBase,Downloads'.indexOf(settype) === -1) {
                saFields.push({ //╠common.Link⇒連接╣
                    name: "Link", title: 'common.Link', width: 150, type: 'text', validate: {
                        validator: 'maxLength',
                        message: "Field value is too long",
                        param: 500
                    },
                    itemTemplate: function (val, item) {
                        var elLink = $('<a />', { class: 'link', 'target': '_new', text: val, href: val });
                        return elLink;
                    }
                });
            }
            if ('ServiceItems,ClassicCase,Video,ServiceBase,Downloads'.indexOf(settype) > -1) {
                saFields.push(
                    {// ╠common.CenterCoordinates⇒中心坐標╣
                        name: "Description", title: { 'ServiceBase': 'common.CenterCoordinates' }[settype] || 'common.Description', width: 150,//╠common.Description⇒描述╣
                        itemTemplate: function (val, item) {
                            val = val || '';
                            return val.length > 100 ? val.substr(0, 100) + '...' : val;
                        },
                        insertTemplate: function (val, item) {
                            var elControl = $('<textarea rows="3" cols="20" />', {
                                class: "form-control w100p"
                            });
                            return this.insertControl = elControl;
                        },
                        insertValue: function () {
                            return this.insertControl.val();
                        },
                        editTemplate: function (val, item) {
                            var elControl = $('<textarea rows="3" cols="20" />', {
                                class: "form-control w100p"
                            });
                            return this.editControl = elControl.val(val);
                        },
                        editValue: function () {
                            return this.editControl.val();
                        }
                    });
            }
            if ('ServiceItems,ClassicCase,ServiceBase'.indexOf(settype) > -1) {
                if (settype === 'ServiceBase') {
                    saFields.push(
                        {// ╠common.GlobalStronghold⇒全球據點坐標╣
                            name: "Content", title: 'common.GlobalStronghold', width: 150,
                            insertTemplate: function (val, item) {
                                var elControl = $('<textarea rows="3" cols="20" />', {
                                    class: "form-control w100p"
                                });
                                return this.insertControl = elControl;
                            },
                            insertValue: function () {
                                return this.insertControl.val();
                            },
                            editTemplate: function (val, item) {
                                var elControl = $('<textarea rows="3" cols="20" />', {
                                    class: "form-control w100p"
                                });
                                return this.editControl = elControl.val(val);
                            },
                            editValue: function () {
                                return this.editControl.val();
                            }
                        });
                }
                else {
                    saFields.push({
                        name: "Content", title: { 'ServiceItems': 'common.ServiceItemsInfo' }[settype] || 'common.Info', width: 80, align: 'center',
                        itemTemplate: function (val, item) {//╠common.CarouselPic⇒輪播圖片╣   ╠common.ServiceItemsInfo⇒服務項目明細╣ ╠common.Info⇒內容明細╣
                            var oEdit = $('<a />', {
                                class: 'link', text: i18next.t('common.Edit'),
                                click: function () {
                                    layer.open({
                                        type: 1, //0（信息框，默认）1（页面层）2（iframe层）3（加载层）4（tips层）
                                        title: i18next.t('common.Info'),
                                        area: ['960px', '600px'],//寬度
                                        shade: 0.75,//遮罩
                                        shadeClose: true,
                                        maxmin: true, //开启最大化最小化按钮
                                        id: 'layer_Info', //设定一个id，防止重复弹出
                                        offset: '50px',
                                        anim: 0,//彈出動畫
                                        btn: [i18next.t('common.Confirm'), i18next.t('common.Cancel')],
                                        btnAlign: 'c',//按鈕位置
                                        content: $('#UeditorInfo'),
                                        success: function (layero, index) {
                                            var iframe = layero.find('iframe').contents(),
                                                iZindex_layer = layero.css('z-index');
                                            setTimeout(function () {
                                                iframe.find('body').html(item.Content || '');
                                            }, 200);
                                            layero.find('#UE_SettingInfo').children(":first").css('z-index', iZindex_layer * 1 + 1);//為解決POP視窗ueditor上傳pop被遮擋之問題
                                        },
                                        yes: function (index, layero) {
                                            item.Content = canDo.UE_Editor.SettingInfo.getContent();
                                            fnSetUpdate(item).done(function () {
                                                layer.close(index);
                                            });
                                        },
                                        end: function () {
                                            $('#UeditorInfo').hide();
                                        }
                                    });
                                }
                            });
                            return oEdit;
                        }
                    });
                }
            }
            saFields.push(
                {//╠common.Styles⇒坐標樣式╣
                    name: "Memo", title: { 'ServiceBase': 'common.Styles' }[settype] || 'common.Memo', width: 150,
                    insertTemplate: function (val, item) {
                        var elControl = $('<textarea rows="3" cols="20" />', {
                            class: "form-control w100p"
                        });
                        return this.insertControl = elControl;
                    },
                    insertValue: function () {
                        return this.insertControl.val();
                    },
                    editTemplate: function (val, item) {
                        var elControl = $('<textarea rows="3" cols="20" />', {
                            class: "form-control w100p"
                        });
                        return this.editControl = elControl.val(val);
                    },
                    editValue: function () {
                        return this.editControl.val();
                    }
                },
                {
                    name: "OrderByValue", title: 'common.OrderByValue', width: 80, align: 'center',
                    itemTemplate: function (val, item) {
                        return this._createSelect = $("<select>", {
                            class: 'w70',
                            html: createOptions(item.OrderCount),
                            change: function () {
                                var sOldValue = val,
                                    sNewValue = this.value;
                                g_api.ConnectLite(canDo.ProgramId, canDo._api.order, {
                                    Id: item.Guid,
                                    OldOrderByValue: sOldValue,
                                    NewOrderByValue: sNewValue
                                }, function (res) {
                                    if (res.RESULT) {
                                        oGrid[sKey].openPage(1);
                                    }
                                });
                            }
                        }).val(val);
                    }
                },
                {
                    name: "Active", title: 'common.Active', width: 100, align: 'center',
                    itemTemplate: function (val, item) {//╠common.Active⇒有效狀態╣
                        return val.toString() === "true" ? i18next.t('common.Effective') : i18next.t('common.Invalid');
                    },
                    insertTemplate: function (val, item) {
                        var elControl = fnCreateActiveInput('add' + sKey);
                        setTimeout(function () {
                            elControl.find('label:first').click();
                            canDo._uniformInit(elControl);
                        }, 100);
                        return this.insertControl = elControl;
                    },
                    insertValue: function () {
                        return this.insertControl.find(':input:checked').val();
                    },
                    editTemplate: function (val, item) {
                        var elControl = fnCreateActiveInput('edit' + sKey);
                        setTimeout(function () {
                            elControl.find(':input[value="' + val + '"]').click();
                            canDo._uniformInit(elControl);
                        }, 100);
                        return this.editControl = elControl;
                    },
                    editValue: function () {
                        return this.editControl.find(':input:checked').val();
                    }
                },
                {
                    type: "control", width: 50
                });

            $('#jsGrid_' + sKey).jsGrid({
                width: "100%",
                height: "auto",
                autoload: true,
                pageLoading: true,
                inserting: true,
                editing: true,
                sorting: true,
                paging: true,
                pageIndex: 1,
                pageSize: 1000,
                pageButtonCount: parent.SysSet.GridPages || 15,
                invalidMessage: '输入的数据无效！',
                confirmDeleting: true,
                deleteConfirm: "確定要刪除嗎？",
                pagePrevText: "<",
                pageNextText: ">",
                pageFirstText: "<<",
                pageLastText: ">>",
                rowClick: function (args) {
                },
                fields: saFields,
                controller: {
                    loadData: function (args) {
                        return fnGetSetting(args, settype, lang);
                    },
                    insertItem: function (args) {
                        args.SetType = settype;
                        args.LangId = oLang[lang];
                        args.IconId = guid();
                        args.SubIconId = guid();
                        args.CoverId = guid();
                        args.SubCoverId = guid();
                        var saCurrent = [];
                        if (oGrid[sKey].data.length > 0) {
                            saCurrent = $.grep(oGrid[sKey].data, function (item) {
                                if (args.ParentId) {
                                    return item.ParentId === args.ParentId;
                                } else {
                                    return !item.ParentId;
                                }
                            });
                        }
                        args.OrderByValue = saCurrent.length + 1;
                        return fnSetInsert(args);
                    },
                    updateItem: function (args) {
                        fnSetUpdate(args);
                    },
                    deleteItem: function (args) {
                        return fnSetDelete(args);
                    }
                },
                onInit: function (args) {
                    if (settype === 'ClassicCase' && !oOption[sKey]) {
                        oOption[sKey] = {};
                    }
                    oGrid[sKey] = args.grid;
                }
            });
        },
        /**
         * 上傳附件
         * @param {Array} files 當前文件
         * @param {String} parentid 父層id
         * @param {String} finput file input id
         */
        fnUpload = function (files, parentid, finput) {
            var option = {},
                sFilesType = finput.attr('data-file');
            option.input = finput;
            option.limit = 1;
            option.type = 'one';
            option.theme = 'dragdropbox' + parentid;
            option.parentid = parentid;
            option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png', 'svg'];
            option.folder = 'WebSiteSetup';
            if (files) {
                option.files = files;
            }
            if (sFilesType === 'Downloads') {
                delete option.extensions;
            }
            else if (sFilesType === 'PicShowId') {
                option.limit = 99;
                delete option.type;
            }
            fnUploadRegister(option);
        };
};

require(['base', 'filer', 'jsgrid', 'cando'], fnPageInit);