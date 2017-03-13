using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Support;
using AntServiceStack.Text;

namespace AntServiceStackSwagger.Extentions
{
    internal static class TypeConverter
    {
        public static PropertyGetterDelegate CreateTypeConverter(Type fromType, Type toType)
        {
            if (fromType == toType)
                return (PropertyGetterDelegate)null;
            if (fromType == typeof(string))
                return (PropertyGetterDelegate)(fromValue => TypeSerializer.DeserializeFromString((string)fromValue, toType));
            if (toType == typeof(string))
                return new PropertyGetterDelegate(TypeSerializer.SerializeToString<object>);
            if (toType.IsEnum() || fromType.IsEnum())
            {
                if (toType.IsEnum() && fromType.IsEnum())
                    return (PropertyGetterDelegate)(fromValue => Enum.Parse(toType, fromValue.ToString(), true));
                if (toType.IsNullableType())
                {
                    Type genericArg = toType.GenericTypeArguments()[0];
                    if (genericArg.IsEnum())
                        return (PropertyGetterDelegate)(fromValue => Enum.ToObject(genericArg, fromValue));
                }
                else if (toType.IsIntegerType())
                {
                    if (fromType.IsNullableType())
                    {
                        Type genericArg = fromType.GenericTypeArguments()[0];
                        if (genericArg.IsEnum())
                            return (PropertyGetterDelegate)(fromValue => Enum.ToObject(genericArg, fromValue));
                    }
                    return (PropertyGetterDelegate)(fromValue => Enum.ToObject(fromType, fromValue));
                }
                return (PropertyGetterDelegate)null;
            }
            if (toType.IsNullableType())
            {
                Type toTypeBaseType = toType.GenericTypeArguments()[0];
                if (toTypeBaseType.IsEnum() && (fromType.IsEnum() || fromType.IsNullableType() && fromType.GenericTypeArguments()[0].IsEnum()))
                    return (PropertyGetterDelegate)(fromValue => Enum.ToObject(toTypeBaseType, fromValue));
                return (PropertyGetterDelegate)null;
            }
            if (typeof(IEnumerable).IsAssignableFromType(fromType))
                return (PropertyGetterDelegate)(fromValue => TranslateListWithElements.TryTranslateCollections(fromType, toType, fromValue) ?? fromValue);
            if (toType.IsValueType())
                return (PropertyGetterDelegate)(fromValue => Convert.ChangeType(fromValue, toType, (IFormatProvider)null));
            return (PropertyGetterDelegate)(fromValue =>
            {
                if (fromValue == null)
                    return fromValue;
                object instance = toType.CreateInstance();
                object from = fromValue;
                instance.PopulateWith<object, object>(from);
                return instance;
            });
        }
    }
}
