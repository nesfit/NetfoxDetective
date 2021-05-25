using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Castle.Core.Logging;
using Netfox.Core.Infrastructure;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Infrastructure
{
    class WorkspaceSerializationPersistor : ISerializationPersistor<Workspace>
    {
        private readonly INetfoxSettings _netfoxSettings;
        private readonly IFileSystem _fileSystem;
        private readonly ISerializerFactory _serializerFactory;
        public ILogger Logger { get; set; }

        public WorkspaceSerializationPersistor(INetfoxSettings netfoxSettings, IFileSystem fileSystem,
            ISerializerFactory serializerFactory)
        {
            this._netfoxSettings = netfoxSettings ?? throw new ArgumentNullException(nameof(netfoxSettings));
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this._serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
        }

        public Workspace Load(string path)
        {
            var fileData = this._fileSystem.File.ReadAllText(path);

            var deserializer = _serializerFactory.Create(typeof(Workspace));
            Workspace workspace;
            using (var fs = new StringReader(fileData))
            {
                using (var reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(fs.ReadToEnd()),
                    new XmlDictionaryReaderQuotas()))
                {
                    workspace = (Workspace) deserializer.ReadObject(reader, false);
                }
            }

            workspace.LastRecentlyUsed = DateTime.Now;
            return workspace;
        }

        public void Save(Workspace workspace)
        {
            if (workspace == null || workspace.WorkspaceDirectoryInfo == null || string.IsNullOrEmpty(workspace.Name))
            {
                return;
            }

            var serializer = new DataContractSerializer(typeof(Workspace),
                new[] {typeof(System.IO.Abstractions.FileSystem), typeof(DirectoryInfoWrapper)});
            try
            {
                var workspaceFileInfo = new FileInfo(Path.Combine(workspace.WorkspaceDirectoryInfo.FullName,
                    workspace.Name + "." + _netfoxSettings.WorkspaceFileExtension));
                using (var writer = new FileStream(workspaceFileInfo.FullName, FileMode.Create))
                {
                    serializer.WriteObject(writer, workspace);
                }
            }
            catch (IOException ex)
            {
                this.Logger?.Error($"Worspace save failed: {workspace?.Name}", ex);
            }
        }
    }
}