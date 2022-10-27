var widgetInit = function () {
    $.widget('custom.ToolBar', {
        options: {
            custom: {},
            common: {
                classname: 'btn-custom orange'
            },
            fncallback: null,
            //btext: {
            //    Add: '新增', ReAdd: '儲存後新增', Save: '儲存', Add: '新增', Del: '刪除', Clear: '清除', Upd: '修改', Qry: '查詢',
            //    Exp: '匯出', Imp: '匯入', Leave: '離開', Cpy: '複製', Print: '列印'
            //}
            btext: {
                //╠common.Toolbar_View⇒檢視╣╠common.Toolbar_Add⇒新增╣╠common.Toolbar_ReAdd⇒儲存后新增╣╠common.Toolbar_Save⇒儲存╣╠common.Toolbar_Del⇒刪除╣╠common.Toolbar_Clear⇒清除╣╠common.Toolbar_Upd⇒修改╣╠common.Toolbar_Qry⇒查詢╣
                //╠common.Toolbar_Exp⇒匯出╣╠common.Toolbar_Imp⇒匯入╣╠common.Toolbar_Leave⇒離開╣╠common.Toolbar_Copy⇒複製╣╠common.Toolbar_Print⇒列印╣╠common.Toolbar_Void⇒作廢╣
                Add: 'common.Toolbar_Add', ReAdd: 'common.Toolbar_ReAdd', Save: 'common.Toolbar_Save', Del: 'common.Toolbar_Del', Clear: 'common.Toolbar_Clear', Upd: 'common.Toolbar_Upd', Qry: 'common.Toolbar_Qry',
                Exp: 'common.Toolbar_Exp', Imp: 'common.Toolbar_Imp', Leave: 'common.Toolbar_Leave', Cpy: 'common.Toolbar_Copy', Print: 'common.Toolbar_Print', Void: 'common.Toolbar_Void'
            }
        },

        _createBtn: function () {
            //return $('<input type="button" />');
            return $('<button type="button" />');
        },

        _create: function () {
            var self = this,
                hotkeys = {};

            $.each(self.options.btns, function (idx, btnInfo) {
                var btnInst = self._createBtn(),
                    dicAttr = {};
                for (var attr in btnInfo) {
                    if (typeof btnInfo[attr] !== 'function') {
                        dicAttr[attr] = btnInfo[attr];
                    }
                }

                dicAttr.id = dicAttr.id || 'Toolbar_' + dicAttr.key;
                dicAttr.name = dicAttr.id;
                //dicAttr.value = dicAttr.value || self.options.btext[dicAttr.key];
                dicAttr["data-i18n"] = dicAttr.value || self.options.btext[dicAttr.key];// new fo i18n
                dicAttr.class = dicAttr.classname || self.options.common.classname;

                btnInst.attr(dicAttr);
                self.element.append(btnInst);
                btnInst.on("click", function (e) {
                    self.options.fncallback(this, e);
                });
                if (dicAttr.hasOwnProperty("hotkey")) {
                    hotkeys[dicAttr.hotkey] = dicAttr;
                }
            });

            $(document).on('compositionend', function (e) {
                e.target.ime = false;
            });

            $(document).on('compositionstart', function (e) {
                e.target.ime = true;
            });

            $(document).on('keydown', function (e) {
                var sKey = self._keyMapping[e.keyCode],
                    btn = null;

                if (sKey === "escape") {
                    sKey = "esc";
                }
                else if (e.keyCode >= 112 && e.keyCode <= 123) {
                    sKey = "f" + (e.keyCode - 111).toString();
                }

                if (e.ctrlKey === true) {
                    sKey = "ctrl + " + sKey;
                }

                // 非中文
                if (!e.target.ime) {
                    if (sKey !== "esc" && sKey !== "enter" && sKey !== "ctrl + enter") {
                        // Input
                        if (this !== e.target && (/textarea|select/i.test(e.target.nodeName) || e.target.type === "text")) {
                            return;
                        }
                    }
                    // Process hot key
                    if (hotkeys.hasOwnProperty(sKey)) {
                        btn = hotkeys[sKey];
                    }

                    if (btn) {
                        if (!$("#" + btn.id).is(":disabled") && $("#" + btn.id).is(":visible")) {
                            $("#" + btn.id).trigger("click");
                            e.preventDefault();
                        }
                    }
                }
            });
        },
        destroy: function () {
            this.element.empty();
            $.Widget.prototype.destroy.call(this);
        },

        _keyMapping: {
            3: "break",
            8: "backspace / delete",
            9: "tab",
            12: 'clear',
            13: "enter",
            16: "shift",
            17: "ctrl ",
            18: "alt",
            19: "pause/break",
            20: "caps lock",
            27: "escape",
            32: "spacebar",
            33: "page up",
            34: "page down",
            35: "end",
            36: "home ",
            37: "left arrow ",
            38: "up arrow ",
            39: "right arrow",
            40: "down arrow ",
            41: "select",
            42: "print",
            43: "execute",
            44: "Print Screen",
            45: "insert ",
            46: "delete",
            48: "0",
            49: "1",
            50: "2",
            51: "3",
            52: "4",
            53: "5",
            54: "6",
            55: "7",
            56: "8",
            57: "9",
            58: ":",
            59: "semicolon (firefox), equals",
            60: "<",
            61: "equals (firefox)",
            63: "ß",
            64: "@ (firefox)",
            65: "a",
            66: "b",
            67: "c",
            68: "d",
            69: "e",
            70: "f",
            71: "g",
            72: "h",
            73: "i",
            74: "j",
            75: "k",
            76: "l",
            77: "m",
            78: "n",
            79: "o",
            80: "p",
            81: "q",
            82: "r",
            83: "s",
            84: "t",
            85: "u",
            86: "v",
            87: "w",
            88: "x",
            89: "y",
            90: "z",
            91: "Windows Key / Left ⌘ / Chromebook Search key",
            92: "right window key ",
            93: "Windows Menu / Right ⌘",
            96: "numpad 0 ",
            97: "numpad 1 ",
            98: "numpad 2 ",
            99: "numpad 3 ",
            100: "numpad 4 ",
            101: "numpad 5 ",
            102: "numpad 6 ",
            103: "numpad 7 ",
            104: "numpad 8 ",
            105: "numpad 9 ",
            106: "multiply ",
            107: "add",
            108: "numpad period (firefox)",
            109: "subtract ",
            110: "decimal point",
            111: "divide ",
            112: "f1 ",
            113: "f2 ",
            114: "f3 ",
            115: "f4 ",
            116: "f5 ",
            117: "f6 ",
            118: "f7 ",
            119: "f8 ",
            120: "f9 ",
            121: "f10",
            122: "f11",
            123: "f12",
            124: "f13",
            125: "f14",
            126: "f15",
            127: "f16",
            128: "f17",
            129: "f18",
            130: "f19",
            131: "f20",
            132: "f21",
            133: "f22",
            134: "f23",
            135: "f24",
            144: "num lock ",
            145: "scroll lock",
            160: "^",
            161: '!',
            163: "#",
            164: '$',
            165: 'ù',
            166: "page backward",
            167: "page forward",
            169: "closing paren (AZERTY)",
            170: '*',
            171: "~ + * key",
            173: "minus (firefox), mute/unmute",
            174: "decrease volume level",
            175: "increase volume level",
            176: "next",
            177: "previous",
            178: "stop",
            179: "play/pause",
            180: "e-mail",
            181: "mute/unmute (firefox)",
            182: "decrease volume level (firefox)",
            183: "increase volume level (firefox)",
            186: "semi-colon / ñ",
            187: "equal sign ",
            188: "comma",
            189: "dash ",
            190: "period ",
            191: "forward slash / ç",
            192: "grave accent / ñ",
            193: "?, / or °",
            194: "numpad period (chrome)",
            219: "open bracket ",
            220: "back slash ",
            221: "close bracket ",
            222: "single quote ",
            223: "`",
            224: "left or right ⌘ key (firefox)",
            225: "altgr",
            226: "< /git >",
            230: "GNOME Compose Key",
            233: "XF86Forward",
            234: "XF86Back",
            255: "toggle touchpad"
        }
    });
},
    delayWidgetInit = function () {
        if ($.widget) {
            widgetInit();
        }
        else {
            setTimeout(function () {
                delayWidgetInit();
            }, 100);
        }
    };

delayWidgetInit();