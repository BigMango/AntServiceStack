using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Message
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class MessageSensitivityAttribute : Attribute
    {
        /// <summary>
        /// 默认为Default
        /// </summary>
        public SensitivityMode RequestSensitivity { get; set; }

        /// <summary>
        /// 默认为Default
        /// </summary>
        public SensitivityMode ResponseSensitivity { get; set; }

        /// <summary>
        /// 默认为false
        /// </summary>
        public bool LogResponse { get; set; }

        /// <summary>
        /// 默认为false
        /// </summary>
        public bool DisableLog { get; set; }
    }
}
