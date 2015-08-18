using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using log4net.Config;

namespace PdfGenerationTesting.NhlPdf
{
    public class DataLoader
    {
        // I made this static because it was used in the constructor and I wasn't sure if non static members would be created yet...
        public static HtmlNode LoadHtml(string url)
        {
            using (var client = new WebClient())
            {
                string webPageContent = client.DownloadString(url);

                using (var textReader = new StringReader(webPageContent))
                {
                    var htmlDoc = new HtmlDocument();

                    htmlDoc.OptionFixNestedTags = true;
                    htmlDoc.Load(textReader);

                    return htmlDoc.DocumentNode;
                }
            }
        }

        /* ********************************************* */

        public DataLoader(string url)
        {
            _url = url;
            _rosterPage = LoadHtml(_url);
        }

        public TeamInfo LoadTeamInfo()
        {
            var loadedTeam = new TeamInfo();

            var rosterTable = _rosterPage.SelectSingleNode(
                "//div[@class='tieUp']"
                );

            var rosterTablesByPosition = rosterTable.SelectNodes("table");

            LoadForwards(loadedTeam, rosterTablesByPosition[0]);
            LoadDefensemen(loadedTeam, rosterTablesByPosition[1]);
            LoadGoalies(loadedTeam, rosterTablesByPosition[2]);

            Log.InfoFormat("Forward count: {0}", loadedTeam.Forwards.Count);
            Log.InfoFormat("Defencemen count: {0}", loadedTeam.Defencemen.Count);
            Log.InfoFormat("Goalie count: {0}", loadedTeam.Goalies.Count);

            return loadedTeam;
        }

        private void LoadForwards(TeamInfo loadedTeam, HtmlNode forwards)
        {
            Log.Info("### FORWARDS ###");

            var rows = forwards.SelectNodes("tr");

            foreach (var row in rows)
            {
                if (row.Attributes["class"].Value == "hdr" ||
                    row.SelectNodes("td").Count == 1)
                {
                    continue;
                }

                Log.Info(row.ChildNodes.Count);
                loadedTeam.Forwards.Add(CreatePlayerFromRosterRow(row));
            }
        }

        private void LoadDefensemen(TeamInfo loadedTeam, HtmlNode defensemen)
        {
            Log.Info("### DEFENSEMEN ###");

            var rows = defensemen.SelectNodes("tr");

            foreach (var row in rows)
            {
                if (row.Attributes["class"].Value == "hdr" ||
                    row.SelectNodes("td").Count == 1)
                {
                    continue;
                }

                Log.Info(row.ChildNodes.Count);
                loadedTeam.Defencemen.Add(CreatePlayerFromRosterRow(row));
            }
        }

        private void LoadGoalies(TeamInfo loadedTeam, HtmlNode goalies)
        {
            Log.Info("### GOALIES ###");

            var rows = goalies.SelectNodes("tr");

            foreach (var row in rows)
            {
                if (row.Attributes["class"].Value == "hdr" ||
                    row.SelectNodes("td").Count == 1)
                {
                    continue;
                }

                Log.Info(row.ChildNodes.Count);
                loadedTeam.Goalies.Add(CreatePlayerFromRosterRow(row));
            }
        }

        private PlayerInfo CreatePlayerFromRosterRow(HtmlNode row)
        {
            var rowElements = row.SelectNodes("td");
            var childNodeCount = rowElements.Count;

            var pictureUrl = rowElements[1].SelectSingleNode("nobr/a").Attributes["href"].Value;
            var jerseyNumber = rowElements[0].SelectSingleNode("span").InnerText;
            var name = rowElements[1].SelectSingleNode("nobr/a").InnerText;
            var position = childNodeCount == 8 ? rowElements[2].InnerText : null;

            // Only forwards have position data, so count gets messed up
            var height = rowElements[childNodeCount - 5].InnerText;
            var weight = rowElements[childNodeCount - 4].InnerText;
            var dateOfBirth = rowElements[childNodeCount - 3].InnerText;
            var age = rowElements[childNodeCount - 2].InnerText;
            var birthPlace = rowElements[childNodeCount - 1].InnerText;

            var player = new PlayerInfo()
            {
                PictureUrl = pictureUrl,
                JerseyNumber = jerseyNumber,
                Name = name,
                Position = position,
                Height = height,
                Weight = weight,
                DateOfBirth = dateOfBirth,
                Age = age,
                BirthPlace = birthPlace
            };

            Log.InfoFormat("picurl = {0}\njernum {1}\nname {2}\npos {3}\nheight {4}\nweight {5}\ndob {6}\nage {7}\nbp {8}\n",
                player.PictureUrl,
                player.JerseyNumber,
                player.Name,
                player.Position,
                player.Height,
                player.Weight,
                player.DateOfBirth,
                player.Age,
                player.BirthPlace);

            return player;
        }


        /* ********************************************* */

        private readonly string _url;
        private readonly HtmlNode _rosterPage;

        private static readonly ILog Log = LogManager.GetLogger(typeof(DataLoader));
    }
}
