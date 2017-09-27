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

        public ActionResult Ordenes()
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