using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freeway.Logging
{
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
    }
    public interface ILog
    {
        void Debug(string title, string message);

        void Debug(string title, Exception exception);

        void Debug(string title, string message, Dictionary<string, string> addInfo);

        void Debug(string title, Exception exception, Dictionary<string, string> addInfo);

        void Debug(string message);

        void Debug(Exception exception);

        void Debug(string message, Dictionary<string, string> addInfo);

        void Debug(Exception exception, Dictionary<string, string> addInfo);

        void Error(string title, string message);

        void Error(string title, Exception exception);

        void Error(string title, string message, Dictionary<string, string> addInfo);

        void Error(string title, Exception exception, Dictionary<string, string> addInfo);

        void Error(string message);

        void Error(Exception exception);

        void Error(string message, Dictionary<string, string> addInfo);

        void Error(Exception exception, Dictionary<string, string> addInfo);

        void Fatal(string title, string message);

        void Fatal(string title, Exception exception);

        void Fatal(string title, string message, Dictionary<string, string> addInfo);

        void Fatal(string title, Exception exception, Dictionary<string, string> addInfo);

        void Fatal(string message);

        void Fatal(Exception exception);

        void Fatal(string message, Dictionary<string, string> addInfo);

        void Fatal(Exception exception, Dictionary<string, string> addInfo);

        void Info(string title, string message);

        void Info(string title, Exception exception);

        void Info(string title, string message, Dictionary<string, string> addInfo);

        void Info(string title, Exception exception, Dictionary<string, string> addInfo);

        void Info(string message);

        void Info(Exception exception);

        void Info(string message, Dictionary<string, string> addInfo);

        void Info(Exception exception, Dictionary<string, string> addInfo);

        void Warn(string title, string message);

        void Warn(string title, Exception exception);

        void Warn(string title, string message, Dictionary<string, string> addInfo);

        void Warn(string title, Exception exception, Dictionary<string, string> addInfo);

        void Warn(string message);

        void Warn(Exception exception);

        void Warn(string message, Dictionary<string, string> addInfo);

        void Warn(Exception exception, Dictionary<string, string> addInfo);

        void Debug(string title, string message, DateTime time);

        void Debug(string title, Exception exception, DateTime time);

        void Debug(string title, string message, Dictionary<string, string> addInfo, DateTime time);

        void Debug(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Debug(string message, DateTime time);

        void Debug(Exception exception, DateTime time);

        void Debug(string message, Dictionary<string, string> addInfo, DateTime time);

        void Debug(Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Error(string title, string message, DateTime time);

        void Error(string title, Exception exception, DateTime time);

        void Error(string title, string message, Dictionary<string, string> addInfo, DateTime time);

        void Error(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Error(string message, DateTime time);

        void Error(Exception exception, DateTime time);

        void Error(string message, Dictionary<string, string> addInfo, DateTime time);

        void Error(Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Fatal(string title, string message, DateTime time);

        void Fatal(string title, Exception exception, DateTime time);

        void Fatal(string title, string message, Dictionary<string, string> addInfo, DateTime time);

        void Fatal(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Fatal(string message, DateTime time);

        void Fatal(Exception exception, DateTime time);

        void Fatal(string message, Dictionary<string, string> addInfo, DateTime time);

        void Fatal(Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Info(string title, string message, DateTime time);

        void Info(string title, Exception exception, DateTime time);

        void Info(string title, string message, Dictionary<string, string> addInfo, DateTime time);

        void Info(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Info(string message, DateTime time);

        void Info(Exception exception, DateTime time);

        void Info(string message, Dictionary<string, string> addInfo, DateTime time);

        void Info(Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Warn(string title, string message, DateTime time);

        void Warn(string title, Exception exception, DateTime time);

        void Warn(string title, string message, Dictionary<string, string> addInfo, DateTime time);

        void Warn(string title, Exception exception, Dictionary<string, string> addInfo, DateTime time);

        void Warn(string message, DateTime time);

        void Warn(Exception exception, DateTime time);

        void Warn(string message, Dictionary<string, string> addInfo, DateTime time);

        void Warn(Exception exception, Dictionary<string, string> addInfo, DateTime time);

        //ICallCountInfo GetCallCount();

        IDictionary<Exception, long> GetExceptionCount();
    }
}
