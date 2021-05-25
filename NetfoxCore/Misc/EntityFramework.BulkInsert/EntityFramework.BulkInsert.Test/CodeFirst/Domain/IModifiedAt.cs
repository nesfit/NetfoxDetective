using System;

namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public interface IModifiedAt
    {
        DateTime? ModifiedAt { get; set; }
    }
}