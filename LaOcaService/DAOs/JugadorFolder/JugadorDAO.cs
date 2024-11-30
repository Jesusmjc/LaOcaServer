using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;

namespace LaOcaService.DAOs.JugadorFolder
{
    public class JugadorDAO : IJugadorDAO
    {
        public JugadorDAO() {}
        
        public void CrearJugador(Jugador jugador, string referenciaImagen)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaExistente = contexto.Cuentas.Find(jugador.IdCuenta);
                if (cuentaExistente == null)
                {
                    throw new KeyNotFoundException($"La cuenta con id {jugador.IdCuenta} no existe.");
                }

                var aspectoExistente = contexto.Aspectos.Find(jugador.IdFotoPerfil);
                if (aspectoExistente == null)
                {
                    var nuevoAspecto = new Aspectos
                    {
                        IdAspecto = jugador.IdFotoPerfil,
                        tipo = "FotoPerfil",
                        referencia = referenciaImagen
                    };

                    contexto.Aspectos.Add(nuevoAspecto);
                    contexto.SaveChanges();
                    aspectoExistente = nuevoAspecto;
                }

                var puntuacionExistente = contexto.Puntuaciones.Find(jugador.IdPuntuacion);
                if (puntuacionExistente == null)
                {
                    var nuevaPuntuacion = new Puntuaciones
                    {
                        casillasRecorridasGlobal = 0,
                        monedasObtenidasGlobal = 0,
                        partidasGanadasGlobal = 0,
                        monedasActuales = 0
                    };
                    contexto.Puntuaciones.Add(nuevaPuntuacion);
                    contexto.SaveChanges();
                    puntuacionExistente = nuevaPuntuacion;
                }

                var jugadorBD = new Jugadores
                {
                    nombreUsuario = jugador.NombreUsuario,
                    IdFotoPerfil = aspectoExistente.IdAspecto,
                    IdCuenta = cuentaExistente.IdCuenta,
                    IdPuntuacion = puntuacionExistente.IdPuntuacion
                };

                contexto.Jugadores.Add(jugadorBD);
                contexto.SaveChanges();
                cuentaExistente.IdJugador = jugadorBD.IdJugador;
                contexto.SaveChanges();
                puntuacionExistente.IdJugador = jugadorBD.IdJugador;
                contexto.SaveChanges();
                Console.WriteLine($"Jugador creado con id: {jugadorBD.IdJugador}, idFotoPerfil: {jugadorBD.IdFotoPerfil}, idCuenta: {jugadorBD.IdCuenta}, idPuntuacion: {jugadorBD.IdPuntuacion}");
            }
        }

        public void ModificarJugador(Jugador jugador)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var jugadorBD = contexto.Jugadores.Find(jugador.IdJugador);
                if (jugadorBD == null)
                {
                    return;
                }

                jugadorBD.nombreUsuario = jugador.NombreUsuario;
                jugadorBD.IdFotoPerfil = jugador.IdFotoPerfil;
                contexto.SaveChanges();
            }
        }

        public Jugador ObtenerJugadorPorId(int idJugador)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var jugadorBD = contexto.Jugadores.Find(idJugador);
                if (jugadorBD == null) return null;

                return new Jugador
                {
                    IdJugador = jugadorBD.IdJugador,
                    NombreUsuario = jugadorBD.nombreUsuario,
                    IdFotoPerfil = (int)jugadorBD.IdFotoPerfil,
                    IdPuntuacion = (int)jugadorBD.IdPuntuacion,
                    IdCuenta = (int)jugadorBD.IdCuenta
                };
            }
        }

        public bool NombreUsuarioExisteCrear(string nombreUsuario)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                return contexto.Jugadores.Any(j => j.nombreUsuario == nombreUsuario);
            }
        }

        public bool NombreUsuarioExisteModificar(string nombreUsuario, int idJugadorActual)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                return contexto.Jugadores.Any(j => j.nombreUsuario == nombreUsuario && j.IdJugador != idJugadorActual);
            }
        }
    }
}