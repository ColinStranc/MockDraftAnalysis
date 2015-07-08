function PopulateResultsList(count) {
    var resultsList = $("#resultsList");
    var html = "<table class='table'><thead><tr><td class='cell headerCell'></td><td class='cell headerCell'>Name</td><td class='cell headerCell'>Team</td></tr></thead><tbody>";

    html += "<tr><td class='cell'>1</td><td class='cell'>Auston Matthews</td><td class='cell'>Toronto MapleLeafs</td></tr></tbody></table>";
    resultsList.html(html);
}