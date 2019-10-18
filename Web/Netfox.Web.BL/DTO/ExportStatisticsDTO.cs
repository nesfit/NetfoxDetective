using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Netfox.Web.BL.Facades;
using Netfox.Web.DAL.Entities;
using Newtonsoft.Json;

namespace Netfox.Web.BL.DTO
{
    public class ExportStatisticsDTO
    {
        public long TotalExportedObject { get; set; }
        public long TotalCalls { get; set; }
        public long TotalMessage { get; set; }
        public long TotalEmail { get; set; }
        public long TotalOther { get; set; }
    }
}