var defaultYear = "2016";

function commonReady() {
    initializeCookies();
};

function initializeCookies() {
    initializeYearCookie();
}

$("#yearDropDownList").on("change", function () {
    commonOnYearChange();
});
/* *************************** */

function initializeYearCookie() {
    if (!cookieIsInitialized("year")) {
        setCookieValue("year", defaultYear);

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
    setCookieValue("year", $("#yearDropDownList").val());

    customOnYearChange();
}

function setCookieValue(name, value) {
    //  delete any pre-existing cookie under this name
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC";

    document.cookie = name + "=" + value + "; path=/";
};

/* *************************** */
/* *************************** */
/* *************************** */
/* *************************** */