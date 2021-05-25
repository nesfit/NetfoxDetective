using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Text;
using Castle.MicroKernel.Handlers;
using Castle.Windsor;
using Netfox.Core.Database;
using Netfox.Core.Models.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.ApplicationProtocolExport.Snoopers
{
    public abstract class SnooperBase : ISnooper
    {
        /// <summary>
        ///     todo this should be DB collection
        /// </summary>
        private readonly List<SnooperExportBase> _snooperExportsList = new List<SnooperExportBase>();

        protected Boolean ForceExportOnAllConversations;
        protected SelectedConversations SelectedConversations;
        protected IEnumerable<SnooperExportBase> SourceExports;

        private DirectoryInfo _exportBaseDirectory;
        private readonly List<SnooperExportBase> _snooperExportCache = new List<SnooperExportBase>();

        /// <summary>
        ///     Parameterless constructor has to be always present in derived classes
        /// </summary>
        protected SnooperBase()
        {
            this.IsPrototype = true;
        }

        protected SnooperBase(
            WindsorContainer investigationWindsorContainer,
            DirectoryInfo exportDirectory)
        {
            this.InvestigationWindsorContainer = investigationWindsorContainer;
            this.ExportBaseDirectory = new DirectoryInfo(Path.Combine(exportDirectory.FullName, this.Name));
            this.SnooperExportCollection = this.InvestigationWindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
        }

        public VirtualizingObservableDBSetPagedCollection<SnooperExportBase> SnooperExportCollection { get; set; }

        protected SnooperBase(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags) : this(investigationWindsorContainer, exportDirectory)
        {
            this.SelectedConversations = conversations;
            this.ForceExportOnAllConversations = ignoreApplicationTags;
        }

        protected SnooperBase(
            WindsorContainer investigationWindsorContainer,
            IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory) : this(investigationWindsorContainer, exportDirectory)
        {
            this.SourceExports = sourceExports;
        }

        protected SnooperBase(WindsorContainer investigationWindsorContainer, IEnumerable<FileInfo> sourceFiles,
            DirectoryInfo exportDirectory) : this(investigationWindsorContainer, exportDirectory)
        {
            //this.CreateExportDirectory();
            this.SourceFiles = sourceFiles;
        }

        public WindsorContainer InvestigationWindsorContainer { get; }

        public IEnumerable<FileInfo> SourceFiles { get; set; }

        protected internal L7Conversation CurrentConversation { get; set; }

        protected internal DirectoryInfo ExportBaseDirectory
        {
            get { return this._exportBaseDirectory; }
            private set
            {
                this._exportBaseDirectory = value;
                this.CreateExportDirectory();
            }
        }

        protected SnooperExportBase SnooperExport { get; set; }

        private Boolean IsPrototype { get; }
        public abstract String Description { get; }

        public abstract Int32[] KnownApplicationPorts { get; }

        //private string _exportDirectory;
        public abstract String Name { get; }
        public abstract String[] ProtocolNBARName { get; }

        public abstract SnooperExportBase PrototypeExportObject { get; }

        public void Run()
        {
            if (this.IsPrototype)
            {
                Console.WriteLine("Prototype of snooper cannot be used to export data!");
                return;
            }

            // create export directory if it doesn't exist
            if (!this.ExportBaseDirectory.Exists)
            {
                this.ExportBaseDirectory.Create();
            }

            try
            {
                this.RunBody();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.SnooperExport.AddExportReport(ExportReport.ReportLevel.Error, this.Name,
                    "unexpected exception caught", new[] {this.CurrentConversation}, ex);
                this.FinalizeExports();
            }

            this.SnooperExport?.CheckExportState();

            this.FlushExportCache();
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Export:");
            sb.AppendLine(this.SnooperExport.ToString());
            return sb.ToString();
        }

        protected void CreateExportDirectory()
        {
            if (!this.ExportBaseDirectory.Exists)
                this.ExportBaseDirectory.Create();
        }

        protected abstract SnooperExportBase CreateSnooperExport();

        protected void OnAfterDataExporting() => this.SnooperExport.OnAfterDataExporting();

        protected void OnAfterProtocolParsing() => this.SnooperExport.OnAfterProtocolParsing();

        protected void OnBeforeDataExporting() => this.SnooperExport.OnBeforeDataExporting();

        protected void OnBeforeProtocolParsing() => this.SnooperExport.OnBeforeProtocolParsing();

        protected void OnConversationProcessingBegin() => this.SnooperExport = this.CreateSnooperExport();

        protected void OnConversationProcessingEnd()
        {
            this.FinalizeExports();
        }

        protected void ProcessAssignedConversations()
        {
            this.SelectedConversations.LockSelectedConversations();

            //Main cycle on all conversations
            while (this.SelectedConversations.TryGetNextConversations(this.GetType(), out var currentConversation,
                out _))
            {
                var selectedL7Conversations = new List<L7Conversation>();

                if (currentConversation is L7Conversation l7Conversation
                ) //todo refactor to SnooperBase.. or make more readable.. to method or somenting...
                {
                    selectedL7Conversations.Add(l7Conversation);
                }
                else if (currentConversation is L4Conversation l4Conversation)
                {
                    selectedL7Conversations.AddRange(l4Conversation.L7Conversations);
                }
                else if (currentConversation is L3Conversation l3Conversation)
                {
                    selectedL7Conversations.AddRange(l3Conversation.L7Conversations);
                }

                foreach (var selectedL7Conversation in selectedL7Conversations)
                {
                    this.CurrentConversation = selectedL7Conversation;

                    if (!this.ForceExportOnAllConversations &&
                        !this.CurrentConversation.IsXyProtocolConversation(this.ProtocolNBARName))
                    {
                        continue;
                    }

                    this.OnConversationProcessingBegin();

                    this.ProcessConversation();

                    this.OnConversationProcessingEnd();
                }
            }
        }

        private void FlushExportCache()
        {
            try
            {
                this.SnooperExportCollection.AddRange(this._snooperExportCache);
            }
            catch (HandlerException)
            {
                Debugger.Break();
            }
        }


        protected abstract void ProcessConversation();

        protected abstract void RunBody();

        private void FinalizeExports()
        {
            this.SnooperExport.OnAfterDataExporting();
            this._snooperExportsList.Add(this.SnooperExport);
            this.OnSnooperExport(this.SnooperExport);
        }

        private void OnSnooperExport(SnooperExportBase snooperExportBase)
        {
            if (this.SnooperExportCollection.IsNotifyImmidiately)
            {
                this.SnooperExportCollection.Add(snooperExportBase);
            }
            else
            {
                this._snooperExportCache.Add(snooperExportBase);
            }
        }
    }
}