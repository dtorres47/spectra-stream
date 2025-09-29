using System;

namespace SpectraStream.Api.Models
{
    public class TTSItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Voice { get; set; } = string.Empty;
        public string Donor { get; set; } = string.Empty;
        public long AmountCents { get; set; }
        public string Msg { get; set; } = string.Empty;
        public long CreatedUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Status { get; set; } = "pending";
    }
}