
using System.Collections.Generic;

namespace MockCentennial.Models
{
    namespace TimetableModel
    {
        public class ClassDisplay
        {
            public string MeetingTime { get; set; }
            public string Instructor { get; set; }
            public string Campus { get; set; }
            public string Room { get; set; }
            public bool IsLecture { get; set; }
        }
        public class SectionDisplay
        {
            public int SectionId { get; set; }
            public string SectionNum { get; set; }
            public bool SectionIsSelected { get; set; }
            public List<ClassDisplay> Classes { get; set; }
        }

        public class PrereqDisplay
        {
            public string CourseCode { get; set; }
        }
        public class CourseDisplay
        {
            public bool CourseIsOffered { get; set; }
            public bool CourseIsCompleted { get; set; }

            public int CourseId { get; set; }
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public double CourseCredits { get; set; }
            public bool IsAcademic { get; set; }
            
            public List<PrereqDisplay> Prereqs { get; set; }
            public List<SectionDisplay> Sections { get; set; }
        }

        public class ElectiveDisplay
        {
            //public bool CourseIsOffered { get; set; }
            //public bool CourseIsCompleted { get; set; }
            public bool IsTechnicalElective { get; set; }
            public List<CourseDisplay> Courses { get; set; }
        }

        public class TermDisplay
        {
            public string ProgramSemesterName { get; set; }
            public List<CourseDisplay> Courses { get; set; }
            public List<ElectiveDisplay> Electives { get; set; }
        }
        public class TimetableBuilder
        {
            public int TermId { get; set; }
            public List<TermDisplay> Terms { get; set; }
        }

        public class TimetableEntry
        {
            public int startDay { get; set; }
            public int startHour { get; set; }
            public int startMinute { get; set; }
            public int endDay { get; set; }
            public int endHour { get; set; }
            public int endMinute { get; set; }
            public int classId { get; set; }
            public int courseId { get; set; }
            public string courseCode { get; set; }
            public string section { get; set; }
            public string room { get; set; }
            public bool isLecture { get; set; }
        }
    }
}