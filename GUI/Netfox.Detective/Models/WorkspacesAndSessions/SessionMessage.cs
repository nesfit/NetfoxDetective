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
    /// Detective message class. Informs about creation or selection of application session.
    ///// </summary>
    //public class SessionMessage : DetectiveMessage
    //{
    //    public enum Type
    //    {
    //        Created,
    //        Selected
    //    }

    //    public Session Session { get; set; }
    //    public Type MessageType { get; set; }

    //    public SessionMessage()
    //    {
    //        Sender = "SessionMessage";
    //    }

    //    public static void SendSessionMessage(Session session, Type messageType)
    //    {
    //        var newSessionMessage = new SessionMessage()
    //        {
    //            Session = session,
    //            MessageType = messageType
    //        };

    //        AsyncSendMessage(newSessionMessage);
    //    }
    //}
}