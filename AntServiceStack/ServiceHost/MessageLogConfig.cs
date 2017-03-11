using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Message;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Configuration;
using AntServiceStack.WebHost.Endpoints;

namespace AntServiceStack.ServiceHost
{
    internal class MessageLogConfig
    {
        public const long DefalutRequestLogMaxSize = 1024L;
        public const long DefalutResponseLogMaxSize = 1024L;

        public static long RequestLogMaxSize { get; private set; }
        public static long ResponseLogMaxSize { get; private set; }

        public static MessageLogConfig FrameworkDefalutMessageLogConfigOfTestEnv { get; private set; }
        public static MessageLogConfig FrameworkDefalutMessageLogConfigOfUatEnv { get; private set; }
        public static MessageLogConfig FrameworkDefalutMessageLogConfigOfProdEnv { get; private set; }
        public static MessageLogConfig FrameworkDefalutMessageLogConfigOfNullEnv { get; private set; }

        public static MessageLogConfig CurrentFrameworkDefaultMessageLogConfig { get; private set; }

        public bool? IsRequestSensitive { get; set; }

        public bool? IsResponseSensitive { get; set; }

        public bool? LogRequest { get; set; }

        public bool? LogResponse { get; set; }

        public bool? DisableLog { get; set; }

        static MessageLogConfig()
        {
            RequestLogMaxSize = DefalutRequestLogMaxSize;
            ResponseLogMaxSize = DefalutResponseLogMaxSize;

            FrameworkDefalutMessageLogConfigOfTestEnv = new MessageLogConfig()
            {
                IsRequestSensitive = false,
                IsResponseSensitive = false,
                LogRequest = true,
                LogResponse = false,
                DisableLog = false,
            };

            FrameworkDefalutMessageLogConfigOfUatEnv = new MessageLogConfig()
            {
                IsRequestSensitive = false,
                IsResponseSensitive = false,
                LogRequest = true,
                LogResponse = false,
                DisableLog = false,
            };

            FrameworkDefalutMessageLogConfigOfProdEnv = new MessageLogConfig()
            {
                IsRequestSensitive = true,
                IsResponseSensitive = true,
                LogRequest = true,
                LogResponse = false,
                DisableLog = false,
            };

            FrameworkDefalutMessageLogConfigOfNullEnv = new MessageLogConfig() 
            {
                IsRequestSensitive = true,
                IsResponseSensitive = true,
                LogRequest = false,
                LogResponse = false,
                DisableLog = true,
            };

            switch (EnvironmentUtility.CurrentEnv)
            {
                case EnvironmentUtility.TestEnv:
                    CurrentFrameworkDefaultMessageLogConfig = FrameworkDefalutMessageLogConfigOfTestEnv;
                    break;
                case EnvironmentUtility.UatEnv:
                    CurrentFrameworkDefaultMessageLogConfig = FrameworkDefalutMessageLogConfigOfUatEnv;
                    break;
                case EnvironmentUtility.ProdEnv:
                    CurrentFrameworkDefaultMessageLogConfig = FrameworkDefalutMessageLogConfigOfProdEnv;
                    break;
                default:
                    CurrentFrameworkDefaultMessageLogConfig = FrameworkDefalutMessageLogConfigOfNullEnv;
                    break;
            }
        }
    }
}
