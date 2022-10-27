$(function () {
    'use strict';

    var fnRenderList = function (handle, list) {
        var sHtmlList = $('#temp_list').render(list);
        handle.html(sHtmlList).find('.download-button').on('click', function () {
            var sName = $(this).attr('filename'),
                sPath = $(this).attr('filepath');
            DownLoadFile(sPath, sName);
        });
    },
        fnGetFileList = function (handle, parentid) {
            return g_api.ConnectLite(Service.apiappcom, ComFn.GetFileList, {
                ParentID: parentid
            }, function (res) {
                if (res.RESULT) {
                    var saRes = res.DATA.rel;
                    fnRenderList(handle, saRes);
                }
            });
        },
        init = function () {
            var sLang = $('[http-equiv="content-language"]').attr('content') || 'zh-TW',
                myHelpers = {
                    setFileName: function (val) {
                        return val.split('.')[0];
                    },
                    setDescription: function (val) {
                        return !val ? '-' : val;
                    },
                    setFilePath: function (val) {
                        return gServerUrl + '/' + val.replace(/\\/g, "\/");
                    },
                    checkSubFileName: function (val) {
                        return val.toLowerCase() === 'pdf';
                    }
                },
                oTempl = {
                    'zh-TW': ['FileList1', 'FileList2'],
                    'zh': ['FileList3', 'FileList4'],
                    'en': ['FileList5']
                };
            $.views.helpers(myHelpers);

            $.each(oTempl[sLang], function (idx, item) {
                return g_api.ConnectLite(Service.apiwebcom, ComFn.GetFileInfo, {
                    Id: item
                }, function (res) {
                    if (res.RESULT) {
                        var oRes = res.DATA.rel;
                        if (oRes && oRes.FileID) {
                            fnGetFileList($('#' + item), oRes.FileID);
                        }
                    }
                });
            });
        };

    init();
});