using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

using Microsoft.Win32;

using Telerik.WinControls;
using Telerik.WinControls.UI;

using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PDFSplitIT
{
    public partial class frmMain : Telerik.WinControls.UI.RadForm
    {

        private bool sourceSelected = false;
        private bool analyzeFailed = false;
        private List<BookMarkReference> bookMarkReferences = null;
        private Thread analyzeThread;
        private Thread processFileThread;
        private System.Windows.Forms.Timer listenTimer;

        private delegate void SetProgressBarDelegate(ref RadProgressBar bar, Boolean visible, int MaxValue);
        private delegate void SetProgressBarValueDelegate(ref RadProgressBar bar, int Value);
        private delegate void SetProgressPanelVisibleDelegate(Boolean visible);
        private delegate void SetButtonEnabledDelegate(ref RadButton btn, Boolean enabled);
        private delegate void SetGroupBoxEnabledDelegate(ref RadGroupBox btn, Boolean enabled);
        private delegate void SetPictureVisibleDelegate(ref PictureBox pic, Boolean visible);
        private delegate void SetStatusTextDelegate(String Message);
        private delegate void AddTreeNodeDelegate(RadTreeView tvw, String Text);

        public frmMain()
        {
            InitializeComponent();

            this.Load += frmMain_Load;
            this.txtPageNumbers.KeyPress += TxtPageNumbers_KeyPress;
        }

        private void TxtPageNumbers_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if (e.KeyChar == '\b') {
                e.Handled = false;
                return;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^0-9^\-^\,^]"))
            {
                // Stop the character from being entered into the control since it is illegal.
                e.Handled = true;
            }
        }
        
        void frmMain_Load(object sender, EventArgs e)
        {
            listenTimer = new System.Windows.Forms.Timer();
            listenTimer.Tick += listenTimer_Tick;
            listenTimer.Interval = 500;
            listenTimer.Enabled = true;
            lblDestinationPDF.Text = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "\\PDFSplitIT\\");

            radSplitContainer1.Splitters[0].Fixed = true;

            if (!Directory.Exists(lblDestinationPDF.Text))
            {
                Directory.CreateDirectory(lblDestinationPDF.Text);
            }

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PDFSplitIT\"))
                if (Key != null)
                {
                    Boolean val = Convert.ToBoolean(Key.GetValue("CreateOutputFilename"));
                    Boolean val2 = Convert.ToBoolean(Key.GetValue("AutoOpenOutputDir"));
                    CommonMembers.CreateOutputFileName = val;
                    CommonMembers.AutoOpenOutputDir = val2;
                }
                else
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\PDFSplitIT\")) {
                        key.SetValue("CreateOutputFilename", true);
                        key.SetValue("AutoOpenOutputDir", false);
                    }
                    CommonMembers.CreateOutputFileName = true;
                    CommonMembers.AutoOpenOutputDir = false;
                }
        
        }

        protected void AnalyzeFile()
        {
            analyzeFailed = false;
            this.SetStatusText("Analyzing...");
            SetPictureVisible(ref imgError, false);
            SetPictureVisible(ref imgOK, false);

            if (radRadioButton2.IsChecked && txtPageNumbers.Text.Trim().Length > 0)
            {
                try
                {
                    String[] pages = txtPageNumbers.Text.Trim().Replace(" ", "").Split(',');
                    foreach (string s in pages)
                    {
                        if (s.Contains('-'))
                        {
                            String[] pageRange = s.Split('-');
                            if (Convert.ToInt32(pageRange[0]) >= Convert.ToInt32(pageRange[1]))
                            {
                                MessageBox.Show("Cannot count pages backwards. Please correct your page range.", "Invalid Page Range", MessageBoxButtons.OK);
                                analyzeFailed = true;
                                SetButtonEnabled(ref btnSourcePDF, true);
                                SetButtonEnabled(ref btnDestinationPDF, true);
                                SetPictureVisible(ref imgError, true);
                                SetGroupBoxEnabled(ref radGroupBox1, true);
                                this.SetStatusText("Invalid Page Range");
                                this.SetProgressBar(ref progressStatus, false, 100);
                                this.SetProgressPanelVisible(false);
                                analyzeThread.Abort();
                                return;
                            } else if (pageRange.Length > 2)
                            {
                                MessageBox.Show("Invalid page range entered. Please correct your entry and try again", "Invalid Page Range", MessageBoxButtons.OK);
                                analyzeFailed = true;
                                SetButtonEnabled(ref btnSourcePDF, true);
                                SetButtonEnabled(ref btnDestinationPDF, true);
                                SetGroupBoxEnabled(ref radGroupBox1, true);
                                SetPictureVisible(ref imgError, true);
                                this.SetStatusText("Invalid Page Range");
                                this.SetProgressBar(ref progressStatus, false, 100);
                                this.SetProgressPanelVisible(false);
                                analyzeThread.Abort();
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    analyzeFailed = true;
                    SetButtonEnabled(ref btnSourcePDF, true);
                    SetButtonEnabled(ref btnDestinationPDF, true);
                    SetPictureVisible(ref imgError, true);
                    SetGroupBoxEnabled(ref radGroupBox1, true);
                    this.SetStatusText("Invalid Page Range");
                    this.SetProgressBar(ref progressStatus, false, 100);
                    this.SetProgressPanelVisible(false);
                    analyzeThread.Abort();
                    return;
                }
            }
    
            PdfReader pdfDocument = new PdfReader(lblSourcePDF.Text);
            PdfDictionary document = new PdfDictionary( );            
            SortPagesProvider sortPagesProvider = new SortPagesProvider();
            int page_number = 0;
            
            if (radRadioButton1.IsChecked) {
                bookMarkReferences = new List<BookMarkReference>();
                IList<Dictionary<string, object>> bookmarks = SimpleBookmark.GetBookmark(pdfDocument);

                if (bookmarks != null) {
                    int i = 0;
                    this.SetProgressBar(ref progressStatus, true, bookmarks.Count);
                    foreach (Dictionary<string, object> bk in bookmarks)
                    {
                        foreach (KeyValuePair<string, object> kvr in bk)
                        {
                            if (kvr.Key == "Page" || kvr.Key == "page")                        
                                page_number = Convert.ToInt32(Regex.Match(kvr.Value.ToString(), "[0-9]+").Value);                        
                        }
                    
                        bookMarkReferences.Add(new BookMarkReference(bookmarks[i].Values.ToArray().GetValue(0).ToString(), page_number));
                        this.AddTreeNode(radTreeView1, bookmarks[i].Values.ToArray().GetValue(0).ToString() + " (Page: " + page_number.ToString() + ")");
                        this.SetStatusText(string.Format("Analyzing, {0}...", bookmarks[i].Values.ToArray().GetValue(0).ToString()));
                        this.SetProgressBarValue(ref progressStatus, i);
                        Thread.Sleep(500);
                        i++;
                    
                    }
                
                    bookMarkReferences.Sort(sortPagesProvider);
                    SetButtonEnabled(ref btnSourcePDF, true);
                    SetButtonEnabled(ref btnDestinationPDF, true);
                    SetPictureVisible(ref imgOK, true);
                    SetGroupBoxEnabled(ref radGroupBox1, true);
                    this.SetStatusText("Done.");
                    this.SetProgressBar(ref progressStatus, false, 100);
                    this.SetProgressPanelVisible(false);
                } else {
                    analyzeFailed = true;
                    SetButtonEnabled(ref btnSourcePDF, true);
                    SetButtonEnabled(ref btnDestinationPDF, true);
                    SetPictureVisible(ref imgError, true);
                    SetGroupBoxEnabled(ref radGroupBox1, true);
                    this.SetStatusText("No Bookmarks Found!");
                    this.SetProgressBar(ref progressStatus, false, 100);
                    this.SetProgressPanelVisible(false);                
                }
            } else if (radRadioButton2.IsChecked) {
                String[] pages = txtPageNumbers.Text.Trim().Replace(" ", "").Split(',');

                Int32 pageCount = pdfDocument.NumberOfPages;

                if (pages.Length > 0) {
                    this.SetProgressBar(ref progressStatus, true, pages.Length);
                    try {
                        for (int i = 0; i < pages.Length; i++)
                        {                        
                            this.SetStatusText(string.Format("Analyzing, {0}...", pages[i]));
                            this.SetProgressBarValue(ref progressStatus, i+1);

                            if (pages[i].Trim() != "") {
                                if (pages[i].Contains("-")) {
                                    if (pages[i].Split('-')[1].Trim() != "") {
                                        Int32 lastnum = Convert.ToInt32(pages[i].Split('-')[1]);
                                        if (lastnum > pageCount) {
                                            analyzeFailed = true;
                                            this.AddTreeNode(radTreeView1, "Page(s): " + pages[i] + " - Invalid");
                                        } else {
                                            this.AddTreeNode(radTreeView1, "Page(s): " + pages[i] + " - \u2713");
                                        }

                                    }
                                } else {
                                    if (Convert.ToInt32(pages[i]) > pageCount) {
                                        analyzeFailed = true;
                                        this.AddTreeNode(radTreeView1, "Page(s): " + pages[i] + " - Invalid");
                                    } else {
                                        this.AddTreeNode(radTreeView1, "Page(s): " + pages[i] + " - \u2713");
                                    }
                                }
                            }
                        
                        }

                        this.SetStatusText("Done.");
                        SetButtonEnabled(ref btnSourcePDF, true);
                        SetButtonEnabled(ref btnDestinationPDF, true);
                        SetPictureVisible(ref imgOK, true);
                        SetGroupBoxEnabled(ref radGroupBox1, true);
                        this.SetProgressBar(ref progressStatus, false, 100);
                        this.SetProgressPanelVisible(false);
                    } catch (Exception ex) {
                        MessageBox.Show("There was a problem analyzing your document. Please check your entries and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.SetStatusText("Failed.");
                        SetButtonEnabled(ref btnSourcePDF, true);
                        SetButtonEnabled(ref btnDestinationPDF, true);
                        SetPictureVisible(ref imgError, true);
                        SetGroupBoxEnabled(ref radGroupBox1, true);
                        this.SetProgressBar(ref progressStatus, false, 100);
                        this.SetProgressPanelVisible(false);
                    }
                }
            } else {
                analyzeFailed = true;
                this.SetStatusText("No pages documented.");
                SetButtonEnabled(ref btnSourcePDF, true);
                SetButtonEnabled(ref btnDestinationPDF, true);
                SetPictureVisible(ref imgError, true);
                SetGroupBoxEnabled(ref radGroupBox1, true);
                this.SetProgressBar(ref progressStatus, false, 100);
                this.SetProgressPanelVisible(false);
            }

            pdfDocument.Close();
            pdfDocument.Dispose();            
        }
        
        protected void DoSplitDocument()
        {
            SetPictureVisible(ref imgError, false);
            SetPictureVisible(ref imgOK, false);
            this.SetStatusText("Processing Document...");
            CommonMembers.CurrentFileName = Path.GetFileNameWithoutExtension(lblSourcePDF.Text);
            PdfReader pdfDocument = new PdfReader(lblSourcePDF.Text);
            String destinationDir = this.lblDestinationPDF.Text + (CommonMembers.CreateOutputFileName ? CommonMembers.CurrentFileName + "\\" : "");
            int num = 0;

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            if (radRadioButton1.IsChecked) {
                this.SetProgressBar(ref progressTotalStatus, true, bookMarkReferences.Count);
            
                for (int i = 0; i < bookMarkReferences.Count; i++)
                {
                    int num1 = 1;
                    int count = pdfDocument.NumberOfPages;

                    BookMarkReference item = bookMarkReferences[i];
                    count = (i + 1 != bookMarkReferences.Count ? bookMarkReferences[i + 1].DestinationPageIndex : pdfDocument.NumberOfPages);
                    num1 = count;
                    if (num1 == 0)
                    {
                        num1 = 1;
                    }

                    this.SetStatusText(string.Format("Splitting, {0}...", item.Title));
                    this.SetProgressBar(ref this.progressStatus, true, num1);

                    if (File.Exists(string.Concat(destinationDir, item.Title, ".pdf")))
                        File.Delete(string.Concat(destinationDir, item.Title, ".pdf"));
                
                    Document sourceDocument = new Document(pdfDocument.GetPageSizeWithRotation(item.DestinationPageIndex));
                    PdfCopy pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(string.Concat(destinationDir, item.Title, ".pdf"), FileMode.Create));

                    sourceDocument.Open();

                    for (int j = item.DestinationPageIndex; j <= num1; j++)
                    {
                        PdfImportedPage importPage = pdfCopyProvider.GetImportedPage(pdfDocument, j);
                        pdfCopyProvider.AddPage(importPage);
                        this.SetProgressBarValue(ref this.progressStatus, j);
                    }

                    Thread.Sleep(100);
                
                    sourceDocument.Close();
                    int num2 = num;
                    num = num2 + 1;
                    this.SetProgressBarValue(ref this.progressTotalStatus, num2);
                }
            } else if (radRadioButton2.IsChecked) {
                String[] pages = txtPageNumbers.Text.Trim().Replace(" ", "").Split(',');

                this.SetProgressBar(ref progressTotalStatus, true, pages.Length);

                for (int i = 0; i < pages.Length; i++)
                {
                    int num1 = 1;
                    int count = pdfDocument.NumberOfPages;
                    String pageTitle = (pages[i].Contains('-') ? "Pages" + pages[i].Split('-')[0] + "through" + pages[i].Split('-')[1] : "Page" + pages[i]);
                    count = pages.Length;
                    num1 = count;
                    if (num1 == 0)
                    {
                        num1 = 1;
                    }

                    this.SetStatusText(string.Format("Splitting, {0}...", pages[i]));
                    if (pages[i].Contains('-'))
                    {
                        String[] pageRange = pages[i].Split('-');
                        int x = 0;
                        for (x = 0; x <= Convert.ToInt32(pageRange[1]); x++)
                            x++;
                        this.SetProgressBar(ref this.progressStatus, true, x);
                    }
                    else
                        this.SetProgressBar(ref this.progressStatus, true, 1);

                    if (pages[i].Trim() != "") {                        
                        if (File.Exists(string.Concat(destinationDir, pageTitle, ".pdf")))
                            File.Delete(string.Concat(destinationDir, pageTitle, ".pdf"));

                        Document sourceDocument = new Document(pdfDocument.GetPageSizeWithRotation(1));
                        PdfCopy pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(string.Concat(destinationDir, pageTitle, ".pdf"), FileMode.Create));

                        sourceDocument.Open();

                        if (pages[i].Contains('-')) {
                            String[] pageRange = pages[i].Split('-');
                            for (int j = Convert.ToInt32(pageRange[0]); j <= Convert.ToInt32(pageRange[1]); j++)
                            {
                                PdfImportedPage importPage = pdfCopyProvider.GetImportedPage(pdfDocument, j);
                                pdfCopyProvider.AddPage(importPage);
                                this.SetProgressBarValue(ref this.progressStatus, j - Convert.ToInt32(pageRange[0]));
                            }
                        } else {
                            PdfImportedPage importPage = pdfCopyProvider.GetImportedPage(pdfDocument, Convert.ToInt32(pages[i]));
                            pdfCopyProvider.AddPage(importPage);
                            this.SetProgressBarValue(ref this.progressStatus, 1);
                        }

                        Thread.Sleep(100);

                        sourceDocument.Close();
                    }
                    
                    int num2 = num;
                    num = num2 + 1;
                    this.SetProgressBarValue(ref this.progressTotalStatus, num2);
                }
            }

            pdfDocument.Close();
            this.SetStatusText("Done!");
            SetButtonEnabled(ref btnSourcePDF, true);
            SetButtonEnabled(ref btnDestinationPDF, true);
            SetGroupBoxEnabled(ref radGroupBox1, true);
            this.SetProgressBar(ref this.progressStatus, false, 100);
            this.SetProgressBar(ref this.progressTotalStatus, false, 100);
            this.SetButtonEnabled(ref this.btnOpenFolder, true);
            Thread.Sleep(100);
            if (CommonMembers.AutoOpenOutputDir)
                Process.Start(destinationDir);
        }

        void listenTimer_Tick(object sender, EventArgs e)
        {
            if ((!sourceSelected ? true : analyzeFailed))
            {
                pnlProgress.Visible = false;
                btnSplitIT.Enabled = false;
            }
            else
            {
                btnSplitIT.Enabled = true;
            }
        }

        private void btnSourcePDF_Click(object sender, EventArgs e)
        {
            if (radRadioButton2.IsChecked && txtPageNumbers.Text.Trim().Length == 0) {
                MessageBox.Show("Please enter the pages numbers to split expected to split the document.", "Enter Page Numbers", MessageBoxButtons.OK);
                return;
            }
            if (radRadioButton2.IsChecked && txtPageNumbers.Text.Trim().Length > 0)
            {
                try {
                    String[] pages = txtPageNumbers.Text.Trim().Replace(" ", "").Split(',');
                    foreach (string s in pages) {
                        if (s.Contains('-')) {
                            String[] pageRange = s.Split('-');
                            if (Convert.ToInt32(pageRange[0]) >= Convert.ToInt32(pageRange[1])) {
                                MessageBox.Show("Cannot count pages backwards. Please correct your page range.", "Invalid Page Range", MessageBoxButtons.OK);
                                return;
                            } else if (pageRange.Length > 2) {
                                MessageBox.Show("Invalid page range entered. Please correct your entry and try again", "Invalid Page Range", MessageBoxButtons.OK);
                                return;
                            }
                        }
                    }
                } catch(Exception ex) {
                    return;
                }
            }
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Portable Document Format (*.pdf)|*.pdf",
                AddExtension = true,
                Multiselect = false,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    sourceSelected = true;
                    lblSourcePDF.Text = openFileDialog.FileName;
                    if (analyzeThread != null)
                    {
                        analyzeThread.Abort();
                        analyzeThread = null;
                    }
                    pnlProgress.Visible = true;
                    pnlProgress.Enabled = true;
                    radGroupBox1.Enabled = false;
                    btnSourcePDF.Enabled = false;
                    btnDestinationPDF.Enabled = false;
                    btnOpenFolder.Enabled = false;
                    radTreeView1.Nodes.Clear();
                    analyzeFailed = false;
                    analyzeThread = new Thread(new ThreadStart(AnalyzeFile))
                    {
                        IsBackground = true
                    };
                    analyzeThread.Start();
                }
            }
        }

        private void btnDestinationPDF_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                Description = "Select the directory you want to use to place the converted PDF documents into.",
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.Personal
            };
            if (lblDestinationPDF.Text.Length > 0)
            {
                folderBrowserDialog.SelectedPath = lblDestinationPDF.Text;
            }
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                lblDestinationPDF.Text = String.Concat(folderBrowserDialog.SelectedPath, "\\");
            }
        }

        private void btnSplitIT_Click(object sender, EventArgs e)
        {
            if (processFileThread != null)
                processFileThread.Abort();
            btnSourcePDF.Enabled = false;
            btnDestinationPDF.Enabled = false;
            radGroupBox1.Enabled = false;
            pnlProgress.Visible = true;
            pnlProgress.Enabled = true;
            processFileThread = new Thread(new ThreadStart(DoSplitDocument))
            {
                IsBackground = true
            };
            processFileThread.Start();
        }


        private void SetProgressBar(ref RadProgressBar bar, Boolean visible, int max)
        {
            if (!bar.InvokeRequired)
            {
                bar.Visible = visible;
                bar.Maximum = max;
                bar.Minimum = 0;
            }
            else
            {
                SetProgressBarDelegate setProgressBarDelegate = new SetProgressBarDelegate(SetProgressBar);
                Object[] objArray = new Object[] { bar, visible, max };
                bar.Invoke(setProgressBarDelegate, objArray);
            }
        }

        private void SetProgressBarValue(ref RadProgressBar bar, int value)
        {
            if (!bar.InvokeRequired)
            {
                bar.Value1 = value;
            }
            else
            {
                SetProgressBarValueDelegate setProgressBarValueDelegate = new SetProgressBarValueDelegate(SetProgressBarValue);
                Object[] objArray = new Object[] { bar, value };
                bar.Invoke(setProgressBarValueDelegate, objArray);
            }
        }

        private void SetProgressPanelVisible(Boolean isVisible)
        {
            if (!this.pnlProgress.InvokeRequired)
            {
                this.pnlProgress.Visible = isVisible;
                this.pnlProgress.Enabled = isVisible;
            }
            else
            {
                Panel panelControl = pnlProgress;
                SetProgressPanelVisibleDelegate setProgressPanelVisibleDelegate = new SetProgressPanelVisibleDelegate(SetProgressPanelVisible);
                Object[] objArray = new Object[] { isVisible };
                panelControl.Invoke(setProgressPanelVisibleDelegate, objArray);
            }
        }

        private void SetButtonEnabled(ref RadButton btn, Boolean isEnabled)
        {
            if (!btn.InvokeRequired)
            {
                btn.Enabled = isEnabled;                
            }
            else
            {
                RadButton btnControl = btn;
                SetButtonEnabledDelegate setButtonEnabledDelegate = new SetButtonEnabledDelegate(SetButtonEnabled);
                Object[] objArray = new Object[] { btn, isEnabled };
                btnControl.Invoke(setButtonEnabledDelegate, objArray);
            }
        }

        private void SetGroupBoxEnabled(ref RadGroupBox grp, Boolean isEnabled)
        {
            if (!grp.InvokeRequired)
            {
                grp.Enabled = isEnabled;
            }
            else
            {
                RadGroupBox grpControl = grp;
                SetGroupBoxEnabledDelegate setGroupBoxEnabledDelegate = new SetGroupBoxEnabledDelegate(SetGroupBoxEnabled);
                Object[] objArray = new Object[] { grp, isEnabled };
                grpControl.Invoke(setGroupBoxEnabledDelegate, objArray);
            }
        }

        private void SetPictureVisible(ref PictureBox pic, Boolean isVisible)
        {
            if (!pic.InvokeRequired)
            {
                pic.Visible = isVisible;
            }
            else
            {
                PictureBox picControl = pic;
                SetPictureVisibleDelegate setPictureBoxVisibleDelegate = new SetPictureVisibleDelegate(SetPictureVisible);
                Object[] objArray = new Object[] { pic, isVisible };
                picControl.Invoke(setPictureBoxVisibleDelegate, objArray);
            }
        }

        private void SetStatusText(String Text)
        {
            if (!lblProgressStatus.InvokeRequired)
            {
                lblProgressStatus.Text = Text;
            }
            else
            {
                RadLabel labelControl = lblProgressStatus;
                SetStatusTextDelegate setStatusTextDelegate = new SetStatusTextDelegate(SetStatusText);
                Object[] text = new Object[] { Text };
                labelControl.Invoke(setStatusTextDelegate, text);
            }
        }

        private void AddTreeNode(RadTreeView tvw, String Text)
        {
            if (!tvw.InvokeRequired)
            {
                tvw.Nodes.Add(new RadTreeNode(Text));
            }
            else
            {
                AddTreeNodeDelegate addTreeNodeDelegate = new AddTreeNodeDelegate(AddTreeNode);
                tvw.Invoke(addTreeNodeDelegate, tvw, Text);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            String destinationDir = this.lblDestinationPDF.Text + (CommonMembers.CreateOutputFileName ? CommonMembers.CurrentFileName : "");
            try {
                Process.Start(destinationDir);
            } catch { }
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();
            frm.ShowDialog();
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            frmOptions frm = new frmOptions();
            frm.ShowDialog();
        }

        private void radRadioButton2_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            txtPageNumbers.Enabled = radRadioButton2.IsChecked;
            if (radRadioButton2.IsChecked)
                txtPageNumbers.Focus();

            if (sourceSelected && txtPageNumbers.Text.Trim().Length > 0)
            {
                if (analyzeThread != null)
                {
                    analyzeThread.Abort();
                    analyzeThread = null;
                }
                btnSourcePDF.Enabled = false;
                btnDestinationPDF.Enabled = false;
                radGroupBox1.Enabled = false;
                pnlProgress.Visible = true;
                pnlProgress.Enabled = true;
                btnOpenFolder.Enabled = false;
                radTreeView1.Nodes.Clear();
                analyzeFailed = false;
                analyzeThread = new Thread(new ThreadStart(AnalyzeFile))
                {
                    IsBackground = true
                };
                analyzeThread.Start();
            }
        }

        private void radRadioButton1_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            if (sourceSelected) {
                if (analyzeThread != null)
                {
                    analyzeThread.Abort();
                    analyzeThread = null;
                }
                pnlProgress.Visible = true;
                pnlProgress.Enabled = true;
                radGroupBox1.Enabled = false;
                btnSourcePDF.Enabled = false;
                btnDestinationPDF.Enabled = false;
                btnOpenFolder.Enabled = false;
                radTreeView1.Nodes.Clear();
                analyzeFailed = false;
                analyzeThread = new Thread(new ThreadStart(AnalyzeFile))
                {
                    IsBackground = true
                };
                analyzeThread.Start();
            }
                
        }

        private void txtPageNumbers_Leave(object sender, EventArgs e)
        {
            if (sourceSelected && txtPageNumbers.Text.Trim().Length > 0)
            {
                if (analyzeThread != null)
                {
                    analyzeThread.Abort();
                    analyzeThread = null;
                }
                pnlProgress.Visible = true;
                pnlProgress.Enabled = true;
                radGroupBox1.Enabled = false;
                btnSourcePDF.Enabled = false;
                btnDestinationPDF.Enabled = false;
                btnOpenFolder.Enabled = false;
                radTreeView1.Nodes.Clear();
                analyzeFailed = false;
                analyzeThread = new Thread(new ThreadStart(AnalyzeFile))
                {
                    IsBackground = true
                };
                analyzeThread.Start();
            }
        }
    }
}
