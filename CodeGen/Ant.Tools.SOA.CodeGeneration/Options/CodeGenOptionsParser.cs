using System;
using System.Diagnostics;

namespace Ant.Tools.SOA.CodeGeneration.Options
{
    /// <summary>
    /// This class contains the code implementation of the code generation options parser.
    /// </summary>
    [DebuggerStepThrough]
    public class CodeGenOptionsParser
    {
        // Filters the code writer options.
        public static CodeWriterOptions GetCodeWriterOptions(CodeGenOptions options)
        {
            CodeWriterOptions writerOptions = new CodeWriterOptions();
            writerOptions.GenerateSeparateFiles = options.GenerateSeparateFiles;
            writerOptions.GenerateSeparateFilesEachNamespace = options.GenerateSeparateFilesEachNamespace;
			writerOptions.GenerateSeparateFilesEachXsd = options.GenerateSeparateFilesEachXsd;
            writerOptions.OutputLocation = options.OutputLocation;
            writerOptions.ProjectDirectory = options.ProjectDirectory;
            writerOptions.OutputFileName = options.OutputFileName;
            writerOptions.OverwriteExistingFiles = options.OverwriteExistingFiles;
            writerOptions.Language = options.Language;
            writerOptions.TargetNamespace = options.ClrNamespace;
            writerOptions.OnlyUseDataContractSerializer = options.OnlyUseDataContractSerializer;
			writerOptions.AscendingClassByName = options.AscendingClassByName;
            return writerOptions;
        }

        // Filters the metadata resolver options.
        public static MetadataResolverOptions GetMetadataResolverOptions(CodeGenOptions options)
        {
            MetadataResolverOptions resolverOptions = new MetadataResolverOptions();
            resolverOptions.MetadataLocation = options.MetadataLocation;
            resolverOptions.DataContractFiles = options.DataContractFiles;
            resolverOptions.Username = options.Username;
            resolverOptions.Password = options.Password;
            resolverOptions.GenerateSeparateFilesEachXsd = options.GenerateSeparateFilesEachXsd;
            return resolverOptions;
        }
    }
}
