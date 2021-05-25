using System;

namespace Netfox.Framework.Models.Snoopers.Exceptions
{
    public class ExportedObjectAlreadyAdded : Exception
    {
        public override String Message => base.Message + ". SnooperExportedObject already added to SnooperExportBase";
    }
}