using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend_app.Models
{
    public class Invoice
    {
        public int SzamlaID { get; set; }
        public int FelhasznaloID { get; set; }
        public DateTime Datum { get; set; }
        public decimal Osszeg { get; set; }
        public string Leiras { get; set; }
        

    }
}