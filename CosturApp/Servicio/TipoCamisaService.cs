using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using CosturApp.Modelo;

namespace CosturApp.Servicio
{
    public class TipoCamisaService
    {
        private string _rutaDB = Path.Combine(@"C:\CosturApp\data", "anexos.db");
        private string _cadenaConexion => $"Data Source={_rutaDB};Version=3;";

        public TipoCamisaService()
        {
            if (!File.Exists(_rutaDB))
                SQLiteConnection.CreateFile(_rutaDB);

            CrearTablaTipoCamisaSiNoExiste();
        }

        private void CrearTablaTipoCamisaSiNoExiste()
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"CREATE TABLE IF NOT EXISTS TipoCamisa (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Nombre TEXT NOT NULL UNIQUE
                                )";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.ExecuteNonQuery();
                }

                string queryCount = "SELECT COUNT(*) FROM TipoCamisa";
                using (var cmd = new SQLiteCommand(queryCount, conexion))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Insertar valores por defecto
                        var tiposCamisa = new List<string>
                        {
                            "Vestir",
                            "Laboral",
                            "Guayabera",
                            "Arreglos",
                            "Encargos",
                            "Muestras",
                            "Otras"
                        };

                        foreach (var nombre in tiposCamisa)
                        {
                            string insertQuery = "INSERT INTO TipoCamisa (Nombre) VALUES (@nombre)";
                            using (var insertCmd = new SQLiteCommand(insertQuery, conexion))
                            {
                                insertCmd.Parameters.AddWithValue("@nombre", nombre);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        public List<TipoCamisa> ObtenerTodos()
        {
            var lista = new List<TipoCamisa>();
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, Nombre FROM TipoCamisa";
                using (var cmd = new SQLiteCommand(query, conexion))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TipoCamisa
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        public void AgregarSiNoExiste(string nombre)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT OR IGNORE INTO TipoCamisa (Nombre) VALUES (@nombre)";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public TipoCamisa ObtenerPorNombre(string nombre)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, Nombre FROM TipoCamisa WHERE Nombre = @nombre";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TipoCamisa
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public TipoCamisa ObtenerPorId(int id)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, Nombre FROM TipoCamisa WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TipoCamisa
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

    }
}
