using System;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public abstract class FlatInheritenceBase
    {

        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public int Int { get; set; }
    }


    public class FiA : FlatInheritenceBase
    {
        public virtual string StringA { get; set; }
    }

    public class FiB : FlatInheritenceBase
    {
        public virtual string StringB { get; set; }
    }
}