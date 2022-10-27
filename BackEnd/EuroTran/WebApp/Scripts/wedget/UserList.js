$.widget('custom.UserList', {
    options: {
        custom: {},
        common: {},
        fncallback: null,
        btext: {}
    },

    _create: function () {
        var self = this;
    },
    destroy: function () {
        this.element.empty();
        $.Widget.prototype.destroy.call(this);
    }
});