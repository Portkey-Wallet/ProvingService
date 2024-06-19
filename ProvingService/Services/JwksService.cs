using System;
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

public class PublicKeyNotFoundException : Exception
{
    public PublicKeyNotFoundException(string message) : base(message)
    {
    }
}

public class JwksService : IJwksService
{
    private readonly ConcurrentDictionary<string, string> _cache;
    private readonly JwksSettings _jkwsSettings;
    private readonly IHttpClientFactory _clientFactory;

    public JwksService(IOptionsSnapshot<JwksSettings> jkwsEndpointsSettings, IHttpClientFactory clientFactory)
    {
        _jkwsSettings = jkwsEndpointsSettings.Value;
        _clientFactory = clientFactory;
        _cache = new ConcurrentDictionary<string, string>();
    }

    public async Task<string> GetKeyAsync(string kid)
    {
        if (_cache.TryGetValue(kid, out var cert))
        {
            return cert;
        }

        await RefreshKeysAsync();
        if (_cache.TryGetValue(kid, out var cert1))
        {
            return cert1;
        }

        throw new PublicKeyNotFoundException($"Public key with kid {kid} not found");
    }

    private async Task RefreshKeysAsync()
    {
        foreach (var endpoint in _jkwsSettings.Endpoints)
        {
            var client = _clientFactory.CreateClient();
            var jsonStr = await client.GetStringAsync(endpoint);
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