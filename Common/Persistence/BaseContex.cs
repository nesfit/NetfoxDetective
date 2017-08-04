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
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using EntityFramework.BulkInsert.Extensions;
using Netfox.Core;
using Netfox.Core.Database;
using Netfox.Framework.Models.Snoopers;
using Component = Castle.MicroKernel.Registration.Component;

namespace Netfox.Persistence
{
    public abstract class BaseContex<TContext> : DbContext, IObservableNetfoxDBContext where TContext : DbContext
    {
        public delegate void DbSetChangedHandler(DbContext sender, DbSetChangedArgs args);

        static BaseContex()
        {
            System.Data.Entity.Database.SetInitializer<TContext>(null);
        }

        public IWindsorContainer WindsorContainer { get; }
        public virtual bool IsInMemory { get; } = false;

        protected BaseContex(IWindsorContainer windsorContainer, SqlConnectionStringBuilder sqlConnectionStringBuilder) : base(sqlConnectionStringBuilder.ConnectionString)
        {
            this.WindsorContainer = windsorContainer;
        }
        
        public static DataTable ConvertToDataTable<T>(IEnumerable<T> list)
        {
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            for(var i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                var propertyDescriptor = propertyDescriptorCollection[i];
                var propType = propertyDescriptor.PropertyType;
                if(propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    table.Columns.Add(propertyDescriptor.Name, Nullable.GetUnderlyingType(propType));
                }
                else
                {
                    table.Columns.Add(propertyDescriptor.Name, propType);
                }
            }
            var values = new object[propertyDescriptorCollection.Count];
            foreach(var listItem in list)
            {
                for(var i = 0; i < values.Length; i++) { values[i] = propertyDescriptorCollection[i].GetValue(listItem); }
                table.Rows.Add(values);
            }
            return table;
        }

        public virtual void CheckCreateDatabase()
        {
            if (this.Database.Exists()) return;
            this.InitializeDatabase();
            this.Database.Create();
        }

        public void InitializeDatabase()
        {
            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseAlways<TContext>());
        }

        public virtual void InsertToJunctionTable<T>(IEnumerable<T> joinTableValues) where T : class
        {
            var tableName = typeof(T).Name;
            var dataTable = ConvertToDataTable(joinTableValues);

            using(var connection = new SqlConnection(this.Database.Connection.ConnectionString))
            {
                connection.Open();
                using(var sqlBulkCopy = new SqlBulkCopy(connection))
                {
                    foreach(var column in dataTable.Columns) { sqlBulkCopy.ColumnMappings.Add(column.ToString(), column.ToString()); }
                    sqlBulkCopy.DestinationTableName = tableName;
                    sqlBulkCopy.WriteToServer(dataTable);
                }
            }
        }

        public void RegisterDbSetTypes()
        {
            var method = this.GetType().GetMethods().First(m => m.IsGenericMethod && m.Name == "Set");

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var entityTypes = assembly.GetTypes().Where(this.IsTypePersistent);

                    foreach (var type in entityTypes)
                    {
                        try
                        {
                            var generic = method.MakeGenericMethod(type);
                            var dbSet = generic.Invoke(this, null);
                            var dbSetType = typeof(DbSet<>).MakeGenericType(type);
                            this.WindsorContainer.Register(Component.For(dbSetType).LifestyleSingleton().Instance(dbSet).OnlyNewServices());
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public void RegisterVirtualizingObservableDBSetPagedCollections()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var entityTypes = assembly.GetTypes().Where(this.IsTypePersistent);

                    foreach (var type in entityTypes)
                    {
                        try
                        {
                            var dbSetType = typeof(VirtualizingObservableDBSetPagedCollection<>).MakeGenericType(type);
                            //this.WindsorContainer.Register(Component.For(dbSetType).LifestyleSingleton().OnlyNewServices());
                            this.WindsorContainer.Register(Component.For(dbSetType).LifestyleTransient()); //Has to be transparent otherwise it cannot dynamically load appropriete items by filter
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        protected bool IsTypePersistent(Type t)
        {
            return t.GetCustomAttributes(typeof(PersistentAttribute), true).Any() || 
                   t.GetCustomAttributes(typeof(PersistentNonInheritedAttribute), true).Any();
        }

        public event Netfox.Core.Database.DbSetChangedHandler DbSetChanged;

        public abstract void ActivateDbSetChangeNotifier(Type dbSetType);

        #region BulkInsertBuffered
        public virtual async Task BulkInsertBuffered<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options) where TEntity : class
        {
            await BulkInsertExtension.BulkInsertBuffered(this, entities, options);
        }

        public virtual async Task BulkInsertBuffered<T>(IEnumerable<T> entities, int? batchSize = null) where T : class {
            await this.BulkInsertBuffered(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        public virtual async Task BulkInsertBuffered<T>(IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null) where T : class
        {

            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            await this.BulkInsertBuffered(entities, options);
        }

        public virtual async Task BulkInsertBuffered<T>(IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null) where T : class
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            await this.BulkInsertBuffered(entities, options);
        }
        #endregion
        
        #region BulkInsert
        public virtual void BulkInsert<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options) where TEntity : class
        {
            BulkInsertExtension.BulkInsert(this, entities, options);
        }

        public virtual void BulkInsert<TEntity>(IEnumerable<TEntity> entities, int? batchSize = null) where TEntity : class {
            this.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize);
        }


        public virtual void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null) where TEntity : class
        {

            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            this.BulkInsert(entities, options);
        }

        public virtual void BulkInsert<TEntity>(IEnumerable<TEntity> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null) where TEntity : class
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            this.BulkInsert(entities, options);
        }
        #endregion

        #region BulkInsertAsync
        public virtual async Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options) where TEntity : class
        {
            await BulkInsertExtension.BulkInsertAsync(this, entities, options);
        }
        public virtual  Task BulkInsertAsync<T>(IEnumerable<T> entities, int? batchSize = null) where T : class {
            return this.BulkInsertAsync(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        public virtual  Task BulkInsertAsync<T>(IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null) where T : class
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            return this.BulkInsertAsync(entities, options);
        }


        public virtual Task BulkInsertAsync<T>(IEnumerable<T> entities, IDbTransaction transaction, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int? batchSize = null) where T : class
        {
            var options = new BulkInsertOptions { SqlBulkCopyOptions = sqlBulkCopyOptions };
            if (batchSize.HasValue)
            {
                options.BatchSize = batchSize.Value;
            }
            return this.BulkInsertAsync(entities, options);
        }
        #endregion
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            this.OnModelCreatingImportPersistenceTypes(modelBuilder);
        }

        private void OnModelCreatingImportPersistenceTypes(DbModelBuilder modelBuilder)
        {
            //http://romiller.com/2012/03/26/dynamically-building-a-model-with-code-first/
            var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
            var toTableMethods = typeof(EntityTypeConfiguration<>).GetMethods().Where(m => m.Name == "ToTable");
            var toTableMethod = toTableMethods.First(m => m.GetParameters().Length == 1);
            try
            {
                var snooperLoader = this.WindsorContainer.Resolve<SnooperLoader>();
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Concat(snooperLoader.GetSnoopersAssemblies()))
                {
                    try
                    {
                        var entityTypes = assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PersistentAttribute), true).Any());

                        foreach(var type in entityTypes)
                        {
                            try
                            {
                                var entityMethodReturn = entityMethod.MakeGenericMethod(type).Invoke(modelBuilder, new object[]{});
                                //todo windsor register
                            }
                            catch(Exception ex) {
                                Debug.WriteLine(ex);
                            }
                        }
                    }
                    catch(Exception ex) {
                        Debug.WriteLine(ex);
                    }
                }

                //Table per Type mapping for SnooperExportBase
                modelBuilder.Types().Where(t => typeof(SnooperExportBase).IsAssignableFrom(t) && t != typeof(SnooperExportBase)).Configure(t => t.ToTable(t.ClrType.Name));

                //Table per Type mapping for SnooperExportedObjectBase
                modelBuilder.Types().Where(t => typeof(SnooperExportedObjectBase).IsAssignableFrom(t) && t != typeof(SnooperExportedObjectBase)).Configure(t => t.ToTable(t.ClrType.Name));
            }
            catch(Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        protected virtual void OnDbSetChanged(DbSetChangedArgs args) { this.DbSetChanged?.Invoke(this, args); }

        #region Overrides of DbContext
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.Disposed = true;
        }
        protected bool Disposed = false;
        #endregion
    }
}