//-----------------------------------------------------------------------
// <copyright file="EnvironmentUtility.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Configuration;
using System.IO;
using System.Xml;
using Freeway.Logging;

namespace AntServiceStack.Common.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class EnvironmentUtility
    {
        private static ILog log = LogManager.GetLogger(typeof(EnvironmentUtility));
        private static readonly string ReleaseInfoPathName = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\ReleaseInfo.config.info";
        public const string ProdEnv = "prod";
        public const string UatEnv = "uat";
        public const string TestEnv = "test";
        private const string FwsEnv = "fws";
        private const string ReleaseInfoFileName = "ReleaseInfo.config.info";
        private const string EnvTypeNodeXPath = "ReleaseInfo/Target/EnvType";
        private const string ZoneIdNodeXPath = "ReleaseInfo/Target/DC";
        private const string CLoggingServerSettingKey = "LoggingServer.V2.IP";
        private const string ServerPropertiesFile = "C:\\opt\\settings\\server.properties";
        private const string ZoneIdPropertiesKey = "idc";
        private static Dictionary<string, string> EnvToZoneIdMapping;

        public static string CurrentEnv { get; private set; }

        public static bool IsTestEnv { get; private set; }

        public static bool IsUatEnv { get; private set; }

        public static bool IsProdEnv { get; private set; }

        public static string EnvType { get; private set; }

        public static string SubEnvType { get; private set; }

        public static string RegionId { get; private set; }

        public static string ZoneId { get; private set; }

        static EnvironmentUtility()
        {
            try
            {
                EnvironmentUtility.EnvToZoneIdMapping = new Dictionary<string, string>();
                EnvironmentUtility.EnvToZoneIdMapping["test"] = "NTGXH";
                EnvironmentUtility.EnvToZoneIdMapping["uat"] = "NTGXH";
                EnvironmentUtility.EnvToZoneIdMapping["prod"] = "SHAJQ";
                EnvironmentUtility.EnvType = EnvironmentUtility.ReadEnvFromFoundation();
                EnvironmentUtility.SubEnvType = EnvironmentUtility.ReadSubEnvFromFoundation();
                EnvironmentUtility.CurrentEnv = EnvironmentUtility.GetEnvType(EnvironmentUtility.SubEnvType ?? EnvironmentUtility.EnvType) ?? EnvironmentUtility.ReadEnvTypeFromReleaseInfo() ?? EnvironmentUtility.ReadEnvTypeFromCLoggingSetting() ?? "test";
                EnvironmentUtility.IsTestEnv = EnvironmentUtility.CurrentEnv == "test";
                EnvironmentUtility.IsUatEnv = EnvironmentUtility.CurrentEnv == "uat";
                EnvironmentUtility.IsProdEnv = EnvironmentUtility.CurrentEnv == "prod";
                if (EnvironmentUtility.IsUatEnv || EnvironmentUtility.IsProdEnv)
                    EnvironmentUtility.SubEnvType = EnvironmentUtility.EnvType;
                EnvironmentUtility.RegionId = "SHA";
                if (!string.IsNullOrWhiteSpace(EnvironmentUtility.ZoneId))
                    return;
                EnvironmentUtility.ZoneId = EnvironmentUtility.EnvToZoneIdMapping[EnvironmentUtility.CurrentEnv];
                string message = string.Format("Missing {0} property in {1} file, using default value: {2}", (object)"idc", (object)"C:\\opt\\settings\\server.properties", (object)EnvironmentUtility.ZoneId);
                EnvironmentUtility.log.Warn(message);
            }
            catch (Exception ex)
            {
                EnvironmentUtility.log.Error("Error occurred while initializing EnvUtils.", ex);
            }
        }

        private static string GetEnvType(string env)
        {
            if (string.IsNullOrWhiteSpace(env))
                return (string)null;
            env = env.Trim().ToLower();
            if (env == "prod" || env == "pro" || env == "prd")
                return "prod";
            if (env.StartsWith("uat"))
                return "uat";
            if (env == "fws" || env == "dev" || (env.StartsWith("fat") || env.StartsWith("lpt")))
                return "test";
            return (string)null;
        }

        private static string ReadEnvTypeFromReleaseInfo()
        {
            try
            {
                if (File.Exists(EnvironmentUtility.ReleaseInfoPathName))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(EnvironmentUtility.ReleaseInfoPathName);
                    XmlNode xmlNode = xmlDocument.SelectSingleNode("ReleaseInfo/Target/EnvType");
                    if (xmlNode == null || xmlNode.InnerText == null)
                        return (string)null;
                    return EnvironmentUtility.GetEnvType(xmlNode.InnerText);
                }
            }
            catch (Exception ex)
            {
                EnvironmentUtility.log.Warn("Failed to read environment from 'ReleaseInfo' file!", ex, new Dictionary<string, string>()
        {
          {
            "ErrorCode",
            "FXD300029"
          }
        });
            }
            return (string)null;
        }

        private static string ReadEnvTypeFromCLoggingSetting()
        {
            try
            {
                string str = ConfigurationManager.AppSettings["LoggingServer.V2.IP"];
                if (string.IsNullOrWhiteSpace(str))
                    return (string)null;
                string lower = str.ToLower();
                if (lower.Contains(".uat."))
                    return "uat";
                return lower.Contains(".fws.") || lower.Contains(".lpt.") || (lower.Contains(".fat.") || lower.Contains(".dev.")) ? "test" : "prod";
            }
            catch (Exception ex)
            {
                EnvironmentUtility.log.Warn("Failed to read environment from 'LoggingServer.V2.IP' setting!", ex, new Dictionary<string, string>()
        {
          {
            "ErrorCode",
            "FXD300030"
          }
        });
            }
            return (string)null;
        }

        private static string ReadEnvFromFoundation()
        {
            try
            {
                return "dev";
            }
            catch (Exception ex)
            {
                EnvironmentUtility.log.Warn("Error occurred while read env from foundation", ex);
                return (string)null;
            }
        }

        private static string ReadSubEnvFromFoundation()
        {
            try
            {
                return "fws";
            }
            catch (Exception ex)
            {
                EnvironmentUtility.log.Warn("Error occurred while read sub env from foundation", ex);
                return (string)null;
            }
        }
    }
}