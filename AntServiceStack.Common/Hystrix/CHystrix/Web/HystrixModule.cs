namespace CHystrix.Web
{
    using CHystrix;
    using CHystrix.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web;

    internal class HystrixModule : IHttpModule
    {
        private static object Lock = new object();

        private void Context_BeginRequestHandler(object sender, EventArgs e)
        {
            try
            {
                if (!InitSuccess.HasValue)
                {
                    HttpApplication application = sender as HttpApplication;
                    if ((application != null) && Monitor.TryEnter(Lock))
                    {
                        try
                        {
                            if (!InitSuccess.HasValue)
                            {
                                Uri url = application.Context.Request.Url;
                                string applicationPath = application.Context.Request.ApplicationPath;
                                HystrixCommandBase.ApplicationPath = url.Scheme + "://" + url.Authority + applicationPath;
                                InitSuccess = true;
                            }
                        }
                        catch
                        {
                            InitSuccess = false;
                            throw;
                        }
                        finally
                        {
                            Monitor.Exit(Lock);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Fatal, "Failed to init web host info.", exception, new Dictionary<string, string>().AddLogTagData("FXD303026"));
            }
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(this.Context_BeginRequestHandler);
        }

        public static bool? InitSuccess
        {
            get; set;
        }
    }
}

