// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using AntServiceStack.Common.Consul.Discovery;

namespace AntServiceStack.Common.Consul.Dtos
{
    public class GetServicesResponse
    {
        public ConsulService[] Services { get; set; }
    }
}