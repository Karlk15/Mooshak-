﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MooshakPP.Models.ViewModels;
using MooshakPP.Models.Entities;
using System.Configuration;
using System.IO;
using System.Diagnostics;


namespace MooshakPP.Services
{
    public class StudentService
    {
        private Models.ApplicationDbContext db;

        public StudentService()
        {
            db = new Models.ApplicationDbContext();
        }

        public IndexViewModel Index(string userId, int? courseId, int? assignmentId/*, int milestoneId*/)
        {
            IndexViewModel newIndex = new IndexViewModel();
            if (courseId != null)
            {
                newIndex.courses = GetCourses(userId);
                newIndex.assignments = GetAssignments((int)courseId);
                if (assignmentId != null)
                    newIndex.milestones = GetMilestones((int)assignmentId);
                newIndex.currentCourse = GetCourseByID((int)courseId);
                //newIndex.studentSubmissions = GetSubmissions(userId);
            }
            return newIndex;
        }

        public SubmissionViewModel Submissions(string userId, int milestoneId)
        {
            SubmissionViewModel mySubmissions = new SubmissionViewModel();
            mySubmissions.mySubmissions = GetSubmissions(userId, milestoneId);
            return mySubmissions;
        }

        public DescriptionViewModel Description(int milestoneId)
        {
            DescriptionViewModel description = new DescriptionViewModel();
            description.milestone = GetMilestoneByID(milestoneId);
            return description;
        }

        public DetailsViewModel Details(int submissionId)
        {
            return null;
        }

        //mileID is milestone ID
        public bool CreateSubmission(string userID, string userName, int mileID, HttpPostedFileBase file)
        {

            string code;
            string fileName = file.FileName;

            //produce uploaded code
            using (StreamReader sr = new StreamReader(file.InputStream))
            {
                code = sr.ReadToEnd();
            }

            //Get the submission directory relative location from AppSettings
            string submissionDir = ConfigurationManager.AppSettings["SubmissionDir"];

            //Make relative path absolute
            submissionDir = HttpContext.Current.Server.MapPath(submissionDir);

            //Get working directory information
            //Milestone milestone = GetMilestoneByID(mileID); uncomment me when milestones exist

            //PLACEHOLDER
            Milestone milestone = new Milestone();
            milestone.name = "Gagnaskipan";
            milestone.assignmentID = 2;
            //END OF PLACEHOLDER
            Assignment assignment = GetAssignmentByID(milestone.assignmentID);
            Course course = GetCourseByID(assignment.courseID);
            List<TestCase> testCases = GetTestCasesByMilestoneID(1); //PLACEHOLDER
            submissionDir += "\\" +course.name+ "\\" + "\\"+assignment.title+"\\" + "\\"+milestone.name+"\\";
            
            string userSubmission = submissionDir + userName + "\\Submission ";

            //Find an unused submission number
            int i = 1;
            while (Directory.Exists(userSubmission + i))
            {
                i++;
            }
            // the "\\" is vital
            userSubmission += i + "\\";

            Directory.CreateDirectory(userSubmission);

            var workingFolder = userSubmission;
            var cppFileName = fileName;
            var exeFilePath = workingFolder + fileName;

            // Write the code to a file, such that the compiler
            // can find it:
            System.IO.File.WriteAllText(workingFolder + cppFileName, code);

            var compilerFolder = "C:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\VC\\bin\\";


            Process compiler = new Process();
            compiler.StartInfo.FileName = "cmd.exe";
            compiler.StartInfo.WorkingDirectory = workingFolder;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.UseShellExecute = false;

            bool failed = false;
            foreach(TestCase test in testCases)
            {
                compiler.Start();
                compiler.StandardInput.WriteLine("\"" + compilerFolder + "vcvars32.bat" + "\"");
                compiler.StandardInput.WriteLine("cl.exe /nologo /EHsc " + cppFileName);
                compiler.StandardInput.WriteLine("exit");
                string output = compiler.StandardOutput.ReadToEnd();
                compiler.WaitForExit();
                compiler.Close();

                using (StreamReader sr = new StreamReader(test.outputUrl))
                {
                    string expected = sr.ReadToEnd();
                    if(expected != output)
                    {
                        failed = true;
                    }
                }
            }

            //TODO, MAKE SURE CRASHES ARE NOT ACCEPTED, PROBABLY WITH TRY/CATCH
            Submission submission = new Submission();
            submission.fileURL = userSubmission;
            submission.milestoneID = milestone.ID;
            //not yet rated
            submission.status = result.none;
            submission.userID = userID;
            if(!failed)
            {
                submission.status = result.Accepted;
                db.Submissions.Add(submission);
                db.SaveChanges();
            }

            return true;
        }

        public List<TestCase> GetTestCasesByMilestoneID(int milestoneID)
        {
            List<TestCase> testCases = (from c in GetTestCases()
                                        where c.milestoneID == milestoneID
                                        select c).ToList();
            return testCases;
        }

        public List<TestCase> GetTestCases()
        {
            List<TestCase> testCases = (from c in db.Testcases
                                        select c).ToList();
            return testCases;
        }

        public int? GetFirstCourse(string userId)
        {
            List<Course> courses = GetCourses(userId);
            if (courses.Count != 0)
            {
                return courses[0].ID;
            }
            return null;
        }

        public int? GetFirstAssignment(int courseId)
        {
            List<Assignment> assignments = GetAssignments(courseId);
            if (assignments.Count != 0)
            {
                return assignments[0].ID;
            }
            return null;
        }

        public int? GetFirstMilestone(int? assignmentId)
        {
            if(assignmentId != null)
            {
                List<Milestone> milestones = GetMilestones((int)assignmentId);
                if (milestones.Count != 0)
                {
                    return milestones[0].ID;
                }
            }
            return null;
        }

        public Course GetCourse(int courseID)
        {
            Course theCourse = GetCourseByID(courseID);
            return theCourse;
        }

        protected List<Course> GetCourses(string userId)
        {
            List<Course> courses = (from c in db.UsersInCourses
                           where c.userID == userId
                           select c.course).ToList();
            return courses;
        }

        protected Course GetCourseByID(int courseId)
        {
            Course theCourse = (from c in db.Courses
                             where c.ID == courseId
                             select c).SingleOrDefault();
            
            return theCourse;
        }

        protected List<Assignment> GetAssignments(int courseId)
        {
            List<Assignment> assignments = (from a in db.Assignments
                               where a.courseID == courseId
                               select a).ToList();

            return assignments;
        }

        protected Assignment GetAssignmentByID(int assignmentId)
        {
            Assignment assignment = (from a in db.Assignments
                              where a.ID == assignmentId
                              select a).FirstOrDefault();
            return assignment;
        }

        protected List<Milestone> GetMilestones(int assignmentId)
        {
            List<Milestone> milestones = (from m in db.Milestones
                              where m.assignmentID == assignmentId
                              select m).ToList();
            return milestones;
        }

        protected Milestone GetMilestoneByID(int milestoneId)
        {
            Milestone milestone = (from m in db.Milestones
                             where m.ID == milestoneId
                             select m).FirstOrDefault();
            return milestone;
        }

        protected List<Submission> GetSubmissions(string userId, int milestoneId)
        {
            List<Submission> submissions = (from s in db.Submissions
                               where s.userID == userId && s.milestoneID == milestoneId
                               select s).ToList();
            return submissions;
        }

        protected Submission GetSubmissionByID(int submissionId)
        {
            Submission submission = (from s in db.Submissions
                              where s.ID == submissionId
                              select s).FirstOrDefault();
            return submission;
        }

        
    }
}