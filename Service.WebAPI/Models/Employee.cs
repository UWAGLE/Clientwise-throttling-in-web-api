using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Service.WebAPI.Models
{
    public class Employee
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string BirthDate { get; set; }
        public string Designation { get; set; }
        public string Experience { get; set; }
    }
}