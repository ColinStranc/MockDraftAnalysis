using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PdfGenerationTesting
{
    public abstract class APdfSampleBase
    {

        protected Document Document { get; private set; }

        public void CreatePdf(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                Document = new Document(PageSize.A4, 25, 25, 30, 30);

                // writer associates itself with document. document.  Close closes writer when finished.
                PdfWriter.GetInstance(Document, fs);

                Document.Open();
                PopulateDocument();
                Document.Close();
            }
        }

        protected abstract void PopulateDocument();
    }
}
