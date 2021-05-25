using System.Threading.Tasks;
using System.Windows.Input;

namespace Netfox.Detective.Interfaces
{
    public interface IAsyncCommand: ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}