using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    public interface IHystrixCommandProperties
    {
        /// <summary>
        /// �Ƿ��������Ĺ��� Ĭ��true ������ܿ���  �������Ϊtrue ÿ������������������������
        /// </summary>
        IHystrixProperty<bool> CircuitBreakerEnabled { get; }
        /// <summary>
        /// if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit
        /// ������ڵ��ڰٷ�֮50��������10���ڶ�ʧ���� �Ὺ�����ұ���ģʽ
        /// Ĭ��50
        /// </summary>
        IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage { get; }
        /// <summary>
        /// ��������ͨ��״̬ Ĭ��false �������Ϊtrue �ʹ���ر� SOA���� ���������Ĺ��� ��Ҳ�Ǹ��ӿ���
        /// </summary>
        IHystrixDynamicProperty<bool> CircuitBreakerForceClosed { get; }
        /// <summary>
        /// ��բ�ǶϿ���״̬ Ĭ��false
        /// </summary>
        IHystrixDynamicProperty<bool> CircuitBreakerForceOpen { get; }
        /// <summary>
        /// ��10����20��������뷢����ͳ����Ϣ֮ǰ
        /// </summary>
        IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold { get; }

        /// <summary>
        /// ���߶���ʱ�������� Ĭ��5��
        /// </summary>
        IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow { get; }
        /// <summary>
        /// ÿ��������ִ��timeout Ĭ��20��
        /// </summary>
        IHystrixDynamicProperty<TimeSpan?> ExecutionIsolationThreadTimeout { get; }

        /// <summary>
        /// 50�����checkhealthһ�Σ�
        /// </summary>

        IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval { get; }
        /// <summary>
        ///  Ĭ��10��
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; }
        /// <summary>
        /// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 seconds
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; }
        /// <summary>
        /// Ĭ��60��
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferTimeWindowInSeconds { get; }
        /// <summary>
        /// Ĭ��10��Ͱ
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferBucketTimeWindowInSeconds { get; }

        /// <summary>
        /// ����Ĭ��Ϊ200
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferBucketSizeLimit { get; }
        /// <summary>
        /// Ĭ��false
        /// </summary>
        IHystrixProperty<bool> RequestLogEnabled { get; }
    }
}
