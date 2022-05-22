using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    class UnprocessedSnooperExportedObjectBase : Exception
    {
        public override String Message => base.Message +
                                          " There is still unprocessed SnooperExportedObjectBase, use SnooperExportBase.AddExportObject() or SnooperExportBase.DiscardExportObject()";
    }
}