using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Castle.Core.Logging;
using Netfox.Core.Models;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.Base;

namespace Netfox.Detective.Infrastructure
{
    class InvestigationSerializationPersistor : ISerializationPersistor<Investigation>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISerializerFactory _serializerFactory;

        public InvestigationSerializationPersistor(IFileSystem fileSystem, ISerializerFactory serializerFactory)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this._serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
        }

        public Investigation Load(string path)
        {
            var fileData = this._fileSystem.File.ReadAllText(path);

            var deserializer = _serializerFactory.Create(typeof(Investigation));
            Investigation investigation;
            using (var fs = new StringReader(fileData))
            {
                using (var reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(fs.ReadToEnd()),
                    new XmlDictionaryReaderQuotas()))
                {
                    investigation = (Investigation) deserializer.ReadObject(reader, false);
                }
            }

            return investigation;
        }

        public ILogger Logger { get; set; }

        public void Save(Investigation investigation)
        {
            if (investigation == null) return;

            var serializer = new DataContractSerializer(typeof(InvestigationInfo));
            try
            {
                using (var writer = new FileStream(investigation.InvestigationInfo.InvestigationFileInfo.FullName,
                    FileMode.Create))
                {
                    serializer.WriteObject(writer, investigation.InvestigationInfo);
                }
            }
            catch (IOException ex)
            {
                this.Logger?.Error("Saving investigation failed", ex);
            }
        }
    }
}