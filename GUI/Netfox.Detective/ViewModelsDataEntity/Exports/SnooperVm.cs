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

using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    [NotifyPropertyChanged]
    public class SnooperVm : DetectiveDataEntityViewModelBase
    {
        public SnooperVm(WindsorContainer applicationWindsorContainer, ISnooper model) : base(applicationWindsorContainer, model)
        {
            this.Snooper = model;
            this.IsEnabled = false;
            this.IsAlreadyExported = false;
        }

        public ISnooper Snooper { get; }
        [SafeForDependencyAnalysis]
        public string Name => this.Snooper.Name;
        public bool IsEnabled { get; set; }
        public bool IsAlreadyExported { get; private set; }

        private void ReevaluateIsAlreadyExported(IConversationsVm converstionsVm)
        {
            if(converstionsVm == null) { return; }

            this.IsAlreadyExported = converstionsVm.UsedSnoopers.Contains(this);
        }

        #region Equality members
        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode() { return (this.Snooper != null? this.Name.GetHashCode() : 0); }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) { return false; }
            if(ReferenceEquals(this, obj)) { return true; }
            if(obj.GetType() != this.GetType()) { return false; }
            return this.Equals((SnooperVm) obj);
        }

        protected bool Equals(SnooperVm obj) { return Equals(this.Snooper, obj.Snooper); }
        #endregion
    }
}