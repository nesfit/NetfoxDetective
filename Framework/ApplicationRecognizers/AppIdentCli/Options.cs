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

using CommandLine;
using CommandLine.Text;

namespace Netfox.AppIdentCli
{
    public class Options
    {
        [Option('d', "data-source", Required = true, HelpText = "AppIdentDataSource serialized data source file path.")]
        public string AppIdentDataSource { get; set; }

        [Option('f', "random-forest", HelpText = "Use random forest classification.")]
        public bool IsRandomForest { get; set; }
        [Option('p', "best-parameters", HelpText = "BestParameters bin file.")]
        public string BestParametersFilePath { get; set; }
        [Option('b', "bayesian", HelpText = "Use bayesian classification.")]
        public bool IsBayesian { get; set; }

        [Option('e', "epi", HelpText = "Use EPI classification.")]
        public bool IsEpi { get; set; }

        [Option('r', "ratio", HelpText = "Trainnig to verification ratio.")]
        public double TrainingToVerificationRation { get; set; } = 0.7;
        [Option('m', "min-flows", HelpText = "Minimum flows for training and classification.")]
        public int MinFlows { get; set; } = 30;

        [Option('s', "feature-selection", HelpText = "Feature corelation trashold <0-1>.")]
        public double FeatureSelectionTrashold { get; set; } = 2;
        [Option('c', "cross-validation-folds", HelpText = "Cross validation folds (only RF)")]
        public int CrossValidationFolds { get; set; } = 2;
        [Option('n', "use-full-name", HelpText = "Use application protocol full name including application name.")]
        public bool IsUseFullName { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
