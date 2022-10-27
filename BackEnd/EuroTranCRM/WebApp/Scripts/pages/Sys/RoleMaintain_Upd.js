'use strict';
var fnPageInit = function () {
    var canDo = new CanDo({
        /**
         * 當前程式所有ID名稱集合
         */
        idKeys: ['OrgID', 'RuleID'],
        /**
         * 當前程式所有參數名稱集合
         */
        paramKeys: ['RuleID'],
        /**
         * 處理新增資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getInsertParams: function (pargs, data) {
            data.users = [];
            $('#lstRight option').each(function () {
                data.users.push(this.value);
            });
            return data;
        },
        /**
         * 處理修改資料參數
         * @param  {Object} pargs CanDo 對象
         * @param  {Object} data 當前表單資料
         */
        getUpdateParams: function (pargs, data) {
            data = pargs.options.getInsertParams(pargs, data);
            return data;
        },
        /**
         * 頁面初始化
         * @param  {Object} pargs CanDo 對象
         */
        pageInit: function (pargs) {
            var pGet = null;
            if (pargs.action === 'upd') {
                $('#RuleID').prop('disabled', true);
                pGet = pargs._getOne();
            }
            else {
            }

            fnSetUserDrop([
                {
                    Action: canDo.action,
                    CallBack: function (data) {
                        var saLeft = data,
                            saRight = [],
                            getOptions = function () {
                                $('#lstLeft').html(createOptions(saLeft, 'MemberID', 'MemberName')).find('option:first').remove();
                                $('#lstRight').html(createOptions(saRight, 'MemberID', 'MemberName')).find('option:first').remove();
                                optionListSearch($('#lstLeft'), $('#lstRight'), $('#WorkSearch'));
                            };
                        if (pargs.action === 'upd') {
                            pGet.done(function () {
                                $.each(data, function (idx, item) {
                                    if ((canDo.data.ExFeild1 || '').indexOf(item.MemberID) > -1) {
                                        saRight.push(item);
                                    }
                                    else {
                                        saLeft.push(item);
                                    }
                                });
                                getOptions();
                            });
                        }
                        else {
                            getOptions();
                        }
                    }
                }
            ]);

            $('#btnToRight').click(function () {
                optionListMove($('#lstLeft'), $('#lstRight'));
            });
            $('#btnToLeft').click(function () {
                optionListMove($('#lstRight'), $('#lstLeft'));
            });
        }
    });
};

require(['base', 'cando'], fnPageInit);