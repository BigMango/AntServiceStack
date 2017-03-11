namespace CHystrix
{
    using System;
    using System.Runtime.CompilerServices;

    internal class CommandExecutionHealthSnapshot
    {
        public CommandExecutionHealthSnapshot(int totalCount, int failedCount)
        {
            this.TotalCount = totalCount;
            this.ErrorPercentage = (totalCount == 0) ? 0 : ((int) Math.Floor((double) ((((double) failedCount) / ((double) totalCount)) * 100.0)));
        }

        public int ErrorPercentage { get; private set; }

        public int TotalCount { get; private set; }
    }
}

