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
using System.IO;
using System.Text;
using Accord.IO;
using Accord.MachineLearning;
using Netfox.AppIdent.Accord;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Statistics;
using Newtonsoft.Json;

namespace Netfox.AppIdent.Misc
{
    public class AppIdentTestContext
    {
        private readonly string _subFolder = @"Netfox\AppIdent";
        private readonly string _timeFormat = "yyyy-MM-dd-hh-mm-ss-tt";
        private readonly string _userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public AppIdentTestContext(string testName, DateTime? nowDateTime = null) : this(nowDateTime)
        {
            this.TestName = testName;
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
        }

        public AppIdentTestContext(DateTime? nowDateTime = null) { this.DateTime = nowDateTime ?? DateTime.Now; }
        public TimeSpan RunningTime { get; set; }
        public string TestName { get; private set; }
        public DateTime DateTime { get; }
        public string DateTimeString => this.DateTime.ToString(this._timeFormat);

        public string[] Labels { get; set; }

        [JsonIgnore]
        public MulticlassClassifierBase Model { get; set; }

        [JsonIgnore]

        public FeatureSelector FeatureSelector { get; set; }

        [JsonIgnore]

        public GridSearchParameterCollection BestParameters { get; set; }

        [JsonIgnore]
        public AppIdentPcapSource AppIdentPcapSource { get; set; }

        public int MinFlows { get; set; }
        public double FeatureSelectionTreshold { get; set; }
        public int CrossValidationFolds { get; set; }
        public double TrainingToVerificationRation { get; set; }

        [JsonIgnore]
        public AppIdentDataSource AppIdentDataSource { get; set; }

        public bool IsEpi { get; set; }
        public bool IsRandomForest { get; set; }
        public bool IsUseFullName { get; set; }

        public bool IsBayesian { get; set; }

        private DirectoryInfo StoreDirectoryInfo
        {
            get
            {
                var path = Path.Combine(this._userProfile, this._subFolder);
                var storeDirectoryInfo = new DirectoryInfo(path);
                if(!storeDirectoryInfo.Exists) storeDirectoryInfo.Create();
                return storeDirectoryInfo;
            }
        }

        private string LabelsFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_labels.bin");
        private string FeatureSelectorFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_featureSelector.bin");
        private string FeatureSelectorTxtFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_featureSelector.txt");
        private string _bestParametersFilePath;
        public string BestParametersFilePath
        {
            get => this._bestParametersFilePath ?? (this._bestParametersFilePath = Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_bestParameters.bin"));
            set { this._bestParametersFilePath = value; }
        }

        private string BestParametersTxtFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_bestParameters.txt");
        private string AppIdentPcapSourceFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_pcapSource.bin");
        private string AppIdentPcapSourceTxtFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_pcapSource.txt");
        private string AppIdentDataSourceFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_appIdentDataSource.bin");
        private string AppIdentDataSourcePartitioningFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_partitioning.csv");
        private string PrecisionMeasureFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_precMeasure.csv");
        private string PrecisionMeasureTxtFilePath => Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_precMeasure.txt");

        private string PrecisionMeasureCrossValidationFilePath => Path.Combine(this._userProfile, this._subFolder,
            $"{this.DateTimeString}_{this.TestName}_crossValidation_precMeasure.csv");

        private string PrecisionMeasureCrossValidationTxtFilePath => Path.Combine(this._userProfile, this._subFolder,
            $"{this.DateTimeString}_{this.TestName}_crossValidation_precMeasure.txt");

        public void ChangeNameByParameters()
        {
            var sb = new StringBuilder();
            if(this.IsEpi) sb.Append("EPI_");
            if(this.IsRandomForest) sb.Append("RandomForest_");
            if(this.IsBayesian) sb.Append("Bayesian_");
            if(this.IsUseFullName) sb.Append("FullName_");
            if(!this.IsUseFullName) sb.Append("PortBased_");
            sb.Append($"{this.TrainingToVerificationRation}_");
            sb.Append($"{this.FeatureSelectionTreshold}_");
            sb.Append($"{this.CrossValidationFolds}_");
            sb.Append($"{this.MinFlows}");
            this.TestName = sb.ToString();
        }

        public void Load<T>(string modelFilePath, out T obj)
        {
            obj = Serializer.Load<T>(modelFilePath);
        }
        public void Load<T>(string modelFilePath, out T model, out string[] labels)
        {
            model = Serializer.Load<T>(modelFilePath);
            labels = Serializer.Load<string[]>(this.LabelsFilePath);
        }

        public void Load<T>(string modelFilePath, out T model, out string[] labels, out FeatureSelector featureSelector)
        {
            model = Serializer.Load<T>(modelFilePath);
            labels = Serializer.Load<string[]>(this.LabelsFilePath);
            featureSelector = Serializer.Load<FeatureSelector>(this.FeatureSelectorFilePath);
        }

        public AppIdentDataSource LoadAppIdentDataSource(string filePath = null) { return this.ObjectLoad<AppIdentDataSource>(filePath ?? this.AppIdentDataSourceFilePath); }

        public void Save() { this.ObjectDump(this, this.TestNameFilePath(this.TestName)); }

        public string Save(MulticlassClassifierBase model, string[] labels)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();

            var randomForestFilePath = this.GetModelFilePath(model);
            model.Save(randomForestFilePath);
            this.Model = model;

            labels.Save(this.LabelsFilePath);
            this.Labels = labels;
            return randomForestFilePath;
        }

        public void Save(ApplicationProtocolClassificationStatisticsMeter precMeasure)
        {
            precMeasure.SaveToCsv(this.PrecisionMeasureFilePath);
            File.AppendAllText(this.PrecisionMeasureTxtFilePath, precMeasure.ToString());
        }

        public void Save(AppIdentPcapSource pcapSource)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
            pcapSource.Save(this.AppIdentPcapSourceFilePath);
            this.AppIdentPcapSource = pcapSource;
            this.ObjectDump(pcapSource, this.AppIdentPcapSourceTxtFilePath);
        }

        public void Save(AppIdentDataSource appIdentDataSource)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
            this.AppIdentDataSource = appIdentDataSource;
            this.ObjectDump(appIdentDataSource, this.AppIdentDataSourceFilePath);
            appIdentDataSource.SaveToCsv(this.AppIdentDataSourcePartitioningFilePath);
        }

        public void SaveCrossValidation(ApplicationProtocolClassificationStatisticsMeter precMeasure)
        {
            precMeasure.SaveToCsv(this.PrecisionMeasureCrossValidationFilePath);
            File.AppendAllText(this.PrecisionMeasureCrossValidationTxtFilePath, precMeasure.ToString());
        }

        public void SavePartitioning(AppIdentDataSource appIdentDataSource)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
            appIdentDataSource.SaveToCsv(this.AppIdentDataSourcePartitioningFilePath);
        }

        internal void Save(FeatureSelector featureSelector)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
            featureSelector.Save(this.FeatureSelectorFilePath);
            this.FeatureSelector = featureSelector;
            this.ObjectDump(featureSelector, this.FeatureSelectorTxtFilePath);
        }

        internal void Save(GridSearchParameterCollection bestParameters)
        {
            if(!this.StoreDirectoryInfo.Exists) this.StoreDirectoryInfo.Create();
            bestParameters.Save(this.BestParametersFilePath);
            this.BestParameters = bestParameters;
            this.ObjectDump(bestParameters, this.BestParametersTxtFilePath);
        }

        private string GetModelFilePath(MulticlassClassifierBase model)
        {
            return Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{this.TestName}_{model.GetType().Name}.bin");
        }

        private void ObjectDump(object obj, string filePath)
        {
            using(var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, obj);
            }
        }

        private T ObjectLoad<T>(string filePath)
        {
            using(var file = File.OpenText(filePath))
            {
                var serializer = new JsonSerializer();
                return (T) serializer.Deserialize(file, typeof(T));
            }
        }

        private string TestNameFilePath(string testName) { return Path.Combine(this._userProfile, this._subFolder, $"{this.DateTimeString}_{testName}.txt"); }
    }
}