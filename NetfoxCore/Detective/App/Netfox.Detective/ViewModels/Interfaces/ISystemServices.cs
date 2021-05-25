using System.Windows;

namespace Netfox.Detective.ViewModels.Interfaces
{
    public interface ISystemServices
    {
        string OpenFileDialog(string defaultPath, string defaultExt, string filter);
        string[] OpenFilesDialog(string defaultPath, string defaultExt, string filter);
        string SaveFileDialog(string defaultPath, string defaultExt, string filter);

        MessageBoxResult ShowMessageBox(string strMessage, string strCaption = null,
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Warning);
    }
}