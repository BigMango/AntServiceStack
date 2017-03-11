using CHystrix.Utils.CFX;

namespace CHystrix.Web
{
    using CHystrix;
    using CHystrix.Utils;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Routing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Initializer
    {
        private static volatile bool _done;
        private static object _lock = new object();
        private static Action<Type> _registerModuleMethodFromReflection;

        private static Assembly GetAssembly(string assemblyName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }
            string path = null;
            try
            {
                path = HttpRuntime.BinDirectory + assemblyName + ".dll";
                if (!File.Exists(path))
                {
                    CommonUtils.Log.Log(LogLevelEnum.Info, "No assembly " + path + " exists.", new Dictionary<string, string>().AddLogTagData("FXD303048"));
                    return null;
                }
                Assembly assembly2 = Assembly.LoadFrom(path);
                CommonUtils.Log.Log(LogLevelEnum.Info, "Loaded assembly " + path, new Dictionary<string, string>().AddLogTagData("FXD303049"));
                return assembly2;
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Info, "Load assembly " + (path ?? assemblyName) + " failed.", exception, new Dictionary<string, string>().AddLogTagData("FXD303050"));
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void PreApplicationStartCode()
        {
            if (!_done && Monitor.TryEnter(_lock))
            {
                try
                {
                    if (!_done)
                    {
                        _done = true;
                        RegisterHystrixRoutes();
                        if (HostingEnvironment.IsHosted)
                        {
                            PreRegisterModule();
                            RegisterModule(typeof(HystrixModule));
                        }
                    }
                }
                catch (Exception exception)
                {
                    CommonUtils.Log.Log(LogLevelEnum.Fatal, "CHystrix Web Initializer failed at startup.", exception, new Dictionary<string, string>().AddLogTagData("FXD303028"));
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        private static void PreRegisterModule()
        {
            Assembly assembly = GetAssembly("Arch.CFX");
            if (assembly != null)
            {
                Type type = assembly.GetType("Arch.CFramework.InnerAppInternals");
                if (type != null)
                {
                    MethodInfo method = type.GetMethod("RegisterModule");
                    if (method != null)
                    {
                        _registerModuleMethodFromReflection = Delegate.CreateDelegate(typeof(Action<Type>), method) as Action<Type>;
                        return;
                    }
                }
            }
            assembly = GetAssembly("Microsoft.Web.Infrastructure");
            if (assembly != null)
            {
                Type type2 = assembly.GetType("Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility");
                if (type2 != null)
                {
                    MethodInfo info2 = type2.GetMethod("RegisterModule");
                    if (info2 != null)
                    {
                        _registerModuleMethodFromReflection = Delegate.CreateDelegate(typeof(Action<Type>), info2) as Action<Type>;
                        return;
                    }
                }
            }
            _registerModuleMethodFromReflection = new Action<Type>(DynamicModuleUtility.RegisterModule);
        }

        private static void RegisterHystrixRoutes()
        {
            Route item = new Route("{controller}/{*action}", new HystrixRouteHandler());
            RouteValueDictionary dictionary = new RouteValueDictionary();
            dictionary.Add("controller", "__chystrix");
            item.Constraints = dictionary;
            RouteTable.Routes.Add("chystrix-rdkjsoa2", item);
        }

        private static void RegisterModule(Type type)
        {
            _registerModuleMethodFromReflection(type);
        }
    }
}

