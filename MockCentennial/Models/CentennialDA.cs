using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Configuration;
using MockCentennial.Models.EnrollmentModel;
using MockCentennial.Models.SchoolModel;
using MockCentennial.Models.TimetableModel;
using MockCentennial.Models.TranscriptModel;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;


namespace MockCentennial.Models
{
    /// <summary>
    /// Data access class with utility methods
    /// </summary>
    public class CentennialDA
    {
        private string CONNECTION_STR;

        public CentennialDA()
        {
            CONNECTION_STR = "Data Source=(local);Initial Catalog=RegistrationMockup;Integrated Security=true";
        }
        
        /// <summary>
        /// Look up user in Users table.
        /// </summary>
        /// <param name="UserNum">User ID</param>
        /// <param name="UserPassword">Password</param>
        /// <returns>UserId and UserIsStudent, if user is found; null otherwise.</returns>
        public Tuple<int, bool> FindUserByLogin(string UserNum, string UserPassword)
        {
            Tuple<int, bool> result = null;

            string queryStr = "select * from users where usernum=@UserNum";

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                using (SqlCommand cmd = new SqlCommand(queryStr, conn))
                {
                    cmd.Parameters.Add("@UserNum", SqlDbType.VarChar).Value = UserNum;

                    SqlDataReader reader = null;
                    try
                    {
                        conn.Open();
                        reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        if (reader.Read() && UserPassword.Equals((string)reader["UserPassword"]))
                        {
                            result = new Tuple<int, bool>((int) reader["UserId"], (bool) reader["UserIsStudent"]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }
                    finally
                    {
                        reader?.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Look up student in Student table.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns>Student model WITHOUT transcript, or null if no student.</returns>
        public Student FindStudentById(int StudentId)
        {
            Student result = null;
            string queryStr = "select * from student where studentid=@StudentId";

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                using (SqlCommand cmd = new SqlCommand(queryStr, conn))
                {
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;

                    SqlDataReader reader = null;
                    try
                    {
                        conn.Open();
                        reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        if (reader.Read())
                        {
                            result = new Student();
                            result.StudentId = (int)reader["StudentId"];
                            result.StudentNum = (string)reader["StudentNum"];
                            result.StudentLastName = (string)reader["StudentLastName"];
                            result.StudentFirstMidName = (string)reader["StudentFirstMidName"];
                            result.StudentEmail = (string)reader["StudentEmail"];
                            result.StudentHasHolds = (bool)reader["StudentHasHolds"];
                            result.StudentAcademicStanding = (bool)reader["StudentAcademicStanding"];
                            result.StudentCanRegister = (bool)reader["StudentCanRegister"];
                            result.StudentStartDate = (DateTime) reader["StudentStartDate"];
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }
                    finally
                    {
                        reader?.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Look up student in Student table.
        /// </summary>
        /// <param name="StudentNum"></param>
        /// <returns>Student model WITHOUT transcript, or null if no student.</returns>
        public Student FindStudentByStudentNum(string StudentNum)
        {
            Student result = null;
            string queryStr = "select * from student where studentnum=@StudentNum";

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                using (SqlCommand cmd = new SqlCommand(queryStr, conn))
                {
                    cmd.Parameters.Add("@StudentNum", SqlDbType.VarChar).Value = StudentNum;

                    SqlDataReader reader = null;
                    try
                    {
                        conn.Open();
                        reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                        if (reader.Read())
                        {
                            result = new Student();
                            result.StudentId = (int)reader["StudentId"];
                            result.StudentNum = (string)reader["StudentNum"];
                            result.StudentLastName = (string)reader["StudentLastName"];
                            result.StudentFirstMidName = (string)reader["StudentFirstMidName"];
                            result.StudentEmail = (string)reader["StudentEmail"];
                            result.StudentHasHolds = (bool)reader["StudentHasHolds"];
                            result.StudentAcademicStanding = (bool)reader["StudentAcademicStanding"];
                            result.StudentCanRegister = (bool)reader["StudentCanRegister"];
                            result.StudentStartDate = (DateTime)reader["StudentStartDate"];
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                    }
                    finally
                    {
                        reader?.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Insert row in Users and Student tables, with default password.
        /// </summary>
        /// <param name="student">StudentId will be populated by this method.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        public bool CreateStudent(ref Student student)
        {
            bool success = false;
            string UserPassword = "password";
            bool UserIsStudent = true;

            string insertUserStr =
                "insert into users(UserNum, UserPassword, UserIsStudent) " +
                "values(@UserNum, @UserPassword, @UserIsStudent)";

            string getUserIdStr = "select userid from users where usernum=@UserNum";

            string insertStudentStr =
                "insert into student(StudentId, StudentNum, StudentLastName, StudentFirstMidName, StudentEmail, StudentHasHolds, StudentAcademicStanding, StudentCanRegister, StudentStartDate) " +
                "values(@StudentId, @StudentNum, @StudentLastName, @StudentFirstMidName, @StudentEmail, @StudentHasHolds, @StudentAcademicStanding, @StudentCanRegister, @StudentStartDate)";

            using (SqlConnection connection = new SqlConnection(CONNECTION_STR))
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();
                        cmd.Transaction = transaction;

                        // insert row in Users
                        cmd.CommandText = insertUserStr;
                        cmd.Parameters.Add("@UserNum", SqlDbType.VarChar).Value = student.StudentNum;
                        cmd.Parameters.Add("@UserPassword", SqlDbType.VarChar).Value = UserPassword;
                        cmd.Parameters.Add("@UserIsStudent", SqlDbType.Bit).Value = UserIsStudent;
                        cmd.ExecuteNonQuery();

                        // get UserId from Users
                        cmd.CommandText = getUserIdStr;
                        student.StudentId = (int)cmd.ExecuteScalar();

                        // insert row in Student
                        cmd.CommandText = insertStudentStr;
                        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = student.StudentId;
                        cmd.Parameters.Add("@StudentNum", SqlDbType.VarChar).Value = student.StudentNum;
                        cmd.Parameters.Add("@StudentLastName", SqlDbType.VarChar).Value = student.StudentLastName;
                        cmd.Parameters.Add("@StudentFirstMidName", SqlDbType.VarChar).Value = student.StudentFirstMidName;
                        cmd.Parameters.Add("@StudentEmail", SqlDbType.VarChar).Value = student.StudentEmail;
                        cmd.Parameters.Add("@StudentHasHolds", SqlDbType.Bit).Value = student.StudentHasHolds;
                        cmd.Parameters.Add("@StudentAcademicStanding", SqlDbType.Bit).Value = student.StudentAcademicStanding;
                        cmd.Parameters.Add("@StudentCanRegister", SqlDbType.Bit).Value = student.StudentCanRegister;
                        cmd.Parameters.Add("@StudentStartDate", SqlDbType.DateTime).Value = student.StudentStartDate;
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        success = true;
                    }
                    catch (Exception exception)
                    {
                        transaction?.Rollback();
                        Debug.WriteLine(exception);
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Gets StudentProgramId for current student program. 
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        public int? GetStudentProgramId(int StudentId)
        {
            string sql = "select StudentProgramId from studentprogram where studentid=@StudentId and EndDate is null";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                try
                {
                    conn.Open();
                    return (int?)cmd.ExecuteScalar();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks for valid record in Registration table for the specified term
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <returns>true if there is valid registration, false otherwise</returns>
        public bool HasValidRegistration(int StudentId, int TermId)
        {
            int? studentProgramId = GetStudentProgramId(StudentId);
            if (studentProgramId == null)
            {
                return false;
            }

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select count(*) from registration where termid=@TermId and studentprogramid=@StudentProgramId and DateRegistrationCancelled is null";
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = studentProgramId.Value;
                try
                {
                    conn.Open();
                    if ((int) cmd.ExecuteScalar() == 1)
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }

            return false;
        }

        /// <summary>
        /// Reads and returns value from StudentCanRegister column in Student table
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        public bool StudentCanRegister(int StudentId)
        {
            bool result = false;
            string sql = "select StudentCanRegister from student where studentid=@StudentId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                conn.Open();
                result = (bool) cmd.ExecuteScalar();
            }
            return result;
        }

        /// <summary>
        /// Updates StudentCanRegister column to 1 (true)
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        public bool EnableStudentCanRegister(int StudentId)
        {
            string sql = "update student set StudentCanRegister=1 where studentid=@StudentId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates new program registration record, by inserting rows in StudentProgram, Registration tables.
        /// </summary>
        /// <param name="TermId"></param>
        /// <param name="ProgramId"></param>
        /// <param name="CurrentSemester"></param>
        /// <param name="StudentId"></param>
        /// <returns>Returns success status.</returns>
        public bool ProgramRegistrationNew(int TermId, int ProgramId, int CurrentSemester, int StudentId)
        {
            bool success = false;

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                SqlTransaction txn = null;
                try
                {
                    conn.Open();
                    txn = conn.BeginTransaction();
                    cmd.Transaction = txn;

                    // create row in StudentProgram, return StudentProgramId
                    cmd.CommandText =
                        "insert into studentprogram(StudentId, ProgramId, StartDate, CurrentSemester) " +
                        "values(@StudentId, @ProgramId, @StartDate, @CurrentSemester); " +
                        "SELECT CAST(scope_identity() AS int)";
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                    cmd.Parameters.Add("@ProgramId", SqlDbType.Int).Value = ProgramId;
                    cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@CurrentSemester", SqlDbType.SmallInt).Value = CurrentSemester;

                    int StudentProgramId = (int) cmd.ExecuteScalar();

                    // create row in Registration, using StudentProgramId
                    cmd.CommandText =
                        "insert into registration(TermId, StudentProgramId) values(@TermId, @StudentProgramId)";
                    cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                    cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = StudentProgramId;
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        txn.Commit();
                        success = true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    txn?.Rollback();
                }
            }

            return success;
        }

        /// <summary>
        /// Creates registration record for returning student with an existing program.
        /// </summary>
        /// <param name="TermId"></param>
        /// <param name="ProgramId"></param>
        /// <param name="StudentId"></param>
        /// <returns>Status of success</returns>
        public bool ProgramRegistrationReAdmit(int TermId, int StudentId)
        {
            bool result = false;

            int? StudentProgramId = GetStudentProgramId(StudentId);
            if (StudentProgramId == null)
            {
                return result;
            }
            
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                        "insert into registration(TermId, StudentProgramId) values(@TermId, @StudentProgramId)";
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = StudentProgramId;
                try
                {
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        result = true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return result;
        }

        /// <summary>
        /// End dates old records in StudentProgram and Registration tables, then inserts new records in those tables
        /// </summary>
        /// <param name="TermId"></param>
        /// <param name="StudentId"></param>
        /// <param name="ProgramId"></param>
        /// <param name="CurrentSemester"></param>
        /// <returns></returns>
        public bool ProgramRegistrationTransfer(int TermId, int StudentId, int ProgramId, int CurrentSemester)
        {
            int studentProgramId = GetStudentProgramId(StudentId).Value;
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                {
                    // cancel existing StudentProgram and Registration
                    cmd.CommandText = "update registration set DateRegistrationCancelled=getdate() where StudentProgramId=@StudentProgramId and termid=@TermId and DateRegistrationCancelled is null";
                    cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = studentProgramId;
                    cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "update studentprogram set enddate=getdate() where studentprogramid=@StudentProgramId";
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        return false;
                    }

                    // create new StudentProgram and Registration
                    cmd.CommandText = "insert into studentprogram(StudentId,ProgramId,StartDate,CurrentSemester) values(@StudentId,@ProgramId,getdate(),@CurrentSemester); SELECT CAST(scope_identity() AS int)";
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                    cmd.Parameters.Add("@ProgramId", SqlDbType.Int).Value = ProgramId;
                    cmd.Parameters.Add("@CurrentSemester", SqlDbType.Int).Value = CurrentSemester;
                    studentProgramId = (int) cmd.ExecuteScalar();

                    cmd.CommandText = "insert into registration(TermId, StudentProgramId) values(@TermId, @StudentProgramId)";
                    cmd.Parameters["@StudentProgramId"].Value = studentProgramId;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
            }
        }

        private Record CreateRecordForTranscript(string level)
        {
            Record record = new Record();
            record.level = level;
            Institution institution = new Institution();
            institution.totals = new Totals
            {
                attemptHours = 0,
                passedHours = 0,
                earnedHours = 0,
                gpaHours = 0,
                qualityPoints = 0,
                gpa = 0
            };
            institution.terms = new List<Term>();
            record.institution = institution;
            Transfer transfer = new Transfer();
            transfer.totals = new Totals
            {
                attemptHours = 0,
                passedHours = 0,
                earnedHours = 0,
                gpaHours = 0,
                qualityPoints = 0,
                gpa = 0
            };
            transfer.schools = new List<School>();
            record.transfer = transfer;

            return record;
        }

        private Course CreateCourseForTranscript(TransferCredit tc)
        {
            Course course = new Course();
            course.subject = tc.subject;
            course.course = tc.course;
            course.level = tc.level;
            course.title = tc.title;
            course.gradeLetter = tc.gradeLetter;
            course.creditHours = tc.creditHours;
            course.qualityPoints = 0;
            course.r = "";
            return course;
        }
        private School CreateSchoolForTranscript(TransferCredit tc)
        {
            School school=new School();
            school.school = tc.school;

            Course course = CreateCourseForTranscript(tc);
            school.courses=new List<Course>() {course};

            return school;
        }


        /// <summary>
        /// Creates initial bare-bone transcript and stores it as JSON text.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        public bool CreateTranscript(int StudentId)
        {
            bool success = false;

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                SqlTransaction txn = null;
                SqlDataReader reader = null;
                
                try
                {
                    conn.Open();
                    txn = conn.BeginTransaction();
                    cmd.Transaction = txn;

                    string sqlGetInitialValues =
                        "select s.StudentNum as studentNumber, CONCAT(s.StudentFirstMidName, ' ', s.StudentLastName) as studentName, p.DegreeName as degreeSought, p.CampusName as campus, p.DepartmentName as department, p.ProgramName as program, p.DegreeLevel as level " +
                        "from student s join studentprogram sp on s.studentid=sp.studentid join programinfo p on p.programid=sp.programid " +
                        "where s.StudentId=@StudentId and sp.enddate is null";
                    cmd.CommandText = sqlGetInitialValues;
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                    reader = cmd.ExecuteReader();

                    Transcript transcript = null;
                    if (reader.Read())
                    {
                        transcript = new Transcript();
                        transcript.studentNumber = (string) reader["studentNumber"];
                        transcript.studentName = (string) reader["studentName"];

                        History history = new History();
                        history.degreeSought = (string) reader["degreeSought"];
                        history.campus = (string) reader["campus"];
                        history.department = (string) reader["department"];
                        history.program = (string) reader["program"];

                        Record record = CreateRecordForTranscript((string) reader["level"]);
                        history.records = new List<Record>() { record };

                        transcript.history = new List<History>() {history};
                    }
                    else
                    {
                        return success;
                    }

                    reader.Close();

                    string sqlSetTranscriptField =
                        "UPDATE student SET studenttranscript=@StudentTranscript WHERE studentid=@StudentId";
                    cmd.CommandText = sqlSetTranscriptField;
                    cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value =
                        JsonConvert.SerializeObject(transcript);

                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        txn.Commit();
                        success = true;
                    }
                }
                catch (Exception exception)
                {
                    txn?.Rollback();
                    Debug.WriteLine(exception);
                }
                finally
                {
                    reader?.Dispose();
                }
            }
            return success;
        }


        /// <summary>
        /// Constructs and returns Registration object (NOT the database table Registration)
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns>A populated Registration object</returns>
        public Registration GetRegistrationInfo(int StudentId)
        {
            string sql = "select * from registrationinfo where studentid=@StudentId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Registration regInfo = new Registration();
                            regInfo.StudentId = (int) reader["StudentId"];
                            regInfo.StudentNum = (string) reader["StudentNum"];
                            regInfo.StudentFirstMidName = (string) reader["StudentFirstMidName"];
                            regInfo.StudentLastName = (string)reader["StudentLastName"];
                            regInfo.StudentHasHolds = (bool)reader["StudentHasHolds"];
                            regInfo.StudentAcademicStanding = (bool)reader["StudentAcademicStanding"];
                            regInfo.StudentCanRegister = (bool)reader["StudentCanRegister"];
                            regInfo.StudentStartDate = (DateTime)reader["StudentStartDate"];
                            regInfo.DegreeName = (string)reader["DegreeName"];
                            regInfo.DegreeLevel = (string)reader["DegreeLevel"];
                            regInfo.ProgramName = (string)reader["ProgramName"];
                            regInfo.CampusName = (string)reader["CampusName"];
                            regInfo.DepartmentName = (string)reader["DepartmentName"];
                            regInfo.TermName = (string) reader["TermName"];
                            regInfo.ProgramSemesterName = (string)reader["ProgramSemesterName"];

                            double? transferCredit = null, institutionalCredit = null;
                            if (!Convert.IsDBNull(reader["StudentTranscript"]))
                            {
                                Transcript transcript = JsonConvert.DeserializeObject<Transcript>((string)reader["StudentTranscript"]);
                                if (transcript?.history != null)
                                {
                                    foreach (History h in transcript.history)
                                    {
                                        if (h.degreeSought == regInfo.DegreeName)
                                        {
                                            if (h.records != null)
                                            {
                                                foreach (Record r in h.records)
                                                {
                                                    if (r.level == regInfo.DegreeLevel)
                                                    {
                                                        transferCredit = r.transfer?.totals?.earnedHours;
                                                        institutionalCredit = r.institution?.totals?.earnedHours;
                                                        break;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            regInfo.TransferCredit = transferCredit ?? 0;
                            regInfo.InstitutionalCredit = institutionalCredit ?? 0;

                            return regInfo;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }

            return null;
        }

        private string GetCurrentDegreeName(string StudentNum)
        {
            string result;
            string sql = "select DegreeName from RegistrationInfo where StudentNum=@StudentNum";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentNum", SqlDbType.VarChar).Value = StudentNum;
                conn.Open();
                result = (string) cmd.ExecuteScalar();
            }
            return result;
        }

        private History GetCurrentHistoryInTranscript(Transcript transcript)
        {
            string degreeName = GetCurrentDegreeName(transcript.studentNumber);
            foreach (History history in transcript.history)
            {
                if (history.degreeSought == degreeName)
                {
                    return history;
                }
            }
            return null;
        }
        private bool InsertTransferCreditInTranscript(ref Transcript transcript, TransferCredit tc)
        {
            List<Record> records = GetCurrentHistoryInTranscript(transcript).records;
            int iRecord = records.Count - 1;
            while (iRecord >= 0 && tc.level != records[iRecord].level)
            {
                iRecord--;
            }
            if (iRecord < 0)
            {
                //create record
                Record record = CreateRecordForTranscript(tc.level);
                record.transfer.schools.Add(CreateSchoolForTranscript(tc));
                record.transfer.totals.attemptHours += tc.creditHours;
                record.transfer.totals.passedHours += tc.creditHours;
                record.transfer.totals.earnedHours += tc.creditHours;
                records.Add(record);
                return true;
            }

            List<School> schools = records[iRecord].transfer.schools;
            int iSchool = schools.Count - 1;
            while (iSchool >= 0 && tc.school != schools[iSchool].school)
            {
                iSchool--;
            }
            if (iSchool < 0)
            {
                //create school
                schools.Add(CreateSchoolForTranscript(tc));
                records[iRecord].transfer.totals.attemptHours += tc.creditHours;
                records[iRecord].transfer.totals.passedHours += tc.creditHours;
                records[iRecord].transfer.totals.earnedHours += tc.creditHours;
                return true;
            }

            // check duplicate course
            List<Course> courses = schools[iSchool].courses;
            int iCourse = courses.Count - 1;
            while (iCourse >= 0 && (courses[iCourse].subject != tc.subject || courses[iCourse].course != tc.course))
            {
                iCourse--;
            }
            if (iCourse < 0)
            {
                courses.Add(CreateCourseForTranscript(tc));
                records[iRecord].transfer.totals.attemptHours += tc.creditHours;
                records[iRecord].transfer.totals.passedHours += tc.creditHours;
                records[iRecord].transfer.totals.earnedHours += tc.creditHours;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add transfer credit to database
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="transferCredit"></param>
        /// <returns></returns>
        public bool AddTransferCredit(int StudentId, TransferCredit transferCredit)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    conn.Open();
                    using(SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                    {
                        Transcript transcript = null;

                        // get transcript from column
                        cmd.CommandText = "select StudentTranscript from student where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                            }
                        }

                        // modify transcript object in memory
                        if (transcript==null || !InsertTransferCreditInTranscript(ref transcript, transferCredit))
                        {
                            return false;
                        }

                        // send modified transcript object to column
                        cmd.CommandText =
                            "update student set StudentTranscript=@StudentTranscript where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value =
                            JsonConvert.SerializeObject(transcript);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        /// <summary>
        /// In memory modification to remove an institution course from transcript
        /// </summary>
        /// <param name="transcript"></param>
        /// <param name="degreeName"></param>
        /// <param name="courseLevel"></param>
        /// <param name="termName"></param>
        /// <param name="courseSubject"></param>
        /// <param name="courseNum"></param>
        private void RemoveCourseFromTranscript(ref Transcript transcript, string degreeName, string courseLevel, string termName, string courseSubject, string courseNum)
        {
            foreach (History history in transcript.history)
            {
                if (history.degreeSought != degreeName)
                {
                    continue;
                }

                foreach (Record record in history.records)
                {
                    if (record.level != courseLevel)
                    {
                        continue;
                    }
                    foreach (Term term in record.institution.terms)
                    {
                        if (term.term != termName)
                        {
                            continue;
                        }
                        int index =
                            term.courses.FindIndex(
                                c =>
                                    c.subject == courseSubject &&
                                    c.course == courseNum);
                        if (index != -1)
                        {
                            term.courses.RemoveAt(index);
                        }
                        break;
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// In memory modification to remove a course from invoice
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="courseCode"></param>
        private void RemoveCourseFromInvoice(ref Invoice invoice, string courseCode)
        {
            int index = invoice.tuition.FindIndex(c => c.courseCode.Equals(courseCode));
            if (index != -1)
            {
                invoice.tuition.RemoveAt(index);
            }
            UpdateInvoiceTotal(ref invoice);
        }

        /// <summary>
        /// Deletes enrollment record in Enrollment table, 
        /// updates studenttranscript column in Student table, 
        /// and updates invoice column in Registration table
        /// </summary>
        /// <param name="EnrollmentId">EnrollmentId to delete</param>
        /// <param name="cmd">SqlCommand to use for the database operations</param>
        public void DeleteEnrollment(int EnrollmentId, SqlCommand cmd)
        {
            // call stored procedure to get data needed for delete
            cmd.CommandText = "GetDataForDeleteEnrollment";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters["@EnrollmentId"].Value = EnrollmentId;

            cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@DegreeName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseLevel", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@TermName", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseSubject", SqlDbType.VarChar, 4).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseNum", SqlDbType.VarChar, 3).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseCode", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
            
            cmd.ExecuteNonQuery();

            int studentId = (int) cmd.Parameters["@StudentId"].Value;
            Transcript transcript = JsonConvert.DeserializeObject<Transcript>((string) cmd.Parameters["@StudentTranscript"].Value);
            string degreeName = (string) cmd.Parameters["@DegreeName"].Value;
            string courseLevel = (string) cmd.Parameters["@CourseLevel"].Value;
            string termName = (string)cmd.Parameters["@TermName"].Value;
            string courseSubject = (string)cmd.Parameters["@CourseSubject"].Value;
            string courseNum = (string)cmd.Parameters["@CourseNum"].Value;
            int registrationId = (int)cmd.Parameters["@RegistrationId"].Value;
            Invoice invoice = JsonConvert.DeserializeObject<Invoice>((string)cmd.Parameters["@Invoice"].Value);
            string courseCode = (string) cmd.Parameters["@CourseCode"].Value;

            // modify transcript and invoice in memory
            RemoveCourseFromTranscript(ref transcript, degreeName, courseLevel, termName, courseSubject, courseNum);
            RemoveCourseFromInvoice(ref invoice, courseCode);

            // call stored procedure to update and delete
            cmd.CommandText = "DoDeleteEnrollment";
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar,-1).Direction = ParameterDirection.Input;

            cmd.Parameters["@EnrollmentId"].Value = EnrollmentId;
            cmd.Parameters["@StudentId"].Value = studentId;
            cmd.Parameters["@RegistrationId"].Value = registrationId;
            cmd.Parameters["@StudentTranscript"].Value = JsonConvert.SerializeObject(transcript);
            cmd.Parameters["@Invoice"].Value = JsonConvert.SerializeObject(invoice);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes enrollment record in Enrollment table, 
        /// updates studenttranscript column in Student table, 
        /// and updates invoice column in Registration table
        /// </summary>
        /// <param name="EnrollmentId"></param>
        /// <returns></returns>
        public bool DeleteEnrollment(int EnrollmentId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();
                    DeleteEnrollment(EnrollmentId, cmd);
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        /// <summary>
        /// Adds new course by inserting new enrollment record, and updating student transcript and invoice
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="SectionId"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool AddEnrollment(int StudentId, int SectionId, SqlCommand cmd)
        {
            // call stored procedure to get data needed for adding an enrollment
            cmd.CommandText = "GetDataForAddEnrollment";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@SectionId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters["@StudentId"].Value = StudentId;
            cmd.Parameters["@SectionId"].Value = SectionId;

            cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@DegreeName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseLevel", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@TermName", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseSubject", SqlDbType.VarChar, 4).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseNum", SqlDbType.VarChar, 3).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseTitle", SqlDbType.VarChar, 35).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseCredits", SqlDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@CourseCode", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();

            // modify transcript in memory
            Transcript transcript = JsonConvert.DeserializeObject<Transcript>((string)cmd.Parameters["@StudentTranscript"].Value);
            string degreeName = (string)cmd.Parameters["@DegreeName"].Value;
            double courseCredits = Convert.ToDouble((decimal) cmd.Parameters["@CourseCredits"].Value);
            InstitutionalCredit institutionalCredit = new InstitutionalCredit
            {
                term = (string)cmd.Parameters["@TermName"].Value,
                subject = (string)cmd.Parameters["@CourseSubject"].Value,
                course = (string)cmd.Parameters["@CourseNum"].Value,
                level = (string)cmd.Parameters["@CourseLevel"].Value,
                title = (string)cmd.Parameters["@CourseTitle"].Value,
                creditHours = courseCredits
            };
            if (!InsertInstitutionalCreditInTranscript(ref transcript, institutionalCredit, degreeName))
            {
                return false;
            }

            // modify invoice in memory
            Invoice invoice = JsonConvert.DeserializeObject<Invoice>((string)cmd.Parameters["@Invoice"].Value);
            string courseCode = (string)cmd.Parameters["@CourseCode"].Value;
            invoice.tuition.Add(NewInvoiceCourse(courseCode, courseCredits));
            UpdateInvoiceTotal(ref invoice);

            int registrationId = (int)cmd.Parameters["@RegistrationId"].Value;
            
            // call stored procedure to update database
            cmd.CommandText = "DoAddEnrollment";
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@SectionId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Input;
            cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Input;

            cmd.Parameters["@SectionId"].Value = SectionId;
            cmd.Parameters["@StudentId"].Value = StudentId;
            cmd.Parameters["@RegistrationId"].Value = registrationId;
            cmd.Parameters["@StudentTranscript"].Value = JsonConvert.SerializeObject(transcript);
            cmd.Parameters["@Invoice"].Value = JsonConvert.SerializeObject(invoice);

            cmd.ExecuteNonQuery();
            return true;
        }

        /// <summary>
        /// Adds new course by inserting new enrollment record, and updating student transcript and invoice
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="SectionId"></param>
        /// <returns></returns>
        public bool AddEnrollment(int StudentId, int SectionId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(null,conn,transaction))
                    {
                        try
                        {
                            if (AddEnrollment(StudentId, SectionId, cmd))
                            {
                                transaction.Commit();
                                return true;
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception);
                            transaction.Rollback();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        private Course CreateCourseForTranscript(InstitutionalCredit ic)
        {
            Course course = new Course();
            course.subject = ic.subject;
            course.course = ic.course;
            course.level = ic.level;
            course.title = ic.title;
            course.gradeNumeric = ic.gradeNumeric;
            course.gradeLetter = ic.gradeLetter;
            course.creditHours = ic.creditHours;
            course.qualityPoints = ic.qualityPoints;
            course.r = ic.r;
            return course;
        }

        private Term CreateTermForTranscript(InstitutionalCredit ic)
        {
            Term term=new Term();
            term.term = ic.term;

            Course course = CreateCourseForTranscript(ic);
            term.courses=new List<Course>() { course };

            return term;
        }

        private bool InsertInstitutionalCreditInHistory(ref History history, InstitutionalCredit ic)
        {
            List<Record> records = history.records;
            int iRecord = records.Count - 1;
            while (iRecord >= 0 && ic.level != records[iRecord].level)
            {
                iRecord--;
            }
            if (iRecord < 0)
            {
                //create record
                Record record = CreateRecordForTranscript(ic.level);
                record.institution.terms.Add(CreateTermForTranscript(ic));
                records.Add(record);
                return true;
            }

            List<Term> terms = records[iRecord].institution.terms;
            int iTerm = terms.Count - 1;
            while (iTerm >= 0 && ic.term != terms[iTerm].term)
            {
                iTerm--;
            }
            if (iTerm < 0)
            {
                //create term
                terms.Add(CreateTermForTranscript(ic));
                return true;
            }

            // check duplicate course
            List<Course> courses = terms[iTerm].courses;
            int iCourse = courses.Count - 1;
            while (iCourse >= 0 && (courses[iCourse].subject != ic.subject || courses[iCourse].course != ic.course))
            {
                iCourse--;
            }
            if (iCourse < 0)
            {
                courses.Add(CreateCourseForTranscript(ic));
                return true;
            }

            return false;
        }

        /// <summary>
        /// In memory modifications of transcript
        /// </summary>
        /// <param name="transcript"></param>
        /// <param name="ic"></param>
        /// <param name="degreeName"></param>
        /// <returns></returns>
        private bool InsertInstitutionalCreditInTranscript(ref Transcript transcript, InstitutionalCredit ic, string degreeName)
        {
            History history = transcript.history.Find(h => h.degreeSought == degreeName);
            return InsertInstitutionalCreditInHistory(ref history, ic);
        }
        private bool InsertInstitutionalCreditInTranscript(ref Transcript transcript, InstitutionalCredit ic)
        {
            History history = GetCurrentHistoryInTranscript(transcript);
            return InsertInstitutionalCreditInHistory(ref history, ic);
        }

        private bool DeleteInstitutionalCreditInTranscript(ref Transcript transcript,InstitutionalCredit ic)
        {
            bool isModified = false;
            foreach (Record record in GetCurrentHistoryInTranscript(transcript).records)
            {
                if (record.level != ic.level)
                {
                    continue;
                }
                foreach (Term term in record.institution.terms)
                {
                    if (term.term != ic.term)
                    {
                        continue;
                    }
                    int index =
                        term.courses.FindIndex(
                            c =>
                                c.subject == ic.subject &&
                                c.course == ic.course);
                    if (index != -1)
                    {
                        term.courses.RemoveAt(index);
                        isModified = true;
                    }
                    break;
                }
                break;
            }
            return isModified;
        }

        private InstitutionalCredit GetInstitutionalCredit(int TermId, int CourseId)
        {
            InstitutionalCredit institutionalCredit = new InstitutionalCredit();
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "select TermName from term where termid=@TermId";
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                institutionalCredit.term = (string) cmd.ExecuteScalar();

                cmd.CommandText = "select CourseSubject, CourseNum, CourseLevel, CourseTitle, CourseCredits from course where courseid=@CourseId";
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = CourseId;
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        institutionalCredit.subject = (string)reader["CourseSubject"];
                        institutionalCredit.course = (string)reader["CourseNum"];
                        institutionalCredit.level = (string)reader["CourseLevel"];
                        institutionalCredit.title = (string)reader["CourseTitle"];
                        institutionalCredit.creditHours = Convert.ToDouble((decimal)reader["CourseCredits"]);
                    }
                }
            }
            return institutionalCredit;
        }

        /// <summary>
        /// Updates transcript in database by adding the specified new course to it.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermCourseId"></param>
        /// <returns>true if new course is added; false otherwise</returns>
        public bool AddCourseToTranscript(int StudentId, int TermCourseId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                    {
                        // get course info
                        InstitutionalCredit institutionalCredit = null;
                        cmd.CommandText =
                            "select t.TermName, c.CourseSubject, c.CourseNum, c.CourseLevel, c.CourseTitle, c.CourseCredits " +
                            "from course c join termcourse tc on c.CourseId=tc.CourseId join term t on t.TermId=tc.TermId " +
                            "where tc.TermCourseId=@TermCourseId";
                        cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                institutionalCredit = new InstitutionalCredit();
                                institutionalCredit.term = (string) reader["TermName"];
                                institutionalCredit.subject = (string)reader["CourseSubject"];
                                institutionalCredit.course = (string)reader["CourseNum"];
                                institutionalCredit.level = (string)reader["CourseLevel"];
                                institutionalCredit.title = (string)reader["CourseTitle"];
                                institutionalCredit.creditHours = Convert.ToDouble((decimal)reader["CourseCredits"]);
                            }
                        }
                        if (institutionalCredit == null)
                        {
                            return false;
                        }

                        Transcript transcript = null;

                        // get transcript from column
                        cmd.CommandText = "select StudentTranscript from student where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                            }
                        }

                        // modify transcript object in memory
                        if (transcript == null || !InsertInstitutionalCreditInTranscript(ref transcript, institutionalCredit))
                        {
                            return false;
                        }

                        // send modified transcript object to column
                        cmd.CommandText =
                            "update student set StudentTranscript=@StudentTranscript where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value =
                            JsonConvert.SerializeObject(transcript);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        public bool AddCourseToTranscript(int StudentId, int CourseId, int TermId)
        {
            int TermCourseId = 0;
            string sql = "select TermCourseId from TermCourse where CourseId=@CourseId and TermId=@TermId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = CourseId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                TermCourseId = (int) cmd.ExecuteScalar();
            }
            if (TermCourseId == 0)
            {
                return false;
            }
            return AddCourseToTranscript(StudentId, TermCourseId);
        }

        /// <summary>
        /// Updates transcript in database by removing the specified course.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermCourseId"></param>
        /// <returns></returns>
        public bool RemoveCourseFromTranscript(int StudentId, int TermCourseId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                    {
                        // get course info
                        InstitutionalCredit institutionalCredit = null;
                        cmd.CommandText =
                            "select t.TermName, c.CourseSubject, c.CourseNum, c.CourseLevel " +
                            "from course c join termcourse tc on c.CourseId=tc.CourseId join term t on t.TermId=tc.TermId " +
                            "where tc.TermCourseId=@TermCourseId";
                        cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                institutionalCredit = new InstitutionalCredit();
                                institutionalCredit.term = (string)reader["TermName"];
                                institutionalCredit.subject = (string)reader["CourseSubject"];
                                institutionalCredit.course = (string)reader["CourseNum"];
                                institutionalCredit.level = (string)reader["CourseLevel"];
                            }
                        }
                        if (institutionalCredit == null)
                        {
                            return false;
                        }

                        Transcript transcript = null;

                        // get transcript from column
                        cmd.CommandText = "select StudentTranscript from student where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                            }
                        }
                        if (transcript == null)
                        {
                            return false;
                        }

                        // modify transcript object in memory
                        if (!DeleteInstitutionalCreditInTranscript(ref transcript, institutionalCredit))
                        {
                            return false;
                        }

                        // send modified transcript object to column
                        cmd.CommandText =
                            "update student set StudentTranscript=@StudentTranscript where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value =
                            JsonConvert.SerializeObject(transcript);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        public bool RemoveCourseFromTranscript(int StudentId, int CourseId, int TermId)
        {
            int TermCourseId = 0;
            string sql = "select TermCourseId from TermCourse where CourseId=@CourseId and TermId=@TermId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = CourseId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                TermCourseId = (int)cmd.ExecuteScalar();
            }
            if (TermCourseId == 0)
            {
                return false;
            }
            return RemoveCourseFromTranscript(StudentId, TermCourseId);
        }

        /// <summary>
        /// Gets list of TermOption for terms within the specified date range
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns>list containing TermOption</returns>
        public List<TermOption> GetTermOptions(DateTime fromDate, DateTime toDate)
        {
            List<TermOption> list = new List<TermOption>();
            string sql =
                @"select TermId, TermName from Term where TermStartDate >= @fromDate and TermEndDate <= @toDate";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using(SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@fromDate", SqlDbType.Date).Value = fromDate;
                cmd.Parameters.Add("@toDate", SqlDbType.Date).Value = toDate;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TermOption {TermId = (int) reader[0], TermName = (string) reader[1]});
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets list of TermOption for all terms
        /// </summary>
        /// <returns>list containing TermOption</returns>
        public List<TermOption> GetAllTermOptions()
        {
            List<TermOption> list = new List<TermOption>();
            string sql = "select TermId, TermName from Term";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TermOption { TermId = (int)reader[0], TermName = (string)reader[1] });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets all program select options
        /// </summary>
        /// <returns>List containing ProgramOption</returns>
        public List<ProgramOption> GetProgramOptions()
        {
            List<ProgramOption> list = new List<ProgramOption>();
            string sql = @"select ProgramId, ProgramCode, ProgramName, ProgramSemesters from Program order by 3";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ProgramOption
                        {
                            ProgramId = (int)reader["ProgramId"],
                            ProgramCode = (string)reader["ProgramCode"],
                            ProgramName = (string)reader["ProgramName"],
                            ProgramSemesters = (Int16)reader["ProgramSemesters"]
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// gets CourseId,CourseCode,CourseTitle for all courses
        /// </summary>
        /// <returns>list containing CourseId,CourseCode,CourseTitle</returns>
        public List<CourseInfo> GetAllCourses()
        {
            List<CourseInfo> courses=new List<CourseInfo>();
            string sql = "select CourseId,CourseCode,CourseTitle from course order by 2";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using(SqlCommand cmd=new SqlCommand(sql,conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new CourseInfo
                        {
                            CourseId = (int)reader["CourseId"],
                            CourseCode = (string)reader["CourseCode"],
                            CourseTitle = (string)reader["CourseTitle"]
                        });
                    }
                }
            }
            return courses;
        }

        /// <summary>
        /// Gets list of CourseOption for all courses
        /// </summary>
        /// <returns>List containing CourseOption</returns>
        public List<CourseOption> GetAllCourseOptions()
        {
            List<CourseOption> list = new List<CourseOption>();
            string sql = "select CourseId, CourseCode, CourseTitle from course";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CourseOption
                        {
                            CourseId = (int)reader[0],
                            CourseCode = (string)reader[1],
                            CourseTitle = (string)reader[2]
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// gets CourseInfo object for a given CourseId
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns>CourseInfo object</returns>
        public CourseInfo GetCourseInfo(int courseId)
        {
            CourseInfo courseInfo = new CourseInfo();
            
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select * from courseinfo where courseid=@CourseId";
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        courseInfo.CourseId = (int) reader["CourseId"];
                        courseInfo.CourseCode = (string) reader["CourseCode"];
                        courseInfo.CourseTitle = (string)reader["CourseTitle"];
                        courseInfo.CourseSubject = (string)reader["CourseSubject"];
                        courseInfo.CourseNum = (string)reader["CourseNum"];
                        courseInfo.CourseLevel = (string)reader["CourseLevel"];
                        courseInfo.CourseCredits = Convert.ToDouble(reader["CourseCredits"]);
                        courseInfo.Prereqs =
                            JsonConvert.DeserializeObject<List<CoursePrereq>>((string) reader["Prereqs"]);
                    }
                }
            }
            return courseInfo;
        }

        /// <summary>
        /// Updates Course, CoursePrerequisite tables with values from parameter
        /// </summary>
        /// <param name="courseInfo"></param>
        /// <returns>true if successful, false otherwise</returns>
        public bool UpdateCourseInfo(CourseInfo courseInfo)
        {
            string updateCourseStr = new StringBuilder("update course set ").
                Append("CourseCode = @CourseCode,").
                Append("CourseTitle = @CourseTitle,").
                Append("CourseSubject = @CourseSubject,").
                Append("CourseNum = @CourseNum,").
                Append("CourseLevel = @CourseLevel,").
                Append("CourseCredits = @CourseCredits where CourseId = @CourseId").ToString();

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = updateCourseStr;
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = courseInfo.CourseId;
                cmd.Parameters.Add("@CourseCode", SqlDbType.VarChar).Value = courseInfo.CourseCode;
                cmd.Parameters.Add("@CourseTitle", SqlDbType.VarChar).Value = courseInfo.CourseTitle;
                cmd.Parameters.Add("@CourseSubject", SqlDbType.VarChar).Value = courseInfo.CourseSubject;
                cmd.Parameters.Add("@CourseNum", SqlDbType.VarChar).Value = courseInfo.CourseNum;
                cmd.Parameters.Add("@CourseLevel", SqlDbType.VarChar).Value = courseInfo.CourseLevel;
                cmd.Parameters.Add("@CourseCredits", SqlDbType.Decimal).Value = courseInfo.CourseCredits;

                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                cmd.Transaction = transaction;
                // update course
                if (cmd.ExecuteNonQuery() != 1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return false;
                }
                // update prereqs
                cmd.CommandText = "select CoursePrerequisiteId, PrereqCourseId from courseprerequisite where courseid=@CourseId";
                Dictionary<int, int> existingPrereqs = new Dictionary<int, int>();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingPrereqs.Add((int)reader[1], (int)reader[0]);
                    }
                }

                int existing = existingPrereqs.Count;
                int current = courseInfo.PrereqIds?.Count ?? 0;

                // ignore same prereqs
                for (int i = current - 1; i >= 0; i--)
                {
                    int prereqId = courseInfo.PrereqIds[i];
                    if (existingPrereqs.ContainsKey(prereqId))
                    {
                        existingPrereqs.Remove(prereqId);
                        courseInfo.PrereqIds.RemoveAt(i);
                        existing--;
                        current--;
                    }
                }
                // delete excess old prereqs
                cmd.CommandText = "delete from courseprerequisite where CoursePrerequisiteId=@CoursePrerequisiteId";
                cmd.Parameters.Add("@CoursePrerequisiteId", SqlDbType.Int).Value = 0;
                while (existing > current)
                {
                    KeyValuePair<int, int> kvp = existingPrereqs.First();
                    cmd.Parameters["@CoursePrerequisiteId"].Value = kvp.Value;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        return false;
                    }
                    existingPrereqs.Remove(kvp.Key);
                    existing--;
                }
                // insert excess new prereqs
                cmd.CommandText = "insert into courseprerequisite(courseid,prereqcourseid) values(@CourseId, @PrereqCourseId)";
                cmd.Parameters.Add("@PrereqCourseId", SqlDbType.Int).Value = 0;
                while (existing < current)
                {
                    cmd.Parameters["@PrereqCourseId"].Value = courseInfo.PrereqIds[current - 1];
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        return false;
                    }
                    courseInfo.PrereqIds.RemoveAt(current - 1);
                    current--;
                }
                // update changed prereqs
                cmd.CommandText =
                    "update CoursePrerequisite set PrereqCourseId=@PrereqCourseId where CoursePrerequisiteId=@CoursePrerequisiteId";
                int index = 0;
                foreach (var kvp in existingPrereqs)
                {
                    int coursePrerequisiteId = kvp.Value;
                    int prereqCourseId = courseInfo.PrereqIds[index];
                    cmd.Parameters["@PrereqCourseId"].Value = prereqCourseId;
                    cmd.Parameters["@CoursePrerequisiteId"].Value = coursePrerequisiteId;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        return false;
                    }
                    index++;
                }

                transaction.Commit();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts new entries into TermCourse table
        /// </summary>
        /// <param name="TermId"></param>
        /// <param name="CourseIds"></param>
        /// <returns>true if successful, false otherwise</returns>
        public bool AddTermCourses(int TermId, List<int> CourseIds)
        {
            string sql =
                "insert into termcourse(TermId,CourseId,TermCourseIsOffered) values(@TermId,@CourseId,@TermCourseIsOffered)";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                conn.Open();
                using(SqlTransaction transaction = conn.BeginTransaction())
                using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                {
                    cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                    cmd.Parameters.Add("@TermCourseIsOffered", SqlDbType.Bit).Value = true;
                    cmd.Parameters.Add("@CourseId", SqlDbType.Int);
                    foreach (int cid in CourseIds)
                    {
                        cmd.Parameters["@CourseId"].Value = cid;
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    transaction.Commit();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates TermCourse table with provided TermId and list of CourseId
        /// </summary>
        /// <param name="TermId">TermId for the provided term</param>
        /// <param name="CourseIds">List of CourseId to update for the provided term</param>
        /// <returns>Success status</returns>
        public bool UpdateTermCourses(int TermId, List<int> CourseIds)
        {
            bool isSuccess = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = "select CourseId, TermCourseId from TermCourse where TermId=@TermId and TermCourseIsOffered=1";
                        cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;

                        Dictionary<int, int> existingCourseIds = new Dictionary<int, int>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                existingCourseIds.Add(reader.GetInt32(0), reader.GetInt32(1));
                            }
                        }

                        Dictionary<int, int> existingOnly = new Dictionary<int, int>();
                        foreach (var kvp in existingCourseIds)
                        {
                            if (!CourseIds.Contains(kvp.Key))
                            {
                                existingOnly.Add(kvp.Key, kvp.Value);
                            }
                        }

                        List<int> currentOnly = CourseIds.Where(id => !existingCourseIds.ContainsKey(id)).ToList();
                        int existing = existingOnly.Count;
                        int current = currentOnly.Count;

                        if (existing > current)
                        {
                            cmd.Parameters.Add("@TermCourseId", SqlDbType.Int);
                            do
                            {
                                cmd.CommandText = "select sectionid from section where termcourseid=@TermCourseId";
                                KeyValuePair<int, int> temp = existingOnly.Last();
                                cmd.Parameters["@TermCourseId"].Value = temp.Value;
                                List<int> SectionIds = new List<int>();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SectionIds.Add(reader.GetInt32(0));
                                    }
                                }
                                foreach (int sid in SectionIds)
                                {
                                    DeleteSection(sid);
                                }

                                cmd.CommandText = "delete from termcourse where termcourseid=@TermCourseId";
                                if (cmd.ExecuteNonQuery() != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                                existingOnly.Remove(temp.Key);
                                existing--;
                            } while (existing > current);
                        }
                        else if (existing < current)
                        {
                            cmd.CommandText =
                                "insert into termcourse(TermId,CourseId,TermCourseIsOffered) values(@TermId,@CourseId,1)";
                            cmd.Parameters.Add("@CourseId", SqlDbType.Int);
                            do
                            {
                                cmd.Parameters["@CourseId"].Value = currentOnly[current - 1];
                                if (cmd.ExecuteNonQuery() != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                                currentOnly.RemoveAt(current - 1);
                                current--;
                            } while (existing < current);
                        }

                        if (existing > 0)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@TermCourseId", SqlDbType.Int);
                            cmd.Parameters.Add("@CourseId", SqlDbType.Int);
                            
                            foreach (int newId in currentOnly)
                            {
                                cmd.CommandText = "select sectionid from section where termcourseid=@TermCourseId";
                                KeyValuePair<int, int> temp = existingOnly.Last();
                                cmd.Parameters["@TermCourseId"].Value = temp.Value;
                                cmd.Parameters["@CourseId"].Value = newId;
                                List<int> SectionIds = new List<int>();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        SectionIds.Add(reader.GetInt32(0));
                                    }
                                }
                                foreach (int sid in SectionIds)
                                {
                                    DeleteSection(sid);
                                }
                                existingOnly.Remove(temp.Key);

                                cmd.CommandText = "update termcourse set courseid=@CourseId where termcourseid=@TermCourseId";
                                if (cmd.ExecuteNonQuery() != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        transaction.Commit();
                        isSuccess = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            
            return isSuccess;
        }

        /// <summary>
        /// Gets list of courses offered for the given term
        /// </summary>
        /// <param name="TermId">TermId for the given term</param>
        /// <returns>List of CourseId's</returns>
        public List<int> GetCoursesOffered(int TermId)
        {
            List<int> list = new List<int>();
            string sql = "select CourseId from TermCourse where TermId=@TermId and TermCourseIsOffered=1";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((int)reader[0]);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Retrieves the EndDate value for a given term
        /// </summary>
        /// <param name="TermId"></param>
        /// <returns></returns>
        public DateTime GetTermEndDate(int TermId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select TermEndDate from term where TermId=@TermId";
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                return (DateTime) cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Gets list containing TermCourseId, CourseHeading for the specified term
        /// </summary>
        /// <param name="TermId">TermId for specified term</param>
        /// <returns>List of TermCourseOption</returns>
        public List<TermCourseOption> GetTermCourses(int TermId)
        {
            List<TermCourseOption> list=new List<TermCourseOption>();
            string sql =
                "select TermCourseId, CONCAT(CourseCode, ': ', CourseTitle) from termcourseinfo where termid=@TermId and TermCourseIsOffered=1";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TermCourseOption
                        {
                            TermCourseId = reader.GetInt32(0),
                            CourseHeading = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets all SectionOption for specified TermCourseId
        /// </summary>
        /// <param name="TermCourseId"></param>
        /// <returns>list of SectionOption</returns>
        public List<SectionOption> GetSectionOptions(int TermCourseId)
        {
            List<SectionOption> sections = new List<SectionOption>();
            string sql = "select SectionId,SectionNum from section where termcourseid=@TermCourseId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sections.Add(new SectionOption
                        {
                            SectionId = reader.GetInt32(0),
                            SectionNum = reader.GetString(1)
                        });
                    }
                }
            }
            return sections;
        }

        /// <summary>
        /// Retrieves SectionInfo list. Each item represents 1 section (which can have multiple classes).
        /// </summary>
        /// <param name="TermCourseId"></param>
        /// <returns>List of SectionInfo</returns>
        public List<SectionInfo> GetSectionsForTermCourse(int TermCourseId)
        {
            List<SectionInfo> list = new List<SectionInfo>();
            string sql = "select * from SectionClasses where termcourseid=@TermCourseId order by sectionid";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int prevSectionId = 0;
                    List<ClassInfo> classInfos = null;
                    while (reader.Read())
                    {
                        int currSectionId = (int) reader["SectionId"];
                        if (prevSectionId == currSectionId)
                        {
                            // same section, diff class
                            if (!reader.IsDBNull(reader.GetOrdinal("ClassId")))
                            {
                                classInfos.Add(new ClassInfo
                                {
                                    ClassId = (int)reader["ClassId"],
                                    ClassIsLecture = (bool)reader["ClassIsLecture"],
                                    InstructorId = (int)reader["InstructorId"],
                                    RoomId = (int)reader["RoomId"],
                                    ClassStartTime = (int)reader["ClassStartTime"],
                                    ClassEndTime = (int)reader["ClassEndTime"]
                                });
                            }
                        }
                        else
                        {
                            // new section
                            SectionInfo sectionInfo = new SectionInfo();
                            sectionInfo.SectionId = currSectionId;
                            sectionInfo.SectionNum = (string) reader["SectionNum"];
                            classInfos = new List<ClassInfo>();
                            if (!reader.IsDBNull(reader.GetOrdinal("ClassId")))
                            {
                                classInfos.Add(new ClassInfo
                                {
                                    ClassId = (int)reader["ClassId"],
                                    ClassIsLecture = (bool)reader["ClassIsLecture"],
                                    InstructorId = (int)reader["InstructorId"],
                                    RoomId = (int)reader["RoomId"],
                                    ClassStartTime = (int)reader["ClassStartTime"],
                                    ClassEndTime = (int)reader["ClassEndTime"]
                                });
                            }
                            sectionInfo.ClassInfos = classInfos;
                            list.Add(sectionInfo);
                            prevSectionId = currSectionId;
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Retrieves list of RoomOption for all rooms
        /// </summary>
        /// <returns>list of RoomOption</returns>
        public List<RoomOption> GetAllRoomOptions()
        {
            List<RoomOption> list=new List<RoomOption>();
            string sql = "select RoomId, CONCAT(campuscode,': ',building,' ',roomnum) from room";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RoomOption()
                        {
                            RoomId = reader.GetInt32(0),
                            RoomLocation = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        public string ConvertDay(int day)
        {
            switch (day)
            {
                case 1:
                    return "M";
                case 2:
                    return "T";
                case 3:
                    return "W";
                case 4:
                    return "R";
                case 5:
                    return "F";
                case 6:
                    return "S";
                default:
                    return "N";
            }
        }

        public string ConvertTime(int hour, int minute)
        {
            return $"{hour:D2}:{minute:D2}";
        }

        /// <summary>
        /// Retrieves list of TimeOption for all class times
        /// </summary>
        /// <returns>list of TimeOption</returns>
        public List<TimeOption> GetAllTimeOptions()
        {
            List<TimeOption> list = new List<TimeOption>();
            string sql = "select TimeId, day,hour,minute from time";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TimeOption()
                        {
                            TimeId = reader.GetInt32(0),
                            TimeText = $"{ConvertDay(reader.GetInt16(1))} {ConvertTime(reader.GetInt16(2), reader.GetInt16(3))}"
                        });
                        
                    }
                }
            }
            return list;
        }


        /// <summary>
        /// Retrieves list of InstructorOption for all instructors
        /// </summary>
        /// <returns>list of InstructorOption</returns>
        public List<InstructorOption> GetAllInstructorOptions()
        {
            List<InstructorOption> list = new List<InstructorOption>();
            string sql = "select InstructorId, CONCAT(InstructorFirstMidName,' ',InstructorLastName) from instructor";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new InstructorOption()
                        {
                            InstructorId = reader.GetInt32(0),
                            InstructorName = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Inserts new record into Section table, returns SectionId of newly created record
        /// </summary>
        /// <param name="TermCourseId"></param>
        /// <param name="SectionNum"></param>
        /// <returns>SectionId of the new record, or 0 if operation was not successful</returns>
        public int CreateSectionForTermCourse(int TermCourseId, string SectionNum)
        {
            string sql =
                "insert into section(TermCourseId,SectionNum) values(@TermCourseId,@SectionNum); select cast(scope_identity() AS int)";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                cmd.Parameters.Add("@SectionNum", SqlDbType.VarChar).Value = SectionNum;
                conn.Open();
                return (int) cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Updates record in Section with new SectionNum for the specified SectionId.
        /// Column TermCourseId is not updatable after its first assignment.
        /// </summary>
        /// <param name="SectionId"></param>
        /// <param name="SectionNum"></param>
        /// <returns>true if successful, false otherwise</returns>
        public bool UpdateSection(int SectionId, string SectionNum)
        {
            string sql = "update section set sectionnum=@SectionNum where sectionid=@SectionId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@SectionNum", SqlDbType.VarChar).Value = SectionNum;
                cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the specified Section record from Section table.
        /// Also deletes any records in Class and Enrollment linked to the section.
        /// </summary>
        /// <param name="SectionId">SectionId of the record to be deleted</param>
        /// <returns>success status</returns>
        public bool DeleteSection(int SectionId)
        {
            string selectEnrollments = "select EnrollmentId from enrollment where sectionid=@SectionId";
            string deleteClasses = "delete from class where sectionid=@SectionId";
            string deleteSection = "delete from section where sectionid=@SectionId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                using(SqlCommand cmd=new SqlCommand(null,conn,transaction))
                {
                    try
                    {
                        List<int> enrollmentIdsToDelete = new List<int>();
                        cmd.CommandText = selectEnrollments;
                        cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enrollmentIdsToDelete.Add(reader.GetInt32(0));
                            }
                        }
                        foreach (int enrollmentId in enrollmentIdsToDelete)
                        {
                            DeleteEnrollment(enrollmentId, cmd);
                        }

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;

                        cmd.CommandText = deleteClasses;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = deleteSection;
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        transaction.Rollback();
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Inserts a new row in the Class table with specified info
        /// </summary>
        /// <param name="SectionId">SectionId for the class</param>
        /// <param name="classInfo">structure containing class information</param>
        /// <returns>true on success, false otherwise</returns>
        public bool CreateClassForSection(int SectionId, ClassInfo classInfo)
        {
            string sql =
                "insert into class(SectionId,InstructorId,RoomId,ClassIsLecture,ClassStartTime,ClassEndTime) values(@SectionId,@InstructorId,@RoomId,@ClassIsLecture,@ClassStartTime,@ClassEndTime)";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;
                cmd.Parameters.Add("@InstructorId", SqlDbType.Int).Value = classInfo.InstructorId;
                cmd.Parameters.Add("@RoomId", SqlDbType.Int).Value = classInfo.RoomId;
                cmd.Parameters.Add("@ClassIsLecture", SqlDbType.Bit).Value = classInfo.ClassIsLecture;
                cmd.Parameters.Add("@ClassStartTime", SqlDbType.Int).Value = classInfo.ClassStartTime;
                cmd.Parameters.Add("@ClassEndTime", SqlDbType.Int).Value = classInfo.ClassEndTime;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Updates a record in table Class with the provided new values.
        /// SectionId cannot be changed after it's assigned to a class, so it's omitted from update.
        /// </summary>
        /// <param name="classInfo">structure containing field values to be updated</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool UpdateClass(ClassInfo classInfo)
        {
            string sql =
                "update class set instructorid=@InstructorId, roomid=@RoomId, classislecture=@ClassIsLecture, classstarttime=@ClassStartTime, classendtime=@ClassEndTime where classid=@ClassId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@InstructorId", SqlDbType.Int).Value = classInfo.InstructorId;
                cmd.Parameters.Add("@RoomId", SqlDbType.Int).Value = classInfo.RoomId;
                cmd.Parameters.Add("@ClassIsLecture", SqlDbType.Bit).Value = classInfo.ClassIsLecture;
                cmd.Parameters.Add("@ClassStartTime", SqlDbType.Int).Value = classInfo.ClassStartTime;
                cmd.Parameters.Add("@ClassEndTime", SqlDbType.Int).Value = classInfo.ClassEndTime;
                cmd.Parameters.Add("@ClassId", SqlDbType.Int).Value = classInfo.ClassId;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes a record from table Class
        /// </summary>
        /// <param name="ClassId"></param>
        /// <returns>success status</returns>
        public bool DeleteClass(int ClassId)
        {
            string sql = "delete from class where classid=@ClassId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ClassId", SqlDbType.Int).Value = ClassId;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CourseTaken(Transcript transcript, Tuple<string, string> code)
        {
            foreach (History history in transcript.history)
            {
                foreach (Record record in history.records)
                {
                    foreach (Term term in record.institution.terms)
                    {
                        foreach (Course course in term.courses)
                        {
                            if (course.subject == code.Item1 && course.course == code.Item2 &&
                                (course.gradeNumeric ?? 50) >= 50)
                            {
                                return true;
                            }
                        }
                    }
                    foreach (School school in record.transfer.schools)
                    {
                        foreach (Course course in school.courses)
                        {
                            if (course.subject == code.Item1 && course.course == code.Item2)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool PrerequisiteMet(Transcript transcript, List<Prereq> prereqs)
        {
            foreach (Prereq prereq in prereqs)
            {
                bool met = false;
                foreach (History history in transcript.history)
                {
                    foreach (Record record in history.records)
                    {
                        foreach (Term term in record.institution.terms)
                        {
                            foreach (Course course in term.courses)
                            {
                                if (course.subject == prereq.courseSubject && course.course == prereq.courseNum &&
                                    (course.gradeNumeric ?? 0) >= 50)
                                {
                                    // prereq met
                                    met = true;
                                    goto next_prereq;
                                }
                            }
                        }
                        foreach (School school in record.transfer.schools)
                        {
                            foreach (Course course in school.courses)
                            {
                                if (course.subject == prereq.courseSubject && course.course == prereq.courseNum)
                                {
                                    met = true;
                                    goto next_prereq;
                                }
                            }
                        }
                    }
                }
                next_prereq:
                if (!met)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates Enrollment table and Student's transcript and invoice with new enrollment data
        /// </summary>
        /// <param name="StudentId">for student</param>
        /// <param name="TermId">for the term to change</param>
        /// <param name="newEnrollments">Dictionary of CourseId as key, SectionId as value</param>
        /// <returns>success status</returns>
        public bool UpdateEnrollment(int StudentId, int TermId, Dictionary<int,int> newEnrollments)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    cmd.Transaction = transaction;

                    // get existing enrollment info before changes
                    Dictionary<int, int> oldEnrollments = new Dictionary<int, int>();
                    cmd.CommandText = "select CourseId, SectionId from EnrollmentInfo where StudentId=@StudentId and TermId=@TermId";
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                    cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oldEnrollments.Add(reader.GetInt32(0), reader.GetInt32(1));
                        }
                    }

                    // compare new enrollment info vs existing info
                    List<int> coursesToDelete = new List<int>();
                    List<int> coursesToAdd = new List<int>();
                    Transcript transcript = null;
                    bool isTranscriptModified = false;
                    cmd.CommandText = "select studenttranscript from student where studentid=@StudentId";
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                        }
                    }
                    if (transcript == null)
                    {
                        return false;
                    }

                    string updateEnrollment = "update enrollment set sectionid=@newSectionId where studentid=@StudentId and sectionid=@oldSectionId";
                    string insertEnrollment = "insert into enrollment(StudentId,SectionId) values(@StudentId,@newSectionId)";
                    string deleteEnrollment = "delete from enrollment where studentid=@StudentId and sectionid=@oldSectionId";
                    string selectCourseInfo = "select CourseSubject, CourseNum, Prereqs from courseinfo where courseid=@CourseId";

                    cmd.Parameters.Add("@oldSectionId", SqlDbType.Int).Value=0;
                    cmd.Parameters.Add("@newSectionId", SqlDbType.Int).Value=0;
                    cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = 0;

                    foreach (var oldEnrollment in oldEnrollments)
                    {
                        int CourseId = oldEnrollment.Key;
                        int oldSectionId = oldEnrollment.Value;
                        if (!newEnrollments.ContainsKey(CourseId))
                        {
                            // drop course from enrollment table, and transcript
                            cmd.CommandText = deleteEnrollment;
                            cmd.Parameters["@oldSectionId"].Value = oldSectionId;
                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                return false;
                            }

                            InstitutionalCredit institutionalCredit = GetInstitutionalCredit(TermId, CourseId);
                            if (!DeleteInstitutionalCreditInTranscript(ref transcript, institutionalCredit))
                            {
                                return false;
                            }
                            isTranscriptModified = true;
                            coursesToDelete.Add(CourseId);
                        }
                    }
                    foreach (var newEnrollment in newEnrollments)
                    {
                        int CourseId = newEnrollment.Key;
                        int newSectionId = newEnrollment.Value;
                        int oldSectionId;
                        if (oldEnrollments.TryGetValue(CourseId, out oldSectionId))
                        {
                            if (oldSectionId != newSectionId)
                            {
                                // same course, diff section
                                cmd.CommandText = updateEnrollment;
                                cmd.Parameters["@oldSectionId"].Value = oldSectionId;
                                cmd.Parameters["@newSectionId"].Value = newSectionId;
                                if (cmd.ExecuteNonQuery() != 1)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // new course
                            Tuple<string, string> courseSubjectNum = null;
                            string coursePrereqs = null;
                            cmd.CommandText = selectCourseInfo;
                            cmd.Parameters["@CourseId"].Value = CourseId;
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (reader.Read())
                                {
                                    courseSubjectNum = new Tuple<string, string>((string)reader["CourseSubject"], (string)reader["CourseNum"]);
                                    coursePrereqs = (string)reader["Prereqs"];
                                }
                            }
                            if (courseSubjectNum == null || coursePrereqs == null)
                            {
                                return false;
                            }

                            // check this course isn't already taken by student
                            if (CourseTaken(transcript, courseSubjectNum))
                            {
                                return false;
                            }

                            // check pre-requisites 
                            List<Prereq> prereqs = JsonConvert.DeserializeObject<List<Prereq>>(coursePrereqs) ?? new List<Prereq>();
                            if (!PrerequisiteMet(transcript, prereqs))
                            {
                                // log a msg ?
                                Debug.WriteLine($"pre-requisite not met for: {courseSubjectNum.Item1}{courseSubjectNum.Item2}");
                            }

                            // add row in enrollment, and transcript
                            cmd.CommandText = insertEnrollment;
                            cmd.Parameters["@newSectionId"].Value = newSectionId;
                            if (cmd.ExecuteNonQuery() != 1)
                            {
                                return false;
                            }

                            InstitutionalCredit institutionalCredit = GetInstitutionalCredit(TermId, CourseId);
                            if (!InsertInstitutionalCreditInTranscript(ref transcript, institutionalCredit))
                            {
                                return false;
                            }
                            isTranscriptModified = true;
                            coursesToAdd.Add(CourseId);
                        }
                    }

                    // update StudentTranscript column in Student table
                    if (isTranscriptModified)
                    {
                        cmd.CommandText = "update student set StudentTranscript=@StudentTranscript where studentid=@StudentId";
                        cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value = JsonConvert.SerializeObject(transcript);
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            return false;
                        }
                    }

                    // add or delete courses from invoice
                    if (coursesToAdd.Count > 0 || coursesToDelete.Count > 0)
                    {
                        if (!UpdateInvoice(StudentId, TermId, coursesToAdd, coursesToDelete))
                        {
                            return false;
                        }
                    }
                    
                    // save changes
                    transaction.Commit();
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    transaction?.Rollback();
                }
                
            }

            return false;
        }

        /// <summary>
        /// Updates Invoice column in Registration table with new data
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <param name="coursesToAdd"></param>
        /// <param name="coursesToDelete"></param>
        /// <returns></returns>
        public bool UpdateInvoice(int StudentId, int TermId, List<int> coursesToAdd, List<int> coursesToDelete)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                    {
                        int registrationId = 0;
                        Invoice invoice = null;
                        cmd.CommandText =
                            "select RegistrationId,Invoice from InvoiceInfo where StudentId=@StudentId and TermId=@TermId";
                        cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                        cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                registrationId = reader.GetInt32(0);
                                if (!reader.IsDBNull(1))
                                {
                                    invoice = JsonConvert.DeserializeObject<Invoice>(reader.GetString(1));
                                }
                            }
                        }
                        if (registrationId == 0)
                        {
                            return false;
                        }
                        if (invoice == null)
                        {
                            invoice = NewInvoice();
                        }

                        cmd.CommandText = "select coursecode, coursecredits from course where courseid=@CourseId";
                        cmd.Parameters.Add("@CourseId", SqlDbType.Int);
                        foreach (int courseId in coursesToDelete)
                        {
                            cmd.Parameters["@CourseId"].Value = courseId;
                            string courseCode = (string) cmd.ExecuteScalar();
                            int index = invoice.tuition.FindIndex(c => c.courseCode.Equals(courseCode));
                            invoice.tuition.RemoveAt(index);
                        }
                        foreach (int courseId in coursesToAdd)
                        {
                            cmd.Parameters["@CourseId"].Value = courseId;
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                if (reader.Read())
                                {
                                    string courseCode = reader.GetString(0);
                                    double courseCredits = Convert.ToDouble(reader.GetDecimal(1));
                                    invoice.tuition.Add(NewInvoiceCourse(courseCode, courseCredits));
                                }
                            }
                        }
                        UpdateInvoiceTotal(ref invoice);
                        cmd.CommandText = "update registration set invoice=@Invoice where registrationid=@RegistrationId";
                        cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = registrationId;
                        cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar).Value = JsonConvert.SerializeObject(invoice);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        private void UpdateInvoiceTotal(ref Invoice invoice)
        {
            invoice.isFullTime = invoice.tuition.Count >= 4;
            invoice.registrationFee = invoice.isFullTime ? 200 : 100;
            invoice.total = invoice.registrationFee;
            foreach (var c in invoice.tuition)
            {
                invoice.total += c.fee;
            }
        }
        private InvoiceCourse NewInvoiceCourse(string courseCode, double courseCredits)
        {
            return new InvoiceCourse {courseCode = courseCode, fee = courseCredits*200};
        }
        private Invoice NewInvoice()
        {
            Invoice invoice = new Invoice()
            {
                isFullTime = true,
                registrationFee = 200,
                total = 200,
                tuition = new List<InvoiceCourse>()
            };
            return invoice;
        }
        private HashSet<string> GetAllCompletedCourses(Transcript transcript, string currentTerm)
        {
            HashSet<string> completedCourses = new HashSet<string>();
            foreach (History history in transcript.history)
            {
                foreach (Record record in history.records)
                {
                    foreach (Term term in record.institution.terms)
                    {
                        if (term.term == currentTerm)
                        {
                            continue;
                        }
                        foreach (Course course in term.courses)
                        {
                            if ((course.gradeNumeric ?? 50) >= 50)
                            {
                                completedCourses.Add(course.subject + course.course);
                            }
                            
                        }
                    }
                    foreach (School school in record.transfer.schools)
                    {
                        foreach (Course course in school.courses)
                        {
                            completedCourses.Add(course.subject + course.course);
                        }
                    }
                }
            }
            return completedCourses;
        }

        /// <summary>
        /// Retrieves a list of EnrollmentOption from a stored procedure,
        /// readys records for transformation into TimetableBuilder.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <returns></returns>
        public List<EnrollmentOption> GetEnrollmentOptions(int StudentId, int TermId)
        {
            List<EnrollmentOption> options=new List<EnrollmentOption>();
            Transcript transcript = null;
            string currentTerm = null;

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "EnrollmentOptions";
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction=ParameterDirection.Input;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction=ParameterDirection.Output;
                cmd.Parameters.Add("@TermName",SqlDbType.VarChar, 20).Direction=ParameterDirection.Output;

                cmd.Parameters["@StudentId"].Value = StudentId;
                cmd.Parameters["@TermId"].Value = TermId;

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EnrollmentOption o=new EnrollmentOption();
                            o.ProgramSemesterId = (int) reader["ProgramSemesterId"];
                            o.CourseId = (int)reader["CourseId"];
                            o.ProgramSemesterNum = Convert.ToInt16(reader["ProgramSemesterNum"]);
                            o.ProgramSemesterName = (string)reader["ProgramSemesterName"];
                            o.IsAcademic = (bool)reader["IsAcademic"];
                            o.IsMandatory = (bool)reader["IsMandatory"];
                            o.IsTechnicalElective = (bool)reader["IsTechnicalElective"];
                            o.IsGeneralElective = (bool)reader["IsGeneralElective"];
                            o.CourseCode = (string)reader["CourseCode"];
                            o.CourseTitle = (string) reader["CourseTitle"];
                            o.CourseCredits = Convert.ToDouble((decimal)reader["CourseCredits"]);
                            o.Prereqs = JsonConvert.DeserializeObject<List<Prereq>>((string) reader["Prereqs"]) ?? new List<Prereq>();
                            o.CourseIsOffered = (bool) reader["CourseIsOffered"];
                            o.SectionIsSelected = (bool) reader["SectionIsSelected"];

                            if (o.CourseIsOffered)
                            {
                                o.SectionId = (int)reader["SectionId"];
                                o.SectionNum = (string)reader["SectionNum"];
                                o.TermCourseId = (int)reader["TermCourseId"];
                                o.Classes = JsonConvert.DeserializeObject<List<Class>>((string)reader["Classes"]) ?? new List<Class>();
                            }

                            options.Add(o);
                        }
                    }

                    transcript = JsonConvert.DeserializeObject<Transcript>((string) cmd.Parameters["@StudentTranscript"].Value);
                    currentTerm = (string) cmd.Parameters["@TermName"].Value;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }

            HashSet<string> completedCourses = GetAllCompletedCourses(transcript, currentTerm);
            foreach (EnrollmentOption o in options)
            {
                o.courseIsCompleted = completedCourses.Contains(o.CourseCode);
            }

            return options;
        }

        /// <summary>
        /// Gets list of Grade for the specified SectionId
        /// </summary>
        /// <param name="SectionId"></param>
        /// <returns></returns>
        public List<Grade> GetGrades(int SectionId)
        {
            List<Grade> grades = new List<Grade>();
            string sql =
                "select e.GradeNumeric, e.EnrollmentId,s.StudentId,s.StudentNum,s.StudentLastName,s.StudentFirstMidName from Enrollment e join Student s on s.StudentId=e.StudentId where e.SectionId=@SectionId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double? g = null;
                        if (!reader.IsDBNull(0))
                        {
                            g = Convert.ToDouble(reader.GetDecimal(0));
                        }
                        grades.Add(new Grade
                        {
                            GradeNumeric = g,
                            EnrollmentId = (int)reader["EnrollmentId"],
                            StudentId = (int)reader["StudentId"],
                            StudentNum = (string)reader["StudentNum"],
                            StudentLastName = (string)reader["StudentLastName"],
                            StudentFirstMidName = (string)reader["StudentFirstMidName"]
                        });
                    }
                }
            }
            return grades;
        }

        
        public bool IsGoodAcademicStanding(double gpa)
        {
            return gpa > 2; // TODO
        }
        private void CopyTotals(Totals fromTotals, ref Totals toTotals)
        {
            toTotals.attemptHours = fromTotals.attemptHours;
            toTotals.passedHours = fromTotals.passedHours;
            toTotals.earnedHours = fromTotals.earnedHours;
            toTotals.gpaHours = fromTotals.gpaHours;
            toTotals.qualityPoints = fromTotals.qualityPoints;
            toTotals.gpa = fromTotals.gpa;
        }
        private void AddCreditToTotals(Course course, ref Totals totals)
        {
            double hrs = course.creditHours.Value;
            totals.attemptHours += hrs;
            if (course.gradeNumeric >= 50)
            {
                totals.passedHours += hrs;
                totals.earnedHours += hrs;
                totals.gpaHours += hrs;
            }
            totals.qualityPoints += course.qualityPoints;
            totals.gpa = totals.qualityPoints/totals.attemptHours;
        }

        private CurrentTerm NewCurrentTerm()
        {
            CurrentTerm currentTerm=new CurrentTerm();
            currentTerm.totals=new Totals
            {
                attemptHours = 0,
                passedHours = 0,
                earnedHours = 0,
                gpaHours = 0,
                qualityPoints = 0,
                gpa = 0
            };
            return currentTerm;
        }

        private Cumulative NewCumulative()
        {
            Cumulative cumulative=new Cumulative();
            cumulative.totals=new Totals();
            return cumulative;
        }

        /// <summary>
        /// Converts numeric grade to grade letter and gpa
        /// </summary>
        /// <param name="gradeNumeric"></param>
        /// <returns></returns>
        private Tuple<string,double> ConvertGrade(double gradeNumeric)
        {
            string[] letters = {"A+", "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "F"};
            double[] gpas =    { 4.5, 4.0, 4.0,  3.5,  3.0,  3.0,  2.5, 2.0,  2.0,  1.5,  1.0, 1.0,  0.0 };
            double[] grades =  { 90,   85,  80,   77,   73,  70,   67,   63,  60,   57,   53,  50,   0};

            int len = letters.Length;
            for (int i = 0; i < len; i++)
            {
                if (gradeNumeric >= grades[i])
                {
                    return new Tuple<string, double>(letters[i], gpas[i]);
                }
            }
            return null;
        }

        private bool UpdateGradeInTranscript(int TermCourseId, double GradeNumeric, ref Transcript transcript)
        {
            bool isGoodAcademicStanding = false;
            string level = null, subject = null, courseNum = null, termName = null;
            string sql =
                "select c.CourseLevel,c.CourseSubject,c.CourseNum,t.TermName from TermCourse tc join Course c on c.CourseId=tc.CourseId join Term t on t.TermId=tc.TermId where tc.TermCourseId=@TermCourseId";

            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TermCourseId", SqlDbType.Int).Value = TermCourseId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        level = reader.GetString(0);
                        subject = reader.GetString(1);
                        courseNum = reader.GetString(2);
                        termName = reader.GetString(3);
                    }
                }
            }

            foreach (Record record in GetCurrentHistoryInTranscript(transcript).records)
            {
                if (record.level.Equals(level))
                {
                    foreach (Term term in record.institution.terms)
                    {
                        if (term.term.Equals(termName))
                        {
                            foreach (Course course in term.courses)
                            {
                                if (course.subject.Equals(subject) && course.course.Equals(courseNum))
                                {
                                    Tuple<string, double> gradeLetterGpa = ConvertGrade(GradeNumeric);
                                    course.gradeNumeric = GradeNumeric;
                                    course.gradeLetter = gradeLetterGpa.Item1;
                                    course.qualityPoints = course.creditHours * gradeLetterGpa.Item2;

                                    if (term.currentTerm == null)
                                    {
                                        term.currentTerm = NewCurrentTerm();
                                    }
                                    if (term.cumulative == null)
                                    {
                                        term.cumulative = NewCumulative();
                                    }
                                    Totals currentTermTotals = term.currentTerm.totals;
                                    AddCreditToTotals(course, ref currentTermTotals);
                                    Totals institutionTotals = record.institution.totals;
                                    AddCreditToTotals(course, ref institutionTotals);
                                    Totals cumulativeTotals = term.cumulative.totals;
                                    CopyTotals(institutionTotals, ref cumulativeTotals);

                                    isGoodAcademicStanding = IsGoodAcademicStanding(cumulativeTotals.gpa.Value);
                                    if (isGoodAcademicStanding)
                                    {
                                        term.academicStanding = "In good academic standing.";
                                    }
                                    else
                                    {
                                        term.academicStanding = "On academic probation.";
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            return isGoodAcademicStanding;
        }

        /// <summary>
        /// Updates student grade in transcript, enrollment; academic standing is also updated
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermCourseId"></param>
        /// <param name="EnrollmentId"></param>
        /// <param name="GradeNumeric"></param>
        /// <returns></returns>
        public bool UpdateGrade(int StudentId, int TermCourseId, int EnrollmentId, double GradeNumeric)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                {
                    // retrieve transcript from database
                    Transcript transcript = null;
                    cmd.CommandText = "SELECT StudentTranscript FROM student where StudentId=@StudentId";
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                        }
                    }
                    if (transcript == null)
                    {
                        return false;
                    }

                    // modify transcript in memory
                    bool isGoodAcademicStanding = UpdateGradeInTranscript(TermCourseId, GradeNumeric, ref transcript);

                    // update transcript in database, also updates academic standing column
                    cmd.CommandText = "update student set StudentTranscript=@StudentTranscript,StudentAcademicStanding=@StudentAcademicStanding where studentid=@StudentId";
                    cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value = JsonConvert.SerializeObject(transcript);
                    cmd.Parameters.Add("@StudentAcademicStanding", SqlDbType.Bit).Value = isGoodAcademicStanding;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        return false;
                    }

                    // update enrollment in database
                    cmd.CommandText =
                        "update Enrollment set GradeNumeric=@GradeNumeric where EnrollmentId=@EnrollmentId";
                    cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Value = EnrollmentId;
                    cmd.Parameters.Add("@GradeNumeric", SqlDbType.Decimal).Value = GradeNumeric;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets count of records in Enrollment for the specified student and term
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <returns>count of records</returns>
        public int GetNumberEnrolledCourses(int StudentId, int TermId)
        {
            string sql = "select count(*) from EnrollmentInfo where StudentId=@StudentId and TermId=@TermId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                return (int) cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Updates student transcript in database to new program
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns></returns>
        public bool UpdateTranscriptForProgramTransfer(int StudentId, int ProgramId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(null, conn))
            {
                Transcript transcript = null;
                cmd.CommandText = "select StudentTranscript from student where studentid=@StudentId";
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                        }
                    }
                }
                

                cmd.CommandText = "select DegreeName,CampusName,DepartmentName,ProgramName from ProgramInfo where ProgramId=@ProgramId";
                cmd.Parameters.Add("@ProgramId", SqlDbType.Int).Value = ProgramId;
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        History history = GetCurrentHistoryInTranscript(transcript);
                        history.degreeSought = reader.GetString(0);
                        history.campus = reader.GetString(1);
                        history.department = reader.GetString(2);
                        history.program = reader.GetString(3);
                    }
                }

                
                cmd.CommandText = "UPDATE student SET studenttranscript=@StudentTranscript WHERE studentid=@StudentId";
                cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Value = JsonConvert.SerializeObject(transcript);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Gets ProgramId of student's current program
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns>ProgramId on success, or 0 on failure</returns>
        public int GetCurrentProgramId(int StudentId)
        {
            int programId = 0;
            string sql = "select ProgramId from studentprogram where studentid=@StudentId and EndDate is null";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                try
                {
                    conn.Open();
                    programId = (int)cmd.ExecuteScalar();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return programId;
        }

        /// <summary>
        /// This method performs the following:
        /// 1) drop all enrollments for the student and term
        /// 2) update student transcript to reflect new program info
        /// 3) create new records in studentprogram and registration
        /// </summary>
        /// <param name="TermId"></param>
        /// <param name="StudentId"></param>
        /// <param name="ProgramId"></param>
        /// <param name="CurrentSemester"></param>
        /// <returns></returns>
        public bool TransferStudentToProgram(int TermId, int StudentId, int ProgramId, int CurrentSemester)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand("ProgramTransferInfo", conn))
            {
                // call stored procedure to retrieve info needed
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters.Add("@newProgramId", SqlDbType.Int).Direction = ParameterDirection.Input;
                cmd.Parameters["@StudentId"].Value = StudentId;
                cmd.Parameters["@TermId"].Value = TermId;
                cmd.Parameters["@newProgramId"].Value = ProgramId;

                cmd.Parameters.Add("@TermName", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@oldProgramId", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@oldDegreeName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@oldInvoice", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@newDegreeName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@newCampusName", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@newDepartmentName", SqlDbType.VarChar, 40).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@newProgramName", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                try
                {
                    conn.Open();

                    List<InstitutionalCredit> existingCourseEnrollments = new List<InstitutionalCredit>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingCourseEnrollments.Add(new InstitutionalCredit
                            {
                                subject = (string)reader["CourseSubject"],
                                course = (string)reader["CourseNum"],
                                level = (string)reader["CourseLevel"]
                            });
                        }
                    }

                    int oldProgramId = (int)cmd.Parameters["@oldProgramId"].Value;
                    if (oldProgramId == ProgramId)
                    {
                        return false;
                    }

                    string termName = (string)cmd.Parameters["@TermName"].Value;
                    string oldDegreeName = (string)cmd.Parameters["@oldDegreeName"].Value;
                    string newDegreeName = (string)cmd.Parameters["@newDegreeName"].Value;
                    string newCampusName = (string)cmd.Parameters["@newCampusName"].Value;
                    string newDepartmentName = (string)cmd.Parameters["@newDepartmentName"].Value;
                    string newProgramName = (string)cmd.Parameters["@newProgramName"].Value;
                    Transcript transcript = JsonConvert.DeserializeObject<Transcript>((string)cmd.Parameters["@StudentTranscript"].Value);

                    if (transcript == null)
                    {
                        return false;
                    }

                    // modify transcript in memory
                    foreach (History history in transcript.history)
                    {
                        if (history.degreeSought != oldDegreeName)
                        {
                            continue;
                        }

                        history.degreeSought = newDegreeName;
                        history.campus = newCampusName;
                        history.department = newDepartmentName;
                        history.program = newProgramName;

                        foreach (var ic in existingCourseEnrollments)
                        {
                            foreach (Record record in history.records)
                            {
                                if (record.level != ic.level)
                                {
                                    continue;
                                }
                                foreach (Term term in record.institution.terms)
                                {
                                    if (term.term != termName)
                                    {
                                        continue;
                                    }
                                    int index =
                                        term.courses.FindIndex(
                                            c =>
                                                c.subject == ic.subject &&
                                                c.course == ic.course);
                                    if (index != -1)
                                    {
                                        term.courses.RemoveAt(index);
                                    }
                                    break;
                                }
                                break;
                            }
                        }
                    }

                    Invoice oldInvoice = JsonConvert.DeserializeObject<Invoice>((string)cmd.Parameters["@oldInvoice"].Value);
                    // modify invoice in memory
                    if (oldInvoice != null)
                    {
                        foreach (var ic in existingCourseEnrollments)
                        {
                            string courseCode = ic.subject + ic.course;
                            int index = oldInvoice.tuition.FindIndex(c => c.courseCode.Equals(courseCode));
                            oldInvoice.tuition.RemoveAt(index);
                        }
                        UpdateInvoiceTotal(ref oldInvoice);
                    }

                    // call stored procedure to update changes
                    cmd.CommandText = "DoProgramTransfer";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@StudentId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    cmd.Parameters.Add("@TermId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    cmd.Parameters.Add("@newProgramId", SqlDbType.Int).Direction = ParameterDirection.Input;
                    cmd.Parameters.Add("@newCurrentSemester", SqlDbType.SmallInt).Direction = ParameterDirection.Input;
                    cmd.Parameters.Add("@StudentTranscript", SqlDbType.NVarChar).Direction = ParameterDirection.Input;

                    cmd.Parameters["@StudentId"].Value = StudentId;
                    cmd.Parameters["@TermId"].Value = TermId;
                    cmd.Parameters["@newProgramId"].Value = ProgramId;
                    cmd.Parameters["@newCurrentSemester"].Value = CurrentSemester;
                    cmd.Parameters["@StudentTranscript"].Value = JsonConvert.SerializeObject(transcript);

                    if (oldInvoice != null)
                    {
                        cmd.Parameters.Add("@oldInvoice", SqlDbType.NVarChar).Direction = ParameterDirection.Input;
                        cmd.Parameters["@oldInvoice"].Value = JsonConvert.SerializeObject(oldInvoice);
                    }

                    cmd.Parameters.Add("@isSuccessful", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    if ((bool)cmd.Parameters["@isSuccessful"].Value)
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                
            }

            return false;
        }

        /// <summary>
        /// Updates a record in Student table. Fields NOT updated include:
        /// StudentId, StudentNum, StudentTranscript
        /// </summary>
        /// <param name="student"></param>
        /// <returns>true if successful, false otherwise</returns>
        public bool UpdateStudent(Student student)
        {
            string sql = new StringBuilder("update student set")
                .Append(" StudentLastName=@StudentLastName")
                .Append(",StudentFirstMidName=@StudentFirstMidName")
                .Append(",StudentEmail=@StudentEmail")
                .Append(",StudentHasHolds=@StudentHasHolds")
                .Append(",StudentAcademicStanding=@StudentAcademicStanding")
                .Append(",StudentCanRegister=@StudentCanRegister")
                .Append(",StudentStartDate=@StudentStartDate")
                .Append(" where StudentId=@StudentId")
                .ToString();
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentLastName", SqlDbType.VarChar).Value = student.StudentLastName;
                cmd.Parameters.Add("@StudentFirstMidName", SqlDbType.VarChar).Value = student.StudentFirstMidName;
                cmd.Parameters.Add("@StudentEmail", SqlDbType.VarChar).Value = student.StudentEmail;
                cmd.Parameters.Add("@StudentHasHolds", SqlDbType.Bit).Value = student.StudentHasHolds;
                cmd.Parameters.Add("@StudentAcademicStanding", SqlDbType.Bit).Value = student.StudentAcademicStanding;
                cmd.Parameters.Add("@StudentCanRegister", SqlDbType.Bit).Value = student.StudentCanRegister;
                cmd.Parameters.Add("@StudentStartDate", SqlDbType.DateTime).Value = student.StudentStartDate;
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = student.StudentId;
                try
                {
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        /// <summary>
        /// Updates UserPassword for user with UserId
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="UserPassword"></param>
        /// <returns>true if successful, false otherwise</returns>
        public bool UpdateUserPassword(int UserId, string UserPassword)
        {
            string sql = "update users set userpassword=@UserPassword where userid=@UserId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@UserPassword", SqlDbType.VarChar).Value = UserPassword;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                try
                {
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves student transcript from Student table for specified StudentId
        /// </summary>
        /// <param name="StudentId"></param>
        /// <returns>Transcript object if there's one, or null</returns>
        public Transcript GetStudentTranscript(int StudentId)
        {
            Transcript transcript = null;
            string sql = "select studenttranscript from student where studentid=@StudentId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            transcript = JsonConvert.DeserializeObject<Transcript>(reader.GetString(0));
                        }
                    }
                }
            }
            return transcript;
        }

        /// <summary>
        /// Gets StudentProgramId, CurrentSemester, and ProgramOption for student's current program, if there is one
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="StudentProgramId"></param>
        /// <param name="CurrentSemester"></param>
        /// <param name="program"></param>
        /// <returns>true if there is a current program, false otherwise</returns>
        public bool GetStudentProgramOption(int StudentId, out int StudentProgramId, out int CurrentSemester, out ProgramOption program)
        {
            bool hasResult = false;
            StudentProgramId = 0;
            CurrentSemester = 0;
            program = null;
            string sql = "select sp.StudentProgramId, sp.CurrentSemester, p.ProgramId, p.ProgramCode, p.ProgramName, p.ProgramSemesters from studentprogram sp join program p on p.programid=sp.programid where sp.studentid=@StudentId and sp.enddate is null";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        StudentProgramId = (int) reader["StudentProgramId"];
                        CurrentSemester = (Int16) reader["CurrentSemester"];
                        program = new ProgramOption
                        {
                            ProgramId = (int)reader["ProgramId"],
                            ProgramCode=(string)reader["ProgramCode"],
                            ProgramName=(string)reader["ProgramName"],
                            ProgramSemesters = (Int16)reader["ProgramSemesters"]
                        };
                        hasResult = true;
                    }
                }
            }
            return hasResult;
        }

        /// <summary>
        /// Gets list of RegistrationOption for all valid Registration records student has for their program
        /// </summary>
        /// <param name="StudentProgramId"></param>
        /// <returns></returns>
        public List<RegistrationOption> GetValidRegistrations(int StudentProgramId)
        {
            List<RegistrationOption> registrations=new List<RegistrationOption>();
            string sql = "select r.RegistrationId, t.TermId, t.TermName, t.TermEndDate from Registration r join Term t on t.TermId=r.TermId where r.StudentProgramId=@StudentProgramId and r.DateRegistrationCancelled is null";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = StudentProgramId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        registrations.Add(new RegistrationOption
                        {
                            RegistrationId = (int)reader["RegistrationId"],
                            TermId = (int)reader["TermId"],
                            TermName = (string)reader["TermName"],
                            TermEndDate = (DateTime)reader["TermEndDate"]
                        });
                    }
                }
            }
            return registrations;
        }

        /// <summary>
        /// Returns list of EnrollmentSelection.
        /// Each item represents an enrollment in a course by the student for the term,
        /// it also contains all sections for the same course in a convenient list.
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <returns></returns>
        public List<EnrollmentSelection> GetStudentEnrollmentSelections(int StudentId, int TermId)
        {
            List<EnrollmentSelection> list=new List<EnrollmentSelection>();
            string sql =
                "select e.EnrollmentId,s.SectionNum,s.SectionId,e.SectionId as selected_sectionid,c.CourseCode from EnrollmentInfo e join Course c on c.CourseId=e.CourseId join Section s on s.TermCourseId=e.TermCourseId where e.StudentId=@StudentId and e.TermId=@TermId order by 1,2";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int enrollmentId = (int) reader["EnrollmentId"];
                        EnrollmentSelection e = list.Find(s => s.EnrollmentId == enrollmentId);
                        if (e == null)
                        {
                            e=new EnrollmentSelection
                            {
                                EnrollmentId = enrollmentId,
                                SectionId = (int)reader["selected_sectionid"],
                                CourseCode = (string)reader["CourseCode"],
                                Sections = new List<SectionOption>()
                            };
                            list.Add(e);
                        }
                        e.Sections.Add(new SectionOption
                        {
                            SectionId = (int)reader["SectionId"],
                            SectionNum = (string)reader["SectionNum"]
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Converts the supplied info to a TransferCredit object
        /// </summary>
        /// <param name="CourseId"></param>
        /// <param name="school"></param>
        /// <param name="gradeLetter"></param>
        /// <returns>TransferCredit object, or null if the specified CourseId doesn't exist in Course table</returns>
        public TransferCredit ConvertToTransferCredit(int CourseId, string school, string gradeLetter)
        {
            string sql = "select CourseSubject,CourseNum,CourseLevel,CourseTitle,CourseCredits from course where CourseId=@CourseId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@CourseId", SqlDbType.Int).Value = CourseId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        TransferCredit tc=new TransferCredit
                        {
                            school = school,
                            gradeLetter = gradeLetter,
                            subject = reader.GetString(0),
                            course = reader.GetString(1),
                            level = reader.GetString(2),
                            title = reader.GetString(3),
                            creditHours = Convert.ToDouble(reader.GetDecimal(4))
                        };
                        return tc;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets list of TimetableEntry for displaying the timetable
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <returns></returns>
        public List<TimetableEntry> GetTimetable(int StudentId, int TermId)
        {
            List<TimetableEntry> list=new List<TimetableEntry>();
            string sql = new StringBuilder()
                .Append("select c.StartDay,c.StartHour,c.StartMinute,c.EndDay,c.EndHour,c.EndMinute")
                .Append(",c.ClassId,c.ClassIsLecture,c.CampusCode,c.Building,c.RoomNum")
                .Append(",e.SectionNum,co.CourseId,co.CourseCode ")
                .Append("from EnrollmentInfo e join ClassInfo c on e.SectionId=c.SectionId ")
                .Append("join Course co on co.CourseId=e.CourseId ")
                .Append("where e.StudentId=@StudentId and e.TermId=@TermId order by 1,2,3").ToString();
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TimetableEntry
                        {
                            startDay = (Int16)reader["StartDay"],
                            startHour = (Int16)reader["StartHour"],
                            startMinute = (Int16)reader["StartMinute"],
                            endDay = (Int16)reader["EndDay"],
                            endHour = (Int16)reader["EndHour"],
                            endMinute = (Int16)reader["EndMinute"],
                            classId = (int)reader["ClassId"],
                            isLecture = (bool)reader["ClassIsLecture"],
                            room = $"{(string)reader["CampusCode"]} {(string)reader["Building"]}{(string)reader["RoomNum"]}",
                            section = (string)reader["SectionNum"],
                            courseId = (int)reader["CourseId"],
                            courseCode = (string)reader["CourseCode"]
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets the TermOption for the current date
        /// </summary>
        /// <returns></returns>
        public TermOption GetCurrentTermOption()
        {
            TermOption result = null;
            string sql = "select TermId,TermName from term where CAST(GETDATE() as date) between TermStartDate and TermEndDate";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        result = new TermOption {TermId = reader.GetInt32(0), TermName = reader.GetString(1)};
                    }
                }
            }
            return result;
        }

        public void ConvertTimetableArray()
        {
            int firstDayInWeek = 1, lastDayInWeek = 6;
            int firstMinuteInDay = 8 * 60, lastMinuteInDay = 22 * 60 + 30, step = 30;
            int columns = lastDayInWeek - firstDayInWeek + 1;
            int rows = (lastMinuteInDay - firstMinuteInDay) / step + 1;
            List<int>[] cells = new List<int>[columns * rows];

        }

        public void ConvertIndexToTime(int index, out int day, out int hour, out int minute)
        {
            int firstMinuteInDay = 8 * 60, lastMinuteInDay = 22 * 60 + 30, step = 30;
            int rows = (lastMinuteInDay - firstMinuteInDay) / step + 1;
            day = index / rows + 1;
            minute = index % rows * step + firstMinuteInDay;
            hour = minute / 60;
            minute = minute % 60;
        }
        public int ConvertTimeToIndex(int day, int hour, int minute)
        {
            int firstDayInWeek = 1;
            int firstMinuteInDay = 8 * 60, lastMinuteInDay = 22 * 60 + 30, step = 30;

            int rows = (lastMinuteInDay - firstMinuteInDay) / step + 1;
            int index = (hour * 60 + minute - firstMinuteInDay) / step + (day - firstDayInWeek) * rows;
            return index;
        }

        /// <summary>
        /// Simply updates a record in Enrollment to the new SectionId
        /// </summary>
        /// <param name="EnrollmentId"></param>
        /// <param name="SectionId"></param>
        /// <returns></returns>
        public bool UpdateEnrollmentSection(int EnrollmentId, int SectionId)
        {
            string sql = "update enrollment set sectionid=@SectionId where enrollmentid=@EnrollmentId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@EnrollmentId", SqlDbType.Int).Value = EnrollmentId;
                cmd.Parameters.Add("@SectionId", SqlDbType.Int).Value = SectionId;
                try
                {
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
            return false;
        }

        /// <summary>
        /// Updates CurrentSemester field in StudentProgram table for the specified StudentProgramId
        /// </summary>
        /// <param name="StudentProgramId"></param>
        /// <param name="CurrentSemester"></param>
        /// <returns></returns>
        public bool UpdateCurrentSemester(int StudentProgramId, int CurrentSemester)
        {
            string sql = "update studentprogram set CurrentSemester=@CurrentSemester where StudentProgramId=@StudentProgramId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentProgramId", SqlDbType.Int).Value = StudentProgramId;
                cmd.Parameters.Add("@CurrentSemester", SqlDbType.SmallInt).Value = CurrentSemester;
                conn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        private List<int> GetEnrollmentIds(int StudentId, int TermId, SqlCommand cmd)
        {
            List<int> enrollmentIds = new List<int>();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select EnrollmentId from EnrollmentInfo where StudentId=@StudentId and TermId=@TermId";
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
            cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    enrollmentIds.Add(reader.GetInt32(0));
                }
            }
            return enrollmentIds;
        }

        /// <summary>
        /// Cancels student registration by deleting all enrollments in the specified term,
        /// updating student transcript, and invoice, to reflect those changes, and
        /// setting DateRegistrationCancelled to today for the registration record
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="TermId"></param>
        /// <param name="RegistrationId"></param>
        /// <returns>success status</returns>
        public bool CancelRegistration(int StudentId, int TermId, int RegistrationId)
        {
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                using (SqlCommand cmd = new SqlCommand(null, conn, transaction))
                {
                    try
                    {
                        // delete enrolled courses from enrollment, transcript, and invoice
                        List<int> enrollmentIds = GetEnrollmentIds(StudentId, TermId, cmd);
                        foreach (int enrollmentId in enrollmentIds)
                        {
                            DeleteEnrollment(enrollmentId, cmd);
                        }

                        // end date registration record
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "update Registration set DateRegistrationCancelled=GETDATE() where RegistrationId=@RegistrationId";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@RegistrationId", SqlDbType.Int).Value = RegistrationId;
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception);
                        transaction.Rollback();
                    }
                }
            }
            return false;
        }

        public Invoice GetInvoice(int StudentId,int TermId)
        {
            string sql = "select invoice from invoiceinfo where studentid=@StudentId and termid=@TermId";
            using (SqlConnection conn = new SqlConnection(CONNECTION_STR))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@StudentId", SqlDbType.Int).Value = StudentId;
                cmd.Parameters.Add("@TermId", SqlDbType.Int).Value = TermId;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            Invoice invoice = JsonConvert.DeserializeObject<Invoice>(reader.GetString(0));
                            if (invoice != null)
                            {
                                return invoice;
                            }
                        }
                    }
                }
            }
            return null;
        }

    }


}