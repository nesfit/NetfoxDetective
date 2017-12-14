using System;

using System.Reflection;

using System.Runtime.CompilerServices;

using System.Runtime.InteropServices;


[assembly: AssemblyCompany("Faculty of Information Technology, Brno University of Technology")]

[assembly: AssemblyProduct("Netfox.Detective")]

[assembly: AssemblyCopyright("Copyright Jan Pluskal 2017")]

[assembly: AssemblyTrademark("")]


// Make it easy to distinguish Debug and Release (i.e. Retail) builds;

// for example, through the file properties window.

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]

[assembly: AssemblyDescription("Build: 2017.12.14")]

#else

[assembly: AssemblyConfiguration("Retail")]

[assembly: AssemblyDescription("Flavor=Retail")] // a.k.a. "Comments"

#endif


[assembly: CLSCompliant(true)]


// Setting ComVisible to false makes the types in this assembly not visible

// to COM components.  If you need to access a type in this assembly from

// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]


// Note that the assembly version does not get incremented for every build

// to avoid problems with assembly binding (or requiring a policy or

// <bindingRedirect> in the config file).

//

// The AssemblyFileVersionAttribute is incremented with every build in order

// to distinguish one build from another. AssemblyFileVersion is specified

// in AssemblyVersionInfo.cs so that it can be easily incremented by the

// automated build process.

[assembly: AssemblyVersion("2.0.0.0")]


// By default, the "Product version" shown in the file properties window is

// the same as the value specified for AssemblyFileVersionAttribute.

// Set AssemblyInformationalVersionAttribute to be the same as

// AssemblyVersionAttribute so that the "Product version" in the file

// properties window matches the version displayed in the GAC shell extension.

[assembly: AssemblyInformationalVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
