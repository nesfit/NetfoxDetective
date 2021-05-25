using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.InMemory
{
    public sealed class InheritedDbContext : DbContext
    {
        public InheritedDbContext()
        {
        }

        public InheritedDbContext(DbCompiledModel model) : base(model)
        {
        }

        public InheritedDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public InheritedDbContext(string nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString,
            model)
        {
        }

        public InheritedDbContext(DbConnection existingConnection, bool contextOwnsConnection) : base(
            existingConnection, contextOwnsConnection)
        {
        }

        public InheritedDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) :
            base(existingConnection, model, contextOwnsConnection)
        {
        }

        public InheritedDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext,
            dbContextOwnsObjectContext)
        {
        }

        private TestDbSet<TEntity> CreateDbSet<TEntity>(PropertyInfo dbSetPropertyInfo) where TEntity : class
        {
            var mockDbSetType =
                typeof(TestDbSet<>).MakeGenericType(dbSetPropertyInfo.PropertyType.GenericTypeArguments.First());
            var dbSet = Activator.CreateInstance(mockDbSetType) as TestDbSet<TEntity>;
            dbSetPropertyInfo.SetValue(this, dbSet);
            return dbSet;
        }

        private IEnumerable<PropertyInfo> GetAllDbSetProperties()
        {
            return this.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
        }

        #region Overrides of DbContext

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override bool ShouldValidateEntity(DbEntityEntry entityEntry)
        {
            return base.ShouldValidateEntity(entityEntry);
        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            return base.ValidateEntity(entityEntry, items);
        }

        #region Overrides of DbContext

        public override DbSet<TEntity> Set<TEntity>()
        {
            var dbSetPropertyInfo = this.GetAllDbSetProperties()
                .FirstOrDefault(p => p.PropertyType.GenericTypeArguments.Contains(typeof(TEntity)));
            if (dbSetPropertyInfo == null)
            {
                return null;
            }

            var dbSet = dbSetPropertyInfo.GetValue(this) as TestDbSet<TEntity>;
            if (dbSet == null)
            {
                dbSet = this.CreateDbSet<TEntity>(dbSetPropertyInfo);
            }

            return dbSet;
        }

        public override DbSet Set(Type entityType)
        {
            var method = this.GetType().GetMethods().First(m => m.IsGenericMethod && m.Name == "Set");
            var generic = method.MakeGenericMethod(entityType);
            var ret = generic.Invoke(this, null) as DbSet;
            if (ret == null)
                throw new NotSupportedException(
                    "Unit test conversion of DbSet<TEntity> to DbSet is not supported... se MSDN for details");
            return ret;
            ;
        }

        #endregion

        #endregion
    }
}