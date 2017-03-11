namespace CHystrix.CircuitBreaker
{
    using CHystrix;
    using CHystrix.Utils;
    using CHystrix.Utils.Atomic;
    using System;
    using System.Collections.Generic;

    internal class CircuitBreaker : ICircuitBreaker
    {
        protected readonly AtomicLong CircuitOpenedOrLastTestedTime;
        protected readonly ICommandConfigSet ConfigSet;
        protected readonly ICommandMetrics Metrics;
        protected readonly AtomicBoolean OpenFlag;

        public CircuitBreaker(ICommandConfigSet configSet, ICommandMetrics metrics)
        {
            this.ConfigSet = configSet;
            this.Metrics = metrics;
            this.OpenFlag = new AtomicBoolean();
            this.CircuitOpenedOrLastTestedTime = new AtomicLong();
        }

        public bool AllowRequest()
        {
            if (this.ConfigSet.CircuitBreakerEnabled)
            {
                if (this.ConfigSet.CircuitBreakerForceOpen)
                {
                    return false;
                }
                if (this.ConfigSet.CircuitBreakerForceClosed)
                {
                    this.IsOpen();
                    return true;
                }
                if (this.IsOpen())
                {
                    return this.AllowSingleTest();
                }
            }
            return true;
        }

        private bool AllowSingleTest()
        {
            long expect = this.CircuitOpenedOrLastTestedTime.Value;
            if (((this.OpenFlag != null) && (CommonUtils.CurrentTimeInMiliseconds > (expect + this.ConfigSet.CircuitBreakerSleepWindowInMilliseconds))) && this.CircuitOpenedOrLastTestedTime.CompareAndSet(expect, CommonUtils.CurrentTimeInMiliseconds))
            {
                Dictionary<string, string> tagInfo = new Dictionary<string, string>();
                tagInfo.Add("CircuitBreaker", "AllowSingleRequest");
                CommonUtils.Log.Log(LogLevelEnum.Info, this.ConfigSet.CircuitBreakerSleepWindowInMilliseconds + " milliseconds passed. Allow 1 request to run to see whether the access point has recovered.", tagInfo);
                return true;
            }
            return false;
        }

        public bool IsOpen()
        {
            if (!this.ConfigSet.CircuitBreakerEnabled)
            {
                return false;
            }
            if (!this.ConfigSet.CircuitBreakerForceOpen)
            {
                if (this.OpenFlag != null && this.OpenFlag == true)
                {
                    return true;
                }
                CommandExecutionHealthSnapshot executionHealthSnapshot = this.Metrics.GetExecutionHealthSnapshot();
                if (executionHealthSnapshot.TotalCount < this.ConfigSet.CircuitBreakerRequestCountThreshold)
                {
                    return false;
                }
                if (executionHealthSnapshot.ErrorPercentage < this.ConfigSet.CircuitBreakerErrorThresholdPercentage)
                {
                    return false;
                }
                if (this.OpenFlag.CompareAndSet(false, true))
                {
                    this.CircuitOpenedOrLastTestedTime.Value = CommonUtils.CurrentTimeInMiliseconds;
                    Dictionary<string, string> tagData = new Dictionary<string, string>();
                    tagData.Add("CircuitBreaker", "Open");
                    CommonUtils.Log.Log(LogLevelEnum.Fatal, "Circuit Breaker is open after lots of fail or timeout happen.", tagData.AddLogTagData("FXD303010"));
                }
            }
            return true;
        }

        public void MarkSuccess()
        {
            if (this.ConfigSet.CircuitBreakerEnabled && (this.OpenFlag != null))
            {
                this.OpenFlag.Value = false;
                this.Metrics.Reset();
                Dictionary<string, string> tagInfo = new Dictionary<string, string>();
                tagInfo.Add("CircuitBreaker", "Closed");
                CommonUtils.Log.Log(LogLevelEnum.Info, "Circuit Breaker is closed after a command execution succeeded.", tagInfo);
            }
        }
    }
}

