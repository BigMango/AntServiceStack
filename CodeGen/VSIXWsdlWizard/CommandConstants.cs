//-----------------------------------------------------------------------
// <copyright file="CommandConstants.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

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
    public class CommandGuids
    {
        public const string guidDiffCmdSetString = "e396b698-e00e-444b-9f5f-3dcb1ef74e69";


        public static readonly Guid guidDiffCmdSet = new Guid(guidDiffCmdSetString);


    }


    enum CommandId
    {
        CreateJavaScriptIntellisenseFile = 0x1047,
        CreateTypeScriptIntellisenseFile = 0x1048,
        cmdModelIntellisense = 0x1049,
        cmdXsdUtf8Intellisense = 0x1050,
        cmdClientDll = 0x1051,
       
    }
}