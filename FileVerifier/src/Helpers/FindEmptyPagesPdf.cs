using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace AvaloniaDraft.Helpers;

public class FindEmptyPagesPdf
{
    public static int EmptyPagePdf(string path)
    {
        using (PdfReader reader = new PdfReader(path))
        using (PdfDocument pdfDoc = new PdfDocument(reader))
        {
            int totalPages = pdfDoc.GetNumberOfPages();
            int emptyPages = 0;
            
            for (int i = totalPages; i > 0; i--)
            {
                string text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)).Trim();
                if (string.IsNullOrEmpty(text))
                {
                    emptyPages++;
                }
                else
                {
                    break;
                }
            }
            return emptyPages;
        }
                
    }   
}