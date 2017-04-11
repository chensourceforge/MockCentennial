using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Timetabling;
using Scheduler = Timetabling.Timetabling;


namespace MockCentennial.Controllers
{
    public class AdminController : Controller
    {
        private Scheduler scheduler;
        //private TextBlock[,] txtRoomTime;
        private List<Event> events;
        private List<Time> times;
        private List<Room> rooms;
        private List<Course> courses;
        private List<Instructor> instructors;
        private List<Student> students;
        //private bool generated = false;
        private string[ , ] timetable;
        private string html_timetable;

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerateTimetable()
        {
            //if (generated) return;
            //generated = true;
            GenerateTimetables();

            return View();
        }

        /*private void OnBtnFindPackagesClicked(object sender, RoutedEventArgs e)
        {
            if (!generated) return;
            displayStudent.Children.Clear();
            int studentId;
            if (Int32.TryParse(textboxStudentId.Text, out studentId))
            {
                ShowStudentPackages(studentId);
            }
        }*/

        void GenerateTimetables()
        {
            scheduler = new Scheduler();
            scheduler.PopulateSchedule();
            events = scheduler.ExportEventList();
            times = scheduler.ExportTimeList();
            rooms = scheduler.ExportRoomList();
            courses = scheduler.ExportCourseList();
            instructors = scheduler.ExportInstructorList();
            students = scheduler.ExportStudentList();
            ShowMaster();
            //ShowInstructor();
            ViewBag.HtmlStr = html_timetable;
        }
        void AddTextToArray(string txt, int row, int col)
        {
            timetable[row-1, col-1] = txt;
            /*TextBlock elem = new TextBlock() { Text = txt, Padding = new Thickness(5) };
            Grid.SetRow(elem, row);
            Grid.SetColumn(elem, col);
            grid.Children.Add(elem);
            return elem;*/
        }

        void ShowMaster()
        {
            //Grid tableRoomTime = new Grid();
            html_timetable = "<table border=\"1\">";
            int rows = times.Count + 1, cols = rooms.Count + 1;
            timetable = new string[rows, cols];
            // create grid cells
            /*for (int r = 0; r < rows; r++)
            {
                tableRoomTime.RowDefinitions.Add(new RowDefinition());
            }
            for (int c = 0; c < cols; c++)
            {
                tableRoomTime.ColumnDefinitions.Add(new ColumnDefinition());
            }*/
            // add headers

            /*for (int r = 1, c = 0; r < rows; r++)
            {
                int time = times[r - 1].StartTime;
                AddTextToGrid($"{time / 60}:{time % 60}", tableRoomTime, r, c);
            }*/
            html_timetable = html_timetable + "<tr><th></th>";
            for (int c = 1; c < cols; c++)
            {
                html_timetable = html_timetable + $"<th>room {rooms[c - 1].RoomId}</th>";
                //AddTextToGrid($"room {rooms[c - 1].RoomId}", tableRoomTime, r, c);
            }
            html_timetable = html_timetable + "</tr>";
            // add contents
            foreach (Event evt in events)
            {
                Course course = courses[evt.Course];
                string txt = $"course {course.CourseId} - {course.ClassType}\n{evt.Students} students";
                int roomIndex = evt.Room + 1;
                int startTime = evt.StartTime + 1;
                int endTime = startTime + evt.TimeUnits;
                for (int timeIndex = startTime; timeIndex < endTime; timeIndex++)
                {
                    AddTextToArray(txt, timeIndex, roomIndex);
                }
            }

            for (int i = 0; i < times.Count; i++)
            {
                html_timetable = html_timetable + "<tr>";
                int time = times[i].StartTime;
                html_timetable = html_timetable + $"<td>{time / 60}:{time % 60}</td>";

                for (int j = 0; j < cols; j++)
                {
                    html_timetable = html_timetable + $"<td>{timetable[i, j]}</td>";
                }
                html_timetable = html_timetable + "</tr>";
            }
            //tableRoomTime.ShowGridLines = true;
            //displayMaster.Children.Add(tableRoomTime);
            html_timetable = html_timetable + "</table>";

        }

        /*void ShowInstructor()
        {
            Grid tableInstructorTime = new Grid();
            int rows = times.Count + 1, cols = instructors.Count + 1;
            // create grid cells
            for (int r = 0; r < rows; r++)
            {
                tableInstructorTime.RowDefinitions.Add(new RowDefinition());
            }
            for (int c = 0; c < cols; c++)
            {
                tableInstructorTime.ColumnDefinitions.Add(new ColumnDefinition());
            }
            // add headers
            for (int r = 1, c = 0; r < rows; r++)
            {
                int time = times[r - 1].StartTime;
                AddTextToGrid($"{time / 60}:{time % 60}", tableInstructorTime, r, c);
            }
            for (int r = 0, c = 1; c < cols; c++)
            {
                AddTextToGrid($"instructor {instructors[c - 1].InstructorId}", tableInstructorTime, r, c);
            }
            // add contents
            foreach (Event evt in events)
            {
                Course course = courses[evt.Course];
                string txt = $"course {course.CourseId} - {course.ClassType}\n{evt.Students} students";
                int instructorIndex = evt.Instructor + 1;
                int startTime = evt.StartTime + 1;
                int endTime = startTime + evt.TimeUnits;
                for (int timeIndex = startTime; timeIndex < endTime; timeIndex++)
                {
                    AddTextToGrid(txt, tableInstructorTime, timeIndex, instructorIndex);
                }
            }
            tableInstructorTime.ShowGridLines = true;
            displayInstructor.Children.Add(tableInstructorTime);
        }

        void ShowStudentPackages(int studentId)
        {
            Student student = students.Find(s => s.StudentId == studentId);
            if (student == null)
            {
                MessageBox.Show(this, "Student not found.");
                return;
            }
            List<Package> packages = student.Curriculum.Packages;
            for (int i = 0; i < packages.Count; i++)
            {
                Package p = packages[i];
                TextBlock heading = new TextBlock() { Text = $"Package {i + 1} - {p.Students} available" };
                displayStudent.Children.Add(heading);
                ShowAPackage(p.Events);
            }
        }

        void ShowAPackage(List<Event> events_)
        {
            Grid timetable = new Grid();
            int rows = times.Count / 7 + 1, cols = 7 + 1;
            // create grid cells
            for (int r = 0; r < rows; r++)
            {
                timetable.RowDefinitions.Add(new RowDefinition());
            }
            for (int c = 0; c < cols; c++)
            {
                timetable.ColumnDefinitions.Add(new ColumnDefinition());
            }
            // add headers
            for (int r = 1, c = 0; r < rows; r++)
            {
                int time = times[r - 1].StartTime;
                AddTextToGrid($"{time / 60}:{time % 60}", timetable, r, c);
            }
            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int r = 0, c = 1; c < cols; c++)
            {
                AddTextToGrid(days[c - 1], timetable, r, c);
            }
            // add empty texts
            TextBlock[,] cells = new TextBlock[rows - 1, cols - 1];
            for (int r = 1; r < rows; r++)
            {
                for (int c = 1; c < cols; c++)
                {
                    TextBlock b = AddTextToGrid("", timetable, r, c);
                    cells[r - 1, c - 1] = b;
                }
            }
            // add contents
            foreach (Event evt in events_)
            {
                Course course = courses[evt.Course];
                Instructor instructor = instructors[evt.Instructor];
                Room room = rooms[evt.Room];
                string txt = $"course {course.CourseId} {course.ClassType} - room {room.RoomId} - inst {instructor.InstructorId}\n";
                int startTime = evt.StartTime % (rows - 1);
                int endTime = startTime + evt.TimeUnits;
                int dayIndex = times[evt.StartTime].Day - 1;
                for (int timeIndex = startTime; timeIndex < endTime; timeIndex++)
                {
                    cells[timeIndex, dayIndex].Text += txt;
                }
            }
            timetable.ShowGridLines = true;
            displayStudent.Children.Add(timetable);
        }*/
    }
}