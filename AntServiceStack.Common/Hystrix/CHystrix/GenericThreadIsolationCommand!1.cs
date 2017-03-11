namespace CHystrix
{
    using System;

    internal class GenericThreadIsolationCommand<T> : ThreadIsolationCommand<T>, IFallback<T>
    {
        private readonly Func<T> _Execute;
        private readonly Func<T> _GetFallback;
        private readonly bool _HasFallback;

        public GenericThreadIsolationCommand(string commandKey, string groupKey, string domain, Func<T> execute, Func<T> getFallback, Action<ICommandConfigSet> configCommand) : this(null, commandKey, groupKey, domain, execute, getFallback, configCommand)
        {
        }

        public GenericThreadIsolationCommand(string instanceKey, string commandKey, string groupKey, string domain, Func<T> execute, Func<T> getFallback, Action<ICommandConfigSet> configCommand) : base(instanceKey, commandKey, groupKey, domain, configCommand, getFallback != null)
        {
            if (string.IsNullOrWhiteSpace(commandKey))
            {
                throw new ArgumentNullException("CommandKey cannot be null.");
            }
            if (execute == null)
            {
                throw new ArgumentNullException("execute function cannot be null.");
            }
            this._Execute = execute;
            this._GetFallback = getFallback;
            this._HasFallback = this._GetFallback != null;
        }

        protected override T Execute()
        {
            return this._Execute();
        }

        public T GetFallback()
        {
            if (!this.HasFallback)
            {
                throw new NotImplementedException();
            }
            return this._GetFallback();
        }

        internal override bool HasFallback
        {
            get
            {
                return this._HasFallback;
            }
        }
    }
}

