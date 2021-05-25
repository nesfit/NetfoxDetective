using System;
using System.Collections.Generic;

namespace EntityFramework.Utilities
{
    public class TPHConfiguration
    {
        public Dictionary<Type, string> Mappings { get; set; }
        public string ColumnName { get; set; }
    }
}