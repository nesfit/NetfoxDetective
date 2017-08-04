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
using System.Linq;
using Netfox.SnooperMAFF.Models.Common;
using Netfox.SnooperMAFF.Models.Objects;

namespace Netfox.SnooperMAFF.Models.Archives
{
    /// <summary>
    /// Derived class from ArchivePart class desribing root archive part.
    /// This part is created by adding objects from connection data container
    /// Creates list of time levels for other archive parts
    /// </summary>
    /// <seealso cref="Netfox.SnooperMAFF.Models.Archives.ArchivePart" />
    [ComplexType]
    public class ArchivePartRoot : ArchivePart
    {
        private readonly List<TextObject> _listOfTextObjects;
        private readonly List<BaseObject> _listOfBaseObjects;

        private readonly List<Tuple<string, string>> _listOfReferences;
        private readonly Dictionary<string, Dictionary<string, DateTime>> _treeOfReferenceNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivePartRoot"/> class.
        /// </summary>
        /// <param name="oParentObject">The parent object.</param>
        public ArchivePartRoot(ParentObject oParentObject) : base(oParentObject)
        {
            this._listOfTextObjects = new List<TextObject>();
            this._listOfBaseObjects = new List<BaseObject>();
            this._listOfReferences = new List<Tuple<string, string>>();
            this._treeOfReferenceNames = new Dictionary<string, Dictionary<string, DateTime>>();
        }

        /// <summary>
        /// Overriden class from base ArchivePart class for implementing part extension logic.
        /// </summary>
        public override void ProcessArchivePart()
        {

            foreach (var oTimeLevel in this.ListOfTimeLevels)
            {
                foreach (var oBaseObject in oTimeLevel)
                {
                    //Check if object filename is in export list of export filenames
                    if (this._treeOfReferenceNames.ContainsKey(oBaseObject.FileName))
                    {
                        //If object with this filename is exist (same path to file) and will be updated
                        //Examples: result/styly.css -> styly.css // result/styly.css -> styly.css
                        if (this._treeOfReferenceNames[oBaseObject.FileName].ContainsKey(oBaseObject.PathToFileName))
                        {
                            //Update object content with new one
                            if (Constants.ObjectRewrite)
                            {
                                foreach (var oTextObject in this._listOfTextObjects.Where(oTextObject => oTextObject.PathToFileName.Contains(oBaseObject.PathToFileName)))
                                {
                                    oTextObject.RewriteContent(oBaseObject.GetContent());
                                }

                                foreach (var oNewBaseObject in this._listOfBaseObjects.Where(oNewBaseObject => oNewBaseObject.PathToFileName.Contains(oBaseObject.PathToFileName)))
                                {
                                    oNewBaseObject.RewriteContent(oBaseObject.GetContent());
                                }
                            }
                            //Get next Object
                            continue;
                        }

                        //Add next file path to file. It means next new object will be added (different path to filename)
                        //Examples: etag/styly.css -> styly.css // evalue/styly.css -> 1styly.css
                        this._treeOfReferenceNames[oBaseObject.FileName].Add(oBaseObject.PathToFileName, oBaseObject.TimeStamp);

                        //Update object's filename with number tag (number represents cout in list of unique path to file
                        oBaseObject.FileName = this._treeOfReferenceNames[oBaseObject.FileName].Count + oBaseObject.FileName;
                    }
                    //else if not, add new object by filename, path to file and timestamp
                    else
                    {
                        this._treeOfReferenceNames.Add(oBaseObject.FileName, new Dictionary<string, DateTime>());
                        this._treeOfReferenceNames[oBaseObject.FileName].Add(oBaseObject.PathToFileName, oBaseObject.TimeStamp);
                    }

                    //Add Links by object's reference
                    //this.AddLinkToObject(oBaseObject);

                    //Add object to specific container
                    this.AddObjectToSpecificContainer(oBaseObject);
                }
            }

            //Fill links 
            foreach (var oTextObject in this._listOfTextObjects)
            {
                foreach (var oReference in this._listOfReferences)
                {
                    oTextObject.AddLink(oReference.Item1, oReference.Item2);
                }
            }

            //Export objects to Export List
            this.ExportListOfArchiveObjects.AddRange(this._listOfTextObjects);
            this.ExportListOfArchiveObjects.AddRange(this._listOfBaseObjects);
        }

        /// <summary>
        /// Adds the object to specific container represented by list of text or base objects.
        /// </summary>
        /// <param name="oBaseObject">The o base object.</param>
        private void AddObjectToSpecificContainer(BaseObject oBaseObject)
        {
            //Add reference and new reference to repace old one (link)
            if (!(oBaseObject is ParentObject))
            {
                this._listOfReferences.Add(new Tuple<string, string>(oBaseObject.PathToFileName, oBaseObject.FileName));
            }

            if (oBaseObject is TextObject)
            {
                this._listOfTextObjects.Add((TextObject)oBaseObject);
            }
            else
            {
                this._listOfBaseObjects.Add(oBaseObject);
            }
        }
    }
}
