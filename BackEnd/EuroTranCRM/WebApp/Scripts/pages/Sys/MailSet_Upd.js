'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'EmailID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['EmailID'],
        /**
         * 須初始化的UEEditer 的物件ID集合
         */
        ueEditorIds: ['BodyHtml'],
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            if (pargs.action === 'upd') {
                $('#EmailID').prop('disabled', true);
				
				if(getUrlParam('EmailID') == "Appoint_TE"){
					$('.AppointSite').show();
				}
				
                pargs._getOne().done(function () {
					setTimeout(function () {
						$("#site1").val($("#ueditor_0").contents().find(".Appointa1").attr("href"));
						$("#site2").val($("#ueditor_0").contents().find(".Appointa2").attr("href"));
						$("#site3").val($("#ueditor_0").contents().find(".Appointa3").attr("href"));
					}, 1000);
				});
				
				$('#changeSitehref').click(function () {
					$("#ueditor_0").contents().find(".Appointa1").attr("href",$("#site1").val());
					$("#ueditor_0").contents().find(".Appointa2").attr("href",$("#site2").val());
					$("#ueditor_0").contents().find(".Appointa3").attr("href",$("#site3").val());
					showMsg("已更新所有展會網址", 'success');
				});
            }
        }
    });
};

require(['base', 'cando'], fnPageInit);