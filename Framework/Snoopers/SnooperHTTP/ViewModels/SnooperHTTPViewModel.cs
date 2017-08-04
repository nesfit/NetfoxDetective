// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Vit Janecek
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
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperHTTP.Interfaces;
using Netfox.SnooperHTTP.Models;
using PostSharp.Patterns.Model;

namespace Netfox.SnooperHTTP.ViewModels
{
    public class SnooperHTTPViewModel : DetectiveExportDetailPaneViewModelBase
    {

        public IEnumerable<SnooperExportedDataObjectHTTP> HTTPMsgs { get; set; }

        public SnooperExportedDataObjectHTTP SelectedMsg
        {
            get { return this.ExportVm.SelectedSnooperExportObject as SnooperExportedDataObjectHTTP; }
            set
            {
                this.ExportVm.SelectedSnooperExportObject = value;
                this.OnPropertyChanged(nameof(this.HTTPHeader));
                this.OnPropertyChanged(nameof(this.HTTPContent));
            }
        }
        [SafeForDependencyAnalysis]
        public string HTTPHeader
        {
            get
            {
                if(this.SelectedMsg == null) return string.Empty;
                return this.SelectedMsg.Message.HTTPHeader.ToString();
            }
        }
        [SafeForDependencyAnalysis]
        public string HTTPContent
        {
            get
            {
                if(this.SelectedMsg?.Message.HTTPContent == null) return string.Empty;
                return this.SelectedMsg.Message.HTTPContent.ToString();
            }
        }

        public override string HeaderText => "Snooper http detail";

        //public SnooperHTTPViewModel(WindsorContainer investigationOrAppWindsorContainer, ObservableCollection<HTTPMsg> model) : base(investigationOrAppWindsorContainer, model)
        //{
        //   this.HTTPMsgs = model;
        //}
        public SnooperHTTPViewModel(IWindsorContainer applicationWindsorContainer, ExportVm model, IHTTPExportsView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                Task.Run(() =>
                {
                    this.HTTPMsgs = model.SnooperExportedObjects.Where(i => i is SnooperExportedDataObjectHTTP).Cast<SnooperExportedDataObjectHTTP>().ToArray();
                    this.IsHidden = !this.HTTPMsgs.Any();
                    this.IsActive = this.HTTPMsgs.Any();
                    this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject, p => this.OnPropertyChanged(nameof(this.SelectedMsg)));
                });
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }
    }
}

