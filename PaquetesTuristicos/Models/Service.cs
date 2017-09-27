using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    public class Service
    {
        public ObjectId ServiceId { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public byte[] Image { get; set; }
        public string province { get; set; }
        public string canton { get; set; }
        public string district { get; set; }
        public int idCategory { get; set; }
        public string imagenID { get; set; }
        public Categoria categoria { get; set; }
        public List<Fare> fare { get; set; }
    }
}