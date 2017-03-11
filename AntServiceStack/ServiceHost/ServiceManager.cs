using System;
using System.Collections.Generic;
using System.Reflection;
using Freeway.Logging;
using System.Linq;
using Funq;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Text;
using AntServiceStack.Threading;

namespace AntServiceStack.ServiceHost
{
    public class ServiceManager
        : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceManager));

        /// <summary>
        /// 当前Container容器
        /// </summary>
        public Container Container { get; private set; }
        public ServiceController ServiceController { get; private set; }

        public string ServiceName { get; set; }

        /// <summary>
        /// 元数据集合 装载的操作是在 this.ServiceController  里面
        /// </summary>
        public Dictionary<string, ServiceMetadata> MetadataMap { get; internal set; }

        public Assembly[] AssembliesWithServices { get; private set; }

        public ServiceManager(params Assembly[] assembliesWithServices)
        {
            if (assembliesWithServices == null || assembliesWithServices.Length == 0)
                throw new ArgumentException(
                    "No Assemblies provided in your AppHost's base constructor.\n"
                    + "To register your services, please provide the assemblies where your web services are defined.");

            assembliesWithServices = assembliesWithServices.Distinct().ToArray();
            AssembliesWithServices = assembliesWithServices;

            #region 初始化
            this.Container = new Container { DefaultOwner = Owner.External };
            this.MetadataMap = new Dictionary<string, ServiceMetadata>();
            this.ServiceController = new ServiceController(() => GetAssemblyTypes(assembliesWithServices), this.MetadataMap); 
            #endregion
        }

        public ServiceManager(string serviceName,params Assembly[] assembliesWithServices) : this(assembliesWithServices)
        {
            this.ServiceName = serviceName;
           
        }

        public ServiceManager(params Type[] serviceTypes)
        {
            if (serviceTypes == null || serviceTypes.Length == 0)
                throw new ArgumentException(
                    "No service types provided in your AppHost's base constructor.\n"
                    + "To register your services, please provide the assemblies where your web services are defined.");

            this.Container = new Container { DefaultOwner = Owner.External };
            this.MetadataMap = new Dictionary<string, ServiceMetadata>();
            this.ServiceController = new ServiceController(() => serviceTypes, this.MetadataMap);
        }

        public ServiceManager(string serviceName, params Type[] serviceTypes):this(serviceTypes)
        {
            this.ServiceName = serviceName;
        }

        public ServiceManager(bool autoInitialize, params Assembly[] assembliesWithServices)
            : this(assembliesWithServices)
        {
            if (autoInitialize)
            {
                this.Init();
            }
        }

        public ServiceManager(Container container, params Assembly[] assembliesWithServices)
            : this(assembliesWithServices)
        {
            this.Container = container ?? new Container();
        }

        private List<Type> GetAssemblyTypes(Assembly[] assembliesWithServices)
        {
            var results = new List<Type>();
            string assemblyName = null;
            string typeName = null;

            try
            {
                foreach (var assembly in assembliesWithServices)
                {
                    assemblyName = assembly.FullName;
                    foreach (var type in assembly.GetTypes())
                    {
                        typeName = type.Name;
                        results.Add(type);
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                var msg = string.Format("Failed loading types, last assembly '{0}', type: '{1}'", assemblyName, typeName);
                Log.Fatal(msg, ex, new Dictionary<string, string>(){ { "ErrorCode", "FXD300007" } });
                throw new Exception(msg, ex);
            }
        }

        private ContainerResolveCache typeFactory;

        public void Init()
        {
            typeFactory = new ContainerResolveCache(this.Container);//Type缓存 使用过的Type 缓存起来 避免 重复 反射 影响性能

            this.ServiceController.Register(typeFactory); //装载 controller action route
            
            foreach (ServiceMetadata serviceMetadata in this.MetadataMap.Values)//this.MetadataMap在ServiceController装载的
                this.Container.RegisterAutoWiredTypes(serviceMetadata.ServiceTypes);//自动注入功能
        }

        public void RegisterService<T>()
        {
            bool isCService = ServiceUtils.IsCSerivce(typeof(T));
            if (!isCService)
                throw new ArgumentException("Type {0} is not a Web Service supported by AntServiceStack".Fmt(typeof(T).FullName));

            this.ServiceController.RegisterGService(typeFactory, typeof(T));
            this.Container.RegisterAutoWired<T>();
        }

        public void RegisterService(Type serviceType)
        {
            bool isCService = ServiceUtils.IsCSerivce(serviceType);
            if (!isCService)
                throw new ArgumentException("Type {0} is not a Web Service supported by AntServiceStack".Fmt(serviceType.FullName));

            this.ServiceController.RegisterGService(typeFactory, serviceType);
            this.Container.RegisterAutoWiredType(serviceType);
        }

        public object Execute(string operationName, object dto)
        {
            return this.ServiceController.Execute(operationName, dto, null);
        }

        public void Dispose()
        {

            if (this.Container != null)
            {
                this.Container.Dispose();
            }
        }

        public void AfterInit()
        {
            this.ServiceController.AfterInit();
        }
    }

}
