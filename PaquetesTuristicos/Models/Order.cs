using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaquetesTuristicos.Models
{
    public class Order
    {
        public int idOrden { get; set; }
        public int idCliente { get; set; }
        public bool pagada { get; set; }
        public System.DateTime fechaHora { get; set; }
        public List<Tuple<Service, Fare>> orderList { get; set; }
    }
}