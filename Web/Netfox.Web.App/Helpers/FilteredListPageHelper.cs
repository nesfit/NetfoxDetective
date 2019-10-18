using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.Facades.Base;

namespace Netfox.Web.App.Helpers
{
    public class FilteredListPageHelper<TDTO, TKey, TFilterDTO> : IHelper
        where TDTO : new()
        where TFilterDTO : new()
    {
        [Bind(Direction.None)]
        public IFilteredListFacade<TDTO, TKey, TFilterDTO> Facade { get; }

        public FilteredListPageHelper(IFilteredListFacade<TDTO, TKey, TFilterDTO> facade)
        {
            this.Facade = facade;
        }

        [Bind(Direction.None)]
        public ISortingOptions DefaultSortOptions { get; set; }

        public GridViewDataSet<TDTO> Items { get; set; }

        public TFilterDTO Filter { get; set; } = new TFilterDTO();

        public void Init()
        {
            Items = new GridViewDataSet<TDTO>()
            {
                PagingOptions =
                {
                    PageSize = 10
                },
                SortingOptions = DefaultSortOptions
            };
        }
       public void PreRender()
       {
       }

        public void LoadData()
        {
            OnDataLoading();
            Facade.FillDataSet(Items, Filter);
            OnDataLoaded();
        }

        public void ApplyFilter()
        {
            Items.RequestRefresh();
        }

        public void Delete(TKey id)
        {
            OnItemDeleting(id);
            Facade.Delete(id); OnItemDeleted(id);
        }

        protected virtual void OnDataLoading()
        {
        }

        protected virtual void OnDataLoaded()
        {
        }

        protected virtual void OnItemDeleting(TKey id)
        {
        }

        protected virtual void OnItemDeleted(TKey id)
        {
        }
    }
}