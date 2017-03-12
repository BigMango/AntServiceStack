using System.CodeDom;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Generates a <see cref="CodeCompileUnit"/> from a metadata set.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates the <see cref="CodeCompileUnit"/> based on the provide context.
        /// </summary>
        /// <param name="codeGeneratorContext">The code generator context.</param>
        CodeNamespace GenerateCode(ICodeGeneratorContext codeGeneratorContext);

        CodeCompileUnit GenerateDataContractCode(ICodeGeneratorContext codeGeneratorContext);

		CodeNamespace[] GenerateCodes(ICodeGeneratorContext codeGeneratorContext);
    }
}