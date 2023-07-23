using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pantree.Server.Tests.Utilities
{
    internal static class HttpHelpers
    {
        internal static async Task<T?> DeserializeRequestContent<T>(this HttpContent content)
        {
            string contentText = await content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(contentText, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
