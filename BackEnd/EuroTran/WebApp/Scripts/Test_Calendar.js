//1.读取日历属性
function onReadCalendar() {
    WL.login({
        scope: "wl.calendars"
    }).then(
        function (response) {
            WL.api({
                path: "calendar.4fa89fa6142690ab",
                method: "GET"
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//2.删除日历
function onDeleteCalendar() {
    WL.login({
        scope: "wl.calendars_update"
    }).then(
        function (response) {
            WL.api({
                path: "calendar.a6b2a7e8f2515e5e.11a088a04c28495e8672ecf2bf645461",
                method: "DELETE"
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML = "Deleted.";
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//3.创建日历
function onCreateCalendar() {
    WL.login({
        scope: "wl.calendars_update "
    }).then(
        function (response) {
            debugger;
            WL.api({
                path: "me/calendars",
                method: "POST",
                body: {
                    name: "My example calendar"
                }
            }).then(
                function (response) {
                    debugger;
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    debugger;
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}

//3.更新日历属性
function onCreateCalendar() {
    WL.login({
        scope: "wl.calendars_update "
    }).then(
        function (response) {
            WL.api({
                path: "me/calendars",
                method: "POST",
                body: {
                    name: "My example calendar"
                }
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//4.更新日历属性
function onUpdateCalendar() {
    WL.login({
        scope: "wl.calendars_update"
    }).then(
        function (response) {
            WL.api({
                path: "calendar.a6b2a7e8f2515e5e.11a088a04c28495e8672ecf2bf645461",
                method: "PUT",
                body: {
                    name: "My example calendar updated"
                }
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//读取日历事件
function onReadEvent() {
    WL.login({
        scope: "wl.calendars"
    }).then(
        function (response) {
            debugger;
            WL.api({
                path: "me/events",
                method: "GET"
            }).then(
                function (response) {
                    debugger;
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Location: " + response.location;
                },
                function (responseFailed) {
                    debugger;
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//6.删除日历事件
function onDeleteEvent() {
    WL.login({
        scope: "wl.calendars_update"
    }).then(
        function (response) {
            WL.api({
                path: "event.a6b2a7e8f2515e5e.16c27f2b66ac4ffdae6d59236b692e20.21e30d8fc00841b8841dd16c7ec7a503",
                method: "DELETE"
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML = "Deleted";
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}

//7.創建日历事件
function onCreateEvent() {
    WL.login({
        scope: "wl.events_create"
    }).then(
        function (response) {
            debugger;
            WL.api({
                path: "me/events",
                method: "POST",
                body: {
                    name: "Family Dinner",
                    description: "Dinner with Cynthia's family",
                    start_time: "2018-04-18T09:30:00-08:00",
                    end_time: "2018-04-18T11:00:00-08:00",
                    location: "Coho Vineyard and Winery, 123 Main St., Redmond WA 19532",
                    is_all_day_event: "false",
                    availability: "busy",
                    visibility: "public"
                }
            }).then(
                function (response) {
                    debugger;
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    debugger;
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}
//8.更新日历事件
function onUpdateEvent() {
    WL.login({
        scope: "wl.calendars_update"
    }).then(
        function (response) {
            WL.api({
                path: "event.a6b2a7e8f2515e5e.16c27f2b66ac4ffdae6d59236b692e20.21e30d8fc00841b8841dd16c7ec7a503",
                method: "PUT",
                body: {
                    name: "My example event has changed"
                }
            }).then(
                function (response) {
                    document.getElementById("resultDiv").innerHTML =
                        "ID: " + response.id +
                        "<br/>Name: " + response.name;
                },
                function (responseFailed) {
                    document.getElementById("infoArea").innerText =
                        "Error calling API: " + responseFailed.error.message;
                }
            );
        },
        function (responseFailed) {
            document.getElementById("infoArea").innerText =
                "Error signing in: " + responseFailed.error_description;
        }
    );
}

$(function () {
    $(':input[type="button"]').on('click', function () {
        switch (this.id) {
            case 'id1':
                onReadCalendar();//读取日历属性
                break;
            case 'id2':
                onDeleteCalendar();//删除日历
                break;
            case 'id3':
                onCreateCalendar();//创建日历
                break;
            case 'id4':
                onUpdateCalendar();//更新日历属性
                break;
            case 'id5':
                onReadEvent();//读取日历事件
                break;
            case 'id6':
                onDeleteEvent();//删除日历事件
                break;
            case 'id7':
                onCreateEvent();//建日历事件
                break;
            case 'id8':
                onUpdateEvent();//更新日历事件
                break;
        }
    });
});

$(function () {
    // Check for browser support for sessionStorage
    if (typeof (Storage) === 'undefined') {
        render('#unsupportedbrowser');
        return;
    }

    // Check for browser support for crypto.getRandomValues
    var cryptObj = window.crypto || window.msCrypto; // For IE11
    if (cryptObj === undefined || cryptObj.getRandomValues === 'undefined') {
        render('#unsupportedbrowser');
        return;
    }

    render(window.location.hash);

    $(window).on('hashchange', function () {
        render(window.location.hash);
    });

    function render(hash) {
        var action = hash.split('=')[0];

        // Hide everything
        $('.main-container .page').hide();

        var isAuthenticated = false;

        var pagemap = {
            // Welcome page
            '': function () {
                renderWelcome(isAuthenticated);
            },

            // Receive access token

            // Signout

            // Error display

            // Display inbox

            // Shown if browser doesn't support session storage
            '#unsupportedbrowser': function () {
                $('#unsupported').show();
            }
        }

        if (pagemap[action]) {
            pagemap[action]();
        } else {
            // Redirect to home page
            window.location.hash = '#';
        }
    }

    function setActiveNav(navId) {
        $('#navbar').find('li').removeClass('active');
        $(navId).addClass('active');
    }

    function renderWelcome(isAuthed) {
        if (isAuthed) {
            $('#username').text(sessionStorage.userDisplayName);
            $('#logged-in-welcome').show();
            setActiveNav('#home-nav');
        } else {
            $('#connect-button').attr('href', buildAuthUrl());
            $('#signin-prompt').show();
        }
    }

    // OAUTH FUNCTIONS =============================

    // OUTLOOK API FUNCTIONS =======================

    // HELPER FUNCTIONS ============================
});