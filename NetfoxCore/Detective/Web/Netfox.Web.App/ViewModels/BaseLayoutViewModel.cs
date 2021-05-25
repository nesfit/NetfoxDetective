using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace Netfox.Web.App.ViewModels
{
    public abstract class BaseLayoutViewModel : LayoutViewModel
    {
        #region Overrides of LayoutViewModel
        public override string Title { get; }
        #endregion
    }
}

