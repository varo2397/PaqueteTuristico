﻿using System;
using System.Collections.Generic;
using System.Linq;
using PaquetesTuristicos.Models;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Web.Security;
using System.Web.Helpers;
using System.IO;

namespace PaquetesTuristicos.Controllers
{
    public class VendedorController : Controller
    {
        Neo4jStore neo = new Neo4jStore();

        public ActionResult InicioSesion()
        {
            return PartialView();
        }

        public ActionResult Actualizar(string id)
        {
            MongoConnect mongo = new MongoConnect();
            mongo.updateById(id);
            return RedirectToAction("Servicios", "Vendedor");
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
                    if ((u != null) && (u.idRolUsuario == 2))   // existe el usuario y es tipo vendedor
                    {
                        if (string.Compare(Crypto.Hash(pass), u.contrasena) == 0)
                        {
                            Usuario user = new Usuario();
                            user.idUsuario = u.idUsuario;
                            user.correo = u.correo;
                            
                            Session["USER"] = user;
                            ViewBag.email = email;

                            return RedirectToAction("Servicios", "Vendedor");
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

        public ActionResult Servicios()
        {
            Usuario u = (Usuario)Session["USER"];
            MongoConnect mongo = new MongoConnect();
            var Model = mongo.getServiceByCreatorId(u.correo);
            return View(Model);
        }

        public ActionResult Registrarse()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Registrarse(FormCollection form)
        {
            Usuario user = new Usuario();
            var email = form["correoElectronico"];
            var pass = form["contraseña"];
            var passConfirm = form["confirmarContraseña"];


            if (string.Compare(pass, passConfirm) == 0)
            {
                user.correo = email;
                user.contrasena = Crypto.Hash(pass);

                user.contrasena = user.contrasena;

                user.idRolUsuario = 2;          // 2 = vendedor

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
                            ViewBag.Error = "Usuario creado correctamente";
                            return RedirectToAction("InicioSesion", "Vendedor");
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

        public ActionResult AgregarServicio()
        {
            serviciosCREntities db = new serviciosCREntities();
            var categorias = db.Categorias.ToList();
            return View(categorias);
        }


        [HttpPost]
        public ActionResult AgregarServicio(FormCollection form,List<HttpPostedFileBase> foto, string cantidad, string categoria)
        {
            Service servicio = new Service();
            
            //Obtener datos de los inputs
            string nombreServicio = form["nombreServicio"];
            string nombrePropietario = form["nombrePropietario"];
            string provincia = form["provincia"];
            string canton = form["canton"];
            string distrito = form["distrito"];
            string pueblo = form["pueblo"];
            string latitud = form["latitud"];
            string longuitud = form["longuitud"];

            //Obtener el usuario que tiene sesion iniciada
            Usuario creador = (Usuario)Session["USER"];

            //Cantidad de tarifas que va a tener el servicio
            int cantidadTarifas = Convert.ToInt32(cantidad);

            //Id de la categoria escogida 
            int idCategoria = Convert.ToInt32(categoria);

            servicio.idCreador = creador.correo;
            servicio.name = nombreServicio;
            servicio.owner = nombrePropietario;
            servicio.province = provincia;
            servicio.canton = canton;
            servicio.district = distrito;
            servicio.town = pueblo;
            servicio.KmDistance = "";
            servicio.latitude = latitud.ToString();
            servicio.longitude = longuitud.ToString();
            servicio.idCategory = idCategoria;
                  
            //direcciones en el servidor de las imagenes 
            List<string> fotos = new List<string>();
            foreach (var file in foto)
            {
                file.SaveAs(Server.MapPath("~/App_Data/Upload/" + file.FileName));
                string direccion = Server.MapPath("~/App_Data/Upload/" + file.FileName);
                fotos.Add(direccion);
            }
            
            //datos de las tarifas 
            List<Fare> tarifas = new List<Fare>();
            for (int i = 0; i < cantidadTarifas; i++)
            {
                Fare tarifa = new Fare();
                string nombreT = "tarifaNombre" + (i + 1);
                string precio = "tarifaPrecio" + (i + 1);
                string descripcion = "tarifaDescripcion" + (i + 1);

                tarifa.name = form[nombreT];
                tarifa.description = form[descripcion];
                tarifa.precio = Convert.ToInt32(form[precio]);
                tarifas.Add(tarifa);
            }

            servicio.fare = tarifas;
            
            
            MongoConnect nombre = new MongoConnect();
            Service servicioID = new Service();

            servicioID = nombre.addServiceReturn(servicio,fotos);
            neo.agregarServicio(servicioID);
            neo.categoria_x_Servicio(idCategoria,servicioID);

            //eliminar fotos del servidor
            System.IO.DirectoryInfo di = new DirectoryInfo(Server.MapPath("~/App_Data/Upload/"));

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }   
            return RedirectToAction("Servicios", "Vendedor");
        }


        public string CantidadTarifas(string cantidad)
        {
            int cant = Int32.Parse(cantidad);

            ArrayList valores = new ArrayList();
            valores.Add("1");
            valores.Add("2");
            valores.Add("3");
            valores.Add("4");

            bool esta = false;

            foreach (string v in valores)
            {
                if (v.Equals(cantidad))
                {
                    esta = true;
                }
            }

            if (esta)
            {
                string htmltarifas = "";
                for (int i = 0; i < cant; i++)
                {
                    string tarifa = "<div class=\"form - group\"> <h2> Tarifa " + (i + 1) + "</h2></div>" +
                        "<div class=\"form-group\"><label>Nombre tarifa " + (i + 1) + "</label></div>" +
                        "<div class=\"form-group\"><input type = \"text\" class=\"form-control\" name=\"tarifaNombre" + (i + 1) + "\" required></div>" +
                        "<div class=\"form-group\"><label>Precio de la tarifa " + (i + 1) + "</label></div>" +
                        "<div class=\"form-group\"><input type = \"number\" class=\"form-control\" min=\"1\" max =\"10000\" name=\"tarifaPrecio" + (i + 1) + "\" required ></div>" +
                        "<div class=\"form-group\"><label>Descripcion de la tarifa " + (i + 1) + "</label></div>" +
                        "<div class=\"form-group\"><textarea class=\"form-control\" name=\"tarifaDescripcion" + (i+1) + "\" ></textarea>";
                    htmltarifas += tarifa;
                }
                return htmltarifas;
            }

            return "error";
        }

        public ActionResult CerrarSesion()
        {
            Session.Clear();
            return RedirectToAction("InicioSesion", "Vendedor");
        }
    }
}