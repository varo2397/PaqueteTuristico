using System;
using System.Collections.Generic;
using System.Linq;
using PaquetesTuristicos.Models;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Web.Security;
using System.Web.Helpers;
using System.Globalization;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace PaquetesTuristicos.Controllers
{
    public class ClienteController : Controller
    {
        Neo4jStore neo = new Neo4jStore();
        public ActionResult InicioSesion()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult InicioSesion(FormCollection form)
        {
            var email = form["correoElectronico"];
            var pass = form["contraseña"];
            if (ModelState.IsValid)
            {
                using (serviciosCREntities db = new serviciosCREntities())
                {
                    var u = db.Usuarios.Where(a => a.correo.Equals(email)).FirstOrDefault();
                    if ((u != null) && (u.idRolUsuario == 3))   // existe el usuario y es tipo regular
                    {
                        if (string.Compare(Crypto.Hash(pass), u.contrasena) == 0)
                        {
                            Usuario user = new Usuario();
                            user.idUsuario = u.idUsuario;
                            user.correo = u.correo;
                            Session["USER"] = user;

                            return RedirectToAction("Ordenes", "Cliente");
                        }
                        else
                        {
                            ViewBag.Error = "Contraseña incorrecta";
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Este correo no esta registrado en nuestro sistema.";
                    }
                }
            }
            return PartialView(form);
        }

        public ActionResult BuscarServicios(FormCollection form)
        {
            string buscar = form["buscar"];
            MongoConnect mongo = new MongoConnect();
            var model = mongo.getServiceByName(buscar);
            return View(model);
        }

        public ActionResult Ordenes()
        {
            Usuario user = (Usuario)Session["USER"];
            var listaSqlOrden = new List<Orden>();
            var listaOrden = new List<Order>();
            if (ModelState.IsValid)
            {
                using (serviciosCREntities db = new serviciosCREntities())
                {
                    listaSqlOrden = db.Ordens.Where(a => a.idCliente.Equals(user.idUsuario)).ToList();

                    if (listaSqlOrden != null)
                    {
                        // Llenar lista de Order
                        foreach (Orden o in listaSqlOrden)
                        {
                            Order order = new Order();
                            order.fechaHora = o.fechaHora;
                            order.idCliente = o.idCliente;
                            order.idOrden = o.idOrden;
                            listaOrden.Add(order);
                        }

                        int idOrden;
                        List<ServiciosPorOrden> sopList;
                        int i, j;
                        for (i = 0; i < listaOrden.Count(); ++i)
                        {
                            idOrden = listaOrden[i].idOrden;
                            sopList = db.ServiciosPorOrdens.Where(a => a.idOrden.Equals(idOrden)).ToList();
                            for (j = 0; j < sopList.Count(); ++j)
                            {
                                MongoConnect mongo = new MongoConnect();
                                Service service = mongo.getid((sopList[j].idServicio).ToString());

                                Fare fare = new Fare();
                                foreach (var f in service.fare)
                                {
                                    if ((sopList[j].idTarifa.ToString()).Equals(f.id.ToString()))
                                    {
                                        fare = f;
                                        fare.serviceId = sopList[j].idServicio;
                                        fare.qty = sopList[j].cantidad;
                                    }

                                }
                                Tuple<Service, Fare> item = new Tuple<Service, Fare>(service, fare);
                                listaOrden[i].orderList.Add(item);
                            }

                        }
                    }
                    else
                    {
                        listaOrden = null;
                        ViewBag.Error = "Usted no tiene ninguna orden registrada";
                    }
                }
            }
            //Si no hay ordenes retorna null
            return View(listaOrden);
        }


        public ActionResult CalificarServicio(string id)
        {
            Session["IDServicio"] = id;
            return View();
        }

        [HttpPost]
        public ActionResult CalificarServicio(FormCollection form)
        {
            Usuario user = (Usuario)Session["USER"];
            Calificacion calificacion = new Calificacion();


            calificacion.comentario = form["comentario"];
            calificacion.calificacion1 = Convert.ToDecimal(form["calificacion"], CultureInfo.InvariantCulture);
            calificacion.fechaHora = DateTime.Now;
            calificacion.idServicio = (string)Session["IDServicio"];
            calificacion.idUsuario = user.idUsuario;
            calificacion.Usuario = user;
            user.contrasena = "";
            

            neo.usuario_x_Servicio(calificacion);

            calificacion.Usuario = null;
            if (ModelState.IsValid)
            {
                using (serviciosCREntities db = new serviciosCREntities())
                {
                    db.Calificacions.Add(calificacion);
                    db.SaveChanges();
                    ViewBag.Error = "Servicio calificado!";
                    Session["IDServicio"] = null;
                    return RedirectToAction("Ordenes", "Cliente");

                }
            }

            return View(form);

        }

        public ActionResult Carrito()
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.loadCartItems();
            return View(cart.ShoppingCart);
        }
        public ActionResult AgregarItemAlCarrito(string tipo)
        {
            Service ser = (Service)Session["Servicio"];
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.addToCart(ser.id.ToString(), tipo);
            ViewBag.Error = "Servicio agregado";
            return RedirectToAction("Carrito", "Cliente");
        }

        public ActionResult BorrarItemDelCarrito(string id, string id1)
        {
            Service ser = (Service)Session["Servicio"];
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.remove(id, id1);
            ViewBag.Error = "Servicio eliminado";
            return RedirectToAction("Carrito", "Cliente");
        }

        //Crea una orden con todos los items del carrito si existe almenos un item
        public ActionResult Pagar()
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            Orden orden = new Orden();

            cart.loadCartItems();
            if (cart.ShoppingCart.Count() > 0)
            {
                orden.pagada = true;
                orden.idCliente = user.idUsuario;
                orden.fechaHora = DateTime.Now;

                if (ModelState.IsValid)
                {

                    using (serviciosCREntities db = new serviciosCREntities())
                    {
                        db.Ordens.Add(orden);
                        db.SaveChanges();

                        foreach (Tuple<Service, Fare> e in cart.ShoppingCart)
                        {
                            ServiciosPorOrden spo = new ServiciosPorOrden();
                            spo.cantidad = e.Item2.qty;
                            spo.idOrden = orden.idOrden;
                            spo.idServicio = e.Item1.id.ToString();
                            spo.idTarifa = e.Item2.id.ToString();
                            orden.ServiciosPorOrdens.Add(spo);
                        }

                        foreach (ServiciosPorOrden spo in orden.ServiciosPorOrdens)
                        {
                            db.ServiciosPorOrdens.Add(spo);
                        }
                        db.SaveChanges();
                        cart.clearCart();
                        ViewBag.Error = "Orden creada correctamente";
                        return RedirectToAction("Ordenes", "Cliente");
                    }
                }
            }
            else
            {
                ViewBag.Error = "No hay servicios en el carrito";
            }
            return View();
        }

        public ActionResult Registrarse()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Registrarse(FormCollection form)
        {
            Usuario user = new Usuario();
            Regular regular = new Regular();

            var name = form["nombre"];
            var lastName = form["apellidos"];
            var bankAccount = form["numeroCuenta"];

            var email = form["correoElectronico"];
            var pass = form["contraseña"];
            var passConfirm = form["confirmarContraseña"];

            if (string.Compare(pass, passConfirm) == 0)
            {
                user.correo = email;
                user.contrasena = Crypto.Hash(pass);

                user.contrasena = user.contrasena;

                user.idRolUsuario = 3;          // 3 = regular

                regular.primerNombre = name;
                regular.apellidos = lastName;
                regular.cuentaBancaria = Convert.ToDecimal(bankAccount);
                neo.agregarUsuario(user);

                if (ModelState.IsValid)
                {
                    if (emailExist(email))
                    {
                        ViewBag.Error = "El correo ya esta registrado en el sistema.";
                    }
                    else
                    {
                        using (serviciosCREntities db = new serviciosCREntities())
                        {
                            db.Usuarios.Add(user);
                            db.SaveChanges();
                            var v = db.Usuarios.Where(a => a.correo == user.correo).FirstOrDefault();
                            regular.idUsuario = v.idUsuario;
                            db.Regulars.Add(regular);
                            db.SaveChanges();
                            ViewBag.Error = "Usuario creado correctamente";
                            
                            return RedirectToAction("InicioSesion", "Cliente");
                        }
                        
                    }
                }
            }
            else
            {
                ViewBag.Error = "Contraseñas no coiciden";
            }
            return PartialView(form);
        }

        public ActionResult Servicio(string id)
        {
            MongoConnect mongo = new MongoConnect();
            Service model = mongo.getid(id); //se obtiene el servicio que se selecciono
            Session["Servicio"] = model; 

            List<Opinion> califaciones = neo.calificaciones(model); //se obtienen los comentarios y calificaciones asociadas a un servicio

            Tuple<Service, List<Opinion>> servicioCalificacion= new Tuple<Service, List<Opinion>>(model, califaciones); //tupla que se va a mandar a la vista con toda la informacion

            List<Tuple<Service, List<Opinion>>> informacion = new List<Tuple<Service, List<Opinion>>>(); //se mete la tupla en una lista para que se puede iterar sobre la tupla en la lista
            informacion.Add(servicioCalificacion);

            return View(informacion);
        }

        [NonAction]
        public bool emailExist(string email)
        {
            using (serviciosCREntities db = new serviciosCREntities())
            {
                var v = db.Usuarios.Where(a => a.correo == email).FirstOrDefault();
                return v != null;
            }
        }

        public string EscogerTarifa(string tipo)
        {
            string html = "";
            Service ser = (Service)Session["Servicio"];
            int indice = 0;
            for (int i = 0; i < ser.fare.Count(); i++)
            {
                if (ser.fare[i].name.Equals(tipo))
                {
                    indice = i;
                }
            }
            html += "<h4>Descripcion tarifa nombre</h4>" +
                    "<p id = descripcion >" + ser.fare[indice].description + "</p> <br>" +
                    "<h4> Precio de la tarifa </h4> <p>" + ser.fare[indice].precio + "</ p > ";
            return html;
        }

        public ActionResult Categorias()
        {
            serviciosCREntities db = new serviciosCREntities();

            List<Categoria> categorias = db.Categorias.ToList(); //consulta de sql server para traer todas las categorias
            Usuario user = (Usuario)Session["USER"]; //usuario que tiene sesion iniciada
            List<string> likes = neo.usuario_x_Like(user); //lista de categorias a las cual el usuario les ha dado like

            Tuple<List<Categoria>, List<string>> categoriasLikes = new Tuple<List<Categoria>, List<string>>(categorias, likes); //tupla para mandar toda la informacion

            List<Tuple<List<Categoria>, List<string>>> lista = new List<Tuple<List<Categoria>, List<string>>>();
            lista.Add(categoriasLikes);
            return View(lista);
        }

        public ActionResult Like(int id)
        {
            Usuario user = (Usuario)Session["USER"];
            neo.usuario_x_Categoria(user, id);

            return RedirectToAction("Categorias", "Cliente");
        }

        public ActionResult QuitarLike(int id)
        {
            Usuario user = (Usuario)Session["USER"];
            neo.quitarLike(user.correo, id);

            return RedirectToAction("Categorias", "Cliente");
        }

        public ActionResult Recomendaciones()
        {
            Usuario user = (Usuario)Session["USER"];
            List<Service> servicios = neo.preferencias(user);
            return View(servicios);
        }

        public ActionResult CerrarSesion()
        {
            Session.Clear();
            return RedirectToAction("InicioSesion", "Cliente");
        }
    }
}