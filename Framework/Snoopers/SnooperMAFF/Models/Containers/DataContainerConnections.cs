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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Netfox.SnooperMAFF.Models.Objects;

namespace Netfox.SnooperMAFF.Models.Containers
{
    /// <summary>
    /// Class encapsulate MAFF objects from each captured conversation determined by own connection
    /// Class save information about client side of connection
    /// </summary>
    public class DataContainerConnections
    {
        private IPEndPoint _ipClientEndPoint;
        private readonly HashSet<string> _setOfReferrers = new HashSet<string>();
        private readonly List<BaseObject> _listOfBaseObjects = new List<BaseObject>();

        #region Initialization Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContainerConnections"/> class.
        /// </summary>
        public DataContainerConnections() { }

        /// <summary>
        /// Adds the object to current conversation.
        /// </summary>
        /// <param name="oBaseObject">The o base object.</param>
        public void AddObject(BaseObject oBaseObject)
        {
            if (!this._listOfBaseObjects.Any())
            {
                this._ipClientEndPoint = oBaseObject.ExportSource.SourceEndPoint;
            }
            this._listOfBaseObjects.Add(oBaseObject);
            this._setOfReferrers.Add(oBaseObject.Referrer);
        }

        #endregion

        #region Searching methods
        /// <summary>
        /// Gets the connection ip end point.
        /// </summary>
        /// <returns>Return IPEndPoint of Client </returns>
        public IPEndPoint GetConnectionIpEndPoint() { return this._ipClientEndPoint; }
       
        /// <summary>
        /// Checks the connection obtain object with specified referers by his referent.
        /// </summary>
        /// <param name="sReferent">The referent.</param>
        /// <returns>Return True if referrer was founded</returns>
        public bool CheckConnectionReferers(string sReferent) { return this._setOfReferrers.Contains(sReferent); }
        public List<BaseObject> GetBaseObjectsByReferrer(string sReferent)
        {
            return (from oBaseObject in this._listOfBaseObjects
                    where oBaseObject.Referrer.Contains(sReferent)
                    select oBaseObject).ToList();
        }

        /// <summary>
        /// Gets the base objects by links created by references in object content.
        /// </summary>
        /// <param name="oTextObject">The text object.</param>
        /// <returns>Return List of new object, which obtained current linked reference </returns>
        public List<BaseObject> GetBaseObjectsByLinks(TextObject oTextObject)
        {
            return this._listOfBaseObjects.Where(oBaseObject => oTextObject.CheckLink(oBaseObject.GetObjectFilenamePath())).ToList();
        }
        #endregion
    }
}
