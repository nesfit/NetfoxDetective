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
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Text;
using Netfox.Core.Database;
using Netfox.SnooperMAFF.Models.Common;
using Netfox.SnooperMAFF.Models.Objects;

namespace Netfox.SnooperMAFF.Models.Archives
{
    /// <summary>
    /// Base class desribing each archive part common methods.
    /// </summary>
    //[ComplexType]
    public abstract class ArchivePart : IEntity
    {
        public string BaseFolder { get; set; }
        [NotMapped]
        public List<BaseObject> ExportListOfArchiveObjects { get; } = new List<BaseObject>();
        [NotMapped]
        protected ParentObject ParentObject;
        [NotMapped]
        protected List<List<BaseObject>> ListOfTimeLevels;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivePart"/> class.
        /// </summary>
        /// <param name="oParentObject">The parent object.</param>
        protected ArchivePart(ParentObject oParentObject)
        {
            //Store ParentObject
            this.ParentObject = oParentObject;
        }

        /// <summary>
        /// Fills the archive part by objects from list of time levelss.
        /// </summary>
        /// <param name="oListOfTimeLevels">The o list of time levels.</param>
        public void FillArchivePart(List<List<BaseObject>> oListOfTimeLevels)
        {
            this.ListOfTimeLevels = oListOfTimeLevels;
        }

        /// <summary>
        /// Abstract class for derived class extensions.
        /// </summary>
        public abstract void ProcessArchivePart();

        /// <summary>
        /// Compresses each archive part and construct index.rdf file with webpage base metadata.
        /// </summary>
        /// <param name="oZipArchive">The parent zip archive.</param>
        /// <param name="oTimeStamp">The time stamp of parent archive.</param>
        /// <param name="sExportDirectory">The export directory.</param>
        /// <param name="iPartPosition">The position of archive part.</param>
        public void CompressArchivePart(ZipArchive oZipArchive, DateTime oTimeStamp, string sExportDirectory, int iPartPosition)
        {
            // INDEX RDF
            var sRdfData = "<?xml version=\"1.0\"?>\n" +
                            "       <RDF:RDF xmlns:MAF=\"http://maf.mozdev.org/metadata/rdf#\"\n" +
                            "       xmlns:NC=\"http://netfox.fit.vutbr.cz/About.en.cshtml\"\n" +
                            "       xmlns:RDF=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n" +
                            "  <RDF:Description RDF:about=\"urn:root\">\n" +
                            "    <MAF:originalurl RDF:resource=\"" + "http://" + this.ParentObject.HostAddress + "/\"/>\n" +
                            "    <MAF:title RDF:resource=\"" + this.ParentObject.GetTitle() + "\"/>\n" +
                            "    <MAF:archivetime RDF:resource=\"" + DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local).ToString("dd.MM.yyyy hh: mm:ss tt") + "\"/>\n" +
                            "    <MAF:indexfilename RDF:resource=\"index.html\"/>\n" +
                            "    <MAF:charset RDF:resource=\"" + this.ParentObject.GetCharSet() + "\"/>\n" +
                            "  </RDF:Description>\n" +
                            "</RDF:RDF>";
            // END INDEX RDF

            //Do zip comprimation
            this.BaseFolder = ComputeHash.GetMd5Hash(oTimeStamp.Ticks + this.ParentObject.HostAddress + this.ParentObject.GetCharSet() + this.ExportListOfArchiveObjects.Count + iPartPosition);

            //RDF WRITE
            var rdfEntry = oZipArchive.CreateEntry(this.BaseFolder + "/" + "index.rdf");
            var rdfEntryWriteStream = new BinaryWriter(rdfEntry.Open());
            rdfEntryWriteStream.Write(Encoding.GetEncoding(437).GetBytes(sRdfData));
            rdfEntryWriteStream.Dispose();
            //END RDF WRITE

            foreach (var oItemToArchive in this.ExportListOfArchiveObjects)
            {
                if (!(oItemToArchive is ParentObject))
                {
                    oItemToArchive.FileName = "index_files/" + oItemToArchive.FileName;
                    if (oItemToArchive is TextObject)
                    {
                        ((TextObject)oItemToArchive).ProcessReferences();
                    }
                }
                else
                {
                    ((ParentObject)oItemToArchive).ProcessReferences();
                }

                oItemToArchive.SaveObjectContent(oZipArchive, sExportDirectory, this.BaseFolder);
            }
        }

        #region Implementation of IEntity
        [ForeignKey("id")]
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime FirstSeen { get; }
        #endregion
    }
}
