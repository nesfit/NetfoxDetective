using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    public class EmptySourceInSnooperExportedObject : Exception
    {
        public override String Message => base.Message + ". Cannot add an export object without a source";
    }
}