﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AntServiceStack.Common.Web;
using AntServiceStack.Text;

namespace AntServiceStack.ServiceHost
{
    public static class HttpRequestAuthentication
    {
        public static string GetBasicAuth(this IHttpRequest httpReq)
        {
            var auth = httpReq.Headers[HttpHeaders.Authorization];
            if (auth == null) return null;

            var parts = auth.Split(' ');
            if (parts.Length != 2) return null;
            return parts[0].ToLower() == "basic" ? parts[1] : null;
        }

        public static KeyValuePair<string, string>? GetBasicAuthUserAndPassword(this IHttpRequest httpReq)
        {
            var userPassBase64 = httpReq.GetBasicAuth();
            if (userPassBase64 == null) return null;
            var userPass = Encoding.UTF8.GetString(Convert.FromBase64String(userPassBase64));
            var parts = userPass.SplitOnFirst(':');
            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }
        public static Dictionary<string, string> GetDigestAuth(this IHttpRequest httpReq)
        {
            var auth = httpReq.Headers[HttpHeaders.Authorization];
            if (auth == null) return null;
            var parts = auth.Split(' ');
            // There should be at least to parts
            if (parts.Length < 2) return null;
            // It has to be a digest request
            if (parts[0].ToLower() != "digest") return null;
            // Remove uptil the first space
            auth = auth.Substring(auth.IndexOf(' '));

            int i = 0;
            int line = 0;
            bool inQuotes = false;
            bool escape = false;

            var prts = new List<String> { "" };
            auth = auth.Trim(new char[2] { ' ', ',' });
            while (i < auth.Length)
            {

                if (auth[i] == '"' && !escape)
                    inQuotes = !inQuotes;

                if (auth[i] == ',' && !inQuotes && !escape)
                {
                    i++;
                    prts.Add("");
                    line++;
                }
                else
                {
                    escape = auth[i] == '\\';
                    prts[line] += auth[i];
                    i++;
                }
            }

            parts = prts.ToArray();

            try
            {
                var result = new Dictionary<string, string>();
                foreach (var item in parts)
                {
                    var param = item.Trim().Split(new char[] { '=' }, 2);
                    result.Add(param[0], param[1].Trim(new char[] { '"' }));
                }
                result.Add("method", httpReq.HttpMethod);
                result.Add("userhostaddress", httpReq.UserHostAddress);
                return result;
            }
            catch (Exception)
            {
            }
            return null;
        }
        public static string GetCookieValue(this IHttpRequest httpReq, string cookieName)
        {
            Cookie cookie;
            httpReq.Cookies.TryGetValue(cookieName, out cookie);
            return cookie != null ? cookie.Value : null;
        }

        public static string GetItemStringValue(this IHttpRequest httpReq, string itemName)
        {
            object val;
            if (!httpReq.Items.TryGetValue(itemName, out val)) return null;
            return val as string;
        }

    }
}
