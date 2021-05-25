namespace Netfox.Core.Infrastructure
{
    public interface INetfoxSettings
    {
        bool AutoLoadLastSession { get; set; }

        string LastWorkspaces { get; set; }

        bool DontShowStartupWizard { get; set; }

        bool SkipEmtpyExportResults { get; set; }

        string WorkspaceFileExtension { get; }

        string InvestigationFileExtension { get; }

        string AppDataLogPath { get; set; }

        string ConnectionString { get; set; }

        string DatabaseFileExtension { get; }

        string DefaultWorkspaceStoragePath { get; }

        bool StoreDatabaseWithInvestigation { get; set; }

        string DefaultInMemoryConnectionString { get; }

        string ExplicitNotifications { get; set; }

        string ToLogMessages { get; set; }

        bool DecapsulateGseOverUdp { get; set; }

        void Save();
    }
}