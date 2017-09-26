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

namespace PaquetesTuristicos.Controllers
{
    public class VendedorController : Controller
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
                    if ((u != null) && (u.idRolUsuario == 2))   // existe el usuario y es tipo vendedor
                    {
                        if (string.Compare(Crypto.Hash(pass), u.contrasena) == 0)
                        {
                            Usuario user = new Usuario();
                            user.idUsuario = u.idUsuario;
                            user.correo = u.correo;
                            
                            Session["USER"] = user;

                            return RedirectToAction("Vendedor", "Servicios");
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
            var email = form["correoElectronico"];
            var pass = form["contraseña"];
            var passConfirm = form["confirmarContraseña"];


            if (string.Compare(pass, passConfirm) == 0)
            {
                user.correo = email;
                user.contrasena = Crypto.Hash(pass);
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
                            return RedirectToAction("Vendedor", "InicioSesion");
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
            return View();
        }

        //public ActionResult AgregarServicio(FormCollection form)
        //{

        //}


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
                        "<div class=\"form-group\"><input type = \"text\" class=\"form-control\" id\"=tarifaNombre" + (i + 1) + "\" required></div>" +
                        "<div class=\"form-group\"><label>Precio de la tarifa " + (i + 1) + "</label></div>" +
                        "<div class=\"form-group\"><input type = \"number\" class=\"form-control\" min=\"1\" max =\"10000\" id\"=tarifaPrecio" + (i + 1) + "\" required ></div>" +
                        "<div class=\"form-group\"><label>Descripcion de la tarifa " + (i + 1) + "</label></div>" +
                        "<div class=\"form-group\"><textarea class=\"form-control\" ></textarea>";
                    htmltarifas += tarifa;
                }
                return htmltarifas;
            }

            return "error";
        }
    }
}