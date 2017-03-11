using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace AntServiceStack.Client.CHystrix
{
    internal static partial class CHystrixIntegration
    {
        const string CHystrixRegisterCustomBadRequestExceptionCheckerMethodName = "RegisterCustomBadRequestExceptionChecker";

        private static Action<string, Func<Exception, bool>> _registerCustomBadRequestExceptionChecker;

        public static bool HasCustomBadRequestExceptionSupport { get; private set; }

        private static void InitRegisterCustomBadRequestExceptionMethod(Type hystrixCommandType)
        {
            try
            {
                MethodInfo method = hystrixCommandType.GetMethod(CHystrixRegisterCustomBadRequestExceptionCheckerMethodName);
                if (method == null)
                {
                    Log.Info("The CHystrix.dll version is low. No custom bad request exception support in it.",
                        new Dictionary<string, string>() { { "ErrorCode", "FXD301019" } });
                    return;
                }

                _registerCustomBadRequestExceptionChecker = MakeRegisterCustomBadRequestExceptionCheckerDelegate(hystrixCommandType);
                if (_registerCustomBadRequestExceptionChecker == null)
                {
                    Log.Info("The CHystrix.dll version is low. No required custom bad request exception support method was made.",
                        new Dictionary<string, string>() { { "ErrorCode", "FXD301020" } });
                    return;
                }

                HasCustomBadRequestExceptionSupport = true;
                Log.Info("Successfully inited CHystrix custom bad request exception support.");
            }
            catch (Exception ex)
            {
                Log.Info("The CHystrix.dll version is low. Failed to init CHystrix custom bad request exception support.", ex,
                    new Dictionary<string, string>() { { "ErrorCode", "FXD301021" } });
            }
        }

        private static Action<string, Func<Exception, bool>> MakeRegisterCustomBadRequestExceptionCheckerDelegate(Type hystrixCommandType)
        {
            Type stringType = typeof(string);
            Type funcType = typeof(Func<Exception, bool>);
            var nameParameter = Expression.Parameter(stringType, "name");
            var isBadRequestExceptionDelegateParameter = Expression.Parameter(funcType, "isBadRequestExceptionDelegate");
            Expression callMethod = Expression.Call(
                hystrixCommandType,
                CHystrixRegisterCustomBadRequestExceptionCheckerMethodName,
                new Type[] { },
                new Expression[]
                {
                    Expression.Convert(nameParameter, stringType),
                    Expression.Convert(isBadRequestExceptionDelegateParameter, funcType),
                });
            return Expression.Lambda<Action<string, Func<Exception, bool>>>(callMethod, nameParameter, isBadRequestExceptionDelegateParameter).Compile();
        }
    }
}
