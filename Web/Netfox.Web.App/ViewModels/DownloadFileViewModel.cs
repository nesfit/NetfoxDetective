using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using DotVVM.Framework.ViewModel;

namespace Netfox.Web.App.ViewModels
{
    public class DownloadFileViewModel : BlankLayoutViewModel
    {
        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; private set; }

        [FromQuery("Filename")]
        public String Filename { get; private set; }

        [FromQuery("Content")]
        public String Content { get; private set; }

        [FromQuery("Path")]
        public String InvestigationFilePath { get; private set; }

        public String Path { get; set; }

        public DownloadFileViewModel() { }

        #region Overrides of DotvvmViewModelBase
        public override Task PreRender()
        {

            if(!this.InvestigationFilePath.IsNullOrEmpty() && !this.Content.IsNullOrEmpty() && !this.Filename.IsNullOrEmpty())
            {
                this.Path = this.AppPath + "Investigations/NFX_" + this.InvestigationId + "/" + this.InvestigationFilePath;

                if(File.Exists(this.Path))
                {
                    var bytes = File.ReadAllBytes(this.Path);
                    Context.ReturnFile(bytes, this.Filename, this.Content);
                }
            }
            return base.PreRender();
        }
        #endregion
    }
}

