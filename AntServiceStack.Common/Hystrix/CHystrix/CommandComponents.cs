namespace CHystrix
{
    using System;
    using System.Runtime.CompilerServices;

    internal class CommandComponents
    {
        public ICircuitBreaker CircuitBreaker { get; set; }

        public CHystrix.CommandInfo CommandInfo { get; set; }

        public ICommandConfigSet ConfigSet { get; set; }

        public IsolationModeEnum IsolationMode { get; set; }

        public ILog Log { get; set; }

        public ICommandMetrics Metrics { get; set; }
    }
}

