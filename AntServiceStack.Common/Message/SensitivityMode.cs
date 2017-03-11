using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Message
{
    public enum SensitivityMode
    {
        /// <summary>
        /// 具体行为和环境有关，当前生产环境等效于Sensitive，非生产环境等效于Insensitive。以后可能随着公司安全策略变化而变化。
        /// </summary>
        Default,

        /// <summary>
        /// 报文中包含敏感信息：身份证、密码等等
        /// </summary>
        Sensitive,

        /// <summary>
        /// 报文中不包含敏感信息
        /// </summary>
        Insensitive
    }
}
