//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LaOcaDataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class Amistades
    {
        public int IdAmistad { get; set; }
        public string estado { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public Nullable<int> IdJugadorSolicitante { get; set; }
        public Nullable<int> IdJugadorReceptor { get; set; }
    
        public virtual Jugadores Jugadores { get; set; }
        public virtual Jugadores Jugadores1 { get; set; }
    }
}
