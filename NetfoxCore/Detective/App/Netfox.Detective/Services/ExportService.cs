using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Core.Attributes;
using Netfox.Core.Database;
using Netfox.Core.Interfaces;
using Netfox.Detective.Models.Exports;
using Netfox.Detective.Models.SourceLog;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Interfaces;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.Services
{
    public class ExportService : DetectiveInvestigationService
    {
        public IWindsorContainer InvestigationWindsorContainer { get; }
        public IFrameworkController FrameworkController { get; }
        public IInvestigationInfo InvestigationInfo { get; }
        public ISnooperFactory SnooperFactory { get; }
        private BgTasksManagerService _bgTasksManagerService;

        public ExportService(IWindsorContainer investigationWindsorContainer, IFrameworkController frameworkController,
            IInvestigationInfo investigationInfo, ISnooperFactory snooperFactory,
            VirtualizingObservableDBSetPagedCollection<ExportGroup> exportGroups) : base()
        {
            this.InvestigationWindsorContainer = investigationWindsorContainer;
            this.FrameworkController = frameworkController;
            this.InvestigationInfo = investigationInfo;
            this.SnooperFactory = snooperFactory;
            var snooperToExportGroupDictionary = new Dictionary<Type, ExportGroup>();
            foreach (var snooper in this.SnooperFactory.AvailableSnoopers)
            {
                try
                {
                    var exportGroup = exportGroups.FirstOrDefault(g => g.Name == snooper.Name && g.Parent == null);
                    if (exportGroup == null)
                    {
                        var snooperExportObjectType = snooper.PrototypeExportObject.GetType();

                        exportGroup = new ExportGroup(snooperExportObjectType, this.InvestigationWindsorContainer)
                        {
                            Name = snooper.Name
                        };
                        exportGroups.Add(exportGroup);
                    }

                    snooperToExportGroupDictionary.Add(snooper.PrototypeExportObject.GetType(), exportGroup);
                }
                catch (Exception ex)
                {
                    this.Logger?.Error("Cannot create export groups", ex);
                    Debugger.Break();
                }
            }

            this.SnooperToExportGroupDictionary = snooperToExportGroupDictionary;
        }

        [IgnoreAutoChangeNotification]
        public BgTasksManagerService BgTasksManagerService
            => this._bgTasksManagerService ?? (this._bgTasksManagerService =
                this.InvestigationWindsorContainer.Resolve<BgTasksManagerService>());

        [IgnoreAutoChangeNotification]
        //public InvestigationVm InvestigationVm => this._investigationVm ?? (this._investigationVm = this.InvestigationWindsorContainer.Resolve<InvestigationVm>());

        private IReadOnlyDictionary<Type, ExportGroup> SnooperToExportGroupDictionary { get; }

        /// <summary>
        ///     todo support exportGroupName, createSubGroup, showExportedObjects
        /// </summary>
        /// <param name="conversations"></param>
        /// <param name="exporters"></param>
        /// <param name="exportGroupName"></param>
        /// <param name="createSubGroup"></param>
        /// <param name="showExportedObjects"></param>
        /// <param name="dontUseApplicationTag"></param>
        /// <param name="finishAction"></param>
        [AsyncTask(Title = nameof(Export), Description = "Exporting application data.")]
        public async Task Export(
            IEnumerable<ILxConversation> conversations,
            IEnumerable<ISnooper> exporters,
            string exportGroupName,
            bool createSubGroup,
            bool showExportedObjects,
            bool dontUseApplicationTag)
        {
            if (exporters == null)
            {
                throw new ArgumentException(@"exporters cannot be null", nameof(exporters));
            }

            await Task.Run(() =>
            {
                var lxConversations = conversations as ILxConversation[] ?? conversations.ToArray();
                this.Logger?.Error(
                    $"Starting export application data (captures): {lxConversations.Count()} conversations to group {exportGroupName}");
                this.ExportDataAction(conversations, null, exporters, exportGroupName, createSubGroup,
                    dontUseApplicationTag);
            });
        }

        [AsyncTask(Title = nameof(Export), Description = "Exporting application data.")]
        public async Task Export(
            IEnumerable<SourceLog> sourceLogs,
            IEnumerable<ISnooper> exporters,
            string exportGroupName,
            bool createSubGroup,
            bool showExportedObjects)
        {
            if (exporters == null)
            {
                throw new ArgumentException(@"exporters cannot be null", nameof(exporters));
            }

            await Task.Run(() =>
            {
                var logs = new SourceLog[sourceLogs.Count()];
                var i = 0;
                foreach (var sourceLog in sourceLogs)
                {
                    logs[i++] = sourceLog;
                } //to overcome ToArray BUG

                this.Logger?.Error(
                    $"Starting export application data (logs): {logs.Count()} conversations to group {exportGroupName}");
                this.ExportDataAction(null, sourceLogs, exporters, exportGroupName, createSubGroup);
            });
        }

        private void ExportDataAction(
            IEnumerable<ILxConversation> conversations,
            IEnumerable<SourceLog> sourceLogs,
            IEnumerable<ISnooper> exporters,
            string exportGroupName,
            bool createSubGroups,
            bool dontUseApplicationTag = false)

        {
            try
            {
                this.Logger?.Info("Exporting application data.");
                this.Logger?.Info("Preparing ...");


                //TODO rewrite this nicer with filtration
                var conversationSnoopers = exporters.Where(s =>
                    s.GetType().GetConstructors().Any(c =>
                        c.GetParameters().Any(p => p.ParameterType == typeof(SelectedConversations))));
                var exportSnoopers = exporters.Where(s => s.GetType().GetConstructors().Any(c =>
                    c.GetParameters().Any(p => p.ParameterType == typeof(IEnumerable<SnooperExportBase>))));
                var sourceLogSnoopers = exporters.Where(s =>
                    s.GetType().GetConstructors().Any(c =>
                        c.GetParameters().Any(p => p.ParameterType == typeof(IEnumerable<FileInfo>))));


                this.FrameworkController.ExportData(conversationSnoopers, conversations,
                    this.InvestigationInfo.ExportsDirectoryInfo, dontUseApplicationTag);

                var exports = this.InvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();

                //(IEnumerable<ISnooper> snoopers, ConcurrentBag<SnooperExportBase> snooperExports, DirectoryInfo exportDirectory)
                this.FrameworkController.ExportData(exportSnoopers, exports,
                    this.InvestigationInfo.ExportsDirectoryInfo);

                if (sourceLogs != null)
                {
                    this.FrameworkController.ExportData(sourceLogSnoopers, sourceLogs.Select(s => s.SourceLogFileInfo),
                        this.InvestigationInfo.ExportsDirectoryInfo);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"Application data export failed!", ex);
                return;
            }
            
            this.Logger?.Info("Export done.");
        }
    }
}