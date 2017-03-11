namespace CHystrix
{
    using System;
    using System.Collections.Generic;

    internal interface ILog
    {
        void Log(LogLevelEnum level, string message);
        void Log(LogLevelEnum level, string message, Dictionary<string, string> tagInfo);
        void Log(LogLevelEnum level, string message, Exception ex);
        void Log(LogLevelEnum level, string message, Exception ex, Dictionary<string, string> tagInfo);
    }
}

