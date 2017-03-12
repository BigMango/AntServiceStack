using System;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Text;
using System.Linq;

using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.Exceptions;
using Ant.Tools.SOA.CodeGeneration.Extensions;
using System.Xml.Schema;

namespace Ant.Tools.SOA.CodeGeneration.CodeWriter
{
    /// <summary>
    /// This class implements the methods that are required to produce the final code
    /// out of the CodeDom object graph and write the generated code in to the desired 
    /// location.
    /// </summary>
    public class CodeWriter
    {
        #region Private Fields

        private CodeCompileUnit codeCompileUnit;
        // Reference to the configuration object to be written to the disk.
        //private Configuration configuration;
        // Reference to the CodeWriterOptions instance containing the code 
        // writer options.
        private CodeWriterOptions options;
        // Reference to the CodeProvider instance that we use to generate code.
        private CodeDomProvider provider;
        // Reference to the array of strings holding the generated file names.
        private string[] generatedCodeFileNames;
        // Reference to the generated configuration file path.
        //private string configurationFile;
        // Reference to the CodeGeneratorOptions instance we use for writing the 
        // code to files.
        private CodeGeneratorOptions codeGenerationOptions;

        private Dictionary<string, CodeNamespace> namespaceTypesMapping;
        private Dictionary<string, string> typeNamespaceMapping;

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor.
        /// </summary>
        CodeWriter(CodeNamespace codeNamespace, CodeWriterOptions options)
            : this(options)
        {
            this.codeCompileUnit = new CodeCompileUnit();
            this.codeCompileUnit.Namespaces.Add(codeNamespace);
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        CodeWriter(CodeNamespace[] codeNamespaces, CodeWriterOptions options)
            : this(options)
        {
            this.codeCompileUnit = new CodeCompileUnit();
            this.codeCompileUnit.Namespaces.AddRange(codeNamespaces);
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        CodeWriter(CodeCompileUnit codeCompileUnit, CodeWriterOptions options)
            : this(options)
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        CodeWriter(CodeWriterOptions options)
        {
            this.provider = CodeDomProvider.CreateProvider(options.Language.ToString());
            this.options = options;
        }

        #endregion

        /// <summary>
        /// Writes the code to the disk according to the given options.
        /// </summary>
        private void WriteCodeFiles()
        {
            // Ensure the output directory exist in the file system.
            EnsureDirectoryExists(options.OutputLocation);

            // Create the CodeGenerationOptions instance.
            codeGenerationOptions = CodeWriter.CreateCodeGeneratorOptions();

            // Do we have to generate separate files each type?
            if (options.GenerateSeparateFiles)
            {
                // Write the code into separate files.
                WriteSeparateCodeFiles();
            }
            // Do we have to generate separate files each namespace?
            else if (options.GenerateSeparateFilesEachNamespace)
            {
                // Write the code into separate files.
                WriteSeparateCodeFilesEachNamespace();
            }
            else if (options.GenerateSeparateFilesEachXsd && !options.OnlyUseDataContractSerializer)
            {
                WriteSeparateCodeFilesEachXsd();
            }
            else
            {
                // Write the code into a singl file.
                WriteSingleCodeFile();
            }
        }

        /// <summary>
        /// This method writes the generated code into a single file.
        /// </summary>
        private void WriteSingleCodeFile()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");
            Debug.Assert(!string.IsNullOrEmpty(options.OutputFileName), "This action cannot be performed when output file name is null or an empty string");

            CodeExtension.RefineCodeWithShortName(codeCompileUnit);
            CodeNamespace codeNamespace = CodeExtension.GenerateCode(codeCompileUnit);
            codeNamespace.Name = options.TargetNamespace;
            CodeCompileUnit tempUnit = new CodeCompileUnit();
            tempUnit.Namespaces.Add(codeNamespace);

            // Get the destination file name.
            string fileName = CodeWriter.GetUniqueFileName(options.OutputLocation, options.OutputFileName, options.Language, options.OverwriteExistingFiles);
            // Create a StreamWriter for writing to the destination file.
            StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
            try
            {
                // Write out the code to the destination file.
                provider.GenerateCodeFromCompileUnit(tempUnit, writer, codeGenerationOptions);
                // Flush all buffers in the writer.
                writer.Flush();
                // Initialize generatedFileNames array to hold the one and only one 
                // file we just generated.
                generatedCodeFileNames = new string[1];
                // Finally add the file name to the generatedFileNames array.
                generatedCodeFileNames[0] = fileName;
            }
            catch (IOException e)
            {
                // Wrap the IOException in a CodeWriterException with little bit 
                // more information.
                throw new CodeWriterException(
                    string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
            }
            finally
            {
                // No matter what happens, dispose the stream writer and release the unmanaged 
                // resources.
                writer.Dispose();
            }
        }

        /// <summary>
        /// This method writes each type generated into a separate file. 
        /// The type name is used as the file name.
        /// </summary>
        private void WriteSeparateCodeFiles()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");

            List<string> fileNameList = new List<string>();
            foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
            {
                CodeCompileUnit tempUnit = new CodeCompileUnit();
                CodeNamespace tempns = new CodeNamespace(codeNamespace.Name);
                foreach (CodeNamespaceImport @import in codeNamespace.Imports)
                    tempns.Imports.Add(@import);
                tempUnit.Namespaces.Add(tempns);
                for (int i = 0; i < codeNamespace.Types.Count; i++)
                {
                    tempns.Types.Clear();

                    // Take a reference to the CodeTypeDeclaration at the current index.
                    CodeTypeDeclaration ctd = codeNamespace.Types[i];

                    // Add the type to the temporary namespace.
                    tempns.Types.Add(ctd);
                    // Get the destination file name.
                    string fileName = CodeWriter.GetUniqueFileName(options.OutputLocation, ctd.Name, options.Language, options.OverwriteExistingFiles);
                    // Create a StreamWriter for writing to the destination file.
                    StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
                    try
                    {
                        // Write out the code to the destination file.
                        provider.GenerateCodeFromCompileUnit(tempUnit, writer, codeGenerationOptions);
                        // Flush all buffers in the writer.
                        writer.Flush();
                        // Finally add the file name to the generated file names array.
                        fileNameList.Add(fileName);
                    }
                    catch (IOException e)
                    {
                        // Wrap the IOException in a CodeWriterException with little bit 
                        // more information.
                        throw new CodeWriterException(
                            string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
                    }
                    finally
                    {
                        // No matter what happens, dispose the stream writer and release the unmanaged 
                        // resources.
                        writer.Dispose();
                    }
                }
            }

            generatedCodeFileNames = fileNameList.ToArray();
        }

        /// <summary>
        /// This method writes types in each namespace into a separate file. 
        /// The namespace name is used as the file name.
        /// </summary>
        private void WriteSeparateCodeFilesEachNamespace()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");

            List<string> fileNameList = new List<string>();
            string extension = "." + CodeWriter.GetExtension(options.Language);
            if (options.OnlyUseDataContractSerializer)
            {
                foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
                {
                    string fileName = CodeWriter.GetUniqueFileName(
                        options.OutputLocation,
                        (codeNamespace.Name == options.TargetNamespace ? options.OutputFileName : codeNamespace.Name) + extension,
                        options.Language,
                        options.OverwriteExistingFiles);
                    try
                    {
                        CodeCompileUnit tempUnit = new CodeCompileUnit();
                        tempUnit.Namespaces.Add(codeNamespace);
                        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                        {
                            provider.GenerateCodeFromCompileUnit(tempUnit, writer, codeGenerationOptions);
                            writer.Flush();
                            fileNameList.Add(fileName);
                        }
                    }
                    catch (IOException e)
                    {
                        throw new CodeWriterException(
                            string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
                    }
                }
            }
            else
            {
                CodeNamespace codeNamespace = codeCompileUnit.Namespaces[0];
                InitTypeMappings();

                foreach (KeyValuePair<string, CodeNamespace> namespacePair in namespaceTypesMapping)
                {
                    CodeNamespace @namespace = namespacePair.Value;
                    List<string> dependentNamespaces = GetDependentNamespaces(@namespace);
                    foreach (string item in dependentNamespaces)
                    {
                        @namespace.Imports.Add(new CodeNamespaceImport(item));
                    }

                    CodeCompileUnit tempUnit = new CodeCompileUnit();
                    tempUnit.Namespaces.Add(@namespace);

                    string fileName = CodeWriter.GetUniqueFileName(
                        options.OutputLocation,
                        namespacePair.Key == codeNamespace.Name ? options.OutputFileName : (namespacePair.Key + extension),
                        options.Language,
                        options.OverwriteExistingFiles);
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                        {
                            provider.GenerateCodeFromCompileUnit(tempUnit, writer, codeGenerationOptions);
                            writer.Flush();
                            fileNameList.Add(fileName);
                        }
                    }
                    catch (IOException e)
                    {
                        throw new CodeWriterException(
                            string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
                    }
                }
            }

            generatedCodeFileNames = fileNameList.ToArray();
        }

        private void WriteSeparateCodeFilesEachXsd()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");

            List<string> fileNameList = new List<string>();
            string extension = "." + CodeWriter.GetExtension(options.Language);

            foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
            {
                string FileName;
                XmlSchema xsdSchema = codeNamespace.UserData[Constants.SCHEMA] as XmlSchema;
                if (xsdSchema != null)
                {
                    string Url = ((System.Xml.Schema.XmlSchema)codeNamespace.UserData[Constants.SCHEMA]).SourceUri;
                    string XsdFileName = Url.Substring(Url.LastIndexOf('/') + 1);
                    FileName = XsdFileName.Substring(0, XsdFileName.LastIndexOf('.') + 1);
                }
                else
                {
                    FileName = codeNamespace.UserData[Constants.FILE_NAME] as string;
                }
                string FullName = CodeWriter.GetUniqueFileName(options.OutputLocation, FileName, options.Language, options.OverwriteExistingFiles);
                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(FullName, false, Encoding.UTF8);
                    CodeCompileUnit tempUnit = new CodeCompileUnit();
                    tempUnit.Namespaces.Add(codeNamespace);
                    provider.GenerateCodeFromCompileUnit(tempUnit, writer, codeGenerationOptions);
                    writer.Flush();
                    fileNameList.Add(FullName);
                }
                catch (IOException e)
                {
                    throw new CodeWriterException(
                        string.Format("An error occurred while trying write to file {0}: {1}", FullName, e.Message), e);
                }
                finally
                {
                    if(writer != null) writer.Close();
                    writer = null;
                }
            }

            generatedCodeFileNames = fileNameList.ToArray();
        }

        private void InitTypeMappings()
        {
            namespaceTypesMapping = new Dictionary<string, CodeNamespace>();
            typeNamespaceMapping = new Dictionary<string, string>();
            foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
            {
                CodeNamespaceImport[] imports = codeNamespace.Imports.OfType<CodeNamespaceImport>().ToArray();
                foreach (CodeTypeDeclaration type in codeNamespace.Types)
                {
                    string @namespace = GetTypeNamespace(type, codeNamespace);
                    typeNamespaceMapping[type.Name] = @namespace;
                    if (!namespaceTypesMapping.ContainsKey(@namespace))
                    {
                        namespaceTypesMapping[@namespace] = new CodeNamespace(@namespace);
                        namespaceTypesMapping[@namespace].Imports.AddRange(imports);
                    }
                    namespaceTypesMapping[@namespace].Types.Add(type);
                }
            }
        }

        private List<string> GetDependentNamespaces(CodeTypeDeclaration type)
        {
            List<CodeTypeReference> dependentTypes = new List<CodeTypeReference>();
            if (type.IsInterface || (type.BaseTypes.Count == 1 && type.BaseTypes[0].BaseType == "ServiceClientBase`1"))
            {
                foreach (CodeMemberMethod method in type.Members.OfType<CodeMemberMethod>())
                {
                    if (method.Parameters.Count == 1 && method.ReturnType.BaseType != "System.Void")
                    {
                        CodeTypeReference requestType = method.Parameters[0].Type;
                        dependentTypes.Add(requestType);
                        dependentTypes.Add(method.ReturnType);
                    }
                }
            }
            else
            {
                foreach (CodeMemberProperty property in type.Members.OfType<CodeMemberProperty>())
                {
                    if (property.Type.TypeArguments.Count == 1)
                        dependentTypes.Add(property.Type.TypeArguments[0]);
                    else if (property.Type.ArrayElementType != null && property.Type.ArrayElementType.ArrayElementType != null)
                        dependentTypes.Add(property.Type.ArrayElementType.ArrayElementType);
                    else
                        dependentTypes.Add(property.Type);
                }
            }

            List<string> dependentNamespaces = new List<string>();
            foreach (CodeTypeReference typeReference in dependentTypes)
            {
                if (typeNamespaceMapping.ContainsKey(typeReference.BaseType)
                    && !dependentNamespaces.Contains(typeNamespaceMapping[typeReference.BaseType]))
                    dependentNamespaces.Add(typeNamespaceMapping[typeReference.BaseType]);
            }

            return dependentNamespaces;
        }

        private List<string> GetDependentNamespaces(CodeNamespace @namespace)
        {
            List<string> dependentNamespaces = new List<string>();
            foreach (CodeTypeDeclaration type in @namespace.Types)
            {
                dependentNamespaces.AddRange(GetDependentNamespaces(type));
            }

            return dependentNamespaces.Distinct().Except(new string[] { @namespace.Name }).ToList();
        }

        private string GetTypeNamespace(CodeTypeDeclaration type, CodeNamespace codeNamespace)
        {
            string @namespace = codeNamespace.Name;
            foreach (CodeAttributeDeclaration attribute in type.CustomAttributes)
            {
                if (attribute.Name == Constants.XML_TYPE_ATTRIBUTE_NAME
                    || attribute.Name == Constants.DATA_CONTRACT_ATTRIBUTE_NAME
                    || attribute.Name == Constants.COLLECTION_DATA_CONTRACT_ATTRIBUTE_NAME)
                {
                    foreach (CodeAttributeArgument argument in attribute.Arguments)
                    {
                        if (argument.Name == "Namespace")
                            @namespace = GenerateNamespaceCodeString(Convert.ToString(((CodePrimitiveExpression)argument.Value).Value));
                    }

                    if (!string.IsNullOrWhiteSpace(@namespace))
                        break;
                }
            }

            return @namespace;
        }

        private static string GenerateNamespaceCodeString(string @namespace)
        {
            @namespace = @namespace.Trim().Replace("http://", string.Empty).Replace("urn:", string.Empty).Replace("soa.ant.com", string.Empty);
            string[] parts = @namespace.Split(new char[] { ':', '/', '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            @namespace = string.Empty;
            foreach (string part in parts)
            {
                if (@namespace != string.Empty)
                    @namespace += ".";
                @namespace += char.ToUpper(part[0]) + part.Substring(1);
            }
            return @namespace;
        }

        /// <summary>
        /// This is a helper method for acquiring a unique file name. 
        /// </summary>
        /// <returns>
        /// A string representing the absolute path of a given file. If overwrite flag is turned off and 
        /// if the evaluvated file name already exists in the file system, this function creates a new 
        /// file name by appending a numeric constant at the end of the file name.
        /// </returns>
        private static string GetUniqueFileName(string directory, string fileName, CodeLanguage language, bool overwrite)
        {
            // Get the appropriate file extension for the selected programming language.
            string ext = CodeWriter.GetExtension(language);
            // Read the file name without the extension.
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            // Construct the absolute path of the file by concatanating directory name, file name and the 
            // extension.
            string absPath = Path.Combine(directory, string.Format("{0}.{1}", fileNameWithoutExt, ext));

            // Can't we overwrite files?
            if (!overwrite)
            {
                // We arrive here if we cannot overwrite existing files.

                // Counter used for generating the numeric constant for unique file name generation.
                int counter = 1;

                // Create a new file name until the created file name does not exist in the file system.
                while (File.Exists(absPath))
                {
                    // Create the new file name.
                    absPath = Path.Combine(directory, string.Format("{0}{1}.{2}", fileNameWithoutExt, counter.ToString(), ext));
                    // Increment the counter.
                    counter++;
                }
            }

            // Finally return the generated absolute path of the file.
            return absPath;
        }

        /// <summary>
        /// This is a helper method to ensure that a given directory
        /// exists in the file system.
        /// </summary>
        /// <remarks>
        /// If the specified directory does not exist in the file system
        /// this method creates it before returning.
        /// </remarks>
        private static void EnsureDirectoryExists(string directory)
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(directory), "directory parameter could not be null or an empty string.");

            try
            {
                // Can't we see the directory in the file system?
                if (!Directory.Exists(directory))
                {
                    // Create it.
                    Directory.CreateDirectory(directory);
                }
            }
            catch (IOException e)
            {
                throw new CodeWriterException(
                    string.Format("An error occurred while trying verify the output directory: {0}", e.Message), e);
            }
        }

        /// <summary>
        /// This is a helper method to create an instance of 
        /// CodeGeneratorOptions class with desired code generation options.
        /// </summary>        
        private static CodeGeneratorOptions CreateCodeGeneratorOptions()
        {
            // Create and instance of CodeGeneratorOptions class.
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            // Set the bracing style to "C". This will make sure that braces start on the line following 
            // the statement or declaration that they are associated with.
            options.BracingStyle = "C";
            // Finally return the CodeGeneratorOptions class instance.
            return options;
        }

        #region Public Static Methods

        /// <summary>
        /// Generates the code using the appropriate code provider and writes it to the 
        /// desired location.
        /// </summary>        
        public static CodeWriterOutput Write(CodeNamespace codeNamespace, CodeWriterOptions options)
        {
            if (options.AscendingClassByName)
            {
                SortAsc(codeNamespace.Types);
            }

            // Create a new instance of CodeWriter class with given options.
            CodeWriter writer = new CodeWriter(codeNamespace, options);
            // Execute the code writing procedure.
            writer.WriteCodeFiles();
            // Crate an instance of CodeWriterOutput class with the code writer's output.
            CodeWriterOutput output = new CodeWriterOutput(writer.generatedCodeFileNames);
            // Finally return the CodeWriterOutput.
            return output;
        }

        /// <summary>
        /// Generates the code using the appropriate code provider and writes it to the 
        /// desired location.
        /// </summary>        
        public static CodeWriterOutput Write(CodeCompileUnit codeCompileUnit, CodeWriterOptions options)
        {
            if (options.AscendingClassByName)
            {
                foreach (CodeNamespace codeNamespace in codeCompileUnit.Namespaces)
                {
                    SortAsc(codeNamespace.Types);
                }
            }

            // Create a new instance of CodeWriter class with given options.
            CodeWriter writer = new CodeWriter(codeCompileUnit, options);
            // Execute the code writing procedure.
            writer.WriteCodeFiles();
            // Crate an instance of CodeWriterOutput class with the code writer's output.
            CodeWriterOutput output = new CodeWriterOutput(writer.generatedCodeFileNames);
            // Finally return the CodeWriterOutput.
            return output;
        }

        /// <summary>
        /// Generates the code using the appropriate code provider and writes it to the 
        /// desired location.
        /// </summary>    
        public static CodeWriterOutput Write(CodeNamespace[] codeNamespaces, CodeWriterOptions options)
        {
            if (options.AscendingClassByName)
            {
                foreach (CodeNamespace codeNamespace in codeNamespaces)
                {
                    SortAsc(codeNamespace.Types);
                }
            }

            // Create a new instance of CodeWriter class with given options.
            CodeWriter writer = new CodeWriter(codeNamespaces, options);
            // Execute the code writing procedure.
            writer.WriteCodeFiles();
            // Crate an instance of CodeWriterOutput class with the code writer's output.
            CodeWriterOutput output = new CodeWriterOutput(writer.generatedCodeFileNames);
            // Finally return the CodeWriterOutput.
            return output;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Helper method to get the code file extension for a given programming 
        /// language.
        /// </summary>
        private static string GetExtension(CodeLanguage language)
        {
            // Switch the language.
            switch (language)
            {
                case CodeLanguage.CSharp:   // C#
                    return "cs";
                case CodeLanguage.VisualBasic: // Visual Basic
                    return "vb";
                default:
                    // If it's anything else we simply return an empty string
                    // representing an unknown language.
                    return string.Empty;
            }
        }

        private static void SortAsc(CodeTypeDeclarationCollection TypeCollection)
        {
            List<CodeTypeDeclaration> codeTypes = new List<CodeTypeDeclaration>();
            codeTypes.AddRange(TypeCollection.OfType<CodeTypeDeclaration>().ToArray());
            CodeTypeDeclaration generatedType = codeTypes.FirstOrDefault(
                type =>
                {
                    if (type.UserData[Constants.GENERATED_TYPE] as string != null) return true;
                        return false;
                });
            if (generatedType != null)
                codeTypes.Remove(generatedType);

            //Sort Types Ascending By Type Name
            codeTypes.Sort((a, b) => { return String.Compare(a.Name, b.Name); });
            TypeCollection.Clear();
            TypeCollection.AddRange(codeTypes.ToArray());
            if (generatedType != null) 
                TypeCollection.Add(generatedType);
        }

        #endregion
    }
}
