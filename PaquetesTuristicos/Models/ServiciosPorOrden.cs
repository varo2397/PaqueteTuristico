//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PaquetesTuristicos.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ServiciosPorOrden
    {
        public int id { get; set; }
        public int idOrden { get; set; }
        public int idServicio { get; set; }
        public int cantidad { get; set; }
    
        public virtual Orden Orden { get; set; }
    }
}
