using System;
using System.Collections.Generic;
using Castle.Windsor.Installer;

namespace Netfox.Core.Infrastructure
{
    public class InstallerFactoryFilter:InstallerFactory
    {
        public Type ImplementingInterface { get; }

        public InstallerFactoryFilter(Type implementingInterface) { this.ImplementingInterface = implementingInterface; }
        #region Overrides of InstallerFactory
        public override IEnumerable<Type> Select(IEnumerable<Type> installerTypes)
        {
            foreach(var installerType in installerTypes)
            {
                if(installerType.Assembly.IsDynamic)
                    continue; //skipping dynamic assembly because otherwise it emmits  "System.NotSupportedException : The invoked member is not supported in a dynamic assembly." when running test on VS TS.
                //if(this.ImplementingInterface.IsInstanceOfType(installerType)) { yield return installerType; }
                //if(installerType.IsInstanceOfType(this.ImplementingInterface)) { yield return installerType; }
                if (this.ImplementingInterface.IsAssignableFrom(installerType))
                {
                    yield return installerType;
                }

            }
        }
        #endregion
    }
}