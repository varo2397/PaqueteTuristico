using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;

namespace PaquetesTuristicos.Controllers
{
    public class VendedorController : Controller
    {
        public ActionResult InicioSesion()
        {
            return PartialView();
        }

        public ActionResult Servicios()
        {
            return View();
        }

        public ActionResult Registrarse()
        {
            return PartialView();
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
                        "<div class=\"form-group\"><textarea></textarea>";
                    htmltarifas += tarifa;
                }
                return htmltarifas;
            }

            return "error";
        }

        //public ActionResult

        //public ActionResult Login()
        //{
        //    ViewBag.Error = "";

        //    return View();

        //}
        //[HttpPost]
        //public ActionResult Login(FormCollection form)
        //{
        //    var USER = form["USER"];
        //    var pass = form["pass"];
        //    // this action is for handle post (login)
        //    if (ModelState.IsValid)
        //    {
        //        using (serviciosCREntities db = new serviciosCREntities())
        //        {
        //            var v = db.Usuarios.Where(a => a.correo.Equals(USER)).FirstOrDefault();
        //            if (v != null)
        //            {
        //                Session["LogedUserID"] = v.idUsuario.ToString();

        //                //"Identificacion,Cuenta_bancaria,Primer_apellido,Segundo_apellido,Nombre,Nacionalidad,Genero,Fecha_de_nacimiento,Nombre_de_Usuario,Contraseña")] 
        //                return RedirectToAction("AfterLogin", "Hikers");
        //            }
        //        }
        //    }
        //    ViewBag.Error = "Error";
        //    return View(form);
        //}
    }
}