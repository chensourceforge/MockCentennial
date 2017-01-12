using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MockCentennial.Models
{
    public class Registration
    {
        public int StudentId { get; set; }
        public string StudentNum { get; set; }
        public string StudentFirstMidName { get; set; }
        public string StudentLastName { get; set; }
        public bool StudentHasHolds { get; set; }
        public bool StudentAcademicStanding { get; set; }
        public bool StudentCanRegister { get; set; }
        public DateTime StudentStartDate { get; set; }


        public double? TransferCredit { get; set; }
        public double? InstitutionalCredit { get; set; }
        public string DegreeName { get; set; }
        public string DegreeLevel { get; set; }
        public string ProgramName { get; set; }

        public string CampusName { get; set; }
        public string DepartmentName { get; set; }
        public string TermName { get; set; }
        public string ProgramSemesterName { get; set; }

    }

    public class TermOption
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
    }

    public class ProgramOption
    {
        public int ProgramId { get; set; }
        public string ProgramCode { get; set; }
        public string ProgramName { get; set; }
        public int ProgramSemesters { get; set; }
    }

    public class ProgramRegistration
    {
        public int StudentId { get; set; }
        
        public List<TermOption> TermOptions { get; set; }
        public int TermId { get; set; }
        public List<ProgramOption> ProgramOptions { get; set; }
        public int ProgramId { get; set; }
        public int CurrentSemester { get; set; }
    }
}