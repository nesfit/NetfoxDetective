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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Netfox.Core.Database;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Snoopers.SnooperMAFF.Models.Common;
using Netfox.Snoopers.SnooperMAFF.Models.Containers;
using Netfox.Snoopers.SnooperMAFF.Models.Objects;

namespace Netfox.Snoopers.SnooperMAFF.Models.Archives
{
    /// <summary>
    /// Class desribes whole archive structure divined in archive parts
    /// Class implements construction of compressed file in Zip format
    /// </summary>
    public class Archive : IEntity
    {
        private FileStream _fsZipFileStream;
        private ZipArchive _zipArchive;
        private readonly ParentObject _oParentObject;
        private PersistableJsonSerializableGuid _ArhivePartsGuids;
        private readonly List<List<BaseObject>> _listOfTimeLevels = new List<List<BaseObject>>();

        public string ArchiveName { get; set; }
        public string ExportDirectory { get; set; }
        public DateTime ArchiveDateTime { get; set; }
        public IExportSource ArchiveExportSource { get; set; }


        public PersistableJsonSerializableGuid ArchivePartsGuids
        {
            get { return this._ArhivePartsGuids ?? new PersistableJsonSerializableGuid(this.ListOfArchiveParts.Select(f => f.Id)); }
            set { this._ArhivePartsGuids = value; }
        }

        [NotMapped]
        public virtual List<ArchivePart> ListOfArchiveParts { get; set; } = new List<ArchivePart>();


        #region Base Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Archive"/> class.
        /// </summary>
        protected Archive() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Archive"/> class.
        /// </summary>
        /// <param name="oParentObject">The parent object.</param>
        /// <param name="oExportDirectory">The export directory.</param>
        public Archive(ParentObject oParentObject, DirectoryInfo oExportDirectory)
        {
            
            this.ArchiveDateTime = oParentObject.TimeStamp;
            this.ArchiveExportSource = oParentObject.ExportSource;

            this._oParentObject = oParentObject;
            this.ExportDirectory = oExportDirectory.ToString();
        }

        /// <summary>
        /// Gets parent host referent from main part of archive.
        /// </summary>
        /// <returns>Return main part referent</returns>
        
        public string GetMainPartParentHostReferent() => this._oParentObject.GetObjectReferent(); 
        #endregion //Base Methods

        #region Process Methods
        /// <summary>
        /// Processes the archive parts.
        /// </summary>
        /// <param name="oContainer">The connection data container.</param>
        public void ProcessArchive(DataContainer oContainer)
        {
            this.GetArchiveObjects(oContainer);
            this.CreateAndFillArchiveParts();

            //Process ArchiveParts
            foreach (var oArchivePart in this.ListOfArchiveParts)
            {
                oArchivePart.ProcessArchivePart();
            }
        }

        /// <summary>
        /// Gets the archive objects from data container by heurestic methods.
        /// </summary>
        /// <param name="oContainer">The connection data container.</param>
        private void GetArchiveObjects(DataContainer oContainer)
        {
            var lActualPosition = 0;
            var listOfObjects = new List<BaseObject>();
            var setOfObjectsHashes = new HashSet<string>();

            //Parent Object
            listOfObjects.Add(this._oParentObject);

            //Check actual object
            while ((lActualPosition < listOfObjects.Count))
            {
                //get objects by their referent 
                var tempListOfObjects = oContainer.GetConnectionsByReferrers(listOfObjects.ElementAt(lActualPosition).GetObjectReferent());

                //Get objects by their references in object
                if (listOfObjects.ElementAt(lActualPosition) is TextObject)
                {
                    tempListOfObjects.AddRange(oContainer.GetConnectionByFileNamePath((TextObject)listOfObjects.ElementAt(lActualPosition)));
                }
                foreach (var oTempObject in tempListOfObjects)
                {
                    if (!setOfObjectsHashes.Contains(oTempObject.UniqueHash))
                    {
                        setOfObjectsHashes.Add(oTempObject.UniqueHash);
                        listOfObjects.Add(CloneGeneric.CloneObject(oTempObject));
                    }
                }

                lActualPosition++;
            }

            //SortObjects
            listOfObjects = listOfObjects.OrderBy(x => x.TimeStamp).ToList();

            //Set timeStopper
            var timeStopper = this._oParentObject.TimeStamp.AddMilliseconds(Constants.SnapshotsTimeSeparator);

            //Add current list
            this._listOfTimeLevels.Add(new List<BaseObject>());

            foreach (var oBaseObject in listOfObjects)
            {
                if ((oBaseObject.TimeStamp > timeStopper) && (oBaseObject is TextObject))
                {
                    this._listOfTimeLevels.Add(new List<BaseObject>());
                    timeStopper = oBaseObject.TimeStamp.AddMilliseconds(Constants.SnapshotsTimeSeparator);
                }
                this._listOfTimeLevels.Last().Add(oBaseObject);
            }
        }

        /// <summary>
        /// Creates and fill other archive part, by snapshot heurestic method.
        /// </summary>
        private void CreateAndFillArchiveParts()
        {
            //Create Root ArchivePart
            this.ListOfArchiveParts.Add(new ArchivePartRoot(this._oParentObject));

            //Fill Archive Root Part
            this.ListOfArchiveParts.Last().FillArchivePart(this._listOfTimeLevels.Select(CloneGeneric.CloneList).ToList());

            //Do we want generate Snaphosts
            if (!Constants.GenerateSnapshots) { return; }

            //Interate Through TimeLevelCollection
            for (var ulLoopCounter = 0; ulLoopCounter < this._listOfTimeLevels.Count; ++ulLoopCounter)
            {
                this.ListOfArchiveParts.Add(new ArchivePartRoot(this._oParentObject));
                //TODO this.ListOfArchiveParts.Add(new ArchivePartOther(this._oParentObject));

                var oTemplateList = new List<List<BaseObject>>();

                for (var oTimeLevelCounter = 0; oTimeLevelCounter <= ulLoopCounter; ++oTimeLevelCounter)
                {
                    oTemplateList.Add(CloneGeneric.CloneList(this._listOfTimeLevels[oTimeLevelCounter]));
                }

                this.ListOfArchiveParts.Last().FillArchivePart(oTemplateList);
            }
        }
        #endregion //Process Methods


        #region Finalize Methods
        /// <summary>
        /// Finalize all arhive parts and compres whole archive for export.
        /// </summary>
        public void ArchiveFinalize()
        {
            this.CompressInitialize();

            var iCurrentPartPosition = 0;
            foreach (var oArchivePart in this.ListOfArchiveParts)
            {
                oArchivePart.CompressArchivePart(this._zipArchive, this._oParentObject.TimeStamp, this.ExportDirectory, iCurrentPartPosition);
                iCurrentPartPosition++;
            }

            this.CompressFinalize();
        }
        /// <summary>
        /// Initialization of archive compressing and constructing compressed file.
        /// </summary>
        private void CompressInitialize()
        {
            this.ArchiveName = this._oParentObject.HostAddress + "_" + ComputeHash.GetMd5Hash(this._oParentObject.HostAddress + this._oParentObject.TimeStamp.Ticks) + ".maff";
            this._fsZipFileStream = new FileStream(this.ExportDirectory + @"\" + this.ArchiveName, FileMode.Create);
            this._zipArchive = new ZipArchive(this._fsZipFileStream, ZipArchiveMode.Update);
        }

        /// <summary>
        /// Finalization of archive compressing.
        /// </summary>
        private void CompressFinalize()
        {
            this._zipArchive.Dispose();
            this._fsZipFileStream.Dispose();
        }
        #endregion //Finalize Methods

        #region Implementation of IEntity
        [Key]
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime FirstSeen { get; }
        #endregion
    }
}
