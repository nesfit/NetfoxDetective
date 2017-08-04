using System;

namespace EntityFramework.MappingAPI
{
    public interface IDbMapping
    {
        /// <summary>
        /// Tables in database
        /// </summary>
        ITableMapping[] Tables { get; }

        /// <summary>
        /// Get table mapping by entity type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ITableMapping this[Type type] { get; }

        /// <summary>
        /// Get table mapping by entity type full name
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        ITableMapping this[string typeFullName] { get; }
    }
}