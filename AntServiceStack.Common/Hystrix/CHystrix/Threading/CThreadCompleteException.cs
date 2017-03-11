namespace CHystrix.Threading
{
    using System;

    internal class CThreadCompleteException : Exception
    {
        private static readonly string msg = "some error happend when thread finish the work";

        public CThreadCompleteException(Exception innerException) : base(msg, innerException)
        {
        }
    }
}

