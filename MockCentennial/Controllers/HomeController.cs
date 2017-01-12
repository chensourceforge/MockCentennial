using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockCentennial.Models;
using MockCentennial.Models.TranscriptModel;
using Newtonsoft.Json;

namespace MockCentennial.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            
            if (Session["student"] != null)
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return View("DisplayLogin");
            }

            
        }


    }
}