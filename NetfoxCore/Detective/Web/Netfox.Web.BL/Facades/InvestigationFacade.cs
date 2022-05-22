using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using AutoMapper;
using DotVVM.Framework.Controls;
using Netfox.Core.Models;
using Netfox.Web.App.Settings;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Queries;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades
{

    public class InvestigationFacade : AppFilteredCrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO, InvestigationFilterDTO>
    {
        protected InvestigationInfo InvestigationInfo { get; }

        protected IRepository<UserInvestigation, Guid> UserInvestigationRepository { get; set; }

        protected UserFacade UserFacade{ get; set; }

        protected StatsFacade StatsFacade { get; set; }

        public Func<LastInvestigationListQuery> LastInvestidationListQueryFactory { get; set; }

        private readonly INetfoxWebSettings _settings;
        
        public InvestigationFacade(INetfoxWebSettings settings, Func<InvestidationListQuery> queryFactory, IRepository<Investigation, Guid> repository, IEntityDTOMapper<Investigation, InvestigationDTO> dtoMapper, IRepository<UserInvestigation, Guid> userInvestigationRepository, UserFacade userFacade, InvestigationInfo container, StatsFacade statsFacade) : base(queryFactory, repository, dtoMapper)
        {
            _settings = settings;
            this.InvestigationInfo = container;
            this.UserInvestigationRepository = userInvestigationRepository;
            this.UserFacade = userFacade;
            this.StatsFacade = statsFacade;
        }

        public InvestigationDTO AddInvestigation(InvestigationDTO detail, List<Guid> investigatorIDs, string appPath)
        {
            CrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO> crudFacadeBase = this;
            using (IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var entity = crudFacadeBase.Repository.InitializeNew();
                base.PopulateDetailToEntity(detail, entity);
                entity.Created = DateTime.Now;

                entity.ExportStats = StatsFacade.ExportStatsRepository.InitializeNew();
                if (!investigatorIDs.Contains(entity.OwnerID)) { investigatorIDs.Add(entity.OwnerID); }

                var investigators = this.UserFacade.Repository.GetByIds(investigatorIDs);

                foreach (var investigator in investigators)
                {
                    this.UserInvestigationRepository.Insert(new UserInvestigation() { Investigation = entity, User = investigator, LastAccess = (DateTime)SqlDateTime.MinValue });
                }

                var result = base.Save(entity, true, detail, uow);
                CreateInvestigationFolder(result.Id, appPath);
                return result;
            }
        }

        public void RemoveInvestigation(Guid id, string appPath)
        {
            this.RemoveInvestigationFolder(_settings.InvestigationPrefix + id, appPath);
            

            CrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO> crudFacadeBase = this;
            using (IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var entity = crudFacadeBase.Repository.GetById(id).ExportStats;
                uow.Commit();
                this.Repository.Delete(id);
                this.StatsFacade.ExportStatsRepository.Delete(entity.Id);
                uow.Commit();
            }
        }

        public InvestigationDTO Save(InvestigationDTO detail, List<Guid> investigatorIDs)
        {
            CrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO> crudFacadeBase = this;
            using (IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var entity = crudFacadeBase.Repository.GetById(detail.Id);

                // Add owner - last access
                if (!investigatorIDs.Contains(detail.OwnerID)) { investigatorIDs.Add(detail.OwnerID); }
                // Add old owner - last access
                if (!investigatorIDs.Contains(entity.OwnerID)) { investigatorIDs.Add(entity.OwnerID); }

                base.PopulateDetailToEntity(detail, entity);
                
                var investigatorsList = new List<UserInvestigation>();
                foreach (var investigatorId in investigatorIDs)
                {
                    var item = entity.UserInvestigations.SingleOrDefault(i => i.UserId == investigatorId);

                    if(item != null) item.User = this.UserFacade.Repository.GetById(investigatorId);
                    investigatorsList.Add(item ?? new UserInvestigation() { Investigation = entity, User = this.UserFacade.Repository.GetById(investigatorId), LastAccess = (DateTime)SqlDateTime.MinValue });
                }
                entity.UserInvestigations = investigatorsList;
                return base.Save(entity, false, detail, uow);
            }
           
        }

        public void FillDataSet(GridViewDataSet<InvestigationDTO> items, InvestigationFilterDTO filter, UserDTO user)
        {
            using (this.UnitOfWorkProvider.Create())
            {
                var query = (InvestidationListQuery)QueryFactory();
                query.Filter = filter;
                query.User = user;
                DotvvmFacadeExtensions.LoadFromQuery(items, query);

                foreach(var item in items.Items)
                {
                    item.CanEditRemove = (user.Role.Name == "Administrator" || item.OwnerID == user.Id);
                    item.Owner = AutoMapper.Mapper.Map< User, UserDTO >(this.UserFacade.Repository.GetById(item.OwnerID));
                }
            }
        }
        public void FillLastInvestigationDataSet(GridViewDataSet<InvestigationDTO> items, UserDTO user)
        {
            using (this.UnitOfWorkProvider.Create())
            {
                var query = (LastInvestigationListQuery) LastInvestidationListQueryFactory();
               
                query.User = user;
                DotvvmFacadeExtensions.LoadFromQuery(items, query);
            }
        }
        public void UpdateLastAccess(Guid investigationId, string username)
        {
            CrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO> crudFacadeBase = this;
            using(IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var investigation = crudFacadeBase.Repository.GetById(investigationId);
                var user = this.UserFacade.GetUser(username);
                var investigator = investigation.UserInvestigations?.SingleOrDefault(u => u.UserId == user.Id);

                if (user.Role.Name == "Administrator" && investigator == null)
                {
                    investigator = this.UserInvestigationRepository.InitializeNew();
                    investigator.Investigation = investigation;
                    investigator.User = this.UserFacade.Repository.GetById(user.Id);
                    investigator.LastAccess = DateTime.Now.ToLocalTime();
                    this.UserInvestigationRepository.Insert(investigator);
                }
                else
                {
                    investigator = this.UserInvestigationRepository.GetById(investigator.Id);
                    investigator.LastAccess = DateTime.UtcNow.ToLocalTime();
                    this.UserInvestigationRepository.Update(investigator);
                }
                uow.Commit();
            }
        }

        public UserDTO GetOwner(Guid investigationId)
        {
            CrudFacadeBase<Investigation, Guid, InvestigationDTO, InvestigationDTO> crudFacadeBase = this;
            using (IUnitOfWork uow = crudFacadeBase.UnitOfWorkProvider.Create())
            {
                var investigation = crudFacadeBase.Repository.GetById(investigationId);
                var user = this.UserFacade.Repository.GetById(investigation.OwnerID);
                return AutoMapper.Mapper.Map<UserDTO>(user);
            }
        }

        public void CreateInvestigationFolder(Guid id, string appPath)
        {
            var investigationsPath = appPath + _settings.InvestigationsFolder;

            if (!Directory.Exists(investigationsPath))
            {
                Directory.CreateDirectory(investigationsPath);
            }

            
            this.InvestigationInfo.InvestigationsDirectoryInfo = new DirectoryInfo(investigationsPath);
            this.InvestigationInfo.Guid = id;
            this.InvestigationInfo.InvestigationName = _settings.DefaultInvestigationName;
            this.InvestigationInfo.CreateFileStructure();
        }

        public void RemoveInvestigationFolder(string investigationFolder, string appPath)
        {
            var investigationsPath = appPath + _settings.InvestigationsFolder;
            var investigationFolderPath = investigationsPath + investigationFolder;

            if (Directory.Exists(investigationsPath) && Directory.Exists(investigationFolderPath))
            {
                Directory.Delete(investigationFolderPath, true);
            }
        }
    }
}