var prospectLoadedCount = -1;
var minProspectsVisibleCount = 1;

function customReady() {
    commonOnYearChange();
}

function customOnYearChange() {
    PopulateResultsList(prospectLoadedCount, minProspectsVisibleCount);
}

$("#showMoreToggle").on("click", function() {
    toggleAreProspectsHidden(minProspectsVisibleCount);
});