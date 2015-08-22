using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace PdfGenerationTesting
{
    /// <summary>
    /// A Picture
    /// </summary>
    public class PdfSample4 : APdfSampleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PdfSample1));

        protected override void PopulateDocument()
        {
            const string imageUrl = "https://adobe99u.files.wordpress.com/2013/06/calvin-hobbes-calvin-26-hobbes-254155_1024_768.jpg";

            var jpg = iTextSharp.text.Image.GetInstance(imageUrl);
            var jWidth = jpg.Width;
            var dWidth = Document.PageSize.Width - (Document.LeftMargin + Document.RightMargin);

            float scale = dWidth/jWidth*100;

            jpg.ScalePercent(scale);
            Document.Add(jpg);
        }
    }
}