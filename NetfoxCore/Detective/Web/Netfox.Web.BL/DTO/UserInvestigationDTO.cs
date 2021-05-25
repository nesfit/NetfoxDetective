using System;

namespace Netfox.Web.BL.DTO
{
    public class UserInvestigationDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserDTO User { get; set; }
        public Guid InvestigationId { get; set; }
        public DateTime LastAccess { get; set; }
    }
}