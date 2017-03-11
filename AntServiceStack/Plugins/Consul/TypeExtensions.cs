// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 

using System;
using System.Linq;

namespace AntServiceStack.Plugins.Consul
{
    public static class TypeExtensions
    {
        public static TAttr[] AllAttributes<TAttr>(this Type type)
        {
            return type.GetCustomAttributes(typeof(TAttr), true).OfType<TAttr>().ToArray<TAttr>();
        }
        

        

       
    }
}