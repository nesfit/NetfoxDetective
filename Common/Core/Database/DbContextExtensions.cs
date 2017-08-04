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
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Netfox.Core.Database
{
    public static class DbContextExtensions
    {
        private static Type[] _dbSetBaseClasses;

        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            return objectContext.GetTableName<T>();
        }
        public static string GetTableName<T>(this IObservableNetfoxDBContext context) where T : class
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            return objectContext.GetTableName<T>();
        }
        public static string GetTableName(this IObservableNetfoxDBContext context, Type type) 
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            return objectContext.GetTableName(type);
        }

        public static Type[] DbSetBaseClasses
          =>
              _dbSetBaseClasses
              ?? (_dbSetBaseClasses =
                  AppDomain.CurrentDomain.GetAssemblies()?.SelectMany(assembly => assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(PersistentAttribute), true).Any())).ToArray())
          ;

        public static string GetTableName<T>(this ObjectContext context) where T : class
        {
            try
            {
                var baseType = DbSetBaseClasses.First(t => t.IsAssignableFrom(typeof(T)));
                var method = typeof(ObjectContext).GetMethods().First(m => m.IsGenericMethod && m.Name == "CreateObjectSet");
                var generic = method.MakeGenericMethod(baseType);
                dynamic query = generic.Invoke(context, null); //objectContext exists only for base types in DB

                var sql = query.ToTraceString();
                var regex = new Regex("FROM (?<table>.*) AS");
                var match = regex.Match(sql);
                var table = match.Groups["table"].Value;
                return table;
            }
            catch(ArgumentException)
            {
                //dynamically added types, default mapping
                return $"[dbo].[{typeof(T).Name}s]";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Debugger.Break();
                throw;
            }
        }

        public static string GetTableName(this ObjectContext objectContext, Type type)
        {
            var metadata = objectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var table = mapping
                .EntityTypeMappings.First()
                .Fragments.First()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            var schemaName = (string)table.MetadataProperties["Schema"].Value ?? table.Schema;
            var tableName = (string)table.MetadataProperties["Table"].Value ?? table.Name;
            return $"[{schemaName}].[{tableName}]";
        }
    

    public static void ReloadNavigationProperty<TEntity, TElement>(
        this DbContext context,
        TEntity entity,
        Expression<Func<TEntity, ICollection<TElement>>> navigationProperty)
        where TEntity : class
        where TElement : class
        {
            context.Entry(entity).Collection<TElement>(navigationProperty).Query();
        }

        public static void ReloadEntity<TEntity>(
       this DbContext context,
       TEntity entity)
       where TEntity : class
        {
            context.Entry(entity).Reload();
        }

        public static void Filter<TContext, TParentEntity, TCollectionEntity>(this TContext context, Expression<Func<TContext, IDbSet<TParentEntity>>> path, Expression<Func<TParentEntity, ICollection<TCollectionEntity>>> collection, Expression<Func<TCollectionEntity, Boolean>> filter)

        where TContext : DbContext

          where TParentEntity : class, new()

           where TCollectionEntity : class

       {

           (context as IObjectContextAdapter).ObjectContext.ObjectMaterialized += delegate(Object sender, ObjectMaterializedEventArgs e)

            {

              if (e.Entity is TParentEntity)

              {

                   String navigationProperty = collection.ToString().Split('.')[1];

                  DbCollectionEntry col = context.Entry(e.Entity).Collection(navigationProperty);

                   col.CurrentValue = new FilteredCollection<TCollectionEntity>(null, col, filter);

               }

          };

      }
    }
}
