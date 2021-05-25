namespace Netfox.Web.BL.Facades.Base
{
    public interface IDetailFacade<TDTO, TKey>
    {
        TDTO GetDetail(TKey id);
        TDTO InitializeNew();
        TDTO Save(TDTO item);
    }
}
