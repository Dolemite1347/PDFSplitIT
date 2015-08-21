using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

using Microsoft.Win32;

namespace PDFSplitIT
{
    public partial class frmOptions : Telerik.WinControls.UI.RadForm
    {
        public frmOptions()
        {
            InitializeComponent();

            this.Load += FrmOptions_Load;
        }

        private void FrmOptions_Load(object sender, EventArgs e)
        {
            chkAutoOpen.Checked = CommonMembers.AutoOpenOutputDir;
            chkCreateOutput.Checked = CommonMembers.CreateOutputFileName;
        }

        private void chkCreateOutput_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PDFSplitIT\", true))
                if (Key != null)
                {
                    Key.SetValue("CreateOutputFilename", chkCreateOutput.Checked);
                }
                else
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\PDFSplitIT\"))
                        Key.SetValue("CreateOutputFilename", chkCreateOutput.Checked);
                }

            CommonMembers.CreateOutputFileName = chkCreateOutput.Checked;
        }

        private void chkAutoOpen_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PDFSplitIT\", true))
                if (Key != null)
                {
                    Key.SetValue("AutoOpenOutputDir", chkAutoOpen.Checked);
                }
                else
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\PDFSplitIT\"))
                        Key.SetValue("AutoOpenOutputDir", chkAutoOpen.Checked);
                }

            CommonMembers.AutoOpenOutputDir = chkAutoOpen.Checked;
        }
    }
}
