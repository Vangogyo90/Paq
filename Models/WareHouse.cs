using System;
using System.Collections.Generic;

namespace Paq.Models
{
    public partial class WareHouse
    {
        public int? IdWareHouse { get; set; }
        public int? CityId { get; set; }
        public string? Adress { get; set; }
        public City? City { get; set; }
    }
}
