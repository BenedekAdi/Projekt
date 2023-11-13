using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend_app.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Felh_nev { get; set; }
        public string Jelszo { get; set; }
        public string Vezetek_nev { get; set; }
        public string Kereszt_nev { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }

        public string számla { get; set; }
    }
}