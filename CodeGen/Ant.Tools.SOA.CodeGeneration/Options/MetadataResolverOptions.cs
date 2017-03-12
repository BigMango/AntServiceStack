using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Ant.Tools.SOA.CodeGeneration.Options
{
    /// <summary>
    /// This class defines the data structure used for holding metadata resolver options.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class MetadataResolverOptions
    {
        private string metadataLocation;
        private string userName;
        private string password;
        private bool metadataLocationChanged;

        public MetadataResolverOptions()
        {
        }

        public string MetadataLocation
        {
            get { return metadataLocation; }
            set
            {
                if (metadataLocation != null && metadataLocation != value)
                {
                    metadataLocationChanged = true;
                }
                metadataLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the data contract files (XSD and WSDL).
        /// </summary>
        public string[] DataContractFiles { get; set; }

        public string Username
        {
            get { return userName; }
            set { userName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public bool MetadataLocationChanged
        {
            get { return metadataLocationChanged; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to generate separate files each xsd file.
        /// </summary>
        public bool GenerateSeparateFilesEachXsd { get; set; }
    }
}
