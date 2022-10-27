'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    fnPageInit = function () {
        /**
         * 目的 取得路徑功能
         * @param {String} sModID  模組ID
         * @return {Array} 菜單標籤
         */
        var getSort = function (sModID) {
            var list = "",
                saProgramList = g_db.GetDic('programList') || [];

            $.each(saProgramList, function (indx, oProgram) {
                if (oProgram.ModuleID === sModID) {
                    if (oProgram.AccountNameSort !== "") {
                        var sAccNameList = oProgram.AccountNameSort,
                            saAccNameList = sAccNameList.split("/"),
                            sModList = oProgram.ModuleIDSort,
                            saModList = sModList.split("/"),
                            color,           //定義路徑顏色
                            DivRow = "",
                            intLenght;
                        for (intLenght = 0; intLenght < saAccNameList.length; intLenght++) {
                            switch (intLenght) {
                                case 0:                     //第一層
                                    color = "Blue";
                                    DivRow = intLenght;
                                    break;
                                case 1:                     //第二層
                                    color = "Red";
                                    DivRow = intLenght;
                                    break;
                                case 2:                     //第三層
                                    color = "Green";
                                    DivRow = intLenght;
                                    break;
                                case 3:                     //第四層
                                    color = "#C4B23B";
                                    DivRow = intLenght;
                                    break;
                            }
                            var sName = saAccNameList[intLenght] + (intLenght < saAccNameList.length - 1 ? "  >" : "");
                            list += "<a id='" + saModList[intLenght] + "' style='cursor:pointer;color:" + color + "'>" + sName + "</a>";
                        }
                    }
                }
            }
            );
            return list;
        },
            oIcon = {
                ExhibitionImport_Qry: 'import-s',
                ExhibitionExport_Qry: 'export-s',
                ComlyExhibitionImport_Qry: 'import',
                ComlyExhibitionExport_Qry: 'export',
                OtherBusiness_Qry: 'OtherBusiness_Qry',
				OtherExhibitionTG_Qry: 'OtherExhibitionTG_Qry',
                AnnouncementList_Qry: 'Announcement_Qry',
                Exhibition_Qry: 'Exhibition_Qry',
                Customers_Qry: 'users'
            },
            /*
             * 目的 開啟畫面並刷新頁籤
             * @param {String} sModID  模組ID
             */
            openModule = function (sModID) {
                sModID = sModID || '';
                var list = "",
                    saProgramList = g_db.GetDic('programList') || [];

                $.each(saProgramList, function (indx, oProgram) {
                    if (oProgram.ModuleID === sModID) {
                        list += '<div id="' + oProgram.ParentID + '"  class="col-md-2 col-sm-4 col-xs-12 item-box" FilePath="#">';
                        list += '<div class="item-w">';
                        list += '<img src="/images/goback-01.png" />';
                        list += '<h2 class="c__title"><span data-i18n="common.GoParent">返回上一級...</span></h2>';//╠common.GoParent⇒返回上一級...╣
                        list += '</div></div>';
                    }
                });

                $.each(saProgramList, function (indx, oProgram) {
                    var saChild = Enumerable.From(saProgramList).Where(function (item) { return item.ParentID === oProgram.ModuleID && item.ShowInHome.toLowerCase() === 'y' && item.ShowTop !== 1; }).ToArray();
                    if (oProgram.ParentID === sModID && oProgram.ShowInHome.toLowerCase() === 'y' && oProgram.FilePath === '#' && saChild.length > 0 ||
                        sModID === '' && oProgram.ShowInHome.toLowerCase() === 'y' && oProgram.ShowTop) {
                        list += '<div id="' + oProgram.ModuleID + '" class="col-md-2 col-sm-4 col-xs-12 item-box" FilePath="' + oProgram.FilePath + '">';
                        list += '<div class="item-w">';
                        list += '<img src="/images/' + (oProgram.FilePath === '#' ? 'folder' : oIcon[oProgram.ModuleID] || 'invoice') + '.png" />';
                        list += '<h2 class="c__title"><span data-i18n="common.' + oProgram.ModuleID + '"></span></h2>';
                        list += '</div></div>';
                    }
                });

                $.each(saProgramList, function (indx, oProgram) {
                    if (oProgram.ParentID === sModID && oProgram.ShowInHome.toLowerCase() === 'y' && oProgram.ShowTop !== 1 && oProgram.FilePath !== '#') {
                        list += '<div id="' + oProgram.ModuleID + '" class="col-md-2 col-sm-4 col-xs-12 item-box" FilePath="' + oProgram.FilePath + '">';
                        list += '<div class="item-w">';
                        list += '<img src="/images/' + (oIcon[oProgram.ModuleID] || 'invoice') + '.png" />';
                        list += '<h2 class="c__title"><span data-i18n="common.' + oProgram.ModuleID + '"></span></h2>';
                        list += '</div></div>';
                    }
                });

                list += " </ul>";
                return list;
            },
            /*
             * 目的 更新模組清單
             * @param {String} sModID  模組ID
             */
            GetDiv = function (sModID) {
                var sModList = openModule(sModID),
                    sSortList = '';
                $("#layout").html(sModList).find('.item-box').click(function () {
                    var sFilepath = $(this).attr('filepath'),
                        sId = this.id;

                    if (sFilepath === '#' || sId === '') {
                        GetDiv(sId);
                    }
                    else {
                        parent.openPageTab(sId);
                    }
                });

                if (sModID) {
                    sSortList = getSort(sModID);
                }
                $("#LocSort").html(sSortList).find('a').click(function () {
                    GetDiv(this.id);
                });
                transLang($('#layout'));
            },
            /*
             * 獲取背景圖片
             * @return {Object} Ajax 物件
             */
            fnGetBackgroundImage = function () {
                return CallAjax(ComFn.W_Com, ComFn.GetOne, {
                    Type: '',
                    Params: {
                        files: {
                            ParentID: parent.OrgInfo.BackgroundImage
                        }
                    }
                }, function (res) {
                    if (res.d) {
                        var oFiles = $.parseJSON(res.d);
                        if (oFiles.FileID) {
                            $('.c__item01').attr('style', 'background: #222 url(' + gServerUrl + '/' + oFiles.FilePath.replace(/\\/g, "\/") + ') no-repeat center left;');
                        }
                    }
                });
            },
            /**
             * 初始化 function
             */
            init = function () {
                //$('#test').click(function () {
                //    debugger;
                //    parent.msgs.server.pushTransfer(parent.OrgID, 'EURPOTRAN', '轉換小助手~手動','',0);
                //});
                if (parent.OrgInfo) {
                    $('.sys-cnname').text(parent.OrgInfo.SystemCName);
                    $('.sys-enname').text(parent.OrgInfo.SystemEName);

                    fnGetBackgroundImage();
                }

                GetDiv();

                goTop();//置頂

                onresize();
            };

        init();
    };
require(['base', 'filer', 'util'], fnPageInit);