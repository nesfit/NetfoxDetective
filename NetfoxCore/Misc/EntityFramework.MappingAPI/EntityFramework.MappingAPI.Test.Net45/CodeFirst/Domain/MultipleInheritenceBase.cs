using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public abstract class MultipleInheritenceBase
    {

        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public int Int { get; set; }
    }

    public class MiA:MultipleInheritenceBase
    {
        public Guid? MiRefARefId { get; set; }
        [ForeignKey(nameof(MiRefARefId))]
        public virtual MiRefA MiRefA { get; set; }
    }

    public class MiB : MiA
    {
        public Guid? MiRefBRefId { get; set; }
        [ForeignKey(nameof(MiRefBRefId))]
        public virtual MiRefB MiRefB { get; set; }
    }

    public class MiC : MiB
    {
        public Guid? MiRefCRefId { get; set; }
        [ForeignKey(nameof(MiRefCRefId))]
        public virtual MiRefC MiRefC { get; set; }
    }

    public class MiRefA
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        [InverseProperty(nameof(MiA.MiRefA))]
        public virtual ICollection<MiA> MiAs { get; set; }
    }

    public class MiRefB
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        [InverseProperty(nameof(MiB.MiRefB))]
        public virtual ICollection<MiB> MiBs { get; set; }
    }

    public class MiRefC
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        [InverseProperty(nameof(MiC.MiRefC))]
        public virtual ICollection<MiC> MiCs { get; set; }
    }
}
