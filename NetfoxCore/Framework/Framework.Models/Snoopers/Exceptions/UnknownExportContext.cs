using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    public class UnknownExportContext : Exception
    {
        public override String Message => base.Message +
                                          "unknown export context, use SnooperBase.OnAfter* and SnooperBase.OnBefore* functions";
    }
}