using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Netfox.Web.App
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);

            config.ExperimentalFeatures.ExplicitAssemblyLoading.Enable();
            config.Markup.AddAssembly(typeof(DotvvmStartup).Assembly.GetName().Name);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(i => i.GetName().Name.EndsWith(".WEB")))
            {
                config.Markup.AddAssembly(asm.GetName().Name);
            }
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dothtml");
            config.RouteTable.Add("Settings_User", "Settings/UserManagement/User/{UserId:guid}", "Views/Settings/User.dothtml", new { UserId = Guid.Empty });
            config.RouteTable.Add("Investigations_Investigation", "Investigations/Investigation/{InvestigationId:guid}", "Views/Investigations/investigation.dothtml", new { InvestigationId = Guid.Empty });
            config.RouteTable.Add("Investigation", "Investigation/{InvestigationId:guid}", "Views/Investigation/overview.dothtml", new { InvestigationId = Guid.Empty });
            config.RouteTable.Add("Investigation_Capture", "Investigation/{InvestigationId:guid}/Capture/{CaptureId:guid}", "Views/Investigation/capture.dothtml", new { InvestigationId = Guid.Empty, CaptureId = Guid.Empty });
            config.RouteTable.Add("Investigation_L3_Detail", "Investigation/{InvestigationId:guid}/L3Detail/{ConversationId:guid}", "Views/Investigation/L3Detail.dothtml", new { InvestigationId = Guid.Empty, ConversationId = Guid.Empty });
            config.RouteTable.Add("Investigation_L4_Detail", "Investigation/{InvestigationId:guid}/L4Detail/{ConversationId:guid}", "Views/Investigation/L4Detail.dothtml", new { InvestigationId = Guid.Empty, ConversationId = Guid.Empty });
            config.RouteTable.Add("Investigation_L7_Detail", "Investigation/{InvestigationId:guid}/L7Detail/{ConversationId:guid}", "Views/Investigation/L7Detail.dothtml", new { InvestigationId = Guid.Empty, ConversationId = Guid.Empty });
            config.RouteTable.Add("Investigation_Frame_Detail", "Investigation/{InvestigationId:guid}/FrameDetail/{FrameId:guid}", "Views/Investigation/FrameDetail.dothtml", new { InvestigationId = Guid.Empty, FrameId = Guid.Empty });
            config.RouteTable.Add("DownloadFile", "DownloadFile/{InvestigationId:guid}", "Views/DownloadFile.dothtml", new { InvestigationId = Guid.Empty, Export = String.Empty });

            config.RouteTable.AutoDiscoverRoutes(new NetfoxRouteStrategy(config));       
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
            config.Markup.AddMarkupControl("cc", "Menu", "Controls/Menu.dotcontrol");
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
            
            config.Resources.Register("fonts", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/fonts/fa/css/all.min.css")
            });
            config.Resources.Register("bootstrap-css", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/css/bootstrap/bootstrap.min.css")
            });
            config.Resources.Register("template-css", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/css/template.css")
            });
            config.Resources.Register("bootstrap-theme", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/bootstrap/bootstrap-theme.min.css")
            });
            config.Resources.Register("jquery-ui-css", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/css/jquery-ui.css"),
               
            });
            config.Resources.Register("bootstrap-datepicker-css", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/Template/css/bootstrap-datetimepicker.min.css")
            });

            config.Resources.Register("bootstrap", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/bootstrap/bootstrap.min.js"),
                Dependencies = new[] { "bootstrap-css", "jquery" }
            });
            config.Resources.Register("jquery", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/jquery-3.3.1.min.js")
            });
            config.Resources.Register("chart", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/chart.min.js"),
                Dependencies = new[] { "jquery" }
            });
            config.Resources.Register("jquery-ui", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/jquery-ui.js"),
                Dependencies = new[] { "jquery" }
            });
            config.Resources.Register("fancybox", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/jquery.fancybox.min.js"),
                Dependencies = new[] { "jquery"}
            });
            config.Resources.Register("scripts", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/scripts.js"),
                Dependencies = new[] { "bootstrap", "jquery", "bs-datepicker", "fancybox" }
            });
            config.Resources.Register("moment", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/moment.js"),
                Dependencies = new[] { "jquery" }
            });
            config.Resources.Register("bs-datepicker", new ScriptResource
            {
                Location = new UrlResourceLocation("~/Template/js/bootstrap-datetimepicker.min.js"),
                Dependencies = new[] { "jquery", "moment" }
            });
        }

        public void ConfigureServices(IDotvvmServiceCollection options)
        {
        }
    }
}
