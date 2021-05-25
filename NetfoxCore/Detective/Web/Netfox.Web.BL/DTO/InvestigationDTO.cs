using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.DTO
{
    public class InvestigationDTO : IEntity<Guid>
    { 
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public Guid OwnerID { get; set; }

        public UserDTO Owner { get; set; }

        public bool CanEditRemove { get; set; }

        public DateTime LastAccess { get; set; }
        public List<string> Jobs { get; set; }

        public string JobsSerialized
        {
            get { return JsonConvert.SerializeObject(Jobs); }
            set { Jobs = (string.IsNullOrEmpty(value) || value == "null")? new List<string>() : JsonConvert.DeserializeObject<List<string>>(value); }
        }

        public virtual ICollection<UserInvestigationDTO> UserInvestigations { get; set; }

        public virtual ICollection<CaptureDTO> Stats { get; set; }

        public virtual ExportStatisticsDTO ExportStats { get; set; }



    }
}