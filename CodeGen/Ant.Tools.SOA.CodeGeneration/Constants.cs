using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ant.Tools.SOA.CodeGeneration
{
    public static class Constants
    {
        public const string XML_TYPE_ATTRIBUTE_NAME = "System.Xml.Serialization.XmlTypeAttribute";
        public const string XML_ROOT_ATTRIBUTE_NAME = "System.Xml.Serialization.XmlRootAttribute";
        public const string DATA_CONTRACT_ATTRIBUTE_NAME = "System.Runtime.Serialization.DataContractAttribute";
        public const string DATA_MEMBER_ATTRIBUTE_NAME = "System.Runtime.Serialization.DataMemberAttribute";
        public const string COLLECTION_DATA_CONTRACT_ATTRIBUTE_NAME = "System.Runtime.Serialization.CollectionDataContractAttribute";

        public const string COMMON_TYPE_NAMESPACE_NAME = "soa.ant.com.common.types.v1";
        public const string MOBILE_COMMON_TYPE_NAMESPACE_NAME = "soa.ant.com.mobile.common.types.v1";

        public const string ANT_SOA_COMMON_TYPE_NAMESPACE = "http://soa.ant.com/common/types/v1";
        public const string ANT_SOA_MOBILE_COMMON_TYPE_NAMESPACE = "http://soa.ant.com/mobile/common/types/v1";

        public const string C_SERVICE_STACK_COMMON_TYPES_NAMESPACE = "AntServiceStack.Common.Types";
        public const string RESPONSE_STATUS_PROPERTY_NAME = "ResponseStatus";
        public const string RESPONSE_STATUS_TYPE_NAME = "ResponseStatusType";
        public const string C_SERVICE_STACK_SERVICE_HOST_NAMESPACE = "AntServiceStack.ServiceHost";
        public const string SYSTEM_THREADING_TASKS_NAMESPACE = "System.Threading.Tasks";
        public const string COMMON_REQUEST_PROPERTY_NAME = "CommonRequest";
        public const string COMMON_REQUEST_TYPE_NAME = "CommonRequestType";

        public const string FILE_NAME = "FileName";
        public const string GENERATED_TYPE = "GeneratedType";
        public const string SCHEMA = "Schema";

        public static readonly Dictionary<string, string> DEFAULT_TYPE_MAPPINGS = new Dictionary<string, string>()
        {
            { "Guid", "System.Guid" }
        };
    }
}
