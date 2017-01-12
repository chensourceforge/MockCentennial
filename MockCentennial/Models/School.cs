using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MockCentennial.Models.EnrollmentModel;

namespace MockCentennial.Models
{
    namespace SchoolModel
    {
        public class CoursePrereq
        {
            public int courseId { get; set; }
            public string courseSubject { get; set; }
            public string courseNum { get; set; }
        }

        public class CourseOption
        {
            public int CourseId { get; set; }
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
        }
        public class CourseInfo
        {
            public int CourseId { get; set; }
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public string CourseSubject { get; set; }
            public string CourseNum { get; set; }
            public string CourseLevel { get; set; }
            public double CourseCredits { get; set; }
            public List<CoursePrereq> Prereqs { get; set; }
            public List<int> PrereqIds { get; set; }
            public List<CourseOption> CourseOptions { get; set; }
        }

        public class TermCourseOffering
        {
            public List<TermOption> TermOptions { get; set; }
            public int TermId { get; set; }
            public List<CourseOption> CourseOptions { get; set; }
            public List<int> CourseIds { get; set; }
        }

        

        public class TermCourseOption
        {
            public int TermCourseId { get; set; }
            public string CourseHeading { get; set; }
        }

        public class InstructorOption
        {
            public int InstructorId { get; set; }
            public string InstructorName { get; set; }
        }

        public class RoomOption
        {
            public int RoomId { get; set; }
            public string RoomLocation { get; set; }
        }

        public class TimeOption
        {
            public int TimeId { get; set; }
            public string TimeText { get; set; }
        }

        public class ClassInfo
        {
            public int ClassId { get; set; }
            public bool ClassIsLecture { get; set; }
            public int InstructorId { get; set; }
            public int RoomId { get; set; }
            public int ClassStartTime { get; set; }
            public int ClassEndTime { get; set; }
        }
        public class SectionInfo
        {
            public int SectionId { get; set; }
            public string SectionNum { get; set; }
            public List<ClassInfo> ClassInfos { get; set; }
        }

        public class SectionDetails
        {
            public List<SectionInfo> SectionInfos { get; set; }
            public List<TimeOption> TimeOptions { get; set; }
            public List<RoomOption> RoomOptions { get; set; }
            public List<InstructorOption> InstructorOptions { get; set; }
            public int TermCourseId { get; set; }
        }

        public class SectionOffering
        {
            public List<TermOption> TermOptions { get; set; }
            public int TermId { get; set; }
            
            public List<TermCourseOption> TermCourseOptions { get; set; }
            public int TermCourseId { get; set; }

        }

        public class SectionOption
        {
            public int SectionId { get; set; }
            public string SectionNum { get; set; }
        }

        public class Grade
        {
            public int EnrollmentId { get; set; }
            public int StudentId { get; set; }
            public string StudentNum { get; set; }
            public string StudentLastName { get; set; }
            public string StudentFirstMidName { get; set; }
            public double? GradeNumeric { get; set; }
        }
        public class Grading
        {
            public List<TermOption> TermOptions { get; set; }
            public int TermId { get; set; }

            public List<TermCourseOption> TermCourseOptions { get; set; }
            public int TermCourseId { get; set; }

            public List<SectionOption> SectionOptions { get; set; }
            public int SectionId { get; set; }
        }

        public class RegistrationOption
        {
            public int RegistrationId { get; set; }
            public int TermId { get; set; }
            public string TermName { get; set; }
            public DateTime TermEndDate { get; set; }
        }

        public class EnrollmentSelection
        {
            public int EnrollmentId { get; set; }
            public int SectionId { get; set; }
            public string CourseCode { get; set; }
            public List<SectionOption> Sections { get; set; }
        }

        public class StudentEnrollmentDisplay
        {
            
        }

    }
    
}