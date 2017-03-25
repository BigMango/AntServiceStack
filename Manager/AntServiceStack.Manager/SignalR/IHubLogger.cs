using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntServiceStack.Manager.SignalR
{
    public interface IHubLogger
    {
        void Error(string title, string msg, Exception exception);
        void Error(string title, Exception exception);
        void Error(string title, string msg);

        void Warn(string title, string msg);
        void Warn(string title, Exception exception);
        void Warn(string title, string msg, Exception exception);

        void Info(string title, string msg);
        void Info(string title, Exception exception);
        void Info(string title, string msg, Exception exception);
    }
}