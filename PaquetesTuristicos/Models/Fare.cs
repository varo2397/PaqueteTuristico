﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    [BsonIgnoreExtraElements]
    public class Fare
    {
        public ObjectId id { get; set; }
        public string serviceId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int precio { get; set; }
        public int qty { get; set; }
    }
}