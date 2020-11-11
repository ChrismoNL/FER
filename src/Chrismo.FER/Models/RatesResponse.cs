using System;
using System.Collections.Generic;

namespace Chrismo.FER.Models
{
    public class RatesResponse
    {
        public Currencies Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<Currencies, decimal> Rates { get; set; }
    }
}
