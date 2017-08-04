// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AlphaChiTech.Virtualization;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Detective.Services;
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
                    try { this.Dispatcher.Invoke(a); }
                    catch (Exception ex)
                    {
                        this.Logger?.Error("Detective have an unexpected exception in UI.", ex);
                    }
                };
                new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, delegate { VirtualizationManager.Instance.ProcessActions(); }, this.Dispatcher).Start();
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

        [STAThread]
        public static void Main()
        {
            SetCurrentWorkingDirectoryToAssemblyBase();

            //var log = Logger.Logger.Instance; //dirty hack to instantiate create singleton
            //log = null;

            var app = new App();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //var splashScreen = new SplashScreen("splash.png");
            //splashScreen.Show(true);

            base.OnStartup(e);
            StyleManager.ApplicationTheme = new Windows8Theme();
            this.ApplicationWindsorContainer = new WindsorContainer("DetectiveApp", new DefaultKernel(), new DefaultComponentInstaller());
            this.ApplicationWindsorContainer.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(this.ApplicationWindsorContainer));
            this.ApplicationWindsorContainer.Install(FromAssembly.InDirectory(new AssemblyFilter(".").FilterByAssembly(a=>!a.IsDynamic), new InstallerFactoryFilter(typeof(IDetectiveApplicationWindsorInstaller))));

            this.Logger = this.ApplicationWindsorContainer.Resolve<ILogger>();
            this.Logger?.Info("Application Starts");
            this._applicationShell = this.ApplicationWindsorContainer.Resolve<IApplicationShell>();
            this._applicationShell.Run();
            this.Logger?.Info("Application Started");
            this.CheckCommangLineArgs();
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception)args.ExceptionObject;
            this.Logger?.Error("An unhandled exception just occurred: " + ex.Message, ex);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            this.Logger?.Error("An unhandled exception just occurred: " + ex.Exception.Message, ex.Exception);
            ex.Handled = true;
        }

        /// <summary>
        ///     Check commang line arguments.
        ///     Argument could be only file paths to associated files:
        ///     *.nfw - netfox workspace
        ///     *.nfi - netfox investigation
        /// </summary>
        /// <remarks>   Jan, 25. 10. 2014. </remarks>
        private void CheckCommangLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Count() == 1) { return; }
            if (args.Count() > 2)
            {
                var msg = args.Skip(1).Aggregate("Application started with too many arguments> ", (current, s) => current + (s + " "));
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

        private async void OpenSelectedWorkspace(string workspaceFilePath)
        {
            var workspaceManager = this.ApplicationWindsorContainer.Resolve<WorkspacesManagerService>();
            await workspaceManager.OpenWorkspace(workspaceFilePath);
        }

        private static void SetCurrentWorkingDirectoryToAssemblyBase()
        {
            var execfilepathUri = Assembly.GetExecutingAssembly().GetName().CodeBase;
            var uri = new Uri(execfilepathUri);
            var absolutePath = Uri.UnescapeDataString(uri.AbsolutePath);
            var path = Path.GetDirectoryName(absolutePath);
            Directory.SetCurrentDirectory(path);
        }
    }
}