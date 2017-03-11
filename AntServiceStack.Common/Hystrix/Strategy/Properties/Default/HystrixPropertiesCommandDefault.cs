using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.Strategy.Properties
{
    public class HystrixPropertiesCommandDefault : IHystrixCommandProperties
    {
        private const int DefaultMetricsRollingStatisticalWindowInMilliseconds = 10000;// default => statisticalWindow: 10000 = 10 seconds
        private const int DefaultMetricsRollingStatisticalWindowBuckets = 10;// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 seconds window so each bucket is 1 second
        private const int DefaultCircuitBreakerRequestVolumeThreshold = 20;// default => statisticalWindowVolumeThreshold: 20 requests in 10 seconds must occur before statistics matter
        private static readonly TimeSpan DefaultCircuitBreakerSleepWindow = TimeSpan.FromSeconds(5.0);// default => sleepWindow: 5000 = 5 seconds that we will sleep before trying again after tripping the circuit
        private const int DefaultCircuitBreakerErrorThresholdPercentage = 50;// default => errorThresholdPercentage = 50 = if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit
        private const bool DefaultCircuitBreakerForceOpen = false;// default => forceCircuitOpen = false (we want to allow traffic)
        private const bool DefaultCircuitBreakerForceClosed = false;// default => ignoreErrors = false 
        public static readonly TimeSpan DefaultExecutionIsolationThreadTimeout = TimeSpan.FromSeconds(20.0); // default => executionTimeoutInMilliseconds: 1000 = 1 second
        private const bool DefaultRequestLogEnabled = false;
        private const bool DefaultCircuitBreakerEnabled = true;
        private static readonly TimeSpan DefaultMetricsHealthSnapshotInterval = TimeSpan.FromMilliseconds(500); // default to 500ms as max frequency between allowing snapshots of health (error percentage etc)

        private const int DefaultMetricsIntegerBufferTimeWindowInSeconds = 60;
        private const int DefaultMetricsIntegerBufferBucketTimeWindowInSeconds = 10;
        private const int DefaultMetricsIntegerBufferBucketSizeLimit = 200;

        /// <summary>
        /// �Ƿ��������Ĺ��� Ĭ��true ������ܿ���
        /// </summary>
        public IHystrixProperty<bool> CircuitBreakerEnabled { get; private set; }

        /// <summary>
        /// if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit
        /// ������ڵ��ڰٷ�֮50��������10���ڶ�ʧ���� �Ὺ�����ұ���ģʽ
        /// Ĭ��50
        /// </summary>
        public IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage { get; private set; }

        /// <summary>
        /// ��������ͨ��״̬
        /// </summary>
        public IHystrixDynamicProperty<bool> CircuitBreakerForceClosed { get; private set; }

        /// <summary>
        /// ��բ�ǶϿ���״̬
        /// </summary>
        public IHystrixDynamicProperty<bool> CircuitBreakerForceOpen { get; private set; }

        /// <summary>
        /// ��10����20��������뷢����ͳ����Ϣ֮ǰ
        /// </summary>
        public IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold { get; private set; }

        /// <summary>
        /// ���߶���ʱ�������� Ĭ��5��
        /// </summary>
        public IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow { get; private set; }

        /// <summary>
        /// ÿ��������ִ��timeout Ĭ��20��
        /// </summary>
        public IHystrixDynamicProperty<TimeSpan?> ExecutionIsolationThreadTimeout { get; private set; }

        /// <summary>
        /// 50�����checkhealthһ�Σ�
        /// </summary>
        public IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval { get; private set; }

        /// <summary>
        ///  Ĭ��10��
        /// </summary>
        public IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; private set; }

        /// <summary>
        /// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 seconds
        /// </summary>
        public IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; private set; }

        /// <summary>
        /// Ĭ��false
        /// </summary>
        public IHystrixProperty<bool> RequestLogEnabled { get; private set; }

        /// <summary>
        /// Ĭ��60��
        /// </summary>
        public IHystrixProperty<int> MetricsIntegerBufferTimeWindowInSeconds { get; private set; }

        /// <summary>
        /// Ĭ��10��Ͱ
        /// </summary>
        public IHystrixProperty<int> MetricsIntegerBufferBucketTimeWindowInSeconds { get; private set; }

        /// <summary>
        /// ����Ĭ��Ϊ200
        /// </summary>
        public IHystrixProperty<int> MetricsIntegerBufferBucketSizeLimit { get; private set; }

        public HystrixPropertiesCommandDefault(HystrixCommandPropertiesSetter setter)
        {
            CircuitBreakerEnabled = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerEnabled, DefaultCircuitBreakerEnabled);
            CircuitBreakerErrorThresholdPercentage = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerErrorThresholdPercentage, DefaultCircuitBreakerErrorThresholdPercentage);
            // dynamic property can be updated at runtime
            CircuitBreakerForceClosed = HystrixPropertyFactory.AsDynamicProperty(setter.CircuitBreakerForceClosed, DefaultCircuitBreakerForceClosed);
            // dynamic property can be updated at runtime
            CircuitBreakerForceOpen = HystrixPropertyFactory.AsDynamicProperty(setter.CircuitBreakerForceOpen, DefaultCircuitBreakerForceOpen);
            CircuitBreakerRequestVolumeThreshold = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerRequestVolumeThreshold, DefaultCircuitBreakerRequestVolumeThreshold);
            CircuitBreakerSleepWindow = HystrixPropertyFactory.AsProperty(setter.CircuitBreakerSleepWindow, DefaultCircuitBreakerSleepWindow);
            // dynamic property can be updated at runtime
            ExecutionIsolationThreadTimeout = HystrixPropertyFactory.AsDynamicProperty(setter.ExecutionIsolationThreadTimeout);
            MetricsHealthSnapshotInterval = HystrixPropertyFactory.AsProperty(setter.MetricsHealthSnapshotInterval, DefaultMetricsHealthSnapshotInterval);
            MetricsRollingStatisticalWindowInMilliseconds = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowInMilliseconds, DefaultMetricsRollingStatisticalWindowInMilliseconds);
            MetricsRollingStatisticalWindowBuckets = HystrixPropertyFactory.AsProperty(setter.MetricsRollingStatisticalWindowBuckets, DefaultMetricsRollingStatisticalWindowBuckets);
            MetricsIntegerBufferTimeWindowInSeconds = HystrixPropertyFactory.AsProperty(setter.MetricsIntegerBufferTimeWindowInSeconds, DefaultMetricsIntegerBufferTimeWindowInSeconds);
            MetricsIntegerBufferBucketTimeWindowInSeconds = HystrixPropertyFactory.AsProperty(setter.MetricsIntegerBufferBucketTimeWindowInSeconds, DefaultMetricsIntegerBufferBucketTimeWindowInSeconds);
            MetricsIntegerBufferBucketSizeLimit = HystrixPropertyFactory.AsProperty(setter.MetricsIntegerBufferBucketSizeLimit, DefaultMetricsIntegerBufferBucketSizeLimit);
            RequestLogEnabled = HystrixPropertyFactory.AsProperty(setter.RequestLogEnabled, DefaultRequestLogEnabled);
        }
    }
}
