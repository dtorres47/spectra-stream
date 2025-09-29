using System;

namespace SpectraStream.Api.Models
{
    public class RequestItem
    {
        public int Id { get; set; }
        public string Board { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string MaskedPhone { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public long CreatedUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}