﻿using MooshakPP.Models.Entities;
using MooshakPP.Models.ViewModels;
using MooshakPP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooshakPP.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        private AdminService service = new AdminService();

        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ManageCourse(int? ID)
        {
            ManageCourseViewModel model = service.GetCourses();
            ViewBag.selectedCourse = ID;
            return View(model);
        }



        //The action variable is passed by the button pressed in the view to determine what action was requested 
        //Currently the delete button is the only one who can pass a value, others use actionlinks
        [HttpPost]
        public ActionResult ManageCourse(Course newCourse, int? ID, string action)
        {
            if (action == "delete")
            {
                if (ID != null)
                {
                    int courseID = Convert.ToInt32(ID);
                    List<Course> courses = service.GetCourses().courses;
                    Course course = (from n in courses
                                     where n.ID == courseID
                                     select n).FirstOrDefault();
                    //if course is default, the ID was not found, should never happen
                    service.RemoveCourse(course);
                    return RedirectToAction("ManageCourse");
                }
            }
            ManageCourseViewModel model = service.GetCourses();
            if (!string.IsNullOrEmpty(newCourse.name))
            {
                service.CreateCourse(newCourse);
                return RedirectToAction("ManageCourse", model);
            }
            return View(model);
        }

        

        [HttpGet]
        public ActionResult CreateUser()
        {
            service.GetAllUsers();
            return View();
        }
        /// <summary>
        /// collection[1] seeks 
        /// collection[2] seeks 
        /// </summary>
        [HttpPost]
        public ActionResult CreateUser(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                string temp = collection[1];
                string b = collection[2];
                string[] userName = temp.Split(',');
                string[] isTeacher = b.Split(',');

                for (int i = 0; i < 10; i++)
                {
                    if (userName[i] == "")
                    {

                    }
                    else
                    {
                        if (isTeacher[i] == "true")
                        {
                            service.CreateUser(userName[i], true);
                        }
                        else if (isTeacher[i] == "false")
                        {
                            service.CreateUser(userName[i], false);
                        }
                    }
                }
            }
            return View();
        }

        //ID is the course.ID
        [HttpGet]
        public ActionResult ConnectUser(int? ID)
        {
            AddConnectionsViewModel model = service.AddConnections();
            return View(model);
        }

        //ID is the course.ID
        //users is a string array of users you are performing an action on
        //action specifies whether you are adding or removing students, defined by which button you pressed
        [HttpPost]
        public ActionResult ConnectUser(int? ID, int[] users)
        {
            return RedirectToAction("ConnectUser");
        }
    }
}