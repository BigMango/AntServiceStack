using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Xml;
using System.Web.Services.Discovery;
using System.Net;
using System.Xml.Schema;
using System.Net.Security;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Text;

using Ant.Tools.SOA.CodeGeneration.Options;
using Ant.Tools.SOA.CodeGeneration.Exceptions;

using WebServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace Ant.Tools.SOA.CodeGeneration
{
    /// <summary>
    /// Retrieves and imports meta data for WSDL documents and XSD files.
    /// </summary>
    internal sealed class MetadataFactory
    {
        private static void IgnoreTypeAddError(object sender, ValidationEventArgs e)
        {
            if (e.Exception.SourceSchemaObject is XmlSchemaType && e.Exception.Message.EndsWith("has already been declared."))
                return;
            if (e.Severity == XmlSeverityType.Error)
                throw e.Exception;
        }

        #region Public methods

        public static XmlSchemaSet CreateXmlSchemaSet()
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.CompilationSettings = new XmlSchemaCompilationSettings();
            schemaSet.CompilationSettings.EnableUpaCheck = false;
            schemaSet.ValidationEventHandler += IgnoreTypeAddError;
            return schemaSet;
        }

        /// <summary>
        /// Gets the XML schemas from a given WSDL document
        /// </summary>
        /// <param name="options">The metadata resolving options.</param>
        /// <returns>A collection of the XML schemas.</returns>
        public static XmlSchemas GetXmlSchemaSetFromWsdlFile(MetadataResolverOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException("MetadataResolverOptions could not be null.");
            }

            if (string.IsNullOrEmpty(options.MetadataLocation))
            {
                throw new ArgumentException("MetadataLocation option could not be null or an empty string.");
            }

            try
            {
                // First download the contracts if they are accessed over the web.
                DownloadContract(options);

                // Resolve metadata using a CtripDiscoveryClientProtocol.
                CtripDiscoveryClientProtocol dcp = new CtripDiscoveryClientProtocol();
                dcp.Credentials = GetCredentials(options);
                dcp.AllowAutoRedirect = true;
                dcp.DiscoverAny(options.MetadataLocation);
                dcp.ResolveAll();

                XmlSchemaSet schemaSet = CreateXmlSchemaSet();
                foreach (string url in dcp.Documents.Keys)
                {
                    object document = dcp.Documents[url];
                    if (document is System.Web.Services.Description.ServiceDescription)
                    {
                        foreach (XmlSchema schema in ((WebServiceDescription)document).Types.Schemas)
                        {
                            if (!IsEmptySchema(schema))
                            {
                                if (options.GenerateSeparateFilesEachXsd)
                                {
                                    if (string.IsNullOrWhiteSpace(schema.SourceUri))
                                        schema.SourceUri = url;
                                    ResolveIncludedSchemas(schemaSet, schema);
                                }
                                schemaSet.Add(schema);
                            }
                        }
                        continue;
                    }
                    if (document is XmlSchema)
                    {
                        XmlSchema xmlSchema = (XmlSchema)document;
                        if (options.GenerateSeparateFilesEachXsd)
                        {
                            if (string.IsNullOrWhiteSpace(xmlSchema.SourceUri))
                                xmlSchema.SourceUri = url;
                            ResolveIncludedSchemas(schemaSet, xmlSchema);
                        }
                        schemaSet.Add(xmlSchema);
                    }
                }

                RemoveDuplicates(ref schemaSet);
                schemaSet.Compile();

                XmlSchemas schemas = new XmlSchemas();
                foreach (XmlSchema schema in schemaSet.Schemas())
                {
                    schemas.Add(schema);
                }

                return schemas;

            }
            catch (Exception ex)
            {
                // TODO: Log exception.
                throw new MetadataResolveException("Could not resolve metadata. " + ex, ex);
            }
        }

        /// <summary>
        /// Gets the XML schemas for generating data contracts.
        /// </summary>
        /// <param name="options">The metadata resolving options.</param>
        /// <returns>A collection of the XML schemas.</returns>
        public static XmlSchemas GetXmlSchemaSetFromDataContractFiles(MetadataResolverOptions options)
        {
            if (options.DataContractFiles == null)
                throw new ArgumentNullException("No data contract files provided");

            // Resolve the schemas.
            XmlSchemaSet schemaSet = CreateXmlSchemaSet();
            for (int fi = 0; fi < options.DataContractFiles.Length; fi++)
            {
                // Skip the non xsd/wsdl files.
                string lowext = Path.GetExtension(options.DataContractFiles[fi]).ToLower();
                if (lowext == ".xsd") // This is an XSD file.
                {
                    XmlTextReader xmltextreader = null;

                    try
                    {
                        xmltextreader = new XmlTextReader(options.DataContractFiles[fi]);
                        XmlSchema schema = XmlSchema.Read(xmltextreader, null);
                        if (options.GenerateSeparateFilesEachXsd)
                        {
                            if (string.IsNullOrWhiteSpace(schema.SourceUri))
                                schema.SourceUri = options.DataContractFiles[fi];
                            ResolveIncludedSchemas(schemaSet, schema);
                        }
                        schemaSet.Add(schema);
                    }
                    finally
                    {
                        if (xmltextreader != null)
                        {
                            xmltextreader.Close();
                        }
                    }
                }
                else if (lowext == ".wsdl") // This is a WSDL file.
                {
                    CtripDiscoveryClientProtocol dcp = new CtripDiscoveryClientProtocol();
                    dcp.AllowAutoRedirect = true;
                    dcp.Credentials = CredentialCache.DefaultCredentials;
                    dcp.DiscoverAny(options.DataContractFiles[fi]);
                    dcp.ResolveAll();
                    foreach (string url in dcp.Documents.Keys)
                    {
                        object document = dcp.Documents[url];
                        if (document is XmlSchema)
                        {
                            XmlSchema xmlSchema = (XmlSchema)document;
                            if (options.GenerateSeparateFilesEachXsd)
                            {
                                if (string.IsNullOrWhiteSpace(xmlSchema.SourceUri))
                                    xmlSchema.SourceUri = url;
                                ResolveIncludedSchemas(schemaSet, xmlSchema);
                            }
                            schemaSet.Add(xmlSchema);
                        }
                        if (document is WebServiceDescription)
                        {
                            foreach (XmlSchema schema in ((WebServiceDescription)document).Types.Schemas)
                            {
                                if (!IsEmptySchema(schema))
                                {
                                    if (options.GenerateSeparateFilesEachXsd)
                                    {
                                        if (string.IsNullOrWhiteSpace(schema.SourceUri))
                                            schema.SourceUri = url;
                                        ResolveIncludedSchemas(schemaSet, schema);
                                    }
                                    schemaSet.Add(schema);
                                }
                            }
                        }
                    }
                }
            }

            RemoveDuplicates(ref schemaSet);
            schemaSet.Compile();

            XmlSchemas schemas = new XmlSchemas();
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                schemas.Add(schema);
            }

            return schemas;
        }

        #endregion

        #region Private methods

        private static bool IsEmptySchema(XmlSchema targetSchema)
        {
            return targetSchema.AttributeGroups.Count == 0 &&
                targetSchema.Attributes.Count == 0 &&
                targetSchema.Elements.Count == 0 &&
                targetSchema.SchemaTypes.Count == 0;
        }

        private static void DownloadContract(MetadataResolverOptions options)
        {
            // Return if we don't have an http endpoint.
            if (!options.MetadataLocation.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) &&
                !options.MetadataLocation.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            // Create a Uri for the specified metadata location.
            Uri uri = new Uri(options.MetadataLocation);
            string tempFilename = GetTempFilename(uri);

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CertValidation);

            WebRequest req = WebRequest.Create(options.MetadataLocation);
            WebResponse result = req.GetResponse();
            Stream receiveStream = result.GetResponseStream();
            WriteStream(receiveStream, tempFilename);
            options.MetadataLocation = tempFilename;
        }

        private static string GetTempFilename(Uri metadataUri)
        {
            string tempDir = Path.GetTempPath();
            string filename = null;
            Debug.Assert(tempDir != null, "Could not determine the temp directory.");

            // Check if the contracts are published in the root.
            if (metadataUri.Segments.Length == 1)
            {
                if (metadataUri.Segments[0] == "/")
                {
                    filename = Guid.NewGuid().ToString();
                }
                else
                {
                    // I haven't yet thought about this case and AFAIK,
                    // this code should never arrive here.
                    Debug.Assert(false, "Special case.");
                }
            }
            else
            {
                filename = metadataUri.Segments[metadataUri.Segments.Length - 1];
                filename = Path.GetFileNameWithoutExtension(filename);
            }

            filename = filename + ".wsdl";
            return Path.Combine(tempDir, filename);
        }

        private static bool CertValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        private static void WriteStream(Stream stream, string targetFile)
        {
            XmlWriter writer = null;
            StreamReader reader = null;
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
                reader = new StreamReader(stream);
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.OmitXmlDeclaration = true;
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.Encoding = Encoding.UTF8;
                writer = XmlWriter.Create(fileStream, xmlWriterSettings);
                string wsdl = reader.ReadToEnd();
                writer.WriteRaw(wsdl);
                writer.Flush();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (writer != null)
                {
                    writer.Close();
                }
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
                TrySetTempAttribute(targetFile);
            }
        }

        private static bool TrySetTempAttribute(string file)
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Temporary);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an object of ICredentials type according to the options.
        /// </summary>        
        private static ICredentials GetCredentials(MetadataResolverOptions options)
        {
            if (string.IsNullOrEmpty(options.Username))
            {
                return CredentialCache.DefaultCredentials;
            }
            else
            {
                return new NetworkCredential(options.Username, options.Password);
            }
        }

        /// <summary>
        /// Removes the duplicate schemas in a given XmlSchemaSet instance.
        /// </summary>
        private static void RemoveDuplicates(ref XmlSchemaSet set)
        {
            ArrayList schemaList = new ArrayList(set.Schemas());
            HashSet<XmlSchema> duplicatedSchemaSet = new HashSet<XmlSchema>();

            for (int schemaIndex = 0; schemaIndex < schemaList.Count; schemaIndex++)
            {
                for (int lowerSchemaIndex = schemaIndex + 1; lowerSchemaIndex < schemaList.Count; lowerSchemaIndex++)
                {
                    XmlSchema sourceSchema = (XmlSchema)schemaList[schemaIndex];
                    XmlSchema targetSchema = (XmlSchema)schemaList[lowerSchemaIndex];
  
                    if (!String.IsNullOrWhiteSpace(sourceSchema.SourceUri) && !String.IsNullOrWhiteSpace(targetSchema.SourceUri))
                    {
                        if (sourceSchema.SourceUri == targetSchema.SourceUri)
                        {
                            duplicatedSchemaSet.Add(targetSchema);
                        }

                    }
                    else 
                    {
                        if (!String.IsNullOrWhiteSpace(sourceSchema.Id) && !String.IsNullOrWhiteSpace(targetSchema.Id))
                        {
                            if (sourceSchema.Id == targetSchema.Id && sourceSchema.TargetNamespace == targetSchema.TargetNamespace)
                            {
                                duplicatedSchemaSet.Add(targetSchema);
                            }
                        }
                    }
                }
            }
            foreach (XmlSchema schema in duplicatedSchemaSet)
            {
                set.Remove(schema);
            }
        }

        private static void ResolveIncludedSchemas(XmlSchemaSet xmlSchemaSet, XmlSchema xmlSchema)
        {
            List<XmlSchema> xmlSchemas = new List<XmlSchema>();
            xmlSchemas.Add(xmlSchema);
            for (int i = 0; i < xmlSchemas.Count; i++)
            {
                foreach (XmlSchemaObject item in xmlSchema.Includes)
                {
                    if (item is XmlSchemaInclude)
                    {
                        XmlSchema include = ((XmlSchemaInclude)item).Schema;
                        if (include != null)
                        {
                            if (String.IsNullOrWhiteSpace(include.SourceUri))
                            {
                                string path = xmlSchema.SourceUri.Substring(0, xmlSchema.SourceUri.LastIndexOf('/') + 1);
                                include.SourceUri = path + ((XmlSchemaInclude)item).SchemaLocation;
                            }
                            // check item is not in list
                            bool listExist = false;
                            foreach (XmlSchema listXml in xmlSchemas)
                            {
                                if (listXml.SourceUri == include.SourceUri)
                                {
                                    listExist = true;
                                    break;
                                }
                                else
                                {
                                    if (!String.IsNullOrWhiteSpace(listXml.Id) && !String.IsNullOrWhiteSpace(include.Id))
                                    {
                                        if (listXml.Id == include.Id)
                                        {
                                            listExist = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!listExist)
                                xmlSchemas.Add(include);
                            // check item is not in set
                            bool setExist = false;
                            foreach (XmlSchema setXml in xmlSchemaSet.Schemas())
                            {
                                if (setXml.SourceUri == include.SourceUri)
                                {
                                    setExist = true;
                                    break;
                                }
                                else
                                {
                                    if (!String.IsNullOrWhiteSpace(setXml.Id) && !String.IsNullOrWhiteSpace(include.Id))
                                    {
                                        if (setXml.Id == include.Id)
                                        {
                                            setExist = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!setExist)
                                xmlSchemaSet.Add(include);
                        }
                    }
                }
            }
        }

        #endregion
    }
}