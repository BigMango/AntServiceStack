//-----------------------------------------------------------------------
// <copyright file="EnumClass.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.ComponentModel;

namespace AntServiceStack.DbModel.Mysql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public enum NodeTypeEnum
    {
        [Description("自注册")]
        SelfRegister = 0,
        [Description("Consul")]
        Consul = 1
    }
}