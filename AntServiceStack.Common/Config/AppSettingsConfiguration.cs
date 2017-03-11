//-----------------------------------------------------------------------
// <copyright file="AppSettingsConfiguration.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Configuration;

namespace AntServiceStack.Common.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class AppSettingsConfiguration : IConfiguration
    {
        public string this[string key]
        {
            get
            {
                return this.GetPropertyValue(key);
            }
        }

        public string GetPropertyValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}