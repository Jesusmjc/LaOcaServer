using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LaOcaDataAccess;

namespace LaOcaService.DAOs.JugadorFolder
{
    internal class JugadorDAO : IJugadorDAO
    {
        public JugadorDAO() {}
        public void CrearJugador(Jugador jugador, string referenciaImagen)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var cuentaExistente = contexto.Cuentas.Find(jugador.idCuenta);
                if (cuentaExistente == null)
                {
                    throw new Exception($"La cuenta con id {jugador.idCuenta} no existe.");
                }

                var aspectoExistente = contexto.Aspectos.Find(jugador.idFotoPerfil);
                if (aspectoExistente == null)
                {
                    var nuevoAspecto = new Aspectos
                    {
                        idAspecto = jugador.idFotoPerfil,
                        tipo = "Foto de perfil",
                        referencia = referenciaImagen
                    };
                    

                    contexto.Aspectos.Add(nuevoAspecto);
                    contexto.SaveChanges();
                    aspectoExistente = nuevoAspecto;
                }

                var puntuacionExistente = contexto.Puntuaciones.Find(jugador.idPuntuacion);
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
                    nombreUsuario = jugador.nombreUsuario,
                    idFotoPerfil = aspectoExistente.idAspecto,
                    idCuenta = cuentaExistente.idCuenta,
                    idPuntuacion = puntuacionExistente.IdPuntuacion
                };

                contexto.Jugadores.Add(jugadorBD);
                contexto.SaveChanges();
                cuentaExistente.idJugador = jugadorBD.idJugador;
                contexto.SaveChanges();
                puntuacionExistente.IdJugador = jugadorBD.idJugador;
                contexto.SaveChanges();
                Console.WriteLine($"Jugador creado con id: {jugadorBD.idJugador}, idFotoPerfil: {jugadorBD.idFotoPerfil}, idCuenta: {jugadorBD.idCuenta}, idPuntuacion: {jugadorBD.idPuntuacion}");
            }
        }

        public void ModificarJugador(Jugador jugador)
        {
            using (var contexto = new LaOcaBDEntities())
            {
                var jugadorBD = contexto.Jugadores.Find(jugador.idJugador);
                if (jugadorBD == null)
                {
                    return;
                }

                jugadorBD.nombreUsuario = jugador.nombreUsuario;
                jugadorBD.idFotoPerfil = jugador.idFotoPerfil;
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
                    idJugador = jugadorBD.idJugador,
                    nombreUsuario = jugadorBD.nombreUsuario,
                    idFotoPerfil = (int)jugadorBD.idFotoPerfil,
                    idPuntuacion = (int)jugadorBD.idPuntuacion,
                    idCuenta = (int)jugadorBD.idCuenta
                };
            }
        }
    }
}