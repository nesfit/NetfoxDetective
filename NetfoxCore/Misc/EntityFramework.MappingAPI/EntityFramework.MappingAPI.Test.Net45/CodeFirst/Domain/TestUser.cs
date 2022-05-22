﻿using System;
using EntityFramework.MappingAPI.Test.CodeFirst.Domain.ComplexTypes;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class TestUser : EntityWithTypedId<Guid>
    {
        public Contact Contact { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); }}
    }
}