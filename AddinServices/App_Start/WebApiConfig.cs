using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AddinServices
{
    public static class WebApiConfig
    {
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			config.Formatters.Clear();
			config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());

			config.Routes.MapHttpRoute(
					name: "DefaultApi",
					routeTemplate: "api/{controller}"
			);
	}
	}
}
