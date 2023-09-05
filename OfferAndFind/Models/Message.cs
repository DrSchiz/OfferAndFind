using System;
using System.Collections.Generic;

namespace OfferAndFindAPI.Models
{
    public partial class Message
    {
        public int? IdMessage { get; set; }
        public string? Text { get; set; }
        public int? IdAd { get; set; }
        public int? IdChat { get; set; }
        public int? IdType { get; set; }
    }
}
