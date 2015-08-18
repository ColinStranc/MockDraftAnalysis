using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace PdfGenerationTesting
{
    /// <summary>
    /// Horizontally Centered Text
    /// </summary>
    public class PdfSample2 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PdfSample1));

        protected override void PopulateDocument()
        {
            var paragraph = new Paragraph(string.Format("Wow, I've always wanted to be the center of attention! {0}", DateTime.Now))
            {
                Alignment = (Element.ALIGN_CENTER)
            };

            Document.Add(paragraph);
        }
    }
}