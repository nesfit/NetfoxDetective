using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Netfox.Core.Infrastructure;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Infrastructure
{
     class WorkspacePathSerializationPersistor : IWorkspacePathSerializationPersistor
    {
        private readonly INetfoxSettings _netfoxSettings;
        private readonly ISerializerFactory _serializerFactory;
        private readonly WorkspacePathMapper _workspacePathMapper;

        public WorkspacePathSerializationPersistor(INetfoxSettings netfoxSettings, ISerializerFactory serializerFactory,
            WorkspacePathMapper workspacePathMapper)
        {
            this._netfoxSettings = netfoxSettings ?? throw new ArgumentNullException(nameof(netfoxSettings));
            this._serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
            this._workspacePathMapper =
                workspacePathMapper ?? throw new ArgumentNullException(nameof(_workspacePathMapper));
        }

        public IEnumerable<WorkspacePath> Load(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<WorkspacePath>();
            }

            var deserializer = _serializerFactory.Create(typeof(List<WorkspacePath>));
            List<WorkspacePath> deserializedItems;
            using (var fs = new StringReader(source))
            {
                using (var reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(fs.ReadToEnd()),
                    new XmlDictionaryReaderQuotas()))
                {
                    deserializedItems = (List<WorkspacePath>) deserializer.ReadObject(reader, false);
                }
            }

            return deserializedItems;
        }

        public IEnumerable<WorkspacePath> Load()
        {
            return this.Load(this._netfoxSettings.LastWorkspaces);
        }

        public void Save(IEnumerable<WorkspacePath> workspacePaths)
        {
            var serializer = _serializerFactory.Create(typeof(List<WorkspacePath>), new[]
            {
                typeof(WorkspacePath)
            });

            using (var writer = new MemoryStream())
            {
                serializer.WriteObject(writer, workspacePaths);
                writer.Position = 0;

                this._netfoxSettings.LastWorkspaces = Encoding.UTF8.GetString(writer.ToArray());
            }

            _netfoxSettings.Save();
        }

        public void Save(IEnumerable<Workspace> workspaces)
        {
            var workspacePaths = this._workspacePathMapper.FromWorkspace(workspaces).ToList();

            this.Save(workspacePaths);
        }
    }
}