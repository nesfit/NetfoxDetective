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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using CommandLine;
using Netfox.AppIdent;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Misc;
using Netfox.AppIdent.Models;

namespace Netfox.AppIdentCli
{
    class Program
    {
        private static void DebuggerCheckExit()
        {
            if(Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadLine();
            }
        }

        private static void Epi(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            Console.WriteLine($"{DateTime.Now} Running classification");
            var classificationStatisticsMeter = appIdentService.EpiClasify(appIdentDataSource, featureSelector, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
        }

        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();

            Options options = null;
                DebuggerCheckExit();
            Parser.Default.ParseArguments(args)
                  .WithParsed(o => options = o as Options)
                  .WithNotParsed(errors =>DebuggerCheckExit());

            //options.TrainingToVerificationRation = 0.7;
            //options.FeatureSelectionTrashold = 0.7;
            //options.IsEpi = true;
            //options.IsUseFullName = false;
            //options.MinFlows = 17;

            if(!options.IsEpi && !options.IsRandomForest && !options.IsBayesian)
            {
                Console.WriteLine("No classification method selected.");
                DebuggerCheckExit();
                return;
            }

            var context = new AppIdentTestContext("cli", DateTime.Now)
            {
                MinFlows = options.MinFlows,
                TrainingToVerificationRation = options.TrainingToVerificationRation,
                FeatureSelectionTreshold = options.FeatureSelectionTrashold,
                CrossValidationFolds = options.CrossValidationFolds,
                IsEpi = options.IsEpi,
                IsRandomForest = options.IsRandomForest,
                IsBayesian = options.IsBayesian,
                IsUseFullName = options.IsUseFullName,
                BestParametersFilePath = options.BestParametersFilePath,
            };
            context.ChangeNameByParameters();
            context.Save();

            SetLabelType(options);

            Console.WriteLine($"{DateTime.Now} Loading: {options.AppIdentDataSource}");
            var appIdentDataSource = context.LoadAppIdentDataSource(options.AppIdentDataSource);

            Console.WriteLine($"{DateTime.Now} Repartitioning ratio {context.TrainingToVerificationRation} with min flows {context.MinFlows}");
            appIdentDataSource.RepartitionFeatureVectorsTestingAndVerificationDatasets(context.TrainingToVerificationRation, context.MinFlows);
            context.SavePartitioning(appIdentDataSource);
            context.AppIdentDataSource = appIdentDataSource;

            if(!appIdentDataSource.TrainingSet.Any() || !appIdentDataSource.VerificationSet.Any())
            {
                Console.WriteLine("No testing or verification data present after partitioning and filtering by minflow.");
                DebuggerCheckExit();
                return;
            }

            var appIdentService = new AppIdentService();

            if(options.IsRandomForest) { RandomForest(appIdentService, appIdentDataSource, context); }
            if(options.IsEpi) { Epi(appIdentService, appIdentDataSource, context); }
            if(options.IsBayesian) { Bayesian(appIdentService, appIdentDataSource, context); }

            watch.Stop();
            context.RunningTime = watch.Elapsed;
            context.Save();

            
            Console.WriteLine($"{DateTime.Now} Running time: {watch.Elapsed}");

            DebuggerCheckExit();


        }

        private static void Bayesian(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            Console.WriteLine($"{DateTime.Now} Running classification.");
            var classificationStatisticsMeter = appIdentService.BayesianClassify(appIdentDataSource, context.TrainingToVerificationRation, 0.99, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
            context.Save();
        }

        private static void RandomForest(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            GridSearchParameterCollection bestParameters;
            if(!File.Exists(context.BestParametersFilePath))
            {
                Console.WriteLine($"{DateTime.Now} Looking for best parameters.");
                bestParameters = appIdentService.RandomForestGetBestParameters(appIdentDataSource, featureSelector, context);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} Loading best parameters.");
                context.Load<GridSearchParameterCollection>(context.BestParametersFilePath, out bestParameters);
            }

            Console.WriteLine($"{DateTime.Now} Running cross validation.");
            var classificationStatisticsMeter =
                appIdentService.RandomForestCrossValidation(appIdentDataSource, context.FeatureSelector, bestParameters, context.CrossValidationFolds, context);

            Console.WriteLine($"{DateTime.Now} Cross validation results:");
            classificationStatisticsMeter.PrintResults();

            var model = context.Model;

            Console.WriteLine($"{DateTime.Now} Running classification");
            appIdentService.AccordClassify(appIdentDataSource, model, context.FeatureSelector, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
            context.Save();
        }

        private static void SetLabelType(Options options)
        {
            LabelSelector.LabelType = options.IsUseFullName? LabelSelector.ELabelType.ApplicationProtocolNameFull : LabelSelector.ELabelType.ApplicationProtocolName;
        }
    }
}