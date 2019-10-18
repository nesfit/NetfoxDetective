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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.InMemory;
using Netfox.Core.Database;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.ViewModels;

namespace Netfox.Persistence
{
    public sealed class NetfoxDbContextInMemory : NetfoxDbContext
    {
        public NetfoxDbContextInMemory(WindsorContainer windsorContainer, SqlConnectionStringBuilder sqlConnectionStringBuilder) : base(windsorContainer,
            sqlConnectionStringBuilder) { }

        public override bool IsInMemory { get; } = true;

        private TestDbSet<TEntity> CreateDbSet<TEntity>() where TEntity : class { return this.CreateDbSet(typeof(TEntity)) as TestDbSet<TEntity>; }

        private object CreateDbSet(Type type) { return this.CreateDbSet(type, null); }

        private object CreateDbSet(Type type, DbSet dbSet)
        {
            var mockDbSetType = typeof(TestDbSet<>).MakeGenericType(type);
            var mockOnAddCallback = this.GetType()
                .GetMethod(nameof(this.OnAddTestDbSetItem), BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance)
                .MakeGenericMethod(type);
            var mockOnAddBulkCallback = this.GetType()
                .GetMethod(nameof(this.OnAddBulkTestDbSetItems), BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance)
                .MakeGenericMethod(type);
            var mockOnRemoveCallback = this.GetType()
                .GetMethod(nameof(this.OnRemoveTestDbSetItem), BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance)
                .MakeGenericMethod(type);

            

            var delegateType = typeof(Action<>).MakeGenericType(type);
            var onAdd = Delegate.CreateDelegate(delegateType, this, mockOnAddCallback);
            var onRemove = Delegate.CreateDelegate(delegateType, this, mockOnRemoveCallback);

            
            var ienumerableType = typeof(IEnumerable<>).MakeGenericType(type);
            var delegateTypeIenumerable = typeof(Action<>).MakeGenericType(ienumerableType);
            var onAddBulk = Delegate.CreateDelegate(delegateTypeIenumerable, this, mockOnAddBulkCallback);

            return Activator.CreateInstance(mockDbSetType, onAdd, onAddBulk, onRemove, dbSet);
        }

        private IEnumerable<PropertyInfo> GetAllDbSetProperties()
        {
            return this.GetType().GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
        }

        private PropertyInfo GetAllGenericDbSetProperties(Type type)
        {
            return this.GetAllDbSetProperties().FirstOrDefault(p => p.PropertyType.GenericTypeArguments.Contains(type));
        }

        private void SetDbSetProperty<TEntity>(DbSet<TEntity> dbSet, PropertyInfo dbSetPropertyInfo) where TEntity : class { dbSetPropertyInfo.SetValue(this, dbSet); }

        #region Overrides of BaseContex<NetfoxDbContext>
        public override void CheckCreateDatabase() { }
        public override void InsertToJunctionTable<T>(IEnumerable<T> joinTableValues) { }

        public override void ActivateDbSetChangeNotifier(Type dbSetType)
        {
            var autoRefreshArgs = this.AutoRefreshArgsDictionary.GetOrAdd(dbSetType, type => new AutoRefreshArgs(dbSetType));

            var lastEntityCount = this.Set(dbSetType).Local.Count;
            var prt = PeriodicalRepetitiveTask.Create((now, ct) => Task.Run(() =>
            {
                var currentEntityCount = this.Set(dbSetType).Local.Count;
                if(lastEntityCount == currentEntityCount) return;
                lastEntityCount = currentEntityCount;
                this.OnDbSetChanged(new DbSetChangedArgs(dbSetType));
            }, ct), autoRefreshArgs.CancellationTokenSource.Token, TimeSpan.FromSeconds(5));
            prt.Post(DateTimeOffset.Now);
        }
        #endregion

        #region BulkInsertBuffered
        public override async Task BulkInsertBuffered<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options)
        {
            await Task.Run(() =>
            {
                var entitiesArray = entities as TEntity[] ?? entities.ToArray();
                this.InjectWindsorContainer(entitiesArray);
                var dbSet = this.Set<TEntity>() as TestDbSet<TEntity>;
                dbSet.AddRange(entitiesArray,false);
            });
        }

        public override async Task BulkInsertBuffered<T>(IEnumerable<T> entities, int? batchSize = null)
        {
            await this.BulkInsertBuffered(entities, SqlBulkCopyOptions.Default, batchSize);
        }

        public override async Task BulkInsertBuffered<T>(IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            await this.BulkInsertBuffered(entities, options);
        }

        public override async Task BulkInsertBuffered<T>(
            IEnumerable<T> entities,
            IDbTransaction transaction,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            await this.BulkInsertBuffered(entities, options);
        }
        #endregion

        #region BulkInsert
        public override void BulkInsert<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            this.InjectWindsorContainer(entitiesArray);
            var dbSet = this.Set<TEntity>() as TestDbSet<TEntity>;
            if(dbSet == null)
            {
                var method = this.GetType().GetMethods().First(m => m.IsGenericMethod && m.Name == nameof(BulkInsert));
                var generic = method.MakeGenericMethod(typeof(TEntity).BaseType);
                var ret = generic.Invoke(this, new object[]
                {
                    entities,
                    options
                });
                return;
            }
            dbSet.AddRange(entitiesArray,false);
        }

        private void InjectWindsorContainer<TEntity>(IEnumerable<TEntity> entities)
        {
            if(!typeof(IWindsorContainerChanger).IsAssignableFrom(typeof(TEntity))) return;
            foreach(var entity in entities) (entity as IWindsorContainerChanger).InvestigationWindsorContainer = this.WindsorContainer;
        }

        public override void BulkInsert<TEntity>(IEnumerable<TEntity> entities, int? batchSize = null) { this.BulkInsert(entities, SqlBulkCopyOptions.Default, batchSize); }

        public override void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            this.BulkInsert(entities, options);
        }

        public override void BulkInsert<TEntity>(
            IEnumerable<TEntity> entities,
            IDbTransaction transaction,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            this.BulkInsert(entities, options);
        }
        #endregion

        #region BulkInsertAsync
        public override Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, BulkInsertOptions options) { return Task.Run(() => { this.BulkInsert(entities, options); }); }

        public override Task BulkInsertAsync<T>(IEnumerable<T> entities, int? batchSize = null) { return this.BulkInsertAsync(entities, SqlBulkCopyOptions.Default, batchSize); }

        public override Task BulkInsertAsync<T>(IEnumerable<T> entities, SqlBulkCopyOptions sqlBulkCopyOptions, int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            return this.BulkInsertAsync(entities, options);
        }

        public override Task BulkInsertAsync<T>(
            IEnumerable<T> entities,
            IDbTransaction transaction,
            SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default,
            int? batchSize = null)
        {
            var options = new BulkInsertOptions
            {
                SqlBulkCopyOptions = sqlBulkCopyOptions
            };
            if(batchSize.HasValue) options.BatchSize = batchSize.Value;
            return this.BulkInsertAsync(entities, options);
        }
        #endregion

        #region Overrides of DbContext
        protected override void OnModelCreating(DbModelBuilder modelBuilder) { base.OnModelCreating(modelBuilder); }

        public override int SaveChanges() { return base.SaveChanges(); }
        public override Task<int> SaveChangesAsync() { return base.SaveChangesAsync(); }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken) { return base.SaveChangesAsync(cancellationToken); }
        protected override bool ShouldValidateEntity(DbEntityEntry entityEntry) { return base.ShouldValidateEntity(entityEntry); }
        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items) { return base.ValidateEntity(entityEntry, items); }

        private ConcurrentDictionary<Type, object> DbSetsNotInDbContext { get; } = new ConcurrentDictionary<Type, object>();

        public override DbSet<TEntity> Set<TEntity>()
        {
            var dbSetPropertyInfo = this.GetAllGenericDbSetProperties(typeof(TEntity));
            if(dbSetPropertyInfo == null)
            {
                var type = typeof(TEntity);
                do
                {
                    if(type == null) throw new InvalidOperationException("The entity type NetfoxDbContextInMemoryTests is not part of the model for the current context.");
                    if(this.IsTypePersistent(type) && !this.IsTypePersistent(type?.BaseType))
                    {
                        this.DbSetsNotInDbContext.GetOrAdd(type, this.CreateDbSet);
                        break;
                    }
                    dbSetPropertyInfo = this.GetAllGenericDbSetProperties(type);

                    if(dbSetPropertyInfo == null) type = type?.BaseType;
                } while(dbSetPropertyInfo == null);

                return this.DbSetsNotInDbContext.GetOrAdd(typeof(TEntity), t => this.CreateDbSet(t, this.Set(type))) as TestDbSet<TEntity>;
            }

            var dbSet = this.GetDbSetProperty<TEntity>(dbSetPropertyInfo);
            if(dbSet == null)
            {
                var type = typeof(TEntity);
                var baseType = type.BaseType;
                while(baseType != typeof(object))
                {
                    type = baseType;
                    baseType = baseType?.BaseType;
                }

                if(type != typeof(TEntity)) dbSet = this.Set(type) as TestDbSet<TEntity>;
                else dbSet = this.CreateDbSet<TEntity>();

                this.SetDbSetProperty(dbSet, dbSetPropertyInfo);
            }

            return dbSet;
        }

        private void OnAddTestDbSetItem<TEntity>(TEntity item)
        {
            var properties = this.GetPersistingProperties<TEntity>();
            foreach(var property in properties)
            {
                var propType = property.PropertyType.IsGenericType? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;
                var dbSet = this.GetTestDbSetForType(propType);
                dbSet.Add(property.GetValue(item));
            }
        }

        private void OnAddBulkTestDbSetItems<TEntity>(IEnumerable<TEntity> items)
        {
            var itemsByType = items.GroupBy(item => item.GetType());
            if(itemsByType.Count() != 1)
            {
                Debugger.Break();
                throw new NotSupportedException($"Only items with the same type can be inserted by {nameof(this.OnAddBulkTestDbSetItems)}");
            }
            var properties = this.GetPersistingProperties<TEntity>();

            foreach(var property in properties)
            {
                var propertyOrAttributeType = property.PropertyType.IsGenericType? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;
                dynamic dbSet = this.GetTestDbSetForType(propertyOrAttributeType);
                
                var  persistingObjects = new List<object>();
                foreach(var entity in items)
                {
                    if (property.PropertyType.IsGenericType) //collection
                    {
                        var itemValues =  property.GetValue(entity) as ICollection;
                        if (itemValues == null) continue;
                        dbSet.AddRange(itemValues);
                    }
                    else
                    { //normal property
                        var item = property.GetValue(entity);
                        if (item == null) continue;
                        persistingObjects.Add(item);
                    }
                }
                dbSet.AddRange(persistingObjects);
            }
        }

        private void OnRemoveTestDbSetItem<TEntity>(TEntity item)
        {
            var properties = this.GetPersistingProperties<TEntity>();
            foreach(var property in properties)
            {
                var propType = property.PropertyType.IsGenericType? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;
                var dbSet = this.GetTestDbSetForType(propType);
                dbSet.Remove(property.GetValue(item));
            }
        }

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _typeCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private IEnumerable<PropertyInfo> GetPersistingProperties<TEntity>()
        {

            return _typeCache.GetOrAdd(typeof(TEntity), this.GetPersistingPropertiesReflection);
        }

        private IEnumerable<PropertyInfo> GetPersistingPropertiesReflection(Type type)
        {
            var propertyInfos = type.GetProperties();
            var properties = propertyInfos
                .Where(p => this.IsTypePersistent(p.PropertyType) || this.IsTypeOfCollectionOPersistent(p.PropertyType) || this.IsDeclaredOnDbContext(p.PropertyType))
                .Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(NotMappedAttribute)));
            return properties;
        }

        private bool IsDeclaredOnDbContext(Type type)
        {
            return this
                .GetType()
                .GetProperties()
                .Any(x => (this.IsTypePersistent(x.PropertyType) || x.PropertyType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(type) && this.IsTypePersistent(x.PropertyType.GetGenericArguments()[0]))
                          && x.PropertyType == type);
        }

        private bool IsTypeOfCollectionOPersistent(Type type)
        {
            var typeIsGenericType = type.IsGenericType;
            if(!typeIsGenericType) return false;

            var genericArgument = type.GetGenericArguments()[0];
            var icollectionType = typeof(ICollection<>).MakeGenericType(genericArgument);
            var isAssignableFrom = icollectionType.IsAssignableFrom(type);
            var isTypePersistent = this.IsTypePersistent(genericArgument);
            return typeIsGenericType && isAssignableFrom && isTypePersistent;
        }

        private TestDbSet<TEntity> GetDbSetProperty<TEntity>(PropertyInfo dbSetPropertyInfo) where TEntity : class
        {
            return dbSetPropertyInfo.GetValue(this) as TestDbSet<TEntity>;
        }

        private IList GetTestDbSetForType(Type type)
        {
            var method = this.GetType().GetMethods().First(m => m.IsGenericMethod && m.Name == "Set");
            var generic = method.MakeGenericMethod(type);
            var testDbSetForType = generic.Invoke(this, null) as IList;
            return testDbSetForType;
        }

        public override DbSet Set(Type entityType)
        {
            var testDbSetGeneric = this.GetTestDbSetForType(entityType);
            var ret = new TestDbSet(testDbSetGeneric, entityType);
            return ret;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this.Disposed) if(disposing) foreach(var autoRefreshArgs in this.AutoRefreshArgsDictionary) autoRefreshArgs.Value.CancellationTokenSource.Cancel();
            base.Dispose(disposing);
        }
        #endregion
    }
}