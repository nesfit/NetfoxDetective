using System.Runtime.Serialization;

namespace Netfox.Detective.Models.WorkspacesAndSessions
{
    [DataContract(Name = "WorkspacePath", Namespace = "Netfox.Detective.Models.WorkspacesAndSessions")]
    public class WorkspacePath
    {
        [DataMember] public string Path { get; set; }
    }
}