using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFSplitIT
{
    public static class CommonMembers
    {

        private static bool createOutputFileName = true;
        private static bool autoOpen = false;
        private static string currentFileName = String.Empty;
        
        public static bool CreateOutputFileName {
            get { return createOutputFileName; }
            set { createOutputFileName = value; }
        }

        public static bool AutoOpenOutputDir
        {
            get { return autoOpen; }
            set { autoOpen = value; }
        }

        public static string CurrentFileName
        {
            get { return currentFileName; }
            set { currentFileName = value; }
        }
    }
}
