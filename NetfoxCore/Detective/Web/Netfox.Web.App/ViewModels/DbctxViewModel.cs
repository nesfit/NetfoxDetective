using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper.QueryableExtensions;
using Castle.Windsor;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Persistence;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Providers;
using Newtonsoft.Json;
using DataPager = DotVVM.BusinessPack.Controls.DataPager;
using SnooperExportedObjectBase = Netfox.Framework.Models.Snoopers.SnooperExportedObjectBase;

namespace Netfox.Web.App.ViewModels
{
    public class DbctxViewModel : BaseLayoutViewModel
    {
        private IWindsorContainer container { get; set; }

        public GridViewDataSet<PmFrameBaseDTO> ss { get; set; } = new GridViewDataSet<PmFrameBaseDTO>();

        protected StatsFacade stats { get; set; }

        protected CaptureFacade capture { get; set; }
        protected NetfoxUnitOfWorkProvider providerOld { get; set; }

        public DbctxViewModel(IWindsorContainer container, CaptureFacade facade, StatsFacade stats, NetfoxUnitOfWorkProvider o)
        {

            this.container = container;
            this.stats = stats;
            this.capture = facade;
            this.providerOld = o;


            this.ss.PagingOptions.PageSize = 15;
            this.ss.SortingOptions.SortDescending = false;
            this.ss.SortingOptions.SortExpression = nameof(L3ConversationDTO.FirstSeen);

            //sta

        }

        public void ctx1()
        {
            using (var uow = this.providerOld.Create(Guid.Parse("05ee467b-dc47-e911-9eb4-54271ebdb7b1")))
            {
                /*var guid = Guid.Parse("f45ecdb9-b337-4342-8266-784c3e8dff95");
                var query = (IQueryable<PmFrameBase>)uow.DbContext.Set<PmFrameBase>();
                query = query.Where(c => c.PmCaptureRefId == guid);
                var aa = query.ToList();*/

                var guid = Guid.Parse("f45ecdb9-b337-4342-8266-784c3e8dff95");
                var query = (IQueryable<L7Conversation>)uow.DbContext.Set<L7Conversation>();
                query = query.Where(c => c.Captures.Any(cc => cc.Id == guid));
                var aa = query.ToList();

            }
        }


        public void ctx2()
        {


        }

    }
}

