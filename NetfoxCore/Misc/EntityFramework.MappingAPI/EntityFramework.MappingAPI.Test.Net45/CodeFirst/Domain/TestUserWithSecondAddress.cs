using System;
using EntityFramework.MappingAPI.Test.CodeFirst.Domain.ComplexTypes;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class TestUserWithSecondAddress : EntityWithTypedId<Guid>
    {
        public Contact Contact { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return string.Format("{0} {1}", this.FirstName, this.LastName); } }
        // complex type must be the last member
        public Address Address { get; set; }
    }
}