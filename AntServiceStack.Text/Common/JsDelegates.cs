
using System;
using System.Collections.Generic;
using System.IO;

namespace AntServiceStack.Text.Common
{
    internal delegate void WriteListDelegate(TextWriter writer, object oList, WriteObjectDelegate toStringFn);

    internal delegate void WriteGenericListDelegate<T>(TextWriter writer, IList<T> list, WriteObjectDelegate toStringFn);

    internal delegate void WriteDelegate(TextWriter writer, object value);

    internal delegate ParseStringDelegate ParseFactoryDelegate();

    public delegate void WriteObjectDelegate(TextWriter writer, object obj);

    public delegate void SetPropertyDelegate(object instance, object propertyValue);

    public delegate object ParseStringDelegate(string stringValue);

    public delegate object ConvertObjectDelegate(object fromObject);

    public delegate object ConvertInstanceDelegate(object obj, Type type);
}
