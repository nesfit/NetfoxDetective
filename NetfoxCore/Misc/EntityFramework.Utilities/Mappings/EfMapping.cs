using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace EntityFramework.Utilities.Mappings
{
    /// <summary>
    /// Represents that mapping between entity types and tables in an EF model
    /// </summary>
    public class EfMapping
    {
        /// <summary>
        /// Mapping information for each entity type in the model
        /// </summary>
        public Dictionary<Type, TypeMapping> TypeMappings { get; set; }

        /// <summary>
        /// Initializes an instance of the EfMapping class
        /// </summary>
        /// <param name="db">The context to get the mapping from</param>
        public EfMapping(DbContext db)
        {
            this.TypeMappings = new Dictionary<Type, TypeMapping>();

            var metadata = ((IObjectContextAdapter) db).ObjectContext.MetadataWorkspace;

            //EF61Test(metadata);

            // Conceptual part of the model has info about the shape of our entity classes
            var conceptualContainer = metadata.GetItems<EntityContainer>(DataSpace.CSpace).Single();

            // Storage part of the model has info about the shape of our tables
            var storeContainer = metadata.GetItems<EntityContainer>(DataSpace.SSpace).Single();

            // Object part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection) metadata.GetItemCollection(DataSpace.OSpace));

            // Loop thru each entity type in the model
            foreach (var set in conceptualContainer.BaseEntitySets.OfType<EntitySet>())
            {
                // Find the mapping between conceptual and storage model for this entity set
                var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == set);


                var typeMapping = new TypeMapping
                {
                    TableMappings = new List<TableMapping>(),
                    EntityType = GetClrType(metadata, objectItemCollection, set)
                };

                this.TypeMappings.Add(typeMapping.EntityType, typeMapping);

                var tableMapping = new TableMapping
                {
                    PropertyMappings = new List<PropertyMapping>(),
                };
                var mappingToLookAt = mapping.EntityTypeMappings.FirstOrDefault(m => m.IsHierarchyMapping) ??
                                      mapping.EntityTypeMappings.First();
                tableMapping.Schema = mappingToLookAt.Fragments[0].StoreEntitySet.Schema;
                tableMapping.TableName = mappingToLookAt.Fragments[0].StoreEntitySet.Table ??
                                         mappingToLookAt.Fragments[0].StoreEntitySet.Name;
                typeMapping.TableMappings.Add(tableMapping);

                Action<Type, System.Data.Entity.Core.Mapping.PropertyMapping, string> recurse = null;
                recurse = (t, item, path) =>
                {
                    if (item is ComplexPropertyMapping)
                    {
                        var complex = item as ComplexPropertyMapping;
                        foreach (var child in complex.TypeMappings[0].PropertyMappings)
                        {
                            recurse(t, child, path + complex.Property.Name + ".");
                        }
                    }
                    else if (item is ScalarPropertyMapping)
                    {
                        var scalar = item as ScalarPropertyMapping;
                        tableMapping.PropertyMappings.Add(new PropertyMapping
                        {
                            ColumnName = scalar.Column.Name,
                            DataType = scalar.Column.TypeName,
                            DataTypeFull = GetFullTypeName(scalar),
                            PropertyName = path + item.Property.Name,
                            ForEntityType = t
                        });
                    }
                };

                Func<MappingFragment, Type> getClr = m =>
                {
                    return GetClrTypeFromTypeMapping(metadata, objectItemCollection,
                        m.TypeMapping as EntityTypeMapping);
                };

                if (mapping.EntityTypeMappings.Any(m => m.IsHierarchyMapping))
                {
                    var withConditions =
                        mapping.EntityTypeMappings.Where(m => m.Fragments[0].Conditions.Any()).ToList();
                    tableMapping.TPHConfiguration = new TPHConfiguration
                    {
                        ColumnName = withConditions.First().Fragments[0].Conditions[0].Column.Name,
                        Mappings = new Dictionary<Type, string>()
                    };
                    foreach (var item in withConditions)
                    {
                        tableMapping.TPHConfiguration.Mappings.Add(
                            getClr(item.Fragments[0]),
                            ((ValueConditionMapping) item.Fragments[0].Conditions[0]).Value.ToString()
                        );
                    }
                }

                foreach (var entityType in mapping.EntityTypeMappings)
                {
                    foreach (var item in entityType.Fragments[0].PropertyMappings)
                    {
                        recurse(getClr(entityType.Fragments[0]), item, "");
                    }
                }

                //Inheriting propertymappings contains duplicates for id's. 
                tableMapping.PropertyMappings = tableMapping.PropertyMappings.GroupBy(p => p.PropertyName)
                    .Select(g =>
                        g.OrderByDescending(outer =>
                            g.Count(inner => inner.ForEntityType.IsSubclassOf(outer.ForEntityType))).First())
                    .ToList();
                foreach (var item in tableMapping.PropertyMappings)
                {
                    if ((mappingToLookAt.EntityType ?? mappingToLookAt.IsOfEntityTypes[0]).KeyProperties.Any(p =>
                        p.Name == item.PropertyName))
                    {
                        item.IsPrimaryKey = true;
                    }
                }
            }
        }

        private string GetFullTypeName(ScalarPropertyMapping scalar)
        {
            if (scalar.Column.TypeName == "nvarchar" || scalar.Column.TypeName == "varchar")
            {
                return string.Format("{0}({1})", scalar.Column.TypeName, scalar.Column.MaxLength);
            }

            if (scalar.Column.TypeName == "decimal" || scalar.Column.TypeName == "numeric")
            {
                return string.Format("{0}({1},{2})", scalar.Column.TypeName, scalar.Column.Precision,
                    scalar.Column.Scale);
            }

            return scalar.Column.TypeName;
        }

        private Type GetClrTypeFromTypeMapping(MetadataWorkspace metadata, ObjectItemCollection objectItemCollection,
            EntityTypeMapping mapping)
        {
            return GetClrType(metadata, objectItemCollection, mapping.EntityType ?? mapping.IsOfEntityTypes.First());
        }

        private static dynamic GetProperty(string property, object instance)
        {
            var type = instance.GetType();
            return type.InvokeMember(property, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, instance, null);
        }

        private string GetFullName(XElement propertyMapping)
        {
            if (propertyMapping.Parent.Name.LocalName == "ComplexProperty")
            {
                return GetFullName(propertyMapping.Parent) + "." + propertyMapping.Attribute("Name").Value;
            }

            return propertyMapping.Attribute("Name").Value;
        }

        private static Type GetClrType(MetadataWorkspace metadata, ObjectItemCollection objectItemCollection,
            EntitySet set)
        {
            return GetClrType(metadata, objectItemCollection, set.ElementType);
        }

        private static Type GetClrType(MetadataWorkspace metadata, ObjectItemCollection objectItemCollection,
            EntityTypeBase type)
        {
            return metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .Select(objectItemCollection.GetClrType)
                .Single(e => e.Name == type.Name);
        }

        private static XDocument GetEdmx(DbContext db)
        {
            XDocument doc;
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(
                    memoryStream, new XmlWriterSettings
                    {
                        Indent = true
                    }))
                {
                    EdmxWriter.WriteEdmx(db, xmlWriter);
                }

                memoryStream.Position = 0;

                doc = XDocument.Load(memoryStream);
            }

            return doc;
        }
    }
}