using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics;

namespace AppIdentAccord
{
    public class FeatureSelection
    {
        public FeatureSelection(FeatureSelector featureSelector) { this.FeatureSelector = featureSelector; }
        public FeatureSelector FeatureSelector { get; }
        public double[,] GetCorrelationMatrix(AppIdentAcordSource appIdentAcordSource) => appIdentAcordSource.Samples2D.Correlation();

        public Type GetMostCorellatedFeature(double[,] correlationMatrix, out double correlation)
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

        public double[] GetColumn(double[,] correlationMatrix, int index)
        {
            var column = new double[correlationMatrix.GetLength(1)];
            for (var i = 0; i < correlationMatrix.GetLength(1); i++)
            {
                column[i]=correlationMatrix[index, i];
               
            }
            return column;
        }

        public double[] GetLine(double[,] correlationMatrix, int index)
        {
            var line = new double[correlationMatrix.GetLength(0)];
            for (var i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                line[i] = correlationMatrix[i, index];

            }
            return line;
        }

        public double[] TransformAbs(double[] array) => array.Select(Math.Abs).ToArray();

        public IEnumerable<double[,]> ProcessFeatureSelection(AppIdentAcordSource appIdentAcordSource, double trashold)
        {
            var iterationResults = new List<double[,]>();
            Type mostCorrelatedFeature = null;
            double correlation = 1;
            while (correlation > trashold)
            {
                if(mostCorrelatedFeature != null) appIdentAcordSource.FeatureSelector.RemoveFeature(mostCorrelatedFeature);
                var correlationMatrix = this.GetCorrelationMatrix(appIdentAcordSource);
                iterationResults.Add(correlationMatrix);
                mostCorrelatedFeature = this.GetMostCorellatedFeature(correlationMatrix, out correlation);
                //Console.WriteLine(correlation);
            }
            return iterationResults;
        }

        
    }
}