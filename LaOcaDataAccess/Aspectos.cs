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
    
    public partial class Aspectos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Aspectos()
        {
            this.Jugadores = new HashSet<Jugadores>();
            this.JugadoresAspectos = new HashSet<JugadoresAspectos>();
        }
    
        public int IdAspecto { get; set; }
        public string tipo { get; set; }
        public string referencia { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Jugadores> Jugadores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JugadoresAspectos> JugadoresAspectos { get; set; }
    }
}
