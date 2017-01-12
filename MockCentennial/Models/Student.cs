using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MockCentennial.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentNum { get; set; }
        
        public string StudentLastName { get; set; }
        public string StudentFirstMidName { get; set; }
        public string StudentEmail { get; set; }
        public bool StudentHasHolds { get; set; }
        public bool StudentAcademicStanding { get; set; }
        public bool StudentCanRegister { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StudentStartDate { get; set; }

    }
}