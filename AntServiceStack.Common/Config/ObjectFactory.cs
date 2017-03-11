//-----------------------------------------------------------------------
// <copyright file="ObjectFactory.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------
namespace AntServiceStack.Common.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class ObjectFactory
    {


        public static IConfiguration CreateAppSettingConfiguration()
        {
            return (IConfiguration)new AppSettingsConfiguration();
        }
    }
}