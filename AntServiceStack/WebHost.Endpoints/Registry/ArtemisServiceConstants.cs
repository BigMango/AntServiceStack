//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Com.ant.Soa.Caravan.Metric;
//using Com.ant.Soa.Caravan.Metric.ant;

//namespace AntServiceStack.WebHost.Endpoints.Registry
//{
//    static class ArtemisServiceConstants
//    {
//        public const string ManagerId = "soa.service";

//        public const string ArtemisUrlPropertyKey = "artemis.client." + ManagerId + ".service.domain.url";

//        internal static IEventMetricManager EventMetricManager { get; private set; }

//        internal static IAuditMetricManager AuditMetricManager { get; private set; }

//        static ArtemisServiceConstants()
//        {
//            EventMetricManager = new CLogEventMetricManager(ManagerId);
//            AuditMetricManager = new CLogAuditMetricManager(ManagerId);
//        }
//    }
//}
