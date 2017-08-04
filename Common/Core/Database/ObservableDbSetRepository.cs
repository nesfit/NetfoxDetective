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
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using AlphaChiTech.Virtualization.Interfaces;
using Castle.Windsor;
using PostSharp.Patterns.Contracts;

namespace Netfox.Core.Database
{
    public class ObservableDbSetRepository<T> : IRepository<T>,INotifyImmediately, INotifyCollectionChanged, ISynchronized where T : class, IEntity
    {
        private Type[] _dbSetBaseClasses;
        private bool _isNotifyImmidiately;

        public bool IsNotifyImmidiately
        {
            get => this._isNotifyImmidiately;
            set
            {
                if(value)
                {
                    if (this.DbSet is INotifyCollectionChanged iNotifyCollectionChangedDbSet)
                    {
                        iNotifyCollectionChangedDbSet.CollectionChanged -= this.INotifyCollectionChangedDbSetOnCollectionChanged; //To ensure that only one instance is subscribed
                        iNotifyCollectionChangedDbSet.CollectionChanged += this.INotifyCollectionChangedDbSetOnCollectionChanged;
                        this.DbContext.DbSetChanged -= this.DbContextOnDbSetChanged;

                        
                    }
                }
                else
                {
                    if (this.DbSet is INotifyCollectionChanged iNotifyCollectionChangedDbSet)
                    {
                        iNotifyCollectionChangedDbSet.CollectionChanged -= this.INotifyCollectionChangedDbSetOnCollectionChanged;
                        this.DbContext.DbSetChanged -= this.DbContextOnDbSetChanged; //To ensure that only one instance is subscribed
                        this.DbContext.DbSetChanged += this.DbContextOnDbSetChanged;
                    }
                }
               
                this._isNotifyImmidiately = value;
            }
        }
        

        private void INotifyCollectionChangedDbSetOnCollectionChanged(object o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Debugger.Log(0, "Debug", "NEW OBSERVABLE COLLECTION CHANGED ");
            this.CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
        }

        public ObservableDbSetRepository(
            [NotNull] IWindsorContainer investigationOrAppWindsorContainer,
            [NotNull] IObservableNetfoxDBContext dbContext,
            Expression<Func<T, Boolean>> filter = null,
            IEnumerable<string> eagerLoadProperties = null)
        {
            this.InvestigationOrAppWindsorContainer = investigationOrAppWindsorContainer;
            this.DbContext = dbContext;

            this.DbContext.DbSetChanged += this.DbContextOnDbSetChanged;
            this.DbContext.ActivateDbSetChangeNotifier(typeof(T));
            this.Ctx.Configuration.LazyLoadingEnabled = true;
            this.Ctx.Configuration.AutoDetectChangesEnabled = false;
            this.EagerLoadProperties = eagerLoadProperties;
            this.QueryFilter = filter;
        }

        private void DbContextOnDbSetChanged(DbContext sender, DbSetChangedArgs args) { this.OnChange(); }

        public int Count => this.ConcurrentLock(() => this.QueryWithoutEagerLoading.Count());

        public Type[] DbSetBaseClasses
            =>
                this._dbSetBaseClasses
                ?? (this._dbSetBaseClasses =
                    AppDomain.CurrentDomain.GetAssemblies()?
                        .SelectMany(assembly => assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PersistentAttribute), true).Any()))
                        .ToArray());

        private Expression<Func<T, bool>> QueryFilter { get; }
        private IWindsorContainer InvestigationOrAppWindsorContainer { get; }
        private IObservableNetfoxDBContext DbContext { get; set; }
        private DbContext Ctx => this.DbContext as DbContext;

        private IQueryable<T> QueryWithoutEagerLoading => this.QueryFilter == null? this.DbSet : this.DbSet.Where(this.QueryFilter);

        private IQueryable<T> Query
        {
            get
            {
                IQueryable<T> query = this.DbSet;

                query = query.OrderBy(i => i.FirstSeen);

                if(this.QueryFilter != null) { query = query.Where(this.QueryFilter); }
                if(this.EagerLoadProperties != null) { query = this.EagerLoadProperties.Aggregate(query, (current, propertyName) => current.Include(propertyName)); }

                return query;
            }
        }

        private DbSet<T> DbSet => this.Ctx.Set<T>();
        private IEnumerable<string> EagerLoadProperties { get; set; }
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Delete(T entity)
        {
            this.ConcurrentLock(() =>
            {
                this.DbSet.Remove(entity);
                this.Ctx.SaveChanges();
            });
        }

        public bool Exists(Guid key)
        {
            var single = this.SingleOrDefault(key);
            return single != null;
        }

        /// <summary>
        ///     Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public T FindById(Guid id)
        {
            return this.ConcurrentLock(() => this.DbSet.Find(id));
        }

        public T Insert(T entity)
        {
            //return AsyncHelpers.RunSync(() => this.InsertAsync(entity));
            try
            {
                return this.ConcurrentLock(() =>
                {
                    this.DbSet.Add(entity);
                    this.Ctx.SaveChanges();
                    return this.DbSet.First();
                });
            }
            catch(DbEntityValidationException e)
            {
                foreach(var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach(var ve in eve.ValidationErrors) { Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage); }
                }
                throw;
            }
        }

        /// <summary>
        ///     Inserts the specified current.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <returns></returns>
        public async Task<T> InsertAsync(T current)
        {
            try
            {
                await this.ConcurrentLock(async () =>
                {
                    this.DbSet.Add(current);
                    await this.Ctx.SaveChangesAsync();
                });
            }
            catch(DbEntityValidationException e)
            {
                foreach(var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach(var ve in eve.ValidationErrors) { Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage); }
                }
                throw;
            }

            return await this.DbSet.FirstAsync();
        }

        public T Single(Guid key)
        {
            var single = this.SingleOrDefault(key);
            if(single != null) { return single; }
            throw new InvalidOperationException($"No element with GUID={key} found.");
        }

        public T SingleOrDefault(Guid key)
        {
            if(typeof(IEntity).IsAssignableFrom(typeof(T))) { return null; }
            return this.ConcurrentLock(() => this.Query.SingleOrDefault(m => m.Id == key));
        }

        public void Update(T entity)
        {
            //AsyncHelpers.RunSync(() => this.UpdateAsync(entity));
            this.DbContext.Entry(entity).State = EntityState.Modified;

            try {
                this.ConcurrentLock(() => this.Ctx.SaveChanges());
            }
            catch(DbEntityValidationException e)
            {
                foreach(var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the f  ollowing validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach(var ve in eve.ValidationErrors) { Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage); }
                }
                throw;
            }
        }

        /// <summary>
        ///     Inserts the specified current.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <returns></returns>
        public async Task UpdateAsync(T current)
        {
            this.DbContext.Entry(current).State = EntityState.Modified;

            try {
                await this.ConcurrentLock(async () => await this.Ctx.SaveChangesAsync());
            }
            catch(DbEntityValidationException e)
            {
                foreach(var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the f  ollowing validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach(var ve in eve.ValidationErrors) { Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage); }
                }
                throw;
            }
        }

        public bool IsSynchronized { get; } = true;
        public object SyncRoot => this.DbContext;
        public bool Contains(T item) { return this.ConcurrentLock(() => this.Query.Any(i => i.Id == item.Id)); }

        /// <summary>
        ///     Deletes the specified identifier permanently.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            var current = await this.ConcurrentLock(async () => await this.FindByIdAsync(id));
            if(current != null)
            {
                this.DbSet.Remove(current);
                await this.Ctx.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<T> FindByIdAsync(Guid id)
        {
            return await this.ConcurrentLock(async () => await this.DbSet.FindAsync(id));
        }

        public int IndexOf(T item)
        {
            //TODO fix .AsEnumerable() workaround
            return this.ConcurrentLock(() => this.Query.AsEnumerable().TakeWhile(i => i.Id != item.Id).Count());
        }

        public void OnChange()
        {
            Debugger.Log(0, "Debug", "NEW OBSERVABLE COLLECTION CHANGED ");
            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public T[] SkipTake(int skip, int take) { return this.ConcurrentLock(() => this.Query.Skip(skip).Take(take).ToArray()); }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(this.Ctx != null)
                {
                    this.Ctx.Dispose();
                    this.DbContext = null;
                }
            }
        }

        private Task<T> ConcurrentLock(Func<Task<T>> func)
        {
            lock(this.DbContext) { return func.Invoke(); }
        }

        private Task ConcurrentLock(Func<Task> func)
        {
            lock(this.DbContext) { return func.Invoke(); }
        }

        private TLock ConcurrentLock<TLock>(Func<TLock> func)
        {
            lock(this.DbContext) { return func.Invoke(); }
        }

        private void ConcurrentLock(Action func)
        {
            lock(this.DbContext) { func.Invoke(); }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="BaseRepository" /> class.
        /// </summary>
        ~ObservableDbSetRepository()
        {
            this.Dispose(false);
        }
    }
}