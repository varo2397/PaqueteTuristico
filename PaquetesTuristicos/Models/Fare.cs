using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    public class Fare
    {
        public ObjectId fareId { get; set; }
        public string serviceId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}