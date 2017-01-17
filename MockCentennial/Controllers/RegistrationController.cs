using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MockCentennial.Models;
using MockCentennial.Models.EnrollmentModel;
using MockCentennial.Models.TimetableModel;
using Newtonsoft.Json;


namespace MockCentennial.Controllers
{
    public class RegistrationController : Controller
    {
        private CentennialDA dao;
        public RegistrationController()
        {
            dao=new CentennialDA();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RegistrationStatus()
        {
            Student student = (Student) Session["student"];
            Registration registration = dao.GetRegistrationInfo(student.StudentId);
            return View(registration);
        }

        [HttpPost]
        public ActionResult BuildTimetable(string choices, int termId)
        {
            Student student = (Student)Session["student"];
            Dictionary<int, int> newChoices = JsonConvert.DeserializeObject<Dictionary<int, int>>(choices);
            bool success = dao.UpdateEnrollment(student.StudentId, termId, newChoices);
            if (success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("BuildTimetable", new {TermId = termId});
            }
        }

        public ActionResult BuildTimetable(int? TermId)
        {
            if (TermId == null)
            {
                return RedirectToAction("ChooseTermForTimetable");
            }

            int termId = TermId.Value;
            int studentId = ((Student)Session["student"]).StudentId;

            if (!CanEnroll(studentId, termId))
            {
                return RedirectToAction("RegistrationStatus");
            }

            List<EnrollmentOption> enrollmentOptions = dao.GetEnrollmentOptions(studentId, termId);
            
            List<TermDisplay> terms = new List<TermDisplay>();
            TermDisplay currentTerm = null;
            CourseDisplay currentCourse = null;
            int prevTermId = 0, prevCourseId = 0;

            foreach (EnrollmentOption o in enrollmentOptions)
            {
                if (prevTermId != o.ProgramSemesterId)
                {
                    // new term, new course
                    prevTermId = o.ProgramSemesterId;
                    prevCourseId = o.CourseId;
                    currentTerm = new TermDisplay()
                    {
                        ProgramSemesterName = o.ProgramSemesterName,
                        Courses = new List<CourseDisplay>(),
                        Electives = new List<ElectiveDisplay>()
                    };
                    terms.Add(currentTerm);

                    currentCourse = AddCourseToTerm(o, currentTerm);
                }
                else if (prevCourseId != o.CourseId)
                {
                    // same term, new course
                    prevCourseId = o.CourseId;
                    currentCourse = AddCourseToTerm(o, currentTerm);
                }
                else
                {
                    // same course, another section
                    AddSectionToCourse(o, currentCourse);
                }
            }

            ViewBag.JSONString = JsonConvert.SerializeObject(enrollmentOptions);
            TimetableBuilder timetableBuilder = new TimetableBuilder() {Terms = terms, TermId = termId };
            return View(timetableBuilder);
        }

        public ActionResult ViewTimetable()
        {
            Student student = (Student) Session["student"];
            TermOption term = dao.GetCurrentTermOption();
            List<TimetableEntry> model = dao.GetTimetable(student.StudentId, term.TermId);
            return View(model);
        }

        private bool CanEnroll(int StudentId, int TermId)
        {
            if (((Student) Session["student"]).StudentCanRegister)
            {
                if (dao.HasValidRegistration(StudentId, TermId))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddSectionToCourse(EnrollmentOption o, CourseDisplay currentCourse)
        {
            if (!currentCourse.CourseIsOffered)
            {
                return;
            }
            currentCourse.Sections.Add(new SectionDisplay
            {
                SectionId = o.SectionId,
                SectionNum = o.SectionNum,
                SectionIsSelected = o.SectionIsSelected,
                Classes = o.Classes.Select(c => ConvertToClassDisplay(c)).ToList()
            });
        }
        private CourseDisplay AddCourseToTerm(EnrollmentOption o, TermDisplay currentTerm)
        {
            CourseDisplay currentCourse = new CourseDisplay
            {
                CourseIsOffered = o.CourseIsOffered,
                CourseIsCompleted = o.courseIsCompleted,
                CourseId = o.CourseId,
                CourseCode = o.CourseCode,
                CourseTitle = o.CourseTitle,
                CourseCredits = o.CourseCredits,
                IsAcademic = o.IsAcademic,
                Prereqs = o.Prereqs.Select(p => new PrereqDisplay { CourseCode = $"{p.courseSubject}{p.courseNum}" }).ToList(),
                Sections = new List<SectionDisplay>()
            };

            if (o.IsMandatory)
            {
                currentTerm.Courses.Add(currentCourse);
            }
            else
            {
                bool electiveAdded = false;
                foreach (ElectiveDisplay e in currentTerm.Electives)
                {
                    if (e.IsTechnicalElective == o.IsTechnicalElective)
                    {
                        e.Courses.Add(currentCourse);
                        electiveAdded = true;
                    }
                }
                if (!electiveAdded)
                {
                    currentTerm.Electives.Add(new ElectiveDisplay
                    {
                        IsTechnicalElective = o.IsTechnicalElective,
                        Courses = new List<CourseDisplay>() { currentCourse }
                    });
                }
            }

            AddSectionToCourse(o, currentCourse);
            return currentCourse;
        }
        private ClassDisplay ConvertToClassDisplay(Class c)
        {
            ClassDisplay cd=new ClassDisplay
            {
                MeetingTime = $"{dao.ConvertDay(c.startDay)} {dao.ConvertTime(c.startHour, c.startMinute)} - {dao.ConvertTime(c.endHour, c.endMinute)}",
                Instructor = $"{c.instructorFirstMidName} {c.instructorLastName}",
                Campus = c.campus,
                Room = $"{c.building} {c.room}",
                IsLecture = c.isLecture
            };
            return cd;
        }

        public ActionResult ChooseTermForTimetable()
        {
            TermPicker termPicker = new TermPicker();

            // for development only
            termPicker.TermOptions = dao.GetTermOptions(new DateTime(), new DateTime(9999, 12, 31));

            // for production
            //DateTime today = DateTime.Today;
            //DateTime fromDate = today.AddDays(-30);
            //DateTime toDate = today.AddMonths(5);
            //termPicker.TermOptions = dao.GetTermOptions(fromDate, toDate);

            return View(termPicker);
        }

        private bool SetStudentCanRegister(Student student)
        {
            if (student.StudentCanRegister)
            {
                return true;
            }
            if (dao.EnableStudentCanRegister(student.StudentId))
            {
                student.StudentCanRegister = true;
                Session["student"] = student;
                return true;
            }
            return false;
        }

        private bool CreateRegistrationRecord(int TermId, Student student)
        {
            if (dao.HasValidRegistration(student.StudentId, TermId))
            {
                return true;
            }
            return dao.ProgramRegistrationReAdmit(TermId, student.StudentId);
        }

        public ActionResult ReAdmit()
        {
            // use in development only
            List<TermOption> model = dao.GetAllTermOptions();

            // uncomment for production
            //DateTime fromDate=DateTime.Today.AddDays(-30);
            //DateTime toDate=DateTime.Today.AddMonths(5);
            //List<TermOption> model = dao.GetTermOptions(fromDate, toDate);
            return View(model);
        }

        
        [HttpPost]
        public string ReAdmit(int TermId)
        {
            Student student = (Student) Session["student"];
            if (student.StudentHasHolds)
            {
                return "Sorry, you have holds on your account preventing registration.";
            }
            if (!student.StudentAcademicStanding)
            {
                return "Sorry, your academic standing prevents registration.";
            }

            if (CreateRegistrationRecord(TermId, student))
            {
                if (SetStudentCanRegister(student))
                {
                    return "You have successfully registered.";
                }
            }
            return "A problem occured during registration.";
        }


        public int ConfirmProgramTransfer(int TermId)
        {
            Student student = (Student) Session["student"];
            return dao.GetNumberEnrolledCourses(student.StudentId, TermId);
        }

        public ActionResult ProgramTransfer()
        {
            ProgramRegistration model=new ProgramRegistration();
            // use in development only
            model.TermOptions = dao.GetAllTermOptions();
            model.ProgramOptions = dao.GetProgramOptions();
            return View(model);
        }

        [HttpPost]
        public string ProgramTransfer(int TermId, int ProgramId)
        {
            Student student = (Student)Session["student"];
            int studentId = student.StudentId;
            int toSemester = dao.GetCurrentSemesterInStudentProgram(studentId, ProgramId);
            if (dao.TransferStudentToProgram(TermId, studentId, ProgramId, toSemester))
            {
                return "Program transferred successfully.";
            }
            else
            {
                return "A problem occured during program transfer operation.";
            }
        }

        public ActionResult FeeAccountSummary()
        {
            Student student = (Student)Session["student"];
            int studentId = student.StudentId;
            int termId = dao.GetCurrentTermOption().TermId;
            return View(dao.GetInvoiceInfo(studentId, termId));
        }

        [HttpPost]
        public string MakePayment(int registrationId, double amount)
        {
            Student student = (Student)Session["student"];
            int studentId = student.StudentId;
            if (dao.ReceivePayment(studentId, registrationId, amount))
            {
                student = dao.FindStudentById(studentId);
                Session["student"] = student;
                return "Thank you for your payment.";
            }
            else
            {
                return "A problem occurred while processing payment.";
            }
        }

    }
}