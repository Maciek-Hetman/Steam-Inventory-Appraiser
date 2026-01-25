using Microsoft.AspNetCore.Mvc;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/steam")]
public class SteamController : ControllerBase
{
    private readonly ISteamInventoryService _inventory;

    public SteamController(ISteamInventoryService inventory)
    {
        _inventory = inventory;
    }

    [HttpGet("inventory/{steamId64}")]
    public async Task<IActionResult> GetInventory(
        string steamId64,
        [FromQuery] int appId = 730,
        [FromQuery] string contextId = "2")
    {
        if (!ulong.TryParse(steamId64, out _) || steamId64.Length != 17)
            return BadRequest("Invalid SteamID64.");

        var inventory = await _inventory.GetInventoryAsync(
            steamId64, appId, contextId);

        if (inventory == null || inventory.Success != 1)
            return BadRequest("Inventory not accessible.");

        return Ok(inventory);
    }
}
