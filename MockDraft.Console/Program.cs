using System;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using MockDraft.Pdf;
using log4net;

namespace MockDraft.Console
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                Log.Info("MockDraft.Console Starting");

                var doc1 = new Document(PageSize.A10);

                //string path = "../../pdfs";
                string path = "../../pdfs";
                PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));

                doc1.Open();
                doc1.Add(new Paragraph("Hello to this funny world."));
                doc1.Close();

                Log.Info("Finsished with an Astounding Lack of Errors");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
