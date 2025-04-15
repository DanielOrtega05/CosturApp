using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosturApp.Modelo;

namespace CosturApp.Servicio
{
    public class AnexoService
    {
        private string _rutaDB = "anexos.db";
        private string _cadenaConexion => $"Data Source={_rutaDB};Version=3;";

        public AnexoService()
        {
            // si no existe lo crea
            if (!File.Exists(_rutaDB))
                SQLiteConnection.CreateFile(_rutaDB);

            CrearTablaSiNoExiste();
        }

        // Crear la tabla de anexos
        private void CrearTablaSiNoExiste()
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"CREATE TABLE IF NOT EXISTS Anexos (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Titulo TEXT NOT NULL,
                                    FechaCreacion TEXT NOT NULL
                                 )";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Hace insert de los anexos
        public void AgregarAnexo(Anexo anexo)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT INTO Anexos (Titulo, FechaCreacion) VALUES (@titulo, @fecha)";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@titulo", anexo.Titulo);
                    cmd.Parameters.AddWithValue("@fecha", anexo.FechaCreacion.ToString("o")); // formato ISO 8601
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Obtiene los anexos para mostrar en el datagrid
        public List<Anexo> ObtenerAnexos()
        {
            var lista = new List<Anexo>();

            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, Titulo, FechaCreacion FROM Anexos";

                using (var cmd = new SQLiteCommand(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Anexo
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Titulo = reader["Titulo"].ToString(),
                            FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        // Elimina el anexo
        public void EliminarAnexo(int id)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "DELETE FROM Anexos WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
