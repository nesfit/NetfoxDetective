using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.MappingAPI.Exceptions;
using EntityFramework.MappingAPI.Extensions;
#if EF6
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
#else
using System.Data.Metadata.Edm;
#endif



namespace EntityFramework.MappingAPI.Mappers
{
    internal class DbFirstMapper : MapperBase
    {
        public DbFirstMapper(MetadataWorkspace metadataWorkspace, EntityContainer entityContainer)
            : base(metadataWorkspace, entityContainer)
        {
        }

        protected override PrepareMappingRes PrepareMapping(string typeFullName, EdmType edmItem)
        {
            string tableName = GetTableName(typeFullName);

            // find existing parent storageEntitySet
            // thp derived types does not have storageEntitySet
            EntitySet storageEntitySet;
            EdmType baseEdmType = edmItem;
            while (!EntityContainer.TryGetEntitySetByName(tableName, false, out storageEntitySet))
            {
                if (baseEdmType.BaseType == null)
                {
                    break;
                }
                baseEdmType = baseEdmType.BaseType;
            }

            if (storageEntitySet == null)
            {
                return null;
            }

            var isRoot = baseEdmType == edmItem;
            if (!isRoot)
            {
                var parent = _entityMaps.Values.FirstOrDefault(x => x.EdmType == baseEdmType);
                // parent table has not been mapped yet
                if (parent == null)
                {
                    throw new ParentNotMappedYetException();
                }
            }

            return new PrepareMappingRes { TableName = tableName, StorageEntitySet = storageEntitySet, IsRoot = isRoot, BaseEdmType = baseEdmType };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        protected string GetTableName(string typeFullName)
        {
#if EF6
            // Get the entity type from the model that maps to the CLR type
            var entityType = MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace).Single(e => e.FullName == typeFullName);


            // Get the entity set that uses this entity type
            var entitySet = MetadataWorkspace
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);


#if EF61
            // Find the mapping between conceptual and storage model for this entity set
            var mapping = MetadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity sets (tables) that the entity is mapped
            var tables = mapping
                .EntityTypeMappings.Single()
                .Fragments;

            // Return the table name from the storage entity set
            return tables.Select(f => (string)f.StoreEntitySet.MetadataProperties["Table"].Value ?? f.StoreEntitySet.Name)
                .FirstOrDefault();
#else
            var entitySetMappings = (IEnumerable<object>)MetadataWorkspace.GetItems(DataSpace.CSSpace)
                .First()
                .GetPrivateFieldValue("EntitySetMappings");

            object mapping = entitySetMappings
                .FirstOrDefault(map => map.GetPrivateFieldValue("EntitySet") == entitySet);


            object entityTypeMapping = ((IEnumerable<object>)mapping.GetPrivateFieldValue("EntityTypeMappings")).FirstOrDefault();
            var tables = (IEnumerable<object>)entityTypeMapping.GetPrivateFieldValue("MappingFragments");

            return (string)tables.FirstOrDefault().GetPrivateFieldValue("Table").GetPrivateFieldValue("Name");
#endif

            
#else
            return null;
#endif
        }
        

        protected override Dictionary<string, EntityType> GetTypeMappingsEf4()
        {
            var entityTypes = MetadataWorkspace.GetItems(DataSpace.CSpace)
                .OfType<EntityType>()
                .ToDictionary(x => x.ToString());

            return entityTypes;
        }

        protected override Dictionary<string, TphData> GetTphData()
        {
#if EF4 || EF5
            throw new NotImplementedException("EF4 and EF5 DbFirst is not supported yet");
#else
            return base.GetTphData();
#endif
        }

    }
}