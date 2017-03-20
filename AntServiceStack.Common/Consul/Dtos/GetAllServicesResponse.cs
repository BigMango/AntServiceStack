using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Consul.Dtos
{
    public class GetAllServicesResponse
    {
        public SimpleService[] SimpleServiceList { get; set; }
    }

    public class SimpleService
    {
        public string Name { get; set; }
        public string[] Nodes { get; set; }
        public int ChecksPassing { get; set; }
        public int ChecksWarning { get; set; }
        public int ChecksCritical { get; set; }
    }
}




