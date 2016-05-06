﻿using MooshakPP.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MooshakPP.Models.ViewModels
{
    public class CreateUserViewModel
    {
        public List<ApplicationUser> allUsers { get; set; }
        public bool isTeacher { get; set; }
        public User newUser { get; set; }
    }
}