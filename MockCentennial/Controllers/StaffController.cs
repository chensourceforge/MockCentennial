using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using MockCentennial.Models;
using MockCentennial.Models.EnrollmentModel;
using MockCentennial.Models.SchoolModel;
using MockCentennial.Models.TranscriptModel;
using Newtonsoft.Json;

namespace MockCentennial.Controllers
{
    public class StaffController : Controller
    {
        private CentennialDA dao;

        public StaffController()
        {
            dao = new CentennialDA();
        }

        public ActionResult FindStudent(string StudentNum)
        {
            Student student = dao.FindStudentByStudentNum(StudentNum);
            if (student != null)
            {
                Session["student"] = student;
                return View("ChangeStudentInfo", student);
            }
            else
            {
                return View("CreateStudent", new Student { StudentNum = StudentNum, StudentStartDate = DateTime.Today });
            }
        }

        public int GetSemesters(int programId)
        {
            List<ProgramOption> options = (List<ProgramOption>)Session["ProgramOptions"];
            foreach (ProgramOption option in options)
            {
                if (programId == option.ProgramId)
                {
                    return option.ProgramSemesters;
                }
            }
            return 0;
        }

        [HttpPost]
        public ActionResult CreateStudent(Student student)
        {
            if (dao.CreateStudent(ref student))
            {
                Session["student"] = student;
                return RedirectToAction("CreateProgramRegistration", new {StudentId = student.StudentId});
            }
            
            return View(student);
        }

        public ActionResult CreateProgramRegistration(int StudentId)
        {
            ProgramRegistration model = new ProgramRegistration();
            model.StudentId = StudentId;

            // Uncomment for production
            //DateTime today = DateTime.Today;
            //DateTime fromDate = today.AddDays(-30);
            //DateTime toDate = today.AddMonths(5);
            //model.TermOptions = dao.GetTermOptions(fromDate, toDate);

            model.TermOptions = dao.GetAllTermOptions();
            model.ProgramOptions = dao.GetProgramOptions();

            Session["TermOptions"] = model.TermOptions;
            Session["ProgramOptions"] = model.ProgramOptions;

            return View(model);
        }
        

        [HttpPost]
        public ActionResult CreateProgramRegistration(ProgramRegistration model)
        {
            if (dao.ProgramRegistrationNew(model.TermId, model.ProgramId, model.CurrentSemester, model.StudentId))
            {
                if (dao.CreateTranscript(model.StudentId))
                {
                    return View("Index");
                }
            }
            model.TermOptions = (List<TermOption>)Session["TermOptions"];
            model.ProgramOptions = (List<ProgramOption>) Session["ProgramOptions"];
            return View(model);
        }

        public ActionResult EditProgramRegistration(int StudentId, int ProgramId)
        {
            ProgramRegistration model = new ProgramRegistration();
            model.StudentId = StudentId;
            model.ProgramId = ProgramId;
            model.ProgramOptions = dao.GetProgramOptions();

            DateTime today = DateTime.Today;
            DateTime fromDate = today.AddDays(-30);
            DateTime toDate = today.AddMonths(5);
            model.TermOptions = dao.GetTermOptions(fromDate, toDate);

            Session["TermOptions"] = model.TermOptions;
            Session["ProgramOptions"] = model.ProgramOptions;

            return View(model);
        }

        [HttpPost]
        public ActionResult TransferStudentProgram(ProgramRegistration model)
        {
            if (dao.TransferStudentToProgram(model.TermId, model.StudentId, model.ProgramId, model.CurrentSemester))
            {
                // transfer success
                return RedirectToAction("Index");
            }
            else
            {
                // transfer fail
                return View("EditProgramRegistration", model);
            }
        }

        [HttpPost]
        public JsonResult UpdateCurrentSemester(int StudentProgramId, int CurrentSemester)
        {
            bool success = dao.UpdateCurrentSemester(StudentProgramId, CurrentSemester);
            return Json(new {success=success});
        }

        [HttpPost]
        public JsonResult CancelRegistration(int StudentId, int TermId, int RegistrationId)
        {
            bool success = dao.CancelRegistration(StudentId, TermId, RegistrationId);
            return Json(new { success = success });
        }

        [HttpPost]
        public ActionResult ChangeStudentInfo(Student student, bool resetPassword)
        {
            bool success = false;
            if (dao.UpdateStudent(student))
            {
                if (resetPassword)
                {
                    if (dao.UpdateUserPassword(student.StudentId, "password"))
                    {
                        success = true;
                    }
                }
                else
                {
                    success = true;
                }
            }
            if (success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(student);
            }
        }

        public PartialViewResult GetStudentTranscript(int StudentId)
        {
            Transcript transcript = dao.GetStudentTranscript(StudentId);
            return PartialView("StudentTranscript", transcript);
        }

        [HttpPost]
        public JsonResult UpdateStudentTranscript(int StudentId, string transcriptText)
        {
            bool success = false;
            // TODO
            return Json(new { success = success });
        }

        public PartialViewResult GetStudentProgramRegistration(int StudentId)
        {
            ViewBag.studentId = StudentId;
            ViewBag.hasProgram = false;

            int studentProgramId, currentSemester;
            ProgramOption programOption;
            if (dao.GetStudentProgramOption(StudentId, out studentProgramId, out currentSemester, out programOption))
            {
                ViewBag.hasProgram = true;

                ViewBag.studentProgramId = studentProgramId;
                ViewBag.currentSemester = currentSemester;
                ViewBag.programOption = programOption;

                ViewBag.registrationOptions = dao.GetValidRegistrations(studentProgramId);
            }

            return PartialView("StudentProgramRegistration");
        }

        public PartialViewResult GetStudentEnrollment(int StudentId)
        {
            ViewBag.phase = 1;
            ViewBag.termOptions = dao.GetAllTermOptions();
            return PartialView("StudentEnrollment");
        }
        public PartialViewResult GetStudentEnrollmentForTerm(int StudentId, int TermId)
        {
            ViewBag.phase = 2;
            ViewBag.enrollmentSelections = dao.GetStudentEnrollmentSelections(StudentId, TermId);
            ViewBag.termCourseOptions = dao.GetTermCourses(TermId);
            ViewBag.termId = TermId;
            return PartialView("StudentEnrollment");
        }

        [HttpPost]
        public JsonResult UpdateEnrollmentSection(int EnrollmentId, int SectionId)
        {
            return Json(new {success = dao.UpdateEnrollmentSection(EnrollmentId, SectionId)});
        }

        [HttpPost]
        public JsonResult DropEnrollment(int EnrollmentId)
        {
            return Json(new { success = dao.DeleteEnrollment(EnrollmentId) });
        }

        [HttpPost]
        public JsonResult AddEnrollment(int StudentId, int SectionId)
        {
            return Json(new { success = dao.AddEnrollment(StudentId, SectionId) });
        }

        public JsonResult GetSectionOptions(int TermCourseId)
        {
            return Json(dao.GetSectionOptions(TermCourseId), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetStudentTransferCredit(int StudentId)
        {
            ViewBag.courseOptions = dao.GetAllCourseOptions();
            return PartialView("StudentTransferCredit");
        }

        [HttpPost]
        public JsonResult AddTransferCredit(int StudentId, int CourseId, string school, string gradeLetter)
        {
            bool success = false;
            TransferCredit transferCredit = dao.ConvertToTransferCredit(CourseId, school, gradeLetter);
            if (transferCredit != null)
            {
                if (dao.AddTransferCredit(StudentId, transferCredit))
                {
                    success = true;
                }
            }
            return Json(new { success = success });
        }

        public ActionResult ListCourses()
        {
            List<CourseInfo> courses = dao.GetAllCourses();
            return View(courses);
        }
        public ActionResult EditCourse(int courseId)
        {
            CourseInfo courseInfo = dao.GetCourseInfo(courseId);
            courseInfo.CourseOptions =
                dao.GetAllCourses()
                    .Where(c => c.CourseId != courseId)
                    .Select(c => new CourseOption {CourseId = c.CourseId, CourseCode = c.CourseCode})
                    .ToList();
            courseInfo.PrereqIds = courseInfo.Prereqs?.Select(p => p.courseId).ToList();
            return View(courseInfo);
        }

        [HttpPost]
        public ActionResult EditCourse(CourseInfo courseInfo)
        {
            courseInfo.CourseCode = $"{courseInfo.CourseSubject}{courseInfo.CourseNum}";
            if (dao.UpdateCourseInfo(courseInfo))
            {
                return RedirectToAction("ListCourses");
            }
            return RedirectToAction("EditCourse", new {courseId = courseInfo.CourseId});
        }

        [HttpPost]
        public ActionResult EditCourseOfferings(TermCourseOffering offering)
        {
            int TermId = offering.TermId;
            bool isEditable = true;

            // uncomment this in production
            //if (dao.GetTermEndDate(TermId).CompareTo(DateTime.Today) < 0) isEditable = false;

            if (isEditable && dao.UpdateTermCourses(TermId, offering.CourseIds))
            {
                return View("Index");
            }
            else
            {
                return View();
            }
        }

        public ActionResult EditCourseOfferings()
        {
            TermCourseOffering offering=new TermCourseOffering();
            offering.CourseOptions = dao.GetAllCourseOptions();
            offering.TermOptions = dao.GetAllTermOptions();
            return View(offering);
        }

        public string GetCoursesOffered(int termId)
        {
            return JsonConvert.SerializeObject(dao.GetCoursesOffered(termId));
        }


        [HttpPost]
        public ActionResult EditSections(int TermId)
        {
            SectionOffering offering = (SectionOffering)Session["SectionOffering"];
            offering.TermId = TermId;

            offering.TermCourseOptions = dao.GetTermCourses(TermId);

            return View(offering);
        }
        public ActionResult EditSections()
        {
            SectionOffering offering=new SectionOffering();
            offering.TermOptions = dao.GetAllTermOptions();
            Session["SectionOffering"] = offering;

            Session["InstructorOptions"] = dao.GetAllInstructorOptions();
            Session["RoomOptions"] = dao.GetAllRoomOptions();
            Session["TimeOptions"] = dao.GetAllTimeOptions();

            return View(offering);
        }

        private SectionDetails GetExistingSectionDetails(int TermCourseId)
        {
            SectionDetails sectionDetails = new SectionDetails();
            sectionDetails.SectionInfos = dao.GetSectionsForTermCourse(TermCourseId);
            sectionDetails.InstructorOptions = (List<InstructorOption>)Session["InstructorOptions"];
            sectionDetails.RoomOptions = (List<RoomOption>)Session["RoomOptions"];
            sectionDetails.TimeOptions = (List<TimeOption>)Session["TimeOptions"];
            sectionDetails.TermCourseId = TermCourseId;
            return sectionDetails;
        }
        public ActionResult EditSectionDetails(int TermCourseId)
        {
            SectionDetails sectionDetails = GetExistingSectionDetails(TermCourseId);
            return PartialView(sectionDetails);
        }

        
        public ActionResult CreateNewSection(int TermCourseId)
        {
            SectionDetails sectionDetails = GetExistingSectionDetails(TermCourseId);
            SectionInfo newSectionInfo = new SectionInfo();
            newSectionInfo.ClassInfos=new List<ClassInfo>() {new ClassInfo()};
            sectionDetails.SectionInfos.Add(newSectionInfo);
            return PartialView("EditSectionDetails", sectionDetails);
        }

        [HttpPost]
        public ActionResult DeleteSection(int TermCourseId, int SectionId)
        {
            if (SectionId != 0)
            {
                dao.DeleteSection(SectionId);
            }
            return RedirectToAction("EditSectionDetails", new { TermCourseId = TermCourseId });
        }

        public ActionResult CreateNewClass(int TermCourseId, int SectionId)
        {
            if (SectionId == 0)
            {
                return RedirectToAction("CreateNewSection", new {TermCourseId = TermCourseId});
            }
            else
            {
                SectionDetails sectionDetails = GetExistingSectionDetails(TermCourseId);
                SectionInfo sectionInfo = sectionDetails.SectionInfos.Find(s => s.SectionId == SectionId);
                sectionInfo.ClassInfos.Add(new ClassInfo());
                return PartialView("EditSectionDetails", sectionDetails);
            }
        }

        [HttpPost]
        public ActionResult DeleteClass(int TermCourseId, int ClassId)
        {
            if (ClassId != 0)
            {
                dao.DeleteClass(ClassId);
            }
            return RedirectToAction("EditSectionDetails", new { TermCourseId = TermCourseId });
        }

        [HttpPost]
        public ActionResult SaveClassDetails(int TermCourseId, int SectionId, int ClassId, string SectionNum, 
            bool ClassIsLecture, int InstructorId, int RoomId, int ClassStartTime, int ClassEndTime)
        {
            bool isSuccessful = false;
            if (ClassId != 0)
            {
                // both section and class already exist
                isSuccessful = dao.UpdateSection(SectionId, SectionNum);
                if (isSuccessful)
                {
                    isSuccessful = dao.UpdateClass(new ClassInfo
                    {
                        ClassId = ClassId,
                        ClassIsLecture = ClassIsLecture,
                        InstructorId = InstructorId,
                        RoomId = RoomId,
                        ClassStartTime = ClassStartTime,
                        ClassEndTime = ClassEndTime
                    });
                }
            }
            else if (SectionId != 0)
            {
                // section already exists, but class doesn't exist yet
                isSuccessful = dao.UpdateSection(SectionId, SectionNum);
                if (isSuccessful)
                {
                    isSuccessful = dao.CreateClassForSection(SectionId, new ClassInfo
                    {
                        ClassIsLecture = ClassIsLecture,
                        InstructorId = InstructorId,
                        RoomId = RoomId,
                        ClassStartTime = ClassStartTime,
                        ClassEndTime = ClassEndTime
                    });
                }
            }
            else
            {
                // neither section nor class exist yet
                SectionId = dao.CreateSectionForTermCourse(TermCourseId, SectionNum);
                if (SectionId != 0)
                {
                    isSuccessful = dao.CreateClassForSection(SectionId, new ClassInfo
                    {
                        ClassIsLecture = ClassIsLecture,
                        InstructorId = InstructorId,
                        RoomId = RoomId,
                        ClassStartTime = ClassStartTime,
                        ClassEndTime = ClassEndTime
                    });
                }
            }
            return RedirectToAction("EditSectionDetails", new {TermCourseId = TermCourseId});
        }


        public ActionResult UpdateStudentGrades(int? TermId, int? TermCourseId, int? SectionId)
        {
            if (TermId == null)
            {
                Grading model = new Grading();
                model.TermOptions = dao.GetAllTermOptions();
                model.TermCourseOptions = new List<TermCourseOption>();
                model.SectionOptions = new List<SectionOption>();
                return View(model);
            }
            if (TermCourseId == null)
            {
                return Json(dao.GetTermCourses(TermId.Value), JsonRequestBehavior.AllowGet);
            }
            if (SectionId == null)
            {
                return Json(dao.GetSectionOptions(TermCourseId.Value), JsonRequestBehavior.AllowGet);
            }

            return PartialView("EditGrades", dao.GetGrades(SectionId.Value));
        }

        [HttpPost]
        public JsonResult UpdateGrade(int StudentId, int TermCourseId, int EnrollmentId, double GradeNumeric)
        {
            bool success = dao.UpdateGrade(StudentId, TermCourseId, EnrollmentId, GradeNumeric);
            return Json(new { success = success });
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}