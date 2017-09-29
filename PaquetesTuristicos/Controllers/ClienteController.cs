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

namespace PaquetesTuristicos.Controllers
{
    public class ClienteController : Controller
    {
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
                        if (string.Compare(Crypto.Hash(pass).Substring(0, 50), u.contrasena) == 0)
                        {
                            Usuario user = new Usuario();
                            user.idUsuario = u.idUsuario;
                            user.correo = u.correo;

                            //Conseguir informacion del usuario?
                            //var u2 = db.Regulars.Where(a => a.idUsuario.Equals(u.idUsuario)).FirstOrDefault();
                            Session["USER"] = user;

                            return RedirectToAction("Ordenes", "Cliente");
                        }else
                        {
                            ViewBag.Error = "Contraseña incorrecta";
                        }
                    }else
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
            var model = mongo.getService(buscar);
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
                    //var o = db.Ordens.Where(a => a.idCliente.Equals(user.idUsuario)).FirstOrDefault();

                    // Saca la lista de ordenes
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
                            idOrden = listaOrden[0].idOrden;
                            sopList = db.ServiciosPorOrdens.Where(a => a.idOrden.Equals(idOrden)).ToList();
                            for (j = 0; j < sopList.Count(); ++j)
                            {
                                MongoConnect mongo = new MongoConnect();
                                Service service = mongo.getid((sopList[j].idServicio).ToString());
                                Tuple<Service, int> item = new Tuple<Service, int>(service, sopList[j].cantidad);
                                listaOrden[i].orderList.Add(item);
                            }
                            
                        }
                    } else
                    {
                        listaOrden = null;
                        ViewBag.Error = "Usted no tiene ninguna orden registrada";
                    }
                }
            }
            //Si no hay ordenes retorna null
            return View(listaOrden);
        }

        
        public ActionResult Calificar(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Calificar(int id, FormCollection form)
        {
            Usuario user = (Usuario)Session["USER"];
            Calificacion calificacion = new Calificacion();
            

            calificacion.comentario = form["comentario"];
            calificacion.calificacion1 = Convert.ToDecimal(form["calificacion"], CultureInfo.InvariantCulture);
            calificacion.fechaHora = DateTime.Now;
            calificacion.idServicio = id;
            calificacion.idUsuario = user.idUsuario;

            if (ModelState.IsValid)
            {
                using (serviciosCREntities db = new serviciosCREntities())
                {
                    db.Calificacions.Add(calificacion);
                    db.SaveChanges();
                    ViewBag.Error = "Servicio calificado!";
                    return RedirectToAction("Ordenes", "Cliente");
                }
            }
            return PartialView(form);
        }
        
        public ActionResult Carrito()
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.loadCartItems();
            return View(cart.ShoppingCart);
        }
        public ActionResult AgregarItemAlCarrito(int id, int qty)
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.addToCart(id, qty);
            ViewBag.Error = "Servicio agregado";
            return RedirectToAction("Carrito", "Cliente");
        }

        public ActionResult BorrarItemDelCarrito(int id)
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            cart.remove(id);
            ViewBag.Error = "Servicio eliminado";
            return RedirectToAction("Carrito", "Cliente");
        }

        //Crea una orden con todos los items del carrito si existe almenos un item
        public ActionResult Pagar()
        {
            Usuario user = (Usuario)Session["USER"];
            Cart cart = new Cart(user.idUsuario);
            Orden orden = new Orden();
            
            if (cart.getCartSize() > 0)
            {
                cart.loadCartItems();

                orden.pagada = true;
                orden.idCliente = user.idUsuario;
                orden.fechaHora = DateTime.Now;

                if (ModelState.IsValid)
                {
                    
                    using (serviciosCREntities db = new serviciosCREntities())
                    {
                        db.Ordens.Add(orden);
                        db.SaveChanges();
                        var o = db.Ordens.Where(a => a.fechaHora == orden.fechaHora).FirstOrDefault();
                        orden.idOrden = o.idOrden;

                        foreach(Tuple<Service, int> e in cart.ShoppingCart)
                        {
                            ServiciosPorOrden spo = new ServiciosPorOrden();
                            spo.cantidad = e.Item2;
                            spo.idOrden = orden.idOrden;
                            spo.idServicio = Convert.ToInt32(e.Item1.ServiceId);
                            orden.ServiciosPorOrdens.Add(spo);
                        }

                        foreach(ServiciosPorOrden spo in orden.ServiciosPorOrdens)
                        {
                            db.ServiciosPorOrdens.Add(spo);
                        }
                        db.SaveChanges();
                        cart.clearCart();
                        ViewBag.Error = "Orden creada correctamente";
                        return RedirectToAction("Ordenes", "Cliente");
                    }
                }
            }else
            {
                ViewBag.Error = "No hay servicios en el carrito";
            }
            return PartialView();
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

                user.contrasena = user.contrasena.Substring(0, 50);

                user.idRolUsuario = 3;          // 3 = regular

                regular.primerNombre = name;
                regular.apellidos = lastName;
                regular.cuentaBancaria = Convert.ToDecimal(bankAccount);

                if (ModelState.IsValid)
                {
                    if (emailExist(email))
                    {
                        ViewBag.Error = "El correo ya esta registrado en el sistema.";
                    }else
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

        [NonAction]
        public bool emailExist(string email)
        {
            using (serviciosCREntities db = new serviciosCREntities())
            {
                var v = db.Usuarios.Where(a => a.correo == email).FirstOrDefault();
                return v != null;
            }
        }

        public ActionResult CerrarSesion()
        {
            Session.Clear();
            return RedirectToAction("InicioSesion", "Cliente");
        }
    }
}