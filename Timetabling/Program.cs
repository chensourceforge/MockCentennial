using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timetabling
{
    public class Time
    {
        public int TimeId { get; set; }
        public int StartTime { get; set; }
        //public int Duration { get; set; }
        public int Day { get; set; }
    }

    public enum RoomFeature
    {
        Projecter, Whiteboard, Router, PC, Mac
    }
    public class Room
    {
        public int RoomId { get; set; }
        public HashSet<RoomFeature> Features { get; set; }
        public int MinCapacity { get; set; }
        public int MaxCapacity { get; set; }
    }

    public enum InstructorPreference
    {
        Preferred, OK, NotAtAll
    }
    public class Instructor
    {
        public int InstructorId { get; set; }
        public Dictionary<Course, InstructorPreference> CoursePreferences { get; set; }
    }

    public enum ClassType
    {
        Lecture1, Lab1, Tutorial1, Lecture2, Lab2, Tutorial2
    }
    public class Course
    {
        public int CourseId { get; set; }
        public ClassType ClassType { get; set; }
        public int TimeUnits { get; set; }
        public HashSet<RoomFeature> Features { get; set; }
        //public Dictionary<Room, int> RoomRequirements { get; set; }
        //public Dictionary<int, int> RoomRequirements { get; set; }
        public int MinEvents { get; set; }
    }

    public class Event
    {
        public int StartTime { get; set; }
        public int TimeUnits { get; set; }
        public int Course { get; set; }
        public int Room { get; set; }
        public int Instructor { get; set; }
        public int Students { get; set; }
        public int Packages { get; set; }
    }
    public class Package
    {
        public int Students { get; set; }
        public List<Event> Events { get; set; }
    }

    //public enum CourseCategory
    //{
    //    Required, OptionalGroup1, OptionalGroup2, NotRequired
    //}

    public class CourseChoice : IEquatable<CourseChoice>
    {
        public int SelectHowMany { get; set; }
        public Dictionary<int, double> SelectFrom { get; set; }
        //public int CourseId { get; set; }
        //public CourseCategory Category { get; set; }
        //public double Likelihood { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            CourseChoice other = (CourseChoice) obj;
            return this.Equals(other);
        }
        public override int GetHashCode()
        {
            return SelectHowMany ^ SelectFrom.Keys.Sum();
        }

        public bool Equals(CourseChoice other)
        {
            if (this.SelectHowMany != other.SelectHowMany) return false;
            Dictionary<int, double> d1 = this.SelectFrom;
            Dictionary<int, double> d2 = other.SelectFrom;
            if (d1 == null || d2 == null) return false;
            if (d1.Count != d2.Count) return false;
            foreach (var kvp in d1)
            {
                double val;
                if (!d2.TryGetValue(kvp.Key, out val)) return false;
                if (kvp.Value != val) return false;
            }
            return true;
        }
    }
    public class Curriculum
    {
        public int Students { get; set; }
        //public Dictionary<Course, CourseCategory> CourseRequirements { get; set; }
        //public Dictionary<int, CourseCategory> CourseRequirements { get; set; }
        public List<CourseChoice> CourseRequirements { get; set; }
        public List<Package> Packages { get; set; }
        public byte Priority { get; set; }
    }
    public class Student
    {
        public int StudentId { get; set; }
        public Curriculum Curriculum { get; set; }
    }

    public class EventCandidate
    {
        public int RoomIndex { get; set; }
        public int InstructorIndex { get; set; }
        public int StartTimeIndex { get; set; }

        public EventCandidate(int roomIndex, int instructorIndex, int startTimeIndex)
        {
            RoomIndex = roomIndex;
            InstructorIndex = instructorIndex;
            StartTimeIndex = startTimeIndex;
        }

        public void Replace(int roomIndex, int instructorIndex, int startTimeIndex)
        {
            RoomIndex = roomIndex;
            InstructorIndex = instructorIndex;
            StartTimeIndex = startTimeIndex;
        }
    }
    public class EventHeuristic : IComparable<EventHeuristic>
    {
        public Event Event { get; set; }
        public int SeatsBeforeMaximum { get; set; }
        public int SeatsBeforeMinimum { get; set; }

        public EventHeuristic(Event evt, int numStudents, int minCap, int maxCap)
        {
            Event = evt;
            SeatsBeforeMinimum = numStudents < minCap ? minCap - numStudents : 0;
            SeatsBeforeMaximum = maxCap - numStudents;
        }
        public int CompareTo(EventHeuristic other)
        {
            if (other == null) return -1;
            if (this == other) return 0;
            if (this.Event == other.Event) return 0;
            if (this.SeatsBeforeMinimum > other.SeatsBeforeMinimum) return -1;
            if (this.SeatsBeforeMinimum < other.SeatsBeforeMinimum) return 1;
            if (this.SeatsBeforeMaximum > other.SeatsBeforeMaximum) return -1;
            if (this.SeatsBeforeMaximum < other.SeatsBeforeMaximum) return 1;
            return 0;
        }
    }
    public class InstructorHeuristic : IComparable<InstructorHeuristic>
    {
        public int Index { get; set; }
        public byte Satisfaction { get; set; }
        public int FreeTime { get; set; }
        public int Courses { get; set; }

        public InstructorHeuristic(int index, byte satisfaction, int freeTime, int numCourses)
        {
            Index = index;
            Satisfaction = satisfaction;
            FreeTime = freeTime;
            Courses = numCourses;
        }
        public int CompareTo(InstructorHeuristic other)
        {
            if (other == null) return -1;
            if (this == other) return 0;
            if (this.Index == other.Index) return 0;
            if (this.Satisfaction > other.Satisfaction) return -1;
            if (this.Satisfaction < other.Satisfaction) return 1;
            if (this.FreeTime > other.FreeTime) return -1;
            if (this.FreeTime < other.FreeTime) return 1;
            if (this.Courses > other.Courses) return 1;
            if (this.Courses < other.Courses) return -1;
            return 0;
        }
    }

    public class RoomHeuristic : IComparable<RoomHeuristic>
    {
        public int Index { get; set; }
        public byte Satisfaction { get; set; }
        public int Emptiness { get; set; }

        public RoomHeuristic(int index, byte satisfaction, int emptiness)
        {
            Index = index;
            Satisfaction = satisfaction;
            Emptiness = emptiness;
        }

        public int CompareTo(RoomHeuristic other)
        {
            if (other == null) return -1;
            if (this == other) return 0;
            if (this.Index == other.Index) return 0;
            if (this.Satisfaction > other.Satisfaction) return -1;
            if (this.Satisfaction < other.Satisfaction) return 1;
            if (this.Emptiness > other.Emptiness) return -1;
            if (this.Emptiness < other.Emptiness) return 1;
            return 0;
        }
    }

    public class ConflictedEventHeuristic : IComparable<ConflictedEventHeuristic>
    {
        public Event Event { get; set; }
        public int EventsRemaining { get; set; }
        public int SeatsBeforeMaximum { get; set; }

        public ConflictedEventHeuristic(Event evt, int eventsRemaining, int seatsRemaining)
        {
            Event = evt;
            EventsRemaining = eventsRemaining;
            SeatsBeforeMaximum = seatsRemaining;
        }
        public int CompareTo(ConflictedEventHeuristic other)
        {
            if (other == null) return -1;
            if (this == other) return 0;
            if (this.Event == other.Event) return 0;
            if (this.EventsRemaining > other.EventsRemaining) return -1;
            if (this.EventsRemaining < other.EventsRemaining) return 1;
            if (this.SeatsBeforeMaximum > other.SeatsBeforeMaximum) return 1;
            if (this.SeatsBeforeMaximum < other.SeatsBeforeMaximum) return -1;
            return 0;
        }
    }

    //public class CourseGrouping
    //{
    //    public int Students { get; set; }
    //    public Curriculum Curriculum { get; set; }
    //    public List<int> CourseIds { get; set; }
    //}

    class Timetabling
    {
        public bool IsSameCurriculum(Curriculum c1, Curriculum c2)
        {
            // TODO
            if (c1.Priority != c2.Priority) return false;
            foreach (var cc in c1.CourseRequirements)
            {
                if (!c2.CourseRequirements.Contains(cc)) return false;
            }
            foreach (var cc in c2.CourseRequirements)
            {
                if (!c1.CourseRequirements.Contains(cc)) return false;
            }
            return true;
        }

        //public Dictionary<int, CourseCategory> GetRequiredCoursesByStudent(int studentId)
        //{
        //    // TODO
        //    Dictionary<int, CourseCategory> requiredCourses = new Dictionary<int, CourseCategory>();
        //    return requiredCourses;
        //}

        public List<Student> GetStudents()
        {
            // TODO
            List<Student> students = new List<Student>();
            int[] numStudents = {134, 141, 166, 109};
            byte[] priorities = {0, 0, 1, 2};
            List<int>[] courseLists =
            {
                new List<int>() {1, 2, 3, 4, 5},
                new List<int>() {6, 7, 8, 9, 10},
                new List<int>() {2, 7, 4, 5, 3},
                new List<int>() {1, 3, 5, 9, 10}
            };
            int studentId = 1;
            for (int i = 0; i < 4; i++)
            {
                int nStudents = numStudents[i];
                byte priority = priorities[i];
                List<int> courseIds = courseLists[i];
                for (int j = 0; j < nStudents; j++, studentId++)
                {
                    List<CourseChoice> courseList = new List<CourseChoice>();
                    courseList.Add(new CourseChoice
                    {
                        SelectHowMany = courseIds.Count,
                        SelectFrom = courseIds.ToDictionary(c => c, c => 1.0)
                    });

                    Curriculum curriculum = new Curriculum { CourseRequirements = courseList, Priority = priority };
                    students.Add(new Student { StudentId = studentId, Curriculum = curriculum });
                }
            }
            return students;
        }

        public List<Curriculum> GetCurricula(List<Student> students)
        {
            List<Curriculum> curricula = new List<Curriculum>();
            foreach (Student student in students)
            {
                //Dictionary<int, CourseCategory> requiredCourses = GetRequiredCoursesByStudent(student.StudentId);
                Curriculum curriculum = curricula.Find(c => IsSameCurriculum(c, student.Curriculum));
                if (curriculum == null)
                {
                    curriculum = student.Curriculum;
                    curriculum.Students = 1;
                    curriculum.Packages = new List<Package>();
                    curricula.Add(curriculum);
                }
                else
                {
                    curriculum.Students += 1;
                    student.Curriculum = curriculum;
                }
            }
            return curricula;
        }

        public int MinEventsRequired(int demand, HashSet<RoomFeature> requiredFeatures, List<Room> rooms)
        {
            int maxCapacity = rooms.Where(r => demand >= r.MinCapacity && r.Features.IsSupersetOf(requiredFeatures)).Max(r => r.MaxCapacity);
            if (maxCapacity == 0) return 0;
            return demand / maxCapacity;
        }
        public List<Course> GetCourses(List<Curriculum> curricula, List<Room> rooms)
        {
            // TODO
            Dictionary<int, double> courseDemands = new Dictionary<int, double>();
            foreach (Curriculum curriculum in curricula)
            {
                int curriculumTotal = curriculum.Students;
                foreach (var courseGroup in curriculum.CourseRequirements)
                {
                    foreach (var courseProbability in courseGroup.SelectFrom)
                    {
                        int courseId = courseProbability.Key;
                        double likelihood = courseProbability.Value;
                        double demand = curriculumTotal*likelihood;
                        double courseDemand;
                        if (courseDemands.TryGetValue(courseId, out courseDemand))
                        {
                            courseDemands[courseId] = courseDemand + demand;
                        }
                        else
                        {
                            courseDemands.Add(courseId, demand);
                        }
                    }
                }
            }

            List<Course> courses = new List<Course>();
            foreach (var cd in courseDemands)
            {
                int courseId = cd.Key;
                int demand = (int) cd.Value;
                {
                    HashSet<RoomFeature> requiredFeatures = new HashSet<RoomFeature> { RoomFeature.Projecter, RoomFeature.PC };
                    courses.Add(new Course
                    {
                        CourseId = courseId,
                        ClassType = ClassType.Lecture1,
                        TimeUnits = 4,
                        Features = requiredFeatures,
                        MinEvents = MinEventsRequired(demand, requiredFeatures, rooms)
                    });
                }
                {
                    HashSet<RoomFeature> requiredFeatures = new HashSet<RoomFeature> {RoomFeature.PC};
                    courses.Add(new Course
                    {
                        CourseId = courseId,
                        ClassType = ClassType.Lab1,
                        TimeUnits = 4,
                        Features = requiredFeatures,
                        MinEvents = MinEventsRequired(demand, requiredFeatures, rooms)
                    });
                }
            }
            return courses;
        }

        public List<Room> GetRooms()
        {
            // TODO
            List<Room> rooms = new List<Room>();
            for (int i = 1; i < 5; i++)
            {
                rooms.Add(new Room
                {
                    RoomId = i,
                    MinCapacity = 10,
                    MaxCapacity = 40,
                    Features = new HashSet<RoomFeature> { RoomFeature.Projecter, RoomFeature.PC }
                });
            }
            return rooms;
        }

        public List<Time> GetTimes()
        {
            // TODO
            List<Time> times = new List<Time>();
            int interval = 30, firstMinute = 8 * 60, lastMinute = 23 * 60, timeId = 1;
            for (int day = 1; day < 8; day++)
            {
                for (int time = firstMinute; time < lastMinute; time += interval)
                {
                    times.Add(new Time
                    {
                        TimeId = timeId++,
                        StartTime = time,
                        //Duration = 30,
                        Day = day
                    });
                }
            }
            return times;
        }

        public List<Instructor> GetInstructors(List<Course> courses)
        {
            // TODO
            List<Instructor> instructors = new List<Instructor>();
            int courseIndex = 0, courseCount = courses.Count;
            for (int instructorId = 1; instructorId < 10; instructorId++)
            {
                instructors.Add(new Instructor
                {
                    InstructorId = instructorId,
                    CoursePreferences = new Dictionary<Course, InstructorPreference>
                    {
                        {courses[courseIndex++ % courseCount], InstructorPreference.Preferred },
                        {courses[courseIndex++ % courseCount], InstructorPreference.Preferred },
                        {courses[courseIndex++ % courseCount], InstructorPreference.Preferred },
                        {courses[courseIndex++ % courseCount], InstructorPreference.OK }
                    }
                });
            }
            return instructors;
        }

        private const byte /*NotRequired = 0x00, Required = 0x01, Optional1 = 0x02, Optional2 = 0x03,*/
            MaxFeatureSatisfaction = 0x0A, Preferred = 0x02, OK = 0x01, NotAtAll = 0x00;

        //public byte[,] GetCurriculumCourse(List<Curriculum> curricula, List<Course> courses)
        //{
        //    int courseCount = courses.Count, curriculumCount = curricula.Count;
        //    byte[,] matrix = new byte[courseCount, curriculumCount];

        //    for (int curriculumIndex = 0; curriculumIndex < curriculumCount; curriculumIndex++)
        //    {
        //        Dictionary<int, CourseCategory> courseRequirements = curricula[curriculumIndex].CourseRequirements;
        //        for (int courseIndex = 0; courseIndex < courseCount; courseIndex++)
        //        {
        //            int courseId = courses[courseIndex].CourseId;
        //            CourseCategory category;
        //            byte value = NotRequired;
        //            if (courseRequirements.TryGetValue(courseId, out category))
        //            {
        //                switch (category)
        //                {
        //                    case CourseCategory.Required:
        //                        value = Required;
        //                        break;
        //                    case CourseCategory.OptionalGroup1:
        //                        value = Optional1;
        //                        break;
        //                    case CourseCategory.OptionalGroup2:
        //                        value = Optional2;
        //                        break;
        //                }
        //            }
        //            matrix[courseIndex, curriculumIndex] = value;
        //        }
        //    }
        //    return matrix;
        //}

        public byte[,] GetRoomCourse(List<Room> rooms, List<Course> courses)
        {
            int roomCount = rooms.Count, courseCount = courses.Count;
            byte[,] matrix = new byte[courseCount, roomCount];
            for (int courseIndex = 0; courseIndex < courseCount; courseIndex++)
            {
                HashSet<RoomFeature> featuresRequired = courses[courseIndex].Features;
                int req = featuresRequired.Count;
                for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
                {
                    HashSet<RoomFeature> featuresOffered = rooms[roomIndex].Features;
                    int met = featuresRequired.Intersect(featuresOffered).Count();
                    matrix[courseIndex, roomIndex] = (byte)((double)met / (double)req * MaxFeatureSatisfaction);
                }
            }
            return matrix;
        }

        public byte[,] GetInstructorCourse(List<Instructor> instructors, List<Course> courses)
        {
            int instructorCount = instructors.Count, courseCount = courses.Count;
            byte[,] matrix = new byte[courseCount, instructorCount];
            for (int instructorIndex = 0; instructorIndex < instructorCount; instructorIndex++)
            {
                Dictionary<Course, InstructorPreference> prefs = instructors[instructorIndex].CoursePreferences;
                for (int courseIndex = 0; courseIndex < courseCount; courseIndex++)
                {
                    byte value = NotAtAll;
                    InstructorPreference preference;
                    if (prefs.TryGetValue(courses[courseIndex], out preference))
                    {
                        switch (preference)
                        {
                            case InstructorPreference.Preferred:
                                value = Preferred;
                                break;
                            case InstructorPreference.OK:
                                value = OK;
                                break;
                        }
                    }
                    matrix[courseIndex, instructorIndex] = value;
                }
            }
            return matrix;
        }

        public Event[,] GetRoomTime(List<Room> rooms, List<Time> times)
        {
            int roomCount = rooms.Count, timeCount = times.Count;
            Event[,] matrix = new Event[timeCount, roomCount];
            return matrix;
        }

        public Event[,] GetInstructorTime(List<Instructor> instructors, List<Time> times)
        {
            int instructorCount = instructors.Count, timeCount = times.Count;
            Event[,] matrix = new Event[timeCount, instructorCount];
            return matrix;
        }

        public int[] GetPackageTime(List<Time> times)
        {
            int timeCount = times.Count;
            int[] array = new int[timeCount];
            return array;
        }

        public int[,] GetPackageTime(List<Curriculum> curricula, List<Time> times)
        {
            int timeCount = times.Count;
            int packageCount = curricula.Sum(c => c.Packages.Count);
            int[,] matrix = new int[timeCount, packageCount];
            int packageIndex = 0;
            foreach (Curriculum curriculum in curricula)
            {
                foreach (Package package in curriculum.Packages)
                {
                    foreach (Event evt in package.Events)
                    {
                        int startIndex = evt.StartTime;
                        int endIndex = startIndex + evt.TimeUnits;
                        for (int timeIndex = startIndex; timeIndex < endIndex; timeIndex++)
                        {
                            matrix[timeIndex, packageIndex] += 1;
                        }
                    }
                    packageIndex++;
                }
            }
            return matrix;
        }

        private byte[,] /*mCurriculumCourse,*/ mRoomCourse, mInstructorCourse;
        private Event[,] mRoomTime, mInstructorTime;
        //private int[] aPackageTime;
        private List<Student> students;
        private List<Curriculum> curricula;
        private List<Course> courses;
        private List<Room> rooms;
        private List<Instructor> instructors;
        private List<Time> times;
        private List<Event> events;
        private Lookup<int, int> courseIdIndexes;
        //private List<Package> packages;
        //private int curriculumCount, courseCount, roomCount, instructorCount, timeCount;

        public void PopulateSchedule()
        {
            times = GetTimes();
            rooms = GetRooms();
            students = GetStudents();
            curricula = GetCurricula(students);
            courses = GetCourses(curricula, rooms);
            instructors = GetInstructors(courses);

            //mCurriculumCourse = GetCurriculumCourse(curricula, courses);
            mRoomCourse = GetRoomCourse(rooms, courses);
            mInstructorCourse = GetInstructorCourse(instructors, courses);
            mRoomTime = GetRoomTime(rooms, times);
            mInstructorTime = GetInstructorTime(instructors, times);

            int[] aPackageTime = GetPackageTime(times);
            //packages = new List<Package>();
            events = new List<Event>();

            int curriculumCount = curricula.Count;
            //int courseCount = courses.Count;
            //roomCount = rooms.Count;
            //instructorCount = instructors.Count;
            //timeCount = times.Count;

            // course id -> course indexes
            courseIdIndexes = (Lookup<int, int>) courses.ToLookup(c => c.CourseId, c => courses.IndexOf(c));

            for (int curriculumIndex = 0; curriculumIndex < curriculumCount; curriculumIndex++)
            {
                Curriculum curriculum = curricula[curriculumIndex];
                List<int> courseIndexes = GetCoursesInPackage(curriculum);

                //List<int> required = new List<int>(), optional1 = new List<int>(), optional2 = new List<int>();
                //for (int courseIndex = 0; courseIndex < courseCount; courseIndex++)
                //{
                    //byte category = mCurriculumCourse[courseIndex, curriculumIndex];
                    //switch (category)
                    //{
                    //    case Required:
                    //        required.Add(courseIndex);
                    //        break;
                    //    case Optional1:
                    //        optional1.Add(courseIndex);
                    //        break;
                    //    case Optional2:
                    //        optional2.Add(courseIndex);
                    //        break;
                    //}
                //}
                bool canCreatePackage = true;
                while (canCreatePackage && curriculum.Students > 0)
                {
                    Array.Clear(aPackageTime, 0, aPackageTime.Length);
                    //Package package = new Package {Events = new List<Event>(),Students = 0};

                    List<Event> existingEvents;
                    List<int> coursesWithoutExistingEvent;
                    bool allowConflicts = false;
                    GetExistingEvents(courseIndexes, out existingEvents, out coursesWithoutExistingEvent, allowConflicts);

                    int studentsAffected = curriculum.Students;

                    foreach (var existingEvent in existingEvents)
                    {
                        studentsAffected = AddEventToPackage(studentsAffected, existingEvent, aPackageTime, true);
                    }

                    List<Event> newEvents = new List<Event>();
                    bool topTierOnly = true;
                    foreach (int courseIndex in coursesWithoutExistingEvent)
                    {
                        Event newEvent = CreateEvent(courseIndex, aPackageTime, topTierOnly, allowConflicts);
                        if (newEvent == null)
                        {
                            canCreatePackage = false;
                            break;
                        }
                        studentsAffected = AddEventToPackage(studentsAffected, newEvent, aPackageTime, false);
                        newEvents.Add(newEvent);
                    }

                    if (canCreatePackage)
                    {
                        Package package = null;
                        if (newEvents.Count == 0)
                        {
                            // only existing
                            foreach (var p in curriculum.Packages)
                            {
                                List<Event> inPackage = p.Events;
                                bool diff = existingEvents.Any(e => !inPackage.Contains(e)) || inPackage.Any(e => !existingEvents.Contains(e));
                                if (!diff)
                                {
                                    package = p;
                                    foreach (var e in package.Events)
                                    {
                                        e.Students += studentsAffected;
                                    }
                                    break;
                                }
                            }
                        }

                        if (package == null)
                        {
                            package = new Package { Events = new List<Event>(), Students = 0 };
                            package.Events.AddRange(existingEvents);
                            package.Events.AddRange(newEvents);
                            curriculum.Packages.Add(package);
                            newEvents.ForEach(e => events.Add(e));
                            foreach (var e in package.Events)
                            {
                                e.Students += studentsAffected;
                                e.Packages += 1;
                            }
                        }

                        package.Students += studentsAffected;
                        curriculum.Students -= studentsAffected;
                    }
                    else
                    {
                        ClearEventsFromPackage(newEvents);
                    }

                }
            }

        }

        public List<int> GetCoursesInPackage(Curriculum curriculum)
        {
            // TODO
            List<int> courseIndexes = new List<int>();
            foreach (var choiceGroup in curriculum.CourseRequirements)
            {
                foreach (var choiceProbability in choiceGroup.SelectFrom)
                {
                    int courseId = choiceProbability.Key;
                    courseIndexes.AddRange(courseIdIndexes[courseId]);
                }
            }
            return courseIndexes;
        }

        public bool GetSuitableExistingEvents(List<List<Event>> existingEvents, Stack<Event> eventCandidates, int[] timeArray, int currentCourse, ref int minConflicts, ref List<Event> suitableEvents)
        {
            if (currentCourse == existingEvents.Count)
            {
                Array.Clear(timeArray, 0, timeArray.Length);
                foreach (var evt in eventCandidates)
                {
                    int startTimeIndex = evt.StartTime;
                    int endTimeIndex = startTimeIndex + evt.TimeUnits;
                    for (int t = startTimeIndex; t < endTimeIndex; t++)
                    {
                        timeArray[t] += 1;
                    }
                }
                int conflicts = timeArray.Where(c => c > 1).Sum();
                if (conflicts < minConflicts)
                {
                    suitableEvents.Clear();
                    suitableEvents.AddRange(eventCandidates);
                    minConflicts = conflicts;
                }
                return minConflicts == 0;
            }

            foreach (var evt in existingEvents[currentCourse])
            {
                eventCandidates.Push(evt);

                bool isConflictFree = GetSuitableExistingEvents(existingEvents, eventCandidates, timeArray, currentCourse + 1, ref minConflicts, ref suitableEvents);
                if (isConflictFree) return true;

                eventCandidates.Pop();
            }
            return false;
        }
        public void GetExistingEvents(List<int> coursesInCurriculum, out List<Event> existingEvents, out List<int> coursesWithoutExistingEvent, bool allowConflicts)
        {
            existingEvents = new List<Event>();
            coursesWithoutExistingEvent = new List<int>();
            List<List<Event>> coursesWithExistingEvents = new List<List<Event>>();
            foreach (int courseIndex in coursesInCurriculum)
            {
                List<EventHeuristic> existingEventsForCourse = new List<EventHeuristic>();
                foreach (Event evt in events)
                {
                    if (evt.Course != courseIndex) continue;
                    Room room = rooms[evt.Room];
                    int min = room.MinCapacity;
                    int max = room.MaxCapacity;
                    int numStudents = evt.Students;
                    if (numStudents < max)
                    {
                        existingEventsForCourse.Add(new EventHeuristic(evt, numStudents, min, max));
                    }
                }
                if (existingEventsForCourse.Count > 0)
                {
                    existingEventsForCourse.Sort();
                    coursesWithExistingEvents.Add(existingEventsForCourse.Select(e => e.Event).ToList());
                }
                else
                {
                    coursesWithoutExistingEvent.Add(courseIndex);
                }
            }

            if (coursesWithExistingEvents.Count == 0) return;

            int conflicts = Int32.MaxValue;
            bool conflictFree = GetSuitableExistingEvents(coursesWithExistingEvents, new Stack<Event>(), GetPackageTime(times), 0, ref conflicts, ref existingEvents);
            if (conflictFree) return;

            // conflicts in existing events
            if (allowConflicts) return;
            List<Event> conflictingEvents = RemoveConflictingEvents(existingEvents);
            foreach (var e in conflictingEvents)
            {
                coursesWithoutExistingEvent.Add(e.Course);
            }
        }

        public List<Event> RemoveConflictingEvents(List<Event> existingEvents)
        {
            List<Event> ignored = new List<Event>();
            Dictionary<Event, int> conflictsInEvent = new Dictionary<Event, int>();
            Dictionary<Event, List<int>> eventTimes = new Dictionary<Event, List<int>>();
            foreach (var e in existingEvents)
            {
                List<int> timesInEvent = new List<int>();
                int startTime = e.StartTime;
                int endTime = startTime + e.TimeUnits;
                for (int t = startTime; t < endTime; t++)
                {
                    timesInEvent.Add(t);
                }
                eventTimes.Add(e, timesInEvent);
                conflictsInEvent.Add(e, 0);
            }
            int eventCount = existingEvents.Count;

            for (;;)
            {
                for (int i = 0; i < eventCount - 1; i++)
                {
                    Event first = existingEvents[i];
                    for (int j = i + 1; j < eventCount; j++)
                    {
                        Event second = existingEvents[j];
                        int conflictingTimes = eventTimes[first].Intersect(eventTimes[second]).Count();
                        conflictsInEvent[first] += conflictingTimes;
                        conflictsInEvent[second] += conflictingTimes;
                    }
                }
                int maxConflicts = conflictsInEvent.Values.Max();
                if (maxConflicts == 0) break;

                List<ConflictedEventHeuristic> mostConflictedEvents =
                    conflictsInEvent.Where(kvp => kvp.Value == maxConflicts).Select(
                        kvp =>
                        {
                            Event evt = kvp.Key;
                            int eventsRemaining = courses[evt.Course].MinEvents;
                            int seatsRemaining = rooms[evt.Room].MaxCapacity - evt.Students;
                            return new ConflictedEventHeuristic(evt, eventsRemaining, seatsRemaining);
                        }).ToList();
                mostConflictedEvents.Sort();

                Event _event = mostConflictedEvents[0].Event;
                existingEvents.Remove(_event);
                eventTimes.Remove(_event);
                conflictsInEvent.Remove(_event);
                foreach (var e in conflictsInEvent.Keys)
                {
                    conflictsInEvent[e] = 0;
                }
                eventCount--;
                ignored.Add(_event);
            }

            return ignored;
        }

        public List<RoomHeuristic> GetSuitableRooms(int courseIndex)
        {
            List<RoomHeuristic> suitableRooms = new List<RoomHeuristic>();
            int roomCount = rooms.Count, timeCount = times.Count;
            for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
            {
                byte satisfaction = mRoomCourse[courseIndex, roomIndex];
                int emptySlots = 0;
                for (int timeIndex = 0; timeIndex < timeCount; timeIndex++)
                {
                    if (mRoomTime[timeIndex, roomIndex] == null) emptySlots++;
                }
                suitableRooms.Add(new RoomHeuristic(roomIndex, satisfaction, emptySlots));
            }
            suitableRooms.Sort();
            return suitableRooms;
        }

        public List<InstructorHeuristic> GetSuitableInstructors(int courseIndex)
        {
            List<InstructorHeuristic> suitableInstructors = new List<InstructorHeuristic>();
            int instructorCount = instructors.Count, timeCount = times.Count;
            for (int instructorIndex = 0; instructorIndex < instructorCount; instructorIndex++)
            {
                byte satisfaction = mInstructorCourse[courseIndex, instructorIndex];
                if (satisfaction == NotAtAll) continue;
                int coursesTaught = instructors[instructorIndex].CoursePreferences.Count;
                int freeTimes = 0;
                for (int timeIndex = 0; timeIndex < timeCount; timeIndex++)
                {
                    if (mInstructorTime[timeIndex, instructorIndex] == null) freeTimes++;
                }
                suitableInstructors.Add(new InstructorHeuristic(instructorIndex, satisfaction, freeTimes, coursesTaught));
            }
            suitableInstructors.Sort();
            return suitableInstructors;
        }

        public int GetFirstFreeTimeSlotForInstructor(int instructorIndex, int requiredTimeSlots, int startTimeIndex)
        {
            int freeTimeSlots = 0, currentDay = -1, newTime = -1, timeCount = times.Count;
            for (int timeIndex = startTimeIndex; timeIndex < timeCount; timeIndex++)
            {
                if (mInstructorTime[timeIndex, instructorIndex] == null)
                {
                    int newDay = times[timeIndex].Day;
                    if (newDay != currentDay)
                    {
                        currentDay = newDay;
                        freeTimeSlots = 0;
                    }
                    if (freeTimeSlots == 0)
                    {
                        newTime = timeIndex;
                    }
                    freeTimeSlots++;
                }
                else
                {
                    freeTimeSlots = 0;
                }
                if (requiredTimeSlots == freeTimeSlots)
                {
                    return newTime;
                }
            }
            return -1;
        }

        public int GetFirstFreeTimeSlotForRoom(int roomIndex, int requiredTimeSlots, int startTimeIndex)
        {
            int freeTimeSlots = 0, currentDay = -1, newTime = -1, timeCount = times.Count;
            for (int timeIndex = startTimeIndex; timeIndex < timeCount; timeIndex++)
            {
                if (mRoomTime[timeIndex, roomIndex] == null)
                {
                    int newDay = times[timeIndex].Day;
                    if (newDay != currentDay)
                    {
                        currentDay = newDay;
                        freeTimeSlots = 0;
                    }
                    if (freeTimeSlots == 0)
                    {
                        newTime = timeIndex;
                    }
                    freeTimeSlots++;
                }
                else
                {
                    freeTimeSlots = 0;
                }
                if (requiredTimeSlots == freeTimeSlots)
                {
                    return newTime;
                }
            }
            return -1;
        }

        public int GetFeasibleStartTimeForInstructorAndRoom(int instructorIndex, int roomIndex, int requiredTimeSlots, int startTimeIndex = 0)
        {
            int instructorStartTime = -1, roomStartTime = -1;
            for (;;)
            {
                instructorStartTime = GetFirstFreeTimeSlotForInstructor(instructorIndex, requiredTimeSlots, startTimeIndex);
                if (instructorStartTime == -1)
                {
                    // instructor not available
                    return -1;
                }
                if (instructorStartTime == roomStartTime)
                {
                    // feasible solution
                    return instructorStartTime;
                }
                startTimeIndex = instructorStartTime;
                roomStartTime = GetFirstFreeTimeSlotForRoom(roomIndex, requiredTimeSlots, startTimeIndex);
                if (roomStartTime == -1)
                {
                    // room not available
                    return -1;
                }
                if (instructorStartTime == roomStartTime)
                {
                    // feasible solution
                    return roomStartTime;
                }
                startTimeIndex = roomStartTime;
            }
        }

        public int ConflictsInPackageForTime(int startTimeIndex, int requiredTimeSlots, int[] aPackageTime)
        {
            int conflicts = 0;
            for (int i = 0; i < requiredTimeSlots; i++)
            {
                conflicts += aPackageTime[startTimeIndex + i];
            }
            return conflicts;
        }

        public int GetSuitableStartTimeForInstructorAndRoom(int instructorIndex, int roomIndex, int requiredTimeSlots, int[] aPackageTime, out int conflictsInPackage)
        {
            int suitableTime = -1;
            conflictsInPackage = -1;
            int startTimeIndex = GetFeasibleStartTimeForInstructorAndRoom(instructorIndex, roomIndex, requiredTimeSlots);
            while (startTimeIndex != -1)
            {
                // has feasible time
                if (suitableTime == -1)
                {
                    suitableTime = startTimeIndex;
                    conflictsInPackage = ConflictsInPackageForTime(startTimeIndex, requiredTimeSlots, aPackageTime);
                }
                else
                {
                    // compare time suitability
                    int conflicts = ConflictsInPackageForTime(startTimeIndex, requiredTimeSlots, aPackageTime);
                    if (conflicts < conflictsInPackage)
                    {
                        // replace existing
                        suitableTime = startTimeIndex;
                        conflictsInPackage = conflicts;
                    }
                }
                if (conflictsInPackage == 0)
                {
                    // best time ever
                    return suitableTime;
                }
                startTimeIndex = GetFeasibleStartTimeForInstructorAndRoom(instructorIndex, roomIndex, requiredTimeSlots, startTimeIndex + 1);
            }
            return suitableTime;
        }
        public EventCandidate GetSuitableEvent(List<RoomHeuristic> suitableRooms, List<InstructorHeuristic> suitableInstructors, int[] aPackageTime, int requiredTimeSlots, bool topTierOnly, bool allowConflicts)
        {
            EventCandidate eventCandidate = null;

            List<byte> roomSatisfactions = new List<byte>();
            List<byte> instructorSatisfactions = new List<byte>();
            foreach (var r in suitableRooms)
            {
                byte satisfaction = r.Satisfaction;
                if (!roomSatisfactions.Contains(satisfaction)) roomSatisfactions.Add(satisfaction);
                if (topTierOnly) break;
            }
            foreach (var i in suitableInstructors)
            {
                byte satisfaction = i.Satisfaction;
                if (!instructorSatisfactions.Contains(satisfaction)) instructorSatisfactions.Add(satisfaction);
                if (topTierOnly) break;
            }

            int minCostForPackage = 0;
            foreach (byte roomSatisfaction in roomSatisfactions)
            {
                foreach (byte instructorSatisfaction in instructorSatisfactions)
                {
                    foreach (var instructorCandidate in suitableInstructors)
                    {
                        if (instructorCandidate.Satisfaction != instructorSatisfaction) continue;
                        int instructorIndex = instructorCandidate.Index;
                        foreach (var roomCandidate in suitableRooms)
                        {
                            if (roomCandidate.Satisfaction != roomSatisfaction) continue;
                            int roomIndex = roomCandidate.Index;
                            int cost;
                            int startTimeIndex = GetSuitableStartTimeForInstructorAndRoom(instructorIndex, roomIndex, requiredTimeSlots, aPackageTime, out cost);
                            if (startTimeIndex == -1) continue;
                            if (eventCandidate == null)
                            {
                                eventCandidate = new EventCandidate(roomIndex, instructorIndex, startTimeIndex);
                                minCostForPackage = cost;
                            }
                            else if (cost < minCostForPackage)
                            {
                                eventCandidate.Replace(roomIndex, instructorIndex, startTimeIndex);
                                minCostForPackage = cost;
                            }
                            if (cost == 0) return eventCandidate;
                        }
                    }
                    if (allowConflicts && eventCandidate != null) return eventCandidate;
                }
            }
            return null;
        }
        public Event CreateEvent(int courseIndex, int[] aPackageTime, bool topTierOnly, bool allowConflicts)
        {
            Event suitableEvent = null;
            Course course = courses[courseIndex];
            if (course.MinEvents <= 0) return suitableEvent;

            // find sorted list of suitable rooms
            List<RoomHeuristic> suitableRooms = GetSuitableRooms(courseIndex);

            // find sorted list of suitable instructors
            List<InstructorHeuristic> suitableInstructors = GetSuitableInstructors(courseIndex);

            // create a suitable event
            int requiredTimeSlots = course.TimeUnits;
            EventCandidate eventCandidate = GetSuitableEvent(suitableRooms, suitableInstructors, aPackageTime, requiredTimeSlots, topTierOnly, allowConflicts);
            if (eventCandidate != null)
            {
                suitableEvent = new Event
                {
                    StartTime = eventCandidate.StartTimeIndex,
                    TimeUnits = requiredTimeSlots,
                    Course = courseIndex,
                    Room = eventCandidate.RoomIndex,
                    Instructor = eventCandidate.InstructorIndex,
                    Students = 0, // TODO
                    Packages = 0
                };
            }
            else
            {
                // no solution
            }
            return suitableEvent;
        }

        public int AddEventToPackage(int numStudents, Event evt, int[] aPackageTime, bool isExistingEvent)
        {
            int startTimeIndex = evt.StartTime;
            int endTimeIndex = startTimeIndex + evt.TimeUnits;
            int instructorIndex = evt.Instructor;
            int roomIndex = evt.Room;
            int studentsInEvent = evt.Students;
            int seatsRemaining = rooms[roomIndex].MaxCapacity - studentsInEvent;
            int maxParticipants = Math.Min(seatsRemaining, numStudents);
            //package.Students = Math.Min(maxParticipants, package.Students);
            //package.Events.Add(evt);
            if (isExistingEvent)
            {
                for (int t = startTimeIndex; t < endTimeIndex; t++)
                {
                    aPackageTime[t] += 1;
                }
            }
            else
            {
                for (int t = startTimeIndex; t < endTimeIndex; t++)
                {
                    aPackageTime[t] += 1;
                    mInstructorTime[t, instructorIndex] = evt;
                    mRoomTime[t, roomIndex] = evt;
                }
                courses[evt.Course].MinEvents -= 1;
            }
            return maxParticipants;
        }

        public void ClearEventsFromPackage(List<Event> newEvents)
        {
            foreach (var evt in newEvents)
            {
                int startTimeIndex = evt.StartTime;
                int endTimeIndex = startTimeIndex + evt.TimeUnits;
                int instructorIndex = evt.Instructor;
                int roomIndex = evt.Room;
                for (int t = startTimeIndex; t < endTimeIndex; t++)
                {
                    mInstructorTime[t, instructorIndex] = null;
                    mRoomTime[t, roomIndex] = null;
                }
                courses[evt.Course].MinEvents += 1;
            }
        }


        public void DisplaySchedule()
        {
            StringBuilder str = new StringBuilder("Schedule:\n");
            int timeCount = times.Count, roomCount = rooms.Count;
            for (int timeIndex = 0; timeIndex < timeCount; timeIndex++)
            {
                for (int roomIndex = 0; roomIndex < roomCount; roomIndex++)
                {
                    Event evt = mRoomTime[timeIndex, roomIndex];
                    if (evt == null)
                    {
                        str.Append($" | {"",4} {"",12}");
                    }
                    else
                    {
                        Course course = courses[evt.Course];
                        str.Append($" | {course.CourseId,4} {course.ClassType,12}");
                    }
                }
                str.Append("\n  ");
                for (int i = 0; i < roomCount*20; i++) str.Append("-");
                str.Append("\n");
            }
            File.WriteAllText("Room_Time.txt", str.ToString());
        }

        public void DisplayPackages(int studentId)
        {
            StringBuilder str=new StringBuilder($"Student ID: {studentId}\n");
            Student student = students.Find(s => s.StudentId == studentId);
            if (student == null)
            {
                str.Append("not found.");
                Console.WriteLine(str);
                return;
            }
            List<Package> packages = student.Curriculum.Packages;
            for (int i=0; i< packages.Count;i++)
            {
                Package p = packages[i];
                str.Append($"\n######## Package {i+1} - {p.Students} available ########\n");
                foreach (Event e in p.Events)
                {
                    Course c = courses[e.Course];
                    int roomId = rooms[e.Room].RoomId;
                    int instructorId = instructors[e.Instructor].InstructorId;
                    Time t = times[e.StartTime];
                    int start = t.StartTime;
                    int end = start + e.TimeUnits*30;
                    string startTime = $"{t.Day} {start / 60}:{start % 60}";
                    string endTime = $"{end/60}:{end%60}";
                    str.Append($"* course: {c.CourseId,2} {c.ClassType,12} - room: {roomId} - inst: {instructorId} - time: {startTime} ~ {endTime}\n");
                }
            }
            Console.WriteLine(str);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Timetabling scheduler=new Timetabling();
            scheduler.PopulateSchedule();
            //scheduler.DisplaySchedule();
            scheduler.DisplayPackages(300);
        }
    }
}
