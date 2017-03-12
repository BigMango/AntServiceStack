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
using Ant.Tools.SOA.WsdlWizard;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
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

        private void WsdlCommandInvoke(object sender, EventArgs e)
        {
            var items = ProjectHelpers.GetSelectedItemPaths(_dte);
            if (items.Count() == 1 &&
                (items.ElementAt(0).ToLower().EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase)))
            {
               var _file = items.ElementAt(0);
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

       
    }
}