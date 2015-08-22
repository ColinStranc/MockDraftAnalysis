using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace PdfGenerationTesting
{
    /// <summary>
    /// Table
    /// </summary>
    public class PdfSample3 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PdfSample1));

        protected override void PopulateDocument()
        {
            var table = new PdfPTable(3);
            var cell = new PdfPCell(new Phrase("Header spanning 3 columns"))
            {
                Colspan = 3, 
                HorizontalAlignment = 1
            };
            table.AddCell(cell);
            table.AddCell("Col 1 Row 1");
            table.AddCell("Col 2 Row 1");
            table.AddCell("Col 3 Row 1");
            table.AddCell("Col 1 Row 2");
            table.AddCell("Col 2 Row 2");
            table.AddCell("Col 3 Row 2");
            Document.Add(table);
        }
    }
}