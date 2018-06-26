using MJIoT_WebAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MJIoT_WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.EnableCors();  //added by me to enable CORS

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.MessageHandlers.Add(new TokenValidator());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }  //dzięki temu nie trzeba podawac ID (w moim przypadku nie wykrozytsam pewnie ID nigdy)
            );
        }
    }
}
