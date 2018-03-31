using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SS.SignalR
{
    [HubName("notificationHub")]
    public class DashboardHub : Hub
    {
        /// <summary>
        /// Updates a clients message box if online with new message real time
        /// </summary>
        /// <param name="identity">The email of the user that the message is designated to</param>
        [HubMethodName("broadcastUserMessages")]
        public static void BroadcastUserMessages(String identity)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>();
            context.Clients.User(identity).updateUserMessages();
        }

        /// <summary>
        /// Updates a clients message box if online with new message real time
        /// </summary>
        /// <param name="identity">The email of the user that the message is designated to</param>
        [HubMethodName("broadcastMeeting")]
        public static void broadcastMeeting(String identity)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>();
            context.Clients.User(identity).updateMeeting();
        }

        /// <summary>
        /// Updates a property owner of a new requisition
        /// </summary>
        /// <param name="identity"></param>
        [HubMethodName("alertRequisition")]
        public static void alertRequisition(String identity)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>();
            context.Clients.User(identity).newRequisitionAlert();
        }
    }
}