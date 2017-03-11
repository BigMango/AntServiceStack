namespace CHystrix
{
    using CHystrix.Threading;
    using CHystrix.Utils;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public abstract class ThreadIsolationCommand<T> : HystrixCommandBase<T>, IThreadIsolation<T>
    {
        private readonly IsolationSemaphore FallbackExecutionSemaphore;

        protected ThreadIsolationCommand() : this(null, null, null, null, false)
        {
        }

        internal ThreadIsolationCommand(string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config, bool hasFallback = false) : this(null, commandKey, groupKey, domain, config, hasFallback)
        {
        }

        internal ThreadIsolationCommand(string instanceKey, string commandKey, string groupKey, string domain, Action<ICommandConfigSet> config, bool hasFallback) : base(instanceKey, commandKey, groupKey, domain, config)
        {
            Func<string, IsolationSemaphore> valueFactory = null;
            HandleConfigChangeDelegate handleConfigChange = null;
            if (hasFallback || this.HasFallback)
            {
                if (valueFactory == null)
                {
                    valueFactory = key => new IsolationSemaphore(base.ConfigSet.FallbackMaxConcurrentCount);
                }
                this.FallbackExecutionSemaphore = HystrixCommandBase.FallbackExecutionSemaphores.GetOrAdd(this.Key, valueFactory);
            }
            if (handleConfigChange == null)
            {
                handleConfigChange = delegate (ICommandConfigSet cf) {
                    ((ThreadIsolationCommand<T>) this).UpdateMaxConcurrentCount<T>(cf.CommandMaxConcurrentCount);
                    ((ThreadIsolationCommand<T>) this).UpdateCommandTimeoutInMilliseconds<T>(cf.CommandTimeoutInMilliseconds);
                };
            }
            base.ConfigSet.SubcribeConfigChangeEvent(handleConfigChange);
        }

        private T ExecuteFallback()
        {
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
                    base.Log.Log(LogLevelEnum.Error, "HystrixCommand fallback execution failed.", exception, base.GetLogTagInfo().AddLogTagData("FXD303038"));
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.FallbackFailed);
                    this.Status = CommandStatusEnum.FallbackFailed;
                    throw exception;
                }
                finally
                {
                    this.FallbackExecutionSemaphore.Release();
                }
            }
            string message = "HystrixCommand fallback execution was rejected.";
            base.Log.Log(LogLevelEnum.Error, message, base.GetLogTagInfo().AddLogTagData("FXD303039"));
            base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.FallbackRejected);
            this.Status = CommandStatusEnum.FallbackRejected;
            throw new HystrixException(FailureTypeEnum.FallbackRejected, base.GetType(), this.Key, message);
        }

        private Task<T> ExecuteWithThreadPool()
        {
            EventHandler<StatusChangeEventArgs> onStatusChange = null;
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            try
            {
                if (!base.CircuitBreaker.AllowRequest())
                {
                    string message = "Circuit Breaker is open. Execution was short circuited.";
                    base.Log.Log(LogLevelEnum.Error, message, base.GetLogTagInfo().AddLogTagData("FXD303033"));
                    this.Status = CommandStatusEnum.ShortCircuited;
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ShortCircuited);
                    throw new HystrixException(FailureTypeEnum.ShortCircuited, base.GetType(), this.Key, message);
                }
                this.Status = CommandStatusEnum.Started;
                if (onStatusChange == null)
                {
                    onStatusChange = delegate (object o, StatusChangeEventArgs e) {
                        CWorkItem<T> item = o as CWorkItem<T>;
                        bool flag = false;
                        try
                        {
                            if (item.IsCompleted || item.IsCanceled)
                            {
                                ((ThreadIsolationCommand<T>) this).Metrics.MarkTotalExecutionLatency((long) item.ExeuteMilliseconds);
                                ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionLatency((long) item.RealExecuteMilliseconds);
                            }
                            switch (e.Status)
                            {
                                case CTaskStatus.RanToCompletion:
                                    ((ThreadIsolationCommand<T>) this).Status = CommandStatusEnum.Success;
                                    ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Success);
                                    ((ThreadIsolationCommand<T>) this).CircuitBreaker.MarkSuccess();
                                    tcs.SetResult(item.Result);
                                    goto Label_01C0;

                                case CTaskStatus.Running:
                                    ((ThreadIsolationCommand<T>) this).Status = CommandStatusEnum.Started;
                                    goto Label_01C0;

                                case CTaskStatus.Faulted:
                                    ((ThreadIsolationCommand<T>) this).Status = CommandStatusEnum.Failed;
                                    if (!item.Exception.IsBadRequestException())
                                    {
                                        break;
                                    }
                                    ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionEvent(CommandExecutionEventEnum.BadRequest);
                                    ((ThreadIsolationCommand<T>) this).Log.Log(LogLevelEnum.Error, "HystrixCommand request is bad.", item.Exception, ((ThreadIsolationCommand<T>) this).GetLogTagInfo().AddLogTagData("FXD303035"));
                                    goto Label_0162;

                                case CTaskStatus.Canceled:
                                    ((ThreadIsolationCommand<T>) this).Status = CommandStatusEnum.Timeout;
                                    ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Timeout);
                                    ((ThreadIsolationCommand<T>) this).Log.Log(LogLevelEnum.Warning, string.Format("timed out before executing run(), the wait time was {0} milliseconds; ", item.ExeuteMilliseconds), ((ThreadIsolationCommand<T>) this).GetLogTagInfo().AddLogTagData("FXD303034"));
                                    flag = true;
                                    goto Label_01C0;

                                default:
                                    goto Label_01C0;
                            }
                            ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Failed);
                            ((ThreadIsolationCommand<T>) this).Log.Log(LogLevelEnum.Error, "HystrixCommand execution failed.", item.Exception, ((ThreadIsolationCommand<T>) this).GetLogTagInfo().AddLogTagData("FXD303036"));
                        Label_0162:
                            flag = true;
                        Label_01C0:
                            if (flag)
                            {
                                if (((ThreadIsolationCommand<T>) this).HasFallback)
                                {
                                    try
                                    {
                                        tcs.SetResult(((ThreadIsolationCommand<T>) this).ExecuteFallback());
                                        return;
                                    }
                                    catch (Exception exception)
                                    {
                                        throw exception;
                                    }
                                }
                                if (e.Status == CTaskStatus.Faulted)
                                {
                                    throw item.Exception;
                                }
                                if (e.Status == CTaskStatus.Canceled)
                                {
                                    throw new HystrixException(FailureTypeEnum.ExecutionTimeout, ((ThreadIsolationCommand<T>) this).GetType(), ((ThreadIsolationCommand<T>) this).Key, string.Format("timed out before executing run(), maxt waiting time was {0} milliseconds,the task waiting time was {1} milliseconds", ((ThreadIsolationCommand<T>) this).ConfigSet.CommandTimeoutInMilliseconds, item.ExeuteMilliseconds), item.Exception, new Exception("no fallback found"));
                                }
                            }
                        }
                        catch (Exception exception2)
                        {
                            ((ThreadIsolationCommand<T>) this).Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                            tcs.TrySetException(exception2);
                        }
                    };
                }
                ((ThreadIsolationCommand<T>) this).QueueWorkItem<T>(new Func<T>(this.Execute), onStatusChange);
            }
            catch (Exception exception)
            {
                if ((exception is HystrixException) && (((HystrixException) exception).FailureType == FailureTypeEnum.ThreadIsolationRejected))
                {
                    base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.Rejected);
                    base.Log.Log(LogLevelEnum.Error, "HystrixCommand execution rejected.", exception, base.GetLogTagInfo().AddLogTagData("FXD303037"));
                    this.Status = CommandStatusEnum.Rejected;
                }
                if (this.HasFallback)
                {
                    tcs.Task.ContinueWith(delegate (Task<T> t) {
                        try
                        {
                             exception = t.Exception;
                        }
                        catch
                        {
                        }
                    });
                    return this.GetFallBack();
                }
                this.Status = CommandStatusEnum.Failed;
                base.Metrics.MarkExecutionEvent(CommandExecutionEventEnum.ExceptionThrown);
                tcs.TrySetException(exception);
            }
            return tcs.Task;
        }

        private Task<T> GetFallBack()
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            try
            {
                source.SetResult(this.ExecuteFallback());
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }
            return source.Task;
        }

        public Task<T> RunAsync()
        {
            return this.ExecuteWithThreadPool();
        }

        internal override IsolationModeEnum IsolationMode
        {
            get
            {
                return IsolationModeEnum.ThreadIsolation;
            }
        }
    }
}

