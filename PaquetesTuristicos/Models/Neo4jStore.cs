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

        public void AgregarUsuario(Usuario usuario)
        {
            client.Connect();

            var node = usuario;

            client.Cypher
                .Create("(u:Usuario {nuevoUsuario})")
                .WithParam("nuevoUsuario", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void AgregarServicio(Service servicio)
        {
            client.Connect();

            var node = servicio;

            client.Cypher
                .Create("(s:Servicio {nuevoServicio})")
                .WithParam("nuevoServicio", servicio)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void AgregarCategoria(Categoria categoria)
        {
            client.Connect();

            var node = categoria;

            client.Cypher
                .Create("(s:Servicio {nuevoServicio})")
                .WithParam("nuevoServicio", node)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void Usuario_x_Categoria(Usuario usuario, Categoria categoria)
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

        public void Usuario_x_Servicio(Usuario usuario, Service servicio, int calif, string coment)
        {
            client.Connect();

            var actor = new Opinion
            {
                calificacion = calif,
                comentario = coment
            };

            client.Cypher
                .Match("(u:Usuario)", "(s:Servicio)")
                .Where((Usuario u) => u.idUsuario == usuario.idUsuario)
                .AndWhere((Service s) => s.id == servicio.id)
                .Create("(u)-[:OPINION {opinion}]->(s)")
                .WithParam("opinion", actor)
                .ExecuteWithoutResultsAsync()
                .Wait();
        }

        public void Categoria_x_Servicio(Categoria categoria, Service servicio)
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

        public int Likes_x_Categorias(Categoria categoria)
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

            int cantidadLikes = query.Count();

            return cantidadLikes;
        }

        public void Opiniones(int servicio)
        {
        }
    }
}