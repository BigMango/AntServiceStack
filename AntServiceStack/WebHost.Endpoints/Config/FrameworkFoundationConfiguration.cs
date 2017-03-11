//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Com.Ctrip.Soa.Caravan.Configuration;
//using Com.Ctrip.Soa.Caravan.Utility;
//using AntServiceStack.ServiceHost;

//namespace AntServiceStack.Common.Configuration
//{
//    class FrameworkFoundationConfiguration : IConfiguration
//    {
//        public string GetPropertyValue(string key)
//        {
//            if (string.Equals(key, ServiceMetadata.SERVICE_REGISTRY_ENV_KEY, StringComparison.OrdinalIgnoreCase))
//                return EnvironmentUtility.SubEnvType;

//            return null;
//        }

//        public string this[string index]
//        {
//            get { return GetPropertyValue(index); }
//        }
//    }
//}
