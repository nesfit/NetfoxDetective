/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using AlphaChiTech.Virtualization.Interfaces;
using Castle.Windsor;
using Netfox.Core.Attributes;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Models;
using Netfox.Core.Properties;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models.PmLib.Captures
{
    [Serializable]
    [DataContract]
    [Persistent]
    [KnownType(typeof(PmCaptureMnm))]
    [KnownType(typeof(PmCapturePcap))]
    [KnownType(typeof(PmCapturePcapNg))]
    public abstract class PmCaptureBase: IEntity, IConversationsModel, IWindsorContainerChanger
    {
       private readonly object _pcapHashCurrentLock = new object();
        private IObservableCollection<L4Conversation> _l4Conversations;
        private IObservableCollection<L7Conversation> _l7Conversations;
        private IObservableCollection<L3Conversation> _l3Conversations;
        private IObservableCollection<PmFrameBase> _frames;
        private string _pcapHash;
        private FileInfo _fileInfo;
        private IObservableCollection<SnooperExportBase> _snooperExports;
        private NotifyTaskCompletion<byte[]> _pcapHashCurrent;

        protected PmCaptureBase(){}

        protected PmCaptureBase(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo; 
            this.Frames = new WeakConccurentCollection<PmFrameBase>();
            this.L3Conversations = new WeakConccurentCollection<L3Conversation>();
            this.L4Conversations = new WeakConccurentCollection<L4Conversation>();
            this.L7Conversations = new WeakConccurentCollection<L7Conversation>();
        }
        
        [Key]
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime FirstSeen
        {
            get { return (DateTime) (this._firstSeen ?? (this._firstSeen = this.Frames.FirstOrDefault()?.FirstSeen ?? DateTime.MinValue)); }
            set { this._firstSeen = value; }
        }

        public virtual ICollection<CaptureL4> CaptureL4S { get; set; }  = new List<CaptureL4>();

        #region Implementation of IConversationsModel

        [NotMapped]
        public IObservableCollection<PmFrameBase> Frames
        {
            get
            {
                if(this._frames == null)
                {
                        Expression<Func<PmFrameBase, Boolean>> func = f => f.PmCapture.Id == this.Id;
                        this._frames = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>(new
                        {
                            filter = func
                        });
                }
                return this._frames;
            }
            set { this._frames = value; }
        }
        
        public IObservableCollection<L3Conversation> L3Conversations
        {
            get
            {
                if (this._l3Conversations == null)
                {
                    Expression <Func<L3Conversation, Boolean>> func = l3C => l3C.Captures.AsQueryable().Any(c => c.Id == this.Id);
                        this._l3Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L3Conversation>>(new
                        {
                            filter = func,
                            eagerLoadProperties = new[] {nameof(L3Conversation.ConversationFlowStatistics) }
                        });
                }
                return this._l3Conversations;
            }
            set { this._l3Conversations = value; }
        }
        
        
        public IObservableCollection<L4Conversation> L4Conversations
        {
            get
            {
                if (this._l4Conversations == null)
                {
                        Expression<Func<L4Conversation, Boolean>> func = l4C => l4C.Captures.AsQueryable().Any(c => c.Id == this.Id);
                        this._l4Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L4Conversation>>(new
                        {
                            filter = func,
                            eagerLoadProperties = new[] { nameof(L4Conversation.ConversationFlowStatistics) }
                        });
                }
                return this._l4Conversations;
            }
            set { this._l4Conversations = value; }
        }
        
        public  IObservableCollection<L7Conversation> L7Conversations
        {
            get
            {
                if(this._l7Conversations == null)
                {
                   Expression<Func<L7Conversation, Boolean>> func = l7C => l7C.Captures.AsQueryable().Any(c => c.Id == this.Id);
                        this._l7Conversations = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(new
                        {
                            filter = func,
                            eagerLoadProperties = new[] { nameof(L7Conversation.L7PDUs), nameof(L7Conversation.ConversationFlowStatistics), nameof(L7Conversation.L4Conversation), $"{nameof(L7Conversation.UnorderedL7PDUs)}.{nameof(L7PDU.UnorderedFrameList)}" }
                        });
                    
                }
                return this._l7Conversations;
            }
            set { this._l7Conversations = value; }
        }

        [NotMapped]
        public IObservableCollection<SnooperExportBase> SnooperExports
        {
            get
            {
                if (this._snooperExports == null)
                {
                    //this._l3Conversations = new ConcurrentIObservableVirtualizingObservableCollection<L3Conversation>();
                    //TODO FILTER  Expression<Func<SnooperExportBase, Boolean>> func = (SnooperExportBase sne) => sne.Captures.Contains(this);
                    //this._snooperExports = this.ControllerCaptureProcessorWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>(new { filter = func });
                    this._snooperExports = this.InvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
                }
                return this._snooperExports;
            }
        }

        [NotMapped]
        public IObservableCollection<ISnooper> UsedSnoopers { get; } = new ConcurrentIObservableVirtualizingObservableCollection<ISnooper>();
#endregion

        [NotMapped]
        public FileInfo FileInfo
        {
            get
            {
                if(this._fileInfo != null) return this._fileInfo;
                if(this.InvestigationWindsorContainer != null)
                {
                    var investigaitonInfo = this.InvestigationWindsorContainer.Resolve<InvestigationInfo>();
                    this._fileInfo = new FileInfo(Path.Combine(investigaitonInfo.SourceCaptureDirectoryInfo.FullName,this.RelativeFilePath));
                }
                return this._fileInfo;
            }
            set { this._fileInfo = value; }
        }

        public string RelativeFilePath
        {
            get { return this._relativeFilePath ?? (this.FileInfo != null? (this._relativeFilePath = Path.Combine(Path.GetFileName(this.FileInfo.DirectoryName), this.FileInfo.Name)):null); }
            set { this._relativeFilePath = value; }
        }

        public String FilePath => this.FileInfo?.FullName;

        [NotMapped]

        public NotifyTaskCompletion<Byte[]> PcapHashCurrent
        {
            get {
                lock(this._pcapHashCurrentLock)
                {
                    return this._pcapHashCurrent
                           ?? (this._pcapHashCurrent =
                               new NotifyTaskCompletion<Byte[]>(this.ComputePcapHashAsync, () => this.OnPropertyChanged(nameof(this.PcapHashCurrent)), false));
                }
            }
            set { this._pcapHashCurrent = value; }
        }

        [MaxLength(20)]
        public Byte[] PcapHashOriginal { get; set; }

        /// <summary>
        ///     Gets a CaptureProcessor file hash retrieved from the ComputePcapHash() as a string
        /// </summary>
        [IgnoreAutoChangeNotification]
        public String PcapHash => this._pcapHash ?? (this._pcapHash = this.HashToString(this.PcapHashOriginal));


        public Boolean IsChecksumCorrect => this.IsChecksumCorrectAsync;

        public NotifyTaskCompletion<bool> IsChecksumCorrectAsync =>
            this._isChecksumCorrectAsync ??
            (this._isChecksumCorrectAsync = new NotifyTaskCompletion<bool>(async () =>
            {
                var pcapHash = await this.PcapHashCurrent;
                return this.PcapHashOriginal.SequenceEqual(pcapHash);
            }, () => this.OnPropertyChanged(nameof(this.IsChecksumCorrectAsync))));

        private NotifyTaskCompletion<bool> _isChecksumCorrectAsync;


        /// <summary>
        ///     Computes SHA1 hash of PCAP file
        /// </summary>
        /// <returns>Returns 20 B long SHA1 hash</returns>
        [AsyncTask(Title = nameof(ComputePcapHashAsync), Description = "Computing PCAP hash.")]
        public async Task<Byte[]> ComputePcapHashAsync()
        {
            return await Task.Run(() => this.ComputePcapHash());
        }

        public byte[] ComputePcapHash()
        {
            if(this.FileInfo == null) return null;
            BinaryReader binReader = null;
            try
            {
                binReader = this.BinaryReadersPool.GetReader();
                return this.HashAlgorithm.ComputeHash(binReader.BaseStream);
            }
            finally { this.BinaryReadersPool.PutReader(binReader); }
        }

        public HashAlgorithm HashAlgorithm => new MD5CryptoServiceProvider();

        private string HashToString(byte[] hash)
        {
            var sb = new StringBuilder();
            var enumerable = hash?.Select(b => b.ToString("x2"));
            if(enumerable != null)
            {
                foreach(var hex in enumerable) { sb.Append(hex); }
            }
            return sb.ToString();
        }

        private BinaryReadersPool _binaryReadersPool;
        private string _relativeFilePath;
        private DateTime? _firstSeen;
        private object BinaryReadersPoolLock { get; } = new object();
        [NotMapped]
        public BinaryReadersPool BinaryReadersPool
        {
            get
            {
                if (this._binaryReadersPool != null) return this._binaryReadersPool;

                lock (this.BinaryReadersPoolLock)
                {
                    if (this._binaryReadersPool == null) { this._binaryReadersPool = new BinaryReadersPool(this.FileInfo); }
                }
                return this._binaryReadersPool;
            }
        }
        
        #region Implementation of IWindsorContainerChanger
        [NotMapped]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #region Debugging Methods
        /// <summary>
        ///     Slow but safe approach of importing RealFrames ADT from a given file
        /// </summary>
        /// <param name="fileName">Created file name</param>
        /// <returns>Everything went fine ? true : false</returns>
        /// <summary>
        ///     Slow but safe approach when exporting RealFrames ADT into a given file
        /// </summary>
        /// <param name="fileName">File name of index file</param>
        /// <returns>Everything went fine ? true : false</returns>
        /// <summary>
        ///     Function printing detail content of Frame Vector, purely for debugging purposes
        /// </summary>
        /// <summary>
        ///     Function printing brief content of Frame Vector, purely for debugging purposes
        /// </summary>
        /// <summary>
        ///     Prints basic content of PmFrameBase ADT
        /// </summary>
        /// <param name="frame">One particular frame</param>
        //public void PrintFrameInfo(PmFrameBase frame)
        //{
        //    ("----------- FRAME " + frame.FrameIndex + " ------------").PrintInfoEol();
        //    ("FrameOffset> " + frame.FrameOffset).PrintInfoEol();
        //    ("FrameType> " + frame.PmFrameType).PrintInfoEol();
        //    ("LinkType> " + frame.PmLinkType).PrintInfoEol();
        //    ("OriginalLength> " + frame.OriginalLength).PrintInfoEol();
        //    ("IncludedLength> " + frame.IncludedLength).PrintInfoEol();
        //    ("Malformed> " + frame.IsMalformed).PrintInfoEol();
        //    ("L2 offset> " + frame.L2Offset).PrintInfoEol();
        //    ("L3 offset> " + frame.L3Offset).PrintInfoEol();
        //    ("L4 offset> " + frame.L4Offset).PrintInfoEol();
        //    ("L7 offset> " + frame.L7Offset).PrintInfoEol();
        //    ("SrcEndPoint> " + frame.SourceEndPoint).PrintInfoEol();
        //    ("DstEndPoint> " + frame.DestinationEndPoint).PrintInfoEol();
        //    ("IPProtocol> " + frame.IpProtocol).PrintInfoEol();
        //    //PmConsolePrinter.PrintInfoEol("TCP flags>" + frame.TcpFlags);
        //    ("TCP Sequence Number>" + frame.TcpSequenceNumber).PrintInfoEol();
        //    ("TCP Acknowledgement Number>" + frame.TcpAcknowledgementNumber).PrintInfoEol();
        //}
        #endregion


        #region SEARCH

        /// <summary>
        ///     Retrieve L7 PDU as concatenated data from a given frames list
        /// </summary>
        /// <param name="frames">RealFrames list</param>
        /// <returns>Data representing L4 payload</returns>
        public Byte[] GetMessage(List<PmFrameBase> frames)
        {
            //From input array remove all the frames that DO NOT carry any L4Payload (L7PDU)
            frames.RemoveAll(pmFrame => pmFrame.L7PayloadLength < 1);
            //Count total length of retrieved data
            var len = frames.Sum(pmFrame => pmFrame.IncludedLength - (pmFrame.MessageOffset - pmFrame.L2Offset));
            //Return byte array corresponding to 
            var ms = new MemoryStream(new Byte[len], 0, (Int32)len, true, true);
            foreach (var data in frames.Select(pmFrame => pmFrame.L7Data())) { ms.Write(data, 0, data.Length); }
            return ms.GetBuffer();
        }
        
        /// <summary>
        ///     Search all frames in PCAP for occurence of a given set of patterns
        /// </summary>
        /// <param name="patterns">Array of byte array patterns</param>
        /// <returns>Returns list of frames that actually contain any of a given patterns.</returns>
        public PmFrameBase[] SearchFrames(IEnumerable<Byte[]> patterns) => this.Frames.Where(fr => fr.MatchPatterns(patterns)).ToArray();

        /// <summary>
        ///     Search a list of frames for occurence of a given set of patterns
        /// </summary>
        /// <param name="frames">Enumeration of frames</param>
        /// <param name="patterns">Array of byte array patterns</param>
        /// <returns></returns>
        public PmFrameBase[] SearchFrames(IEnumerable<PmFrameBase> frames, IEnumerable<Byte[]> patterns) => frames.Where(fr => fr.MatchPatterns(patterns)).ToArray();
        #endregion
    }
}