using System.Threading.Tasks;

namespace AlphaChiTech.VirtualizingCollection.Pageing
{
    public interface IAsyncResetProvider
    {
        Task<int> GetCountAsync();
    }
}