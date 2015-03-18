using System.Web.Mvc;
using System.Web.Routing;

namespace DirEx.Web
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "home", action = "index", id = UrlParameter.Optional },
				namespaces: new string[] { "DirEx.Web.Controllers" }
			);
		}
	}
}
