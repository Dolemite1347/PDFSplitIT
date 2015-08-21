using System;
using iTextSharp.text.pdf;

namespace PDFSplitIT
{
    public sealed class BookMarkReference
    {
        private String title;
        private int destinationPage;

        public BookMarkReference()
        {
        }

        public BookMarkReference(String Title, int PageStartIndex)
        {
            title = Title.Trim();
            destinationPage = PageStartIndex;
        }

        public int DestinationPageIndex
        {
            get
            {
                return destinationPage;
            }
        }
        
        public String Title
        {
            get
            {
                return title;
            }
        }

    }
}
