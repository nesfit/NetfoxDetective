using System.IO;
using System.IO.Abstractions;
using System.Runtime.Serialization;
using System.Xml;
using Castle.Core.Logging;
using Netfox.Core.Interfaces;

namespace Netfox.Core.Models
{
    public class InvestigationInfoLoader : IInvestigationInfoLoader
    {
        public ILogger Logger { get; }

        public InvestigationInfoLoader(ILogger logger)
        {
            this.Logger = logger;
        }

        public IInvestigationInfo Load(FileInfoBase fileInfo)
        {
            //todo catch exception in caller
            if (!fileInfo.Exists)
            {
                this.Logger?.Error("Investigation cannot be loaded from non existing path");
                return null;
            }

            var deserializer = new DataContractSerializer(typeof(InvestigationInfo));

            using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                using (var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {
                    var investigationInfo = (InvestigationInfo) deserializer.ReadObject(reader, true);
                    return investigationInfo;
                }
            }
        }
    }
}