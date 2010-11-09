
namespace Sample
{
    public class PdfRenderResult
    {
        public string OutputFileName { get; set; }

        /// <summary>
        /// Contains the number of pages written 
        /// </summary>
        public int PageCount { get; set; }

        public bool HasError { get; set; }

        public string ErrorMessage { get; set; }
    }
}
