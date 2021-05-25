using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace Netfox.Web.App.ViewModels
{
    public abstract class MasterpageViewModel : BaseLayoutViewModel
    {
        public abstract string ColumnName { get; }

        public abstract bool ShowToolbar { get; }

        public abstract string ColumnCSSClass { get; }

    }
}

