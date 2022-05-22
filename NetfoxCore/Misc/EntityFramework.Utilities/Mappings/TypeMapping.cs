using System;
using System.Collections.Generic;

namespace EntityFramework.Utilities.Mappings
{
    /// <summary>
    /// Represents the mapping of an entitiy type to one or mode tables in the database
    ///
    /// A single entity can be mapped to more than one table when 'Entity Splitting' is used
    /// Entity Splitting involves mapping different properties from the same type to different tables
    /// See http://msdn.com/data/jj591617#2.7 for more details
    /// </summary>
    public class TypeMapping
    {
        /// <summary>
        /// The type of the entity from the model
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// The table(s) that the entity is mapped to
        /// </summary>
        public List<TableMapping> TableMappings { get; set; }
    }
}