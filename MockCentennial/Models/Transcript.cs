using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockCentennial.Models
{
    namespace TranscriptModel
    {
        public class Transcript
        {
            public string studentNumber { get; set; }
            public string studentName { get; set; }
            public List<History> history { get; set; }

        }

        public class Course
        {
            public string subject { get; set; }
            public string course { get; set; }
            public string level { get; set; }
            public string title { get; set; }
            public double? gradeNumeric { get; set; }
            public string gradeLetter { get; set; }
            public double? creditHours { get; set; }
            public double? qualityPoints { get; set; }
            public string r { get; set; }
        }

        public class School
        {
            public string school { get; set; }
            public List<Course> courses { get; set; }
        }

        public class Totals
        {
            public double? attemptHours { get; set; }
            public double? passedHours { get; set; }
            public double? earnedHours { get; set; }
            public double? gpaHours { get; set; }
            public double? qualityPoints { get; set; }
            public double? gpa { get; set; }
        }

        public class CurrentTerm
        {
            public Totals totals { get; set; }
        }
        public class Cumulative
        {
            public Totals totals { get; set; }
        }

        public class Term
        {
            public string term { get; set; }
            public string academicStanding { get; set; }
            public List<Course> courses { get; set; }
            public CurrentTerm currentTerm { get; set; }
            public Cumulative cumulative { get; set; }
        }

        public class Institution
        {
            public Totals totals { get; set; }
            public List<Term> terms { get; set; }
        }

        public class Transfer
        {
            public Totals totals { get; set; }
            public List<School> schools { get; set; }
        }

        public class Record
        {
            public string level { get; set; }
            public Institution institution { get; set; }
            public Transfer transfer { get; set; }
        }

        public class History
        {
            public string degreeSought { get; set; }
            public DateTime? degreeDate { get; set; }
            public string campus { get; set; }
            public string department { get; set; }
            public string program { get; set; }
            public List<Record> records { get; set; }
        }

        public class TransferCredit
        {
            public string school { get; set; }
            public string subject { get; set; }
            public string course { get; set; }
            public string level { get; set; }
            public string title { get; set; }
            public string gradeLetter { get; set; }
            public double? creditHours { get; set; }
        }

        public class InstitutionalCredit
        {
            public string term { get; set; }
            public string subject { get; set; }
            public string course { get; set; }
            public string level { get; set; }
            public string title { get; set; }
            public double? gradeNumeric { get; set; }
            public string gradeLetter { get; set; }
            public double? creditHours { get; set; }
            public double? qualityPoints { get; set; }
            public string r { get; set; }
        }
    }
}