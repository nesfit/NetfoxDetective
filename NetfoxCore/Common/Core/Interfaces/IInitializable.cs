using System.Threading.Tasks;

namespace Netfox.Core.Interfaces
{
    public interface IInitializable
    {
        Task Initialize();
    }
}