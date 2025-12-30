using System.Globalization;
using MyApi.Models;

namespace MyApi
{
    public interface ISteamMarketService
    {
        Task<decimal?> GetItemPriceAsync(string marketHashName);
    }

    public class SteamMarketService : ISteamMarketService
    {
        private readonly HttpClient _http;

        public SteamMarketService(HttpClient http)
        {
            _http = http;
        }

        public async Task<decimal?> GetItemPriceAsync(string marketHashName)
        {
            if (string.IsNullOrWhiteSpace(marketHashName)) return null;
            var url = $"https://steamcommunity.com/market/priceoverview/?appid=730&currency=1&market_hash_name={Uri.EscapeDataString(marketHashName)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0");
            
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<SteamPriceResponse>();

            if (data == null || !data.success) return null;

            return ParsePrice(data.lowest_price) ?? ParsePrice(data.median_price);
        }

        private decimal? ParsePrice(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            
            var cleaned = new string(
                value.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray() 
            );

            cleaned = cleaned.Replace(",", ".");

            return decimal.TryParse(
                cleaned,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var price) ? price : null;
            
        }
    }
}