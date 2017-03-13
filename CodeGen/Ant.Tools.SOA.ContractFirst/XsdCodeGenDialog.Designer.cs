namespace Ant.Tools.SOA.ContractFirst
{
    partial class XsdCodeGenDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XsdCodeGenDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbWscf = new System.Windows.Forms.PictureBox();
            this.pbWizard = new System.Windows.Forms.PictureBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tbFileNames = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbDataBinding = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbOnlyUseDataContractSerializer = new System.Windows.Forms.CheckBox();
            this.cbLazyLoading = new System.Windows.Forms.CheckBox();
            this.cbList = new System.Windows.Forms.CheckBox();
            this.cbCollections = new System.Windows.Forms.CheckBox();
            this.cbComment = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbTargetFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbNamespace = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbOverwrite = new System.Windows.Forms.CheckBox();
            this.cbMultipleFiles = new System.Windows.Forms.CheckBox();
            this.bnCancel = new System.Windows.Forms.Button();
            this.bnGenerate = new System.Windows.Forms.Button();
            this.cbSettings = new System.Windows.Forms.CheckBox();
            this.cbMultipleFilesEachNamespace = new System.Windows.Forms.CheckBox();
            this.cbMultipleFilesEachXsd = new System.Windows.Forms.CheckBox();
            this.cbAscendingClassByName = new System.Windows.Forms.CheckBox();
            this.cbEnableBaijiSerialization = new System.Windows.Forms.CheckBox();
            this.tbCustomRequestInterface = new System.Windows.Forms.TextBox();
            this.cbAddCustomRequestInterface = new System.Windows.Forms.CheckBox();
            this.cbForceElementName = new System.Windows.Forms.CheckBox();
            this.cbForceElementNamespace = new System.Windows.Forms.CheckBox();
            this.cbGenerateAsyncOperations = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWscf)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWizard)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.pbWscf);
            this.panel1.Controls.Add(this.pbWizard);
            this.panel1.Location = new System.Drawing.Point(-3, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(655, 37);
            this.panel1.TabIndex = 0;
            // 
            // pbWscf
            // 
            this.pbWscf.Image = ((System.Drawing.Image)(resources.GetObject("pbWscf.Image")));
            this.pbWscf.Location = new System.Drawing.Point(551, 3);
            this.pbWscf.Name = "pbWscf";
            this.pbWscf.Size = new System.Drawing.Size(104, 30);
            this.pbWscf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWscf.TabIndex = 11;
            this.pbWscf.TabStop = false;
            this.pbWscf.Click += new System.EventHandler(this.pbWscf_Click);
            // 
            // pbWizard
            // 
            this.pbWizard.Image = ((System.Drawing.Image)(resources.GetObject("pbWizard.Image")));
            this.pbWizard.Location = new System.Drawing.Point(10, 3);
            this.pbWizard.Name = "pbWizard";
            this.pbWizard.Size = new System.Drawing.Size(40, 30);
            this.pbWizard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWizard.TabIndex = 10;
            this.pbWizard.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tbFileNames);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Location = new System.Drawing.Point(8, 43);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(644, 45);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "XSD information  ";
            // 
            // tbFileNames
            // 
            this.tbFileNames.Location = new System.Drawing.Point(75, 17);
            this.tbFileNames.Name = "tbFileNames";
            this.tbFileNames.ReadOnly = true;
            this.tbFileNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbFileNames.Size = new System.Drawing.Size(563, 21);
            this.tbFileNames.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "XSD file(s):";
            // 
            // cbDataBinding
            // 
            this.cbDataBinding.AutoSize = true;
            this.cbDataBinding.Location = new System.Drawing.Point(101, 24);
            this.cbDataBinding.Name = "cbDataBinding";
            this.cbDataBinding.Size = new System.Drawing.Size(96, 16);
            this.cbDataBinding.TabIndex = 2;
            this.cbDataBinding.Text = "Data binding";
            this.cbDataBinding.UseVisualStyleBackColor = true;
            this.cbDataBinding.Click += new System.EventHandler(this.cbDataBinding_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbOnlyUseDataContractSerializer);
            this.groupBox4.Controls.Add(this.cbLazyLoading);
            this.groupBox4.Controls.Add(this.cbList);
            this.groupBox4.Controls.Add(this.cbCollections);
            this.groupBox4.Controls.Add(this.cbDataBinding);
            this.groupBox4.Controls.Add(this.cbComment);
            this.groupBox4.Location = new System.Drawing.Point(8, 94);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(644, 49);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Code generation options  ";
            // 
            // cbOnlyUseDataContractSerializer
            // 
            this.cbOnlyUseDataContractSerializer.AutoSize = true;
            this.cbOnlyUseDataContractSerializer.Location = new System.Drawing.Point(481, 24);
            this.cbOnlyUseDataContractSerializer.Name = "cbOnlyUseDataContractSerializer";
            this.cbOnlyUseDataContractSerializer.Size = new System.Drawing.Size(132, 16);
            this.cbOnlyUseDataContractSerializer.TabIndex = 3;
            this.cbOnlyUseDataContractSerializer.Text = "Data Contract Only";
            this.cbOnlyUseDataContractSerializer.UseVisualStyleBackColor = true;
            this.cbOnlyUseDataContractSerializer.Click += new System.EventHandler(this.cbOnlyUseDataContractSerializer_Click);
            // 
            // cbLazyLoading
            // 
            this.cbLazyLoading.AutoSize = true;
            this.cbLazyLoading.Checked = true;
            this.cbLazyLoading.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLazyLoading.Location = new System.Drawing.Point(385, 24);
            this.cbLazyLoading.Name = "cbLazyLoading";
            this.cbLazyLoading.Size = new System.Drawing.Size(90, 16);
            this.cbLazyLoading.TabIndex = 2;
            this.cbLazyLoading.Text = "LazyLoading";
            this.cbLazyLoading.UseVisualStyleBackColor = true;
            this.cbLazyLoading.Click += new System.EventHandler(this.cbLazyLoading_Click);
            // 
            // cbList
            // 
            this.cbList.AutoSize = true;
            this.cbList.Checked = true;
            this.cbList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbList.Location = new System.Drawing.Point(301, 24);
            this.cbList.Name = "cbList";
            this.cbList.Size = new System.Drawing.Size(66, 16);
            this.cbList.TabIndex = 2;
            this.cbList.Text = "List<T>";
            this.cbList.UseVisualStyleBackColor = true;
            this.cbList.CheckedChanged += new System.EventHandler(this.cbList_CheckedChanged);
            // 
            // cbCollections
            // 
            this.cbCollections.AutoSize = true;
            this.cbCollections.Location = new System.Drawing.Point(203, 24);
            this.cbCollections.Name = "cbCollections";
            this.cbCollections.Size = new System.Drawing.Size(90, 16);
            this.cbCollections.TabIndex = 2;
            this.cbCollections.Text = "Collections";
            this.cbCollections.UseVisualStyleBackColor = true;
            this.cbCollections.CheckedChanged += new System.EventHandler(this.cbCollections_CheckedChanged);
            // 
            // cbComment
            // 
            this.cbComment.Checked = true;
            this.cbComment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbComment.Location = new System.Drawing.Point(16, 22);
            this.cbComment.Name = "cbComment";
            this.cbComment.Size = new System.Drawing.Size(79, 21);
            this.cbComment.TabIndex = 0;
            this.cbComment.Text = "Comment";
            this.cbComment.Click += new System.EventHandler(this.cbComment_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbTargetFileName);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.tbNamespace);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(8, 149);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(644, 79);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Files and namespaces  ";
            // 
            // tbTargetFileName
            // 
            this.tbTargetFileName.Location = new System.Drawing.Point(141, 45);
            this.tbTargetFileName.Name = "tbTargetFileName";
            this.tbTargetFileName.Size = new System.Drawing.Size(497, 21);
            this.tbTargetFileName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Namespace:";
            // 
            // tbNamespace
            // 
            this.tbNamespace.Location = new System.Drawing.Point(141, 20);
            this.tbNamespace.Name = "tbNamespace";
            this.tbNamespace.Size = new System.Drawing.Size(497, 21);
            this.tbNamespace.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "Target file name:";
            // 
            // cbOverwrite
            // 
            this.cbOverwrite.Location = new System.Drawing.Point(185, 238);
            this.cbOverwrite.Name = "cbOverwrite";
            this.cbOverwrite.Size = new System.Drawing.Size(169, 22);
            this.cbOverwrite.TabIndex = 5;
            this.cbOverwrite.Text = "Overwrite existing files";
            // 
            // cbMultipleFiles
            // 
            this.cbMultipleFiles.Location = new System.Drawing.Point(7, 266);
            this.cbMultipleFiles.Margin = new System.Windows.Forms.Padding(0);
            this.cbMultipleFiles.Name = "cbMultipleFiles";
            this.cbMultipleFiles.Size = new System.Drawing.Size(169, 22);
            this.cbMultipleFiles.TabIndex = 4;
            this.cbMultipleFiles.Text = "Separate files each type";
            this.cbMultipleFiles.Click += new System.EventHandler(this.cbMultipleFiles_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(528, 269);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(93, 30);
            this.bnCancel.TabIndex = 5;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // bnGenerate
            // 
            this.bnGenerate.Location = new System.Drawing.Point(410, 269);
            this.bnGenerate.Name = "bnGenerate";
            this.bnGenerate.Size = new System.Drawing.Size(93, 30);
            this.bnGenerate.TabIndex = 4;
            this.bnGenerate.Text = "Generate";
            this.bnGenerate.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbSettings
            // 
            this.cbSettings.Location = new System.Drawing.Point(7, 238);
            this.cbSettings.Name = "cbSettings";
            this.cbSettings.Size = new System.Drawing.Size(128, 21);
            this.cbSettings.TabIndex = 3;
            this.cbSettings.Text = "Remember settings";
            // 
            // cbMultipleFilesEachNamespace
            // 
            this.cbMultipleFilesEachNamespace.AutoSize = true;
            this.cbMultipleFilesEachNamespace.Location = new System.Drawing.Point(185, 269);
            this.cbMultipleFilesEachNamespace.Name = "cbMultipleFilesEachNamespace";
            this.cbMultipleFilesEachNamespace.Size = new System.Drawing.Size(198, 16);
            this.cbMultipleFilesEachNamespace.TabIndex = 6;
            this.cbMultipleFilesEachNamespace.Text = "Separate files each namespace";
            this.cbMultipleFilesEachNamespace.UseVisualStyleBackColor = true;
            this.cbMultipleFilesEachNamespace.Click += new System.EventHandler(this.cbMultipleFilesEachNamespace_Click);
            // 
            // cbMultipleFilesEachXsd
            // 
            this.cbMultipleFilesEachXsd.AutoSize = true;
            this.cbMultipleFilesEachXsd.Location = new System.Drawing.Point(185, 296);
            this.cbMultipleFilesEachXsd.Name = "cbMultipleFilesEachXsd";
            this.cbMultipleFilesEachXsd.Size = new System.Drawing.Size(156, 16);
            this.cbMultipleFilesEachXsd.TabIndex = 15;
            this.cbMultipleFilesEachXsd.Text = "Separate file each xsd";
            this.cbMultipleFilesEachXsd.UseVisualStyleBackColor = true;
            this.cbMultipleFilesEachXsd.Click += new System.EventHandler(this.cbMultipleFilesEachXsd_Click);
            // 
            // cbAscendingClassByName
            // 
            this.cbAscendingClassByName.AutoSize = true;
            this.cbAscendingClassByName.Location = new System.Drawing.Point(7, 296);
            this.cbAscendingClassByName.Name = "cbAscendingClassByName";
            this.cbAscendingClassByName.Size = new System.Drawing.Size(162, 16);
            this.cbAscendingClassByName.TabIndex = 14;
            this.cbAscendingClassByName.Text = "Ascending class by name";
            this.cbAscendingClassByName.UseVisualStyleBackColor = true;
            // 
            // cbEnableBaijiSerialization
            // 
            this.cbEnableBaijiSerialization.AutoSize = true;
            this.cbEnableBaijiSerialization.Checked = true;
            this.cbEnableBaijiSerialization.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableBaijiSerialization.Location = new System.Drawing.Point(7, 323);
            this.cbEnableBaijiSerialization.Name = "cbEnableBaijiSerialization";
            this.cbEnableBaijiSerialization.Size = new System.Drawing.Size(138, 16);
            this.cbEnableBaijiSerialization.TabIndex = 16;
            this.cbEnableBaijiSerialization.Text = "Baiji Serialization";
            this.cbEnableBaijiSerialization.UseVisualStyleBackColor = true;
            // 
            // tbCustomRequestInterface
            // 
            this.tbCustomRequestInterface.Location = new System.Drawing.Point(383, 320);
            this.tbCustomRequestInterface.Name = "tbCustomRequestInterface";
            this.tbCustomRequestInterface.Size = new System.Drawing.Size(241, 21);
            this.tbCustomRequestInterface.TabIndex = 18;
            this.tbCustomRequestInterface.Visible = false;
            // 
            // cbAddCustomRequestInterface
            // 
            this.cbAddCustomRequestInterface.AutoSize = true;
            this.cbAddCustomRequestInterface.Location = new System.Drawing.Point(185, 323);
            this.cbAddCustomRequestInterface.Name = "cbAddCustomRequestInterface";
            this.cbAddCustomRequestInterface.Size = new System.Drawing.Size(192, 16);
            this.cbAddCustomRequestInterface.TabIndex = 17;
            this.cbAddCustomRequestInterface.Text = "Add custom request interface";
            this.cbAddCustomRequestInterface.UseVisualStyleBackColor = true;
            // 
            // cbForceElementName
            // 
            this.cbForceElementName.AutoSize = true;
            this.cbForceElementName.Location = new System.Drawing.Point(7, 350);
            this.cbForceElementName.Name = "cbForceElementName";
            this.cbForceElementName.Size = new System.Drawing.Size(156, 16);
            this.cbForceElementName.TabIndex = 19;
            this.cbForceElementName.Text = "Normalize element name";
            this.cbForceElementName.UseVisualStyleBackColor = true;
            // 
            // cbForceElementNamespace
            // 
            this.cbForceElementNamespace.AutoSize = true;
            this.cbForceElementNamespace.Location = new System.Drawing.Point(185, 350);
            this.cbForceElementNamespace.Name = "cbForceElementNamespace";
            this.cbForceElementNamespace.Size = new System.Drawing.Size(186, 16);
            this.cbForceElementNamespace.TabIndex = 20;
            this.cbForceElementNamespace.Text = "Normalize element namespace";
            this.cbForceElementNamespace.UseVisualStyleBackColor = true;
            // 
            // cbGenerateAsyncOperations
            // 
            this.cbGenerateAsyncOperations.AutoSize = true;
            this.cbGenerateAsyncOperations.Checked = true;
            this.cbGenerateAsyncOperations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbGenerateAsyncOperations.Location = new System.Drawing.Point(7, 376);
            this.cbGenerateAsyncOperations.Name = "cbGenerateAsyncOperations";
            this.cbGenerateAsyncOperations.Size = new System.Drawing.Size(174, 16);
            this.cbGenerateAsyncOperations.TabIndex = 21;
            this.cbGenerateAsyncOperations.Text = "Generate Async Operations";
            this.cbGenerateAsyncOperations.UseVisualStyleBackColor = true;
            // 
            // XsdCodeGenDialog
            // 
            this.AcceptButton = this.bnGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(664, 409);
            this.Controls.Add(this.cbGenerateAsyncOperations);
            this.Controls.Add(this.cbForceElementNamespace);
            this.Controls.Add(this.cbForceElementName);
            this.Controls.Add(this.tbCustomRequestInterface);
            this.Controls.Add(this.cbAddCustomRequestInterface);
            this.Controls.Add(this.cbEnableBaijiSerialization);
            this.Controls.Add(this.cbMultipleFilesEachXsd);
            this.Controls.Add(this.cbAscendingClassByName);
            this.Controls.Add(this.cbMultipleFilesEachNamespace);
            this.Controls.Add(this.cbSettings);
            this.Controls.Add(this.cbOverwrite);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.cbMultipleFiles);
            this.Controls.Add(this.bnGenerate);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "XsdCodeGenDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ant SOA Data Contract Code Generation Options ";
            this.Load += new System.EventHandler(this.XsdCodeGenDialog_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbWscf)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWizard)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pbWscf;
        private System.Windows.Forms.PictureBox pbWizard;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbDataBinding;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cbComment;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbTargetFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbNamespace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbMultipleFiles;
        private System.Windows.Forms.CheckBox cbOverwrite;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.Button bnGenerate;
        private System.Windows.Forms.TextBox tbFileNames;
        private System.Windows.Forms.CheckBox cbSettings;
        private System.Windows.Forms.CheckBox cbList;
        private System.Windows.Forms.CheckBox cbCollections;
        private System.Windows.Forms.CheckBox cbLazyLoading;
        private System.Windows.Forms.CheckBox cbMultipleFilesEachNamespace;
        private System.Windows.Forms.CheckBox cbOnlyUseDataContractSerializer;
        private System.Windows.Forms.CheckBox cbMultipleFilesEachXsd;
        private System.Windows.Forms.CheckBox cbAscendingClassByName;
        private System.Windows.Forms.CheckBox cbEnableBaijiSerialization;
        private System.Windows.Forms.TextBox tbCustomRequestInterface;
        private System.Windows.Forms.CheckBox cbAddCustomRequestInterface;
        private System.Windows.Forms.CheckBox cbForceElementName;
        private System.Windows.Forms.CheckBox cbForceElementNamespace;
        private System.Windows.Forms.CheckBox cbGenerateAsyncOperations;
    }
}
