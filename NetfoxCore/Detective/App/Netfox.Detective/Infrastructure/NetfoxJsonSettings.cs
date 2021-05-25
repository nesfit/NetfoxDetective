using Netfox.Core.Infrastructure;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Netfox.Detective.Infrastructure
{
	public class NetfoxJsonSettings : IAppSettings
	{
		public bool AutoLoadLastSession { get => _model.AutoLoadLastSession; set => _model.AutoLoadLastSession = value; }
		public string LastWorkspaces { get => _model.LastWorkspaces; set => _model.LastWorkspaces = value; }
		public bool DontShowStartupWizard { get => _model.DontShowStartupWizard; set => _model.DontShowStartupWizard = value; }
		public bool SkipEmtpyExportResults { get => _model.SkipEmtpyExportResults; set => _model.SkipEmtpyExportResults = value; }

		public string WorkspaceFileExtension { get => _model.WorkspaceFileExtension; }

		public string InvestigationFileExtension { get => _model.InvestigationFileExtension; }

		public string AppDataLogPath { get => _model.AppDataLogPath; set => _model.AppDataLogPath = value; }
		public string ConnectionString { get => _model.ConnectionString; set => _model.ConnectionString = value; }

		public string DatabaseFileExtension { get => _model.DatabaseFileExtension; }

		public string DefaultWorkspaceStoragePath { get => _model.DefaultWorkspaceStoragePath; }

		public bool StoreDatabaseWithInvestigation { get => _model.StoreDatabaseWithInvestigation; set => _model.StoreDatabaseWithInvestigation = value; }

		public string DefaultInMemoryConnectionString { get => _model.DefaultInMemoryConnectionString; }

		public string ExplicitNotifications { get => _model.ExplicitNotifications; set => _model.ExplicitNotifications = value; }
		public string ToLogMessages { get => _model.ToLogMessages; set => _model.ToLogMessages = value; }

		public bool DecapsulateGseOverUdp { get => _model.DecapsulateGseOverUdp; set => _model.DecapsulateGseOverUdp = value; }

		private readonly Model _model;
		private readonly string _file;

		public NetfoxJsonSettings() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FIT_VUTBR\\Netfox\\Detective", "settings.json"))
		{

		}

		public NetfoxJsonSettings(string file)
		{
			_file = file;
			if (File.Exists(file))
				_model = JsonConvert.DeserializeObject<Model>(File.ReadAllText(file));
			else
				_model = new Model();
		}

		public void Save()
		{
			string dir = Path.GetDirectoryName(_file);
			if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			File.WriteAllText(_file, JsonConvert.SerializeObject(_model));
		}

		private sealed class Model
		{
			public bool AutoLoadLastSession { get; set; } = false;
			public string LastWorkspaces { get; set; } = "";
			public bool DontShowStartupWizard { get; set; } = false;
			public bool SkipEmtpyExportResults { get; set; } = true;

			public string WorkspaceFileExtension { get; set; } = "nfw";

			public string InvestigationFileExtension { get; set; } =DefaultNetfoxSettings.InvestigationFileExtension;

			public string AppDataLogPath { get; set; } = "FIT_VUTBR\\Netfox\\Detective\\Logs";
			public string ConnectionString { get; set; } = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog = NetfoxDetective; Integrated Security=SSPI;MultipleActiveResultSets=True;";

			public string DatabaseFileExtension { get; set; } = "mdf";

			public string DefaultWorkspaceStoragePath { get; set; } = "%userprofile%\\Netfox Detective Workspaces";

			public bool StoreDatabaseWithInvestigation { get; set; } = true;

			public string DefaultInMemoryConnectionString { get; set; } = "Data Source=:memory:;";

			public string ExplicitNotifications { get; set; } = "Info";
			public string ToLogMessages { get; set; } = "Info";

			public bool DecapsulateGseOverUdp { get; set; } = false;
		}
	}
}