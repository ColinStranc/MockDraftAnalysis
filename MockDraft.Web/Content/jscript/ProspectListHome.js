var minProspectsVisibleCount = 1;
var prospectLoadedCount = -1;

function customReady() {
    customOnYearChange();
}

function customOnYearChange() {
    populateResultsListWithHiding();
}

function populateResultsListWithHiding() {
    PopulateResultsList(prospectLoadedCount, minProspectsVisibleCount);
    //var finished = PopulateResultsList();

    //toggleAreProspectsHidden(minProspectsVisibleCount);
}

$("#showMoreToggle").on("click", function() {
    toggleAreProspectsHidden(minProspectsVisibleCount);
});