using System.Text.Json;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    public class StateService
    {
        private readonly string _filePath;

        public StateService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "state.json");
        }

        public async Task SaveStateAsync(PersistState state)
        {
            state.SavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var options = new JsonSerializerOptions { WriteIndented = true };
            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, state, options);
        }

        public async Task<PersistState?> LoadStateAsync()
        {
            if (!File.Exists(_filePath))
                return null;

            using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<PersistState>(stream);
        }
    }
}