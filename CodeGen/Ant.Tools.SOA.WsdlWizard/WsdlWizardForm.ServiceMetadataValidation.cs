using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;
using System.Net;
using Ant.Tools.SOA.ServiceDescription;
using WizardControl;
using WRM.Windows.Forms;
using Message = Ant.Tools.SOA.ServiceDescription.Message;
using Ant.Tools.SOA;
using AntServiceStack.Text;
using System.Xml.Serialization;

namespace Ant.Tools.SOA.WsdlWizard
{
    public partial class WsdlWizardForm
    {
        public const string NoneOptionValue = "None";
        public const string ServiceDomainKey = "DomainName";
        public const string ServiceNameKey = "ServiceName";
        public const string ServiceNamespaceKey = "ServiceNamespace";
        public const string ServiceDocumentationKey = "ServiceDocumentation";
        public const string CacheFolderPathEnvironmentVariableName = "USERPROFILE";
        public const string ConfigFileName = "Ant.Tools.SOA.CodeGeneration.Config.xml";
        public const string CachedServiceRegistryFileName = "Ant.Tools.SOA.CodeGeneration.ServiceRegistry.json";
        public const string LogFileName = "Ant.Tools.SOA.CodeGeneration.log";
        public const string ServiceRegistrySyncFailMessage = @"Sync Registry failed. Repositry Service Lookup Uri: {0}. Please check the network connectivity or contact the framework SOA team.";

        public enum ServiceStateEnum
        {
            Registered,
            Unregistered
        }

        private string RepositryRegisteredServiceInterfaceUrl = "";
        private Dictionary<string, Dictionary<string, List<string>>> _serviceRegistryData;
        private CodeGenerationConfig _config;

        protected static string ConfigFilePath
        {
            get
            {
                return Path.Combine(Environment.GetEnvironmentVariable(CacheFolderPathEnvironmentVariableName), ConfigFileName);
            }
        }

        protected static string CachedServiceRegistryFilePath
        {
            get
            {
                return Path.Combine(Environment.GetEnvironmentVariable(CacheFolderPathEnvironmentVariableName), CachedServiceRegistryFileName);
            }
        }

        protected static string LogFilePath
        {
            get
            {
                return Path.Combine(Environment.GetEnvironmentVariable(CacheFolderPathEnvironmentVariableName), LogFileName);
            }
        }

        protected void InitializeServiceMetadataValidation()
        {
            LoadCodeGenerationConfig();

            string serviceRegistryDataText = LoadCachedServiceRegistryData();
            if (serviceRegistryDataText == null)
            {
                serviceRegistryDataText = SyncServiceRegistryData();
                if (serviceRegistryDataText != null)
                {
                    SaveServiceRegistryData(serviceRegistryDataText);
                    SaveCodeGenerationConfig(false);
                }
            }

            if (serviceRegistryDataText != null)
                ServiceRegistryData = ParseServiceRegistryData(serviceRegistryDataText);
            else
                ServiceRegistryData = null;
            FillServiceDomainCombolBox();

            TargetServiceState = _config.LatestServiceState;
            if (_config.LatestServiceState == ServiceStateEnum.Registered)
                tbServiceDocumentation.Text = _config.LatestUsedServiceDescription;
            tbServiceName.Text = _config.LatestUsedServiceName;
            tbNamespace.Text = _config.LatestUsedServiceNamespance;
            tbServiceDoc.Text = _config.LatestUsedServiceDescription;
            urlText.Text = _config.RepositryRegisteredServiceInterfaceUrl;
        }

        protected ServiceStateEnum TargetServiceState
        {
            get
            {
                return (ServiceStateEnum)Enum.Parse(typeof(ServiceStateEnum), ServiceSettingTabs.SelectedTab.Name);
            }
            set
            {
                foreach (TabPage tabPage in ServiceSettingTabs.TabPages)
                {
                    if ((ServiceStateEnum)Enum.Parse(typeof(ServiceStateEnum), tabPage.Name) == value)
                    {
                        ServiceSettingTabs.SelectedTab = tabPage;
                        break;
                    }
                }
            }
        }

        protected Dictionary<string, string> ServiceMetadata
        {
            get
            {
                Dictionary<string, string> metadata = new Dictionary<string, string>();
                switch(TargetServiceState)
                {
                    case ServiceStateEnum.Registered:
                        metadata[ServiceDomainKey] = cbbServiceDomain.SelectedItem.ToString();
                        metadata[ServiceNameKey] = cbbServiceName.SelectedItem.ToString();
                        metadata[ServiceNamespaceKey] = cbbServiceNamespace.SelectedItem.ToString();
                        metadata[ServiceDocumentationKey] = tbServiceDocumentation.Text;
                        break;
                    case ServiceStateEnum.Unregistered:
                        metadata[ServiceDomainKey] = string.Empty;
                        metadata[ServiceNameKey] = tbServiceName.Text;
                        metadata[ServiceNamespaceKey] = tbNamespace.Text;
                        metadata[ServiceDocumentationKey] = tbServiceDoc.Text;
                        break;
                    default:
                        break;
                }

                return metadata;
            }
        }

        protected Dictionary<string, Dictionary<string, List<string>>> ServiceRegistryData
        {
            get
            {
                return _serviceRegistryData;
            }

            set
            {
                _serviceRegistryData = value ?? new Dictionary<string, Dictionary<string, List<string>>>();
                RefineServiceRegistryData(_serviceRegistryData);
            }
        }

        protected void RefineServiceRegistryData(Dictionary<string, Dictionary<string, List<string>>> serviceRegistryData)
        {
            if (serviceRegistryData == null)
                return;
            if (serviceRegistryData.Count == 0)
                serviceRegistryData.Add(NoneOptionValue, new Dictionary<string, List<string>>());
            foreach (string domainName in serviceRegistryData.Keys)
            {
                if (serviceRegistryData[domainName].Count == 0)
                    serviceRegistryData[domainName].Add(NoneOptionValue, new List<string>());
                foreach (string serviceName in serviceRegistryData[domainName].Keys)
                {
                    if (serviceRegistryData[domainName][serviceName].Count == 0)
                        serviceRegistryData[domainName][serviceName].Add(NoneOptionValue);
                }
            }
        }

        protected void LoadCodeGenerationConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CodeGenerationConfig));
                    using (StreamReader reader = new StreamReader(ConfigFilePath))
                    {
                        _config = (CodeGenerationConfig)serializer.Deserialize(reader);
                        RepositryRegisteredServiceInterfaceUrl = _config.RepositryRegisteredServiceInterfaceUrl;
                        urlText.Text = RepositryRegisteredServiceInterfaceUrl;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine("Error: [LoadCodeGenerationConfig] {0}", ex);
                }
            }
            else
                SaveCodeGenerationConfig(true);

            if (_config == null)
                _config = new CodeGenerationConfig();
        }

        protected void SaveCodeGenerationConfig(bool isInit = false)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CodeGenerationConfig));
                if (isInit)
                    _config = new CodeGenerationConfig();
                else
                {
                    Dictionary<string, string> metadata = ServiceMetadata;
                    _config = new CodeGenerationConfig()
                    {
                        LatestServiceState = TargetServiceState,
                        LatestUsedDomain = metadata[ServiceDomainKey],
                        LatestUsedServiceName = metadata[ServiceNameKey],
                        LatestUsedServiceNamespance = metadata[ServiceNamespaceKey],
                        LatestUsedServiceDescription = metadata[ServiceDocumentationKey]
                    };
                }
                _config.RepositryRegisteredServiceInterfaceUrl = RepositryRegisteredServiceInterfaceUrl;
                using (StreamWriter writer = new StreamWriter(ConfigFilePath))
                {
                    serializer.Serialize(writer, _config);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error: [SaveCodeGenerationConfig] {0}", ex);
            }
        }

        protected string LoadCachedServiceRegistryData()
        {
            string cachedServiceRegistryDataText = null;

            if (File.Exists(CachedServiceRegistryFilePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(CachedServiceRegistryFilePath))
                    {
                        cachedServiceRegistryDataText = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine("Error: [LoadCachedServiceRegistryData] {0}", ex);
                }
            }

            return cachedServiceRegistryDataText;
        }

        protected string SyncServiceRegistryData()
        {
           
            string serviceRegistryDataText = null;
            try
            {
                serviceRegistryDataText = RepositryRegisteredServiceInterfaceUrl.GetJsonFromUrl();
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error: [SyncServiceRegistryData] {0}", ex);
            }

            return serviceRegistryDataText;
        }

        protected Dictionary<string, Dictionary<string, List<string>>> ParseServiceRegistryData(string serviceRegistryDataText)
        {
            Dictionary<string, Dictionary<string, List<string>>> serviceRegistryData = null;

            try
            {
                GetRegisteredServiceListResponse response = serviceRegistryDataText.FromJson<GetRegisteredServiceListResponse>();
                if (response != null && response.Success)
                {
                    serviceRegistryData = new Dictionary<string, Dictionary<string, List<string>>>();
                    if (response.Domains != null)
                    {
                        foreach (Domain domain in response.Domains)
                        {
                            serviceRegistryData.Add(domain.Name, new Dictionary<string, List<string>>());
                            if (domain.Services != null)
                            {
                                foreach (Service service in domain.Services)
                                {
                                    if (!serviceRegistryData[domain.Name].ContainsKey(service.ServiceName))
                                        serviceRegistryData[domain.Name].Add(service.ServiceName, new List<string>());
                                    serviceRegistryData[domain.Name][service.ServiceName].Add(service.Namespace);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error: [ParseServiceRegistryData] {0}", ex);
            }

            return serviceRegistryData;
        }

        protected void SaveServiceRegistryData(string serviceRegistryDataText)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(CachedServiceRegistryFilePath))
                {
                    writer.Write(serviceRegistryDataText);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Error: [SaveServiceRegistryData] {0}", ex);
            }
        }

        protected void FillServiceDomainCombolBox()
        {
            cbbServiceDomain.Items.Clear();
            cbbServiceDomain.Items.AddRange(ServiceRegistryData.Keys.ToArray());
            cbbServiceDomain.SelectedIndex = 0;

            foreach (var item in cbbServiceDomain.Items)
            {
                if (item.ToString().Equals(_config.LatestUsedDomain, StringComparison.InvariantCultureIgnoreCase))
                {
                    cbbServiceDomain.SelectedItem = item;
                    break;
                }
            }

            FillServiceNameCombolBox();
        }

        protected void FillServiceNameCombolBox()
        {
            string domainName = cbbServiceDomain.SelectedItem.ToString();
            cbbServiceName.Items.Clear();
            cbbServiceName.Items.AddRange(ServiceRegistryData[domainName].Keys.ToArray());
            cbbServiceName.SelectedIndex = 0;

            if (domainName == _config.LatestUsedDomain)
            {
                foreach (var item in cbbServiceName.Items)
                {
                    if (item.ToString().Equals(_config.LatestUsedServiceName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        cbbServiceName.SelectedItem = item;
                        break;
                    }
                }
            }

            FillServiceNamespaceCombolBox();
        }

        protected void FillServiceNamespaceCombolBox()
        {
            string domainName = cbbServiceDomain.SelectedItem.ToString();
            string serviceName = cbbServiceName.SelectedItem.ToString();
            cbbServiceNamespace.Items.Clear();
            cbbServiceNamespace.Items.AddRange(ServiceRegistryData[domainName][serviceName].ToArray());
            cbbServiceNamespace.SelectedIndex = 0;

            if (domainName == _config.LatestUsedDomain && serviceName == _config.LatestUsedServiceName)
            {
                foreach (var item in cbbServiceNamespace.Items)
                {
                    if (item.ToString().Equals(_config.LatestUsedServiceNamespance, StringComparison.InvariantCultureIgnoreCase))
                    {
                        cbbServiceNamespace.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void cbbServiceDomain_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillServiceNameCombolBox();
        }

        private void cbbServiceName_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillServiceNamespaceCombolBox();
        }

        private void btnSyncServiceRegistryData_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(urlText.Text))
            {
                MessageBox.Show(string.Format(ServiceRegistrySyncFailMessage, RepositryRegisteredServiceInterfaceUrl));
                return;
            }
            RepositryRegisteredServiceInterfaceUrl = urlText.Text;

            this.Enabled = false;

            string serviceRegistryDataText = SyncServiceRegistryData();
            if (serviceRegistryDataText != null)
            {
                SaveServiceRegistryData(serviceRegistryDataText);
                SaveCodeGenerationConfig(false);
                ServiceRegistryData = ParseServiceRegistryData(serviceRegistryDataText);
                FillServiceDomainCombolBox();
            }
            else
                MessageBox.Show(string.Format(ServiceRegistrySyncFailMessage, RepositryRegisteredServiceInterfaceUrl));

            this.Enabled = true;
        }

        public class CodeGenerationConfig
        {
            public string RepositryRegisteredServiceInterfaceUrl { get; set; }
            public ServiceStateEnum LatestServiceState { get; set; }
            public string LatestUsedDomain { get; set; }
            public string LatestUsedServiceName { get; set; }
            public string LatestUsedServiceNamespance { get; set; }
            public string LatestUsedServiceDescription { get; set; }
        }

        public class GetRegisteredServiceListResponse
        {
            public bool Success { get; set; }
            public List<Domain> Domains { get; set; }
        }

        public class Domain
        {
            public string Name { get; set; }
            public List<Service> Services { get; set; }
        }

        public class Service
        {
            public string ServiceName { get; set; }
            public string Namespace { get; set; }
        }

        static class Logger
        {
            public static void WriteLine(string format, params object[] paramList)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                    {
                        string message = string.Format(format, paramList);
                        writer.WriteLine("[{0}]{1}", DateTime.Now, message);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
