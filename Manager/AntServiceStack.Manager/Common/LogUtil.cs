using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace AntServiceStack.Manager.Common
{
    public class LogUtil
    {
        /// <summary>
        /// 日志器 级别 INFO
        /// </summary>
        private static readonly log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");

        /// <summary>
        /// 日志器 级别 ERROR
        /// </summary>
        private static readonly log4net.ILog Logerror = log4net.LogManager.GetLogger("logerror");

      
        /// <summary>
        /// 发送DEBUG 级别
        /// </summary>
        private static readonly log4net.ILog Logdebug = log4net.LogManager.GetLogger("logdebug");



        /// <summary>
        /// 设置配置器入口
        /// </summary>
        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// 设置配置器
        /// </summary>
        /// <param name="configFile"></param>
        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteErrorLog(string msg)
        {
            WriteErrorLog(msg, null);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteErrorLog(System.Exception ex)
        {
            WriteErrorLog(string.Empty, ex);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void WriteErrorLog(string msg, System.Exception ex)
        {
            string methodName = GetMethodName((ex != null && !string.IsNullOrEmpty(msg)));

            StringBuilder temp = new StringBuilder();

            temp.Append(methodName);
            temp.Append(" ");
            temp.Append(msg);
            temp.Append(ex != null
                ? string.Format("{0}{1}{2}{3}", " 异常：", ex.Message, " ", (ex.InnerException != null
                                                    ? string.Format("{0}{1}", " 内部异常：", ex.InnerException.Message)
                                                    : ""))
                : "");

            msg = temp.ToString();

            WriteLog(msg, ex);
        }

        /// <summary>
        /// 得到调用方法名
        /// </summary>
        /// <param name="level">级别，true为2，false为3</param>
        /// <returns></returns>
        public static string GetMethodName(bool level)
        {

            var method = new StackFrame(level ? 2 : 3).GetMethod(); // 这里忽略2-3（看实际层级）层堆栈，也就忽略了当前方法GetMethodName，这样拿到的就正好是外部调用GetMethodName的方法信息
            var property = (
                      from p in method.DeclaringType.GetProperties(
                               BindingFlags.Instance |
                               BindingFlags.Static |
                               BindingFlags.Public |
                               BindingFlags.NonPublic)
                      where p.GetGetMethod(true) == method || p.GetSetMethod(true) == method
                      select p).FirstOrDefault();
            return property == null ? method.Name : property.Name;
        }

      

        /// <summary>
        /// 写Info日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteInfoLog(string msg)
        {
            if (LogUtil.Loginfo.IsInfoEnabled)
            {
                Loginfo.Info(msg);
#if DEBUG
                System.Diagnostics.Trace.WriteLine(msg);
#endif
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(msg);
            }
        }

        /// <summary>
        /// 写debug日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteDebugLog(string msg)
        {
#if DEBUG
            StringBuilder temp = new StringBuilder();
            temp.Append(msg);
            temp.Append(Environment.NewLine);
            temp.Append("【Stack Message:】");
            temp.Append(Environment.NewLine);
            temp.Append(new StackTrace().ToString());
            msg = temp.ToString();

            if (LogUtil.Logdebug.IsDebugEnabled)
            {
                Logdebug.Debug(msg);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(msg);
            }
#endif
        }


        #region private

      

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        private static void WriteLog(string info, System.Exception se)
        {
          

            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info, se);
            }
        }

        #endregion
    }
}