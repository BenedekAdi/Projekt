using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Backend_app.Models;

namespace Backend_app.Controllers
{
    [RoutePrefix("api/Test")]
    public class TestController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["conn"].ConnectionString);
        SqlCommand cmd = null;
        SqlDataAdapter da = null;

        [HttpPost]
        [Route("Registration")]
        public string Registration(User user)
        {
            string msg = string.Empty;
            try
            {
                cmd = new SqlCommand("Regist", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Vezetek_nev", user.Vezetek_nev);
                cmd.Parameters.AddWithValue("@Kereszt_nev", user.Kereszt_nev);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@telefonszam", user.PhoneNo);
                cmd.Parameters.AddWithValue("@jelszo", user.Jelszo);

                conn.Open();
                int i = cmd.ExecuteNonQuery();
                conn.Close();
                if (i > 0)
                {
                    cmd = new SqlCommand("SELECT Felhasz_id FROM Felhasznalo WHERE email = @email", conn);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    user.Felhasz_id = (int)cmd.ExecuteScalar();

                    if (user.Felhasz_id > 0)
                    {
                        cmd = new SqlCommand("INSERT INTO Szamla (Felhasz_id, Datum, Osszeg, Leiras) VALUES (@Felhasz_id, GETDATE(), 0, 'Kezdeti számla')", conn);
                        cmd.Parameters.AddWithValue("@Felhasz_id", user.Felhasz_id);
                        cmd.ExecuteNonQuery();
                    }

                    msg = "Data inserted.";
                }
                else
                {
                    msg = "Error.";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally { conn.Close(); }
            
            return msg;
        }

        [HttpPost]
        [Route("Login")]
        public string Login(User user)
        {
            string msg = string.Empty;
            try
            {
                da = new SqlDataAdapter("LoginUser", conn);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@email", user.Email);
                da.SelectCommand.Parameters.AddWithValue("@jelszo", user.Jelszo);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    msg = "User is valid";
                  
                }
                else
                {
                    msg = "User is Invalid";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return msg;
        }

        [HttpGet]
        [Route("GetInvoiceHistory/{userId}")]
        public IHttpActionResult GetInvoiceHistory(int userId)
        {
            try
            {
                cmd = new SqlCommand("SELECT * FROM Szamla WHERE Felhasz_id = @Felhasz_id", conn);
                cmd.Parameters.AddWithValue("@Felhasz_id", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                List<Invoice> invoices = new List<Invoice>();

                while (reader.Read())
                {
                    Invoice invoice = new Invoice
                    {
                        SzamlaID = (int)reader["SzamlaID"],
                        FelhasznaloID = (int)reader["Felhasz_id"],
                        Datum = (DateTime)reader["Datum"],
                        Osszeg = (decimal)reader["Osszeg"],
                        Leiras = reader["Leiras"].ToString()
                    };

                    invoices.Add(invoice);
                }

                conn.Close();

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [Route("TransferMoney")]
        public string TransferMoney(int feladoID, int cimzettID, decimal osszeg)
        {
            string msg = string.Empty;
            try
            {
                
                cmd = new SqlCommand("SELECT Egyenleg FROM Szamla WHERE Felhasz_id = @Felhasz_id", conn);
                cmd.Parameters.AddWithValue("@Felhasz_id", feladoID);
                conn.Open();
                decimal egyenleg = (decimal)cmd.ExecuteScalar();
                conn.Close();

                if (egyenleg >= osszeg)
                {
                    
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    cmd = new SqlCommand("INSERT INTO Utalas (Felhasz_id, CimzettID, Osszeg, Datum) VALUES (@Felhasz_id, @CimzettID, @Osszeg, GETDATE())", conn, transaction);
                    cmd.Parameters.AddWithValue("@FeladoID", feladoID);
                    cmd.Parameters.AddWithValue("@CimzettID", cimzettID);
                    cmd.Parameters.AddWithValue("@Osszeg", osszeg);

                    int i = cmd.ExecuteNonQuery();

                    
                    if (i > 0)
                    {
                        cmd = new SqlCommand("UPDATE Szamla SET Egyenleg = Egyenleg - @Osszeg WHERE Felhasz_id = @Felhasz_id", conn, transaction);
                        cmd.Parameters.AddWithValue("@Osszeg", osszeg);
                        cmd.Parameters.AddWithValue("@Felhasz_id", feladoID);
                        cmd.ExecuteNonQuery();

                        
                        cmd = new SqlCommand("UPDATE Szamla SET Egyenleg = Egyenleg + @Osszeg WHERE Felhasz_id = @Felhasz_id", conn, transaction);
                        cmd.Parameters.AddWithValue("@Osszeg", osszeg);
                        cmd.Parameters.AddWithValue("@Felhasz_id", cimzettID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        msg = "Transfer successful.";
                    }
                    else
                    {
                        transaction.Rollback();
                        msg = "Transfer failed.";
                    }

                    conn.Close();
                }
                else
                {
                    msg = "Insufficient funds.";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally
            {
                conn.Close();
            }

            return msg;
        }




    }//routerprefic vége
}
