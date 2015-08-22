using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PdfGenerationTesting
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private const string FileName = "mypdf1.pdf";

        static void Main(string[] args)
        {
            try
            {
                Log.Info("PdfGenerationTesting -- Start");
                
                
                var pdf = new PdfSample6();

                pdf.CreatePdf(FileName);


                Log.Info("Finished with an Astounding Lack of Errors");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
