using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Netfox.Web.BL.DTO;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.BL.Queries
{
    public class RoleListQuery : AppQueryBase<Role>
    {
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public RoleListQuery(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider) { }

        protected override IQueryable<Role> GetQueryable()
        {
            return this.Context.Roles.AsQueryable();
        }
    }

}