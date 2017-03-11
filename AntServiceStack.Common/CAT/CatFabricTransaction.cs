using System;
using Freeway.Logging;
using System.Collections.Generic;

namespace AntServiceStack.Common.CAT
{
    internal class CatFabricTransaction : CatTransactionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CatFabricTransaction));
        private static readonly Dictionary<string, string> tags = new Dictionary<string, string>() { { "ErrorCode", "FXD301043" } };

        protected string Type { get; private set; }
        protected string Identity { get; private set; }

        
        public CatFabricTransaction(string type, string identity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identity))
                    return;

                this.Type = type;
                this.Identity = identity;

               
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred in 'CatFabricTransaction'.", ex, tags);
            }
        }

        /// <summary>
        /// Sync Mode
        /// </summary>
        public override void Start()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred in 'CatFabricTransaction'.", ex, tags);
            }
        }

        /// <summary>
        /// Async Mode
        /// </summary>
        public virtual void Fork()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred in 'CatFabricTransaction'.", ex, tags);
            }
        }
    }
}
