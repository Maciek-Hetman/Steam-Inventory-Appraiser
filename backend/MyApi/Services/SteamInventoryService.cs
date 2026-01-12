using System.Text.Json;
using MyApi.Models;

namespace MyApi.Services;

public class SteamInventoryService : ISteamInventoryService
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<SteamInventoryService> _logger;

    public SteamInventoryService(
        IHttpClientFactory http,
        ILogger<SteamInventoryService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SteamInventoryResponse?> GetInventoryAsync(
        string steamId64,
        int appId = 730,
        string contextId = "2")
    {
        var client = _http.CreateClient("SteamClient");
        var url = $"inventory/{steamId64}/{appId}/{contextId}/?l=english&count=1000";

        _logger.LogInformation("Steam inventory URL: {Url}", client.BaseAddress + url);

        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Failed to fetch inventory for {SteamId}. Status: {StatusCode}",
                steamId64,
                response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<SteamInventoryResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
