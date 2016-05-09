using System;
using System.Dynamic;
using System.Reflection;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Zhichkin.ORM
{
    public sealed class QueryService
    {
        private readonly string ConnectionString;
        public QueryService(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public int ExecuteNonQuery(string sql)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }
        public object ExecuteScalar(string sql)
        {
            object result = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    result = command.ExecuteScalar();
                }
            }
            return result;
        }
        public List<dynamic> Execute(string sql)
        {
            List<dynamic> result = new List<dynamic>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic item = new ExpandoObject();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string name = reader.GetName(i);
                                object value = reader.GetValue(i);
                                ((IDictionary<string, object>)item).Add(name, value);
                            }
                            result.Add(item);
                        }
                    }
                }
            }
            return result;
        }
    }
}