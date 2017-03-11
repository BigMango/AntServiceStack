//-----------------------------------------------------------------------
// <copyright file="LogManager.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Freeway.Logging.Impl;

namespace Freeway.Logging
{
    public sealed class LogManager
    {
        private static Dictionary<string, ILog> _logs = new Dictionary<string, ILog>();
        private static object lockObject = new object();

        private LogManager()
        {
        }

        public static ILog GetLogger(Type type)
        {
            if (type == (Type)null)
                return LogManager.GetLogger("NoName");
            return LogManager.GetLogger(type.FullName);
        }

        public static ILog GetLogger(string name)
        {
            string str = name;
            if (string.IsNullOrEmpty(name) || name.Trim().Length == 0)
                str = "defaultLogger";
            ILog log;
            if (!LogManager._logs.TryGetValue(str, out log))
            {
                lock (LogManager.lockObject)
                {
                    if (!LogManager._logs.TryGetValue(str, out log))
                    {
                        log = (ILog)new CommonLogger(str);
                        LogManager._logs = new Dictionary<string, ILog>((IDictionary<string, ILog>)LogManager._logs)
                        {
                          {
                            str,
                            log
                          }
                        };
                    }
                }
            }
            return log;
        }
    }
}