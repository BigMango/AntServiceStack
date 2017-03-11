//-----------------------------------------------------------------------
// <copyright file="CommonLogger.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Freeway.Logging.Impl
{
   
    internal class CommonLogger : ILog
    {
        private string _logName = string.Empty;
        

        static CommonLogger()
        {
        }

        public CommonLogger(string logName)
        {
            if (string.IsNullOrEmpty(logName) || logName.Trim().Length == 0)
                this._logName = "defaultLogName";
            else
                this._logName = logName;
        }

        private void WriteLog(LogLevel logLevel, string title, string message, Exception exception, Dictionary<string, string> attrs)
        {
            this.WriteLog(logLevel, title, message, exception, attrs, DateTime.UtcNow);
        }

        private void WriteLog(LogLevel logLevel, string title, string message, Exception exception, Dictionary<string, string> attrs, DateTime time)
        {
            Trace.WriteLine(title??string.Empty + Environment.NewLine + (message ?? string.Empty) + Environment.NewLine + exception?.Message );
        }

        public void Debug(string title, string message)
        {
            this.WriteLog(LogLevel.DEBUG, title, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Debug(string title, Exception exception)
        {
            this.WriteLog(LogLevel.DEBUG, title, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Debug(string title, string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.DEBUG, title, message, (Exception)null, addInfo);
        }

        public void Debug(string title, Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.DEBUG, title, (string)null, exception, addInfo);
        }

        public void Debug(string message)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Debug(Exception exception)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Debug(string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, message, (Exception)null, addInfo);
        }

        public void Debug(Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, (string)null, exception, addInfo);
        }

        public void Error(string title, string message)
        {
            this.WriteLog(LogLevel.ERROR, title, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Error(string title, Exception exception)
        {
            this.WriteLog(LogLevel.ERROR, title, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Error(string title, string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.ERROR, title, message, (Exception)null, addInfo);
        }

        public void Error(string title, Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.ERROR, title, (string)null, exception, addInfo);
        }

        public void Error(string message)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Error(Exception exception)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Error(string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, message, (Exception)null, addInfo);
        }

        public void Error(Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, (string)null, exception, addInfo);
        }

        public void Fatal(string title, string message)
        {
            this.WriteLog(LogLevel.FATAL, title, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Fatal(string title, Exception exception)
        {
            this.WriteLog(LogLevel.FATAL, title, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Fatal(string title, string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.FATAL, title, message, (Exception)null, addInfo);
        }

        public void Fatal(string title, Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.FATAL, title, (string)null, exception, addInfo);
        }

        public void Fatal(string message)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Fatal(Exception exception)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Fatal(string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, message, (Exception)null, addInfo);
        }

        public void Fatal(Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, (string)null, exception, addInfo);
        }

        public void Info(string title, string message)
        {
            this.WriteLog(LogLevel.INFO, title, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Info(string title, Exception exception)
        {
            this.WriteLog(LogLevel.INFO, title, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Info(string title, string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.INFO, title, message, (Exception)null, addInfo);
        }

        public void Info(string title, Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.INFO, title, (string)null, exception, addInfo);
        }

        public void Info(string message)
        {
            this.WriteLog(LogLevel.INFO, (string)null, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Info(Exception exception)
        {
            this.WriteLog(LogLevel.INFO, (string)null, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Info(string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.INFO, (string)null, message, (Exception)null, addInfo);
        }

        public void Info(Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.INFO, (string)null, (string)null, exception, addInfo);
        }

        public void Warn(string title, string message)
        {
            this.WriteLog(LogLevel.WARN, title, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Warn(string title, Exception exception)
        {
            this.WriteLog(LogLevel.WARN, title, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Warn(string title, string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.WARN, title, message, (Exception)null, addInfo);
        }

        public void Warn(string title, Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.WARN, title, (string)null, exception, addInfo);
        }

        public void Warn(string message)
        {
            this.WriteLog(LogLevel.WARN, (string)null, message, (Exception)null, (Dictionary<string, string>)null);
        }

        public void Warn(Exception exception)
        {
            this.WriteLog(LogLevel.WARN, (string)null, (string)null, exception, (Dictionary<string, string>)null);
        }

        public void Warn(string message, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.WARN, (string)null, message, (Exception)null, addInfo);
        }

        public void Warn(Exception exception, Dictionary<string, string> addInfo)
        {
            this.WriteLog(LogLevel.WARN, (string)null, (string)null, exception, addInfo);
        }

        public void Debug(string title, string message, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, title, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Debug(string title, Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, title, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Debug(string title, string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, title, message, (Exception)null, addInfo, time);
        }

        public void Debug(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, title, (string)null, exception, addInfo, time);
        }

        public void Debug(string message, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Debug(Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Debug(string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, message, (Exception)null, addInfo, time);
        }

        public void Debug(Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.DEBUG, (string)null, (string)null, exception, addInfo, time);
        }

        public void Error(string title, string message, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, title, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Error(string title, Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, title, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Error(string title, string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, title, message, (Exception)null, addInfo, time);
        }

        public void Error(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, title, (string)null, exception, addInfo, time);
        }

        public void Error(string message, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Error(Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Error(string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, message, (Exception)null, addInfo, time);
        }

        public void Error(Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.ERROR, (string)null, (string)null, exception, addInfo, time);
        }

        public void Fatal(string title, string message, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, title, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Fatal(string title, Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, title, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Fatal(string title, string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, title, message, (Exception)null, addInfo, time);
        }

        public void Fatal(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, title, (string)null, exception, addInfo, time);
        }

        public void Fatal(string message, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Fatal(Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Fatal(string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, message, (Exception)null, addInfo, time);
        }

        public void Fatal(Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.FATAL, (string)null, (string)null, exception, addInfo, time);
        }

        public void Info(string title, string message, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, title, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Info(string title, Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, title, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Info(string title, string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, title, message, (Exception)null, addInfo, time);
        }

        public void Info(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, title, (string)null, exception, addInfo, time);
        }

        public void Info(string message, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, (string)null, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Info(Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, (string)null, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Info(string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, (string)null, message, (Exception)null, addInfo, time);
        }

        public void Info(Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.INFO, (string)null, (string)null, exception, addInfo, time);
        }

        public void Warn(string title, string message, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, title, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Warn(string title, Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, title, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Warn(string title, string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, title, message, (Exception)null, addInfo, time);
        }

        public void Warn(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, title, (string)null, exception, addInfo, time);
        }

        public void Warn(string message, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, (string)null, message, (Exception)null, (Dictionary<string, string>)null, time);
        }

        public void Warn(Exception exception, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, (string)null, (string)null, exception, (Dictionary<string, string>)null, time);
        }

        public void Warn(string message, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, (string)null, message, (Exception)null, addInfo, time);
        }

        public void Warn(Exception exception, Dictionary<string, string> addInfo, DateTime time)
        {
            this.WriteLog(LogLevel.WARN, (string)null, (string)null, exception, addInfo, time);
        }

        public IDictionary<Exception, long> GetExceptionCount()
        {
            throw new NotImplementedException();
        }
    }
}