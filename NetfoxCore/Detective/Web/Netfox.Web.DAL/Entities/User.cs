using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.DAL.Entities
{
    public class User : IEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public bool IsEnable { get; set; }
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }
        
        public virtual  Role Role { get; set; }
        public virtual ICollection<UserInvestigation> UserInvestigations { get; set; }
    }
}
