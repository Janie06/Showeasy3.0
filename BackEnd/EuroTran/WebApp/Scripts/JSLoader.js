'use strict';
var version = '?v=201904291',
    bundles = {
        base: [//基本所有頁面都需要用到的js
            '/Scripts/plugins/jquery-ui/jquery-ui.min.js',
            '/Scripts/plugins/bootstrap/js/bootstrap.min.js',
            '/Scripts/plugins/jquery-blockui/jquery.blockui.min.js',
            '/Scripts/plugins/jquery-slimscroll/jquery.slimscroll.min.js',
            '/Scripts/plugins/jquery-validation/jquery.validate.min.js',
            '/Scripts/plugins/uniform/jquery.uniform.min.js',
            '/Scripts/plugins/toastr/toastr.min.js',
            '/Scripts/3rd/underscore-1.9.0.min.js',
            '/Scripts/3rd/jquery.serialize-object.min.js',
            '/Scripts/3rd/linq/linq.min.js',
            '/Scripts/3rd/jsrender.min.js',
            '/Scripts/3rd/base64.min.js',
            '/Scripts/MessageBox.min.js',
            '/Scripts/wedget/ToolBar.js' + version,
            '/Scripts/Common.min.js' + version
        ],
        base_css: [
            '/Content/googleapis.css',
            '/Scripts/plugins/jquery-ui/jquery-ui.min.css',
            '/Scripts/plugins/bootstrap/css/bootstrap.min.css',
            '/Scripts/plugins/fontawesome/css/font-awesome.css',
            '/Scripts/plugins/line-icons/simple-line-icons.css',
            '/Scripts/plugins/uniform/css/uniform.default.min.css',
            '/Scripts/plugins/toastr/toastr.min.css',
            '/Content/modern.min.css',
            '/Content/custom.min.css',
            '/Content/themes/red.css',
            '/Content/style2.0.css',
        ],
        timepicker: [//時間選擇器的要引用
            '/Scripts/3rd/datetimepicker-addon/jquery-ui-timepicker-addon.min.js',
            '/Scripts/3rd/datetimepicker-addon/i18n/jquery-ui-timepicker-zh-TW.js'
        ],
        timepicker_css: [//時間選擇器的要引用
            '/Scripts/3rd/datetimepicker-addon/jquery-ui-timepicker-addon.min.css',
            '/Scripts/3rd/datetimepicker-addon/i18n/jquery-ui-timepicker-zh-TW.js'
        ],
        daterangepicker: [//日曆控件需要選擇區間的要引用
            '/Scripts/3rd/daterangepicker/moment.min.js',
            '/Scripts/3rd/daterangepicker/jquery.daterangepicker.js'
        ],
        daterangepicker_css: ['/Scripts/3rd/daterangepicker/css/daterangepicker.css'],
        clndr: ['/Scripts/3rd/clndr/moment.js', '/Scripts/3rd/clndr/clndr.js'],//（月）行事曆
        clndr_css: ['/Scripts/3rd/clndr/clndr.css'],
        jbox: ['/Scripts/3rd/jBox/jBox.js'],//提示插件
        jbox_css: ['/Scripts/3rd/jBox/jBox.css'],
        convetlng: ['/Scripts/lib/ConvertLang.min.js'],//多語系轉換js
        jqprint: ['/Scripts/3rd/jquery.jqprint-0.3.js'],//列印插件
        jqtree: ['/Scripts/3rd/jqtree/tree.jquery.min.js'],//樹插件
        jqtree_css: ['/Scripts/3rd/jqtree/jqtree.css'],//樹插件
        spectrum: ['/Scripts/3rd/Spectrum/spectrum.js'],//顏色選擇器
        spectrum_css: ['/Scripts/3rd/Spectrum/spectrum.css'],
        autocompleter: ['/Scripts/3rd/quickQuery/jquery.autocompleter.min.js' + version],//模糊搜索插件
        autocompleter_css: ['/Scripts/3rd/quickQuery/jquery.autocompleter.css'],
        jquerytoolbar: ['/Scripts/3rd/jquery.toolbar/jquery.toolbar.min.js'],//客製化按鈕插件
        jquerytoolbar_css: ['/Scripts/3rd/jquery.toolbar/jquery.toolbar.css'],
        formatnumber: ['/Scripts/3rd/format-number.min.js'],//金額插件
        select2: ['/Scripts/3rd/select2/js/select2.full.min.js'],//select2選擇器插件
        select2_css: ['/Scripts/3rd/select2/css/select2.min.css'],
        jsgrid: ['/Scripts/3rd/jGrid/jsgrid.min.js' + version],//grid插件
        jsgrid_css: ['/Scripts/3rd/jGrid/jsgrid.min.css', '/Scripts/3rd/jGrid/jsgrid-theme.min.css'],
        ajaxfile: ['/Scripts/3rd/jquery.filer/ajaxfileupload.js'],//上傳插件
        filer: [//上傳插件
            '/Scripts/3rd/jquery.filer/jquery.filer.js' + version,
            '/Scripts/3rd/jquery.dragsort/jquery.dragsort-0.5.2.min.js'
        ],
        filer_css: [
            '/Scripts/3rd/jquery.filer/jquery.filer.css',
            '/Scripts/3rd/jquery.filer/jquery.filer-dragdropbox-theme.css',
            '/Scripts/3rd/jquery.filer/file.theme.list.css'
        ],
        common_opm: ['/Scripts/Common.Opm.min.js' + version],
        common_eip: ['/Scripts/Common.Eip.js' + version],
        util: ['/Scripts/lib/Util.min.js' + version],
        cando: ['/Scripts/lib/PageUtil.min.js' + version]
    },

    /**
     * 通過url獲取程式id
     * @param  {String} path 文件路徑
     * @return {String} json字串
     */
    getProgramId = function (path) {
        var sHerf = path || document.location.href,
            saHerf = sHerf.split('/');
        saHerf = saHerf[saHerf.length - 1].split('.');

        return saHerf[0] || '';
    },
    /**
     * 通過編輯程式ID獲取查詢程式ID
     * @return {String} 編輯頁面程式ID
     */
    getQueryPrgId = function () {
        var sPrgId = getProgramId();
        return sPrgId.replace('_Upd', '_Qry');
    },
    /**
     * 通過查詢程式ID獲取編輯頁面程式ID
     * @return {String} 編輯頁面程式ID
     */
    getEditPrgId = function () {
        var sPrgId = getProgramId();
        return sPrgId.replace('_Qry', '_Upd');
    },
    /**
     * 取得Url參數
     * @param  {String} name 取得部分的名稱 例如輸入"Action"，就能取到"Add"之類參數
     * @return {String}參數值
     */
    getUrlParam = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //構造一個含有目標參數的正則表達式對象
        var r = window.location.search.substr(1).match(reg);  //匹配目標參數
        if (r !== null) return unescape(r[2]); return null; //返回參數值
    },
    /**
     * 取得Url參數
     * @param  {Array} bundleIds 要加載的文件（js）組名稱
     * @param  {Function} callbackFn 文件（js）加載完回調函數
     * @param  {Boolean} asyncs 需要同步加載執行的文件組名稱
     */
    require = function (bundleIds, callbackFn, asyncs) {
        asyncs = asyncs || '';
        bundleIds.forEach(function (bundleId) {
            var basync = true,
                bundleCssId = bundleId + '_css';
            if (asyncs.indexOf(bundleId) > -1) {
                basync = false;
            }
            if (!loadjs.isDefined(bundleId)) {
                loadjs(bundles[bundleId], bundleId, { async: basync });
                if (!loadjs.isDefined(bundleCssId) && bundles[bundleCssId]) {
                    //loadjs(bundles[bundleCssId], bundleCssId);
                }
            }
        });
        loadjs.ready(bundleIds, callbackFn);
    };