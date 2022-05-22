// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    /// <summary>
    /// This is interface of Spotter Visitors of Visitor design pattern.
    /// Spotter Visitors extends functionality of spotters.
    /// </summary>
    public interface ISpotterVisitor
    {
        object applyOn(SpotterJson spotter);

        object applyOn(SpotterKeyValue spotter);

        object applyOn(SpotterMultipart spotter);

        object applyOn(SpotterText spotter);

        object applyOn(SpotterFRPC spotter);
    }
}