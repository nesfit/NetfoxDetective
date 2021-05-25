using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.MappingAPI.Test.CodeFirst.Domain.ComplexTypes;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class House
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // complex type must be the last member
        public Address Address { get; set; }
    }
}
