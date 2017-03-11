using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace Ant.Tools.SOA.ServiceDescription
{
    [ServiceContract(Name = Constants.InternalContractName)]
    internal interface IDummyContract
    {
        [OperationContract]
        void DummyOperation();
    }
}
