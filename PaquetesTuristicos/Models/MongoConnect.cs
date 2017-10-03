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


        public List<Service> getALLServices()
        {
            List<Service> services;
            try
            {
                services = dataBase.GetCollection<Service>("Services").FindAll().ToList();

                for (int i = 0; i < services.Count(); i++)
                {
                    services[i].fare = getFares(services[i].id.ToString());
                    services[i].ImagList = getImages(services[i].id.ToString());
                    for (int j = 0; j < services[i].ImagList.Count(); j++)
                    {
                        services[i].ImagList[j].Image = getImagen(services[i].ImagList[j].imageGridFS);
                    }

                }

            }
            catch (Exception e)
            {
                throw new Exception("Can not access to db server.", e);
            }
            return services;
        }


        //public void addService(Service service, List<string> rutaImags)
        //{

        //    var tempService = new BsonDocument
        //        {
        //            {"name",service.name },
        //            {"owner" , service.owner},
        //            {"idCreador" , service.idCreador},
        //            {"province" , service.province},
        //            {"canton" , service.canton},
        //            {"district" , service.district},
        //            {"disponible",service.disponible },
        //            {"town",service.town},
        //            {"KmDistance",service.KmDistance},
        //            {"latitude",service.latitude},
        //            {"longitude",service.longitude},
        //            {"idCategory" , service.idCategory}
        //        };

        //    dataBase.GetCollection<Service>("Services").Insert(tempService);

        //    string idSTring = tempService["_id"].ToString();

        //    for (int i = 0; i < rutaImags.Count(); i++)
        //    {

        //        string idImagen = saveImagen(rutaImags[i]);

        //        var tempImage = new BsonDocument
        //        {
        //            { "imageGridFS",idImagen},
        //            { "id",idSTring }

        //        };
        //        addImage(tempImage);

        //    }

        //    for (int i = 0; i < service.fare.Count(); i++)
        //    {

        //        var tempFare = new BsonDocument
        //            {
        //                {"id",idSTring },
        //                {"name",service.fare[i].name},
        //                {"description",service.fare[i].description},
        //                {"precio",service.fare[i].precio}

        //            };

        //        addFare(tempFare);
        //    }


        //}

        public Service addServiceReturn(Service service, List<string> rutaImags)
        {

            var tempService = new BsonDocument
                {
                    {"name",service.name },
                    {"owner" , service.owner},
                    {"idCreador" , service.idCreador},
                    {"province" , service.province},
                    {"canton" , service.canton},
                    {"district" , service.district},
                    {"disponible",service.disponible },
                    {"town",service.town},
                    {"KmDistance",service.KmDistance},
                    {"latitude",service.latitude},
                    {"longitude",service.longitude},
                    {"idCategory" , service.idCategory}
                };

            dataBase.GetCollection<Service>("Services").Insert(tempService);

            string idSTring = tempService["_id"].ToString();

            for (int i = 0; i < rutaImags.Count(); i++)
            {

                string idImagen = saveImagen(rutaImags[i]);

                var tempImage = new BsonDocument
                {
                    { "imageGridFS",idImagen},
                    { "id",idSTring }

                };
                addImage(tempImage);

            }

            for (int i = 0; i < service.fare.Count(); i++)
            {

                var tempFare = new BsonDocument
                    {
                        {"id",idSTring },
                        {"name",service.fare[i].name},
                        {"description",service.fare[i].description},
                        {"precio",service.fare[i].precio}

                    };

                addFare(tempFare);
            }

            return getid(idSTring);

        }

        public List<Fare> getFares(string idService)
        {

            MongoCollection<Fare> collection = dataBase.GetCollection<Fare>("Fares");
            var query = Query.EQ("id", idService);
            List<Fare> fares = collection.Find(query).ToList();
            return fares;
        }

        public void addFare(BsonDocument fare)
        {
            dataBase.GetCollection<Fare>("Fares").Insert(fare);
        }

        public void addImage(BsonDocument imag)
        {
            dataBase.GetCollection<Fare>("Imagenes").Insert(imag);
        }

        public List<Imagen> getImages(string idServicio)
        {
            MongoCollection<Imagen> collection = dataBase.GetCollection<Imagen>("Imagenes");
            var query = Query.EQ("id", idServicio);
            List<Imagen> images = collection.Find(query).ToList();
            return images;
        }

        public List<Service> getServiceByCreatorId(string idCreador)
        {

            MongoCollection<Service> collection = dataBase.GetCollection<Service>("Services");
            var query = Query.EQ("idCreador", idCreador);

            List<Service> services = collection.Find(query).ToList();
            if (services.Count() > 0)
            {
                for (int i = 0; i < services.Count(); i++)
                {
                    services[i].fare = getFares(services[i].id.ToString());
                    services[i].ImagList = getImages(services[i].id.ToString());

                    for (int j = 0; j < services[i].ImagList.Count(); j++)
                    {
                        services[i].ImagList[j].Image = getImagen(services[i].ImagList[j].imageGridFS);
                    }
                    using (serviciosCREntities db = new serviciosCREntities())
                    {
                        int id = services[i].idCategory;
                        var v = db.Categorias.Where(a => a.idCategoria == id).FirstOrDefault();
                        services[i].categoria = v;
                    }
                }
            }


            return services;


        }

        public List<Service> getServiceByName(string nombre)
        {

            MongoCollection<Service> collection = dataBase.GetCollection<Service>("Services");

            var query = Query.Matches("name", ".*" + nombre + ".*");

            List<Service> services = collection.Find(query).ToList();

            for (int i = 0; i < services.Count(); i++)
            {
                services[i].fare = getFares(services[i].id.ToString());
                services[i].ImagList = getImages(services[i].id.ToString());

                for (int j = 0; j < services[i].ImagList.Count(); j++)
                {
                    services[i].ImagList[j].Image = getImagen(services[i].ImagList[j].imageGridFS);
                }
                using (serviciosCREntities db = new serviciosCREntities())
                {
                    int id = services[i].idCategory;
                    var v = db.Categorias.Where(a => a.idCategoria == id).FirstOrDefault();
                    services[i].categoria = v;
                }

            }



            return services;

        }


        public Service getid(string id)
        {

            var query = Query.EQ("_id", new ObjectId(id));
            MongoCollection<Service> collection = dataBase.GetCollection<Service>("Services");

            Service tempService = collection.FindOne(query);
            tempService.fare = getFares(tempService.id.ToString());

            tempService.ImagList = getImages(tempService.id.ToString());

            for (int i = 0; i < tempService.ImagList.Count(); i++)
            {
                tempService.ImagList[i].Image = getImagen(tempService.ImagList[i].imageGridFS);
            }

            using (serviciosCREntities db = new serviciosCREntities())
            {
                int idC = tempService.idCategory;
                var v = db.Categorias.Where(a => a.idCategoria == idC).FirstOrDefault();
                tempService.categoria = v;
            }

            return tempService;
        }

        public string getImagen(string idImagen)
        {

            var fileID = new ObjectId(idImagen);
            var file = dataBase.GridFS.FindOneById(fileID);
            string temp;

            using (var fs = file.OpenRead())
            {

                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                var stream = new MemoryStream(bytes);
                temp = "data:Image/png;base64," + Convert.ToBase64String(bytes); ;
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

        public void updateById(string id)
        {

            string estado;

            var coleccion = dataBase.GetCollection<Service>("Services");
            var query = Query.EQ("_id", new ObjectId(id));

            Service tempService = coleccion.FindOne(query);

            if (tempService.disponible == false) estado = "true"; else estado = "false";

            var update = Update.Set("disponible", estado);
            coleccion.Update(query, update);


        }

        public List<Service> getServicesById(List<String> ids)
        {

            List<Service> tempList = new List<Service>();

            for (int i = 0; i < ids.Count(); i++)
            {
                tempList.Add(getid(ids[i]));
            }

            return tempList;

        }

    }
}