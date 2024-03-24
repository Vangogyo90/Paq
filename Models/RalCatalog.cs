using System;
using System.Collections.Generic;

namespace Paq.Models
{
    public partial class RalCatalog
    {
        public int? IdRalCatalog { get; set; }
        public string NameRal { get; set; } = null!;
        public string ColorRal { get; set; } = null!;
        public string ColorHtml { get; set; } = null!;
    }
}
