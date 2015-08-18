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
        }

        private void GenerateTable(IEnumerable<PlayerInfo> players, string title)
        {
            var numberOfCols = title == "Forwards" ? NumberColsWithPosition : NumberColsWithPosition - 1; 
            var titleTable = new PdfPTable(1);

            var baseFont = BaseFont.CreateFont(BaseFont.COURIER_BOLD, BaseFont.CP1252, false);
            var titleFont = new Font(baseFont, 12, Font.BOLD, BaseColor.BLUE);
            var font = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);

            titleTable.AddCell(new Phrase(title, titleFont));
            WriteTable(titleTable);

            var table = new PdfPTable(numberOfCols);
            
            AddHeaderRow(table);
            foreach (var player in players)
            {
                table.AddCell(new Phrase(player.JerseyNumber, font));
                table.AddCell(new Phrase(player.Name, font));
                if (table.NumberOfColumns == NumberColsWithPosition)
                {
                    table.AddCell(new Phrase(player.Position, font));
                }
                table.AddCell(new Phrase(player.Height, font));
                table.AddCell(new Phrase(player.Weight, font));
                table.AddCell(new Phrase(player.DateOfBirth, font));
                table.AddCell(new Phrase(player.Age, font));
                table.AddCell(new Phrase(player.BirthPlace, font));
            }

            WriteTable(table);
        }

        private void WriteTable(PdfPTable table)
        {
            AddBreakRow(table);
            _document.Add(table);
        }

        private void AddHeaderRow(PdfPTable table)
        {
            table.AddCell("#");
            table.AddCell("NAME");
            if (table.NumberOfColumns == NumberColsWithPosition)
            {
                table.AddCell("POS");
            }
            table.AddCell("HEIGHT");
            table.AddCell("WEIGHT");
            table.AddCell("DATE OF BIRTH");
            table.AddCell("AGE");
            table.AddCell("BIRTH PLACE");
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


        private const int NumberColsWithPosition = 8;

        private Document _document;
    }
}
