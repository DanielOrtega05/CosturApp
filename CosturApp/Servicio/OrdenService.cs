using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using CosturApp.Modelo;

namespace CosturApp.Servicio
{
    public class OrdenService
    {
        private string _rutaDB = Path.Combine(@"C:\CosturApp\data", "anexos.db");
        private string _cadenaConexion => $"Data Source={_rutaDB};Version=3;";

        public OrdenService()
        {
            string carpeta = Path.GetDirectoryName(_rutaDB);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

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
                string query = @"
                CREATE TABLE IF NOT EXISTS TipoCamisa (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL UNIQUE
                );

                CREATE TABLE IF NOT EXISTS Ordenes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NumeroOrden TEXT NOT NULL,
                    TotalCamisetas INTEGER NOT NULL,
                    TipoCamisaId INTEGER NOT NULL,
                    AnexoId INTEGER,
                    FOREIGN KEY(TipoCamisaId) REFERENCES TipoCamisa(Id),
                    FOREIGN KEY(AnexoId) REFERENCES Anexos(Id)
                );";

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

        // Agregar una nueva orden a la tabla
        public void AgregarOrden(Orden orden)
        {
            using (var conexion = new SQLiteConnection(_cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT INTO Ordenes (NumeroOrden, TotalCamisetas, TipoCamisaId, AnexoId) VALUES (@numeroOrden, @totalCamisetas, @tipoCamisaId, @anexoId); SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@numeroOrden", orden.NumeroOrden);
                    cmd.Parameters.AddWithValue("@totalCamisetas", orden.TotalCamisetas);
                    cmd.Parameters.AddWithValue("@tipoCamisaId", orden.TipoCamisaId);
                    cmd.Parameters.AddWithValue("@anexoId", orden.AnexoId);

                    // Devuelve el ID generado de la base de datos
                    long idGenerado = (long)cmd.ExecuteScalar();
                    orden.Id = (int)idGenerado;
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
                string query = "SELECT Id, NumeroOrden, TotalCamisetas, TipoCamisaId, AnexoId FROM Ordenes WHERE AnexoId = @idAnexo";

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
                                TipoCamisaId = Convert.ToInt32(reader["TipoCamisaId"]),
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
                string query = "UPDATE Ordenes SET NumeroOrden = @numeroOrden, TotalCamisetas = @totalCamisetas, TipoCamisaId = @tipoCamisaId WHERE Id = @id";
                using (var cmd = new SQLiteCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@numeroOrden", orden.NumeroOrden);
                    cmd.Parameters.AddWithValue("@totalCamisetas", orden.TotalCamisetas);
                    cmd.Parameters.AddWithValue("@tipoCamisaId", orden.TipoCamisaId);
                    cmd.Parameters.AddWithValue("@id", orden.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
