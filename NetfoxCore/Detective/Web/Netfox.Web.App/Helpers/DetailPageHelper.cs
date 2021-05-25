using System;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.Facades.Base;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.App.Helpers
{
    public class DetailPageHelper<TDTO, TKey> : ISaveCancelViewModel
        where TDTO : IEntity<TKey>
    {
        [Bind(Direction.None)]
        public IDetailFacade<TDTO, TKey> Facade { get; }

        public DetailPageHelper(IDetailFacade<TDTO, TKey> facade)
        {
            this.Facade = facade;
        }

        public TDTO Data { get; set; }

        public TKey CurrentItemId { get; set; }

        public bool IsNew => Equals(CurrentItemId, default(TKey));

        public TDTO CurrentItem { get; set; }


        public void Init(TKey id)
        {
            if (id != null)
            {
                CurrentItemId = (TKey)Convert.ChangeType(id, typeof(TKey));
            }

        }

        public void PreRender()
        {
            if (!IsNew)
            {
                CurrentItem = Facade.GetDetail(CurrentItemId);
            }
            else
            {
                CurrentItem = Facade.InitializeNew();
            }
            OnItemLoaded();   
        }


        protected virtual void OnItemLoaded()
        {
        }

        protected virtual void OnItemSaving()
        {
        }

        protected virtual void OnItemSaved()
        {
        }


        public void Save()
        {
            OnItemSaving();

            CurrentItem = Facade.Save(CurrentItem);
            CurrentItemId = CurrentItem.Id;

            OnItemSaved();

            
        }

        public void Cancel()
        {
        }
    }
}