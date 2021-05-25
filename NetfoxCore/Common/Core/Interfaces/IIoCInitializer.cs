using System.Threading.Tasks;

namespace Netfox.Core.Interfaces
{
    public interface IIoCInitializer
    {
        Task RegisterAll();
    }
}