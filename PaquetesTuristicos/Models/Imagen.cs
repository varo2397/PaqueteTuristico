using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    public class Imagen
    {
        public ObjectId imagId { get; set; }
        public string imageGridFS { get; set; }
        public string serviceId { get; set; }
        public byte[] Image { get; set; }


    }
}