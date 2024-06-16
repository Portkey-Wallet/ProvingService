using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProvingService.Services;

public interface IJwksService
{
    Task<string> GetKeyAsync(string kid);
}

public class JwksService : IJwksService
{
    private readonly ConcurrentDictionary<string, string> _cache;
    private readonly HttpClient _httpClient;
    private readonly JwksSettings _jkwsSettings;

    public JwksService(IOptionsSnapshot<JwksSettings> jkwsEndpointsSettings)
    {
        _jkwsSettings = jkwsEndpointsSettings.Value;
        _cache = new ConcurrentDictionary<string, string>();
        _httpClient = new HttpClient();
    }

    public async Task<string> GetKeyAsync(string kid)
    {
        if (_cache.TryGetValue(kid, out var cert))
        {
            return cert;
        }

        await RefreshKeysAsync();
        return _cache.GetValueOrDefault(kid, "");
    }

    private async Task RefreshKeysAsync()
    {
        foreach (var endpoint in _jkwsSettings.Endpoints)
        {
            var jsonStr = await _httpClient.GetStringAsync(endpoint);
            var jsonValue = JsonConvert.DeserializeObject<JObject>(jsonStr);
            var keys = jsonValue?["keys"];
            if (keys == null) continue;
            foreach (var key in keys)
            {
                var kid = key["kid"]?.ToString();
                var n = key["n"]?.ToString();
                if (kid == null) continue;
                if (n != null)
                    _cache.TryAdd(kid, n);
            }
        }
    }
}