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
        public static IConfigurationManager CreateDefaultConfigurationManager(params IConfigurationSource[] sources)
        {
            return (IConfigurationManager)new DefaultConfigurationManager((IEnumerable<IConfigurationSource>)sources);
        }

        public static IConfigurationSource CreateDefaultConfigurationSource(int priority, string sourceId, IConfiguration configuration)
        {
            return (IConfigurationSource)new DefaultConfigurationSource(priority, sourceId, configuration);
        }

        public static IDynamicConfigurationSource CreateDefaultDynamicConfigurationSource(int priority, string sourceId, IDynamicConfiguration dynamicConfiguration)
        {
            return (IDynamicConfigurationSource)new DefaultDynamicConfigurationSource(priority, sourceId, dynamicConfiguration);
        }

        public static IConfiguration CreateAppSettingConfiguration()
        {
            return (IConfiguration)new AppSettingsConfiguration();
        }
    }
}