using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace PaquetesTuristicos.Models
{


    public class Neo4jStore
    {
        //Conecta a la base de datos de Neo4j
        private GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data/"), "neo4j", "mirainikki")
        {
            JsonContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public void agregarUsuario(Usuario usuario)
        {
            //Agrega usuarios a la BD
            client.Connect();

            var node = usuario;

            client.Cypher
                .Create("(u:Usuario {nuevoUsuario})")
                .WithParam("nuevoUsuario", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void agregarServicio(Service servicio)
        {
            //Agrega servicios a la BD
            client.Connect();

            var node = servicio.id.ToString();

            client.Cypher
                .Create("(s:Servicio {id:{nuevoServicio}})")
                .WithParams(new { nuevoServicio = node })
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void agregarCategoria(Categoria categoria)
        {
            //Agrega categorías a la BD
            client.Connect();

            var node = categoria;

            client.Cypher
                .Create("(c:Categoria {nuevaCategoria})")
                .WithParam("nuevaCategoria", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void usuario_x_Categoria(Usuario usuario, int categoria)
        {
            //Un usuario da like a una categoría
            client.Connect();

            client.Cypher
                .Match("(u:Usuario)", "(c:Categoria)")
                .Where((Usuario u) => u.correo == usuario.correo)
                .AndWhere((Categoria c) => c.idCategoria == categoria)
                .Create("(u)-[:LIKE]->(c)")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void usuario_x_Servicio(Calificacion calificacion)
        {
            //Un usuario da una calificación a un servicio
            client.Connect();

            Opinion opinion = new Opinion();

            opinion.calificacion = (int)calificacion.calificacion1;
            opinion.comentario = calificacion.comentario;
            opinion.fecha = calificacion.fechaHora.ToString();
            var node = calificacion;

            client.Cypher
                .Match("(u:Usuario)", "(s:Servicio)")
                .Where((Usuario u) => u.correo == calificacion.Usuario.correo)
                .AndWhere((Service s) => s.id.ToString() == calificacion.idServicio)
                .Create("(u)-[:CALIFICACION {calificacion}]->(s)")
                .WithParam("calificacion", opinion)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void categoria_x_Servicio(int categoria, Service servicio)
        {
            //Una categoría se conecta a un servicio
            client.Connect();

            client.Cypher
                .Match("(c:Categoria)", "(s:Servicio)")
                .Where((Categoria c) => c.idCategoria == categoria)
                .AndWhere((Service s) => s.id.ToString() == servicio.id.ToString())
                .Create("(c)-[:TIPO]->(s)")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public List<string> usuario_x_Like(Usuario usuario)
        {
            //Retorna las categorias a las que un usuario le ha dado like
            client.Connect();

            List<Categoria> query = client.Cypher
                .OptionalMatch("(u:Usuario)-[l:LIKE]->(c:Categoria)")
                .Where((Usuario u) => u.correo == usuario.correo)
                .Return(c => c.As<Categoria>())
                .Results
                .ToList();    

            List<string> nombres = new List<string>();

            if (query.Count() != 0 && !(query[0] == null))
            {
                
                foreach (var c in query)
                {
                    nombres.Add(c.categoria1);
                }
            }

            return nombres;
        }

        public List<Opinion> calificaciones(Service servicio)
        {
            //Retorna las calificaciones que los usuarios han hecho sobre un servicio
            client.Connect();

            List<Opinion> query = client.Cypher
                .OptionalMatch("(u:Usuario)-[c:CALIFICACION]->(s:Servicio)")
                .Where((Service s) => s.id.ToString() == servicio.id.ToString())
                .Return(c => c.As<Opinion>())
                .Results
                .ToList();

            return query;
        }

        public void quitarLike(string correo, int categoria)
        {
            //Quita el like de un usuario de una categoría
            client.Connect();
            client.Cypher
                .OptionalMatch("(u:Usuario)-[l]->(c:Categoria)")
                .Where((Categoria c) => c.idCategoria == categoria)
                .AndWhere((Usuario u) => u.correo == correo)
                .Delete("l")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public List<Service> preferencias(Usuario usuario)
        {
            //Retorna los servicios los servicios conectados a las categorias que el usuario ha dado like
            client.Connect();

            List<Nombre> query = client.Cypher
                .Match("(u:Usuario)-[:LIKE]->(c:Categoria)-[:TIPO]->(s:Servicio)")
                .Where((Usuario u) => u.correo == usuario.correo)
                .Return(s => s.As<Nombre>())
                .Results
                .ToList();
            
            List<string> nombres = new List<string>();

            if (query.Count() != 0 && !(query[0] == null))
            {
                foreach (var a in query)
                {
                    nombres.Add(a.id);
                }
            }

            MongoConnect mongo = new MongoConnect();

            return mongo.getServicesById(nombres);
        }

        public void editarCategoria(Categoria categoria)
        {
            //Edita una categoría
            client.Connect();

            client.Cypher
                .Match("(c:Categoria)")
                .Where((Categoria c) => c.idCategoria == categoria.idCategoria)
                .Set("c.categoria1 = {nombre}")
                .WithParam("nombre", categoria.categoria1)
                .ExecuteWithoutResults();
        }
    }
}