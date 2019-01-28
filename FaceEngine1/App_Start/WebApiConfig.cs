using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace FaceEngine1
{
    /// <summary>
    /// Klasa konfiguracji interfejsu.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Rejestruje konfiguracje strony.
        /// Ustawia scieżke dostępu do metody.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            // Konfiguracja i usługi interfejsu Web API

            // Trasy interfejsu Web API 
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{type}/{id}/{recepta}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
