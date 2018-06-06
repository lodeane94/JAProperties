using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.SignalR
{
    [HubName("applicationHub")]
    public class ApplicationHub : Hub
    {
        /// <summary>
        /// Updates a clients message box if online with new message real time
        /// </summary>
        /// <param name="identity">The email of the user that the message is designated to</param>
        [HubMethodName("updateUserSessionCount")]
        public static void UpdateUserSessionCount(int usersSessionCount)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ApplicationHub>();
            context.Clients.All.updateUserSessionCount(usersSessionCount);
        }
    }
}