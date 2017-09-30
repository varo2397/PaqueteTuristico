using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    [BsonIgnoreExtraElements]
    public class Imagen
    {
        public ObjectId imagId { get; set; }
        public string imageGridFS { get; set; }
        public string serviceId { get; set; }
        public string Image { get; set; }


    }
}