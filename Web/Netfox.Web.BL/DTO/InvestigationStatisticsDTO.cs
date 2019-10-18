using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Netfox.Web.BL.Facades;
using Newtonsoft.Json;

namespace Netfox.Web.BL.DTO
{
    public class InvestigationStatisticsDTO
    {
        public long CountCaptures { get; set; }
        public long TotalL3Conversations { get; set; }
        public long TotalL4Conversations { get; set; }
        public long TotalL7Conversations { get; set; }
        public long TotalFrames { get; set; }
        public long TotalSize { get; set; }
        public string TotalSizeText { get; set; }

        public long InProgressCapture { get; set; }
        public long InProgressExport { get; set; }
        public long FinishedCapture { get; set; }
        public long FinishedExport { get; set; }

        public ExportStatisticsDTO Export { get; set; }
    }
}