using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockCentennial.Models;

namespace MockCentennial.Controllers
{
    public class AccountController : Controller
    {
        private CentennialDA dao = null;

        public AccountController()
        {
            dao = new CentennialDA();
        }


        [HttpPost]
        public ActionResult Login(string UserNum, string UserPassword)
        {
            Tuple<int, bool> u = dao.FindUserByLogin(UserNum, UserPassword);
            
            if (u != null)
            {
                if (u.Item2)
                {
                    // is student
                    Student student = dao.FindStudentById(u.Item1);
                    if (student != null)
                    {
                        Session["student"] = student;
                    }
                }
                else if (UserNum.Equals("000000001"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    // is staff
                    return RedirectToAction("Index", "Staff");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            
            return RedirectToAction("Index", "Home");
        }
    }
}