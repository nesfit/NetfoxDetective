using System.Threading;

namespace Netfox.Core.Interfaces
{
    public interface IBgTasksManagerService : IDetectiveService
    {
        IBgTask CreateTask(string title, string description, CancellationToken cancellationToken);
    }
}