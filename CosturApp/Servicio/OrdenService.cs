using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using CosturApp.Modelo;

namespace CosturApp.Servicio
{
    public class OrdenService
    {
        private string _rutaDB = "anexos.db";
        private string _cadenaConexion => $"Data Source={_rutaDB};Version=3;";

        public OrdenService()
        {
            // Si la base de datos no existe, la crea
            if (!File.Exists(_rutaDB))
                SQLiteConnection.CreateFile(_rutaDB);

            CrearTablaOrdenesSiNoExiste(); // Crear la tabla de ordenes si no existe
        }

        // Crea tabla de ordenes si no existe
        private void CrearTablaOrdenesSiNoExiste()
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = @"CREATE TABLE IF NOT EXISTS Ordenes (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            NumeroOrden TEXT NOT NULL,
                            TotalCamisetas INTEGER NOT NULL,
                            AnexoId INTEGER,
                            FOREIGN KEY(AnexoId) REFERENCES Anexos(Id)
                         )";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Agregar una nueva orden a la tabla
        public void AgregarOrden(Orden orden)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT INTO Ordenes (NumeroOrden, TotalCamisetas, AnexoId) VALUES (@numeroOrden, @totalCamisetas, @anexoId)";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@numeroOrden", orden.NumeroOrden);
                    cmd.Parameters.AddWithValue("@totalCamisetas", orden.TotalCamisetas);
                    cmd.Parameters.AddWithValue("@anexoId", orden.AnexoId); // Relacionado con el anexo
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Obtener las ordenes asociadas a un anexo concreto
        public List<Orden> ObtenerOrdenesPorAnexo(int idAnexo)
        {
            var lista = new List<Orden>();

            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Id, NumeroOrden, TotalCamisetas, AnexoId FROM Ordenes WHERE AnexoId = @idAnexo";

                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@idAnexo", idAnexo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Orden
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                NumeroOrden = reader["NumeroOrden"].ToString(),
                                TotalCamisetas = Convert.ToInt32(reader["TotalCamisetas"]),
                                AnexoId = Convert.ToInt32(reader["AnexoId"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // Eliminar una orden
        public void EliminarOrden(int id)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "DELETE FROM Ordenes WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Editar una orden (es decir actualizar los detalles)
        public void EditarOrden(Orden orden)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "UPDATE Ordenes SET NumeroOrden = @numeroOrden, TotalCamisetas = @totalCamisetas WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@numeroOrden", orden.NumeroOrden);
                    cmd.Parameters.AddWithValue("@totalCamisetas", orden.TotalCamisetas);
                    cmd.Parameters.AddWithValue("@id", orden.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
