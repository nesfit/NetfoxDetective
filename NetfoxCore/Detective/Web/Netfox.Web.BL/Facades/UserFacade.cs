using System;
using System.Collections.Generic;
using System.Linq;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Queries;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades
{
    public class  UserFacade : AppFilteredCrudFacadeBase<User, Guid, UserDTO, UserDTO, UserFilterDTO>
    {
        public Func<RoleListQuery> RolesListFactory { get; set; }

        public Func<UserQuery> UserFactory { get; set; }

        public LoginFacade LoginFacade { get; set; }

        public UserFacade(Func<UserListQuery> queryFactory, IRepository<User, Guid> repository, IEntityDTOMapper<User, UserDTO> mapper, LoginFacade loginFacade) : base(queryFactory, repository, mapper)
        {
            this.LoginFacade = loginFacade;
        }

        public Func<VerifyCredetialQuery> VerifyCredentialFactory { get; set; }

        public UserDTO Save(UserDTO detail, string passwd)
        {
            CrudFacadeBase<User, Guid, UserDTO, UserDTO> crudFacadeBase = this;
            using (IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var entity = crudFacadeBase.Repository.InitializeNew();
                base.PopulateDetailToEntity(detail, entity);
                entity.Password = LoginFacade.SHA256Hash(passwd);
                return base.Save(entity, true ,detail, uow);
            }
        }

        public IEnumerable<UserDTO> GetUserList()
        {
            using (IUnitOfWork uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.QueryFactory();
                return q.Execute();
            }
        }

        public UserDTO GetUser(string username)
        {
            using (IUnitOfWork uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.UserFactory();
                q.Username = username;
                return q.Execute().SingleOrDefault();
            }
        }

        public List<Role> GetRoles()
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.RolesListFactory();
                return q.Execute().ToList();
            }
        }

        public bool ChangePassword(Guid userId, string oldPasswd, string newPasswd)
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var user = this.Repository.GetById(userId);

                if (user.Password != oldPasswd) { return false; }

                user.Password = newPasswd;
                this.Repository.Update(user);
                uow.Commit();
                return true;
            }
        }
    }
}
