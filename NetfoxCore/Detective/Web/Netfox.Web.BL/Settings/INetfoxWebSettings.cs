namespace Netfox.Web.App.Settings
{
    public interface INetfoxWebSettings
    {
        public string InvestigationsFolder { get; }
        public string InvestigationPrefix { get; }
        public string DefaultInvestigationName { get; }
        public string PathToBin { get; }
        public string ConnectionString { get; }
    }
}