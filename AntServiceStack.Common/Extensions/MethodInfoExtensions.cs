using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Freeway.Logging;

namespace AntServiceStack.Common.Extensions
{
    public static class MethodInfoExtensions
    {
        private static ILog log = LogManager.GetLogger(typeof(MethodInfoExtensions));
        /// <summary>
        /// 不允许为Null。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="instance">对象实例。</param>
        /// <param name="name">参数名称。</param>
        /// <returns>对象实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> 为null。</exception>
        public static T NotNull<T>(this T instance, string name) where T : class
        {
            if (instance == null)
                throw new ArgumentNullException(name.NotEmpty("name"));
            return instance;
        }
        /// <summary>
        /// 不允许空字符串。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <param name="name">参数名称。</param>
        /// <returns>字符串。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> 为空。</exception>
        public static string NotEmpty(this string str, string name)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(name.NotEmpty("name"));
            return str;
        }
        /// <summary>
        /// 不允许空和只包含空格的字符串。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <param name="name">参数名称。</param>
        /// <returns>字符串。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> 为空或者全为空格。</exception>
        public static string NotEmptyOrWhiteSpace(this string str, string name)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(name.NotEmpty("name"));
            return str;
        }
        private static Func<object, object[], object> CreateDelegate(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            // parameters to execute
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
            ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            List<Expression> parameterExpressions = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            Expression instanceCast = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }

        public static Func<object, object[], object> CreateDynamicInvoker(this MethodInfo methodInfo)
        {
            try
            {
                return methodInfo.CreateDelegate();
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to create delegate for method '{0}.{1}'!", methodInfo.DeclaringType.FullName, methodInfo.Name);
                log.Warn(message, ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300078" } });
            }

            return null;
        }

        internal static Func<object, object> CreateGetter(this PropertyInfo propertyInfo, bool nonPublic = false)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("properyInfo");

            var invoker = propertyInfo.GetGetMethod(nonPublic).CreateDynamicInvoker();
            if (invoker == null)
                return null;

            return instance => invoker(instance, null);
        }

        internal static Action<InstanceType, FieldType> CreateSetter<InstanceType, FieldType>(this FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            var instanceType = typeof(InstanceType);
            var fieldType = typeof(FieldType);

            var instanceParameterExpression = Expression.Parameter(instanceType, "instance");
            var parameterParameterExpression = Expression.Parameter(fieldType, "value");

            Expression instanceExpression, parameterExpression;

            if (instanceType == fieldInfo.DeclaringType)
                instanceExpression = instanceParameterExpression;
            else
                instanceExpression = Expression.Convert(instanceParameterExpression, fieldInfo.DeclaringType);

            if (fieldType == fieldInfo.FieldType)
                parameterExpression = parameterParameterExpression;
            else
                parameterExpression = Expression.Convert(parameterParameterExpression, fieldInfo.FieldType);

            var memberExpression = Expression.Field(instanceExpression, fieldInfo);
            var assignExpression = Expression.Assign(memberExpression, parameterExpression);
            var lambda = Expression.Lambda<Action<InstanceType, FieldType>>(assignExpression, instanceParameterExpression, parameterParameterExpression);
            return lambda.Compile();
        }
    }
}
