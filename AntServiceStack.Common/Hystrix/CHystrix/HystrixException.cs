namespace CHystrix
{
    using System;
    using System.Runtime.CompilerServices;

    public class HystrixException : Exception
    {
        internal HystrixException(FailureTypeEnum failureType, Type commandType, string commandKey, string message) : this(failureType, commandType, commandKey, message, null, null)
        {
        }

        internal HystrixException(FailureTypeEnum failureType, Type commandType, string commandKey, string message, Exception cause) : this(failureType, commandType, commandKey, message, cause, null)
        {
        }

        internal HystrixException(FailureTypeEnum failureType, Type commandType, string commandKey, string message, Exception cause, Exception fallbackException) : base(message, cause)
        {
            this.FailureType = failureType;
            this.CommandKey = commandKey;
            this.CommandType = commandType;
            this.ExecutionException = cause;
            this.FallbackException = fallbackException;
        }

        public string CommandKey { get; private set; }

        public Type CommandType { get; private set; }

        public Exception ExecutionException { get; private set; }

        public FailureTypeEnum FailureType { get; private set; }

        public Exception FallbackException { get; private set; }
    }
}

