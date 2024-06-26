using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProvingService.Application.Contracts;

namespace ProvingService.Application;

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
    private readonly ILogger<JwksService> _logger;

    public JwksService(IOptionsSnapshot<JwksSettings> jkwsEndpointsSettings, IHttpClientFactory clientFactory,
        ILogger<JwksService> logger)
    {
        _jkwsSettings = jkwsEndpointsSettings.Value;
        _clientFactory = clientFactory;
        _logger = logger;
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
        var client = _clientFactory.CreateClient();
        client.Timeout = TimeSpan.Parse(_jkwsSettings.Timeout ?? "00:02:00");
        foreach (var endpoint in _jkwsSettings.Endpoints)
        {
            try
            {
                await QueryOneProviderAsync(client, endpoint);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error querying JWKS endpoint {endpoint}: {e.Message}");
            }
        }
    }

    private async Task QueryOneProviderAsync(HttpClient client, string endpoint)
    {
        var jsonStr = await client.GetStringAsync(endpoint);
        var jsonValue = JsonConvert.DeserializeObject<JObject>(jsonStr);
        var keys = jsonValue?["keys"];
        if (keys == null) return;
        foreach (var key in keys)
        {
            var kid = key["kid"]?.ToString();
            var n = key["n"]?.ToString();
            if (kid == null) continue;
            if (n == null) continue;
            if (_cache.TryAdd(kid, n))
            {
                _logger.LogDebug($"Added public key with kid {kid}");
            }
        }
    }
}