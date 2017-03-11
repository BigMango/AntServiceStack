namespace CHystrix
{
    using CHystrix.Utils;
    using System;
    using System.Diagnostics;
    using System.Threading;

    public abstract class SemaphoreIsolationCommand<T> : HystrixCommandBase<T>, ISemaphoreIsolation<T>
    {
        private readonly IsolationSemaphore ExecutionSemaphore;
        private readonly IsolationSemaphore FallbackExecutionSemaphore;

        protected SemaphoreIsolationCommand() : this(null, null, null, null, false)
        {
        }

        internal SemaphoreIsolationCommand(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config, bool hasFallback) : this(null, commandKey, groupKey, domain, config, hasFallback)
        {
        }

        internal SemaphoreIsolationCommand(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config, bool hasFallback) : base(instanceKey, commandKey, groupKey, domain, config)
        {
            Func<string, IsolationSemaphore> valueFactory = null;
            Func<string, IsolationSemaphore> func2 = null;
            if (valueFactory == null)
            {
                valueFactory = key => new IsolationSemaphore(base.ConfigSet.CommandMaxConcurrentCount);
            }
            this.ExecutionSemaphore = HystrixCommandBase.ExecutionSemaphores.GetOrAdd(this.Key, valueFactory);
            if (hasFallback || this.HasFallback)
            {
                if (func2 == null)
                {
                    func2 = key => new IsolationSemaphore(base.ConfigSet.FallbackMaxConcurrentCount);
                }
                this.FallbackExecutionSemaphore = HystrixCommandBase.FallbackExecutionSemaphores.GetOrAdd(this.Key, func2);
            }
        }

        private T ExecuteFallback(Exception cause)
        {
            if ((cause is HystrixException) && (cause.InnerException != null))
            {
                cause = cause.InnerException;
            }
            if (this.FallbackExecutionSemaphore.TryAcquire())
            {
                try
                {
                    T fallback = base.ToIFallback().GetFallback();
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.FallbackSuccess);
                    this.Status = CommandStatusEnum.FallbackSuccess;
                    base.Log.Log(LogLevelEnum.Warning, "HystrixCommand execution failed, use fallback instead.");
                    return fallback;
                }
                catch (Exception exception)
                {
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.FallbackFailed);
                    this.Status = CommandStatusEnum.FallbackFailed;
                    if (base.ConfigSet.LogExecutionError)
                    {
                        base.Log.Log(LogLevelEnum.Error, "HystrixCommand fallback execution failed.", exception, base.GetLogTagInfo().AddLogTagData("FXD303024"));
                    }
                    throw;
                }
                finally
                {
                    this.FallbackExecutionSemaphore.Release();
                }
            }
            this.Status = CommandStatusEnum.FallbackRejected;
            base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.FallbackRejected);
            string message = "HystrixCommand fallback execution was rejected.";
            base.Log.Log(LogLevelEnum.Error, message, base.GetLogTagInfo().AddLogTagData("FXD303025"));
            throw new HystrixException(FailureTypeEnum.FallbackRejected, base.GetType(), this.Key, message, cause);
        }

        private T ExecuteWithSemaphoreIsolation()
        {
            if (!base.CircuitBreaker.AllowRequest())
            {
                string str = "Circuit Breaker is open. Execution was short circuited.";
                base.Log.Log(LogLevelEnum.Error, str, base.GetLogTagInfo().AddLogTagData("FXD303019"));
                this.Status = CommandStatusEnum.ShortCircuited;
                base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ShortCircuited);
                throw new HystrixException(FailureTypeEnum.ShortCircuited, base.GetType(), this.Key, str);
            }
            if (this.ExecutionSemaphore.TryAcquire())
            {
                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    this.Status = CommandStatusEnum.Started;
                    T local = this.Execute();
                    stopwatch.Stop();
                    if (stopwatch.ElapsedMilliseconds > base.ConfigSet.CommandTimeoutInMilliseconds)
                    {
                        this.Status = CommandStatusEnum.Timeout;
                        base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Timeout);
                        base.Log.Log(LogLevelEnum.Warning, string.Format("HystrixCommand execution timeout: {0}ms.", stopwatch.ElapsedMilliseconds), base.GetLogTagInfo().AddLogTagData("FXD303020"));
                        return local;
                    }
                    this.Status = CommandStatusEnum.Success;
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Success);
                    base.CircuitBreaker.MarkSuccess();
                    return local;
                }
                catch (Exception exception)
                {
                    if (exception.IsBadRequestException())
                    {
                        this.Status = CommandStatusEnum.Failed;
                        base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.BadRequest);
                        if (base.ConfigSet.LogExecutionError)
                        {
                            base.Log.Log(LogLevelEnum.Error, "HystrixCommand request is bad.", exception, base.GetLogTagInfo().AddLogTagData("FXD303021"));
                        }
                        throw;
                    }
                    this.Status = CommandStatusEnum.Failed;
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Failed);
                    if (base.ConfigSet.LogExecutionError)
                    {
                        base.Log.Log(LogLevelEnum.Error, "HystrixCommand execution failed.", exception, base.GetLogTagInfo().AddLogTagData("FXD303022"));
                    }
                    throw;
                }
                finally
                {
                    this.ExecutionSemaphore.Release();
                    stopwatch.Stop();
                    base.Metrics.MarkExecutionLatency(stopwatch.ElapsedMilliseconds);
                }
            }
            this.Status = CommandStatusEnum.Rejected;
            base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Rejected);
            string message = "HystrixCommand execution was rejected.";
            base.Log.Log(LogLevelEnum.Error, message, base.GetLogTagInfo().AddLogTagData("FXD303023"));
            throw new HystrixException(FailureTypeEnum.SemaphoreIsolationRejected, base.GetType(), this.Key, message);
        }

        public T Run()
        {
            if ((this.Status == CommandStatusEnum.NotStarted) && Monitor.TryEnter(base.ExecutionLock))
            {
                try
                {
                    if (this.Status == CommandStatusEnum.NotStarted)
                    {
                        Stopwatch stopwatch = null;
                        try
                        {
                            stopwatch = new Stopwatch();
                            stopwatch.Start();
                            return this.ExecuteWithSemaphoreIsolation();
                        }
                        catch (Exception exception)
                        {
                            if (!this.HasFallback)
                            {
                                throw;
                            }
                            return this.ExecuteFallback(exception);
                        }
                        finally
                        {
                            if (stopwatch != null)
                            {
                                stopwatch.Stop();
                            }
                            base.Metrics.MarkTotalExecutionLatency(stopwatch.ElapsedMilliseconds);
                        }
                    }
                }
                catch
                {
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                    throw;
                }
                finally
                {
                    Monitor.Exit(base.ExecutionLock);
                }
            }
            string message = "The command has been started or finished. A command can be only run once.";
            base.Log.Log(LogLevelEnum.Error, message, base.GetLogTagInfo().AddLogTagData("FXD303018"));
            throw new InvalidOperationException(message);
        }

        internal override IsolationModeEnum IsolationMode
        {
            get
            {
                return IsolationModeEnum.SemaphoreIsolation;
            }
        }
    }
}

