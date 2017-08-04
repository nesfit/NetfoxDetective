using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math.Optimization.Losses;
using Accord.Statistics;
using AppIdent.Models;
using Framework.Models;
using Accord.Math;
using Accord.Statistics.Analysis;
using AppIdent;

namespace AppIdentAccord
{
    public partial class AccordAppIdent
    {
        public AppIdentService AppIdentService { get; }

        public AccordAppIdent(AppIdentService appIdentService)
        {
            this.AppIdentService = appIdentService;
        }

        public RandomForest GetBestRandomForestsWithGridSearch(AppIdentAcordSource appIdentAcordSource, out GridSearchParameterCollection bestParameters, out double minError)
        {
            // grid search ranges (parameter values)
            GridSearchRange[] parameterRanges =
            {
                new GridSearchRange("trees", new double[]
                {
                    1,
                    3,
                    5,
                    8,
                    11
                }),
                new GridSearchRange("sampleRatio", new[]
                {
                    0.7,
                    0.8,
                    0.9
                }),
                new GridSearchRange("join", new double[]
                {
                    25,
                    50,
                    100,
                    150,
                    200
                })
            };

            var samples = appIdentAcordSource.Samples;
            var labels = appIdentAcordSource.LabelsAsIntegers;
            var decisionVariables = appIdentAcordSource.DecisionVariables;
            // instantiate grid search algorithm for a CLF model
            var gridSearch = new GridSearch<RandomForest>(parameterRanges)
            {
                Fitting = delegate(GridSearchParameterCollection parameters, out double error)
                {
                    Console.WriteLine($"{DateTime.Now} RandomForest grid search");
                    // Use the parameters to build the model
                    // Create a new learning algorithm
                    var rfcModel = CreateRandomForestModel(decisionVariables, parameters, samples, labels);
                    // Measure the model performance to return as an out parameter
                    error = new ZeroOneLoss(labels).Loss(rfcModel.Decide(samples));
                   // Return the current model
                    return rfcModel;
                }
                //,ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 }
            };
            // Compute the grid search to find the best RandomForest model
            return gridSearch.Compute(out bestParameters, out minError);
        }

        public CrossValidationResult<RandomForest> GetCrossValidationResultsOfRandomForestModel(AppIdentAcordSource appIdentAcordSource, GridSearchParameterCollection bestParameters)
        {
            var samples = appIdentAcordSource.Samples;
            var labels = appIdentAcordSource.LabelsAsIntegers;
            // Create a new Cross-validation algorithm passing the data set size and the number of folds
            var crossvalidation = new CrossValidation<RandomForest>(size: samples.Length, folds: 10)
            {
                Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
                {
                    // The fitting function is passing the indices of the original set which
                    // should be considered training data and the indices of the original set
                    // which should be considered validation data.

                    // Lets now grab the training data:
                    var trainingInputs = samples.Get(indicesTrain);
                    var trainingOutputs = labels.Get(indicesTrain);
                    // And now the validation data:
                    var validationInputs = samples.Get(indicesValidation);
                    var validationOutputs = labels.Get(indicesValidation);
                    // create random forest model with the best parameters from grid search results
                    var rfcModel = CreateRandomForestModel(appIdentAcordSource.DecisionVariables, bestParameters, trainingInputs, trainingOutputs);
                    // compute the training error rate with ZeroOneLoss function
                    var trainingError = new ZeroOneLoss(trainingOutputs).Loss(rfcModel.Decide(trainingInputs));
                    // Now we can compute the validation error on the validation data:
                    var validationError = new ZeroOneLoss(validationOutputs).Loss(rfcModel.Decide(validationInputs));
                    // Return a new information structure containing the model and the errors achieved.

                    var tag = new ValidationDataSource(validationInputs, validationOutputs);
                    return new CrossValidationValues<RandomForest>(rfcModel, trainingError, validationError){Tag = tag };
                }
            };
            // Compute the cross-validation
            return crossvalidation.Compute();
        }

        private static RandomForest CreateRandomForestModel(
            DecisionVariable[] decisionVariables,
            GridSearchParameterCollection bestParameters,
            double[][] trainingInputs,
            int[] trainingOutputs)
        {
            var teacher = new RandomForestLearning(decisionVariables)
            {
                NumberOfTrees = (int) bestParameters["trees"].Value,
                SampleRatio = bestParameters["sampleRatio"].Value,
                Join = (int) bestParameters["join"].Value
            };
            // Create a training algorithm and learn the training data
            var rfcModel = teacher.Learn(trainingInputs, trainingOutputs);
            return rfcModel;
        }
    }
}