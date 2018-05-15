using Common.Logging;
using Quartz;
using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SS.Quartz
{
    public class SubscriptionPropertyMaintainanceJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Task t = Task.Run(() => 
            {
                using (var db = new EasyFindPropertiesEntities())
                {
                    var result = db.Sp_UpdatePropertiesAvailability();
                }
            });
        }
    }
}