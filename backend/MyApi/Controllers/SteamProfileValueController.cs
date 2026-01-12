using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Entities;
using MyApi.Helpers;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/value/steam")]
public class SteamProfileValueController : ControllerBase
{
    private readonly ISteamInventoryService _inventory;
    private readonly ISteamMarketService _market;
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _http;

    public SteamProfileValueController(
        ISteamInventoryService inventory,
        ISteamMarketService market,
        AppDbContext db,
        IHttpClientFactory http)
    {
        _inventory = inventory;
        _market = market;
        _db = db;
        _http = http;
    }

    // POST api/value/steam/profile
    [HttpGet("profile")]
    public async Task<IActionResult> ValueProfile([FromQuery] string steamId64)
    {
        if (steamId64 == null)
            return BadRequest("Invalid Steam profile URL.");

        var inventory = await _inventory.GetInventoryAsync(steamId64);

        if (inventory == null || inventory.Success != 1)
            return BadRequest("Inventory not accessible.");

        var amounts = inventory.Assets!
            .GroupBy(a => $"{a.Classid}_{a.Instanceid}")
            .ToDictionary(
                g => g.Key,
                g => g.Sum(a => int.Parse(a.Amount ?? "1"))
            );

        var items = new List<InventoryValuationItem>();

        foreach (var desc in inventory.Descriptions!)
        {
            if (desc.Marketable != 1 || desc.MarketHashName == null)
                continue;

            var key = $"{desc.Classid}_{desc.Instanceid}";
            if (!amounts.TryGetValue(key, out var amount))
                continue;

            var price = await _market.GetItemPriceAsync(desc.MarketHashName);

            // Skip items valued at $0.01 or less to avoid noise in responses
            var itemValue = price.HasValue ? price * amount : null;
            if (!itemValue.HasValue || itemValue.Value <= 0.01m)
                continue;

            items.Add(new InventoryValuationItem
            {
                MarketHashName = desc.MarketHashName,
                Amount = amount,
                ValueUsd = itemValue
            });
        }

        var total = items
            .Where(i => i.ValueUsd.HasValue)
            .Sum(i => i.ValueUsd!.Value);

        // Check if this Steam ID already exists
        var existingValuation = await _db.InventoryValuations
            .FirstOrDefaultAsync(v => v.SteamId64 == steamId64);

        if (existingValuation != null)
        {
            // Update existing valuation instead of creating duplicate
            existingValuation.TotalValueUsd = total;
            existingValuation.CreatedAt = DateTime.UtcNow;
            existingValuation.Items = items;
            _db.InventoryValuations.Update(existingValuation);
        }
        else
        {
            // Create new valuation
            var valuation = new InventoryValuation
            {
                SteamId64 = steamId64,
                TotalValueUsd = total,
                Items = items
            };
            _db.InventoryValuations.Add(valuation);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            steamId64,
            totalValueUSD = total,
            items = items.Select(i => new
            {
                marketHashName = i.MarketHashName,
                valueUSD = i.ValueUsd
            })
        });
    }
}
