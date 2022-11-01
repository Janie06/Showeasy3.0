'use strict';
var sProgramId = getProgramId(),
    sQueryPrgId = getQueryPrgId(),
    fnPageInit = function () {
        var oCurData = {
            ParentId1: '12345678910',
            ParentId2: '12345678911',
            ParentId3: '12345678912',
            Filelist1: [],
            Filelist2: [],
            Filelist3: []
        },
            oValidator = null,
            /*
             * ToolBar 按鈕事件 function
             * @param   {Object}inst 按鈕物件對象
             * @param   {Object} e 事件對象
             * @return  無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            fnButtonHandler = function (inst, e) {
                var sId = inst.id;
                switch (sId) {
                    case "Toolbar_Qry":

                        break;
                    case "Toolbar_Save":

                        break;
                    case "Toolbar_ReAdd":

                        break;
                    case "Toolbar_Clear":

                        clearPageVal();

                        break;
                    case "Toolbar_Leave":

                        pageLeave();

                        break;

                    case "Toolbar_Add":

                        break;
                    case "Toolbar_Upd":

                        break;
                    case "Toolbar_Copy":

                        break;
                    case "Toolbar_Del": // ╠message.ConfirmToDelete⇒確定要刪除嗎 ?╣ ╠common.Tips⇒提示╣

                        layer.confirm(i18next.t("message.ConfirmToDelete"), { icon: 3, title: i18next.t('common.Tips') }, function (index) {
                            fnDel();
                            layer.close(index);
                        });

                        break;
                    default:

                        alert("No handle '" + sId + "'");

                        break;
                }
            },
            /*
             * 初始化 function
             * @param 無
             * @return  無
             * 起始作者：John
             * 起始日期：2016/05/21
             * 最新修改人：John
             * 最新修日期：2016/11/03
             */
            init = function () {
                var callback1 = function (files) {
                    var option = {};
                    option.input = $('#fileInput1');
                    option.limit = 1;
                    option.extensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'png'];
                    option.theme = 'box2';
                    option.folder = 'Test';
                    option.type = 'one';
                    option.parentid = oCurData.ParentId1;
                    option.files = files;
                    fnUploadRegister(option);
                };
                var callback2 = function (files) {
                    var option = {};
                    option.input = $('#fileInput2');
                    option.theme = 'box1';
                    option.folder = 'Test';
                    option.parentid = oCurData.ParentId2;
                    option.files = files;
                    fnUploadRegister(option);
                };
                var callback3 = function (files) {
                    var option = {};
                    option.input = $('#fileInput3');
                    option.theme = 'box2';
                    option.folder = 'Test';
                    option.type = 'list';
                    option.parentid = oCurData.ParentId3;
                    option.files = files;
                    fnUploadRegister(option);
                };
                $.whenArray([fnGetUploadFiles(oCurData.ParentId1, callback1),
                fnGetUploadFiles(oCurData.ParentId2, callback2),
                fnGetUploadFiles(oCurData.ParentId3, callback3)]);
            };

        init();
    };

//require(['base', 'filer', 'util'], fnPageInit);
var jsfnew = bundles.base.concat(bundles.filer, bundles.util);
loadjs(jsfnew, 'jsfnew', { async: false });
loadjs.ready(['jsfnew'], fnPageInit);