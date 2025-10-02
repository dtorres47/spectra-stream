using SpectraStream.Api.Clients;
using SpectraStream.Api.Models;
using System.Text.Json;

namespace SpectraStream.Api.Services
{
    public class DonationService
    {
        private readonly HttpClient _http;
        private readonly IStreamlabsClient _streamlabs;

        private readonly string _filePath;

        public DonationService(IWebHostEnvironment env, IStreamlabsClient streamlabs)
        {
            _streamlabs = streamlabs;
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

        public async Task<string?> GetSpecialStringAsync()
        {
            var response = await _http.GetStringAsync("http://localhost:3000/mock/streamlabs/donations");

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            foreach (var donation in root.GetProperty("donations").EnumerateArray())
            {
                var msg = donation.GetProperty("message").GetString();
                if (msg != null && msg.StartsWith("QST-"))
                {
                    return msg; // return the first matching special string
                }
            }

            return null;
        }

        public async Task<string?> LookupQuestCodeAsync()
        {
            return await _streamlabs.GetSpecialStringAsync();
        }
    }
}