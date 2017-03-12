using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CTrip.Tools.SOA.ContractFirst.Util;

namespace CTrip.Tools.SOA.ContractFirst
{
    public partial class XsdCodeGenDialog : Form
    {
        #region Constructors

        public XsdCodeGenDialog(string[] xsdfiles)
        {
            InitializeComponent();

            // Fill the file names text box.
            tbFileNames.Text = string.Join(";", xsdfiles);

            this.Text += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.cbAddCustomRequestInterface.CheckedChanged += (o, e) =>
            {
                this.tbCustomRequestInterface.Visible = this.cbAddCustomRequestInterface.Checked;
                this.tbCustomRequestInterface.Text = "";
            };
            Win32Utility.SetCueText(this.tbCustomRequestInterface, "Custom request interface");
        }

        #endregion

        #region Event handlers

        private void XsdCodeGenDialog_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(XsdCodeGenDialog_FormClosing);
            LoadFormValues();
        }

        void XsdCodeGenDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tbTargetFileName.Text.Trim() == "" ||
                tbTargetFileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                MessageBox.Show("Please enter a valid name for the target file name",
                                "CTrip SOA code generation",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (tbNamespace.Text.Trim() == "" ||
                !IsMatchingPattern(@"^(?:(?:((?![0-9_])[a-zA-Z0-9_]+)\.?)+)(?<!\.)$", tbNamespace.Text))
            {
                MessageBox.Show("Please enter a valid name for the namespace",
                                "CTrip SOA code generation",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (cbAddCustomRequestInterface.Checked)
            {
                if (tbCustomRequestInterface.Text.Length == 0 ||
                    !CTrip.Tools.SOA.Util.ValidationHelper.IsIdentifier(tbCustomRequestInterface.Text))
                {
                    MessageBox.Show("Please enter a valid name for the interface",
                                                   "CTrip SOA code generation",
                                                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            SaveFormValues();
            this.Close();
        }

        #endregion

        #region Properties

        public bool Comment
        {
            get { return cbComment.Checked; }
        }

        public bool Collections
        {
            get { return cbCollections.Checked; }
        }

        public bool TypedList
        {
            get { return cbList.Checked; }
        }

        public bool LazyLoading
        {
            get { return cbLazyLoading.Checked; }
        }

        public bool DataBinding
        {
            get { return cbDataBinding.Checked; }
        }

        public bool OnlyUseDataContractSerializer
        {
            get { return cbOnlyUseDataContractSerializer.Checked; }
        }

        public bool OverwriteFiles
        {
            get { return cbOverwrite.Checked; }
        }

        public bool GenerateMultipleFiles
        {
            get { return cbMultipleFiles.Checked; }
        }

        public bool GenerateMultipleFilesEachNamespace
        {
            get { return cbMultipleFilesEachNamespace.Checked; }
        }

        public bool GenerateSeparateFilesEachXsd
        {
            get { return cbMultipleFilesEachXsd.Checked; }
        }

        public bool AscendingClassByName
        {
            get { return cbAscendingClassByName.Checked; }
        }

        public bool EnableBaijiSerialization
        {
            get { return cbEnableBaijiSerialization.Checked; }
        }

        public bool ForceElementName
        {
            get { return this.cbForceElementName.Checked; }
        }

        public bool ForceElementNamespace
        {
            get { return this.cbForceElementNamespace.Checked; }
        }

        public bool GenerateAsyncOperations
        {
            get { return this.cbGenerateAsyncOperations.Checked; }
        }

        public bool AddCustomRequestInterface
        {
            get { return this.cbAddCustomRequestInterface.Checked; }
        }

        public string CustomRequestInterface
        {
            get { return this.tbCustomRequestInterface.Text; }
        }

        public string Namespace
        {
            get { return tbNamespace.Text; }
            set { tbNamespace.Text = value; }
        }

        public string TargetFileName
        {
            get { return tbTargetFileName.Text; }
            set { tbTargetFileName.Text = value; }
        }

        #endregion

        bool IsMatchingPattern(string pattern, string value)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(value);
            return match.Success;
        }

        private void pbWscf_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.ctrip.com/");
        }

        /// <summary>
        /// Saves the form values
        /// </summary>
        private void SaveFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager("CodeGen05");
            if (cbSettings.Checked)
            {
                config.Write("xsdComment", cbComment.Checked.ToString());
                config.Write("xsdCollections", cbCollections.Checked.ToString());
                config.Write("xsdList", cbList.Checked.ToString());
                config.Write("xsdLazyLoading", cbLazyLoading.Checked.ToString());
                config.Write("xsdDataBinding", cbDataBinding.Checked.ToString());
                config.Write("xsdOnlyUseDataContractSerializer", cbOnlyUseDataContractSerializer.Checked.ToString());
                config.Write("xsdMultipleFiles", cbMultipleFiles.Checked.ToString());
                config.Write("xsdMultipleFilesEachNamespace", cbMultipleFilesEachNamespace.Checked.ToString());
                config.Write("xsdMultipleFilesEachXsd", cbMultipleFilesEachXsd.Checked.ToString());
                config.Write("xsdOverwrite", cbOverwrite.Checked.ToString());
                config.Write("xsdRememberSettings", cbSettings.Checked.ToString());
                config.Write("xsdDestinationNamespace", tbNamespace.Text);
                config.Write("xsdDestinationFilename", tbTargetFileName.Text);
                config.Write("xsdAscendingClassByName", cbAscendingClassByName.Checked.ToString());
                config.Write("xsdEnableBaijiSerialization", cbEnableBaijiSerialization.Checked.ToString());
                config.Write("xsdAddCustomInterface", cbAddCustomRequestInterface.Checked.ToString());
                config.Write("xsdCustomInterface", tbCustomRequestInterface.Text);
                config.Write("xsdForceElementName", cbForceElementName.Checked.ToString());
                config.Write("xsdForceElementNamespace", cbForceElementNamespace.Checked.ToString());
                config.Write("xsdGenerateAsyncOperations", cbGenerateAsyncOperations.Checked.ToString());
            }
            else
            {
                config.Write("xsdRememberSettings", "false");
            }
            config.Persist();
        }

        /// <summary>
        /// Loads the values for the UI elements from the persisted storage.
        /// </summary>
        private void LoadFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager("CodeGen05");
            if ((cbSettings.Checked = config.ReadBoolean("xsdRememberSettings")))
            {
                cbComment.Checked = config.ReadBoolean("xsdComment");
                cbCollections.Checked = config.ReadBoolean("xsdCollections");
                cbList.Checked = config.ReadBoolean("xsdList");
                cbLazyLoading.Checked = config.ReadBoolean("xsdLazyLoading");
                cbDataBinding.Checked = config.ReadBoolean("xsdDataBinding");
                cbOnlyUseDataContractSerializer.Checked = config.ReadBoolean("xsdOnlyUseDataContractSerializer");
                cbMultipleFiles.Checked = config.ReadBoolean("xsdMultipleFiles");
                cbMultipleFilesEachNamespace.Checked = config.ReadBoolean("xsdMultipleFilesEachNamespace");
                cbMultipleFilesEachXsd.Checked = config.ReadBoolean("xsdMultipleFilesEachXsd");
                cbOverwrite.Checked = config.ReadBoolean("xsdOverwrite");
                tbNamespace.Text = config.Read("xsdDestinationNamespace");
                tbTargetFileName.Text = config.Read("xsdDestinationFilename");
                cbAscendingClassByName.Checked = config.ReadBoolean("xsdAscendingClassByName");
                cbEnableBaijiSerialization.Checked = config.ReadBoolean("xsdEnableBaijiSerialization");
                cbAddCustomRequestInterface.Checked = config.ReadBoolean("xsdAddCustomInterface");
                tbCustomRequestInterface.Text = config.Read("xsdCustomInterface");
                cbForceElementName.Checked = config.ReadBoolean("xsdForceElementName");
                cbForceElementNamespace.Checked = config.ReadBoolean("xsdForceElementNamespace");
                cbGenerateAsyncOperations.Checked = config.ReadBoolean("xsdGenerateAsyncOperations");
                if (cbMultipleFiles.Checked || cbMultipleFilesEachNamespace.Checked || cbMultipleFilesEachXsd.Checked)
                    this.tbTargetFileName.Enabled = false;
                if (cbOnlyUseDataContractSerializer.Checked)
                {
                    cbMultipleFilesEachXsd.Checked = false;
                    cbMultipleFilesEachXsd.Enabled = false;
                }
                else
                    cbMultipleFilesEachXsd.Enabled = true;
            }
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
        }

        private void cbList_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbList.Checked)
            {
                this.cbCollections.Checked = false;
                this.cbOnlyUseDataContractSerializer.Checked = false;
            }
        }

        private void cbCollections_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbCollections.Checked)
            {
                this.cbList.Checked = false;
                this.cbOnlyUseDataContractSerializer.Checked = false;
            }
        }

        private void cbMultipleFiles_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFiles.Checked)
            {
                this.cbMultipleFilesEachNamespace.Checked = false;
                this.cbMultipleFilesEachXsd.Checked = false;
                this.tbTargetFileName.Enabled = false;
            }
            else
            {
                this.tbTargetFileName.Enabled = true;
            }
        }

        private void cbMultipleFilesEachNamespace_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFilesEachNamespace.Checked)
            {
                this.cbMultipleFiles.Checked = false;
                this.cbMultipleFilesEachXsd.Checked = false;
                this.tbTargetFileName.Enabled = false;
            }
            else
            {
                this.tbTargetFileName.Enabled = true;
            }
        }

        private void cbMultipleFilesEachXsd_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFilesEachXsd.Checked)
            {
                this.cbMultipleFiles.Checked = false;
                this.cbMultipleFilesEachNamespace.Checked = false;
                this.tbTargetFileName.Enabled = false;
            }
            else
            {
                this.tbTargetFileName.Enabled = true;
            }
        }

        private void cbOnlyUseDataContractSerializer_Click(object sender, EventArgs e)
        {
            cbLazyLoading.Checked = false;
            cbDataBinding.Checked = false;
            cbCollections.Checked = false;
            cbList.Checked = false;
            cbComment.Checked = false;
            if (cbOnlyUseDataContractSerializer.Checked)
            {
                cbMultipleFilesEachXsd.Checked = false;
                cbMultipleFilesEachXsd.Enabled = false;
            }
            else
                cbMultipleFilesEachXsd.Enabled = true;
        }

        private void cbLazyLoading_Click(object sender, EventArgs e)
        {
            cbOnlyUseDataContractSerializer.Checked = false;
        }

        private void cbDataBinding_Click(object sender, EventArgs e)
        {
            cbOnlyUseDataContractSerializer.Checked = false;
        }

        private void cbComment_Click(object sender, EventArgs e)
        {
            cbOnlyUseDataContractSerializer.Checked = false;
        }
    }
}