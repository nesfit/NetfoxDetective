using System;
using EntityFramework.BulkInsert.Test.CodeFirst.Domain.ComplexTypes;

namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public class TestUser : EntityWithTypedId<Guid>
    {
        public Contact Contact { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return string.Format("{0} {1}", this.FirstName, this.LastName); }}
        public DateTime CreatedAt { get; set; }
    }
}