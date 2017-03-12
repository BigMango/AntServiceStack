using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CTrip.Tools.SOA.Util
{
    public static class ValidationHelper
    {
        public static bool IsWindowsFileName(string fileName)
        {
            const string validWindowsFileNamePattern =
                @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            return Regex.IsMatch(fileName, validWindowsFileNamePattern);
        }

        public static bool IsDotNetNamespace(string namespaceName)
        {
            const string validDotNetNamespaceIdentifierPattern =
                @"^(?:(?:((?![^_\p{L}\p{Nl}])[\p{L}\p{Mn}\p{Mc}\p{Nd}\p{Nl}\p{Pc}\p{Cf}]+)\u002E?)+)(?<!\u002E)$";
            return Regex.IsMatch(namespaceName, validDotNetNamespaceIdentifierPattern);
        }

        public static bool IsIdentifier(string identifier)
        {
            const string validIdentifierPattern = "^(\\s*([a-zA-Z_][0-9a-zA-Z_]*\\.)*[a-zA-Z_][0-9a-zA-Z_]*\\s*)(,\\s*(\\s*([a-zA-Z_][0-9a-zA-Z_]*\\.)*[a-zA-Z_][0-9a-zA-Z_]*\\s*))*$";
            return Regex.IsMatch(identifier, validIdentifierPattern);
        }
    }
}
