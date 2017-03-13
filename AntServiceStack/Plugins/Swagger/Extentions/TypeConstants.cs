using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AntServiceStackSwagger.Extentions
{
    public static class TypeConstants
    {
        public static readonly object EmptyObject = new object();
        public static readonly string[] EmptyStringArray = new string[0];
        public static readonly long[] EmptyLongArray = new long[0];
        public static readonly int[] EmptyIntArray = new int[0];
        public static readonly char[] EmptyCharArray = new char[0];
        public static readonly bool[] EmptyBoolArray = new bool[0];
        public static readonly byte[] EmptyByteArray = new byte[0];
        public static readonly object[] EmptyObjectArray = new object[0];
        public static readonly Type[] EmptyTypeArray = new Type[0];
        public static readonly FieldInfo[] EmptyFieldInfoArray = new FieldInfo[0];
        public static readonly PropertyInfo[] EmptyPropertyInfoArray = new PropertyInfo[0];
        public static readonly byte[][] EmptyByteArrayArray = new byte[0][];
        public static readonly Dictionary<string, string> EmptyStringDictionary = new Dictionary<string, string>(0);
        public static readonly List<string> EmptyStringList = new List<string>(0);
        public static readonly List<long> EmptyLongList = new List<long>(0);
        public static readonly List<int> EmptyIntList = new List<int>(0);
        public static readonly List<char> EmptyCharList = new List<char>(0);
        public static readonly List<bool> EmptyBoolList = new List<bool>(0);
        public static readonly List<byte> EmptyByteList = new List<byte>(0);
        public static readonly List<object> EmptyObjectList = new List<object>(0);
        public static readonly List<Type> EmptyTypeList = new List<Type>(0);
        public static readonly List<FieldInfo> EmptyFieldInfoList = new List<FieldInfo>(0);
        public static readonly List<PropertyInfo> EmptyPropertyInfoList = new List<PropertyInfo>(0);
        public static readonly Task<int> ZeroTask = 0.InTask<int>();
        public static readonly Task<bool> TrueTask = true.InTask<bool>();
        public static readonly Task<bool> FalseTask = false.InTask<bool>();
        public static readonly Task<object> EmptyTask = ((object)null).InTask<object>();

        private static Task<T> InTask<T>(this T result)
        {
            TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();
            T result1 = result;
            completionSource.SetResult(result1);
            return completionSource.Task;
        }
    }
}
