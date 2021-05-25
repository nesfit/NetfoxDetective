using System;
using System.Data.Entity;

namespace Netfox.Framework.Models.Snoopers
{
    public interface ISnooper
    {
        String Name { get; }

        String[] ProtocolNBARName { get; }

        String Description { get; }

        Int32[] KnownApplicationPorts { get; }
        SnooperExportBase PrototypeExportObject { get; }

        void Run();
    }
}