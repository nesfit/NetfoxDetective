using System;
using System.Data.Entity.Infrastructure;
using DotVVM.Framework.Controls;
using Riganti.Utils.Infrastructure;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades.Base
{
    public abstract class AppCrudFacadeBase<TEntity, TKey, TListDTO, TDetailDTO>
        : CrudFacadeBase<TEntity, TKey, TListDTO, TDetailDTO>,
            IListFacade<TListDTO, TKey>,
            IDetailFacade<TDetailDTO, TKey>
        where TDetailDTO : IEntity<TKey>
        where TEntity : IEntity<TKey>
    {

        public AppCrudFacadeBase(Func<IQuery<TListDTO>> queryFactory, IRepository<TEntity, TKey> repository, IEntityDTOMapper<TEntity, TDetailDTO> mapper) : base(queryFactory, repository, mapper)
        {
            
        }

        public void FillDataSet(GridViewDataSet<TListDTO> items)
        {
            DotvvmFacadeExtensions.FillDataSet(this, items);
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
