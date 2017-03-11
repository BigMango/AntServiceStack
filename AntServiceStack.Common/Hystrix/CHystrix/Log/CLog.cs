namespace CHystrix.Log
{
    using CHystrix;
    using Freeway.Logging;
    using System;
    using System.Collections.Generic;

    internal class CLog : CHystrix.ILog
    {
        protected readonly ICommandConfigSet ConfigSet;
        protected readonly Freeway.Logging.ILog Logger;

        public CLog(Type type) : this(null, type)
        {
        }

        public CLog(ICommandConfigSet configSet, Type type)
        {
            try
            {
                this.ConfigSet = configSet;
                this.Logger = LogManager.GetLogger(type);
            }
            catch
            {
            }
        }

        protected void DegradeLogLevelIfNeeded(ref LogLevelEnum level)
        {
            if ((this.ConfigSet != null) && this.ConfigSet.DegradeLogLevel)
            {
                switch (level)
                {
                    case LogLevelEnum.Warning:
                        level = LogLevelEnum.Info;
                        return;

                    case LogLevelEnum.Error:
                        level = LogLevelEnum.Warning;
                        return;

                    case LogLevelEnum.Fatal:
                        level = LogLevelEnum.Error;
                        return;
                }
            }
        }

        public void Log(LogLevelEnum level, string message)
        {
            try
            {
                this.DegradeLogLevelIfNeeded(ref level);
                switch (level)
                {
                    case LogLevelEnum.Info:
                        this.Logger.Info(message);
                        return;

                    case LogLevelEnum.Warning:
                        this.Logger.Warn(message);
                        return;

                    case LogLevelEnum.Error:
                        this.Logger.Error(message);
                        return;

                    case LogLevelEnum.Fatal:
                        this.Logger.Fatal(message);
                        return;
                }
            }
            catch
            {
            }
        }

        public void Log(LogLevelEnum level, string message, Exception ex)
        {
            try
            {
                this.DegradeLogLevelIfNeeded(ref level);
                switch (level)
                {
                    case LogLevelEnum.Info:
                        this.Logger.Info(message, ex);
                        return;

                    case LogLevelEnum.Warning:
                        this.Logger.Warn(message, ex);
                        return;

                    case LogLevelEnum.Error:
                        this.Logger.Error(message, ex);
                        return;

                    case LogLevelEnum.Fatal:
                        this.Logger.Fatal(message, ex);
                        return;
                }
            }
            catch
            {
            }
        }

        public void Log(LogLevelEnum level, string message, Dictionary<string, string> tagInfo)
        {
            try
            {
                this.DegradeLogLevelIfNeeded(ref level);
                switch (level)
                {
                    case LogLevelEnum.Info:
                        this.Logger.Info(message, tagInfo);
                        return;

                    case LogLevelEnum.Warning:
                        this.Logger.Warn(message, tagInfo);
                        return;

                    case LogLevelEnum.Error:
                        this.Logger.Error(message, tagInfo);
                        return;

                    case LogLevelEnum.Fatal:
                        this.Logger.Fatal(message, tagInfo);
                        return;
                }
            }
            catch
            {
            }
        }

        public void Log(LogLevelEnum level, string message, Exception ex, Dictionary<string, string> tagInfo)
        {
            try
            {
                this.DegradeLogLevelIfNeeded(ref level);
                switch (level)
                {
                    case LogLevelEnum.Info:
                        this.Logger.Info(message, ex, tagInfo);
                        return;

                    case LogLevelEnum.Warning:
                        this.Logger.Warn(message, ex, tagInfo);
                        return;

                    case LogLevelEnum.Error:
                        this.Logger.Error(message, ex, tagInfo);
                        return;

                    case LogLevelEnum.Fatal:
                        this.Logger.Fatal(message, ex, tagInfo);
                        return;
                }
            }
            catch
            {
            }
        }
    }
}

