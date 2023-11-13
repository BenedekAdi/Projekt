using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Backend_app.Controllers
{
    public class TranslatorController : ApiController
    {
        public IHttpActionResult Get()
        {
            string connectionString = "Server = A szerver; " +
                                      "Database = A database;" +
                                      "User_Id =  Felhasználónév;" +
                                      "Password = jelszó;";
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM sajját tábla";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> rowData = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                rowData.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(rowData);
                        }
                    }
                }
            }
            return Ok(data);


        }
    }
}
