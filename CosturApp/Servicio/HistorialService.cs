using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using CosturApp.Modelo;

namespace CosturApp.Servicio
{
    public class HistorialService
    {
        private string _rutaDB = "anexos.db";
        private string _cadenaConexion => $"Data Source={_rutaDB};Version=3;";
        public static event Action<Historial> HistorialAgregado;

        public HistorialService()
        {
            if (!File.Exists(_rutaDB))
                SQLiteConnection.CreateFile(_rutaDB);

            CrearTablaHistorialSiNoExiste();
        }

        // Crear tabla de historial si no existe
        private void CrearTablaHistorialSiNoExiste()
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"CREATE TABLE IF NOT EXISTS Historiales (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Titulo TEXT NOT NULL,
                                    Descripcion TEXT NOT NULL,
                                    FechaHistorial TEXT NOT NULL
                                )";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Agregar una entrada al historial
        public void AgregarHistorial(Historial historial)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"INSERT INTO Historiales (Titulo, Descripcion, FechaHistorial) 
                                 VALUES (@titulo, @descripcion, @fecha)";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@titulo", historial.Titulo);
                    cmd.Parameters.AddWithValue("@descripcion", historial.Descripcion);
                    cmd.Parameters.AddWithValue("@fecha", historial.FechaHistorial.ToString("o")); // ISO 8601
                    cmd.ExecuteNonQuery();
                }
            }

            HistorialAgregado?.Invoke(historial); // Notifica el cambio al que este escuchando
        }

        // Obtener todas las entradas del historial
        public List<Historial> ObtenerHistorialCompleto()
        {
            var lista = new List<Historial>();

            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"SELECT Id, Titulo, Descripcion, FechaHistorial 
                                 FROM Historiales 
                                 ORDER BY FechaHistorial DESC";

                using (var cmd = new SQLiteCommand(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Historial
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Titulo = reader["Titulo"].ToString(),
                            Descripcion = reader["Descripcion"].ToString(),
                            FechaHistorial = DateTime.Parse(reader["FechaHistorial"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        // Eliminar historial completo (opcional)
        public void EliminarTodoElHistorial()
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "DELETE FROM Historiales";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
