using System.IO;
using Netfox.Web.App.Settings;
using Newtonsoft.Json;

namespace Netfox.Web.BL.Settings
{
    public class JsonWebSettings : INetfoxWebSettings
    {
        public string InvestigationsFolder => _model.InvestigationsFolder;
        public string InvestigationPrefix => _model.InvestigationPrefix;
        public string DefaultInvestigationName => _model.DefaultInvestigationName;
        public string PathToBin => _model.PathToBin;
        public string ConnectionString => _model.ConnectionString;
        private readonly Model _model;

        public JsonWebSettings() : this("settings.json")
        {
            
        }
        
        public JsonWebSettings(string file)
        {
            if (File.Exists(file))
                _model = JsonConvert.DeserializeObject<Model>(File.ReadAllText(file));
            else
                _model = new Model();
        }
        
        private sealed class Model : INetfoxWebSettings
        {
            public string InvestigationsFolder { get; set; } = "/Investigations/";
            public string InvestigationPrefix { get; set; }="NFX_";
            public string DefaultInvestigationName { get; set; } = "NFX";
            public string PathToBin { get; set; } = "/bin/";

            public string ConnectionString { get; set; } =
                "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog = NetfoxDetective; Integrated Security=SSPI;MultipleActiveResultSets=True;";
        }
    }
}