$(document).ready(function () {
    $(document).ready(function () {
        // Gets total amount of slides
        var count = $(".slide").length;

        // Sets the slide-container and individual slide size
        $(".slide-container").css({ "width": 100 * count + "%", 'margin-left': '-400%' });
        $(".slide").css("width", $(".slide-container").width() / count + "px");

        var rollLeft = function () {
            $(".slide-container").animate({ marginLeft: "-500%" }, 400, "linear", function () {
                $(".slide-container").css({ marginLeft: "-400%" });
                $(".slide-container .slide:first").remove().clone(true).appendTo(".slide-container");
            });
        },
            rollRight = function () {
                $(".slide-container").animate({ marginLeft: "-300%" }, 400, "linear", function () {
                    $(".slide-container").css({ marginLeft: "-400%" });
                    $(".slide-container .slide:last").remove().clone(true).prependTo(".slide-container");
                });
            },
            startRoll = setInterval(rollLeft, 7000);

        $('.prev').click(function (e) {
            e.preventDefault();
            clearInterval(startRoll);
            rollRight();
            startRoll = setInterval(rollLeft, 7000);
        });
        $('.next').click(function (e) {
            e.preventDefault();
            clearInterval(startRoll);
            rollLeft();
            startRoll = setInterval(rollLeft, 7000);
        });
        $(".slider-box").on("swipeleft", function () {
            $(".next").click();
        });
        $(".slider-box").on("swiperight", function () {
            $(".prev").click();
        });
        $(window).resize(function () {
            $(".slide").css("width", $(".slide-container").width() / count + "px");
        });
    });
});