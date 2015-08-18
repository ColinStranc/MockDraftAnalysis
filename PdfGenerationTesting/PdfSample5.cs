using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace PdfGenerationTesting
{
    /// <summary>
    /// Font
    /// </summary>
    public class PdfSample5 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PdfSample1));

        protected override void PopulateDocument()
        {
            var bfCourier = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.CP1252, false);
            var courier = new Font(bfCourier, 12, Font.ITALIC, BaseColor.RED);

            Document.Add(new Paragraph(string.Format("It did. {0}", DateTime.Now), courier));
        }
    }
}