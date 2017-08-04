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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Castle.Core.Logging;
using Netfox.Core.Interfaces;
using WinSCP;

namespace Netfox.AnalyzerSIPFraud.Services
{
    public class SSHFileDownloadService: ILoggable
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SSHFileDownloadService() {}

        public IEnumerable<FileInfo> Download(Uri uri)
        {
            try
            {
                // Setup session options
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = uri.Host,
                    UserName = uri.UserInfo,
                    //Password = "mypassword",
                    GiveUpSecurityAndAcceptAnySshHostKey = true,
                    SshPrivateKeyPath = Path.Combine(Directory.GetCurrentDirectory(), @"Services\netfox2nemea.ppk")
                };

                using (var session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    var transferOptions = new TransferOptions
                    {
                        TransferMode = TransferMode.Binary
                    };

                    TransferOperationResult transferResult;

                    transferResult = session.GetFiles(uri.LocalPath, Path.GetTempPath(), false, transferOptions);
                    
                    // Throw on any error
                    transferResult.Check();
                    foreach(TransferEventArgs transfer in transferResult.Transfers) {
                        Debugger.Break();
                        this.Logger?.Info($"SSHFileDownloadService - Pcap file {transfer.FileName} was downloaded.");
                    }
                    return transferResult.Transfers.Select(tr => new FileInfo(tr.Destination));
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
                this.Logger?.Error($"SSHFileDownloadService - download failed - {uri}", ex);
                //return this.Download(new Uri(@"ssh://pluskal@sauvignon.liberouter.org/home/shared/pluskal/sip_fraud.pcap")); //todo remove
            }
            return new List<FileInfo>();
        }

        #region Implementation of ILoggable
        public ILogger Logger { get; set; } //TODO resolve in IoC
        #endregion
    }
}
