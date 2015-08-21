using System;
using System.Collections.Generic;

namespace PDFSplitIT
{
    public class SortPagesProvider : IComparer<BookMarkReference>
    {
        public SortPagesProvider()
        {
        }

        public int Compare(BookMarkReference x, BookMarkReference y)
        {
            int num;
            if (x == null)
            {
                num = (y != null ? -1 : 0);
            }
            else if (y != null)
            {
                int destinationPageIndex = x.DestinationPageIndex;
                int num1 = destinationPageIndex.CompareTo(y.DestinationPageIndex);
                if (num1 == 0)
                {
                    destinationPageIndex = x.DestinationPageIndex;
                    num = destinationPageIndex.CompareTo(y.DestinationPageIndex);
                }
                else
                {
                    num = num1;
                }
            }
            else
            {
                num = 1;
            }
            return num;
        }
    }
}
