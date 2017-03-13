//-----------------------------------------------------------------------
// <copyright file="SwaggerUiConfig.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Globalization;
using AntServiceStack.Common.Configuration;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStackSwagger.SwaggerUi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// SwaggerUiConfig配置
    /// </summary>
    public class SwaggerUiConfig
    {
        private readonly Dictionary<string, EmbeddedAssetDescriptor> _pathToAssetMap;
        private readonly Dictionary<string, string> _templateParams;

        public EndpointHostConfig HostConfig { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Api版本
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// 是否注入Js
        /// </summary>
        public bool InjectJs  {
            get
            {
                return UseBasicAuth;
            }
        }


        /// <summary>
        /// 是否启动BasicAuth
        /// </summary>
        public bool UseBasicAuth { get; set; }




        /// <summary>
        /// 获取登录态地址
        /// </summary>
        public string H5LoginUrl { get; set; }
        public SwaggerUiConfig(EndpointHostConfig endpointHostConfig, string title = null, string apiVersion = null, bool useBasicAuth = false):this()
        {
            HostConfig = endpointHostConfig;
            Title = title;
            ApiVersion = apiVersion;
            UseBasicAuth = useBasicAuth;
        }

        public SwaggerUiConfig()
        {
            _pathToAssetMap = new Dictionary<string, EmbeddedAssetDescriptor>();
            _templateParams = new Dictionary<string, string>{};
            MapPathsForSwaggerUiAssets();
        }
        internal IAssetProvider GetSwaggerUiProvider()
        {
            return new EmbeddedAssetProvider(_pathToAssetMap, _templateParams);
        }

        private void MapPathsForSwaggerUiAssets()
        {
            var thisAssembly = GetType().Assembly;
            foreach (var resourceName in thisAssembly.GetManifestResourceNames())
            {
                if (resourceName.Contains("CServiceStackSwagger.SwaggerUi.CustomAssets")) continue; // original assets only

                var path = resourceName
                    .Replace("\\", "/");
                    //.Replace(".", "-"); // extensionless to avoid RUMMFAR

                _pathToAssetMap[path] = new EmbeddedAssetDescriptor(thisAssembly, resourceName,false);
            }
        }


        public BasicAuthModel GetLocalAuthModel()
        {
            try
            {

                var userName = ConfigUtils.GetAppSetting("swagger-userName");
                var password = ConfigUtils.GetAppSetting("swagger-userName");
                return new BasicAuthModel
                {
                    UserName = userName,
                    Password = password
                };
            }
            catch (Exception ex)
            {
                return new BasicAuthModel
                {
                    UserName = "admin",
                    Password = "nimda"
                };
            }
        }
        public class BasicAuthModel
        {
            public string UserName { get; set; }

            public string Password { get; set; }
        }
    }
}