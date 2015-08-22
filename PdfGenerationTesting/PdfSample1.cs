using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace PdfGenerationTesting
{
    /// <summary>
    /// Text
    /// </summary>
    public class PdfSample1 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PdfSample1));
        
        protected override void PopulateDocument()
        {
            Document.Add(new Paragraph(string.Format("It did. {0}", DateTime.Now)));
        }
    }
}
