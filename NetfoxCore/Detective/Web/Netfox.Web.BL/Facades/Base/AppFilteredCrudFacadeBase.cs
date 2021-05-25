using System;
using System.Data.Entity.Infrastructure;
using DotVVM.Framework.Controls;
using Riganti.Utils.Infrastructure;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades.Base
{
    public abstract class AppFilteredCrudFacadeBase<TEntity, TKey, TListDTO, TDetailDTO, TFilterDTO> : FilteredCrudFacadeBase<TEntity, TKey, TListDTO, TDetailDTO, TFilterDTO>, IFilteredListFacade<TListDTO, TKey, TFilterDTO>, IDetailFacade<TDetailDTO, TKey>
        where TDetailDTO : IEntity<TKey>
        where TEntity : IEntity<TKey>
    {

        public AppFilteredCrudFacadeBase(Func<IFilteredQuery<TListDTO, TFilterDTO>> queryFactory, IRepository<TEntity, TKey> repository, IEntityDTOMapper<TEntity, TDetailDTO> mapper) : base(queryFactory, repository, mapper)
        {
        }

        public void FillDataSet(GridViewDataSet<TListDTO> items, TFilterDTO filter)
        {
            DotvvmFacadeExtensions.FillDataSet(this, items, filter);
        }

        public override void Delete(TKey id)
        {
            try
            {
                base.Delete(id);
            }
            catch (DbUpdateException)
            {
                throw new UIException("The record could not be deleted!");
            }
        }
    }
}
