using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using AntServiceStack.Manager.Common;

namespace AntServiceStack.Manager.SignalR
{
    public class DebugLogger: IHubLogger
    {
        public void Error(string title, string msg, Exception exception)
        {
            this.Info(title,msg,exception);
        }

        public void Error(string title, Exception exception)
        {
            this.Info(title, null,exception);
        }

        public void Error(string title, string msg)
        {
            this.Info(title, msg,null);
        }

        public void Warn(string title, string msg)
        {
            this.Info(title, msg,null);
        }

        public void Warn(string title, Exception exception)
        {
            this.Info(title,null, exception);
        }

        public void Warn(string title, string msg, Exception exception)
        {
            this.Info(title, msg,exception);
        }

        public void Info(string title, string msg)
        {
            this.Info(title, msg, null);
        }

        public void Info(string title, Exception exception)
        {
            this.Info(title, null, exception);
        }

        public void Info(string title, string msg, Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(title))
            {
                sb.AppendLine("title:" + title);
            }
            if (!string.IsNullOrEmpty(msg))
            {
                sb.AppendLine("msg:" + msg);
            }
            if (exception != null)
            {
                sb.AppendLine("exception:" + exception.ToString());
            }
            LogUtil.WriteInfoLog(sb.ToString());
        }
    }
}