using System;
using System.Security.Policy;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using PdfGenerationTesting.NhlPdf;

namespace PdfGenerationTesting
{
    /// <summary>
    /// 
    /// </summary>
    public class PdfSample6 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PdfSample1));
        private static readonly string[] urls = { "http://jets.nhl.com/club/roster.htm", "http://sharks.nhl.com/club/roster.htm", "http://bluejackets.nhl.com/club/roster.htm" };
    
        protected override void PopulateDocument()
        {
            var pdfGenerator = new PdfGenerator(Document) {ShowPictures = true};

            foreach (var url in urls)
            {
                var dataLoader = new DataLoader(url);
                var teamInfo = dataLoader.LoadTeamInfo();
                
                pdfGenerator.GeneratePages(teamInfo);
            }
        }
    }
}

// DataLoader
//  Accesses webpage
//  Pulls out information to put in pdf

// PdfCreater
//  Creates Pdf
