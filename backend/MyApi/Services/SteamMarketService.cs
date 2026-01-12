using System.Globalization;
using System.Text.Json;
using MyApi.Models;

namespace MyApi.Services;

public class SteamMarketService : ISteamMarketService
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<SteamMarketService> _logger;

    public SteamMarketService(IHttpClientFactory http, ILogger<SteamMarketService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<decimal?> GetItemPriceAsync(string marketHashName)
    {
        if (string.IsNullOrWhiteSpace(marketHashName))
            return null;

        var client = _http.CreateClient("SteamClient");

        // Steam Community Market
        var url =
            $"market/priceoverview/?" +
            $"currency=1&" + // USD
            $"appid=730&" +
            $"market_hash_name={Uri.EscapeDataString(marketHashName)}";

        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to fetch price for {MarketHashName}. Status: {StatusCode}",
                marketHashName,
                response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        var price = JsonSerializer.Deserialize<SteamPriceResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (price == null || !price.success)
            return null;

        var raw = price.lowest_price ?? price.median_price;
        if (raw == null)
            return null;

        raw = raw.Replace("$", "").Trim();

        if (decimal.TryParse(
            raw,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var value))
        {
            return value;
        }

        return null;
    }
}
