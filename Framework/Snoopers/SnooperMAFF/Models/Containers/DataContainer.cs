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
    /// Class encapsulate all client conversations contained in Server Dictionary container
    /// Object must be saved in helper container, becouse older object may be used in newer archive
    /// </summary>
    public class DataContainer
    {
        private readonly Dictionary<string, HashSet<IPEndPoint>> _dictionaryOfHostAddresses = new Dictionary<string, HashSet<IPEndPoint>>();
        private readonly Dictionary<IPEndPoint, DataContainerConnections> _dictionaryOfClientConnections = new Dictionary<IPEndPoint, DataContainerConnections>();

        #region Initialization Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="DataContainer"/> class.
        /// </summary>
        public DataContainer() { }

        /// <summary>
        /// Adds the object to one conversation.
        /// </summary>
        /// <param name="oBaseObject">The base object.</param>
        public void AddObject(BaseObject oBaseObject)
        {
            //Add Source EndPoint to Host Address Dictionary
            if (!(this._dictionaryOfHostAddresses.ContainsKey(oBaseObject.HostAddress)))
            {
                this._dictionaryOfHostAddresses.Add(oBaseObject.HostAddress, new HashSet<IPEndPoint>());
            }
            this._dictionaryOfHostAddresses[oBaseObject.HostAddress].Add(oBaseObject.ExportSource.SourceEndPoint);

            //Add BaseObject to Client Datacontainer
            if (!(this._dictionaryOfClientConnections.ContainsKey(oBaseObject.ExportSource.SourceEndPoint)))
            {
                this._dictionaryOfClientConnections.Add(oBaseObject.ExportSource.SourceEndPoint, new DataContainerConnections());
            }
            this._dictionaryOfClientConnections[oBaseObject.ExportSource.SourceEndPoint].AddObject(oBaseObject);

        }
        #endregion 

        #region Searching methods
        /// <summary>
        /// Checks the referrers was founded in some connection.
        /// </summary>
        /// <param name="sReferent">The referent.</param>
        /// <returns>Return true if referrer was founded</returns>
        public bool CheckReferrers(string sReferent)
        {
            return this._dictionaryOfClientConnections.Values.Any(oClients => oClients.CheckConnectionReferers(sReferent));
        }

        /// <summary>
        /// Gets the connection objects by referrers.
        /// </summary>
        /// <param name="sReferent">The referent.</param>
        /// <returns>Return list of objects</returns>
        public List<BaseObject> GetConnectionsByReferrers(string sReferent)
        {
            var oList = new List<BaseObject>();
            foreach (var oConnection in this._dictionaryOfClientConnections.Values.Where(oConnection => oConnection.CheckConnectionReferers(sReferent)))
            {
                oList.AddRange(oConnection.GetBaseObjectsByReferrer(sReferent));
            }
            return oList;
        }

        /// <summary>
        /// Gets the connection by file name path.
        /// </summary>
        /// <param name="oTextObject">The text object filled by references.</param>
        /// <returns>Return list of objects</returns>
        public List<BaseObject> GetConnectionByFileNamePath(TextObject oTextObject)
        {
            var oList = new List<BaseObject>();
            foreach (var oConnection in this._dictionaryOfClientConnections.Values)
            {
                oList.AddRange(oConnection.GetBaseObjectsByLinks(oTextObject));
            }
            return oList;
        }
        #endregion
    }
}
