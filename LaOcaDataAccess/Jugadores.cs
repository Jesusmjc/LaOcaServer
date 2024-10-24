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
    
    public partial class Jugadores
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Jugadores()
        {
            this.Amistades = new HashSet<Amistades>();
            this.Amistades1 = new HashSet<Amistades>();
            this.JugadoresAspectos = new HashSet<JugadoresAspectos>();
        }
    
        public int IdJugador { get; set; }
        public string nombreUsuario { get; set; }
        public string nombre { get; set; }
        public string apellidoPaterno { get; set; }
        public string apellidoMaterno { get; set; }
        public Nullable<int> IdPuntuacion { get; set; }
        public Nullable<int> IdFotoPerfil { get; set; }
        public Nullable<int> IdCuenta { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Amistades> Amistades { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Amistades> Amistades1 { get; set; }
        public virtual Aspectos Aspectos { get; set; }
        public virtual Cuentas Cuentas { get; set; }
        public virtual Puntuaciones Puntuaciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JugadoresAspectos> JugadoresAspectos { get; set; }
    }
}
