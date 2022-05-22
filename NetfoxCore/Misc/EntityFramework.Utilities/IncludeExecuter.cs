using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFramework.Utilities
{
    public class IncludeExecuter<T>
    {
        internal Type ElementType { get; set; }
        internal Action<IEnumerable<MethodCallExpression>, IEnumerable> Loader { get; set; }
        internal Action<object> SingleItemLoader { get; set; }
    }
}