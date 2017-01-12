using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockCentennial.Models;

namespace MockCentennial.Controllers
{
    public class StudentController : Controller
    {
        private CentennialDA dao = null;
        public StudentController()
        {
            dao = new CentennialDA();
        }

        // GET: Student
        public ActionResult Index()
        {
            return View("Portal", Session["student"]);
        }

        public ActionResult PersonalInfo()
        {
            return View(Session["student"]);
        }

        public ActionResult StudentRecord()
        {
            return View();
        }

        public ActionResult AcademicTranscript()
        {
            Student student = (Student) Session["student"];
            return View(dao.GetStudentTranscript(student.StudentId));
        }
    }
}