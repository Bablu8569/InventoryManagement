using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Data;

namespace InventoryManagement.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("DefaultConnection not found.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public DataTable ExecuteStoredProcedure(string procedureName, Hashtable? parameters = null)
        {
            DataTable dt = new DataTable();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(procedureName, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (DictionaryEntry item in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        item.Key?.ToString() ?? string.Empty,
                        item.Value ?? DBNull.Value
                    );
                }
            }

            conn.Open();

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        public int ExecuteNonQuery(string procedureName, Hashtable? parameters = null)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(procedureName, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (DictionaryEntry item in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        item.Key?.ToString() ?? string.Empty,
                        item.Value ?? DBNull.Value
                    );
                }
            }

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string procedureName, Hashtable? parameters = null)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(procedureName, conn);

            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (DictionaryEntry item in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        item.Key?.ToString() ?? string.Empty,
                        item.Value ?? DBNull.Value
                    );
                }
            }

            conn.Open();
            return cmd.ExecuteScalar();
        }
    }
}