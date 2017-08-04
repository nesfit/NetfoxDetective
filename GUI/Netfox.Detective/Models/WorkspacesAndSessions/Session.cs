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

namespace Netfox.Detective.Models.WorkspacesAndSessions
{
    /// <summary>
    /// Class represents application session.
    /// It consists from provider type, connection string, workspace and investigation name.
    ///// </summary>
    //[Serializable]
    //[XmlType(TypeName = "Session")]
    //public class Session : IEquatable<Session>
    //{
    //    public Session() { }
    //    public Session(Workspace workspace)
    //    {
    //        TimeStamp = DateTime.Now;
    //        Workspace = workspace;
    //    }

    //    public string PersistenceProviderName
    //    {
    //        get
    //        {
    //            if (WorkspacesManager.ProvidersNames.ContainsKey(PersistenceProvider))
    //                return WorkspacesManager.ProvidersNames[PersistenceProvider];

    //            return string.Empty;
    //        }
    //    }
    //    [XmlIgnore]
    //    public string PersistenceProvider { get { return Workspace == null? "" : model.PersistenceProvider; } }
    //    [XmlIgnore]
    //    public string ConnectionString { get { return Workspace == null ? "" :  model.ConnectionString; } }
    //    [XmlIgnore]
    //    public Workspace model { get; set; }
    //    public string InvestigationId { get; set; }
    //    public string InvestigationName { get; set; }
    //    public DateTime TimeStamp { get; set; }

    //    public bool Equals(Session other)
    //    {
    //        return this.InvestigationId == other.InvestigationId;
    //    }
    //}
}