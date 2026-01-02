using MyApi.Models;

namespace MyApi.Services;

public interface ISteamInventoryService
{
    Task<SteamInventoryResponse?> GetInventoryAsync(
        string steamId64,
        int appId = 730,
        string contextId = "2");
}
