using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockCentennial.Models;
using MockCentennial.Models.TranscriptModel;

namespace UnitTests
{
    [TestClass]
    public class CreateStudentTests
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

        private int GetLastStudentNum()
        {
            if (File.Exists(testVariablesFile))
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(testVariablesFile)))
                {
                    return reader.ReadInt32();
                }
            }
            return 0;
        }

        [TestMethod]
        public void NewStudentRegistrationTest()
        {
            DateTime registrationDate = new DateTime(2013, 8, 20);
            int termId = 3, programId = 1, currentSemester = 1;

            int studentNum = GetNextStudentNum();
            Student student=new Student
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

            CentennialDA dao=new CentennialDA();
            dao.CreateStudent(ref student);

            int studentId = student.StudentId;
            Assert.AreNotEqual(0, studentId);

            Console.WriteLine($"StudentId for freshly created student: {studentId}");

            dao.ProgramRegistrationNew(termId, programId, currentSemester, studentId);
            bool programRegistrationDone = dao.HasValidRegistration(studentId, termId);
            Assert.IsTrue(programRegistrationDone);

            dao.CreateTranscript(studentId);
            Transcript transcript = dao.GetStudentTranscript(studentId);
            Assert.AreEqual($"test{studentNum}", transcript.studentNumber);
        }

        [TestMethod]
        public void RegisterStudentForReAdmissionTest()
        {
            int termId = 13;

            int studentNum = GetLastStudentNum();
            Assert.AreNotEqual(0,studentNum);

            CentennialDA dao = new CentennialDA();
            Student student = dao.FindStudentByStudentNum($"test{studentNum}");
            Assert.IsNotNull(student);

            int studentId = student.StudentId;

            student.StudentCanRegister = false;
            dao.UpdateStudent(student);
            bool registrationEnabled = dao.StudentCanRegister(studentId);
            Assert.IsFalse(registrationEnabled);
            
            bool hasRegistration = dao.HasValidRegistration(studentId, termId);
            Assert.IsFalse(hasRegistration);

            dao.EnableStudentCanRegister(studentId);
            registrationEnabled = dao.StudentCanRegister(studentId);
            Assert.IsTrue(registrationEnabled);

            dao.ProgramRegistrationReAdmit(termId, studentId);
            hasRegistration = dao.HasValidRegistration(studentId, termId);
            Assert.IsTrue(hasRegistration);
        }

    }

    
}
