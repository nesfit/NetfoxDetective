using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using AutoMapper;
using DotVVM.Framework.Controls;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.Models;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;

namespace Netfox.Web.BL.Facades
{
    public class ExportFacade : NetfoxFacadeBase
    {
        private ISnooperFactory SnooperFactory { get; set; }

        public Func<ChatMessageQuery> ChatMessagesFactory { get; set; }
        public Func<CallQuery> CallsFactory { get; set; }
        public Func<EmailQuery> EmailsFactory { get; set; }
        public Func<CallStreamQuery> CallStreamsFactory { get; set; }


        public ExportFacade(NetfoxUnitOfWorkProvider unitOfWorkProvider, NetfoxRepositoryProvider repositoryProvider, ISnooperFactory snooperFactory) : base(unitOfWorkProvider, repositoryProvider)
        {
            this.SnooperFactory = snooperFactory;
        }

        public List<KeyValueDTO<string, string>> GetSnooperList()
        {
            var result = new List<KeyValueDTO<string, string>>();
            var snoopers = this.SnooperFactory.AvailableSnoopers;
            foreach (var snooper in snoopers)
            {
                result.Add(new KeyValueDTO<string, string>(snooper.Name, snooper.GetType().FullName)); ;
            }
            return result;
        }

        public void FillDataSet<T>(GridViewDataSet<T> dataset, IQueryable<T> query, Guid investigationId)
        {
            using(var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                dataset.PagingOptions.TotalItemsCount = query.Count();
                dataset.Items = query.Skip(dataset.PagingOptions.PageIndex * dataset.PagingOptions.PageSize).Take(dataset.PagingOptions.PageSize).ToList();
            }
        }

        public void FillChatMessageDataSet(GridViewDataSet<ExportChatMessageDTO> dataset, Guid investigationId, ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = ChatMessagesFactory();
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
            }
        }

        public void FillEmailDataSet(GridViewDataSet<ExportEmailDTO> dataset, Guid investigationId, ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.EmailsFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
                CultureInfo MyCultureInfo = new CultureInfo("en-US");
                foreach (var i in dataset.Items)
                {
                    if(i.Date != null)
                    {
                        var d = Regex.Replace(i.Date, @"\.\d+$", string.Empty);
                        i.Timestamp = DateTime.ParseExact(d, "yyyy-MM-dd HH.mm.ss", MyCultureInfo);
                    }
                }
            }
        }

        public void InitChatMessageFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.ChatMessagesFactory();
                q.InitChatMessageFilter(filter);
            }

        }

        public void InitCallFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.CallsFactory();
                q.InitCallFilter(filter);
            }

        }
        public void InitCallStreamFilter(ExportFilterDTO filter, Guid investigationId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.CallStreamsFactory();
                q.InitCallStreamFilter(filter);
            }

        }

        public void InitEmailFilter(ExportFilterDTO filter)
        {
            /*using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = this.EmailsFactory();
                q.InitEmailFilter(filter);
            }*/

        }

        public void FillCallDataSet(GridViewDataSet<ExportCallDTO> dataset, Guid investigationId, ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = CallsFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
                foreach (var i in dataset.Items)
                {
                    i.DurationText = i.Duration.ToString("g");
                }
            }
        }

        public void FillCallStreamDataSet(GridViewDataSet<ExportCallStreamDTO> dataset, Guid investigationId, ExportFilterDTO filter)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = CallStreamsFactory();
                q.Filters = filter;
                q.SortExpression = dataset.SortingOptions.SortExpression;
                q.SortDescending = dataset.SortingOptions.SortDescending;

                q.FillDataSet(dataset);
                foreach(var i in dataset.Items)
                {
                    var tmp = Regex.Split(i.WavFilePath, "wwwroot");
                    i.WavFilePath = tmp[1];
                    i.DurationText = i.Duration.ToString("g");
                }
                
            }
        }
        public void FillCallStreamOfCall(GridViewDataSet<ExportCallStreamDTO> dataset, Guid investigationId, IEnumerable<string> rtpAddress)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = CallStreamsFactory();
                q.FillCallStreamOfCall(dataset, rtpAddress);
                foreach (var i in dataset.Items)
                {
                    var tmp = Regex.Split(i.WavFilePath, "wwwroot");
                    i.WavFilePath = tmp[1];
                    i.DurationText = i.Duration.ToString("g");
                }
            }
        }

        public ExportCallDetailDTO GetCall(Guid investigationId, Guid callId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = CallsFactory();
                var call = q.GetCall(callId);
                call.DurationText = call.Duration.ToString("g");
                return call;
            }
        }

        public ExportEmailDTO GetEmail(Guid investigationId, Guid emailId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = EmailsFactory();
                var email = q.GetEmail(emailId);
                var tmp = Regex.Split(email.StoredContentFilePath, "wwwroot");
                email.StoredContentFilePath = tmp[1];
                tmp = Regex.Split(email.StoredHeadersFilePath, "wwwroot");
                email.StoredHeadersFilePath = tmp[1];
                return email;
            }
        }

        public void FillAttachmentsOfEmail(GridViewDataSet<EmailAttachmentDTO> dataset, Guid investigationId, Guid emailId)
        {
            using (var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = EmailsFactory();
                q.FillAttachmentsOfEmail(dataset, emailId);
                foreach (var i in dataset.Items)
                {
                    if(i.StoredContentFilePath != null)
                    {
                        var tmp = Regex.Split(i.StoredContentFilePath, "Exports");
                        i.StoredContentFilePath = "/DownloadFile/" + investigationId + "?filename=file" + "&content=" + i.ContentType + "/" + i.ContentSubtype + "&path=Exports/" + tmp[1];
                    }
                }
            }
        }

        public void GetAddressOfEmail(ExportEmailDTO export, Guid emailId, Guid investigationId)
        {
            using(var uow = this.UnitOfWorkProvider.Create(investigationId))
            {
                var q = EmailsFactory();
                q.GetAddressOfEmail(export, emailId);
            }
        }
    }
}
