using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Freeway.Logging;

namespace AntServiceStack.Common.CAT
{
    internal class CatForkedTransaction : CatTransactionBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatForkedTransaction));

        

        protected CatForkedTransaction() { }

      

       

        public override void Start()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

      
    }
}
