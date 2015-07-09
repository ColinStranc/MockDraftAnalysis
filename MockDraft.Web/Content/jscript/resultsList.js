function PopulateResultsList(count) {
    var resultsList = $("#resultsList");

    var searchUrl = "/ProspectList/List";
    searchUrl += "?count=" + count;

    $.get(searchUrl)
        .success(function (r) {
            var html = WriteProspectTableHtml(r);
            resultsList.html(html);
        })
        .fail(function (err) { })
        .done(function (err) { });
}

function WriteProspectTableHtml(prospects) {
    var tableString = "<table class='table'><thead><tr><td class='cell headerCell'></td><td class='cell headerCell'>Pos</td><td class='cell headerCell'>Name</td><td class='cell headerCell'>Hand</td><td class='cell headerCell'>Team</td></tr></thead><tbody>";
    
    $.each(prospects, function (i, prospect) {
        var index = i + 1;
        tableString += "<tr>" +
            "<td class='cell'>" + index + "</td>" +
            "<td class='cell'>" + prospect.Position + "</td>" +
            "<td class='cell'>" + prospect.Name + "</td>" +
            "<td class='cell'>" + prospect.Handedness + "</td>" +
            "<td class='cell'>" + prospect.Team + "</td>" +
            "</tr>";
    });
    
    tableString += "</tbody></table>";

    return tableString;
};