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
            try
            {
                _connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection not found in appsettings.json.");
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("Configuration is null. Please check dependency injection.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing DatabaseHelper: " + ex.Message, ex);
            }
        }

        public SqlConnection GetConnection()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new Exception("Connection string is null or empty.");
                }
                return new SqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating database connection: " + ex.Message, ex);
            }
        }

        public DataTable ExecuteStoredProcedure(string procedureName, Hashtable? parameters = null)
        {
            DataTable dt = new DataTable();

            try
            {
                if (string.IsNullOrEmpty(procedureName))
                {
                    throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
                }

                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn);

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (DictionaryEntry item in parameters)
                    {
                        string key = item.Key?.ToString() ?? string.Empty;
                        object value = item.Value ?? DBNull.Value;

                        if (string.IsNullOrEmpty(key))
                        {
                            continue; // Skip empty keys
                        }

                        cmd.Parameters.AddWithValue(key, value);
                    }
                }

                conn.Open();

                using SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                return dt;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error executing stored procedure '{procedureName}': " + ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Invalid operation: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing stored procedure '{procedureName}': " + ex.Message, ex);
            }
        }

        public int ExecuteNonQuery(string procedureName, Hashtable? parameters = null)
        {
            try
            {
                if (string.IsNullOrEmpty(procedureName))
                {
                    throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
                }

                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn);

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (DictionaryEntry item in parameters)
                    {
                        string key = item.Key?.ToString() ?? string.Empty;
                        object value = item.Value ?? DBNull.Value;

                        if (string.IsNullOrEmpty(key))
                        {
                            continue; // Skip empty keys
                        }

                        cmd.Parameters.AddWithValue(key, value);
                    }
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error executing non-query '{procedureName}': " + ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Invalid operation: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing non-query '{procedureName}': " + ex.Message, ex);
            }
        }

        public object? ExecuteScalar(string procedureName, Hashtable? parameters = null)
        {
            try
            {
                if (string.IsNullOrEmpty(procedureName))
                {
                    throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
                }

                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, conn);

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (DictionaryEntry item in parameters)
                    {
                        string key = item.Key?.ToString() ?? string.Empty;
                        object value = item.Value ?? DBNull.Value;

                        if (string.IsNullOrEmpty(key))
                        {
                            continue; // Skip empty keys
                        }

                        cmd.Parameters.AddWithValue(key, value);
                    }
                }

                conn.Open();

                object? result = cmd.ExecuteScalar();

                // Handle DBNull values
                if (result == DBNull.Value)
                {
                    return null;
                }

                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error executing scalar '{procedureName}': " + ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Invalid operation: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing scalar '{procedureName}': " + ex.Message, ex);
            }
        }
    }
}