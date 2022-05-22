using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    public class UnknownSnooperExportedObject : Exception
    {
        public override String Message => base.Message +
                                          " Unknown reference SnooperExportedObjectBase, use new SnooperExportedDataObject*() (this should NEVER happen by the way...)";
    }
}