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
using System.IO;
using System.IO.Compression;
using Netfox.Snoopers.SnooperMAFF.Models.Archives;

namespace Netfox.Snoopers.SnooperMAFF.WPF.View.Events
{
    /// <summary>
    /// Singleton used for switching between current archive parts and decompressing whole archive with all parts.
    /// </summary>
    public sealed class DecompressArchiveEvent
    {
        /// <summary>
        /// View logic for archive view visualisation and archive view.
        /// Determines if one of windows is still open (using) for deallocating decompressed archive.
        /// </summary>
        private struct ViewModelBooleanLogic
        {
            private bool _bModel1;
            private bool _bModel2;
            public ViewModelBooleanLogic(bool firstmodel, bool secondmodel) { this._bModel1 = firstmodel; this._bModel2 = secondmodel; }
            public void ChangeStateOfFirstViewModel(bool modelstate) { this._bModel1 = modelstate; }
            public void ChangeStateOfSecondViewModel(bool modelstate) { this._bModel2 = modelstate; }
            public bool GetViewModelLogic() { return this._bModel1 || this._bModel2; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DecompressArchiveEvent"/> class from being created.
        /// </summary>
        private DecompressArchiveEvent() { }

        private Dictionary<string, ViewModelBooleanLogic> _decompressArchivesStats;

        private static DecompressArchiveEvent _instance;

        /// <summary>
        /// Creates the directory tree from Archive compressed tree (ZIP).
        /// </summary>
        /// <param name="sPath">The path for extract zip tree.</param>
        /// <param name="sZipPath">The path of zip file.</param>
        private static void CreateDirectoryTree(string sPath, string sZipPath)
        {
            try
            {
                var archive = ZipFile.Open(sZipPath, ZipArchiveMode.Read);
                archive.ExtractToDirectory(sPath);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Deletes the directory tree of decompressed archive.
        /// </summary>
        /// <param name="sExtractedPath">The s extracted path.</param>
        private static void DeleteDirectoryTree(string sExtractedPath)
        {
            try { Directory.Delete(sExtractedPath, true); }
            catch (Exception)
            {
                // ignored
            }
        }
        /// <summary>
        /// Gets the instance of Singleton.
        /// </summary>
        /// <returns>Returns instance of sigleton</returns>
        public static DecompressArchiveEvent GetInstance()
        {
            if (_instance != null) { return _instance; }
            _instance = new DecompressArchiveEvent
            {
                _decompressArchivesStats = new Dictionary<string, ViewModelBooleanLogic>()
            };
            return _instance;
        }

        /// <summary>
        /// Loads the zip archive.
        /// </summary>
        /// <param name="oArchive">The loaded archive.</param>
        /// <param name="sView">The string definition of current view.</param>
        public void LoadZipArchive(Archive oArchive, string sView)
        {
            var archivePath = oArchive.ExportDirectory + @"\" + oArchive.ArchiveName;
            if (!this._decompressArchivesStats.ContainsKey(archivePath))
            {
                var sPath = Environment.GetEnvironmentVariable("temp");
                CreateDirectoryTree(sPath + @"\MAFF", archivePath);
                this._decompressArchivesStats.Add(archivePath, new ViewModelBooleanLogic(false, false));
            }
            if (sView.Equals("SnooperMAFFViewModel"))
            {
                this._decompressArchivesStats[archivePath].ChangeStateOfFirstViewModel(true);
            }
            else if (sView.Equals("SnooperMAFFViewVisualizationModel"))
            {
                this._decompressArchivesStats[archivePath].ChangeStateOfSecondViewModel(true);
            }
        }

        /// <summary>
        /// Unload the zip archive.
        /// </summary>
        /// <param name="oArchive">The unloaded archive.</param>
        /// <param name="sView">The string definition of current view.</param>
        public void UnLoadZipArchive(Archive oArchive, string sView)
        {
            var archivePath = oArchive.ExportDirectory + @"\" + oArchive.ArchiveName;
            if (!this._decompressArchivesStats.ContainsKey(archivePath)) { return; }
            if (sView.Equals("SnooperMAFFViewModel"))
            {
                this._decompressArchivesStats[archivePath].ChangeStateOfFirstViewModel(false);
            }
            else if (sView.Equals("SnooperMAFFViewVisualizationModel"))
            {
                this._decompressArchivesStats[archivePath].ChangeStateOfSecondViewModel(false);
            }

            if (!this._decompressArchivesStats[archivePath].GetViewModelLogic())
            {
                this._decompressArchivesStats.Remove(archivePath);
                var sPath = Environment.GetEnvironmentVariable("temp");
                foreach (var oPart in oArchive.ListOfArchiveParts)
                {
                    DeleteDirectoryTree(sPath + @"\MAFF\" + oPart.BaseFolder);
                }
            }
        }
    }
}
