using System;
using System.Collections.Generic;

namespace OfferAndFindAPI.Models
{
    public partial class Ad
    {
        public int? IdAd { get; set; }
        public string? Header { get; set; } = null!;
        public int? IdUser { get; set; }
        public int? IdStatus { get; set; }
        public int? IdType1 { get; set; }
        public int? IdType { get; set; }
        public string? Text { get; set; } = null!;
        public int? Salary { get; set; }

    }
}
