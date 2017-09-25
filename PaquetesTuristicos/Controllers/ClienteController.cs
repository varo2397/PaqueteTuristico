using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaquetesTuristicos.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult InicioSesion()
        {
            return PartialView();
        }        

        public ActionResult Ordenes()
        {
            return View();
        }

        public ActionResult Registrarse()
        {
            return PartialView();
        }
    }
}