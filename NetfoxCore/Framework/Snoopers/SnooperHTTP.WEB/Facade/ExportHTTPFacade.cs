using System;
using System.IO;
using System.Linq;
using AutoMapper;
using DotVVM.Framework.Controls;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Snoopers.SnooperHTTP.Models;
using Netfox.Snoopers.SnooperHTTP.WEB.DTO;
using Netfox.Snoopers.SnooperHTTP.WEB.Queries;
using Netfox.Web.App.Settings;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Providers;

namespace Netfox.Snoopers.SnooperHTTP.WEB.Facade
{
    public class ExportHTTPFacade : NetfoxFacadeBase
    {
        public Func<HTTPMsgQuery> MessageFactory { get; set; }

        public Func<HTTPFileQuery> FileFactory { get; set; }
        private readonly INetfoxWebSettings _settings;

        public ExportHTTPFacade(INetfoxWebSettings settings, NetfoxUnitOfWorkProvider unitOfWorkProvider,
            NetfoxRepositoryProvider repositoryProvider) : base(unitOfWorkProvider, repositoryProvider)
        {
            _settings = settings;
        }

        public void FillMessages(Guid investigationId, GridViewDataSet<SnooperHTTPListDTO> dataset,
            ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.MessageFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
            }
        }

        public void FillFiles(Guid investigationId, GridViewDataSet<SnooperHTTPFileDTO> dataset, ExportFilterDTO filter,
            string appPath, bool onlyImages = false)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.FileFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;
                q.FilterImage = onlyImages;

                q.FillDataSet(dataset);

                foreach (var img in dataset.Items)
                {
                    this.GetFileHash(investigationId, img, appPath);
                    img.Url = "/DownloadFile/" + investigationId + "?filename=s" + "&content=" + img.ContentType +
                              "&path=" + img.Path;
                }
            }
        }

        public void GetFileHash(Guid investigationId, SnooperHTTPFileDTO image, string appPath)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var query = (IQueryable<PmFrameBase>) uow.DbContext.Set<PmFrameBase>();

                var frames = query.Where(i => image.FrameGuids.Contains(i.Id)).OrderBy(e => e.FirstSeen).ToList();

                foreach (var f in frames)
                {
                    f.PmCapture.FileInfo = new FileInfo(Path.Combine(
                        appPath + _settings.InvestigationsFolder +
                        _settings.InvestigationPrefix + investigationId + "/Sources/Captures",
                        f.PmCapture.RelativeFilePath));
                }

                var prepare = image.TimeStamp.Ticks + string.Join(" ", frames) + image.StatusLine;

                image.Path = "Exports/HTTP/" + HTTPMsg.GetMD5Hash(prepare);
            }
        }

        public SnooperHTTPDetailDTO GetMessage(Guid investigationId, Guid messageId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var repository = this.RepositoryProvider.Create<SnooperExportedDataObjectHTTP>(investigationId, uow);
                return Mapper.Map<SnooperHTTPDetailDTO>(repository.GetById(messageId));
            }
        }

        public void InitMessageFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.MessageFactory();
                q.InitFilter(filter);
            }
        }

        public void InitFileFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.FileFactory();
                q.InitFilter(filter);
            }
        }
    }
}