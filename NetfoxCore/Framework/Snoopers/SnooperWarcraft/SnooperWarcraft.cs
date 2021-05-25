// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using System;
using System.Collections.Generic;
using System.IO;
using Castle.Windsor;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperWarcraft.Models;

namespace Netfox.Snoopers.SnooperWarcraft
{
    public class SnooperWarcraft : SnooperBase
    {
        public override string Name => "Warcraft";

        // here we say what type of conversations (by NBAR name) we want to process
        public override string[] ProtocolNBARName => new[] { "Warcraft" };

        public override string Description => "Snooper for Warcraft chat";

		public override int[] KnownApplicationPorts => new[] { 0 };

        internal new SnooperExportWarcraft SnooperExport => base.SnooperExport as SnooperExportWarcraft;
        public override SnooperExportBase PrototypeExportObject { get; } = new SnooperExportWarcraft();

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportWarcraft();

        protected override void RunBody()
        {
            foreach (var file in this.SourceFiles)
            {
                this.OnConversationProcessingBegin();

                this._file = file;
                this.ProcessConversation();

                this.OnConversationProcessingEnd();
            }   
        }

        protected override void ProcessConversation()
        {
            Console.WriteLine(@"SnooperWarcraft.ProcessConversation() called");

            // we need a stream to read from
            var reader = new StreamReader(this._file.OpenRead());

            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();

                // parse protocol
                // this is self parsing message, it just needs a reader to get data from
                var message = new WarcraftMsg(reader.ReadLine());
                if (!message.Valid) // is not chat message
                    continue;

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();

                var exportedObject = new SnooperExportedWarcraftMessage(this.SnooperExport)
                {
                    TimeStamp = message.Timestamp,
                    Message = message.MessageContent,
                    Sender = message.Sender,
                    Receiver = message.Receiver,
                    Text = message.Text,
                    Type = message.MessageType
                };

                // export
                exportedObject.ExportSources.Add(this.CurrentConversation);
                this.SnooperExport.AddExportObject(exportedObject);

                this.OnAfterDataExporting();
            } while (!reader.EndOfStream);
        }

        private FileInfo _file = null;

        public SnooperWarcraft() { } //Must exists for creation of snooper prototype to obtain describing information
        public SnooperWarcraft(WindsorContainer investigationWindsorContainer, IEnumerable<FileInfo> sourceFiles, DirectoryInfo exportDirectory) : base(investigationWindsorContainer,sourceFiles, exportDirectory) { }
    }
}
