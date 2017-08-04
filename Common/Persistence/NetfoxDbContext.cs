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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Castle.Windsor;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Netfox.Persistence.JunctionTypes;

namespace Netfox.Persistence
{
    public class NetfoxDbContext : BaseContex<NetfoxDbContext>
    {

        public NetfoxDbContext(IWindsorContainer windsorContainer,SqlConnectionStringBuilder sqlConnectionStringBuilder) : base(windsorContainer,sqlConnectionStringBuilder)
        {
            this.Configuration.LazyLoadingEnabled = true;
            this.InitializeIWindsorContainerChanger();

            this.L3ConversationStatistics = this.Set<L3ConversationStatistics>().Where(s => !(s is L4ConversationStatistics)&& !(s is L7ConversationStatistics));
            this.L4ConversationStatistics = this.Set<L4ConversationStatistics>().Where(s => !(s is L7ConversationStatistics));
            this.L7ConversationStatistics = this.Set<L7ConversationStatistics>();
        }

        private void InitializeIWindsorContainerChanger()
        {
            if(this.IsInMemory) return;
            var objectContext = ((IObjectContextAdapter) this).ObjectContext;
            objectContext.ObjectMaterialized += (sender, e) =>
            {
                if (e?.Entity is IWindsorContainerChanger entity) { entity.InvestigationWindsorContainer = this.WindsorContainer; }
            };
        }

        public virtual DbSet<PmFrameBase> Frames { get; set; }
        public virtual DbSet<PmCaptureBase> PmCaptures { get; set; }
        public virtual DbSet<L3Conversation> L3Conversations { get; set; }
        public virtual DbSet<L4Conversation> L4Conversations { get; set; }
        public virtual DbSet<L7Conversation> L7Conversations { get; set; }
        public virtual DbSet<L7PDU> L7PDUs { get; set; }
        public virtual DbSet<ConversationStatisticsBase> ConversationStatistics { get; set; }
        public virtual IQueryable<L3ConversationStatistics> L3ConversationStatistics { get; } 
        public virtual IQueryable<L4ConversationStatistics> L4ConversationStatistics { get; }
        public virtual IQueryable<L7ConversationStatistics> L7ConversationStatistics { get; }
        public virtual DbSet<SnooperExportBase> SnooperExports { get; set; }
        public virtual DbSet<SnooperExportedObjectBase> SnooperExportedObjects { get; set; }

        //public virtual DbSet<CaptureL4> CaptureL4S { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Types().Where(t => typeof(ConversationStatisticsBase).IsAssignableFrom(t)).Configure(t => t.ToTable(nameof(global::Framework.Models.ConversationStatisticsBase)));

            #region Mandatory configuration for all bulkinserted types that needs junction table!!!
            modelBuilder.Entity<PmCaptureBase>().HasMany(p => p.L3Conversations).WithMany(l => l.Captures).Map(pl =>
            {
                pl.MapLeftKey(nameof(PmCaptureConversation.PmCaptureId));
                pl.MapRightKey(nameof(PmCaptureConversation.ConversationId));
                pl.ToTable(typeof(PmCaptureL3Conversation).Name);
            });

            modelBuilder.Entity<PmCaptureBase>().HasMany(p => p.L4Conversations).WithMany(l => l.Captures).Map(pl =>
            {
                pl.MapLeftKey(nameof(PmCaptureConversation.PmCaptureId));
                pl.MapRightKey(nameof(PmCaptureConversation.ConversationId));
                pl.ToTable(typeof(PmCaptureL4Conversation).Name);
            });

            modelBuilder.Entity<PmCaptureBase>().HasMany(p => p.L7Conversations).WithMany(l => l.Captures).Map(pl =>
            {
                pl.MapLeftKey(nameof(PmCaptureConversation.PmCaptureId));
                pl.MapRightKey(nameof(PmCaptureConversation.ConversationId));
                pl.ToTable(typeof(PmCaptureL7Conversation).Name);
            });
            #endregion
        }

        #region Overrides of BaseContex<NetfoxDbContext>
        private const string SqlDependencyCookie = "MS.SqlDependencyCookie";
        private Dictionary<string, AutoRefreshArgs> Connections { get; } = new Dictionary<string, AutoRefreshArgs>();
        protected ConcurrentDictionary<Type, AutoRefreshArgs> AutoRefreshArgsDictionary { get; } = new ConcurrentDictionary<Type, AutoRefreshArgs>();

        protected class AutoRefreshArgs
        {
            public AutoRefreshArgs(Type dbSetType, SqlCommand sqlCommand)
            {
                this.DbSetType = dbSetType;
                this.SqlCommand = sqlCommand;
            }
            public AutoRefreshArgs(Type dbSetType)
            {
                this.DbSetType = dbSetType;
            }
            public SqlConnection SqlConnection { get; set; }
            public Type DbSetType { get; }
            public SqlCommand SqlCommand { get; }
            public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        }
        public void AutoRefresh(Type dbSetType)
        {
            var oldCookie = CallContext.GetData(SqlDependencyCookie);
            try
            {
                SqlDependency.Start(this.Database.Connection.ConnectionString);

                var autoRefreshArgs = this.AutoRefreshArgsDictionary.GetOrAdd(dbSetType, type => new AutoRefreshArgs(dbSetType, this.GetSqlCommandForAutoRefrest(dbSetType)));
                if(autoRefreshArgs.SqlConnection != null) return; //duplicit call for refresh

                var sqlNotificationCommand = autoRefreshArgs.SqlCommand;
                sqlNotificationCommand.CommandType = CommandType.Text;
                sqlNotificationCommand.Notification = null;

                var sqlConnection = new SqlConnection(this.Database.Connection.ConnectionString); //cannot be in using because it would close channel for backwards notification
                autoRefreshArgs.SqlConnection = sqlConnection;
                sqlConnection.Open();
                sqlNotificationCommand.Connection = sqlConnection;

                var dependency = new SqlDependency(sqlNotificationCommand);
                this.Connections.Add(dependency.Id, autoRefreshArgs);
                CallContext.SetData(SqlDependencyCookie, dependency.Id);

                dependency.OnChange += this.Dependency_OnChange;

                var reader = sqlNotificationCommand.ExecuteReaderAsync().Result;
                Debugger.Log(0, "Debug", "AUTOREFRESH COLLECTION " + dbSetType.BaseType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally { CallContext.SetData(SqlDependencyCookie, oldCookie); }
        }

        private void Dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Info == SqlNotificationInfo.Invalid)
            {
                Debugger.Log(0, "Debug", "INVALID DEPENDENCY ON CHANGE COLLECTION ");
                return;
            }
            try
            {
                var id = ((SqlDependency)sender).Id;
                if(this.Connections.TryGetValue(id, out AutoRefreshArgs autoRefreshArgs))
                {
                    autoRefreshArgs.SqlConnection.Close();
                    autoRefreshArgs.SqlConnection = null;
                    this.OnDbSetChanged(new DbSetChangedArgs(autoRefreshArgs.DbSetType));
                    this.AutoRefresh(autoRefreshArgs.DbSetType);
                }
                else
                {
                    Debugger.Break();
                }
                
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private SqlCommand GetSqlCommandForAutoRefrest(Type dbSetType)
        {
            try
            {
                //var table = this.DbContext.GetTableName<T>(); //slow evaluation of query.ToTraceString();
                var table = this.GetTableName(dbSetType);
                return new SqlCommand($"SELECT [Id] FROM {table}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
        public override void ActivateDbSetChangeNotifier(Type dbSetType)
        {
            this.AutoRefresh(dbSetType);
        }
        
        #endregion


    }
}