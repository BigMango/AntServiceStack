//-----------------------------------------------------------------------
// <copyright file="AddIntellisenseFileMenu.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ant.Tools.SOA.CodeGeneration;
using Ant.Tools.SOA.CodeGeneration.CodeWriter;
using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.ContractFirst;
using Ant.Tools.SOA.WsdlWizard;
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
        private readonly DTE2 _dte;
        private readonly OleMenuCommandService _mcs;
        private readonly Func<string[], bool> _itemToHandleFunc;
        private Package _package;
        public AddIntellisenseFileMenu(DTE2 dte, OleMenuCommandService mcs,Func<string[],bool> itemToHandleFunc, Package package)
        {
            _dte = dte;
            _mcs = mcs;
            _itemToHandleFunc = itemToHandleFunc;
            _package = package;
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

            CommandID csId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.cmdModelIntellisense);
            OleMenuCommand csCommand = new OleMenuCommand(ModelCommandInvoke, csId);
            csCommand.BeforeQueryStatus += JavaScript_BeforeQueryStatus;
            _mcs.AddCommand(csCommand);

            CommandID utf8Id = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.cmdXsdUtf8Intellisense);
            OleMenuCommand utf8Command = new OleMenuCommand(Utf8CommandInvoke, utf8Id);
            utf8Command.BeforeQueryStatus += JavaScript_BeforeQueryStatus;
            _mcs.AddCommand(utf8Command);

            CommandID csDllId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.cmdClientDll);
            OleMenuCommand csDllIdCommand = new OleMenuCommand(CsDllCommandInvoke, csDllId);
            csDllIdCommand.BeforeQueryStatus += CS_BeforeQueryStatus;
            _mcs.AddCommand(csDllIdCommand);
        }

        /// <summary>
        /// css生成dll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsDllCommandInvoke(object sender, EventArgs e)
        {
            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items.Count() != 1)
            {
                ProjectHelpers.AddError(_package, "no cs was selected");
                return;
            }
            try
            {
                //System.IServiceProvider oServiceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
                //EnvDTE.ProjectItem oProjectItem = _dte.SelectedItems.Item(1).ProjectItem;
                //Microsoft.Build.Evaluation.Project oBuildProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(oProjectItem.ContainingProject.FullName).SingleOrDefault();
                //Microsoft.Build.Evaluation.ProjectProperty oGUID = oBuildProject.AllEvaluatedProperties.SingleOrDefault(oProperty => oProperty.Name == "ProjectGuid");
                //Microsoft.VisualStudio.Shell.Interop.IVsHierarchy oVsHierarchy = VsShellUtilities.GetHierarchy(oServiceProvider, new Guid(oGUID.EvaluatedValue));
                //Microsoft.VisualStudio.Shell.Interop.IVsBuildPropertyStorage oVsBuildPropertyStorage = (Microsoft.VisualStudio.Shell.Interop.IVsBuildPropertyStorage)oVsHierarchy;

                //string szItemPath = (string)oProjectItem.Properties.Item("FullPath").Value;
                string szItemPath = ProjectHelpers.GetActiveProject().FullName;
                var refDll = new List<string>();
                var csprojText = File.ReadAllLines(szItemPath).Where(r=>r.Contains(@"<ProjectReference Include=") 
                || (r.Contains("<HintPath>")));
                var serviceStack = csprojText.Where(r => r.Contains("AntServiceStack"));
                foreach (var s in serviceStack)
                {
                    string folder = new FileInfo(szItemPath).DirectoryName;
                    if (s.Trim().EndsWith(".csproj\">"))
                    {
                        var s1 = s.Split('"')[1];
                        var s2 = s1.Split('\\');
                        var dotCount = s2.Count(r => r.Equals(".."));
                        for (int i = 0; i < dotCount; i++)
                        {
                            folder = Directory.GetParent(folder).FullName;
                        }

                        for (int i = 0; i < s2.Length - 1; i++)
                        {
                            var ss = s2[i];
                            if (ss.Equals(".."))
                            {
                                continue;
                            }
                            folder = Path.Combine(folder, ss);
                        }
                        folder = Path.Combine(folder, "bin", "Debug");
                        if (!Directory.Exists(folder))
                        {
                            ProjectHelpers.AddError(_package, folder + " not found");
                            return;
                        }
                        var dllname = s2[s2.Length - 1].Replace(".csproj", ".dll");
                        var dllPath = Path.Combine(folder, dllname);
                        if (!File.Exists(dllPath))
                        {
                            ProjectHelpers.AddError(_package, dllPath + " not found");
                            return;
                        }
                        refDll.Add(dllPath);
                    }
                    else if (s.Trim().Contains("<HintPath>") && s.Trim().Contains("AntServiceStack") && s.Trim().Contains("dll"))
                    {
                        var dllPath = s.Trim().Split('>')[1].Split('<')[0];
                        refDll.Add(dllPath);
                    }

                }

                //uint nItemId;
                //oVsHierarchy.ParseCanonicalName(szItemPath, out nItemId);

                //string szOut;
                //oVsBuildPropertyStorage.GetItemAttribute(nItemId, "ClientDllVersion", out szOut);
                //if (string.IsNullOrEmpty(szOut))
                //{
                //    //oVsBuildPropertyStorage.SetItemAttribute(nItemId, "ItemColor", );
                //}


                string file = items.ElementAt(0);
                string baseFileName = Path.GetFileNameWithoutExtension(file);
                FileInfo fileInfo = new FileInfo(file);
                var frameworkPath = RuntimeEnvironment.GetRuntimeDirectory();

                var cscPath = Path.Combine(frameworkPath, "csc.exe");
                //C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\csc.exe /t:library /out:MyCScode.dll *.cs /debug /r:System.dll /r:System.Web.dll /r:System.Data.dll /r:System.Xml.dll
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = fileInfo.DirectoryName;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = cscPath;
                startInfo.Arguments = "/t:library /out:" + baseFileName + ".dll " + baseFileName + ".cs /r:System.dll /r:System.Configuration.dll /r:System.Core.dll /r:System.Runtime.Serialization.dll /r:System.ServiceModel.dll /r:System.Xml.Linq.dll /r:System.Data.DataSetExtensions.dll /r:Microsoft.CSharp.dll /r:System.Data.dll /r:System.Xml.dll" + (refDll.Count > 0 ? " /r:" +  string.Join(" /r:", refDll) : "");
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit(10000);// 10 seconds timeout
                int result = process.ExitCode;
                if (result == -1)
                {
                    ProjectHelpers.AddError(_package,"cs compile err :" + startInfo.Arguments);
                    MessageBox.Show("Error happens" ,
                        "Cs Dll Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                MessageBox.Show("compile is success",
                        "Cs Dll Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = fileInfo.DirectoryName,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {

                ProjectHelpers.AddError(_package, ex.ToString());
                MessageBox.Show("Error happens: " + ex,
                        "Cs Dll Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
            }

        }


        /// <summary>
        /// xsd转成utf8编码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Utf8CommandInvoke(object sender, EventArgs e)
        {
            

            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items.Count() != 1 ||
                !(items.ElementAt(0).ToLower().EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)))
            {
                ProjectHelpers.AddError(_package,"no xsd was selected");
                return;
            }
            string xsdFileFullPath = items.ElementAt(0);
           
            if (!File.Exists(xsdFileFullPath))
            {
                MessageBox.Show("Selected item is no longer existing.",
                        "XSD Encoding Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                return;
            }

            FileInfo fileInfo = new FileInfo(xsdFileFullPath);
            if (fileInfo.IsReadOnly)
            {
                MessageBox.Show("Selected item is readonly.",
                        "XSD Encoding Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                return;
            }
            string xsdFile = fileInfo.Name;
            try
            {
                string contentOfUTF8 = string.Empty;
                using (StreamReader reader = new StreamReader(xsdFileFullPath))
                {
                    byte[] fileContent = reader.CurrentEncoding.GetBytes(reader.ReadToEnd());
                    fileContent = Encoding.Convert(reader.CurrentEncoding, Encoding.UTF8, fileContent);
                    contentOfUTF8 = Encoding.UTF8.GetString(fileContent);
                }
                using (StreamWriter writer = new StreamWriter(xsdFile, false, Encoding.UTF8))
                {
                    writer.Write(contentOfUTF8);
                }

                MessageBox.Show("Selected item is converted to UTF-8 format.",
                        "XSD Encoding Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ProjectHelpers.AddError(_package,ex.ToString());
                MessageBox.Show("Error happens: " + ex,
                        "XSD Encoding Conversion", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        /// 根据xsd生成model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModelCommandInvoke(object sender, EventArgs e)
        {
            //可以选多个
            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items!=null && items.Any(r=>!r.ToLower().EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)))
            {
                ProjectHelpers.AddError(_package,"select file is not xsd file");
                return;
            }
           
            var _file = items.ElementAt(0);
            var project = ProjectHelpers.GetActiveProject();
            var projectFullName = ProjectHelpers.GetActiveProject().FullName;
            var projectInfo = new FileInfo(projectFullName);
            var folderproject = projectInfo.Directory?.FullName;
            try
            {
                string[] dataContractFiles = items.ToArray();
                XsdCodeGenDialog dialogForm = new XsdCodeGenDialog(dataContractFiles);
                if (!project.IsWebProject())
                {
                    dialogForm.Namespace = project.GetProjectProperty("DefaultNamespace");
                }
                dialogForm.TargetFileName = ProjectHelpers.GetDefaultDestinationFilename(_file);
                if (dialogForm.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }

                CodeGenOptions options = new CodeGenOptions();
                options.GenerateDataContractsOnly = true;
                options.DataContractFiles = dataContractFiles;
                options.GenerateSeparateFiles = dialogForm.GenerateMultipleFiles;
                options.GenerateSeparateFilesEachNamespace = dialogForm.GenerateMultipleFilesEachNamespace;
                options.GenerateSeparateFilesEachXsd = dialogForm.GenerateSeparateFilesEachXsd;
                options.AscendingClassByName = dialogForm.AscendingClassByName;
                options.OnlyUseDataContractSerializer = dialogForm.OnlyUseDataContractSerializer;
                options.OverwriteExistingFiles = dialogForm.OverwriteFiles;
                options.EnableDataBinding = dialogForm.DataBinding;
                options.EnableSummaryComment = dialogForm.Comment;
                options.GenerateCollections = dialogForm.Collections;
                options.GenerateTypedLists = dialogForm.TypedList;
                options.EnableLazyLoading = dialogForm.LazyLoading;
                options.OutputFileName = dialogForm.TargetFileName;
                options.OutputLocation = folderproject;
                options.ProjectDirectory = project.FullName;
                options.Language = CodeLanguage.CSharp;
                options.ClrNamespace = dialogForm.Namespace;
                options.EnableSummaryComment = dialogForm.Comment;
                options.ProjectName = project.Name;
                options.EnableBaijiSerialization = dialogForm.EnableBaijiSerialization;
                options.AddCustomRequestInterface = dialogForm.AddCustomRequestInterface;
                options.CustomRequestInterface = dialogForm.CustomRequestInterface;
                options.ForceElementName = dialogForm.ForceElementName;
                options.ForceElementNamespace = dialogForm.ForceElementNamespace;
                options.GenerateAsyncOperations = dialogForm.GenerateAsyncOperations;
                options.EnableInitializeFields = true;

               


                CodeGenerator codeGenerator = new CodeGenerator();
                CodeWriterOutput output = codeGenerator.GenerateCode(options);

                AddGeneratedFilesToProject(output);

                // Finally add the project references.
                ProjectHelpers.AddAssemblyReferences(project);

                MessageBox.Show("Model generation successfully completed.", "Ant.SOA.CodeGen", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ProjectHelpers.AddError(_package, ex.ToString());
                MessageBox.Show(ex.Message, "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                        ProjectHelpers.AddError(_package, ex.ToString());
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
                ProjectHelpers.AddError(_package, ex.ToString());
                MessageBox.Show(ex.Message, "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        void CS_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc(new []{"apiclient.cs","serviceclient.cs"});
        }
        void JavaScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc(new[] { ".xsd"});
        }

        void TypeScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc(new[] { ".wsdl"});
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