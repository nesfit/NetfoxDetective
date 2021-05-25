using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AlphaChiTech.VirtualizingCollection;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Workspaces;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Telerik.Windows.Controls;

namespace Netfox.Detective
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IApplicationShell _applicationShell;

        public App()
        {
            SetCurrentWorkingDirectoryToAssemblyBase();
            this.InitializeComponent();
            //this routine only needs to run once, so first check to make sure the
            //VirtualizationManager isn’t already initialized
            if (!VirtualizationManager.IsInitialized)
            {
                //set the VirtualizationManager’s UIThreadExcecuteAction. In this case
                //we’re using Dispatcher.Invoke to give the VirtualizationManager access
                //to the dispatcher thread, and using a DispatcherTimer to run the background
                //operations the VirtualizationManager needs to run to reclaim pages and manage memory.
                VirtualizationManager.Instance.UiThreadExcecuteAction = a =>
                {
                    try
                    {
                        this.Dispatcher.Invoke(a);
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error("Detective have an unexpected exception in UI.", ex);
                    }
                };
                new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background,
                    delegate { VirtualizationManager.Instance.ProcessActions(); }, this.Dispatcher).Start();
            }

            DispatcherHelper.Initialize();

            //var ci = new CultureInfo("en-US");
            //CultureInfo.DefaultThreadCurrentCulture = ci;
            //CultureInfo.DefaultThreadCurrentUICulture = ci;

            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;

            this.Exit += this.OnExit;

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += this.AppDomain_UnhandledException;
        }

        public ILogger Logger { get; set; }
        private WindsorContainer ApplicationWindsorContainer { get; set; }

        // [STAThread]
        // public static void Main()
        // {
        //     SetCurrentWorkingDirectoryToAssemblyBase();
        //
        //     //var log = Logger.Logger.Instance; //dirty hack to instantiate create singleton
        //     //log = null;
        //
        //     var app = new App();
        //     app.Run();
        // }

        protected override void OnStartup(StartupEventArgs e)
        {

            var splashScreen = new SplashScreen(@"Views\Resources\Netfox_splash.png");
            splashScreen.Show(true);

            base.OnStartup(e);
            StyleManager.ApplicationTheme = new Windows8Theme();
            this.ApplicationWindsorContainer =
                new WindsorContainer("DetectiveApp", new DefaultKernel(), new DefaultComponentInstaller());
            this.ApplicationWindsorContainer.Register(Component.For<IWindsorContainer, WindsorContainer>()
                .Instance(this.ApplicationWindsorContainer));
            this.ApplicationWindsorContainer.Install(FromAssembly.InDirectory(
                new AssemblyFilter(".").FilterByAssembly(a => !a.IsDynamic),
                new InstallerFactoryFilter(typeof(IDetectiveApplicationWindsorInstaller))));
            
            this.Logger = this.ApplicationWindsorContainer.Resolve<ILogger>();
            this.Logger?.Info("Application Starts");
            this._applicationShell = this.ApplicationWindsorContainer.Resolve<IApplicationShell>();
            this._applicationShell.Run();
            this.Logger?.Info("Application Started");
            this.CheckCommandLineArgs();
            
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                // Mega hack.
                string dll = Path.GetFullPath(Path.Combine(".", new AssemblyName(e.Name).Name + ".dll"));
                if (File.Exists(dll))
                    return Assembly.LoadFile(dll);
                    
                return null;
            };
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception) args.ExceptionObject;
            this.Logger?.Error("An unhandled exception just occurred: " + ex.Message, ex);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            this.Logger?.Error("An unhandled exception just occurred: " + ex.Exception.Message, ex.Exception);
            ex.Handled = true;
        }

        /// <summary>
        ///     Check command line arguments.
        ///     Argument could be only file paths to associated files:
        ///     *.nfw - netfox workspace
        ///     *.nfi - netfox investigation
        /// </summary>
        /// <remarks>   Jan, 25. 10. 2014. </remarks>
        private void CheckCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Count() == 1)
            {
                return;
            }

            if (args.Count() > 2)
            {
                var msg = args.Skip(1).Aggregate("Application started with too many arguments> ",
                    (current, s) => current + (s + " "));
                this.Logger?.Error(msg);
                return;
            }

            var filePath = args[1];
            var extension = Path.GetExtension(filePath);
            switch (extension)
            {
                case ".nfw":
                    this.OpenSelectedWorkspace(filePath);
                    break;
                case ".nfi":
                    this.Logger?.Error("Opening of Investigation files is not supported yet!");
                    break;
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            this._applicationShell.Finish();
            this.Logger?.Info("Application Exits");
        }

        private void OpenSelectedWorkspace(string workspaceFilePath)
        {
            var messenger = this.ApplicationWindsorContainer.Resolve<IDetectiveMessenger>();
            var workspacePersistor = this.ApplicationWindsorContainer.Resolve<ISerializationPersistor<Workspace>>();

            var workspace = workspacePersistor.Load(workspaceFilePath);
            messenger.Send(new LoadedWorkspaceMessage
            {
                Workspace = workspace
            });
        }

        private static void SetCurrentWorkingDirectoryToAssemblyBase()
        {
            //var execfilepathUri = Assembly.GetExecutingAssembly().GetName().CodeBase;
            //var uri = new Uri(execfilepathUri);
            //var absolutePath = Uri.UnescapeDataString(uri.AbsolutePath);
            //var path = Path.GetDirectoryName(absolutePath);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(App).Assembly.Location));
        }
    }
}