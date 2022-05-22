using System;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public class MeteringPoint : Entity
    {
        public string EIC { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}