using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockCentennial.Models
{
    namespace EnrollmentModel
    {
        public class Prereq
        {
            public int courseId { get; set; }
            public string courseSubject { get; set; }
            public string courseNum { get; set; }
        }

        public class Class
        {
            public bool isLecture { get; set; }
            public int startDay { get; set; }
            public int startHour { get; set; }
            public int startMinute { get; set; }
            public int endDay { get; set; }
            public int endHour { get; set; }
            public int endMinute { get; set; }
            public string instructorLastName { get; set; }
            public string instructorFirstMidName { get; set; }
            public string campus { get; set; }
            public string building { get; set; }
            public string room { get; set; }
            public int roomCapacity { get; set; }
        }

        public class EnrollmentOption
        {
            public int ProgramSemesterId { get; set; }
            public int CourseId { get; set; }
            public int SectionId { get; set; }
            public int TermCourseId { get; set; }
            public int ProgramSemesterNum { get; set; }
            public string ProgramSemesterName { get; set; }
            public bool IsAcademic { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsTechnicalElective { get; set; }
            public bool IsGeneralElective { get; set; }
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public double CourseCredits { get; set; }
            public List<Prereq> Prereqs { get; set; }
            public string SectionNum { get; set; }
            public List<Class> Classes { get; set; }
            public bool CourseIsOffered { get; set; }
            public bool SectionIsSelected { get; set; }
            public bool courseIsCompleted { get; set; }
        }

        public class TermPicker
        {
            public List<TermOption> TermOptions { get; set; }
            public int TermId { get; set; }
        }
    }
    
}