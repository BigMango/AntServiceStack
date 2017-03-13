using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AntServiceStack.Common;
using AntServiceStack.Common.ServiceModel;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;

namespace AntServiceStackSwagger.Extentions
{
    public static class AttributeExtensions
    {
        public static string GetDescription(this Type type)
        {
            var apiAttr = type.FirstAttribute<ApiAttribute>();
            if (apiAttr != null)
                return apiAttr.Description;

            var componentDescAttr = type.FirstAttribute<System.ComponentModel.DescriptionAttribute>();
            if (componentDescAttr != null)
                return componentDescAttr.Description;
            return string.Empty;
        }

        public static string GetDescription(this MemberInfo mi)
        {
            var apiAttr = mi.FirstAttribute<ApiMemberAttribute>();
            if (apiAttr != null)
                return apiAttr.Description;

            var componentDescAttr = mi.FirstAttribute<System.ComponentModel.DescriptionAttribute>();
            if (componentDescAttr != null)
                return componentDescAttr.Description;
            return string.Empty;
        }

        public static string GetDescription(this ParameterInfo pi)
        {
            var componentDescAttr = pi.FirstAttribute<System.ComponentModel.DescriptionAttribute>();
            if (componentDescAttr != null)
                return componentDescAttr.Description;
            return string.Empty;
        }
        public static bool IsNullableType(this Type type)
        {
            if (type.IsGenericType())
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return false;
        }
      

        public static string LeftPart(this string strVal, char needle)
        {
            if (strVal == null)
                return (string)null;
            int length = strVal.IndexOf(needle);
            if (length != -1)
                return strVal.Substring(0, length);
            return strVal;
        }

        public static Type FirstGenericType(this Type type)
        {
            for (; type != (Type)null; type = type.BaseType())
            {
                if (type.IsGeneric())
                    return type;
            }
            return (Type)null;
        }

        public static string ToPrettyName(this Type type)
        {
            if (!type.IsGenericType())
                return type.Name;

            var genericTypeName = type.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.LeftPart('`');
            var genericArgs = string.Join(",",
                type.GetGenericArguments()
                    .Select(ToPrettyName).ToArray());
            return genericTypeName + "<" + genericArgs + ">";
        }

        public static List<To> Map<To>(this System.Collections.IEnumerable items, Func<object, To> converter)
        {
            if (items == null)
                return new List<To>();

            var list = new List<To>();
            foreach (var item in items)
            {
                list.Add(converter(item));
            }
            return list;
        }

        public static string ToLowercaseUnderscoreNew(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            value = value.ToCamelCase();
            StringBuilder sb = PlatformExtensions.StringBuilderThreadStatic.Allocate();
            foreach (char c in value)
            {
                if (char.IsDigit(c) || char.IsLetter(c) && char.IsLower(c) || (int)c == 95)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("_");
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
            return PlatformExtensions.StringBuilderThreadStatic.ReturnAndFree(sb);
        }

        public static string ToCamelCaseNew(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            int length = value.Length;
            char[] chArray = new char[length];
            bool flag1 = true;
            for (int index = 0; index < length; ++index)
            {
                char ch1 = value[index];
                char ch2 = index < length - 1 ? value[index + 1] : 'A';
                bool flag2 = (int)ch1 >= 65 && (int)ch1 <= 90;
                bool flag3 = (int)ch2 >= 65 && (int)ch2 <= 90;
                if (flag1 & flag2 && (flag3 || index == 0))
                    ch1 += ' ';
                else
                    flag1 = false;
                chArray[index] = ch1;
            }
            return new string(chArray);
        }

      
    }

      
}
