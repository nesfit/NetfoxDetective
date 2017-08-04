namespace AlphaChiTech.Virtualization.Interfaces
{
    public interface IPageExpiryComparer
    {
        bool IsUpdateValid(object pageUpdateAt, object updateAt);
    }
}
