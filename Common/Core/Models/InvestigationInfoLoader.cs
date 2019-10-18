// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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

        public InvestigationInfoLoader(ILogger logger) {
            this.Logger = logger;
        }
        public IInvestigationInfo Load(FileInfoBase fileInfo)
        {
            //todo catch exception in caller
            if(!fileInfo.Exists)
            {
                this.Logger?.Error("Investigation cannot be loaded from non existing path");
                return null;
            }

            var deserializer = new DataContractSerializer(typeof(InvestigationInfo));

            using(var fs = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                using(var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {
                    var investigationInfo = (InvestigationInfo) deserializer.ReadObject(reader, true);
                    return investigationInfo;
                }
            }
        }
    }
}