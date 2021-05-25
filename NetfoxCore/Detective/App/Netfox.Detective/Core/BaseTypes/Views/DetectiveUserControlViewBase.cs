using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Netfox.Detective.Core.BaseTypes.Views
{
    public abstract class DetectiveUserControlViewBase : UserControl
    {
        protected DetectiveUserControlViewBase()
        {
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }
    }
}