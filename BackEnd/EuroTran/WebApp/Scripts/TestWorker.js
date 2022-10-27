var i = 0;
function mainFunc() {
    i++;
    //把i发送到浏览器的js引擎线程里
    postMessage(i);
}
var id = setInterval(mainFunc, 1);

//<input type="text" name="ipt" id="ipt" value="" />
//<button id="start">start</button>
//<button id="stop">stop</button>
//<button id="ale">alert</button>

var ipt = document.getElementById("ipt");
var stop = document.getElementById("stop");
var start = document.getElementById("start");
var ale = document.getElementById("ale");
var worker = new Worker("/Scripts/TestWorker.js");
worker.onmessage = function () {
    ipt.value = event.data;
};
stop.addEventListener("click", function () {
    //用于关闭worker线程
    worker.terminate();
});
start.addEventListener("click", function () {
    //开起worker线程
    worker = new Worker("/Scripts/TestWorker.js");
    worker.onmessage = function () {
        ipt.value = event.data;
    };
});
ale.addEventListener("click", function () {
    alert("i'm a dialog");
});