using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    public class WrongExportContext : Exception
    {
        public override String Message => base.Message + ". Wrong export context";
    }
}