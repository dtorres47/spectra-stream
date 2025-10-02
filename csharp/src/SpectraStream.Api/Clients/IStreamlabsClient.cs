using System.Threading.Tasks;

namespace SpectraStream.Api.Clients
{
    public interface IStreamlabsClient
    {
        Task<string?> GetSpecialStringAsync();
    }
}