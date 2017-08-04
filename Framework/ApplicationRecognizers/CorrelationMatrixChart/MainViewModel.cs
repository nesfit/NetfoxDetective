// Copyright (c) 2017 Jan Pluskal, Haris Daniel
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

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Netfox.CorrelationMatrixChart
{
//    using System.Collections.Generic;
    public class MainViewModel
    {
        //public MainViewModel(string[] featureNameArray, double[][] correlationMatrixData)
        public MainViewModel()
        {
            
        }
            public MainViewModel(string[] featureNameArray, double[,] correlationMatrixData)
        {
            this.CorrelationMatrixHeatMap = this.CreateModel(featureNameArray, correlationMatrixData);

        }
        
        public PlotModel CorrelationMatrixHeatMap { get;
            set;
        }

        private  PlotModel CreateModel(string[] featureNameArray, double[,] correlationMatrixData)
        {
            var model = new PlotModel { Title = "Correlation Matrix of Features" };
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "BottomFeatureAxis",
                ItemsSource = featureNameArray,
                Angle = -45,
                //FontSize = 20
            });
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "LeftFeatureAxis",
                ItemsSource = featureNameArray,
                //FontSize = 20
            });
            model.Axes.Add(new LinearColorAxis
            {
                Minimum = -1,
                Maximum = 1,
                //Palette = OxyPalettes.BlueWhiteRed(200)
                Palette = OxyPalettes.BlueWhiteRed(200)
//                Palette = OxyPalettes.Hot(200)
            });
            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = featureNameArray.Length,
                Y0 = 0,
                Y1 = featureNameArray.Length,
                XAxisKey = "BottomFeatureAxis",
                YAxisKey = "LeftFeatureAxis",
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.3, // neccessary to display the 
                Data = correlationMatrixData
            };
            model.Series.Add(heatMapSeries);
            return model;
        }
    }
}