//-----------------------------------------------------------------------
// <copyright file="AddIntellisenseFileMenu.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

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
        private string _file;
        private Func<bool> _itemToHandleFunc;
        public AddIntellisenseFileMenu(DTE2 dte, OleMenuCommandService mcs,Func<bool> itemToHandleFunc)
        {
            _dte = dte;
            _mcs = mcs;
            _itemToHandleFunc = itemToHandleFunc;
        }
        public void SetupCommands()
        {
            CommandID JsId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.CreateJavaScriptIntellisenseFile);
            OleMenuCommand jsCommand = new OleMenuCommand(CommandInvoke, JsId);
            jsCommand.BeforeQueryStatus += JavaScript_BeforeQueryStatus;
            _mcs.AddCommand(jsCommand);

            CommandID tsId = new CommandID(CommandGuids.guidDiffCmdSet, (int)CommandId.CreateTypeScriptIntellisenseFile);
            OleMenuCommand tsCommand = new OleMenuCommand(CommandInvoke, tsId);
            tsCommand.BeforeQueryStatus += TypeScript_BeforeQueryStatus;
            _mcs.AddCommand(tsCommand);
        }

        private void CommandInvoke(object sender, EventArgs e)
        {

        }

        void JavaScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc();
        }

        void TypeScript_BeforeQueryStatus(object sender, System.EventArgs e)
        {
            OleMenuCommand oCommand = (OleMenuCommand)sender;

            oCommand.Visible = _itemToHandleFunc();
        }

       
    }
}