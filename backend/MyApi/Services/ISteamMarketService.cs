namespace MyApi.Services;

public interface ISteamMarketService
{
    Task<decimal?> GetItemPriceAsync(string marketHashName);
}
