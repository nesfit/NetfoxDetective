using System;

namespace EntityFramework.Utilities.Mappings
{
    /// <summary>
    /// Represents the mapping of a property to a column in the database
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// The property chain leading to this property. For scalar properties this is a single value but for Complex properties this is a dot (.) separated list
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The column that property is mapped to
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Used when we have TPH to exclude entities
        /// </summary>
        public Type ForEntityType { get; set; }

        public string DataType { get; set; }

        public bool IsPrimaryKey { get; set; }

        public string DataTypeFull { get; set; }
    }
}