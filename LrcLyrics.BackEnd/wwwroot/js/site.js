// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const timer = document.getElementById("timer");
let baseTime = Date.now();

const lines = document.getElementsByClassName("lyric-line");

const approachingColor = " intensifying";
const defaultColor = " text-white-50";
const currentColor = " approaching";

if (timer) {
    setInterval(function () {
        var time = new Date((Date.now() - baseTime));
        timer.innerText = formatDate(time, "mm:ss.ff").replace(/100$/, "00");
    }, 60);
}

const highlightStep = 120;

if (document.getElementById("lyric-lines")) {
    setInterval(function () {
        var time = new Date((Date.now() - baseTime)) / 10;
        for (var i = 0; i < lines.length; i++) {
            var elementTime = parseInt(lines[i].id);
            var nextElementTime = (i < lines.length - 1) ? parseInt(lines[i + 1].id) : Number.POSITIVE_INFINITY;
            lines[i].className = lines[i].className.replace(approachingColor, defaultColor);
            lines[i].className = lines[i].className.replace(currentColor, defaultColor);
            if (elementTime < time + 30 && !(nextElementTime < time + 30)) {
                lines[i].className = lines[i].className.replace(defaultColor, currentColor);
            }
            else if (elementTime < time + 100 && !(nextElementTime < time + 100)) {
                lines[i].className = lines[i].className.replace(defaultColor, approachingColor);
            }
            if (i != lines.length - 1 &&
                lines[i].getElementsByClassName("progress").length > 0 &&
                lines[i].getElementsByClassName("progress")[0].getElementsByClassName("progress-bar").length > 0
                //&&
                //time + highlightStep >= elementTime &&
                //time - highlightStep <= parseInt(lines[i + 1].id)
            ) {
                var progress = lines[i].getElementsByClassName("progress")[0].getElementsByClassName("progress-bar")[0];
                if (progress) {
                    const leftOffset = 0;
                    const rightOffset = 100;
                    var length = parseInt(lines[i + 1].id) - elementTime - leftOffset - rightOffset;
                    var relativeTime = time - elementTime;
                    var fill = (relativeTime - leftOffset) / length;
                    if (fill < 0) {
                        fill = 0;
                    } else if (fill > 1) {
                        fill = 1
                    }
                    progress.style.width = fill * 100 + "%";
                }
            }
        }
    }, highlightStep);
}

function seek(line) {
    var previousTime = (Date.now() - baseTime);
    var difference = (parseInt(line.id) * 10) - previousTime;
    baseTime -= difference + 20;
}

for (var i = 0; i < lines.length; i++) {
    let line = lines[i];
    //line.addEventListener("touchstart", () => seek(line));
    line.addEventListener("click", () => seek(line));
}

const timestampsCookieName = "timestampsOn";
let timestampsShown = false;
const timestamps = document.getElementsByClassName("line-timestamp");
const toggleTimestampsButton = document.getElementById("toggle-timestamps");

function toggleTimestamps() {
    for (var i = 0; i < timestamps.length; i++) {
        if (timestampsShown) {
            timestamps[i].className = timestamps[i].className.replace(" d-inline", " d-none");
        } else {
            timestamps[i].className = timestamps[i].className.replace(" d-none", " d-inline");
        }
    }
    timestampsShown = !timestampsShown;
    toggleTimestampsButton.textContent = timestampsShown ? "Only text" : "Timestamps";
    setCookie(timestampsCookieName, timestampsShown, 360);
}

if (getCookie(timestampsCookieName) && getCookie(timestampsCookieName) !== timestampsShown.toString()) {
    toggleTimestamps();
}
if (toggleTimestampsButton) {
    toggleTimestampsButton.addEventListener("click", () => toggleTimestamps());
}

const infoShowCookieName = "infoDisplayOn";

let infoShown = false;
const lyricsInfo = document.getElementById("lyrics-info")
const toggleInfoButton = document.getElementById("toggle-info");

function toggleInfo() {
    if (infoShown) {
        lyricsInfo.className += " d-none";
    } else {
        lyricsInfo.className = lyricsInfo.className.replace(" d-none", "");
    }
    infoShown = !infoShown;
    toggleInfoButton.textContent = infoShown ? "Less" : "More"
    setCookie(infoShowCookieName, infoShown, 360);
}

if (getCookie(infoShowCookieName) && getCookie(infoShowCookieName) !== infoShown.toString()) {
    toggleInfo();
}
if (toggleInfoButton) {
    toggleInfoButton.addEventListener("click", () => toggleInfo());
}

function formatDate(date, format, utc) {
    var MMMM = ["\x00", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var MMM = ["\x01", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var dddd = ["\x02", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var ddd = ["\x03", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    function ii(i, len) {
        var s = i + "";
        len = len || 2;
        while (s.length < len) s = "0" + s;
        return s;
    }

    var y = utc ? date.getUTCFullYear() : date.getFullYear();
    format = format.replace(/(^|[^\\])yyyy+/g, "$1" + y);
    format = format.replace(/(^|[^\\])yy/g, "$1" + y.toString().substr(2, 2));
    format = format.replace(/(^|[^\\])y/g, "$1" + y);

    var M = (utc ? date.getUTCMonth() : date.getMonth()) + 1;
    format = format.replace(/(^|[^\\])MMMM+/g, "$1" + MMMM[0]);
    format = format.replace(/(^|[^\\])MMM/g, "$1" + MMM[0]);
    format = format.replace(/(^|[^\\])MM/g, "$1" + ii(M));
    format = format.replace(/(^|[^\\])M/g, "$1" + M);

    var d = utc ? date.getUTCDate() : date.getDate();
    format = format.replace(/(^|[^\\])dddd+/g, "$1" + dddd[0]);
    format = format.replace(/(^|[^\\])ddd/g, "$1" + ddd[0]);
    format = format.replace(/(^|[^\\])dd/g, "$1" + ii(d));
    format = format.replace(/(^|[^\\])d/g, "$1" + d);

    var H = utc ? date.getUTCHours() : date.getHours();
    format = format.replace(/(^|[^\\])HH+/g, "$1" + ii(H));
    format = format.replace(/(^|[^\\])H/g, "$1" + H);

    var h = H > 12 ? H - 12 : H == 0 ? 12 : H;
    format = format.replace(/(^|[^\\])hh+/g, "$1" + ii(h));
    format = format.replace(/(^|[^\\])h/g, "$1" + h);

    var m = utc ? date.getUTCMinutes() : date.getMinutes();
    format = format.replace(/(^|[^\\])mm+/g, "$1" + ii(m));
    format = format.replace(/(^|[^\\])m/g, "$1" + m);

    var s = utc ? date.getUTCSeconds() : date.getSeconds();
    format = format.replace(/(^|[^\\])ss+/g, "$1" + ii(s));
    format = format.replace(/(^|[^\\])s/g, "$1" + s);

    var f = utc ? date.getUTCMilliseconds() : date.getMilliseconds();
    format = format.replace(/(^|[^\\])fff+/g, "$1" + ii(f, 3));
    f = Math.round(f / 10);
    format = format.replace(/(^|[^\\])ff/g, "$1" + ii(f));
    f = Math.round(f / 10);
    format = format.replace(/(^|[^\\])f/g, "$1" + f);

    var T = H < 12 ? "AM" : "PM";
    format = format.replace(/(^|[^\\])TT+/g, "$1" + T);
    format = format.replace(/(^|[^\\])T/g, "$1" + T.charAt(0));

    var t = T.toLowerCase();
    format = format.replace(/(^|[^\\])tt+/g, "$1" + t);
    format = format.replace(/(^|[^\\])t/g, "$1" + t.charAt(0));

    var tz = -date.getTimezoneOffset();
    var K = utc || !tz ? "Z" : tz > 0 ? "+" : "-";
    if (!utc) {
        tz = Math.abs(tz);
        var tzHrs = Math.floor(tz / 60);
        var tzMin = tz % 60;
        K += ii(tzHrs) + ":" + ii(tzMin);
    }
    format = format.replace(/(^|[^\\])K/g, "$1" + K);

    var day = (utc ? date.getUTCDay() : date.getDay()) + 1;
    format = format.replace(new RegExp(dddd[0], "g"), dddd[day]);
    format = format.replace(new RegExp(ddd[0], "g"), ddd[day]);

    format = format.replace(new RegExp(MMMM[0], "g"), MMMM[M]);
    format = format.replace(new RegExp(MMM[0], "g"), MMM[M]);

    format = format.replace(/\\(.)/g, "$1");

    return format;
};

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return null;
}

const themes = {
    "dark": "css/dark.css",
    "light": "css/light.css",
}