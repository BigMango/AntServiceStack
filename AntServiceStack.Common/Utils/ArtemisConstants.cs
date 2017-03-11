using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    class ArtemisConstants
    {
        public const string RibbonUrlKey = "url";
        public const string RibbonHealthCheckUrlKey = "healthCheckUrl";
        public const string ArtemisSubEnvKey = "subenv";

        public const string MetadataIsNullOrUrlNotExisted = "Metadata is null or " + RibbonUrlKey + " is not existed in server metadata.";
    }
}
