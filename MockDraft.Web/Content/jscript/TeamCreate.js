function customReady() {
    if (cookieIsInitialized("teamName")) {
        populateFields();
    }
}

function customOnYearChange() {

}



$("#createLeagueLink").on("click", function () {
    saveModelState();
})

function saveModelState() {
    var nameField = $("#TeamModel_Name");
    var leagueIdField = $("#LeagueId");

    var localPage = "Team/Create";
    setCookieValue("teamName", nameField[0].value, localPage);
    setCookieValue("leagueId", leagueIdField.val(), localPage);
}

function populateFields() {
    var nameField = $("#TeamModel_Name");
    var leagueIdField = $("#LeagueId");

    nameField[0].value = getCookieValue("teamName");
    leagueIdField.val(getCookieValue("leagueId"));
}