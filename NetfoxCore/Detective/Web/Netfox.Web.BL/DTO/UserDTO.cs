using System;
using System.ComponentModel.DataAnnotations;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.DTO
{
    public class UserDTO : IEntity<Guid>
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Firstname is required.")]
        public string Firstname { get; set; }
        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "IsEnable is required.")]
        public bool IsEnable { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
    }
}