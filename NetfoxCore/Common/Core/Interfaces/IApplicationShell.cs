using System.Threading.Tasks;

namespace Netfox.Core.Interfaces
{
    public interface IApplicationShell
    {
        Task Run();
        void Finish();
    }
}