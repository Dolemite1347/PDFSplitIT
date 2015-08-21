namespace PDFSplitIT
{
    partial class frmOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkCreateOutput = new Telerik.WinControls.UI.RadCheckBox();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radGroupBox1 = new Telerik.WinControls.UI.RadGroupBox();
            this.chkAutoOpen = new Telerik.WinControls.UI.RadCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.chkCreateOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoOpen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // chkCreateOutput
            // 
            this.chkCreateOutput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateOutput.Location = new System.Drawing.Point(12, 54);
            this.chkCreateOutput.Name = "chkCreateOutput";
            this.chkCreateOutput.Size = new System.Drawing.Size(266, 18);
            this.chkCreateOutput.TabIndex = 0;
            this.chkCreateOutput.Text = "Create Output Directory With Original File Name";
            this.chkCreateOutput.ThemeName = "VisualStudio2012Light";
            this.chkCreateOutput.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            this.chkCreateOutput.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.chkCreateOutput_ToggleStateChanged);
            // 
            // radLabel1
            // 
            this.radLabel1.AutoSize = false;
            this.radLabel1.Location = new System.Drawing.Point(12, 12);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(493, 36);
            this.radLabel1.TabIndex = 1;
            this.radLabel1.Text = "Check the box below to create a directory in the output folder with the same name" +
    " as the original file.";
            this.radLabel1.ThemeName = "VisualStudio2012Light";
            // 
            // radButton1
            // 
            this.radButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.radButton1.Location = new System.Drawing.Point(395, 293);
            this.radButton1.Name = "radButton1";
            this.radButton1.Size = new System.Drawing.Size(110, 24);
            this.radButton1.TabIndex = 2;
            this.radButton1.Text = "&OK";
            this.radButton1.ThemeName = "VisualStudio2012Light";
            // 
            // radGroupBox1
            // 
            this.radGroupBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.radGroupBox1.HeaderText = "";
            this.radGroupBox1.Location = new System.Drawing.Point(-8, 274);
            this.radGroupBox1.Name = "radGroupBox1";
            this.radGroupBox1.Size = new System.Drawing.Size(530, 13);
            this.radGroupBox1.TabIndex = 4;
            this.radGroupBox1.ThemeName = "VisualStudio2012Light";
            // 
            // chkAutoOpen
            // 
            this.chkAutoOpen.Location = new System.Drawing.Point(12, 78);
            this.chkAutoOpen.Name = "chkAutoOpen";
            this.chkAutoOpen.Size = new System.Drawing.Size(232, 18);
            this.chkAutoOpen.TabIndex = 1;
            this.chkAutoOpen.Text = "Auto-open output directory after splitting";
            this.chkAutoOpen.ThemeName = "VisualStudio2012Light";
            this.chkAutoOpen.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.chkAutoOpen_ToggleStateChanged);
            // 
            // frmOptions
            // 
            this.AcceptButton = this.radButton1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 329);
            this.Controls.Add(this.chkAutoOpen);
            this.Controls.Add(this.radGroupBox1);
            this.Controls.Add(this.radButton1);
            this.Controls.Add(this.radLabel1);
            this.Controls.Add(this.chkCreateOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOptions";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.ThemeName = "VisualStudio2012Light";
            ((System.ComponentModel.ISupportInitialize)(this.chkCreateOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoOpen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.UI.RadCheckBox chkCreateOutput;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadButton radButton1;
        private Telerik.WinControls.UI.RadGroupBox radGroupBox1;
        private Telerik.WinControls.UI.RadCheckBox chkAutoOpen;
    }
}
