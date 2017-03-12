using System.IO;
using System.Diagnostics;

namespace Ant.Tools.SOA.CodeGeneration.Options
{
    /// <summary>
    /// This class defines the data structure used for holding code writer options.
    /// </summary>
    [DebuggerStepThrough]
    public class CodeWriterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to generate separate files each type.
        /// </summary>
        public bool GenerateSeparateFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate separate files each namespace.
        /// </summary>
        public bool GenerateSeparateFilesEachNamespace { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate separate files each xsd file.
		/// </summary>
		public bool GenerateSeparateFilesEachXsd { get; set; }

        /// <summary>
        /// Gets or sets the output location.
        /// </summary>
        public string OutputLocation { get; set; }

        /// <summary>
        ///	Gets or sets the project directory.
        /// </summary>
        public string ProjectDirectory { get; set; }

        /*
        /// <summary>
        /// Gets or sets the configuration file.
        /// </summary>
        public string ConfigurationFile { get; set; }
        */
        
        /// <summary>
        /// Gets or sets the name of the output file.
        /// </summary>
        public string OutputFileName { get; set; }

        public string TargetNamespace { get; set; }

        public bool OnlyUseDataContractSerializer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing files.
        /// </summary>
        public bool OverwriteExistingFiles { get; set; }

        /// <summary>
        /// Gets or sets the code language.
        /// </summary>
        public CodeLanguage Language { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether classes should be sorted ascending by its' name
		/// </summary>
		public bool AscendingClassByName { get; set; }

        /// <summary>
        /// Gets the directory a file should be written to based on its extension.
        /// </summary>
        /// <returns>The directory to write the file to.</returns>
        public string GetOutputDirectoryForFileType(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();
            return (extension == ".svc" || extension == ".config")
                ? ProjectDirectory
                : OutputLocation;
        }
    }

}
