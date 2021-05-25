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

using System.Collections.Generic;
using Numl;
using Numl.Model;
using Numl.Supervised.DecisionTree;
using Netfox.AppIdent.Models;

namespace Netfox.AppIdent.NUML.Classifiers
{
    public class DecisionTreeClassifier : ClassifierBase
    {
        public DecisionTreeClassifier(FeatureVector[] stats) : base(stats) { }

        public override void CreateClassifier()
        {
            // this.Normalizator = new Normalizator.Normalizator(this.Stats);
            // this.Stats = this.Normalizator.Normalize(this.Stats);
            var testSet = this.Stats;
            var descriptor = Descriptor.Create<FeatureVector>();
            var decisionTreeModel = new DecisionTreeGenerator(descriptor.VectorLength, 2, descriptor, null, 0.0);
            // decisionTreeModel.SetHint(false);
            decisionTreeModel.Generate((IEnumerable<object>) testSet);
            var learnDecisionTree = Learner.Learn(testSet, 0.90, 100, decisionTreeModel);
            this.ClassifierModel = learnDecisionTree.Model;
        }
    }
}