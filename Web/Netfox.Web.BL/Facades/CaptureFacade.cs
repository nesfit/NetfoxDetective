using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DotVVM.Framework.Controls;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Models;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
//using Netfox.Web.DAL.Entities.Framework;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;
using Netfox.Web.DAL.Properties;
using Riganti.Utils.Infrastructure.Core;
using UnitOfWork;
using UnitOfWork.BaseDataEntity;
using UnitOfWork.EF6Repository;

namespace Netfox.Web.BL.Facades
{
    public class CaptureFacade : NetfoxFacadeBase
    {
        public Func<L3ConversationQuery> L3ConvenversationsFactory { get; set; }

        public Func<L4ConversationQuery> L4ConvenversationsFactory { get; set; }

        public Func<L7ConversationQuery> L7ConvenversationsFactory { get; set; }

        public Func<FrameQuery> FramesFactory { get; set; }

        public StatsFacade Stats { get; set; }

        public InvestigationInfo InvestigationInfo { get; set; }

        public CaptureFacade(NetfoxUnitOfWorkProvider uowProvider, NetfoxRepositoryProvider repositoryProvider, StatsFacade stats, InvestigationInfo investigationInfo) : base(uowProvider, repositoryProvider)
        {
            this.Stats = stats;
            this.InvestigationInfo = investigationInfo;
        }

        public IEnumerable<CaptureDTO> GetCaptureList(Guid investigationId)
        {
            return Stats.GetCaptureList(investigationId);
        }

        public ConvesationStatisticsDTO GetConversationStatistics(Guid captureId, Guid investigationId)
        {
            return Stats.GetStats(captureId, investigationId);
        }


        public void FillL3ConversationDataSet(GridViewDataSet<L3ConversationDTO> dataSet, Guid captureId, Guid investigationId, ConversationFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.L3ConvenversationsFactory();
                q.Filters = filter;
                q.SortExpression = dataSet.SortingOptions.SortExpression;
                q.SortDescending = dataSet.SortingOptions.SortDescending;
                q.CaptureId = captureId;

                q.FillDataSet(dataSet);
            }
        }

        public void FillL4ConversationDataSet(GridViewDataSet<L4ConversationDTO> dataSet, Guid filterId, Guid investigationId, ConversationFilterDTO filter, ConversationType filterType = ConversationType.None)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.L4ConvenversationsFactory();
                q.Filters = filter;
                q.FilterType = filterType;
                q.SortExpression = dataSet.SortingOptions.SortExpression;
                q.SortDescending = dataSet.SortingOptions.SortDescending;
                q.FilterId = filterId;

                q.FillDataSet(dataSet);
            }
        }

        public void FillL7ConversationDataSet(GridViewDataSet<L7ConversationDTO> dataSet, Guid filterId, Guid investigationId, ConversationFilterDTO filter, ConversationType filterType = ConversationType.None)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.L7ConvenversationsFactory();
                q.Filters = filter;
                q.FilterType = filterType;
                q.SortExpression = dataSet.SortingOptions.SortExpression;
                q.SortDescending = dataSet.SortingOptions.SortDescending;
                q.FilterId = filterId;

                q.FillDataSet(dataSet);
                
            }
        }

        public void FillPmFrameDataSet(GridViewDataSet<PmFrameBaseDTO> dataSet, Guid filterId, Guid investigationId, FrameFilterDTO filter, ConversationType filterType = ConversationType.None)
        {
            using(var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.FramesFactory();
                q.Filters = filter;
                q.FilterType = filterType;
                q.SortExpression = dataSet.SortingOptions.SortExpression;
                q.SortDescending = dataSet.SortingOptions.SortDescending;
                q.FilterId = filterId;

                q.FillDataSet(dataSet);
            }
        }

        public PmFrameBaseDetailDTO GetFrame(Guid investigationId, Guid frameId, string appPath)
        {
            using(var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<PmFrameBase>(investigationId, uow);
                var frame = repository.GetById(frameId);
                SetupInvestigationInfo(appPath, investigationId);
                //frame.CaptureFolderInfo = this.InvestigationInfo.SourceCaptureDirectoryInfo;
                return Mapper.Map <PmFrameBaseDetailDTO> (frame);
            }
        }

        public L3ConversationDTO GetL3Conversation(Guid investigationId, Guid conversationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<L3Conversation>(investigationId, uow);
                return Mapper.Map<L3ConversationDTO>(repository.GetById(conversationId));
            }
        }

        public L4ConversationDetailDTO GetL4Conversation(Guid investigationId, Guid conversationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<L4Conversation>(investigationId, uow);
                var conversation = repository.GetById(conversationId);
                return Mapper.Map<L4ConversationDetailDTO>(conversation);
            }
        }

        public L7ConversationDetailDTO GetL7Conversation(Guid investigationId, Guid conversationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<L7Conversation>(investigationId, uow);
                return Mapper.Map<L7ConversationDetailDTO>(repository.GetById(conversationId));
            }
        }

        public void AddCypherKey(Guid investigationId, Guid captureId, string privateKey)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<L7Conversation>(investigationId, uow);
                var l7ConversationIds = uow.DbContext.Set<L7Conversation>()
                    .Where(c => c.Captures.Any(cc => cc.Id == captureId)).Select(c => c.Id ).ToList();
                foreach(var l7 in l7ConversationIds )
                {
                    var l7Conv = repository.GetById(l7);
                    l7Conv.Key.ServerPrivateKey = privateKey;
                    repository.Update(l7Conv);
                    uow.SaveChanges();
                }
                uow.Commit();
            }
        }

        private InvestigationInfo SetupInvestigationInfo(string appPath, Guid investigationId)
        {
            this.InvestigationInfo.Guid = investigationId;
            this.InvestigationInfo.InvestigationName = NetfoxWebSettings.Default.DefaultInvestigationName;
            this.InvestigationInfo.InvestigationsDirectoryInfo = new DirectoryInfo(appPath + NetfoxWebSettings.Default.InvestigationsFolder);
            return this.InvestigationInfo;
        }
    }

    public enum ConversationType
    { 
        None,
        L3,
        L4,
        L7
    }
}


