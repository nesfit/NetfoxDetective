using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Netfox.Core.Properties;
using Netfox.Detective.Models.Base;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.Models.Conversations
{
    [NotifyPropertyChanged]
    public class ConversationsStatistics : INotifyPropertyChanged
    {
        private DateTime _captureFinish;
        private DateTime _captureStart;

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

        public long Frames { get; set; }
        public uint TotalConversations => this.IPv4Conversations + this.IPv6Conversations;
        public uint IPv4Conversations { get; set; }
        public uint IPv6Conversations { get; set; }
        public uint UniqueHostsCount { get; set; }
        public uint ReckognizedProtocolsCount { get; set; }

        public long UpFlowFrames { get; set; }
        public long DownFlowFrames { get; set; }
        public long TotalFlowFrames => this.UpFlowFrames + this.DownFlowFrames;

        public long UpFlowBytes { get; set; }
        public long DownFlowBytes { get; set; }
        public long TotalFlowBytes => this.UpFlowBytes + this.DownFlowBytes;

        public uint TcpConversations { get; set; }
        public long TotalTcpBytes { get; set; }
        public uint UdpConversations { get; set; }
        public long TotalUdpBytes { get; set; }

        public long UpFlowTcpLostBytes { get; set; }
        public long DownFlowTcpLostBytes { get; set; }
        public long TotalFlowTcpLostBytes => this.UpFlowTcpLostBytes + this.DownFlowTcpLostBytes;

        [SafeForDependencyAnalysis]
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

        [SafeForDependencyAnalysis]
        public float TotalFlowTcpLostBytePerc
        {
            get
            {
                if (this.TotalTcpBytes == 0)
                {
                    return 0;
                }

                return ((float) this.TotalFlowTcpLostBytes / this.TotalTcpBytes) * 100;
            }
        }

        public KeyValue<string, long>[] AppProtocolsDistribution { get; set; }
        public ProtocolSummaryItem[] AppProtocolsSummary { get; set; }

        public KeyValue<string, long>[] TransportProtocolsDistribution { get; set; }
        public ProtocolSummaryItem[] TransportProtocolsSummary { get; set; }

        public KeyValue<string, long>[] LinkProtocolsDistribution { get; set; }
        public ProtocolSummaryItem[] LinkProtocolsSummary { get; set; }

        public KeyValue<DateTime, long>[] TrafficErrors { get; set; }
        public KeyValue<DateTime, long>[] TrafficHistory { get; set; }

        public HostTraffic[] HostsTraffic { get; set; }

        //public KeyValue<string, long>[] DetectedApplicationProtocols { get; set; }
        //public uint L2MalformedFrames { get; set; }
        //public KeyValue<string, long>[] L2Types { get; set; }
        //public KeyValue<string, long>[] L3Types { get; set; }
        //public KeyValue<string, long>[] L4Types { get; set; }

        //public long TotalTransferedBytes { get; set; }

        //public long ExtractedTcpBytes { get; set; }
        //public float TcpLostPerc { get; set; }
        //public long TcpLostBytes { get; set; }
        //public long FlowBytes { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}