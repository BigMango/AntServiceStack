using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Freeway.Logging;

namespace AntServiceStack.Common.CAT
{
    internal abstract class CatTransactionBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CatTransactionBase));


        protected Exception Error { get; set; }

        public abstract void Start();

        public virtual void MarkSuccess()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
            }
        }

        public virtual void MarkFailure(Exception ex, bool logError)
        {
            try
            {
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }
        }

        public virtual void MarkFailure(Exception ex)
        {
            MarkFailure(ex, true);
        }

        public virtual void End()
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
