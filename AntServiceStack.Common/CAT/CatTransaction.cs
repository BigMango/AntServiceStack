using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.CAT
{
    internal class CatTransaction : CatTransactionBase
    {
        public string Type { get; protected set; }
        public string Identity { get; protected set; }

        protected CatTransaction() { }

        public CatTransaction(string type, string identity)
        {
            Type = type;
            Identity = identity;
        }

        public override void Start()
        {
           
        }
    }
}
