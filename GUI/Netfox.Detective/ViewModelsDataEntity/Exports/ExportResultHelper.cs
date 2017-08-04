// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.IO;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    public class ExportResultHelper
    {
        public static bool IsImageByExtension(string path)
        {
            var ext = Path.GetExtension(path);
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".tiff";
        }

        public class Call
        {
            public DateTime StartTimeStamp { get; set; }
            public DateTime EndTimeStamp { get; set; }
            public TimeSpan Duration { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string State { get; set; }
            public string Protocol { get; set; }
            public ExportVm ResultVm { get; set; }
            public SnooperExportedObjectBase DataVm { get; set; }
        }

        public class Credential
        {
            public DateTime TimeStamp { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Protocol { get; set; }
            public ExportVm ResultVm { get; set; }
            public SnooperExportedObjectBase DataVm { get; set; }
        }

        public class ResultFile
        {
            private long size = -1;
            public string Name { get; set; }
            public DateTime TimeStamp { get; set; }
            public string Detail { get; set; }
            public string Source { get; set; }

            public string Extension => System.IO.Path.GetExtension(this.Name);

            public long Size
            {
                get
                {
                    if(this.size >= 0) { return this.size; }

                    var info = new FileInfo(this.Path);
                    this.size = info.Length;
                    return this.size;
                }
            }

            public string Path { get; set; }
            public ExportVm ResultVm { get; set; }
            public SnooperExportedObjectBase DataVm { get; set; }
        }

        public class ResultImage
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public ExportVm ResultVm { get; set; }
            public SnooperExportedObjectBase DataVm { get; set; }
        }

        public class ResultMessage
        {
            public DateTime TimeStamp { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string Protocol { get; set; }
            public string Message { get; set; }
            public ExportVm ResultVm { get; set; }
            public SnooperExportedObjectBase DataVm { get; set; }
        }

        public class ResultWebPage : ResultFile {}
    }
}