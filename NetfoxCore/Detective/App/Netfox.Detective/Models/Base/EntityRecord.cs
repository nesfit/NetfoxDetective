namespace Netfox.Detective.Models.Base
{
    public class EntityRecord
    {
        public object Data;
        public string Plugin;

        public EntityRecord()
        {
        }

        public EntityRecord(string plugin, object data)
        {
            this.Plugin = plugin;
            this.Data = data;
        }
    }
}