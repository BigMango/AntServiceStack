using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AntServiceStack.Common.Configuration;
using System.IO;
using Freeway.Logging;

namespace AntServiceStack.Common.Utils
{
    internal class EnvUtils
    {
        private static ILog log = LogManager.GetLogger(typeof(EnvUtils));

        private const int FAT_INDEX = 1000000;
        private const int LPT_INDEX = 2000000;
        private const int DEV_INDEX = 4000000;
        private const int UNKNOWN_INDEX = 3000000;

        internal static int GetSubEnvIndex(string subEnv)
        {
            if (string.IsNullOrWhiteSpace(subEnv))
                return UNKNOWN_INDEX;

            subEnv = subEnv.Trim().ToLower();

            switch (subEnv)
            {
                case "fws": return 0;
                case "fat": return FAT_INDEX;
                case "lpt10": return LPT_INDEX - 1;
                case "lpt": return LPT_INDEX;
                case "dev": return DEV_INDEX;
            }

            int startIndex;
            if (subEnv.StartsWith("fat"))
                startIndex = FAT_INDEX;
            else if (subEnv.StartsWith("lpt"))
                startIndex = LPT_INDEX;
            else
                return UNKNOWN_INDEX;

            int index;
            if (int.TryParse(subEnv.Substring(3), out index))
                return index > 0 ? startIndex + index : UNKNOWN_INDEX;
            else
                return UNKNOWN_INDEX;
        }

        internal static bool IsTestEnvironment(string env)
        {
            if (string.IsNullOrWhiteSpace(env))
                return false;
            if (env == "prod" || env == "prd" || env == "pro" || env.StartsWith("uat"))
                return false;
            return true;
        }
    }
}
