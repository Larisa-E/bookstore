using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Internal;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql= MySql.Data.MySqlClient;



public class DatabaseHelper
{
    private static readonly string connectionString = ConfigurationManager.ConnectionStrings["BookstoreDB"].ConnectionString;

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }

    public static void ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static MySqlDataReader ExecuteReader(string sql)
    {
        var conn = GetConnection();
        conn.Open();
        var cmd = new MySqlCommand(sql, conn);
        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }
}