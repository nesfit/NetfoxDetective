using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Queries;
using Netfox.Web.DAL.Entities;
using Newtonsoft.Json;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades
{
    public class StatsFacade : FacadeBase
    {
        public Func<StatsQuery> StatsFactory { get; set; }
        public Func<CapturesQuery> CapturesFactory { get; set; }

        public IRepository<CaptureStats, Guid> Repository { get; set; }
        public IRepository<Investigation, Guid> InvestigationRepository { get; set; }
        public IRepository<ExportStats, Guid> ExportStatsRepository { get; set; }

        public StatsFacade(IRepository<CaptureStats, Guid> repository, IRepository<Investigation, Guid> repositoryInvestigation, IRepository<ExportStats, Guid> exportStatsRepository)
        {
            this.Repository = repository;
            this.InvestigationRepository = repositoryInvestigation;
            this.ExportStatsRepository = exportStatsRepository;
        }

        public ConvesationStatisticsDTO GetStats(Guid captureId, Guid investigationId)
        {
            using(var uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.StatsFactory();
                q.CaptureId = captureId;
                q.InvestigationId = investigationId;
                return q.Execute().SingleOrDefault();
            }
        }

        public IEnumerable<CaptureDTO> GetCaptureList(Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.CapturesFactory();
                q.InvestigationId = investigationId;
                return q.Execute(); 
            }
        }

        public void FillInvestigationStats(InvestigationStatisticsDTO stats, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var q = this.StatsFactory();
                q.InvestigationId = investigationId;
                q.GetInvestigationStats(stats);
            }
        }

        public void Insert(ConvesationStatisticsDTO stats, Guid captureId, string name, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var captureStats = new CaptureStats();
                Mapper.Map(stats, captureStats);
                captureStats.Id = captureId;
                captureStats.Name = name;
                captureStats.Investigation = this.InvestigationRepository.GetById(investigationId);

                Repository.Insert(captureStats);
                uow.Commit();
            }
            
        }
        public void UpdateExportedProtocols(Guid captureId, IList<string> exportedProtocols)
        {

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var stats = this.Repository.GetById(captureId);
                var updateProto = this.GetExportedProtocols(captureId);

                updateProto = updateProto.Union(exportedProtocols).ToList();
                stats.ExportedProtocols = JsonConvert.SerializeObject(updateProto);
                this.Repository.Update(stats);
                uow.Commit();
            }
        }

        public IList<string> GetExportedProtocols(Guid captureId)
        {

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var stats = this.Repository.GetById(captureId);
                var result = new List<string>();

                if (!string.IsNullOrEmpty(stats?.ExportedProtocols))
                {
                    result = JsonConvert.DeserializeObject<List<string>>(stats.ExportedProtocols);

                }

                return result;
            }
        }

        public void RemoveExportedProtocol(Guid captureId, string protocol)
        {
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var stats = this.Repository.GetById(captureId);
                var updateProto = GetExportedProtocols(captureId);
                updateProto.Remove(protocol);
                stats.ExportedProtocols = JsonConvert.SerializeObject(updateProto);
                this.Repository.Update(stats);
                uow.Commit();
            }
        }

        public void UpdateExportStats(Guid investigationId, ExportStatisticsDTO stats)
        {

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var investigation = this.InvestigationRepository.GetById(investigationId);
                Mapper.Map<ExportStatisticsDTO, ExportStats>(stats, investigation.ExportStats);
                this.InvestigationRepository.Update(investigation);
                uow.Commit();
            }
        }
    }
}
