using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Netfox.Core.Database;
using Netfox.Core.Models;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Interfaces;
using Netfox.Web.BL.DTO;
using Netfox.Persistence;
using Riganti.Utils.Infrastructure.Core;
using Netfox.Web.DAL.Properties;
using System.Net;
using System.Net.Sockets;
using Castle.Core.Internal;
using Netfox.Core.Interfaces.Model.Exports;
using PacketDotNet;

namespace Netfox.Web.BL.Facades
{
    public class NetfoxAPIFacade
    {

        public IWindsorContainer WindsorContainer { get; private set; }

        public InvestigationInfo InvestigationInfo { get; set; }

        public IFrameworkController FrameworkController { get; set; }

        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCaptures { get; private set; }

        public VirtualizingObservableDBSetPagedCollection<PmFrameBase> PmFrames { get; private set; }

        public VirtualizingObservableDBSetPagedCollection<L3Conversation> L3Conversations { get; private set; }

        public VirtualizingObservableDBSetPagedCollection<L4Conversation> L4Conversations { get; private set; }

        public VirtualizingObservableDBSetPagedCollection<L7Conversation> L7Conversations { get; private set; }

        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCapturesDb { get; set; }

        public VirtualizingObservableDBSetPagedCollection<SnooperExportBase> SnooperExports { get; private set; }

        public Type[] AvailableSnoopersTypes { get; private set; }

        public ISnooper[] AvailableSnoopers { get; set; }

        public ISnooperFactory SnooperFactory { get; set; }

        public string appPath { get; set; }

        public ConvesationStatisticsDTO ConversationsStatistics { get; set; } = new ConvesationStatisticsDTO();

        private const int TransportProtocolsCount = 2;

        public IUnitOfWork UniteOfWork { get; set; }

        public StatsFacade Stats { get; set; }

        public ExportStatisticsDTO ExportStats { get; set; } = new ExportStatisticsDTO();

        public NetfoxAPIFacade(IWindsorContainer container, Guid investigationId, string appPath)
        {
            this.appPath = appPath;
            this.WindsorContainer = container;
            this.SnooperFactory = this.WindsorContainer.Resolve<ISnooperFactory>();
            this.Stats  = this.WindsorContainer.Resolve<StatsFacade>();
            this.AvailableSnoopersTypes = this.SnooperFactory.AvailableSnoopersTypes;
            this.AvailableSnoopers = this.SnooperFactory.AvailableSnoopers;
            this.FrameworkController = this.WindsorContainer.Resolve<IFrameworkController>();
            this.InvestigationInfo = this.WindsorContainer.Resolve<InvestigationInfo>();
            this.InvestigationInfo.Guid = investigationId;
            this.InvestigationInfo.InvestigationName = NetfoxWebSettings.Default.DefaultInvestigationName;
            this.InvestigationInfo.InvestigationsDirectoryInfo = new DirectoryInfo(appPath + NetfoxWebSettings.Default.InvestigationsFolder);
            this.InvestigationInfo.SqlConnectionStringBuilder = new SqlConnectionStringBuilder(NetfoxWebSettings.Default.ConnectionString)
            {
                InitialCatalog = NetfoxWebSettings.Default.InvestigationPrefix + investigationId
            };
            this.WindsorContainer.Register(Component.For<SqlConnectionStringBuilder>().Instance(this.InvestigationInfo.SqlConnectionStringBuilder));
            this.WindsorContainer.Register(Component.For<NetfoxDbContext>().ImplementedBy<NetfoxDbContext>().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<IObservableNetfoxDBContext>().ImplementedBy<NetfoxDbContext>().Named(nameof(IObservableNetfoxDBContext)).LifestyleSingleton());
            var observableNetfoxDBContext = this.WindsorContainer.Resolve<IObservableNetfoxDBContext>();
            observableNetfoxDBContext.RegisterVirtualizingObservableDBSetPagedCollections();
            observableNetfoxDBContext.Database.CreateIfNotExists();

            this.PmCaptures = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmCaptureBase>>();
            this.PmFrames = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>();
            this.L3Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L3Conversation>>();
            this.L4Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L4Conversation>>();
            this.L7Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(new
            {
                eagerLoadProperties = new[]
                {
                    nameof(L7Conversation.UnorderedL7PDUs),
                    nameof(L7Conversation.ConversationFlowStatistics),
                    nameof(L7Conversation.L4Conversation),
                    $"{nameof(L7Conversation.UnorderedL7PDUs)}.{nameof(L7PDU.UnorderedFrameList)}"
                }
            });

            this.SnooperExports = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            this.SnooperExports.IsNotifyImmidiately = true;
        }

       
        public FileInfo PrepareCaptureForProcessing(string filePath)
        {
            var originalFile = new FileInfo(filePath);
            if (!originalFile.Exists)
            {
                originalFile = new FileInfo(Path.Combine(@"..\", filePath));
            }
            if (!originalFile.Exists)
            {
                throw new ArgumentException($"Capture file does not exists: {filePath}", nameof(filePath));
            }
            var newFile =
                originalFile.CopyTo(Path.Combine(this.InvestigationInfo.SourceCaptureDirectoryInfo.CreateSubdirectory(originalFile.Name + Guid.NewGuid()).FullName,
                    originalFile.Name));
            return newFile;
        }

        public void ProcessCapture(string capturePath)
        {
            this.InvestigationInfo.InvestigationsDirectoryInfo = GetInvestigationsDirectory();
            this.InvestigationInfo.CreateFileStructure();
            var captureFile = this.PrepareCaptureForProcessing(capturePath);
            this.FrameworkController.ProcessCapture(captureFile);

            var capture = this.PmCaptures.SingleOrDefault(c => c.RelativeFilePath.Contains(captureFile.Directory.Name));
            this.ComputeConversationStatistics(capture.Id, captureFile.Length);

            Stats.Insert(ConversationsStatistics, capture.Id, capture.RelativeFilePath.Split('\\').Last(), InvestigationInfo.Guid);

        }

        public void ExportData(List<string> availableSnoopers)
        {
            var captureIds = this.PmCaptures.Select(c => c.Id).ToList();
            foreach(var captureId in captureIds)
            {
                this.ExportData(availableSnoopers, captureId);
            }
            
        }

        public void ExportData(List<string> availableSnoopers, Guid captureId)
        {
            var exportedProtocols = this.InternalExportData(availableSnoopers, captureId);
            this.AfterExport(captureId, exportedProtocols);
        }

        private List<string> InternalExportData(List<string> availableSnoopers, Guid captureId)
        {
            var snoppers = availableSnoopers.Except(this.Stats.GetExportedProtocols(captureId));
            var exportedProtocols = this.SnooperFactory.AvailableSnoopers.Where(s => snoppers.Contains(s.GetType().FullName)).ToArray();

            var L7 = this.L7Conversations.Where(c => c.Captures.Any(cc => cc.Id == captureId)).ToArray();
            this.FrameworkController.ExportData(exportedProtocols, L7, new DirectoryInfo(appPath + NetfoxWebSettings.Default.InvestigationsFolder + NetfoxWebSettings.Default.InvestigationPrefix + InvestigationInfo.Guid + "/Exports"), true);

            var snooperExports = this.SnooperExports.Where(e => e.SourceCaptureId == captureId);
            this.FrameworkController.ExportData(exportedProtocols, snooperExports, new DirectoryInfo(appPath + NetfoxWebSettings.Default.InvestigationsFolder + NetfoxWebSettings.Default.InvestigationPrefix + InvestigationInfo.Guid + "/Exports"));

            return exportedProtocols.Select(s => s.GetType().FullName).ToList();
        }

        private void AfterExport(Guid captureId, List<string> exportedProtocols)
        {
            this.ComputeExportStatistics();
            this.Stats.UpdateExportedProtocols(captureId, exportedProtocols);
            this.Stats.UpdateExportStats(this.InvestigationInfo.Guid, this.ExportStats);
        }

        private DirectoryInfo GetInvestigationsDirectory()
        {
            var dir = new DirectoryInfo(@appPath + NetfoxWebSettings.Default.InvestigationsFolder);
            dir.Create();
            return dir;
        }

        private void ComputeExportStatistics()
        {
            var ExportedObjects = this.SnooperExports.SelectMany(s => s.ExportObjects);
            this.ExportStats.TotalExportedObject = ExportedObjects.Count();
            this.ExportStats.TotalMessage = ExportedObjects.OfType<IChatMessage>().Count();
            this.ExportStats.TotalCalls = ExportedObjects.OfType<ICall>().Count();
            this.ExportStats.TotalEmail = ExportedObjects.OfType<IEMail>().Count();
            this.ExportStats.TotalOther = this.ExportStats.TotalExportedObject - (this.ExportStats.TotalMessage + this.ExportStats.TotalCalls + this.ExportStats.TotalEmail);

        }

        private void ComputeConversationStatistics(Guid captureId, long captureSize)
        {
            var l7Conversations = this.PmCaptures.SingleOrDefault(c => c.Id == captureId).L7Conversations.ToArray();

            var hosts = new List<IPAddress>();
            var protoDistribution = new Dictionary<string, long>();
            var missingBytesListNew = new SortedList<DateTime, KeyValue<DateTime, long>>();
            var historyListNew = new SortedList<DateTime, KeyValue<DateTime, long>>();

            ConversationsStatistics.CaptureStart = DateTime.MaxValue;
            ConversationsStatistics.CaptureFinish = DateTime.MinValue;

            foreach (var l7Conversation in l7Conversations)
            {
                if (!l7Conversation.L7PDUs.Any())
                {
                    // TODO: Is this correct?
                    continue;
                }

                // TO L3 foreach
                if (this.ConversationsStatistics.CaptureStart > l7Conversation.FirstSeen.ToUniversalTime())
                {
                    this.ConversationsStatistics.CaptureStart = l7Conversation.FirstSeen.ToUniversalTime();
                }

                if (this.ConversationsStatistics.CaptureFinish < l7Conversation.LastSeen.ToUniversalTime())
                {
                    this.ConversationsStatistics.CaptureFinish = l7Conversation.LastSeen.ToUniversalTime();
                }

                switch (l7Conversation.L3ProtocolType)
                {
                    case AddressFamily.InterNetwork:
                        this.ConversationsStatistics.IPv4Conversations++;
                        break;
                    case AddressFamily.InterNetworkV6:
                        this.ConversationsStatistics.IPv6Conversations++;
                        break;
                }

                hosts.Add((IPAddress) l7Conversation.SourceEndPoint.Address);
                hosts.Add((IPAddress) l7Conversation.DestinationEndPoint.Address);

                // To L4 foreach
                switch (l7Conversation.L4ProtocolType)
                {
                    case IPProtocolType.TCP:
                        this.ConversationsStatistics.TcpConversations++;
                        this.ConversationsStatistics.UpFlowTcpLostBytes += l7Conversation.UpConversationStatistic?.MissingBytes ?? 0;
                        this.ConversationsStatistics.DownFlowTcpLostBytes += l7Conversation.DownConversationStatistic?.MissingBytes ?? 0;
                        this.ConversationsStatistics.TotalTcpBytes += ((long?)l7Conversation.ConversationStats?.Bytes) ?? 0;
                        break;
                    case IPProtocolType.UDP:
                        this.ConversationsStatistics.UdpConversations++;
                        this.ConversationsStatistics.TotalUdpBytes += ((long?)l7Conversation.ConversationStats?.Bytes) ?? 0;
                        break;
                }

                // TO L7 statistics
                this.ConversationsStatistics.UpFlowFrames += l7Conversation.UpConversationStatistic?.Frames ?? 0;
                this.ConversationsStatistics.DownFlowFrames += l7Conversation.DownConversationStatistic?.Frames ?? 0;
                this.ConversationsStatistics.UpFlowBytes += l7Conversation.UpConversationStatistic?.Bytes ?? 0;
                this.ConversationsStatistics.DownFlowBytes += l7Conversation.DownConversationStatistic?.Bytes ?? 0;

                //TODO how this should be implemented? What happens if there are more app tags at once
                string apptag;
                if (l7Conversation.ApplicationTags.IsNullOrEmpty()) { apptag = "unknown"; }
                else if (l7Conversation.ApplicationTags.Count() == 1)
                {
                    apptag = l7Conversation.ApplicationTags.First();
                    if (apptag.IsNullOrEmpty()) apptag = "unknown";
                }
                else { apptag = "multiple-protocols"; }

                if (l7Conversation.ConversationStats == null) continue;

                if (!protoDistribution.ContainsKey(apptag)) { protoDistribution.Add(apptag, l7Conversation.ConversationStats.Bytes); }
                else { protoDistribution[apptag] += l7Conversation.ConversationStats.Bytes; }


                // Traffic errors and history
                if (l7Conversation.FirstSeen != DateTime.MinValue && l7Conversation.FirstSeen != DateTime.MaxValue)
                {
                    var ct = l7Conversation.FirstSeen.ToUniversalTime();

                    KeyValue<DateTime, long> tmp;
                    if (!missingBytesListNew.TryGetValue(ct, out tmp))
                    {
                        tmp = new KeyValue<DateTime, long>(ct, l7Conversation.ConversationStats.MissingBytes);
                        missingBytesListNew.Add(ct, tmp);
                    }
                    else { tmp.Value += l7Conversation.ConversationStats.MissingBytes; }

                    if (!historyListNew.TryGetValue(ct, out tmp))
                    {
                        tmp = new KeyValue<DateTime, long>(ct, l7Conversation.ConversationStats.Bytes);
                        historyListNew.Add(ct, tmp);
                    }
                    else { tmp.Value += l7Conversation.ConversationStats.Bytes; }
                }
            }

            this.ConversationsStatistics.UniqueHostsCount = (uint)hosts.Distinct().Count();

            //TODO: hack, Cant bind KeyValuePair (value type)
            // this.ConversationsStatistics.AppProtocolsDistribution = protoDistribution.ToArray();
            this.ConversationsStatistics.ReckognizedProtocolsCount = (uint)protoDistribution.Count();
            this.ConversationsStatistics.AppProtocolsDistribution = new KeyValue<string, long>[this.ConversationsStatistics.ReckognizedProtocolsCount];
            this.ConversationsStatistics.AppProtocolsSummary = new ProtocolSummaryItem[this.ConversationsStatistics.ReckognizedProtocolsCount];
            var i = 0;
            foreach (var proto in protoDistribution)
            {
                this.ConversationsStatistics.AppProtocolsDistribution[i] = new KeyValue<string, long>(proto.Key, proto.Value);
                this.ConversationsStatistics.AppProtocolsSummary[i] =
                    new ProtocolSummaryItem(proto.Key, proto.Value, (float)proto.Value / this.ConversationsStatistics.TotalFlowBytes * 100);
                i++;
            }

            this.ConversationsStatistics.TransportProtocolsDistribution = new KeyValue<string, long>[TransportProtocolsCount];
            this.ConversationsStatistics.TransportProtocolsSummary = new ProtocolSummaryItem[TransportProtocolsCount];
            this.ConversationsStatistics.TransportProtocolsDistribution[0] = new KeyValue<string, long>("TCP", this.ConversationsStatistics.TotalTcpBytes);
            this.ConversationsStatistics.TransportProtocolsDistribution[1] = new KeyValue<string, long>("UDP", this.ConversationsStatistics.TotalUdpBytes);
            var tcpPerc = (float)this.ConversationsStatistics.TotalTcpBytes / (this.ConversationsStatistics.TotalTcpBytes + this.ConversationsStatistics.TotalUdpBytes) * 100;
            var udpPerc = (float)this.ConversationsStatistics.TotalUdpBytes / (this.ConversationsStatistics.TotalTcpBytes + this.ConversationsStatistics.TotalUdpBytes) * 100;
            this.ConversationsStatistics.TransportProtocolsSummary[0] = new ProtocolSummaryItem("TCP", this.ConversationsStatistics.TotalTcpBytes, tcpPerc);
            this.ConversationsStatistics.TransportProtocolsSummary[1] = new ProtocolSummaryItem("UDP", this.ConversationsStatistics.TotalUdpBytes, udpPerc);

            // traffic errors
            this.ConversationsStatistics.TrafficErrors = missingBytesListNew.Values.ToArray();

            // history
            this.ConversationsStatistics.TrafficHistory = historyListNew.Values.ToArray();

            // Filters limits
            var l3Conversations = this.PmCaptures.SingleOrDefault(c => c.Id == captureId).L3Conversations.ToArray();
            var l4Conversations = this.PmCaptures.SingleOrDefault(c => c.Id == captureId).L4Conversations.ToArray();

            this.ConversationsStatistics.L3FramesMax = (long)Math.Ceiling(l3Conversations.Max(c => c.Frames.Count) / 10.0) * 10;
            this.ConversationsStatistics.L3FramesMin = (l3Conversations.Min(c => c.Frames.Count) / 10) * 10;
            this.ConversationsStatistics.L3BytesMax = (long)Math.Ceiling(l3Conversations.Max(c => c.ConversationStats.Bytes) / 10.0) * 10;
            this.ConversationsStatistics.L3BytesMin = (l3Conversations.Min(c => c.ConversationStats.Bytes) / 10) * 10;

            this.ConversationsStatistics.L4FramesMax = (long)Math.Ceiling(l4Conversations.Max(c => c.Frames.Count) / 10.0) * 10;
            this.ConversationsStatistics.L4FramesMin = (l4Conversations.Min(c => c.Frames.Count) / 10) * 10;
            this.ConversationsStatistics.L4BytesMax = (long)Math.Ceiling(l4Conversations.Max(c => c.ConversationStats.Bytes) / 10.0) * 10;
            this.ConversationsStatistics.L4BytesMin = (l4Conversations.Min(c => c.ConversationStats.Bytes) / 10) * 10;

            this.ConversationsStatistics.L7FramesMax = (long)Math.Ceiling(l7Conversations.Max(c => c.Frames.Count) / 10.0) * 10;
            this.ConversationsStatistics.L7FramesMin = (l7Conversations.Min(c => c.Frames.Count) / 10) * 10;
            this.ConversationsStatistics.L7BytesMax = (long)Math.Ceiling(l7Conversations.Max(c => c.ConversationStats.Bytes) / 10.0) * 10;
            this.ConversationsStatistics.L7BytesMin = (l7Conversations.Min(c => c.ConversationStats.Bytes) / 10) * 10;

            this.ConversationsStatistics.FramesBytesMax = (long)Math.Ceiling(l3Conversations.SelectMany(c => c.Frames).Max(f => f.OriginalLength) / 10.0) * 10;
            this.ConversationsStatistics.FramesBytesMin = (l3Conversations.SelectMany(c => c.Frames).Min(f => f.OriginalLength) / 10) * 10;

            this.ConversationsStatistics.CaptureStart = this.ConversationsStatistics.CaptureStart.ToLocalTime();
            this.ConversationsStatistics.CaptureFinish = this.ConversationsStatistics.CaptureFinish.ToLocalTime();

            this.ConversationsStatistics.FilterDurationMin = new DateTime(
                this.ConversationsStatistics.CaptureStart.Ticks - (this.ConversationsStatistics.CaptureStart.Ticks % TimeSpan.TicksPerSecond),
                this.ConversationsStatistics.CaptureStart.Kind
            );
            this.ConversationsStatistics.FilterDurationMax = new DateTime(
                this.ConversationsStatistics.CaptureFinish.Ticks - (this.ConversationsStatistics.CaptureFinish.Ticks % TimeSpan.TicksPerSecond),
                this.ConversationsStatistics.CaptureFinish.Kind
            );

            // investigation stats
            this.ConversationsStatistics.TotalFrames = this.PmCaptures.SingleOrDefault(c => c.Id == captureId).Frames.Count();
            this.ConversationsStatistics.TotalL3Conversations = l3Conversations.Count();
            this.ConversationsStatistics.TotalL4Conversations = l4Conversations.Count();
            this.ConversationsStatistics.TotalL7Conversations = l7Conversations.Count();
            this.ConversationsStatistics.Size = captureSize;

        }
    }

    public class KeyValue<TKeyType, TValueType>
    {
        #region Constructors
        public KeyValue() { }

        public KeyValue(TKeyType key, TValueType value)
        {
            this.Key = key;
            this.Value = value;
        }

        public KeyValue(TKeyType key, TValueType value, object oRef)
        {
            this.Key = key;
            this.Value = value;
            this.Ref = oRef;
        }
        #endregion

        #region Properties
        public object Ref { get; private set; }
        public TKeyType Key { get; private set; }
        public TValueType Value { get; set; }
        #endregion
    }
}
