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

using System;
using System.Collections.Generic;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    public class ChatMessageComparer : IEqualityComparer<IChatMessage>
    {
        #region Implementation of IEqualityComparer<in IChatMessage>
        public bool Equals(IChatMessage x, IChatMessage y)
        {
            if(x == null || y == null) { return false; }
            if(x.Sender == null || x.Receiver == null || y.Sender == null || y.Receiver == null) { return false; }
            if((x.Sender == y.Receiver && x.Receiver == y.Sender) || (x.Sender == y.Sender && x.Receiver == y.Receiver)) { return true; }
            return false;
        }

        public int GetHashCode(IChatMessage obj)
        {
            var h1 = 0;
            var h2 = 0;
            try { h1 = obj?.Receiver.GetHashCode() ?? 0; }
            catch(Exception)
            {
                // ignored
            }
            try { h2 = obj?.Sender.GetHashCode() ?? 0; }
            catch(Exception)
            {
                // ignored
            }
            return h1^h2;
        }
        #endregion
    }
}