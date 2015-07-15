function PopulateResultsList(countLoaded, minCountShown) {
    var resultsList = $("#resultsList");

    var searchUrl = "/ProspectList/List";

    searchUrl += "?year=" + getCookieValue("year");
    searchUrl += "&count=" + countLoaded;


    $.get(searchUrl)
        .success(function (r) {
            var html = WriteProspectTableHtml(r, minCountShown);
            resultsList.html(html);
            hideProspects();
        })
        .fail(function (err) { })
        .done(function (err) { });
}

function WriteProspectTableHtml(prospects, minCountShown) {
    var tableString = "<table class='table'><thead><tr><td class='cell headerCell'></td><td class='cell headerCell'>Pos</td><td class='cell headerCell'>Name</td><td class='cell headerCell'>Hand</td><td class='cell headerCell'>Team</td></tr></thead><tbody>";
    
    $.each(prospects, function (i, prospect) {
        var index = i + 1;

        var classNames = "cell";
        if (index > minCountShown) {
            classNames += " hideableCell";
        };

        tableString += "<tr class='prospectRow'>" +
            "<td class='" + classNames + "'>" + index + "</td>" +
            "<td class='" + classNames + "'>" + prospect.Position + "</td>" +
            "<td class='" + classNames + "'>" + prospect.Name + "</td>" +
            "<td class='" + classNames + "'>" + prospect.Handedness + "</td>" +
            "<td class='" + classNames + "'>" + prospect.Team + "</td>" +
            "</tr>";
    });
    
    tableString += "</tbody></table>";

    return tableString;
};

function toggleAreProspectsHidden(minVisibleCount) {
    var resultsTable = $("#resultsList .table");

    if (resultsTable.hasClass("elementsHidden")) {
        showAllProspects();
    } else {
        hideProspects(minVisibleCount);
        
    }
}

function showAllProspects() {
    var resultsTable = $("#resultsList .table");
    var toggleButton = $("#showMoreToggle");

    var hideableRows = $(".hideableCell");
    hideableRows.show();

    toggleButton.text("Show Less");
    resultsTable.removeClass("elementsHidden");
}

function hideProspects(count) {
    var resultsTable = $("#resultsList .table");
    var toggleButton = $("#showMoreToggle");

    var hideableRows = $(".hideableCell");
    hideableRows.hide();

    toggleButton.text("Show More");
    resultsTable.addClass("elementsHidden");
}