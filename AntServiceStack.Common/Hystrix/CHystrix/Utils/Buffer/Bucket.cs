namespace CHystrix.Utils.Buffer
{
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class Bucket
    {
        protected Bucket(long timeInMilliseconds)
        {
            this.TimeInMilliseconds = timeInMilliseconds;
        }

        public long TimeInMilliseconds { get; protected set; }
    }
}

