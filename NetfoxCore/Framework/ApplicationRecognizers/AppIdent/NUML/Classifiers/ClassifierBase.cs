// Copyright (c) 2017 Jan Pluskal
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

using Numl.Supervised;
using Netfox.AppIdent.Models;
using Netfox.AppIdent.Normalization;

namespace Netfox.AppIdent.NUML.Classifiers
{
    public abstract class ClassifierBase
    {
        protected FeatureVector[] Stats;

        protected ClassifierBase(FeatureVector[] stats)
        {
            this.Stats = stats;
            this.Normalizator = new Normalizator();
            this.Normalizator.Initialize(this.Stats);
            this.Normalizator.Normalize(this.Stats);
            this.CreateClassifier();
        }

        public Normalizator Normalizator { get; }

        public IModel ClassifierModel { get; internal set; }

        public abstract void CreateClassifier();
    }
}