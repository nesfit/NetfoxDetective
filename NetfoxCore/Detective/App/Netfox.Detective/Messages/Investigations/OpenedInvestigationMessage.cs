using System;
using System.Diagnostics;

namespace Netfox.Detective.Messages.Investigations
{
    class OpenedInvestigationMessage
    {
        private object _investigation;

        public object InvestigationVm { get; set; }

        public object Investigation
        {
            get
            {
                if (this._investigation != null)
                {
                    return this._investigation;
                }

                if (this.InvestigationVm == null)
                {
                    return null;
                }

                try
                {
                    dynamic investigationVm = this.InvestigationVm;
                    return investigationVm.Investigation;
                }
                catch (Exception)
                {
                    Debugger.Break(); // investigationVm is not instance of investigationVm instance
                    return null;
                }
            }
            set { this._investigation = value; }
        }
    }
}