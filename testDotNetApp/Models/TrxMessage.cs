using System;
using System.Collections.Generic;

namespace testDotNetApp.Models
{
    public partial class TrxMessage
    {
        public string PartnerKey { get; set; }
        public string PartnerRefNo { get; set; }
        public long? TotalAmount { get; set; }
        public string Timestamp { get; set; }
        public ItemDetail[] Items { get; set; }
        public string Sig { get; set; }
        public string PartnerPassword { get; set; }
    }
}