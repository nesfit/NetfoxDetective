using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.GUI.Detective.ViewModels;

namespace Netfox.GUI.Detective.Core
{
    internal class ViewModelLocatorInternal : DynamicObject, IWindsorInstaller, IViewModelLocator
    {
        public static ViewModelLocator Instance = new ViewModelLocator();
        private readonly Dictionary<string, Type> _initializeVmsDictionary = new Dictionary<string, Type>();
        private MethodInfo _castleWindsorResolveMethod;
        private bool _initialized;
        public ViewModelLocatorInternal() { this.IsInDebug = Debugger.IsAttached; }

        public MethodInfo CastleWindsorResolveMethod
        {
            get
            {
                if(this._castleWindsorResolveMethod != null) { return this._castleWindsorResolveMethod; }
                this._castleWindsorResolveMethod =
                    this.Container.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(x => x.Name == "Resolve").Select(x => new
                    {
                        Method = x,
                        Params = x.GetParameters(),
                        Args = x.GetGenericArguments()
                    }).Where(x => x.Params.Length == 1 && x.Args.Length == 0).Select(x => x.Method).SingleOrDefault();
                if(this.CastleWindsorResolveMethod == null) { throw new MissingMethodException("CastleWindsor has missing Resolve method."); }
                return this._castleWindsorResolveMethod;
            }
        }

        public IConfigurationStore Store { get; set; }
        public IWindsorContainer Container { get; set; }

        public void Initialize()
        {
            var assembliesList = new[]
            {
                "Netfox.Detective.ViewModels"
                //"Netfox.Detective.ModelsExports", 
                //"Netfox.Detective.ModelsBase" 
            };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //var solutionPath = Directory.GetCurrentDirectory()+@"\..\..\..\";
            //var projectBaseDir = IsInDebug ? solutionPath : Directory.GetCurrentDirectory();
            //assemblies.AddRange(GetAssembliesPaths(projectBaseDir, assembliesList).Select(Assembly.LoadFrom));
            var vmTypes = new List<Type>();
            foreach(var assembly in assemblies.Distinct())
            {
                try { 
                    vmTypes.AddRange(assembly.GetTypes().Where(t => 
                    (
                    t.IsSubclassOf((typeof(DetectiveApplicationPaneViewModelBase))) ||
                    t.IsSubclassOf((typeof(DetectiveWindowViewModelBase))) 
                    )
                    && !t.IsAbstract));
                    //vmTypes.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf((typeof(ViewModelBase))) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null));
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

            foreach(var vmType in vmTypes)
            {
                try {
                    this.IoCRegisterType(vmType);
                }
                catch(ComponentRegistrationException componentRegistrationException)
                {
                    try
                    {
                        this.InternalyRegisterType(vmType.Name, vmType);
                        Console.WriteLine(componentRegistrationException.Message);
                    }
                    catch(Exception) {
                        Console.WriteLine(componentRegistrationException.Message);
                    }
                }
            }
            this._initialized = true;
        }

        public bool IsInDebug { get; }

        // If you try to get a value of a property  
        // not defined in the class, this method is called. 
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if(!this._initialized) { this.Initialize(); }
            // Converting the property name to lowercase 
            // so that property names become case-insensitive. 
            var type = this._initializeVmsDictionary[binder.Name];
            result = type != null
                ? this.CastleWindsorResolveMethod.Invoke(this.Container, new object[1]
                {
                    type
                })
                : null;
            return result != null;
        }

        // If you try to set a value of a property that is 
        // not defined in the class, this method is called. 
        public override bool TrySetMember(SetMemberBinder binder, object value) { return false; }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            this.Container = container;
            this.Store = store;
            this.Initialize();
        }

        private static IEnumerable<string> GetAssembliesPaths(string baseDirectory, IEnumerable<string> assembliesList)
        {
            var toFindAssemblyList = new List<string>(assembliesList);
            var lstFilesFound = new List<string>();
            try
            {
                foreach(var  f in Directory.GetFiles(baseDirectory, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(f);

                        if(toFindAssemblyList.Contains(assemblyName.Name))
                            //if (toFindAssemblyList.Select(a => a.StartsWith(assemblyName.Name)).Any())
                        {
                            lstFilesFound.Add(f);
                            toFindAssemblyList.Remove(assemblyName.Name);
                            if(!toFindAssemblyList.Any()) { break; }
                        }
                    }
                    catch(Exception excpt) {
                        Console.WriteLine(excpt.Message);
                    }
                }
            }
            catch(Exception excpt) {
                Console.WriteLine(excpt.Message);
            }
            return lstFilesFound.ToArray();
        }

        private void InternalyRegisterType(string MemberInfoName, Type vmType) { this._initializeVmsDictionary.Add(MemberInfoName, vmType); }

        private bool IoCRegisterType(Type vmType)
        {
            if(vmType.ContainsGenericParameters) {
                return false; //VN is not allowed to have a generic parameters
            }
            var normalCctors = vmType.GetConstructors(BindingFlags.Instance|BindingFlags.Public);
            var staticCctors = vmType.GetConstructors(BindingFlags.Static|BindingFlags.NonPublic);
            if(normalCctors.Any()) {
                this.Container.Register(Component.For(vmType));
            }
            else if(staticCctors.Any()) {
                this.Container.Register(Component.For(vmType).Instance(Activator.CreateInstance(vmType, true)));
            }
            else
            {
                return false;
            }
            this.InternalyRegisterType(vmType.Name, vmType);
            return true;
        }
    }
}