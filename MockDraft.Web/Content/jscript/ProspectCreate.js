function customReady() {
    if (cookieIsInitialized("prospectName")) {
        populateFields();
    }
}

function customOnYearChange() {

}

$("createTeamLink").on("click", function() {
    saveModelState();
})

function saveModelState() {
    var nameField = $("#ProspectModel_Name");
    var teamIdField = $("TeamId");
    var heightField = $("#ProspectModel_Height");
    var weightField = $("#ProspectModel_Weight");
    var positionField = $("#ProspectModel_Position");
    var handednessField = $("#ProspectModel_Handedness");
    var birthDateField = $("#ProspectModel_BirthDay");
    var birthCityField = $("#ProspectModel_BirthCity");
    var birthCountryField = $("#ProspectModel_BirthCountry");
    var notesField = $("#ProspectModel_Notes");

    var localPage = "Prospect/Create";
    setCookieValue("prospectName", nameField[0].value, localPage);
    setCookieValue("teamId", teamIdField.val(), localPage);
    setCookieValue("prospectHeight", heightField[0].value, localPage);
    setCookieValue("prospectWeight", weightField[0].value, localPage);
    setCookieValue("prospectPosition", positionField[0].value, localPage);
    setCookieValue("prospectHandedness", handednessField[0].value, localPage);
    setCookieValue("prospectBirthDate", birthDateField[0].value, localPage);
    setCookieValue("prospectBirthCity", birthCityField[0].value, localPage);
    setCookieValue("prospectBirthCountry", birthCountryFie[0].value, localPage);
    setCookieValue("prospectNotes", notesField[0].value, localPage);
}

function populateFields() {
    var nameField = $("#ProspectModel_Name");
    var teamIdField = $("TeamId");
    var heightField = $("#ProspectModel_Height");
    var weightField = $("#ProspectModel_Weight");
    var positionField = $("#ProspectModel_Position");
    var handednessField = $("#ProspectModel_Handedness");
    var birthDateField = $("#ProspectModel_BirthDay");
    var birthCityField = $("#ProspectModel_BirthCity");
    var birthCountryField = $("#ProspectModel_BirthCountry");
    var notesField = $("#ProspectModel_Notes");

    nameField[0].value = getCookieValue("prospectName");
    teamIdField.val(getCookieValue("teamId"));
    heightField[0].value = getCookieValue("prospectHeight");
    weightField[0].value = getCookieValue("prospectWeight");
    positionField[0].value = getCookieValue("prospectPosition");
    handednessField[0].value = getCookieValue("prospectHandedness");
    birthDateField[0].value = getCookieValue("prospectBirthDate");
    birthCityField[0].value = getCookieValue("prospectBirthCity");
    birthCountryFie[0].value = getCookieValue("prospectBirthCountry");
    notesField[0].value  = getCookieValue("prospectNotes");
}