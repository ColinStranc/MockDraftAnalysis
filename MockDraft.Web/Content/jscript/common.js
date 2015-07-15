﻿var defaultYear = "2016";

function commonReady() {
    initializeCookies();
};

function initializeCookies() {
    initializeYear();
}

$("#yearDropDownList").on("change", function () {
    commonOnYearChange();
});
/* *************************** */

function initializeYear() {
    if (!cookieIsInitialized("year")) {
        document.cookie = "year=" + defaultYear + ";";
        $("#yearDropDownList").val(defaultYear);
    }
    else {
        $("#yearDropDownList").val(getCookieValue("year"));
    }
}

function cookieIsInitialized(id) {
    var cookies = document.cookie.split(";");
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        while (cookie.charAt(0) === ' ') {
            cookie = cookie.substring(1);
        }
        
        if (cookie.indexOf(id) == 0) {
            return true;
        }
    }

    return false;
}

function getCookieValue(id) {
    var name = id;
    var cs = document.cookie.split(';');
    for (var i = 0; i < cs.length; i++) {
        var c = cs[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length + 1, c.length);
        }
    }

    return "";
}

function commonOnYearChange() {
    var yearMenu = $("#yearDropDownList");

    document.cookie = "year=" + yearMenu.val() + ";";

    customOnYearChange();
}

/* *************************** */
/* *************************** */
/* *************************** */
/* *************************** */