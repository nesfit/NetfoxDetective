// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperMAFF.Interfaces;
using Netfox.SnooperMAFF.Models.Archives;
using Netfox.SnooperMAFF.Models.Common;
using Netfox.SnooperMAFF.Models.Containers;
using Netfox.SnooperMAFF.Models.Exports;
using Netfox.SnooperMAFF.Models.Objects;
using Netfox.SnooperMAFF.Models.Parsers;

namespace Netfox.SnooperMAFF
{
    /// <summary>
    ///     Core class of MAFF snooper. Class loads HTTP messages and Parses HTTP messages. Core controls archive creating and
    ///     exporting to output MAFF format.
    /// </summary>
    public class SnooperMAFF : SnooperBase
    {
        #region Class Attributes
        private List<Archive> _listOfArchives = new List<Archive>();

        private readonly DataContainer _oContainer = new DataContainer();

        private IWrapperConstants _constains;

        public override string Name => "MAFF";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "ssl",
            "MAFF"
        };

        public override string Description => "MAFF snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            80,
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportMAFF();
        #endregion //Class Attributes

        #region Class Methods
        /// <summary>
        ///     Gets the configuration from settings view or configuration file.
        /// </summary>
        private void GetConfiguration()
        {
            this.GetDataFromConfigFile();
        }

        /// <summary>
        ///     Gets the configuration from settings view.
        /// </summary>
        /// <returns>Return if next configuration may be loaded from file or not</returns>
        private bool GetConfigurationFromSettingsView()
        {
            //ApplicationSettings.Settings
            Constants.GenerateSnapshots = this._constains.GenerateSnapshots;
            Constants.SnapshotsTimeSeparator = this._constains.SnapshotsTimeSeparator;
            Constants.ObjectRewrite = this._constains.ObjectRewrite;
            return this._constains.StaticTurnOffConfigurationFile;
        }

        /// <summary>
        ///     Gets the settings from settings view, or if enabled update settings from configuration file.
        /// </summary>
        private void GetDataFromConfigFile()
        {
            if(!this.GetConfigurationFromSettingsView()) { var oConf = new Config(this.ExportBaseDirectory); }
        }

        /// <summary>
        ///     Creates the template folder for unpacking archives in MAFF visualization.
        /// </summary>
        private void CreateTemplateFolder()
        {
            var sPath = Environment.GetEnvironmentVariable("temp");
            sPath = sPath + @"\MAFF";
            if(Directory.Exists(sPath))
            {
                try { Directory.Delete(sPath, true); }
                catch(Exception)
                {
                    // ignored
                }
            }
            Directory.CreateDirectory(sPath);
        }

        /// <summary>
        ///     Parse,validate HTTP object and create archive objects from HTTP object.
        /// </summary>
        /// <param name="oResponseHeader">The response header of HTTP Message.</param>
        /// <param name="oRequestHTTPMsg">The request HTTP Message.</param>
        private void ParseAndCreateArchiveObject(HttpResponseHeader oResponseHeader, HTTPMsg oRequestHTTPMsg)
        {
            var oArchiveObject = ObjectParser.CreateDataObject(oResponseHeader, oRequestHTTPMsg);
            if(oArchiveObject == null) { return; }
            if(oArchiveObject is ParentObject) { this._listOfArchives.Add(new Archive((ParentObject) oArchiveObject, this.ExportBaseDirectory)); }
            else { this._oContainer.AddObject(oArchiveObject); }
        }

        /// <summary>
        ///     Method 1. phase: Loads exported HTTP objects from HTTP export. Validate and parse HTTP objects.
        /// </summary>
        private void GetExportedHTTPObjects()
        {
            Console.WriteLine(@"SnooperMAFF.HetExportedHTTPObjects() called");
            foreach(var snooperExportBase in this.SourceExports)
            {
                if(!(snooperExportBase is SnooperExportHTTP)) continue;

                if(snooperExportBase.ExportValidity == ExportValidity.Malformed) continue;
                if(snooperExportBase.ExportObjects.Count == 0) continue;

                var oArrayOfExporedObjects = snooperExportBase.ExportObjects.Select(exportedObject => exportedObject as SnooperExportedDataObjectHTTP)
                    .Select(oExportedObject => oExportedObject.Message).Where(oExportedMessage => oExportedMessage.HTTPHeader is HttpRequestHeader).ToArray();

                foreach(var oExportedMsg in oArrayOfExporedObjects)
                {
                    if(oExportedMsg == null) continue;
                    if(!oExportedMsg.Valid) continue;
                    var oHttpRequestHeader = oExportedMsg.HTTPHeader as HttpRequestHeader;

                    /*Check if is it  GET request */
                    if(oHttpRequestHeader?.Method != HTTPRequestMethod.GET) { continue; }
                    if(!oExportedMsg.PairMessages.Any()) continue;
                    if(!oExportedMsg.PairMessages[0].Valid) continue;
                    if(oExportedMsg.PairMessages[0].HTTPContent.Content == null) continue;
                    if(oExportedMsg.PairMessages[0].HTTPContent.Content.LongLength == 0) continue;

                    var oHttpResponseHeader = oExportedMsg.PairMessages[0].HTTPHeader as HttpResponseHeader;
                    if(oHttpResponseHeader == null) continue;
                    if(!oHttpResponseHeader.StatusCode.Equals("200")) continue;

                    this.ParseAndCreateArchiveObject(oHttpResponseHeader, oExportedMsg);
                }
            }
        }

        /// <summary>
        ///     Method 2. Phase:  Constructs the archives from list of root objects. Call finalize state on each archive.
        /// </summary>
        private void ConstructArchives()
        {
            //Check and remove unused archives
            this._listOfArchives = this._listOfArchives.Where(oArchive => this._oContainer.CheckReferrers(oArchive.GetMainPartParentHostReferent())).ToList();

            //Turn on Searching
            foreach(var oArchive in this._listOfArchives) { oArchive.ProcessArchive(this._oContainer); }
        }

        /// <summary>
        ///     Method . Phase: Finalize constructed archives for export. Call finalize state on each archive.
        /// </summary>
        private void ArchivesFinalize()
        {
            foreach(var archive in this._listOfArchives)
            {
                archive.ArchiveFinalize();

                this.OnConversationProcessingBegin();
                this.OnBeforeDataExporting();

                var exportedObject = new SnooperExportedArchive(this.SnooperExport)
                {
                    TimeStamp = archive.ArchiveDateTime,
                    Archive = archive
                };
                exportedObject.ExportSources.Add(archive.ArchiveExportSource);
                this.SnooperExport.AddExportObject(exportedObject);

                this.OnAfterDataExporting();
                this.OnConversationProcessingEnd();
            }
        }

        /// <summary>
        ///     Core method of MAFF snooper seperated to three phases. One load HTTP objects, second construct archives and last
        ///     one finalize archives for exporting.
        /// </summary>
        protected override void RunBody()
        {
            this.CreateTemplateFolder();
            this.GetConfiguration();

            var s1 = Stopwatch.StartNew();
            this.GetExportedHTTPObjects();
            s1.Stop();
            Console.WriteLine("State No.1: " + s1.Elapsed.TotalMilliseconds.ToString("0.00 ms"));

            var s2 = Stopwatch.StartNew();
            this.ConstructArchives();
            s2.Stop();
            Console.WriteLine("State No.2: " + s2.Elapsed.TotalMilliseconds.ToString("0.00 ms"));

            var s3 = Stopwatch.StartNew();
            this.ArchivesFinalize();
            s3.Stop();
            Console.WriteLine("State No.3: " + s3.Elapsed.TotalMilliseconds.ToString("0.00 ms"));
        }

        //deprecated
        protected override void ProcessConversation() { }

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportMAFF();

        public SnooperMAFF() { }

        public SnooperMAFF(
            WindsorContainer investigationWindsorContainer,
            IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory,
            IWrapperConstants constants) : base(investigationWindsorContainer, sourceExports, exportDirectory)
        {
            this._constains = constants;
        }
        #endregion //End Class Methods
    }
}