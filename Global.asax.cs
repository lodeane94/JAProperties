using log4net;
using SS.Quartz;
using SS.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace SS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        int totalOnlineUser = 0;

        protected void Application_Start()
        {
            var config = GlobalConfiguration.Configuration;
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //start quartz job
            JobScheduler.ScheduleJobs().GetAwaiter().GetResult();
            //log4Net initialization
            log4net.Config.XmlConfigurator.Configure();
            //ignoring circular references when serializing objects
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
            = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            Application["TotalOnlineUsers"] = 0;
        }

        protected void Session_Start()
        {
            Application.Lock();
            Application["TotalOnlineUsers"] = (int)Application["TotalOnlineUsers"] + 1;
            Application.UnLock();

            ApplicationHub.UpdateUserSessionCount((int)Application["TotalOnlineUsers"]);

            log.Info("Total Online Users = "+ (int)Application["TotalOnlineUsers"]);
        }

        protected void Session_End()
        {
            Application.Lock();
            Application["TotalOnlineUsers"] = (int)Application["TotalOnlineUsers"] - 1;
            Application.UnLock();

            ApplicationHub.UpdateUserSessionCount((int)Application["TotalOnlineUsers"]);

            log.Info("Total Online Users = " + (int)Application["TotalOnlineUsers"]);
        }
    }
}
