using System.Text.Json;

namespace SpectraStream.Api.Clients
{
    public class StreamlabsClient : IStreamlabsClient
    {
        private readonly HttpClient _http;

        public StreamlabsClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> GetSpecialStringAsync()
        {
            var response = await _http.GetStringAsync("donations");

            using var doc = JsonDocument.Parse(response);
            foreach (var donation in doc.RootElement.GetProperty("donations").EnumerateArray())
            {
                var msg = donation.GetProperty("message").GetString();
                if (!string.IsNullOrEmpty(msg) && msg.StartsWith("QST-"))
                    return msg;
            }

            return null;
        }
    }
}