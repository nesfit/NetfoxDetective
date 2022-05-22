namespace Netfox.Detective.Interfaces
{
    public interface ISerializationPersistor<TItem>
    {
        TItem Load(string path);
        void Save(TItem item);
    }
}