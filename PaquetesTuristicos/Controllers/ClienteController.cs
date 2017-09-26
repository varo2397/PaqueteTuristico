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
                    var v = db.Usuarios.Where(a => a.correo.Equals(email)).FirstOrDefault();
                    if ((v != null) && (v.idRolUsuario == 3))   // existe el usuario y es tipo regular
                    {
                        if (string.Compare(Crypto.Hash(pass), v.contrasena) == 0)
                        {
                            var v2 = db.Regulars.Where(a => a.idUsuario.Equals(v.idUsuario)).FirstOrDefault();
                            Session["USER"] = v2;

                            return RedirectToAction("Home", "Index");
                        }else
                        {
                            System.Diagnostics.Debug.WriteLine("contraseña invalida");
                            //contraseña invalida
                        }
                    }else
                    {
                        System.Diagnostics.Debug.WriteLine("Usuario no existe/ cuenta no es un usuario regular");
                    }
                }
            }
            ViewBag.Error = "Error";
            return View(form);
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

            if (pass != passConfirm)
            {
                //contraseñas no son iguales
                System.Diagnostics.Debug.WriteLine("contraseñas no son iguales");
            }

            user.correo = email;
            user.contrasena = Crypto.Hash(pass);
            user.idRolUsuario = 2;          // 2 = vendedor

            regular.primerNombre = name;
            regular.apellidos = lastName;
            regular.cuentaBancaria = Convert.ToDecimal(bankAccount);

            if (ModelState.IsValid)
            {

                if (emailExist(email))
                {
                    //ModelState.AddModelError("EmailExist", "Email already exist");
                    System.Diagnostics.Debug.WriteLine("Email already exist");
                    return View();
                }

                using (serviciosCREntities db = new serviciosCREntities())
                {
                    db.Usuarios.Add(user);
                    var v = db.Usuarios.Where(a => a.correo == user.correo).FirstOrDefault();
                    regular.idUsuario = v.idUsuario;
                    db.Regulars.Add(regular);
                    db.SaveChanges();
                }
            }
            ViewBag.Error = "Error";
            return View(form);
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
    }
}