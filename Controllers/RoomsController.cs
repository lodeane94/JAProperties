using SS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SS.Controllers
{
    public class RoomsController : Controller
    {
        [HttpPost]
        public ActionResult RequestRoom(REQUISITIONS requisitions, Guid accommodationID)
        {
            JAHomesEntities dbCtx = new JAHomesEntities();

            dbCtx.sp_make_requisition(accommodationID, requisitions.FIRST_NAME, requisitions.LAST_NAME,requisitions.GENDER, requisitions.EMAIL, requisitions.CELL);

            return RedirectToAction("Home", "Home");
        }

        [HttpPost]
        public ActionResult RetrieveSelected(string value)
        {
            JAHomesEntities dbCtx = new JAHomesEntities();

            return Content(dbCtx.ACCOMMODATIONS.Where(r => r.ID.ToString() == value).Select(r => r.ROOM_PIC_URL).Single());
        }
    }
}