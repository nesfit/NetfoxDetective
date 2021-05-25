using System;
using System.Collections.Generic;
using Netfox.Core.Enums;

namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface IExportBase : IExportSource
    {
        ExportValidity ExportValidity { get; }
        List<IExportSource> ExportSources { get; }
        DateTime TimeStamp { get; }
    }
}