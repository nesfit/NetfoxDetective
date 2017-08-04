using System;

namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public interface ICreatedAt
    {
        DateTime CreatedAt { get; set; }
    }
}