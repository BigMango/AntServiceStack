//-----------------------------------------------------------------------
// <copyright file="AddIntellisenseFileMenu.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using Ant.Tools.SOA.CodeGeneration;
using Ant.Tools.SOA.CodeGeneration.CodeWriter;
using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.WsdlWizard;
using CTrip.Tools.SOA.ContractFirst;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VsWebSite;
using VSIXWsdlWizard.Common;

namespace VSIXWsdlWizard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    /// <summary>
    /// 
    /// </summary>
    public class AddIntellisenseFileMenu
    {
        private DTE2 _dte;
        private OleMenuCommandService _mcs;
        private Func<string, bool> _itemToHandleFunc;
        public AddIntellisenseFileMenu(DTE2 dte, OleMenuCommandService mcs,Func<string,bool> itemToHandleFunc)
        {
            _dte = dte;
            _mcs = mcs;
            _itemToHandleFunc = itemToHandleFunc;
        }
        public void SetupCommands()
        {
            CommandID JsId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.CreateJavaScriptIntellisenseFile);
            OleMenuCommand jsCommand = new OleMenuCommand(XsdCommandInvoke, JsId);
            jsCommand.BeforeQueryStatus += JavaScript_BeforeQueryStatus;
            _mcs.AddCommand(jsCommand);

            CommandID tsId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.CreateTypeScriptIntellisenseFile);
            OleMenuCommand tsCommand = new OleMenuCommand(WsdlCommandInvoke, tsId);
            tsCommand.BeforeQueryStatus += TypeScript_BeforeQueryStatus;
            _mcs.AddCommand(tsCommand);
        }

        /// <summary>
        /// xsd生成wsdl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XsdCommandInvoke(object sender, EventArgs e)
        {
            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items.Count() == 1 &&
                (items.ElementAt(0).ToLower().EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)))
            {
                var _file = items.ElementAt(0);
                var fileInfo = new FileInfo(_file);
                var folder = fileInfo.Directory?.FullName;
                var project = ProjectHelpers.GetActiveProject().FullName;
                var projectInfo = new FileInfo(project);
                var folderproject = projectInfo.Directory?.FullName;
                if (!string.IsNullOrEmpty(folder))
                {
                    WsdlWizardForm wizard = null;
                    try
                    {
                      
                        wizard = new WsdlWizardForm(_file);
                        wizard.WsdlLocation = folder;
                        wizard.DefaultPathForImports = "";
                        wizard.ProjectRootDirectory = folderproject;
                        wizard.ShowDialog();

                        string wsdlFile = "";
                        if (wizard.DialogResult == DialogResult.OK)
                        {
                            if (wizard.WsdlLocation.Length > 0)
                            {
                                wsdlFile = wizard.WsdlLocation;
                                ProjectHelpers.AddFileToActiveProject(wsdlFile);
                                //ProcessCodeGenerationRequest(wsdlFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message,"WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        if (wizard != null) wizard.Close();
                    }
                }
            }

        }

        /// <summary>
        /// wsdl生成cs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WsdlCommandInvoke(object sender, EventArgs e)
        {
            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items.Count() != 1 ||
                !(items.ElementAt(0).ToLower().EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            var _file = items.ElementAt(0);
            var project = ProjectHelpers.GetActiveProject();
            var projectFullName = ProjectHelpers.GetActiveProject().FullName;
            var projectInfo = new FileInfo(projectFullName);
            var folderproject = projectInfo.Directory?.FullName;
            try
            {
                // Fist display the UI and get the options.
                WebServiceCodeGenDialogNew dialog = new WebServiceCodeGenDialogNew();
                if (!project.IsWebProject())
                {
                    dialog.DestinationNamespace = project.GetProjectProperty("DefaultNamespace");
                }
                dialog.DestinationFilename = ProjectHelpers.GetDefaultDestinationFilename(_file);
                dialog.WsdlLocation = _file;
               
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return ;
                }
                var wsdlFile = _file;
                // Try the Rpc2DocumentLiteral translation first.
                // wsdlFile = TryTranslateRpc2DocumentLiteral(wsdlFile);

                CodeGenOptions options = new CodeGenOptions();
                options.MetadataLocation = wsdlFile;
                options.OutputFileName = dialog.DestinationFilename;
                options.OutputLocation = folderproject;
                options.ProjectDirectory = project.FullName ;
                options.Language = CodeLanguage.CSharp;
                options.ProjectName = project.Name;

                options.EnableDataBinding = dialog.EnableDataBinding;
                options.EnableSummaryComment = dialog.Comment;
                options.GenerateCollections = dialog.Collections;
                options.GenerateTypedLists = dialog.TypedList;
                options.EnableLazyLoading = dialog.LazyLoading;
                options.GenerateSeparateFiles = dialog.GenerateMultipleFiles;
                options.OnlyUseDataContractSerializer = dialog.OnlyUseDataContractSerializer;
                options.GenerateSeparateFilesEachNamespace = dialog.GenerateMultipleFilesEachNamespace;
                options.GenerateSeparateFilesEachXsd = dialog.GenerateSeparateFilesEachXsd;
                options.AscendingClassByName = dialog.AscendingClassByName;
                options.OverwriteExistingFiles = dialog.Overwrite;
                options.ClrNamespace = dialog.DestinationNamespace;
                options.EnableBaijiSerialization = dialog.EnableBaijiSerialization;
                options.AddCustomRequestInterface = dialog.AddCustomRequestInterface;
                options.CustomRequestInterface = dialog.CustomRequestInterface;
                options.ForceElementName = dialog.ForceElementName;
                options.ForceElementNamespace = dialog.ForceElementNamespace;
                options.GenerateAsyncOperations = dialog.GenerateAsyncOperations;

                if (dialog.ServiceCode)
                    options.CodeGeneratorMode = CodeGeneratorMode.Service;
                else if (dialog.ClientCode)
                    options.CodeGeneratorMode = CodeGeneratorMode.Client;
                else
                    options.CodeGeneratorMode = CodeGeneratorMode.ClientForTest;

                options.EnableInitializeFields = true;


                CodeGenerator codeGenerator = new CodeGenerator();
                CodeWriterOutput output = codeGenerator.GenerateCode(options);

                AddGeneratedFilesToProject(output);

                // Finally add the project references.
                ProjectHelpers.AddAssemblyReferences(project);

                MessageBox.Show("Code generation successfully completed.", "Ant.SOA.CodeGen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
                MessageBox.Show(ex.Message, "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void JavaScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc(".xsd");
        }

        void TypeScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc(".wsdl");
        }

        private void AddGeneratedFilesToProject(CodeWriterOutput output)
        {
            foreach (string file in output.CodeFileNames)
            {
                ProjectHelpers.AddFileToActiveProject(file);
            }
        }

        
    }
}