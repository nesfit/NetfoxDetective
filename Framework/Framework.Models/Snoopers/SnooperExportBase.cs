// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using Netfox.Core.Database;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Models.Exports;
using Netfox.Framework.Models.Snoopers.Enums;
using Netfox.Framework.Models.Snoopers.Exceptions;
using PostSharp.Patterns.Model;

//using Netfox.Framework.CoreController;

namespace Netfox.Framework.Models.Snoopers
{ 
    // Abstract class for collection of exported objects.
    // The reason for this class to be abstract is this:
    //   each object can be identified by the type of its derived class
    //  and there is no need for some type of enum property inside this class
    //  just for type identification (identification will be used for filtering)
    /// <summary>
    [Persistent]
    public abstract class SnooperExportBase : IEntity,  IExportSource
    {
        private List<SnooperExportedObjectBase> _currentObjectBase; //TODO What does this mean? How could be more current export objects?
        private IExportSource _exportSource;
        private DateTime _timeStampFirst;
        private DateTime _timeStampLast;

        private Guid _sourceCaptureId;

        // public SnooperExportBase() { }

        protected SnooperExportBase() //todo protected
        {
            this.ExportValidity = ExportValidity.ValidWhole;
            this._currentObjectBase = new List<SnooperExportedObjectBase>();
        }

       
        public virtual List<SnooperExportedObjectBase> ExportObjects { get; set; } = new List<SnooperExportedObjectBase>();

        public ExportValidity ExportValidity { get; set; }

        
        [IgnoreAutoChangeNotification]
        public IExportSource ExportSource
        {
            get
            {
                return this._exportSource
                       ?? (this._exportSource = this.ExportObjects?.FirstOrDefault()?.ExportSources.FirstOrDefault())
                       ?? (this._exportSource = this.Reports.FirstOrDefault()?.ExportSource);
            }
            set { this._exportSource = value; }
        }

        public virtual List<ExportReport> Reports { get; set; } = new List<ExportReport>();

        public ExportContext ExportContext { get; private set; } = ExportContext.Unknown;

        [NotMapped]
        public SnooperExportedObjectBase CurrentObjectBase
        {
            get { return null; }
            internal set
            {
                if (this._currentObjectBase.Contains(value)) { throw new ExportedObjectAlreadyAdded(); }
                this._currentObjectBase.Add(value);
            }
        }

        [IgnoreAutoChangeNotification]
        [Column(TypeName = "DateTime2")]
        public DateTime TimeStampFirst
        {
            get
            {
                if(this._timeStampFirst != DateTime.MinValue) return this._timeStampFirst;
                    if (this.ExportObjects != null && this.ExportObjects.Any()) { return this.ExportObjects.First().TimeStamp; }
                return this._timeStampFirst;
            }
            set { this._timeStampFirst = value; }
        }

        [IgnoreAutoChangeNotification]
        [Column(TypeName = "DateTime2")]
        public DateTime TimeStampLast
        {
            get
            {
                if (this._timeStampFirst != DateTime.MinValue && this._timeStampFirst != DateTime.MaxValue) return this._timeStampFirst;
                if (this.ExportObjects != null && this.ExportObjects.Any()) { return this.ExportObjects.Last().TimeStamp; }
                return this._timeStampLast;
            }
            set { this._timeStampLast = value; }
        }


        public void AddExportObject(SnooperExportedObjectBase snooperExportedObject)
        {
            //Console.WriteLine("AddExportObject()");
            if (this.ExportObjects == null) { this.ExportObjects = new List<SnooperExportedObjectBase>(); }
            if (!snooperExportedObject.ExportSources.Any()) { throw new EmptySourceInSnooperExportedObject(); }
            if (this.ExportContext == ExportContext.Unknown) { throw new UnknownExportContext(); }
            // adding to list of exported objects
            this.ExportObjects.Add(snooperExportedObject);
            // getting rid of the reference to it
            //this._currentObjectBase = null;
            this._currentObjectBase.Remove(snooperExportedObject);

            switch (snooperExportedObject.ExportValidity)
            {
                case ExportValidity.ValidWhole:
                    break;
                case ExportValidity.ValidFragment:
                    if (this.ExportValidity != ExportValidity.Malformed) { this.ExportValidity = ExportValidity.ValidFragment; }
                    break;
                case ExportValidity.Malformed:
                    this.ExportValidity = ExportValidity.Malformed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

       public void AddExportReport(
            ExportReport.ReportLevel level,
            String sourceComponent,
            String description,
            IEnumerable<IExportSource> sourceExportData,
            Exception exception = null) => this.AddExportReport(level, sourceComponent, description, null, sourceExportData, exception);

        public void CheckExportState()
        {
            if (this.ExportContext != ExportContext.Done) { throw new WrongExportContext(); }
            if (this._currentObjectBase.Any()) { throw new UnprocessedSnooperExportedObjectBase(); }
        }

        public void DiscardExportObject(SnooperExportedObjectBase snooperExportedObject)
        {
            //if(snooperExportedObject != this._currentObjectBase) { throw new UnknownSnooperExportedObject(); }
            if (!this._currentObjectBase.Contains(snooperExportedObject)) { throw new UnknownSnooperExportedObject(); }
            this._currentObjectBase.Remove(snooperExportedObject);
        }

        public void OnAfterDataExporting()
        {
            //if(this._currentObjectBase != null) { throw new UnprocessedSnooperExportedObjectBase(); }
            if (this._currentObjectBase.Any())
            {
                foreach (var objectBase in this._currentObjectBase) { Console.WriteLine(objectBase.ToString()); }
                throw new UnprocessedSnooperExportedObjectBase();
            }
            this.ExportContext = ExportContext.Done;
        }

        public void OnAfterProtocolParsing() => this.ExportContext = ExportContext.Unknown;
        public void OnBeforeDataExporting() => this.ExportContext = ExportContext.Data;
        public void OnBeforeProtocolParsing() => this.ExportContext = ExportContext.Parsing;

        public override String ToString()
        {
            var sb = new StringBuilder();
            if (this.ExportObjects != null)
            {
                sb.AppendLine("  exported objects: " + this.ExportObjects.Count);
                foreach (var exportedObject in this.ExportObjects)
                {
                    sb.AppendLine("    object:");
                    sb.AppendLine("      timestamp: " + exportedObject.TimeStamp);
                    sb.Append("      validity: ");
                    switch (exportedObject.ExportValidity)
                    {
                        case ExportValidity.ValidWhole:
                            sb.AppendLine("valid (whole)");
                            break;
                        case ExportValidity.ValidFragment:
                            sb.AppendLine("valid (fragment)");
                            break;
                        case ExportValidity.Malformed:
                            sb.AppendLine("malformed");
                            break;
                        default:
                            sb.AppendLine("unkown");
                            break;
                    }
                    if (exportedObject.Reports != null)
                    {
                        sb.AppendLine("      reports: " + exportedObject.Reports.Count);
                    }
                    else
                    {
                        sb.AppendLine("      reports: 0");
                    }
                    foreach (var line in exportedObject.ToString().Split('\n'))
                    {
                        sb.Append("      ");
                        sb.AppendLine(line);
                    }
                }
            }
            else
            {
                sb.AppendLine("  exported objects: 0");
            }

            sb.Append("  validity: ");
            switch (this.ExportValidity)
            {
                case ExportValidity.ValidWhole:
                    sb.AppendLine("valid (whole)");
                    break;
                case ExportValidity.ValidFragment:
                    sb.AppendLine("valid (fragment)");
                    break;
                case ExportValidity.Malformed:
                    sb.AppendLine("malformed");
                    break;
                default:
                    sb.AppendLine("unkown");
                    break;
            }
            if (this.Reports != null)
            {
                sb.AppendLine("  reports: " + this.Reports.Count);
                foreach (var report in this.Reports)
                {
                    sb.AppendLine("   report:");
                    sb.AppendLine(report.ToString());
                }
            }
            else
            {
                sb.AppendLine("  reports: 0");
            }
            sb.AppendLine("  timestamp: " + this.TimeStampFirst);
            return sb.ToString();
        }

        private void AddExportReport(
            ExportReport.ReportLevel level,
            String sourceComponent,
            String description,
            String detail,
            IEnumerable<IExportSource> sourceExportData,
            Exception exception = null)
        {
            var exportReport = new ExportReport
            {
                Level = level,
                SourceComponent = sourceComponent,
                Description = description,
                Detail = detail,
                Exception = exception,
                ExportSources = sourceExportData
            };

            switch (this.ExportContext)
            {
                case ExportContext.Data:
                    if (this._currentObjectBase.Any())
                    {
                        this._currentObjectBase.Last().Reports.Add(exportReport);
                        this._currentObjectBase.Last().ExportValidity = ExportValidity.Malformed;
                    }
                    else
                    {
                        this.Reports.Add(exportReport);
                        this.ExportValidity = ExportValidity.Malformed;
                    }
                    break;
                case ExportContext.Parsing:
                    this.Reports.Add(exportReport);
                    this.ExportValidity = ExportValidity.Malformed;
                    break;
                case ExportContext.Done:
                    this.Reports.Add(exportReport);
                    this.ExportValidity = ExportValidity.Malformed;
                    break;
                case ExportContext.Unknown:
                    this.Reports.Add(exportReport);
                    this.ExportValidity = ExportValidity.Malformed;
                    break;
            }
        }
        
        public Guid SourceCaptureId {
            get
            {
                if(this._sourceCaptureId == Guid.Empty)
                {
                    this._sourceCaptureId = (this.ExportSource is L7Conversation) ? ((this.ExportSource as L7Conversation).Captures.First().Id): Guid.Empty;
                }

                if (this._sourceCaptureId == Guid.Empty)
                {
                    this._sourceCaptureId = (this.ExportSource is L7PDU) ? ((this.ExportSource as L7PDU).L7Conversation.Captures.First().Id) : Guid.Empty;
                }

                return this._sourceCaptureId;
            }
            set { this._sourceCaptureId = value; }
        }

        #region Implementation of IExportSource
        
        [IgnoreAutoChangeNotification]
        [NotMapped]
        public IPEndPoint SourceEndPoint => this.ExportSource?.SourceEndPoint;

        [NotMapped]
        [IgnoreAutoChangeNotification]
        public IPEndPoint DestinationEndPoint => this.ExportSource?.DestinationEndPoint;

        #region Implementation of IEntity
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; set; }
        #endregion

        #endregion
    }
    
}