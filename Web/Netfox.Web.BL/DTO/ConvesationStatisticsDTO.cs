using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Netfox.Web.BL.Facades;
using Newtonsoft.Json;

namespace Netfox.Web.BL.DTO
{
    public class ConvesationStatisticsDTO
    {
      
        private DateTime _captureFinish;
        private DateTime _captureStart;

        private ProtocolSummaryItem[] _appProtocolsSummary;
        private ProtocolSummaryItem[] _transportProtocolsSummary;

        private IList<string> _exportedProtocols;


        public DateTime CaptureStart
        {
            get { return this._captureStart; }
            set { this._captureStart = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
        }

        public DateTime CaptureFinish
        {
            get { return this._captureFinish; }
            set { this._captureFinish = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
        }

        public string Name { get; set; }
        public long Frames { get; set; }
        public long TotalConversations => this.IPv4Conversations + this.IPv6Conversations;
        public long IPv4Conversations { get; set; }
        public long IPv6Conversations { get; set; }
        public long UniqueHostsCount { get; set; }
        public long ReckognizedProtocolsCount { get; set; }

        public long UpFlowFrames { get; set; }
        public long DownFlowFrames { get; set; }
        public long TotalFlowFrames => this.UpFlowFrames + this.DownFlowFrames;

        public long UpFlowBytes { get; set; }
        public long DownFlowBytes { get; set; }
        public long TotalFlowBytes => this.UpFlowBytes + this.DownFlowBytes;

        public long TcpConversations { get; set; }
        public long TotalTcpBytes { get; set; }
        public long UdpConversations { get; set; }
        public long TotalUdpBytes { get; set; }

        public long UpFlowTcpLostBytes { get; set; }
        public long DownFlowTcpLostBytes { get; set; }
        public long TotalFlowTcpLostBytes => this.UpFlowTcpLostBytes + this.DownFlowTcpLostBytes;


        /* Filter limits */
        public DateTime FilterDurationMin { get; set;}
        public DateTime FilterDurationMax { get; set; }

        public long L3FramesMin { get; set; }
        public long L3FramesMax { get; set; }
        public long L3BytesMin { get; set; }
        public long L3BytesMax { get; set; }

        public long L4FramesMin { get; set; }
        public long L4FramesMax { get; set; }
        public long L4BytesMin { get; set; }
        public long L4BytesMax { get; set; }

        public long L7FramesMin { get; set; }
        public long L7FramesMax { get; set; }
        public long L7BytesMin { get; set; }
        public long L7BytesMax { get; set; }

        public long FramesBytesMin { get; set; }
        public long FramesBytesMax { get; set; }

        public long TotalL3Conversations { get; set; }
        public long TotalL4Conversations { get; set; }
        public long TotalL7Conversations { get; set; }
        public long TotalFrames { get; set; }
        public long Size { get; set; }
        public Guid InvestigationId { get; set; }

        public string Period
        {
            get
            {
                var strBuilder = new StringBuilder(this.CaptureStart.ToString(CultureInfo.CurrentCulture));
                strBuilder.Append(" - ");
                strBuilder.Append(this.CaptureFinish.ToString(CultureInfo.CurrentCulture));
                return strBuilder.ToString();
            }
        }
        
        public float TotalFlowTcpLostBytePerc
        {
            get
            {
                if (this.TotalTcpBytes == 0) { return 0; }
                return ((float)this.TotalFlowTcpLostBytes / this.TotalTcpBytes) * 100;
            }
        }

        public KeyValue<string, long>[] AppProtocolsDistribution { get; set; }

        public ProtocolSummaryItem[] AppProtocolsSummary
        {
            get
            {
                if(_appProtocolsSummary == null)
                {
                    if(AppProtocolsSummaryJSON != null || AppProtocolsSummaryJSON != "")
                    {
                        this._appProtocolsSummary = JsonConvert.DeserializeObject<ProtocolSummaryItem[]>(this.AppProtocolsSummaryJSON);
                        return this._appProtocolsSummary;
                    }

                    return null;
                }

                return this._appProtocolsSummary;
            }
            set { this._appProtocolsSummary = value; }
        }

        public string AppProtocolsSummaryJSON { get; set; }

        public KeyValue<string, long>[] TransportProtocolsDistribution { get; set; }
        public ProtocolSummaryItem[] TransportProtocolsSummary
        {
            get
            {
                if (_transportProtocolsSummary == null)
                {
                    if (TransportProtocolsSummaryJSON != null || TransportProtocolsSummaryJSON != "")
                    {
                        this._transportProtocolsSummary = JsonConvert.DeserializeObject<ProtocolSummaryItem[]>(this.TransportProtocolsSummaryJSON);
                        return this._transportProtocolsSummary;
                    }

                    return null;
                }

                return this._transportProtocolsSummary;
            }
            set { this._transportProtocolsSummary = value; }
        }

        public string TransportProtocolsSummaryJSON { get; set; }

        public KeyValue<string, long>[] LinkProtocolsDistribution { get; set; }
        public ProtocolSummaryItem[] LinkProtocolsSummary { get; set; }

        public KeyValue<DateTime, long>[] TrafficErrors { get; set; }
        public KeyValue<DateTime, long>[] TrafficHistory { get; set; }

        public string ExportedProtocolsJSON{ get; set; }

        public IList<string> ExportedProtocols
        {
            get
            {
                if (this._exportedProtocols == null)
                {
                    if (ExportedProtocolsJSON != null && ExportedProtocolsJSON != "")
                    {
                        this._exportedProtocols = JsonConvert.DeserializeObject<List<string>>(this.ExportedProtocolsJSON);
                        return this._exportedProtocols;
                    }

                    return null;
                }

                return this._exportedProtocols;
            }
            set { this._exportedProtocols = value; }
        }


    }
    [Serializable]
    public class ProtocolSummaryItem
    {
        public ProtocolSummaryItem(string name, long totalBytes, float percent)
        {
            this.Name = name;
            this.TotalBytes = totalBytes;
            this.Percent = percent;
        }
        public ProtocolSummaryItem(){}

        public string Name { get; set; }
        public long TotalBytes { get; set; }
        public float Percent { get; set; }
    }
}