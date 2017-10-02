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
using System.IO;

namespace PaquetesTuristicos.Controllers
{
    public class AdministradorController : Controller
    {
        private serviciosCREntities db = new serviciosCREntities();
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
                    if ((u != null) && (u.idRolUsuario == 1))   // existe el usuario y es tipo regular
                    {
                        if (string.Compare(Crypto.Hash(pass), u.contrasena) == 0)
                        {
                            Usuario user = new Usuario();
                            user.idUsuario = u.idUsuario;
                            user.correo = u.correo;
                            Session["USER"] = user;

                            return RedirectToAction("Categorias", "Administrador");
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

                user.idRolUsuario = 1;          // 2 = vendedor

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
                            return RedirectToAction("InicioSesion", "Administrador");
                        }
                    }
                }
            }
            else
            {
                ViewBag.Error = "Contraseñas no coiciden";
            }
            return PartialView();
        }

        public ActionResult Categorias()
        {
            return View(db.Categorias.ToList());
        }

        public ActionResult CrearCategoria()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CrearCategoria([Bind(Include = "idCategoria,categoria1,activo")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                db.Categorias.Add(categoria);
                db.SaveChanges();
                neo.agregarCategoria(categoria);
                return RedirectToAction("Categorias","Administrador");
            }

            return View(categoria);
        }

        public ActionResult EditarCategoria(int? id)
        {
            Categoria categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }
            return View(categoria);
        }

        [HttpPost]
        public ActionResult EditarCategoria([Bind(Include = "idCategoria,categoria1,activo")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Categorias","Administrador");
            }
            return View(categoria);
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
            return RedirectToAction("InicioSesion", "Administrador");
        }
    }

}