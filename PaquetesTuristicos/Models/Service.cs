using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    [BsonIgnoreExtraElements]
    public class Service
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }

        public string idCreador { get; set; }


        public string province { get; set; }
        public string canton { get; set; }
        public string district { get; set; }

        public List<Imagen> ImagList { get; set; }

        public string town { get; set; }
        public string KmDistance { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }

        public int idCategory { get; set; }


        public Categoria categoria { get; set; }
        public List<Fare> fare { get; set; }


        public void loadService(string id)
        {

            MongoConnect mc = new MongoConnect();
            Service tempService = mc.getid(new ObjectId(id));
            //id = tempService.id;
            name = tempService.name;
            owner = tempService.owner;
            ImagList = tempService.ImagList;
            province = tempService.province;
            canton = tempService.canton;
            district = tempService.district;
            town = tempService.town;
            KmDistance = tempService.KmDistance;
            latitude = tempService.latitude;
            longitude = tempService.longitude;
            fare = tempService.fare;

        }

    }



}