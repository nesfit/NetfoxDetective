using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.DAL.Entities
{
    public class Investigation : IEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public IEnumerable<string> Jobs { get; set; }

        public string JobsSerialized
        {
            get
            {
                return JsonConvert.SerializeObject(Jobs);
            }
            set
            {
                Jobs = string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(value);
            }
        }
        public DateTime Created { get; set; }
  
        public Guid OwnerID { get; set; }

        public virtual ICollection<UserInvestigation> UserInvestigations { get; set; }
        public virtual ICollection<CaptureStats> Stats { get; set; }
        public virtual ExportStats ExportStats { get; set; }
    }
}