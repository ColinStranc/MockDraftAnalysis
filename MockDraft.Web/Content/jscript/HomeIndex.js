var prospectLoadedCount = 10;
var minProspectsVisibleCount = prospectLoadedCount;

function customReady() {
    commonOnYearChange();
}

function customOnYearChange() {
    PopulateResultsList(prospectLoadedCount, minProspectsVisibleCount);
}

/* *************************** */

