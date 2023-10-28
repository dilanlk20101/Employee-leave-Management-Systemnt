using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Odbc;

namespace Test
{
    internal class DBConnection
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter da;

        public DBConnection()
        {
            con = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True");
        }
        public void bdOpen()
        {
            con.Open();
        }

        public int insert_update_delete(string query)
        {
            con.Open();
            cmd = new SqlCommand(query, con);
            int i = cmd.ExecuteNonQuery();
            con.Close();
            return i;
        }
        public void bdClose()
        {
            con.Close ();
        }

        public SqlDataReader Select(string query)
        {
            // Implement the code to execute the SELECT query using ADO.NET and return a SqlDataReader
            // Example:
            SqlConnection connection = new SqlConnection("Data Source=DILA99;Initial Catalog=RRD;Integrated Security=True");
            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }
    }
}
