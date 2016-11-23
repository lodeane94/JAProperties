using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore };

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "service/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
