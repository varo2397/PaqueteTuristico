using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    public class Service
    {
        // editar attributos a los que se van a utilizar..
        public int ServiceId { get; set; }
        public string Description { get; set; }
        public double Fare { get; set; }
        public byte[] Image { get; set; }
        public string province { get; set; }
        public string canton { get; set; }
        public string district { get; set; }
        public string details { get; set; }
        public Category category { get; set; }
    }
}