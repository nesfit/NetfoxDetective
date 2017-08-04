// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperMAFF.ViewModels;

namespace Netfox.SnooperMAFF.View.Events
{
    public sealed class CurrentPartEvent
    {
        private static CurrentPartEvent _instance;

        private int _iCurrentPart;
        private int _iPartCount;

        private SnooperMAFFViewModel _oViewModel;
        private SnooperMAFFViewVisualizationModel _oViewVisualizationModel;

        private CurrentPartEvent() { }

        public static CurrentPartEvent GetInstance()
        {
            if (_instance != null) { return _instance; }
            _instance = new CurrentPartEvent { _iCurrentPart = 1 };
            return _instance;
        }

        public void Inicialize(DetectiveExportDetailPaneViewModelBase oPanelBase, int iPartCount)
        {
            this._iPartCount = iPartCount;
            this.SetExportView(oPanelBase);
            if ((this._oViewModel == null) || (this._oViewVisualizationModel == null)) { return; }
            this.DisplayCurrentPart();
        }

        private void SetExportView(DetectiveExportDetailPaneViewModelBase oPanelBase)
        {
            if (oPanelBase is SnooperMAFFViewModel)
            {
                if (this._oViewModel != null) { return; }
                this._oViewModel = oPanelBase as SnooperMAFFViewModel;
            }
            else if (oPanelBase is SnooperMAFFViewVisualizationModel)
            {
                if (this._oViewVisualizationModel != null) { return; }
                this._oViewVisualizationModel = oPanelBase as SnooperMAFFViewVisualizationModel;
            }
        }

        public void GetPreviousPart()
        {
            if (this._iCurrentPart <= 1) { return; }
            this._iCurrentPart--;
            this.DisplayCurrentPart();
        }

        public void GetNextPart()
        {
            if (this._iCurrentPart >= this._iPartCount) { return; }
            this._iCurrentPart++;
            this.DisplayCurrentPart();
        }


        private void DisplayCurrentPart()
        {
            //ViewVisualisationModel Update
            var oArchive = this._oViewVisualizationModel.ExportedArchive.Archive;
            if (oArchive.ListOfArchiveParts.Count <= 0 || oArchive.ListOfArchiveParts[this._iCurrentPart - 1].ExportListOfArchiveObjects.Count <= 0) { return; }
            var sPath = Environment.GetEnvironmentVariable("temp");
            this._oViewVisualizationModel.GetPathToArchive = sPath + @"\MAFF\" + oArchive.ListOfArchiveParts[this._iCurrentPart - 1].BaseFolder + @"\index.html";
            this._oViewVisualizationModel.CurrentPart = this._iCurrentPart;

            //ViewModel Update
            oArchive = this._oViewModel.ExportedArchive.Archive;
            this._oViewModel.ArchiveObjects = oArchive.ListOfArchiveParts[this._iCurrentPart - 1].ExportListOfArchiveObjects.ToArray(); ;
            this._oViewModel.CurrentPart = this._iCurrentPart;
        }
    }


}
