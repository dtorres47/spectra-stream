using System.Text.Json;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    public class DonationService
    {
        private readonly string _filePath;

        public DonationService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.WebRootPath, "data", "donations.json");
        }

        public async Task AppendDonationAsync(Donation donation)
        {
            List<Donation> donations = new();

            if (File.Exists(_filePath))
            {
                using var existing = File.OpenRead(_filePath);
                donations = await JsonSerializer.DeserializeAsync<List<Donation>>(existing) ?? new();
            }

            donations.Add(donation);

            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, donations, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}