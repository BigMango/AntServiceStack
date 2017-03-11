// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace AntServiceStack.Common.Consul
{
    /// <summary>
    /// Represents a consul registered service address
    /// </summary>
    public class ConsulServiceResponse
    {
        public string Node { get; private set; }

        public string ServiceID { get;  set; }

        public string ServiceName { get; private set; }

        public string[] ServiceTags { get; private set; }

        public string ServiceAddress { get;  set; }

        public int ServicePort { get; private set; }

        public static ConsulServiceResponse Create(ConsulHealthResponse response)
        {
            if (response?.Node == null || (response.Service == null))
                return null;

            return new ConsulServiceResponse
            {
                Node = response.Node.NodeName,
                ServiceID = response.Service.ID,
                ServiceName = response.Service.Service,
                ServiceTags = response.Service.Tags,
                ServiceAddress = response.Service.Address,
                ServicePort = response.Service.Port
            };
        }
    }
}