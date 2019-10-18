// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Core.Logging;
using Castle.Windsor;
using Netfox.Core;
using Netfox.Core.Database;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Models;
using Netfox.Detective.Models.Base.Detective;

namespace Netfox.Detective.Models.Exports
{
    /// <summary>
    ///     Export group model.
    ///     Class contains children export groups and export results.
    /// </summary>
    [Persistent]
    public class ExportGroup : IWorkspaceObject, IEntity, INotifyPropertyChanged, IWindsorContainerChanger, ILoggable
    {
        public delegate void ExportGroupUpdatedHandler();

        private VirtualizingObservableDBSetPagedCollection<ExportGroup> _exportGroups;
        private IObservableCollection _exports;
        private string _name;
        private Type _snooperExportObjectType;
        private ILogger _logger;
        private ExportGroup() { } //EF

        public ExportGroup(Type snooperExportObjectType, IWindsorContainer investigationWindsorContainer)
        {
            this.SnooperExportObjectType = snooperExportObjectType;
            this.InvestigationWindsorContainer = investigationWindsorContainer;
            this.FirstSeen = DateTime.Now;
        }

        public IObservableCollection Exports
        {
            get
            {
                if(this._exports == null)
                {
                    var snooperExportsCollectionType = typeof(VirtualizingObservableDBSetPagedCollection<>).MakeGenericType(this.SnooperExportObjectType);
                    this._exports = this.InvestigationWindsorContainer.Resolve(snooperExportsCollectionType) as IObservableCollection;
                }
                return this._exports;
            }
        }

        [NotMapped]
        public VirtualizingObservableDBSetPagedCollection<ExportGroup> ExportGroups
        {
            get
            {
                if(this._exportGroups == null)
                {
                    Expression<Func<ExportGroup, Boolean>> func = eg => eg.Parent != null && eg.Parent.Id == this.Id;
                    var dbx = this.InvestigationWindsorContainer.Resolve<IObservableNetfoxDBContext>();
                    this._exportGroups = new VirtualizingObservableDBSetPagedCollection<ExportGroup>(this.InvestigationWindsorContainer, dbx, func);
                }
                return this._exportGroups;
            }
        }

        public ExportGroup Parent { get; set; }

        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        [NotMapped]
        public Type SnooperExportObjectType
        {
            get
            {
                if(this._snooperExportObjectType != null) { return this._snooperExportObjectType; }
                var snooperLoader = this.InvestigationWindsorContainer.Resolve<SnooperLoader>();
                var snooperAssemblies = snooperLoader.GetSnoopersAssemblies();
                if(snooperAssemblies == null) { return this._snooperExportObjectType; }
                foreach(var snooperAssembly in snooperAssemblies)
                {
                    this._snooperExportObjectType = snooperAssembly.GetType(this.SnooperExportObjectTypeFullName);
                    if(this._snooperExportObjectType != null) return this._snooperExportObjectType;
                }
                return this._snooperExportObjectType;
            }
            set
            {
                this._snooperExportObjectType = value;
                this.SnooperExportObjectTypeFullName = value.FullName;
            }
        }

        public string SnooperExportObjectTypeFullName { get; set; }
        public DateTime FirstSeen { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public Guid Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }

        [NotMapped]
        public InvestigationInfo Workspace { get; set; }

        public void Delete(bool notify)
        {
            //if(this.PersistenceProvider != null)
            //{
            //    foreach(var exportGroup in this.ExportGroups) { exportGroup.Delete(false); }

            //    foreach(var exportResult in this.ExportResults) { exportResult.Delete(false); }

            //    this.PersistenceProvider.Delete(this, notify, true);
            //}
            this.Logger?.Info("ExportGroup delete not implemented.");
            //this.PersistenceDelete();
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #region Implementation of ILoggable
        public ILogger Logger
        {
            get { return this._logger ?? (this._logger = this.InvestigationWindsorContainer?.Resolve<ILogger>()); }
            set { this._logger = value; }
        }
        #endregion
    }
}