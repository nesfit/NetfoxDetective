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
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics;

namespace Netfox.AppIdent.Accord
{
    public class FeatureSelection
    {
        private FeatureSelector FeatureSelector { get; set; }
        private double[,] GetCorrelationMatrix(AppIdentAcordSource appIdentAcordSource) => appIdentAcordSource.Samples2D.Correlation();

        private Type GetMostCorellatedFeature(double[,] correlationMatrix, out double correlation)
        {
            var lineMaxs = new List<double>();
            for(var i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                double maxLine = 0;
                for(var j = 0; j < correlationMatrix.GetLength(1); j++)
                {
                    var abs = Math.Abs(correlationMatrix[i, j]);
                    if(i == j) continue;
                    maxLine = abs > maxLine? abs : maxLine;
                }
                lineMaxs.Add(maxLine);
            }
            correlation = lineMaxs.Max();
            var lineIndex = lineMaxs.IndexOf(correlation);
            var columnIndex = lineMaxs.LastIndexOf(correlation);

            var line = this.TransformAbs(this.GetLine(correlationMatrix, lineIndex));
            var column = this.TransformAbs(this.GetColumn(correlationMatrix, columnIndex));

            var lineMean = line.Mean();
            var columnMean = column.Mean();
            return lineMean > columnMean? this.FeatureSelector.SelectedFeatures[lineIndex] : this.FeatureSelector.SelectedFeatures[columnIndex];
        }

        private double[] GetColumn(double[,] correlationMatrix, int index)
        {
            var column = new double[correlationMatrix.GetLength(1)];
            for (var i = 0; i < correlationMatrix.GetLength(1); i++)
            {
                column[i]=correlationMatrix[index, i];
               
            }
            return column;
        }

        private double[] GetLine(double[,] correlationMatrix, int index)
        {
            var line = new double[correlationMatrix.GetLength(0)];
            for (var i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                line[i] = correlationMatrix[i, index];

            }
            return line;
        }

        private double[] TransformAbs(double[] array) => array.Select(Math.Abs).ToArray();

        public IEnumerable<double[,]> ProcessFeatureSelection(AppIdentAcordSource appIdentAcordSource, double trashold)
        {
            this.FeatureSelector = appIdentAcordSource.FeatureSelector;
            this.RemoveFeaturesWithConstantValue(appIdentAcordSource);

            var iterationResults = new List<double[,]>();
            Type mostCorrelatedFeature = null;
            double correlation = 1;
            while (correlation > trashold)
            {
                if(mostCorrelatedFeature != null) appIdentAcordSource.FeatureSelector.RemoveFeature(mostCorrelatedFeature);
                var correlationMatrix = this.GetCorrelationMatrix(appIdentAcordSource);
                iterationResults.Add(correlationMatrix);
                mostCorrelatedFeature = this.GetMostCorellatedFeature(correlationMatrix, out correlation);
            }
            this.FeatureSelector = null;
            return iterationResults;
        }

        private void RemoveFeaturesWithConstantValue(AppIdentAcordSource appIdentAcordSource)
        {
            var matrix = appIdentAcordSource.Samples2D;
            var sampleCount = matrix.GetLength(0);
            var featureCount = matrix.GetLength(1);
            var removedFeatures = 0;
            for(var featureIndex = 0; featureIndex < featureCount; featureIndex++)
            {
                var isConstant = true;
                var firstValue = matrix[0, featureIndex];
                for(var sampleIndex = 1; sampleIndex < sampleCount; sampleIndex++)
                {
                    if(!(Math.Abs(firstValue - matrix[sampleIndex, featureIndex]) > 0)) continue;
                    isConstant = false;
                    break;
                }
                if(isConstant)
                {
                    var feature = this.FeatureSelector.SelectedFeatures[featureIndex - removedFeatures];
                    removedFeatures++;
                    appIdentAcordSource.FeatureSelector.RemoveFeature(feature);
                }
            }
        }
    }
}