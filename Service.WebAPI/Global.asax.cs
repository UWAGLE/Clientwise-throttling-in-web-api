using Service.Throttle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Service.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ThrottleInit.CacheClientThrollingPolicy("Resources", "ThrottleData.json");//Folder name - file name
        }
    }
}
