using System;

namespace SpectraStream.Api.Models
{
    public class Donation
    {
        public DateTime Time { get; set; }
        public string Donor { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}