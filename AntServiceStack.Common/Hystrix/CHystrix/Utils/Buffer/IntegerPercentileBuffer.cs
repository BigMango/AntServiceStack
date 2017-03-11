namespace CHystrix.Utils.Buffer
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class IntegerPercentileBuffer : PercentileBuffer<long>
    {
        public IntegerPercentileBuffer(int timeWindowInSeconds, int bucketTimeWindowInSeconds, int bucketSizeLimit) : base(timeWindowInSeconds, bucketTimeWindowInSeconds, bucketSizeLimit)
        {
        }

        public void GetAuditData(out int count, out long sum, out long min, out long max)
        {
            int iCount = 0;
            long iSum = 0L;
            long iMin = 0x7fffffffffffffffL;
            long iMax = -9223372036854775808L;
            base.VisitData(delegate (long item) {
                iCount++;
                iSum += item;
                if (item < iMin)
                {
                    iMin = item;
                }
                if (item > iMax)
                {
                    iMax = item;
                }
            });
            if (iCount == 0)
            {
                iMin = 0L;
                iMax = 0L;
            }
            count = iCount;
            sum = iSum;
            min = iMin;
            max = iMax;
        }

        public long GetAuditDataAvg()
        {
            int count = 0;
            long sum = 0L;
            base.VisitData(delegate (long item) {
                count++;
                sum += item;
            });
            long num = 0L;
            if (count > 0)
            {
                num = (long) Math.Round((double) (((double) sum) / ((double) count)));
            }
            return num;
        }

        public int GetItemCountInRange(long low, long? high = new long?())
        {
            int count = 0;
            base.VisitData(delegate (long item) {
                if ((item >= low) && (!high.HasValue || (item < high.Value)))
                {
                    count++;
                }
            });
            return count;
        }

        public long GetPercentile(double percent)
        {
            List<long> snapShot = this.GetSnapShot();
            if (snapShot.Count <= 0)
            {
                return 0L;
            }
            snapShot.Sort();
            if (percent <= 0.0)
            {
                return snapShot[0];
            }
            if (percent >= 100.0)
            {
                return snapShot[snapShot.Count - 1];
            }
            int num = (int) ((percent * (snapShot.Count - 1)) / 100.0);
            return snapShot[num];
        }
    }
}

