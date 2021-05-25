using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Frames;
using Netfox.Detective.Models.Base;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Frames;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ConversationVm : DetectiveDataEntityViewModelBase, IDataEntityVm
    {
        #region Memebers

        private QualityFrameVm _selectedQualityFrame;
        private readonly IDetectiveMessenger _messenger;

        #endregion

        #region Constructor

        public ConversationVm(WindsorContainer applicationWindsorContainer, ILxConversation model) : base(
            applicationWindsorContainer, model)
        {
            this.Conversation = model;
            this._messenger = applicationWindsorContainer.Resolve<IDetectiveMessenger>();
        }

        #endregion

        public RelayCommand CShowReassembledStrem => new RelayCommand(() =>
            this.NavigationService.Show(typeof(ReassembledStreamDetailVm), this, true));

        #region Properties

        public ILxConversation Conversation { get; }
        private ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> _Frames;

        public ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> Frames
            =>
                this._Frames
                ?? (this._Frames =
                    new ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase>(
                        this.Conversation.Frames.ToList(), this.ApplicationOrInvestigationWindsorContainer));

        public RelayCommand<FrameVm> CShowFrameDetail =>
            new RelayCommand<FrameVm>(frame => this.NavigationService.Show(typeof(FrameContentVm), frame));

        #region Export

        // public IExportResultsProvider ExportResultsProvider { get; set; }

        //public IEnumerable<ExportVm> AllExportResults
        //{
        //    get
        //    {
        //        if(this.ExportResultsProvider == null) { return null; }

        //        return this.ExportResultsProvider.ExportResults(result => result.CaptureId == this.Conversation.Id && result.ConversationIndex == this.Conversation.Index);
        //    }
        //}
        public IEnumerable<ExportVm> AllExportResults => new List<ExportVm>();

        #endregion

        #region Current Packet

        private FrameVm _currentPacket;

        public FrameVm CurrentPacket
        {
            get { return this._currentPacket; }
            set
            {
                if (this._currentPacket == value)
                {
                    return;
                }

                this._currentPacket = value;
                this.OnPropertyChanged();

                this._messenger.AsyncSend(new ChangedFrameMessage
                {
                    Frame = this._currentPacket,
                    BringToFront = false
                });
            }
        }

        #endregion

        #region Base statistics

        private L7ConversationStatistics L7ConversationStatistics =>
            (this.Conversation.ConversationStats as L7ConversationStatistics);

        public long UpFlowFramesCount => this.Conversation.UpConversationStatistic.Frames;

        public long DownFlowFramesCount => this.Conversation.DownConversationStatistic.Frames;

        public long UpFlowBytes => this.Conversation.UpConversationStatistic.Bytes;

        public long DownFlowBytes => this.Conversation.DownConversationStatistic.Bytes;

        public TimeSpan Duration => this.Conversation.ConversationStats.Duration;

        public double DurationTotalMilliseconds => this.Duration.TotalMilliseconds;

        public DateTime FirstSeen => this.Conversation.FirstSeen;
        public DateTime LastSeen => this.Conversation.LastSeen;

        #endregion

        #region Extended statistics

        public Int64? MalformedFrames => this.Conversation.ConversationStats?.MalformedFrames;

        public Int64? ExtractedBytes => this.L7ConversationStatistics?.ExtractedBytes;

        public Int64? MissingBytes => this.L7ConversationStatistics?.MissingBytes;

        public Int64? MissingFrames => this.L7ConversationStatistics?.MissingFrames;

        public object MissingBytesPerc
        {
            get
            {
                var extractedBytes = this.ExtractedBytes;
                if (extractedBytes != null && extractedBytes != 0)
                {
                    return this.MissingBytes / extractedBytes;
                }

                return 0;
            }
        }

        #endregion

        #region Traffic History

        private IEnumerable CreateTrafficHistory(IEnumerable<FrameVm> frames, bool revert)
        {
            if (frames == null)
            {
                yield break;
            }


            foreach (var p in frames)
            {
                if (p != null)
                {
                    yield return new KeyValue<DateTime, long>(p.TimeStamp,
                        (revert ? -p.Frame.OriginalLength : p.Frame.OriginalLength), p);
                }
            }
        }

        public IEnumerable TrafficHistoryUp =>
            this.CreateTrafficHistory(
                this.Frames.Where(f => this.DeterminFrameDirection(f.Frame) == DaRFlowDirection.up), false);


        public IEnumerable TrafficHistoryDown =>
            this.CreateTrafficHistory(
                this.Frames.Where(f => this.DeterminFrameDirection(f.Frame) == DaRFlowDirection.down), true);

        public IEnumerable TrafficHistory => this.CreateTrafficHistory(this.Frames, false);

        public DaRFlowDirection DeterminFrameDirection(PmFrameBase frame)
        {
            if (this._firstFrame == null) this._firstFrame = this.Frames.FirstOrDefault()?.Frame;
            if (this._firstFrame == null) return DaRFlowDirection.non;

            return frame.SourceEndPoint.Equals(this._firstFrame.SourceEndPoint)
                ? DaRFlowDirection.up
                : DaRFlowDirection.down;
        }

        #endregion

        #region Data quality

        public IEnumerable<KeyValue<DateTime, long>> TrafficHistoryMissing
        {
            get
            {
                foreach (var frameVm in this.Frames.Where(f => f.Frame is PmFrameVirtual))
                {
                    yield return new KeyValue<DateTime, long>(frameVm.TimeStamp, frameVm.FrameSize);
                }
            }
        }

        public IEnumerable MissingDataHistory
        {
            get
            {
                foreach (var frameVm in this.Frames.Where(f => f.Frame is PmFrameVirtual))
                {
                    yield return new ConversationMissingData()
                        {TimeStamp = frameVm.TimeStamp, Length = frameVm.FrameSize};
                }
                //var l7stats = this.Conversation.ConversationStats as L7ConversationStatistics;
                //if(l7stats == null || l7stats.UpMissingData == null || l7stats.DownMissingData == null) { return null; }

                //return l7stats.UpMissingData.Concat(l7stats.DownMissingData);
            }
        }

        public class ConversationMissingData
        {
            private DateTime _timeStamp;

            public DateTime TimeStamp
            {
                get { return this._timeStamp; }
                set { this._timeStamp = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
            }

            public long Length { get; set; }

            public long AfterFrame { get; set; }
        }


        //public ConversationMissingData[] UpMissingData { get; set; }

        //public ConversationMissingData[] DownMissingData { get; set; }


        //private static List<ConversationMissingData> FindMissingDatas(IEnumerable<PmFrameBase> reassembledFrames)
        //{
        //    var missingDatas = new List<ConversationMissingData>();
        //    uint previousFrameIndex = 0;
        //    if (reassembledFrames != null)
        //    {
        //        foreach (var frame in reassembledFrames)
        //        {

        //            if (frame.PmFrameType == PmFrameType.VirtualBlank ||
        //                frame.PmFrameType == PmFrameType.VirutalNoise)
        //            {
        //                var mData = new ConversationMissingData()
        //                {
        //                    AfterFrame = previousFrameIndex,
        //                    TimeStamp = (DateTime)frame.TimeStamp,
        //                    Length = frame.IncludedLength
        //                };

        //                missingDatas.Add(mData);
        //            }


        //            previousFrameIndex = frame.FrameIndex;
        //        }
        //    }
        //    return missingDatas;
        //}

        public IEnumerable DataQualityDistribution
        {
            get
            {
                var l7stats = this.Conversation.ConversationStats as L7ConversationStatistics;
                if (l7stats == null)
                {
                    yield break;
                }

                yield return new KeyValue<string, long>("Extracted Bytes", l7stats.ExtractedBytes);
                yield return new KeyValue<string, long>("Missing Bytes", l7stats.MissingBytes);
            }
        }

        public QualityFrameVm SelectedQualityFrame
        {
            get { return this._selectedQualityFrame; }
            set
            {
                this._selectedQualityFrame = value;
                if (value != null)
                {
                    this.CurrentPacket = this.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new Dictionary<string, object>
                    {
                        {"model", value.Frame},
                        {"investigationOrAppWindsorContainer", this.ApplicationOrInvestigationWindsorContainer}
                    });
                }
            }
        }

        public IEnumerable<QualityFrameVm> FramesQuality(IEnumerable<PmFrameBase> frames)
        {
            if (frames == null)
            {
                yield break;
            }

            foreach (var frame in frames)
            {
                if (frame == null) continue;
                yield return new QualityFrameVm
                {
                    Frame = frame,
                    Length = frame.OriginalLength,
                    Type = frame is PmFrameVirtual ? QualityFrameVm.FrameType.Virtual : QualityFrameVm.FrameType.Normal
                };
            }
        }

        public IEnumerable<QualityFrameVm> FramesQualityUp
        {
            get
            {
                var l7stats = this.Conversation.ConversationStats as L7ConversationStatistics;
                var l7conv = this.Conversation as L7Conversation;

                if (l7conv == null || l7stats == null)
                {
                    return null;
                }

                return this.FramesQuality(l7conv.UpFlowFrames);
            }
        }

        public IEnumerable<QualityFrameVm> FramesQualityDown
        {
            get
            {
                var l7stats = this.Conversation.ConversationStats as L7ConversationStatistics;
                var l7conv = this.Conversation as L7Conversation;

                if (l7conv == null || l7stats == null)
                {
                    return null;
                }

                return this.FramesQuality(l7conv.DownFlowFrames);
            }
        }

        #endregion

        //#region  Reassembled Stream
        private Encoding _l7PlaintextEncoding = Encoding.ASCII;
        private PmFrameBase _firstFrame;

        public EncodingInfo L7PlaintextEncoding
        {
            set
            {
                if (value != null)
                {
                    this._l7PlaintextEncoding = value.GetEncoding();
                }

                this.OnPropertyChanged(nameof(this.ReassembledStream));
            }
        }

        public IEnumerable<ReassembledStreamPduVm> ReassembledStream
        {
            get
            {
                var l7conv = this.Conversation as L7Conversation;
                var reassembledReassembledStream = new ReassembledStreamPduVm[l7conv.L7PDUs.Count()];

                Parallel.ForEach(l7conv.L7PDUs,
                    (L7PDU, state, index) =>
                    {
                        reassembledReassembledStream[index] = new ReassembledStreamPduVm(
                            this.ApplicationOrInvestigationWindsorContainer, L7PDU, this._l7PlaintextEncoding);
                    });

                return reassembledReassembledStream;
            }
        }

        public string Name => this.Conversation.Name;

        //public class DataPDU
        //{
        //    public enum DirectionType
        //    {
        //        Up,
        //        Down,
        //        Unknown
        //    }

        //    public byte[] Data;
        //    public DirectionType Direction;
        //    public DateTime TimeStamp;
        //}

        //private static byte[] MergeBytes(List<byte[]> data, int totalSize = -1)
        //{
        //    byte[] mergedData;
        //    if(totalSize < 0) {
        //        mergedData = new byte[data.Sum(arr => arr.Length)];
        //    }
        //    else
        //    {
        //        mergedData = new byte[totalSize];
        //    }

        //    var writeIdx = 0;
        //    foreach(var byteArr in data)
        //    {
        //        byteArr.CopyTo(mergedData, writeIdx);
        //        writeIdx += byteArr.Length;
        //    }

        //    return mergedData;
        //}

        //public IEnumerable<DataPDU> ReassembledStreamMerged
        //{
        //    get
        //    {
        //        var lasDirection = DaRFlowDirection.non;

        //        var parts = new List<byte[]>();
        //        var totalSize = 0;

        //        foreach(var pdu in this.ReassembledStream)
        //        {
        //            if(lasDirection != pdu.FlowDirection)
        //            {
        //                if(lasDirection != DaRFlowDirection.non && parts.Any())
        //                {
        //                    yield return new DataPDU
        //                    {
        //                        Direction = lasDirection == DaRFlowDirection.up? DataPDU.DirectionType.Up : DataPDU.DirectionType.Down,
        //                        Data = MergeBytes(parts, totalSize),
        //                        TimeStamp = this.Conversation.FirstSeen
        //                    };
        //                }
        //                lasDirection = pdu.FlowDirection;
        //                parts.Clear();
        //                totalSize = 0;
        //            }

        //            totalSize += pdu.Bytes.Length;
        //            parts.Add(pdu.Bytes);
        //        }

        //        yield return new DataPDU
        //        {
        //            Direction = lasDirection == DaRFlowDirection.up? DataPDU.DirectionType.Up : DataPDU.DirectionType.Down,
        //            Data = MergeBytes(parts, totalSize),
        //            TimeStamp = this.Conversation.FirstSeen
        //        };
        //    }
        //}

        //public void SavePDUsAs(IEnumerable<ReassembledStreamPduVm> pdus)
        //{
        //    var path = this.SystemServices.SaveFileDialog(string.Empty, ".bin", "All Files (*.*)|*.*");
        //    if(!string.IsNullOrEmpty(path))
        //    {
        //        try
        //        {
        //            long totalLen = 0;

        //            using(var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        //            {
        //                foreach(var reassembledStreamPduVm in pdus)
        //                {
        //                    totalLen += reassembledStreamPduVm.Bytes.Length;
        //                    writer.Write(reassembledStreamPduVm.Bytes);
        //                }
        //            }

        //            SystemMessage.SendSystemMessage(SystemMessage.Type.Info, "Selected PDUs successfully saved", string.Format("{0} bytes to file \"{1}\"", totalLen, path), this);
        //        }
        //        catch(Exception ex) {
        //            SystemMessage.SendSystemMessage(SystemMessage.Type.Error, "Unable to save selected PDUs", ex.Message, this);
        //        }
        //    }
        //}
        //#endregion

        public void ProtocolsRecognition()
        {
            //ShowProtocolsRecognitionMessage.SendShowProtocolsRecognitionMessage(new ProtocolsRecognitionContext
            //{
            //    Controller = this.Conversation.FwControllerContext,
            //    RecognitionScope = ProtocolsRecognitionContext.RecognitionScopeType.Conversation,
            //    Capture = this.Conversation.FwCaptureContext,
            //    ConversationScope = this.Conversation
            //});
        }

        public ICommand CProtocolsRecognition => new RelayCommand(this.ProtocolsRecognition);

        #endregion
    }
}