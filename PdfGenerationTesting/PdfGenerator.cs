using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerationTesting.NhlPdf;

namespace PdfGenerationTesting
{
    public class PdfGenerator
    {
        public bool ShowPictures { get; set; }

        public PdfGenerator(Document document)
        {
            _document = document;
        }

        public void GeneratePages(TeamInfo teamInfo)
        {
            GenerateTable(teamInfo.Forwards, "Forwards");
            GenerateTable(teamInfo.Defencemen, "Defencemen");
            GenerateTable(teamInfo.Goalies, "Goalies");

            _document.NewPage();
        }

        private void GenerateTable(IEnumerable<PlayerInfo> players, string title)
        {
            var numberOfCols = title == "Forwards" ? NumberColsWithPosition : NumberColsWithPosition - 1; 
            
            var baseFont = BaseFont.CreateFont(BaseFont.COURIER_BOLD, BaseFont.CP1252, false);
            var titleFont = new Font(baseFont, 12, Font.BOLD, BaseColor.WHITE);
            var headerFont = new Font(baseFont, 8, Font.BOLD, BaseColor.BLACK);
            var font = new Font(baseFont, 8, Font.NORMAL, BaseColor.BLACK);

            GenerateTitleTable(title, titleFont);

            var table = GeneratePlayerInfoTableAndHeader(numberOfCols, headerFont);

            var count = 1;
            foreach (var player in players)
            {
                var backgroundColor = count%2 == 0 
                    ? BaseColor.LIGHT_GRAY 
                    : BaseColor.WHITE;

                GenerateRow(table, player, font, backgroundColor);

                count++;
            }

            WriteTable(table);
        }

        private static void GenerateRow(PdfPTable table, PlayerInfo player, Font font, BaseColor backgroundColor)
        {
            var jpg = Image.GetInstance(player.PictureUrl);
            table.AddCell(jpg);

            PdfPCell cell;

            cell = new PdfPCell(new Phrase(player.JerseyNumber, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(player.Name, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            if (table.NumberOfColumns == NumberColsWithPosition)
            {
                cell = new PdfPCell(new Phrase(player.Position, font)) {BackgroundColor = backgroundColor};
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Phrase(player.Height, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(player.Weight, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(player.DateOfBirth, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(player.Age, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(player.BirthPlace, font)) {BackgroundColor = backgroundColor};
            table.AddCell(cell);
        }

        private void GenerateTitleTable(string title, Font titleFont)
        {
            var titleTable = new PdfPTable(1);

            var titleCell = new PdfPCell(new Phrase(title, titleFont));
            titleCell.BackgroundColor = BaseColor.BLUE;

            titleTable.AddCell(titleCell);
            WriteTable(titleTable);
        }

        private PdfPTable GeneratePlayerInfoTableAndHeader(int numberOfCols, Font headerFont)
        {
            var table = new PdfPTable(numberOfCols);

            var widths = numberOfCols == NumberColsWithPosition
                ? new float[] { 70f, 35f, 120f, 60f, 65f, 65f, 120f, 65f, 120f }  // 
                : new float[] { 80f, 40f, 130f,      70f, 70f, 130f, 70f, 130f }; // -40
            table.SetWidths(widths);

            AddHeaderRow(table, headerFont);
            return table;
        }

        private void WriteTable(PdfPTable table)
        {
            AddBreakRow(table);
            _document.Add(table);
        }

        private void AddHeaderRow(PdfPTable table, Font font)
        {
            var backGroundColor = BaseColor.GRAY;
            PdfPCell cell;

            cell= new PdfPCell(new Phrase("", font)) {BackgroundColor = backGroundColor};
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("#", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Name", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);
            
            if (table.NumberOfColumns == NumberColsWithPosition)
            {
                cell = new PdfPCell(new Phrase("Pos", font)) { BackgroundColor = backGroundColor };
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Phrase("Height", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Weight", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Date Of Birth", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Age", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("Birth Place", font)) { BackgroundColor = backGroundColor };
            table.AddCell(cell);
        }

        private void AddBreakRow(PdfPTable table)
        {
            var lineBreak = new PdfPCell()
            {
                Colspan = table.NumberOfColumns,
                FixedHeight = 5f,
                BorderWidthLeft = 0f,
                BorderWidthRight = 0f,
                BorderWidthTop = 0f,
                BorderWidthBottom = 0f
            };

            table.AddCell(lineBreak);
        }


        private const int NumberColsWithPosition = 9;

        private Document _document;
    }
}
