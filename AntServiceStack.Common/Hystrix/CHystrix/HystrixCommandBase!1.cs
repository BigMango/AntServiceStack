namespace CHystrix
{
    using System;

    public abstract class HystrixCommandBase<T> : HystrixCommandBase
    {
        private bool _hasFallback;

        internal HystrixCommandBase() : this(null, null, null, null)
        {
        }

        internal HystrixCommandBase(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config) : this(null, commandKey, groupKey, domain, config)
        {
        }

        internal HystrixCommandBase(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config) : base(instanceKey, commandKey, groupKey, domain, config)
        {
            this._hasFallback = this is IFallback<T>;
        }

        protected abstract T Execute();
        internal IFallback<T> ToIFallback()
        {
            return (this as IFallback<T>);
        }

        internal virtual bool HasFallback
        {
            get
            {
                return this._hasFallback;
            }
        }
    }
}

