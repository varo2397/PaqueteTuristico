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
            //falta conectar
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
            //falta conectar
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
            client.Connect();
            serviciosCREntities db = new serviciosCREntities();
            Usuario usuario = db.Usuarios.Where(a => a.idUsuario == calificacion.idUsuario).FirstOrDefault();

            var node = calificacion;

            client.Cypher
                .Match("(u:Usuario)", "(s:Servicio)")
                .Where((Usuario u) => u.correo == usuario.correo)
                .AndWhere((Service s) => s.id.ToString() == calificacion.idServicio)
                .Create("(u)-[:CALIFICACION {calificacion}]->(s)")
                .WithParam("calificacion", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void categoria_x_Servicio(Categoria categoria, Service servicio)
        {
            //falta conectar
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
            //falta conectar
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

        public List<string> usuario_x_Like(Usuario usuario)
        {
            //falta conectar
            client.Connect();

            List<Categoria> query = client.Cypher
                .OptionalMatch("(u:Usuario)-[l:LIKE]->(c:Categoria)")
                .Where((Usuario u) => u.correo == usuario.correo)
                .Return(c => c.As<Categoria>())
                .Results
                .ToList();

            List<string> nombres = new List<string>();

            if (nombres.Count() != 0)
            {
                foreach (var c in query)
                {
                    nombres.Add(c.categoria1);
                }
            }

            return nombres;
        }

        public List<Calificacion> calificaciones(Service servicio)
        {
            //falta conectar
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
            //falta conectar
            client.Connect();

            List<Service> query = client.Cypher
                .Match("(u: Usuario) -[o: OPINION]->(s: Servicio)")
                .Where((Usuario u) => u.correo == usuario.correo)
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