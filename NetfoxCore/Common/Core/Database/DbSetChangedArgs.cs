using System;

namespace Netfox.Core.Database
{
    public class DbSetChangedArgs
    {
        public DbSetChangedArgs(Type changedDbSetType)
        {
            this.ChangedDbSetType = changedDbSetType;
        }

        public Type ChangedDbSetType { get; }
    }
}