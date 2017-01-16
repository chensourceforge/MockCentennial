using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockCentennial.Models;
using MockCentennial.Models.TranscriptModel;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for issue #4 - program transfer
    /// </summary>
    [TestClass]
    public class Issue4Tests
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
        public void TransferStudentProgramTest()
        {
            // setup student and program
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

            Assert.AreEqual(programId, dao.GetCurrentProgramId(studentId));

            currentSemester = dao.GetCurrentSemesterInStudentProgram(studentId, programId);
            Assert.AreEqual(1, currentSemester);

            // add credits to student's record
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(1, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(2, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(3, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(5, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(6, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(7, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(8, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(17, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(18, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(19, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(21, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(23, "York", "A"));
            dao.AddTransferCredit(studentId, dao.ConvertToTransferCredit(24, "York", "A"));

            // transfer to new program
            programId = 2;
            termId = 13;
            currentSemester = dao.GetCurrentSemesterInStudentProgram(studentId, programId);
            Assert.AreEqual(3, currentSemester);

            Transcript transcript = dao.GetStudentTranscript(studentId);
            string oldProgram = transcript.history[0].program;

            dao.TransferStudentToProgram(termId, studentId, programId, currentSemester);

            transcript = dao.GetStudentTranscript(studentId);
            string newProgram = transcript.history[0].program;
            Assert.AreNotEqual(oldProgram, newProgram);

            Assert.AreEqual(programId, dao.GetCurrentProgramId(studentId));
        }
    }
}
