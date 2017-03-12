using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Xml.Serialization;
using Ant.Tools.SOA.CodeGeneration.Options;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Extension interface
    /// </summary>
    public interface ICodeExtension
    {
        /// <summary>
        /// Process code for extension
        /// </summary>
        /// <param name="code">CodeNamespace type</param>
        /// <param name="codeGeneratorContext">Code generator context</param>
        void Process(CodeNamespace code, ICodeGeneratorContext codeGeneratorContext);
    }
}
