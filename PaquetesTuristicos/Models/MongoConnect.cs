using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.IO;

namespace PaquetesTuristicos.Models
{
    public class MongoConnect
    {
        public string connectionString { get; set; }
        public string databaseName { get; set; }
        public string collectionName { get; set; }
        private MongoClient mongoClient;
        private MongoServer mongoServer;
        private MongoDatabase dataBase;

        public MongoConnect()
        {

            connectionString = "mongodb://localhost:27017";
            databaseName = "Progra";

            try
            {
                mongoClient = new MongoClient(connectionString);
                mongoServer = mongoClient.GetServer();
                dataBase = mongoServer.GetDatabase(databaseName);
            }
            catch (Exception e)
            {
                throw new Exception("Can not access to db server.", e);
            }
        }

        //modificar
        public List<Service> getALLServices()
        {
            List<Service> services;
            try
            {
                services = dataBase.GetCollection<Service>("Services").FindAll().ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Can not access to db server.", e);
            }
            return services;
        }


        public void addService(Service service, string url)
        {

            /*var tempService = new Service
            {
                name = service.name,
                owner = service.owner,
                province = service.province,
                canton = service.canton,
                district = service.district,
                idCategory = service.idCategory
            };*/

            string idImagen = saveImagen(url);

            var tempService = new BsonDocument
                {
                    {"name",service.name },
                    {"owner" , service.owner},
                    {"province" , service.province},
                    {"canton" , service.canton},
                    {"district" , service.district},
                    {"town",service.town},
                    {"KmDistance",service.KmDistance},
                    {"latitude",service.latitude},
                    {"longitude",service.longitude},
                    {"idCategory" , service.idCategory},
                    {"imagenID",idImagen}
                };

            dataBase.GetCollection<Service>("Services").Insert(tempService);

            string idSTring = tempService["_id"].ToString();

            for (int i = 0; i < service.fare.Count(); i++)
            {

                var tempFare = new BsonDocument
                    {
                        {"serviceId",idSTring },
                        {"name",service.fare[i].name},
                        {"description",service.fare[i].description}

                    };

                addFare(tempFare);
            }


        }

        public List<Fare> getFares(string idService)
        {

            MongoCollection<Fare> collection = dataBase.GetCollection<Fare>("Fares");
            var query = Query.EQ("serviceId", idService);
            List<Fare> fares = collection.Find(query).ToList();
            return fares;
        }

        public void addFare(BsonDocument fare)
        {
            dataBase.GetCollection<Fare>("Fares").Insert(fare);
        }


        public List<Service> getService(string nombre)
        {

            MongoCollection<Service> collection = dataBase.GetCollection<Service>("Services");
            var query = Query.EQ("name", nombre);
            List<Service> services = collection.Find(query).ToList();

            for (int i = 0; i < services.Count(); i++)
            {
                services[i].fare = getFares(services[i].ServiceId.ToString());
            }

            return services;

        }


        Service getServiceId(ObjectId id)
        {
            var query = Query.EQ("_id", id);
            MongoCollection<Service> collection = dataBase.GetCollection<Service>("Services");

            Service tempService = collection.FindOne(query);
            tempService.fare = getFares(tempService.ServiceId.ToString());

            return tempService;
        }

        public byte[] getImagen(string idImagen)
        {

            var fileID = new ObjectId(idImagen);
            var file = dataBase.GridFS.FindOneById(fileID);
            byte[] temp;

            using (var fs = file.OpenRead())
            {

                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                var stream = new MemoryStream(bytes);
                temp = bytes;
                //imagen = "data:Image/png;base64," + Convert.ToBase64String(bytes);
            }

            return temp;

        }

        public string saveImagen(string rutaImagen)
        {

            var fileName = rutaImagen;
            string idImagen;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var gridFsInfo = dataBase.GridFS.Upload(fs, fileName);
                var fileId = gridFsInfo.Id;
                idImagen = fileId.ToString();
            }

            return idImagen;

        }

    }
}