using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;

namespace Netfox.Web.App
{
    public class NetfoxRouteStrategy : DefaultRouteStrategy
    {
        public NetfoxRouteStrategy(DotvvmConfiguration configuration, string viewsFolder = "Views") : base(configuration, viewsFolder) { }

        protected override string GetRouteUrl(RouteStrategyMarkupFileInfo file)
        {
            var url = base.GetRouteUrl(file);

            if (url.StartsWith("Investigation/Export"))
            {
                url = url.Replace("Investigation/Export/", String.Empty);
                url = url.Replace("Framework/Snooper.", String.Empty);

                return "Investigation/{InvestigationId:guid}/Export/" + url;
            }
            return base.GetRouteUrl(file);

        }

        
    }
}