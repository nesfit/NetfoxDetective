using DotVVM.Framework.Runtime.Filters;

namespace Netfox.Web.App.ViewModels.Settings
{
    [Authorize]
    public abstract class SettingsViewModel : MasterpageViewModel
    {
        public override string ColumnCSSClass => "wrapper-settings";

        public override string ColumnName => "Settings";

    }
}

