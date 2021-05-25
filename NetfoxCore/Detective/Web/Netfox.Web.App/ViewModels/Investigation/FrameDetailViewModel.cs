using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class FrameDetailViewModel : BlankLayoutViewModel
    {
        private CaptureFacade CaptureFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromRoute("FrameId")]
        public Guid FrameId { get; set; }

        public PmFrameBaseDetailDTO Frame { get; set; }

        public List<string> HexData { get; set; } = new List<string>();

        public List<string> AsciiData { get; set; } = new List<string>();

        public FrameDetailViewModel(CaptureFacade captureFacade) { this.CaptureFacade = captureFacade; }

        public override Task PreRender()
        {
            this.Frame = this.CaptureFacade.GetFrame(this.InvestigationId, this.FrameId, this.AppPath);

            var hex = BitConverter.ToString(Frame.Data);
            this.HexData = hex.Split('-').ToList();
            var AsciiDatachars = Encoding.ASCII.GetString(Frame.Data).ToCharArray();

            foreach(var c in AsciiDatachars)
            {
                this.AsciiData.Add((c > 32) ? c.ToString() : "?");
            }

            return base.PreRender();
        }
        
    }
}

