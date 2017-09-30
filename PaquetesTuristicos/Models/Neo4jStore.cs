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
        private GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data/"), "neo4j", "mirainikki")
        {
            JsonContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public void agregarUsuario(Usuario usuario)
        {
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
            client.Connect();

            var node = servicio;

            client.Cypher
                .Create("(s:Servicio {nuevoServicio})")
                .WithParam("nuevoServicio", servicio)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void agregarCategoria(Categoria categoria)
        {
            client.Connect();

            var node = categoria;

            client.Cypher
                .Create("(s:Servicio {nuevoServicio})")
                .WithParam("nuevoServicio", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void usuario_x_Categoria(Usuario usuario, Categoria categoria)
        {
            client.Connect();

            client.Cypher
                .Match("(u:Usuario)", "(c:Categoria)")
                .Where((Usuario u) => u.idUsuario == usuario.idUsuario)
                .AndWhere((Categoria c) => c.idCategoria == categoria.idCategoria)
                .Create("(u)-[:LIKE]->(c)")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void usuario_x_Servicio(Calificacion calificacion)
        {
            client.Connect();

            var node = calificacion;

            client.Cypher
                .Match("(u:Usuario)", "(s:Servicio)")
                .Where((Usuario u) => u.idUsuario == calificacion.idUsuario)
                .AndWhere((Service s) => Int32.Parse(s.id.ToString()) == calificacion.idServicio)
                .Create("(u)-[:CALIFICACION {calificacion}]->(s)")
                .WithParam("calificacion", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void categoria_x_Servicio(Categoria categoria, Service servicio)
        {
            client.Connect();

            client.Cypher
                .Match("(c:Categoria)", "(s:Servicio)")
                .Where((Categoria c) => c.idCategoria == categoria.idCategoria)
                .AndWhere((Service s) => s.id == servicio.id)
                .Create("(c)-[:TIPO]->(s)")
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public int likes_x_Categorias(Categoria categoria)
        {
            client.Connect();

            var query = client.Cypher
                .OptionalMatch("(u:Usuario)-[l:LIKE]->(c:Categoria)")
                .Where((Categoria c) => c.idCategoria == categoria.idCategoria)
                .Return(c => new
                {
                    Nodes = c.As<Usuario>()
                })
                .Results;

            return query.Count();
        }

        public List<Calificacion> calificaciones(Service servicio)
        {
            client.Connect();

            List<Calificacion> query = client.Cypher
                .OptionalMatch("(u:Usuario)-[c:CALIFICACION]->(s:Servicio)")
                .Where((Service s) => s.id == servicio.id)
                .Return(c => c.As<Calificacion>())
                .Results
                .ToList();

            return query;

            //Optional Match(u:Usuario)-[o: OPINION]->(s: Servicio)
            //Where s.serviceId = 1
            //Return o
        }

        public List<Service> preferencias(Usuario usuario)
        {
            client.Connect();

            List<Service> query = client.Cypher
                .Match("(u: Usuario) -[o: OPINION]->(s: Servicio)")
                .Where((Usuario u) => u.idUsuario == usuario.idUsuario)
                .OptionalMatch("(u) -[:LIKE]->(c: Categoria) -[:TIPO]->(s)")
                .Return(s => s.As<Service>())
                .OrderByDescending("o.calificacion")
                .Results
                .ToList();

            return query;

            //Match(u: Usuario) -[o: OPINION]->(s: Servicio)
            //Where u.idUsuario = 2
            //Match(u) -[:LIKE]->(c: Categoria) -[:TIPO]->(s)
            //Return s
            //Order by(o.calificacion) Desc
        }
    }
}