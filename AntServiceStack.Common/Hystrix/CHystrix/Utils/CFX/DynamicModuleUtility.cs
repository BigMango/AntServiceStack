namespace CHystrix.Utils.CFX
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security;
    using System.Web;
    using System.Web.Configuration;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class DynamicModuleUtility
    {
        [SecuritySafeCritical]
        public static void RegisterModule(Type moduleType)
        {
            if (DynamicModuleReflectionUtil.Fx45RegisterModuleDelegate != null)
            {
                DynamicModuleReflectionUtil.Fx45RegisterModuleDelegate(moduleType);
            }
            else
            {
                LegacyModuleRegistrar.RegisterModule(moduleType);
            }
        }

        [SecurityCritical]
        private static class LegacyModuleRegistrar
        {
            private static bool _integratedPipelineInitialized;
            private static readonly object _lockObj = new object();
            private const string _moduleNameFormat = "__DynamicModule_{0}_{1}";
            private static readonly DynamicModuleReflectionUtil _reflectionUtil = DynamicModuleReflectionUtil.Instance;

            private static void AddModuleToClassicPipeline(Type moduleType)
            {
                HttpModulesSection httpModulesFromAppConfig = null;
                try
                {
                    object target = _reflectionUtil.GetAppConfig();
                    httpModulesFromAppConfig = _reflectionUtil.GetHttpModulesFromAppConfig(target);
                    _reflectionUtil.SetConfigurationElementCollectionReadOnlyBit(httpModulesFromAppConfig.Modules, false);
                    CHystrix.Utils.CFX.DynamicModuleRegistryEntry entry = CreateDynamicModuleRegistryEntry(moduleType);
                    httpModulesFromAppConfig.Modules.Add(new HttpModuleAction(entry.Name, entry.Type));
                }
                finally
                {
                    if (httpModulesFromAppConfig != null)
                    {
                        _reflectionUtil.SetConfigurationElementCollectionReadOnlyBit(httpModulesFromAppConfig.Modules, true);
                    }
                }
            }

            private static void AddModuleToIntegratedPipeline(Type moduleType)
            {
                if (!_integratedPipelineInitialized)
                {
                    _integratedPipelineInitialized = true;
                    InitializeIntegratedPipeline();
                }
                CHystrix.Utils.CFX.DynamicModuleRegistryEntry item = CreateDynamicModuleRegistryEntry(moduleType);
                IntegratedDynamicModule.CriticalStatics.DynamicEntries.Add(item);
            }

            private static CHystrix.Utils.CFX.DynamicModuleRegistryEntry CreateDynamicModuleRegistryEntry(Type moduleType)
            {
                string assemblyQualifiedName = moduleType.AssemblyQualifiedName;
                return new CHystrix.Utils.CFX.DynamicModuleRegistryEntry(string.Format(CultureInfo.InvariantCulture, "__DynamicModule_{0}_{1}", new object[] { assemblyQualifiedName, Guid.NewGuid() }), assemblyQualifiedName);
            }

            private static void InitializeIntegratedPipeline()
            {
                IList list = _reflectionUtil.NewListOfModuleConfigurationInfo();
                string name = "__ASP_IntegratedDynamicModule_Shim";
                string assemblyQualifiedName = typeof(IntegratedDynamicModule).AssemblyQualifiedName;
                string condition = "managedHandler";
                object obj2 = _reflectionUtil.NewModuleConfigurationInfo(name, assemblyQualifiedName, condition);
                list.Add(obj2);
                _reflectionUtil.SetModuleConfigInfo(list);
            }

            public static void RegisterModule(Type moduleType)
            {
                VerifyParameters(moduleType);
                if (_reflectionUtil != null)
                {
                    lock (_lockObj)
                    {
                        _reflectionUtil.ThrowIfPreAppStartNotRunning();
                        AddModuleToClassicPipeline(moduleType);
                        AddModuleToIntegratedPipeline(moduleType);
                    }
                }
            }

            private static void VerifyParameters(Type moduleType)
            {
                if (moduleType == null)
                {
                    throw new ArgumentNullException("moduleType");
                }
                if (!typeof(IHttpModule).IsAssignableFrom(moduleType))
                {
                    throw new ArgumentException("moduleType");
                }
            }
        }
    }
}

