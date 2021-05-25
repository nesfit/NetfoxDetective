using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using Netfox.Core.Database;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Models.Exports;
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models.Snoopers
{
    // <summary>
    // Abstract class for collection of exported objects.
    // The reason for this class to be abstract is this:
    //   each object can be identified by the type of its derived class
    //  and there is no need for some type of enum property inside this class
    //  just for type identification (identification will be used for filtering)
    // </summary>
    [Persistent]
    public abstract class SnooperExportedObjectBase : IEntity, IExportSource, IExportBase
    {
        private IExportSource _exportSource;
        private List<ExportReport> _reports;
        private IPEndPoint _sourceEndPoint;
        private IPEndPoint _destinationEndPoint;


        private string _sourceEndPointString;
        private string _destinationEndPointString;

        protected SnooperExportedObjectBase()
        {
        } //EF

        protected SnooperExportedObjectBase(SnooperExportBase exportBase)
        {
            exportBase.CurrentObjectBase = this;
        }

        [NotMapped] public List<ExportReport> Reports => this._reports ?? (this._reports = new List<ExportReport>());

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IExportSource ExportSource =>
            this._exportSource ?? (this._exportSource = this.ExportSources?.FirstOrDefault());

        [NotMapped] public Type ExportObjectType => this.GetType();

        public virtual List<IExportSource> ExportSources { get; set; } = new List<IExportSource>();

        [NotMapped] public ExportValidity ExportValidity { get; set; } = ExportValidity.ValidWhole;

        [NotMapped]
        public DateTime TimeStamp
        {
            get
            {
                if (this.FirstSeen == DateTime.MinValue)
                {
                    if (this.Reports?.Count > 0) return this.Reports.Min(r => r.FirstSeen);
                }

                return this.FirstSeen;
            }
            set { this.FirstSeen = value; }
        }

        public string SourceEndpointString
        {
            get { return this.ExportSource?.SourceEndPoint.ToString() ?? this._sourceEndPointString; }
            set { this._sourceEndPointString = value; }
        }

        public string DestinationEndpointString
        {
            get { return this.ExportSource?.DestinationEndPoint.ToString() ?? this._destinationEndPointString; }
            set { this._destinationEndPointString = value; }
        }

        //public IEnumerable<L7PDU> SourcePDUs => this.ExpandSourceList(this.ExportSources).Cast<L7PDU>();

        //private IEnumerable<IExportSource> ExpandSourceItem(IExportSource source)
        //{
        //    var pduList = new List<IExportSource>();
        //    var expandList = new List<IExportSource>();

        //    if(source is L7PDU) { pduList.Add(source); }
        //    else if(source is ILxConversation) { expandList.Add(source); }
        //    else if(source is SnooperExportBase) { expandList.Add(source); }
        //    else
        //    { throw new ArgumentException("source"); }

        //    pduList.AddRange(this.ExpandSourceList(expandList));

        //    return pduList;
        //}

        //private IEnumerable<IExportSource> ExpandSourceList(IEnumerable<IExportSource> sourceList)
        //{
        //    var expandedList = new List<IExportSource>();

        //    foreach(var source in sourceList) { expandedList.AddRange(this.ExpandSourceItem(source)); }

        //    return expandedList;
        //}

        #region Implementation of IExportSource

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IPEndPoint SourceEndPoint
        {
            get { return this.ExportSource?.SourceEndPoint ?? this._sourceEndPoint; }
            set
            {
                this.SourceEndpointString = value.ToString();
                this._sourceEndPoint = value;
            }
        }

        [IgnoreAutoChangeNotification]
        [NotMapped]
        public IPEndPoint DestinationEndPoint
        {
            get { return this.ExportSource?.DestinationEndPoint ?? this._destinationEndPoint; }
            set
            {
                this.DestinationEndpointString = value.ToString();
                this._destinationEndPoint = value;
            }
        }

        #region Implementation of IEntity

        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; set; }

        #endregion

        #endregion
    }
}