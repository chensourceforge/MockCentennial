using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockCentennial.Models;
using MockCentennial.Models.SchoolModel;
using MockCentennial.Models.TranscriptModel;

namespace UnitTests
{
    [TestClass]
    public class Issue5Tests
    {
        private string testVariablesFile = @"../../../../MockCentennial_test_variables.dat";
        private int GetNextStudentNum()
        {
            int studentNum = 1;
            if (File.Exists(testVariablesFile))
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(testVariablesFile)))
                {
                    studentNum = reader.ReadInt32();
                }
                studentNum++;
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(testVariablesFile)))
                {
                    writer.Write(studentNum);
                }
            }
            else
            {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(testVariablesFile)))
                {
                    writer.Write(studentNum);
                }
            }
            return studentNum;
        }
        [TestMethod]
        public void CancelRegistrationTest()
        {
            // setup student
            DateTime registrationDate = new DateTime(2013, 8, 20);
            int termId = 3, programId = 1, currentSemester = 1;

            int studentNum = GetNextStudentNum();
            Student student = new Student
            {
                StudentNum = $"test{studentNum}",
                StudentLastName = "LName",
                StudentFirstMidName = "FName",
                StudentEmail = "test",
                StudentHasHolds = false,
                StudentAcademicStanding = true,
                StudentCanRegister = true,
                StudentStartDate = registrationDate
            };

            CentennialDA dao = new CentennialDA();
            dao.CreateStudent(ref student);
            int studentId = student.StudentId;
            dao.ProgramRegistrationNew(termId, programId, currentSemester, studentId);
            dao.CreateTranscript(studentId);

            // enroll in courses
            int coursesToEnroll = 3;
            foreach (TermCourseOption o in dao.GetTermCourses(termId))
            {
                if (coursesToEnroll == 0) break;
                int sectionId = dao.GetSectionsForTermCourse(o.TermCourseId)[0].SectionId;
                dao.AddEnrollment(studentId, sectionId);
                coursesToEnroll--;
            }

            int numberEnrolledCourses = dao.GetNumberEnrolledCourses(studentId, termId);
            Assert.AreEqual(3, numberEnrolledCourses);

            numberEnrolledCourses = dao.GetStudentTranscript(studentId).history[0].records[0].institution.terms[0].courses.Count;
            Assert.AreEqual(3, numberEnrolledCourses);

            numberEnrolledCourses = dao.GetInvoiceInfo(studentId, termId).Invoice.tuition.Count;
            Assert.AreEqual(3, numberEnrolledCourses);

            bool hasRegistration = dao.HasValidRegistration(studentId, termId);
            Assert.IsTrue(hasRegistration);

            // cancel registration
            dao.CancelRegistration(studentId, termId);

            numberEnrolledCourses = dao.GetNumberEnrolledCourses(studentId, termId);
            Assert.AreEqual(0, numberEnrolledCourses);

            numberEnrolledCourses = dao.GetStudentTranscript(studentId).history[0].records[0].institution.terms[0].courses.Count;
            Assert.AreEqual(0, numberEnrolledCourses);
            
            Assert.IsNull(dao.GetInvoiceInfo(studentId, termId));

            hasRegistration = dao.HasValidRegistration(studentId, termId);
            Assert.IsFalse(hasRegistration);
        }
    }
}
