using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using CTrip.Tools.SOA.Util;
using CTrip.Tools.SOA.ContractFirst.Util;

namespace CTrip.Tools.SOA.ContractFirst
{
    /// <summary>
    /// Summary description for WebServiceCodeGenOptions.
    /// </summary>
    public class WebServiceCodeGenDialogNew : Form
    {
        private Button button1;
        private Button button2;
        private GroupBox groupBox2;
        private Label label1;
        private TextBox tbDestinationFilename;
        private Label label2;
        private TextBox tbDestinationNamespace;
        private ToolTip cfTooltip;
        private Panel panel1;
        private GroupBox groupBox5;
        private Label label4;
        private Button bnBrowse;
        private OpenFileDialog openFileDialogWSDL;
        private IContainer components;
        private CheckBox cbSettings;
        private CheckBox cbSeperateFiles;
        private PictureBox pbWizard;
        private ComboBox cbWsdlLocation;
        private System.Windows.Forms.CheckBox cbOverwrite;
        private System.Windows.Forms.PictureBox pbWscf;
        private System.Windows.Forms.CheckBox cbMultipleFiles;
        private System.Windows.Forms.CheckBox cbComment;
        private System.Windows.Forms.RadioButton rbServer;
        private System.Windows.Forms.RadioButton rbClient;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private CheckBox cbDataBinding;
        private ArrayList wsdlFileCache;

        private bool externalFile = false;
        private string wsdlLocation = "";
        private string wsdlPath = "";
        private string wsdlFileName = "";
        private CheckBox cbLazyLoading;
        private CheckBox cbTypedList;
        private CheckBox cbCollections;
        private CheckBox cbMultipleFilesEachNamespace;
        private RadioButton rbClientForTest;
        private CheckBox cbOnlyUseDataContractSerializer;
        private CheckBox cbAscendingClassByName;
        private CheckBox cbMultipleFilesEachXsd;
        private CheckBox cbEnableBaijiSerialization;
        private CheckBox cbAddCustomRequestInterface;
        private TextBox tbCustomRequestInterface;
        private CheckBox cbForceElementName;
        private CheckBox cbForceElementNamespace;
        private bool isLoading = true;
        private CheckBox cbGenerateAsyncOperations;

        private static string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public WebServiceCodeGenDialogNew()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Initialize the .wsdl file cache.
            wsdlFileCache = new ArrayList();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            this.Text += version;

            this.cbAddCustomRequestInterface.CheckedChanged += (o, e) =>
            {
                this.tbCustomRequestInterface.Visible = this.cbAddCustomRequestInterface.Checked;
                this.tbCustomRequestInterface.Text = "";
            };
            Win32Utility.SetCueText(this.tbCustomRequestInterface, "Custom request interface");
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebServiceCodeGenDialogNew));
            this.cbSeperateFiles = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tbDestinationNamespace = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDestinationFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cfTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.cbSettings = new System.Windows.Forms.CheckBox();
            this.pbWscf = new System.Windows.Forms.PictureBox();
            this.cbMultipleFiles = new System.Windows.Forms.CheckBox();
            this.cbComment = new System.Windows.Forms.CheckBox();
            this.rbServer = new System.Windows.Forms.RadioButton();
            this.rbClient = new System.Windows.Forms.RadioButton();
            this.cbOverwrite = new System.Windows.Forms.CheckBox();
            this.cbDataBinding = new System.Windows.Forms.CheckBox();
            this.rbClientForTest = new System.Windows.Forms.RadioButton();
            this.cbAscendingClassByName = new System.Windows.Forms.CheckBox();
            this.cbForceElementName = new System.Windows.Forms.CheckBox();
            this.cbForceElementNamespace = new System.Windows.Forms.CheckBox();
            this.cbGenerateAsyncOperations = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbWizard = new System.Windows.Forms.PictureBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cbWsdlLocation = new System.Windows.Forms.ComboBox();
            this.bnBrowse = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.openFileDialogWSDL = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbOnlyUseDataContractSerializer = new System.Windows.Forms.CheckBox();
            this.cbLazyLoading = new System.Windows.Forms.CheckBox();
            this.cbTypedList = new System.Windows.Forms.CheckBox();
            this.cbCollections = new System.Windows.Forms.CheckBox();
            this.cbMultipleFilesEachNamespace = new System.Windows.Forms.CheckBox();
            this.cbMultipleFilesEachXsd = new System.Windows.Forms.CheckBox();
            this.cbEnableBaijiSerialization = new System.Windows.Forms.CheckBox();
            this.cbAddCustomRequestInterface = new System.Windows.Forms.CheckBox();
            this.tbCustomRequestInterface = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWscf)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWizard)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbSeperateFiles
            // 
            this.cbSeperateFiles.Location = new System.Drawing.Point(0, 0);
            this.cbSeperateFiles.Name = "cbSeperateFiles";
            this.cbSeperateFiles.Size = new System.Drawing.Size(104, 24);
            this.cbSeperateFiles.TabIndex = 0;
            this.cfTooltip.SetToolTip(this.cbSeperateFiles, "Generates collection-based members instead of arrays.");
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(421, 379);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 30);
            this.button1.TabIndex = 5;
            this.button1.Text = "Generate";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(538, 379);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 30);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            // 
            // tbDestinationNamespace
            // 
            this.tbDestinationNamespace.Location = new System.Drawing.Point(182, 53);
            this.tbDestinationNamespace.Name = "tbDestinationNamespace";
            this.tbDestinationNamespace.Size = new System.Drawing.Size(471, 21);
            this.tbDestinationNamespace.TabIndex = 3;
            this.cfTooltip.SetToolTip(this.tbDestinationNamespace, "Please enter the name of .NET namespace for the client proxy.");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbDestinationNamespace);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.tbDestinationFilename);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(10, 253);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(663, 87);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Files and namespaces  ";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Destination file name";
            // 
            // tbDestinationFilename
            // 
            this.tbDestinationFilename.Location = new System.Drawing.Point(182, 28);
            this.tbDestinationFilename.Name = "tbDestinationFilename";
            this.tbDestinationFilename.Size = new System.Drawing.Size(471, 21);
            this.tbDestinationFilename.TabIndex = 1;
            this.cfTooltip.SetToolTip(this.tbDestinationFilename, "Please enter the name of .NET proxy file that gets generated.");
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Destination namespace";
            // 
            // cbSettings
            // 
            this.cbSettings.Location = new System.Drawing.Point(12, 355);
            this.cbSettings.Name = "cbSettings";
            this.cbSettings.Size = new System.Drawing.Size(130, 24);
            this.cbSettings.TabIndex = 3;
            this.cbSettings.Text = "Remember settings";
            this.cfTooltip.SetToolTip(this.cbSettings, "Save dialog settings for future use.");
            // 
            // pbWscf
            // 
            this.pbWscf.Image = ((System.Drawing.Image)(resources.GetObject("pbWscf.Image")));
            this.pbWscf.Location = new System.Drawing.Point(554, 4);
            this.pbWscf.Name = "pbWscf";
            this.pbWscf.Size = new System.Drawing.Size(125, 35);
            this.pbWscf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWscf.TabIndex = 11;
            this.pbWscf.TabStop = false;
            this.cfTooltip.SetToolTip(this.pbWscf, "http://www.ctrip.com/");
            this.pbWscf.Click += new System.EventHandler(this.pbWscf_Click);
            // 
            // cbMultipleFiles
            // 
            this.cbMultipleFiles.Location = new System.Drawing.Point(12, 383);
            this.cbMultipleFiles.Name = "cbMultipleFiles";
            this.cbMultipleFiles.Size = new System.Drawing.Size(169, 26);
            this.cbMultipleFiles.TabIndex = 9;
            this.cbMultipleFiles.Text = "Separate file each type";
            this.cfTooltip.SetToolTip(this.cbMultipleFiles, "Generates each data type into its own seperate source file.");
            this.cbMultipleFiles.Click += new System.EventHandler(this.cbMultipleFiles_Click);
            // 
            // cbComment
            // 
            this.cbComment.Checked = true;
            this.cbComment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbComment.Location = new System.Drawing.Point(19, 26);
            this.cbComment.Name = "cbComment";
            this.cbComment.Size = new System.Drawing.Size(85, 25);
            this.cbComment.TabIndex = 0;
            this.cbComment.Text = "Comment";
            this.cfTooltip.SetToolTip(this.cbComment, "Generate serializable attribute on data contract class");
            this.cbComment.Click += new System.EventHandler(this.cbComment_Click);
            // 
            // rbServer
            // 
            this.rbServer.Location = new System.Drawing.Point(178, 20);
            this.rbServer.Name = "rbServer";
            this.rbServer.Size = new System.Drawing.Size(134, 26);
            this.rbServer.TabIndex = 1;
            this.rbServer.Text = "Service-side stub";
            this.cfTooltip.SetToolTip(this.rbServer, "Select this to generate the service-side stub");
            this.rbServer.Click += new System.EventHandler(this.rbServer_Click);
            // 
            // rbClient
            // 
            this.rbClient.Location = new System.Drawing.Point(20, 20);
            this.rbClient.Name = "rbClient";
            this.rbClient.Size = new System.Drawing.Size(134, 26);
            this.rbClient.TabIndex = 0;
            this.rbClient.Text = "Client-side proxy";
            this.cfTooltip.SetToolTip(this.rbClient, "Select this to generate the client-side proxy");
            this.rbClient.Click += new System.EventHandler(this.rbClient_Click);
            // 
            // cbOverwrite
            // 
            this.cbOverwrite.Location = new System.Drawing.Point(197, 354);
            this.cbOverwrite.Name = "cbOverwrite";
            this.cbOverwrite.Size = new System.Drawing.Size(173, 26);
            this.cbOverwrite.TabIndex = 4;
            this.cbOverwrite.Text = "Overwrite existing files";
            this.cfTooltip.SetToolTip(this.cbOverwrite, "Overwrite all files upon code generation.");
            this.cbOverwrite.CheckedChanged += new System.EventHandler(this.cbOverwrite_CheckedChanged);
            // 
            // cbDataBinding
            // 
            this.cbDataBinding.AutoSize = true;
            this.cbDataBinding.Location = new System.Drawing.Point(95, 30);
            this.cbDataBinding.Name = "cbDataBinding";
            this.cbDataBinding.Size = new System.Drawing.Size(96, 16);
            this.cbDataBinding.TabIndex = 2;
            this.cbDataBinding.Text = "Data binding";
            this.cfTooltip.SetToolTip(this.cbDataBinding, "Implement INotifyPropertyChanged interface on all generated types to enable data " +
        "binding.");
            this.cbDataBinding.UseVisualStyleBackColor = true;
            this.cbDataBinding.Click += new System.EventHandler(this.cbDataBinding_Click);
            // 
            // rbClientForTest
            // 
            this.rbClientForTest.AutoSize = true;
            this.rbClientForTest.Location = new System.Drawing.Point(336, 25);
            this.rbClientForTest.Name = "rbClientForTest";
            this.rbClientForTest.Size = new System.Drawing.Size(233, 16);
            this.rbClientForTest.TabIndex = 3;
            this.rbClientForTest.TabStop = true;
            this.rbClientForTest.Text = "Client-side proxy for QA automation";
            this.cfTooltip.SetToolTip(this.rbClientForTest, "Select this to generate the auto-test client-side proxy");
            this.rbClientForTest.UseVisualStyleBackColor = true;
            this.rbClientForTest.Visible = false;
            this.rbClientForTest.Click += new System.EventHandler(this.rbClientForTest_Click);
            // 
            // cbAscendingClassByName
            // 
            this.cbAscendingClassByName.AutoSize = true;
            this.cbAscendingClassByName.Location = new System.Drawing.Point(12, 415);
            this.cbAscendingClassByName.Name = "cbAscendingClassByName";
            this.cbAscendingClassByName.Size = new System.Drawing.Size(162, 16);
            this.cbAscendingClassByName.TabIndex = 12;
            this.cbAscendingClassByName.Text = "Ascending class by name";
            this.cfTooltip.SetToolTip(this.cbAscendingClassByName, "Make types ascending in file(s)");
            this.cbAscendingClassByName.UseVisualStyleBackColor = true;
            // 
            // cbForceElementName
            // 
            this.cbForceElementName.AutoSize = true;
            this.cbForceElementName.Location = new System.Drawing.Point(12, 469);
            this.cbForceElementName.Name = "cbForceElementName";
            this.cbForceElementName.Size = new System.Drawing.Size(156, 16);
            this.cbForceElementName.TabIndex = 15;
            this.cbForceElementName.Text = "Normalize element name";
            this.cfTooltip.SetToolTip(this.cbForceElementName, "Force XmlRootAttribute\'ElementName to xsd:element:name");
            this.cbForceElementName.UseVisualStyleBackColor = true;
            // 
            // cbForceElementNamespace
            // 
            this.cbForceElementNamespace.AutoSize = true;
            this.cbForceElementNamespace.Location = new System.Drawing.Point(197, 469);
            this.cbForceElementNamespace.Name = "cbForceElementNamespace";
            this.cbForceElementNamespace.Size = new System.Drawing.Size(186, 16);
            this.cbForceElementNamespace.TabIndex = 17;
            this.cbForceElementNamespace.Text = "Normalize element namespace";
            this.cfTooltip.SetToolTip(this.cbForceElementNamespace, "Force XmlRootAttribute\'ElementName to xsd:element:name");
            this.cbForceElementNamespace.UseVisualStyleBackColor = true;
            // 
            // cbGenerateAsyncOperations
            // 
            this.cbGenerateAsyncOperations.AutoSize = true;
            this.cbGenerateAsyncOperations.Checked = true;
            this.cbGenerateAsyncOperations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbGenerateAsyncOperations.Location = new System.Drawing.Point(12, 496);
            this.cbGenerateAsyncOperations.Name = "cbGenerateAsyncOperations";
            this.cbGenerateAsyncOperations.Size = new System.Drawing.Size(174, 16);
            this.cbGenerateAsyncOperations.TabIndex = 18;
            this.cbGenerateAsyncOperations.Text = "Generate Async Operations";
            this.cfTooltip.SetToolTip(this.cbGenerateAsyncOperations, "Use async pattern to generate async service operation");
            this.cbGenerateAsyncOperations.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.pbWscf);
            this.panel1.Controls.Add(this.pbWizard);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(778, 43);
            this.panel1.TabIndex = 10;
            // 
            // pbWizard
            // 
            this.pbWizard.Image = ((System.Drawing.Image)(resources.GetObject("pbWizard.Image")));
            this.pbWizard.Location = new System.Drawing.Point(12, 3);
            this.pbWizard.Name = "pbWizard";
            this.pbWizard.Size = new System.Drawing.Size(48, 35);
            this.pbWizard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWizard.TabIndex = 10;
            this.pbWizard.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cbWsdlLocation);
            this.groupBox5.Controls.Add(this.bnBrowse);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Location = new System.Drawing.Point(10, 52);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(663, 52);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Contract information  ";
            // 
            // cbWsdlLocation
            // 
            this.cbWsdlLocation.Enabled = false;
            this.cbWsdlLocation.Location = new System.Drawing.Point(115, 19);
            this.cbWsdlLocation.MaxDropDownItems = 10;
            this.cbWsdlLocation.Name = "cbWsdlLocation";
            this.cbWsdlLocation.Size = new System.Drawing.Size(485, 20);
            this.cbWsdlLocation.TabIndex = 1;
            this.cbWsdlLocation.SelectedIndexChanged += new System.EventHandler(this.cbWsdlLocation_SelectedIndexChanged);
            this.cbWsdlLocation.TextChanged += new System.EventHandler(this.tbWSDLLocation_TextChanged);
            this.cbWsdlLocation.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cbWsdlLocation_MouseMove);
            // 
            // bnBrowse
            // 
            this.bnBrowse.Location = new System.Drawing.Point(614, 18);
            this.bnBrowse.Name = "bnBrowse";
            this.bnBrowse.Size = new System.Drawing.Size(39, 25);
            this.bnBrowse.TabIndex = 2;
            this.bnBrowse.Text = "...";
            this.bnBrowse.Click += new System.EventHandler(this.bnBrowse_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(10, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "WSDL location:";
            // 
            // openFileDialogWSDL
            // 
            this.openFileDialogWSDL.Filter = "WSDL files|*.wsdl|All Files|*.*";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbClientForTest);
            this.groupBox1.Controls.Add(this.rbServer);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.rbClient);
            this.groupBox1.Location = new System.Drawing.Point(10, 111);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(663, 135);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Code generation  ";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbOnlyUseDataContractSerializer);
            this.groupBox4.Controls.Add(this.cbLazyLoading);
            this.groupBox4.Controls.Add(this.cbTypedList);
            this.groupBox4.Controls.Add(this.cbCollections);
            this.groupBox4.Controls.Add(this.cbDataBinding);
            this.groupBox4.Controls.Add(this.cbComment);
            this.groupBox4.Location = new System.Drawing.Point(10, 49);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(643, 71);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Options  ";
            // 
            // cbOnlyUseDataContractSerializer
            // 
            this.cbOnlyUseDataContractSerializer.AutoSize = true;
            this.cbOnlyUseDataContractSerializer.Location = new System.Drawing.Point(490, 29);
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
            this.cbLazyLoading.Location = new System.Drawing.Point(384, 30);
            this.cbLazyLoading.Name = "cbLazyLoading";
            this.cbLazyLoading.Size = new System.Drawing.Size(90, 16);
            this.cbLazyLoading.TabIndex = 2;
            this.cbLazyLoading.Text = "LazyLoading";
            this.cbLazyLoading.UseVisualStyleBackColor = true;
            this.cbLazyLoading.Click += new System.EventHandler(this.cbLazyLoading_Click);
            // 
            // cbTypedList
            // 
            this.cbTypedList.AutoSize = true;
            this.cbTypedList.Checked = true;
            this.cbTypedList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbTypedList.Location = new System.Drawing.Point(303, 30);
            this.cbTypedList.Name = "cbTypedList";
            this.cbTypedList.Size = new System.Drawing.Size(66, 16);
            this.cbTypedList.TabIndex = 2;
            this.cbTypedList.Text = "List<T>";
            this.cbTypedList.UseVisualStyleBackColor = true;
            this.cbTypedList.CheckedChanged += new System.EventHandler(this.cbTypedList_CheckedChanged);
            // 
            // cbCollections
            // 
            this.cbCollections.AutoSize = true;
            this.cbCollections.Location = new System.Drawing.Point(207, 30);
            this.cbCollections.Name = "cbCollections";
            this.cbCollections.Size = new System.Drawing.Size(90, 16);
            this.cbCollections.TabIndex = 2;
            this.cbCollections.Text = "Collections";
            this.cbCollections.UseVisualStyleBackColor = true;
            this.cbCollections.CheckedChanged += new System.EventHandler(this.cbCollections_CheckedChanged);
            // 
            // cbMultipleFilesEachNamespace
            // 
            this.cbMultipleFilesEachNamespace.AutoSize = true;
            this.cbMultipleFilesEachNamespace.Location = new System.Drawing.Point(197, 388);
            this.cbMultipleFilesEachNamespace.Name = "cbMultipleFilesEachNamespace";
            this.cbMultipleFilesEachNamespace.Size = new System.Drawing.Size(192, 16);
            this.cbMultipleFilesEachNamespace.TabIndex = 11;
            this.cbMultipleFilesEachNamespace.Text = "Separate file each namespace";
            this.cbMultipleFilesEachNamespace.UseVisualStyleBackColor = true;
            this.cbMultipleFilesEachNamespace.Click += new System.EventHandler(this.cbMultipleFilesEachNamespace_Click);
            // 
            // cbMultipleFilesEachXsd
            // 
            this.cbMultipleFilesEachXsd.AutoSize = true;
            this.cbMultipleFilesEachXsd.Location = new System.Drawing.Point(197, 415);
            this.cbMultipleFilesEachXsd.Name = "cbMultipleFilesEachXsd";
            this.cbMultipleFilesEachXsd.Size = new System.Drawing.Size(156, 16);
            this.cbMultipleFilesEachXsd.TabIndex = 13;
            this.cbMultipleFilesEachXsd.Text = "Separate file each xsd";
            this.cbMultipleFilesEachXsd.UseVisualStyleBackColor = true;
            this.cbMultipleFilesEachXsd.Click += new System.EventHandler(this.cbMultipleFilesEachXsd_Click);
            // 
            // cbEnableBaijiSerialization
            // 
            this.cbEnableBaijiSerialization.AutoSize = true;
            this.cbEnableBaijiSerialization.Checked = true;
            this.cbEnableBaijiSerialization.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnableBaijiSerialization.Location = new System.Drawing.Point(12, 442);
            this.cbEnableBaijiSerialization.Name = "cbEnableBaijiSerialization";
            this.cbEnableBaijiSerialization.Size = new System.Drawing.Size(138, 16);
            this.cbEnableBaijiSerialization.TabIndex = 14;
            this.cbEnableBaijiSerialization.Text = "Baiji Serialization";
            this.cbEnableBaijiSerialization.UseVisualStyleBackColor = true;
            // 
            // cbAddCustomRequestInterface
            // 
            this.cbAddCustomRequestInterface.AutoSize = true;
            this.cbAddCustomRequestInterface.Location = new System.Drawing.Point(197, 442);
            this.cbAddCustomRequestInterface.Name = "cbAddCustomRequestInterface";
            this.cbAddCustomRequestInterface.Size = new System.Drawing.Size(192, 16);
            this.cbAddCustomRequestInterface.TabIndex = 15;
            this.cbAddCustomRequestInterface.Text = "Add custom request interface";
            this.cbAddCustomRequestInterface.UseVisualStyleBackColor = true;
            // 
            // tbCustomRequestInterface
            // 
            this.tbCustomRequestInterface.Location = new System.Drawing.Point(404, 439);
            this.tbCustomRequestInterface.Name = "tbCustomRequestInterface";
            this.tbCustomRequestInterface.Size = new System.Drawing.Size(238, 21);
            this.tbCustomRequestInterface.TabIndex = 16;
            this.tbCustomRequestInterface.Visible = false;
            // 
            // WebServiceCodeGenDialogNew
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(684, 528);
            this.Controls.Add(this.cbGenerateAsyncOperations);
            this.Controls.Add(this.cbForceElementNamespace);
            this.Controls.Add(this.tbCustomRequestInterface);
            this.Controls.Add(this.cbAddCustomRequestInterface);
            this.Controls.Add(this.cbForceElementName);
            this.Controls.Add(this.cbEnableBaijiSerialization);
            this.Controls.Add(this.cbMultipleFilesEachXsd);
            this.Controls.Add(this.cbAscendingClassByName);
            this.Controls.Add(this.cbMultipleFilesEachNamespace);
            this.Controls.Add(this.cbOverwrite);
            this.Controls.Add(this.cbMultipleFiles);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cbSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebServiceCodeGenDialogNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CTrip SOA Code Generation ";
            this.Closed += new System.EventHandler(this.WebServiceCodeGenOptions_Closed);
            this.Load += new System.EventHandler(this.WebServiceCodeGenOptions_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWscf)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbWizard)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbAddCustomRequestInterface.Checked)
            {
                if (tbCustomRequestInterface.Text.Length == 0 || !CTrip.Tools.SOA.Util.ValidationHelper.IsIdentifier(tbCustomRequestInterface.Text))
                {
                    MessageBox.Show("Please enter a valid name for the interface",
                                          "CTrip SOA code generation",
                                  MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    this.DialogResult = DialogResult.None;
                    button1.DialogResult = DialogResult.None;
                    return;
                }
            }

            if (rbClient.Checked || rbServer.Checked || rbClientForTest.Checked)
            {
                if (tbDestinationFilename.Text.Length == 0 || !ValidationHelper.IsWindowsFileName(tbDestinationFilename.Text) ||
                    tbDestinationNamespace.Text.Length == 0 || !ValidationHelper.IsDotNetNamespace(tbDestinationNamespace.Text) ||
                    cbWsdlLocation.Text.Length == 0)
                {
                    this.DialogResult = DialogResult.None;
                    button1.DialogResult = DialogResult.None;

                    MessageBox.Show("Sorry, please enter valid values.",
                        "CTrip SOA code generation", MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                this.DialogResult = DialogResult.None;
                button1.DialogResult = DialogResult.None;

                MessageBox.Show("Please choose code generation options.",
                    "CTrip SOA code generation", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        private void WebServiceCodeGenOptions_Load(object sender, EventArgs e)
        {
            if (wsdlLocation.Length == 0)
            {
                cbWsdlLocation.Enabled = true;
                cbWsdlLocation.Focus();
            }

            if (!cbWsdlLocation.Enabled) bnBrowse.Enabled = false;


            LoadFormValues();

            if (rbClient.Checked || rbServer.Checked || rbClientForTest.Checked) button1.Enabled = true;
            isLoading = false;
        }

        private void ttPicBox_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.ctrip.com/");
        }

        private void bnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialogWSDL.ShowDialog() == DialogResult.OK)
            {
                AddWsdlFileToCache(openFileDialogWSDL.FileName);
            }
        }

        private void SaveFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager(wsdlFileName);
            config.Write("ClientCode", rbClient.Checked.ToString());
            config.Write("ServerCode", rbServer.Checked.ToString());
            config.Write("ClientCodeForTest", rbClientForTest.Checked.ToString());

            config.Write("Comment", cbComment.Checked.ToString());
            config.Write("DataBinding", cbDataBinding.Checked.ToString());
            config.Write("Collections", cbCollections.Checked.ToString());
            config.Write("TypedList", cbTypedList.Checked.ToString());
            config.Write("LazyLoading", cbLazyLoading.Checked.ToString());
            config.Write("OnlyUseDataContractSerializer", cbOnlyUseDataContractSerializer.Checked.ToString());
            config.Write("MultipleFiles", cbMultipleFiles.Checked.ToString());
            config.Write("MultipleFilesEachNamespace", cbMultipleFilesEachNamespace.Checked.ToString());
            config.Write("MultipleFilesEachXsd", cbMultipleFilesEachXsd.Checked.ToString());

            config.Write("DestinationFilename", tbDestinationFilename.Text);
            config.Write("DestinationNamespace", tbDestinationNamespace.Text);

            config.Write("Overwrite", cbOverwrite.Checked.ToString());
            config.Write("AscendingClassByName", cbAscendingClassByName.Checked.ToString());
            config.Write("EnableBaijiSerialization", cbEnableBaijiSerialization.Checked.ToString());
            config.Write("AddCustomRequestInterface", cbAddCustomRequestInterface.Checked.ToString());
            config.Write("CustomRequestInterface", tbCustomRequestInterface.Text);
            config.Write("ForceElementName", cbForceElementName.Checked.ToString());
            config.Write("ForceElementNamespace", cbForceElementNamespace.Checked.ToString());
            config.Write("GenerateAsyncOperations", cbGenerateAsyncOperations.Checked.ToString());
            // BDS: Modified the code to store the values pasted to the combo box.
            if (cbWsdlLocation.SelectedItem != null)
            {
                config.Write("WSDLLocation",
                    wsdlFileCache[cbWsdlLocation.SelectedIndex].ToString());
                wsdlPath = wsdlFileCache[cbWsdlLocation.SelectedIndex].ToString();
            }
            else
            {
                config.Write("WSDLLocation", cbWsdlLocation.Text);
                wsdlPath = cbWsdlLocation.Text;
            }

            config.Write("RememberSettings", cbSettings.Checked.ToString());


            string wsdlUrlsString = "";

            // Add the current item.
            if (cbWsdlLocation.SelectedItem == null)
            {
                string fname = AddWsdlFileToCache(cbWsdlLocation.Text);
            }

            foreach (string path in wsdlFileCache)
            {
                wsdlUrlsString += path + ";";
            }

            config.Write("WsdlUrls", wsdlUrlsString);
            config.Persist();
        }

        private void LoadFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager(wsdlFileName);
            string wsdlUrls = config.Read("WsdlUrls");

            //if (wsdlUrls.Length > 0)
            //{
            cbWsdlLocation.Items.Clear();
            wsdlUrls = wsdlUrls.Trim(';');
            string[] urls = wsdlUrls.Split(';');

            // BDS: Changed this code to use new wsdl file cache.
            for (int urlIndex = 0; urlIndex < urls.Length; urlIndex++)
            {
                string fname = AddWsdlFileToCache(urls[urlIndex]);
            }

            if (cbWsdlLocation.Items.Count > 0)
            {
                cbWsdlLocation.SelectedIndex = 0;
            }

            if (wsdlLocation.Length > 0)
            {
                if (!wsdlFileCache.Contains(wsdlLocation))
                {
                    string fname = AddWsdlFileToCache(wsdlLocation);
                }
                else
                {
                    int wsdlIndex = wsdlFileCache.IndexOf(wsdlLocation);
                    cbWsdlLocation.SelectedIndex = wsdlIndex;
                }
            }
            //}

            if (config.CanRead && (cbSettings.Checked = config.ReadBoolean("RememberSettings")))
            {
                rbClient.Checked = config.ReadBoolean("ClientCode");
                rbServer.Checked = config.ReadBoolean("ServerCode");
                rbClientForTest.Checked = config.ReadBoolean("ClientCodeForTest");

                cbComment.Checked = config.ReadBoolean("Serializable");
                cbDataBinding.Checked = config.ReadBoolean("DataBinding");
                cbCollections.Checked = config.ReadBoolean("Collections");
                cbTypedList.Checked = config.ReadBoolean("TypedList");
                cbLazyLoading.Checked = config.ReadBoolean("LazyLoading");
                cbOnlyUseDataContractSerializer.Checked = config.ReadBoolean("OnlyUseDataContractSerializer");
                cbMultipleFiles.Checked = config.ReadBoolean("MultipleFiles");
                cbMultipleFilesEachNamespace.Checked = config.ReadBoolean("MultipleFilesEachNamespace");
                cbMultipleFilesEachXsd.Checked = config.ReadBoolean("MultipleFilesEachXsd");

                tbDestinationFilename.Text = config.Read("DestinationFilename");
                tbDestinationNamespace.Text = config.Read("DestinationNamespace");

                cbOverwrite.Checked = config.ReadBoolean("Overwrite");
                cbAscendingClassByName.Checked = config.ReadBoolean("AscendingClassByName");
                cbEnableBaijiSerialization.Checked = config.ReadBoolean("EnableBaijiSerialization");
                cbAddCustomRequestInterface.Checked = config.ReadBoolean("AddCustomRequestInterface");
                tbCustomRequestInterface.Text = config.Read("CustomRequestInterface");
                cbForceElementName.Checked = config.ReadBoolean("ForceElementName");
                cbForceElementNamespace.Checked = config.ReadBoolean("ForceElementNamespace");
                cbGenerateAsyncOperations.Checked = config.ReadBoolean("GenerateAsyncOperations");
            }
        }

        private void ttPicBox_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Hand;
        }

        private void ttPicBox_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
        }

        private void ttPicBox_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void tbWSDLLocation_TextChanged(object sender, EventArgs e)
        {
        }

        private void WebServiceCodeGenOptions_Closed(object sender, EventArgs e)
        {
            // BDS: Save the values only if the OK button is clicked.
            if (this.DialogResult == DialogResult.OK)
            {
                SaveFormValues();
            }
        }

        private void cbWsdlLocation_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        /// <summary>
        /// Disply the location of the WSDL when moving the mouse over the combo box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void cbWsdlLocation_MouseMove(object sender, MouseEventArgs e)
        {
            if (cbWsdlLocation.SelectedIndex >= 0)
            {
                cfTooltip.SetToolTip(cbWsdlLocation,
                    wsdlFileCache[cbWsdlLocation.SelectedIndex].ToString());
            }
        }

        /// <summary>
        /// Adds a wsdl file info to the wsdl file cache.
        /// </summary>
        /// <param name="path">Path of the wsdl file to add.</param>
        /// <returns>A string indicating the name of the wsdl file.</returns>
        /// <author>William - ctrip</author>
        private string AddWsdlFileToCache(string path)
        {
            if (path.LastIndexOf("\\") > 0 && path.ToLower().EndsWith(".wsdl"))
            {
                if (wsdlFileCache.Count == 10)
                {
                    wsdlFileCache.RemoveAt(0);
                    cbWsdlLocation.Items.RemoveAt(0);
                }
                string fname = path.Substring(path.LastIndexOf("\\") + 1);
                wsdlFileCache.Add(path);
                cbWsdlLocation.SelectedIndex = cbWsdlLocation.Items.Add(fname);
                return fname;
            }

            return "";
        }

        private string GetFileNameFromPath(string path)
        {
            string fname = "";
            if (path.LastIndexOf("\\") < path.Length - 1)
            {
                fname = path.Substring(path.LastIndexOf("\\") + 1);
            }
            return fname;
        }

        private void cbOverwrite_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!isLoading)
            {
                if (cbOverwrite.Checked)
                {
                    if (MessageBox.Show(this,
                        "This will overwrite the existing files in the project. Are you sure you want to enable this option anyway?",
                        "CTrip SOA code generation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.No)
                    {
                        cbOverwrite.Checked = false;
                    }
                }
            }
        }

        private void pbWscf_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.ctrip.com/");
        }

        public bool ServiceCode
        {
            get { return rbServer.Checked; }
        }

        public bool ClientCode
        {
            get { return rbClient.Checked; }
        }

        public bool ClientCodeForTest
        {
            get { return rbClient.Checked; }
        }

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
            get { return cbTypedList.Checked; }
        }

        public bool LazyLoading
        {
            get { return cbLazyLoading.Checked; }
        }

        public string DestinationFilename
        {
            get { return tbDestinationFilename.Text; }
            set { tbDestinationFilename.Text = value; }
        }

        public string DestinationNamespace
        {
            get { return tbDestinationNamespace.Text; }
            set { tbDestinationNamespace.Text = value; }
        }

        public string WsdlLocation
        {
            get { return cbWsdlLocation.Text; }
            set 
            {
                wsdlLocation = value;
                wsdlFileName = Path.GetFileName(wsdlLocation);
            }
        }

        public bool ExternalFile
        {
            get { return externalFile; }
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

        public string WsdlPath
        {
            get { return this.wsdlPath; }
        }

        public bool Overwrite
        {
            get { return this.cbOverwrite.Checked; }
        }

        public bool EnableDataBinding
        {
            get { return this.cbDataBinding.Checked; }
        }

        public bool OnlyUseDataContractSerializer
        {
            get { return this.cbOnlyUseDataContractSerializer.Checked; }
        }

        public bool AscendingClassByName
        {
            get { return this.cbAscendingClassByName.Checked; }
        }

        public bool EnableBaijiSerialization
        {
            get { return this.cbEnableBaijiSerialization.Checked; }
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

        public string CustomRequestInterface
        {
            get { return this.tbCustomRequestInterface.Text; }
        }

        public bool AddCustomRequestInterface
        {
            get { return this.cbAddCustomRequestInterface.Checked; }
        }

        private void cbTypedList_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbTypedList.Checked)
            {
                this.cbCollections.Checked = false;
                this.cbOnlyUseDataContractSerializer.Checked = false;
            }
        }

        private void cbCollections_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbCollections.Checked)
            {
                this.cbTypedList.Checked = false;
                this.cbOnlyUseDataContractSerializer.Checked = false;
            }
        }

        private void rbClient_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;

            string extension = ".cs";
            string fileName = Path.GetFileNameWithoutExtension(this.tbDestinationFilename.Text ?? string.Empty);
            if (fileName.Length > 1 && fileName[0] == 'I' && char.IsUpper(fileName[1]))
                fileName = fileName.Substring(1);
            string lowerCaseFileName = fileName.ToLower();
            if (lowerCaseFileName.EndsWith("client"))
                this.tbDestinationFilename.Text = fileName + extension;
            else
                this.tbDestinationFilename.Text = fileName + "Client" + extension;
        }

        private void rbClientForTest_Click(object sender, EventArgs e)
        {
            rbClient_Click(sender, e);
        }

        private void rbServer_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;

            string extension = ".cs";
            string fileName = Path.GetFileNameWithoutExtension(this.tbDestinationFilename.Text ?? string.Empty).Trim();
            if (fileName.Length > 0 && fileName[0] != 'I' || fileName.Length > 1 && fileName[0] == 'I' && char.IsLower(fileName[1]))
                fileName = "I" + fileName;
            string lowerCaseFileName = fileName.ToLower();
            if (lowerCaseFileName.EndsWith("client"))
            {
                fileName = fileName.Substring(0, fileName.Length - "client".Length);
                lowerCaseFileName = fileName.ToLower();
            }
            if (lowerCaseFileName.EndsWith("service"))
                this.tbDestinationFilename.Text = fileName + extension;
            else
                this.tbDestinationFilename.Text = fileName + "Service" + extension;
        }

        private void cbMultipleFiles_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFiles.Checked)
            {
                this.cbMultipleFilesEachNamespace.Checked = false;
                this.cbMultipleFilesEachXsd.Checked = false;
            }
        }

        private void cbMultipleFilesEachNamespace_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFilesEachNamespace.Checked)
            {
                this.cbMultipleFiles.Checked = false;
                this.cbMultipleFilesEachXsd.Checked = false;
            }
        }

        private void cbMultipleFilesEachXsd_Click(object sender, EventArgs e)
        {
            if (this.cbMultipleFilesEachXsd.Checked)
            {
                this.cbMultipleFiles.Checked = false;
                this.cbMultipleFilesEachNamespace.Checked = false;
            }
        }

        private void cbOnlyUseDataContractSerializer_Click(object sender, EventArgs e)
        {
            cbLazyLoading.Checked = false;
            cbDataBinding.Checked = false;
            cbCollections.Checked = false;
            cbTypedList.Checked = false;
            cbComment.Checked = false;
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
