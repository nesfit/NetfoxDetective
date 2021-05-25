using Castle.Core.Logging;
using Netfox.Core.Interfaces;

namespace Netfox.Detective.Services
{
    public abstract class DetectiveInvestigationService : ILoggable, IInvestigationService
    {
        protected DetectiveInvestigationService()
        {
        }


        #region Implementation of ILoggable

        public ILogger Logger { get; set; }

        #endregion
    }
}