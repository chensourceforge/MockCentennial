using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waitlist
{
    // Course Object for the mock waitlist.
    public class Course
    {
        public int courseID;
        public string courseCode;
        public int capacity = 0;
        public int maxcapacity = 40;
        public int waitlisted = 0;
    }

    // Represents the Student object
    public class Student
    {
        public int studentID;
    }

    class Waitlist
    {
        public List<Course> courseListing = new List<Course>();

        // Used to make a mockup course list the user can pick to register in
       public List<Course> SetupMockCourses(int nocourses)
        {
            List<Course> courseList = new List<Course>();
            System.Console.WriteLine("Creating Mock Courses....");
            for(int i=0; i< nocourses; i++)
            {
                Course newcourse = new Course();
                System.Console.Write("Enter a Course Code:");
                string newcoursecode = Console.ReadLine();
                System.Console.WriteLine("Generating Course " + newcoursecode + "....");
                newcourse.courseID = i;
                newcourse.courseCode = newcoursecode;
                System.Console.WriteLine("Succesfully created course " + newcourse.courseCode + "!");
                System.Console.WriteLine("Adding course " + newcourse.courseCode + " to list...");
                courseList.Add(newcourse);
                System.Console.WriteLine("Successfully added course " + newcourse.courseCode + " to list!");
            }
            System.Console.Clear();
            System.Console.WriteLine("Finished setting up mock courses!");
            return courseList;
        }

        // Show all the courses we made.
        public void DisplayMockCourses(List<Course> courselist)
        {
            System.Console.WriteLine("================================");
            System.Console.WriteLine("            Courses             ");
            System.Console.WriteLine("================================");
            foreach(Course course in courselist)
            {
                System.Console.WriteLine("Course ID: " + course.courseID);
                System.Console.WriteLine("Course Code: " + course.courseCode);
                System.Console.WriteLine("Registered Students: " + course.capacity + " out of " + course.maxcapacity + " students.");
                System.Console.WriteLine("Waitlisted Students: " + course.waitlisted);
                System.Console.WriteLine("--------------------------------");
            }

        }


        // Method to simulate Users being registered for the courses.
        public List<Course> RandomlyFillCourses(List<Course> courselist)
        {
            System.Console.WriteLine("Filling courses with students...");
            Random randomfullcourseidgenerator = new Random(); // Picks a random course from the generated list to fully fill the said course.
            Random randomcapacitygenerator = new Random(); // Picks a random number to specify number of students registered in a course.
            int randomfullcourseid = randomfullcourseidgenerator.Next(0,2);
            System.Console.WriteLine("Picking Course " + courselist[randomfullcourseid].courseCode + " as the full course.");
            foreach(Course course in courselist)
            {
                if(course.courseID == randomfullcourseid)
                {
                    course.capacity = course.maxcapacity;
                    Random randomwaitlistgenerator = new Random(); // Only Generate waitlist students when its full
                    course.waitlisted = randomwaitlistgenerator.Next(0, 20);
                    System.Console.WriteLine("Generating " + course.waitlisted + " students on the waitlist que.");
                }
                else
                {
                    
                    course.capacity = randomcapacitygenerator.Next(0, course.maxcapacity);
                    System.Console.WriteLine("Generating " + course.capacity + " students for course " + course.courseCode + "...");
                    
                }
            }
            return courselist;
        }


        public void PickCourseToAdd(int id)
        {
            char userchoice = 'n';
            if (id > courseListing.Count)
            {
                Console.Write("Please specify another course id as the stated one is invalid");
                id = Convert.ToInt32(Console.ReadLine());
            }
            else
            {
                foreach (Course course in courseListing)
                {
                    if (course.courseID == id)
                    {
                        if (course.capacity < course.maxcapacity)
                        {
                            System.Console.WriteLine("Registered to course " + course.courseCode + "!");
                            course.capacity++;
                        }
                        else
                        {
                            System.Console.Write("Unable to register for course, would you like to be waitlisted? (y/n)");
                            userchoice = System.Console.ReadKey().KeyChar;
                            System.Console.WriteLine();
                            if (userchoice == 'y')
                            {
                                course.waitlisted++;
                            }
                            else
                            {
                                System.Console.WriteLine("Please pick another course to register to.");
                            }
                        }
                    }
                }
            }

        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Initiating mCentennial Waitlist Mockup....");
            Waitlist waitlist = new Waitlist();
            System.Console.Write("Please specify amount of courses:");
            int courseamt = Convert.ToInt32(System.Console.ReadLine());
            waitlist.courseListing = waitlist.SetupMockCourses(courseamt);
            waitlist.courseListing = waitlist.RandomlyFillCourses(waitlist.courseListing);
            waitlist.DisplayMockCourses(waitlist.courseListing);
            System.Console.Write("Please pick a course to register to: ");
            int pickedcourse = Convert.ToInt32(System.Console.ReadLine());
            waitlist.PickCourseToAdd(pickedcourse);
            waitlist.DisplayMockCourses(waitlist.courseListing);

            System.Console.WriteLine("Press any key to terminate simulation...."); // Used for consoles that end after all processes are done.
            System.Console.ReadLine();
        }
    }
}
