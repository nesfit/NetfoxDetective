using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using Castle.Core.Logging;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces.Views;
using Netfox.Logger;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    [NotifyPropertyChanged]
    public class LoggingSettingsTabVm : SettingsBaseVm
    {
        private string _loggingDirectory;
        private readonly INetfoxSettings _settings;

        public LoggingSettingsTabVm(WindsorContainer applicationWindsorContainer, NetfoxLogger netfoxLogger,
            INetfoxSettings settings) : base(
            applicationWindsorContainer)
        {
            _settings = settings;
            _loggingDirectory = settings.AppDataLogPath;
            this.NetfoxLogger = netfoxLogger;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<ILoggingSettingsTab>());
        }

        public override string HeaderText => "Logging";
        public NetfoxLogger NetfoxLogger { get; }

        [SafeForDependencyAnalysis]
        public string LoggingDirectory
        {
            get { return this._loggingDirectory; }
            set
            {
                this._settings.AppDataLogPath = value;
                this._settings.Save();
                this._loggingDirectory = value;
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public LoggerLevel BackgroundLoggingLevels
        {
            get { return this.NetfoxLogger.BackgroundLoggerLevel; }
            set { this.NetfoxLogger.BackgroundLoggerLevel = value; }
        }

        public bool LevelOff
        {
            get => BackgroundLoggingLevels == LoggerLevel.Off;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Off;
            }
        }
        
        public bool LevelDebug
        {
            get => BackgroundLoggingLevels == LoggerLevel.Debug;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Debug;
            }
        }
        
        public bool LevelInfo
        {
            get => BackgroundLoggingLevels == LoggerLevel.Info;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Info;
            }
        }
        
        public bool LevelWarn
        {
            get => BackgroundLoggingLevels == LoggerLevel.Warn;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Warn;
            }
        }
        
        public bool LevelError
        {
            get => BackgroundLoggingLevels == LoggerLevel.Error;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Error;
            }
        }
        
        public bool LevelFatal
        {
            get => BackgroundLoggingLevels == LoggerLevel.Fatal;
            set
            {
                if (value) BackgroundLoggingLevels = LoggerLevel.Fatal;
            }
        }

        [SafeForDependencyAnalysis] public ICommand ChangeCommand => new RelayCommand(this.SettingsChanged);

        [SafeForDependencyAnalysis] public ICommand OpenDirectoryCommand => new RelayCommand(this.OpenDirectory);

        public void OpenDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this._loggingDirectory = dialog.SelectedPath;
                this.LoggingDirectory = this._loggingDirectory;
            }
        }

        private void SettingsChanged()
        {
            _settings.AppDataLogPath = this.LoggingDirectory;
            _settings.Save();
            this.NetfoxLogger.ChangeLoggingDirectory(new DirectoryInfo(this.LoggingDirectory));
            this.Logger?.Info($"Log directory changed: {this.LoggingDirectory}");
        }
    }
}