using Microsoft.AspNetCore.Mvc;
using MyApi.Models;
using System.Text.Json;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SteamController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SteamController> _logger;

        public SteamController(IHttpClientFactory httpClientFactory, ILogger<SteamController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // appId = 730 => Counter-Strike 2
        // contextId is item scope
        [HttpGet("{steamId64}")]
        public async Task<IActionResult> GetInventory(string steamId64, [FromQuery] int appId = 730, [FromQuery] string contextId = "2")
        {
            // Check if steamId64 is valid (must be 17-digits long)
            if (!ulong.TryParse(steamId64, out _) || steamId64.Length != 17)
            {
                return BadRequest("Invalid SteamID64.");
            }

            var client = _httpClientFactory.CreateClient("SteamClient");
            var url = $"inventory/{steamId64}/{appId}/{contextId}/?l=english&count=1000";
            _logger.LogInformation("Steam URL: {Url}", client.BaseAddress + url);

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int) response.StatusCode == 429)
                    {
                        return StatusCode(429, "Too many requests.");
                    }

                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Steam error {Status}: {Body}", response.StatusCode, body);

                    return StatusCode((int) response.StatusCode, "Couldn't get inventory. Make sure your profile and inventory are set to public.");
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var inventoryData = JsonSerializer.Deserialize<SteamInventoryResponse>(jsonString, options);

                if (inventoryData == null) return BadRequest("Inventory is empty.");
                else if (inventoryData.Success != 1) return BadRequest("Couldn't read inventory data");

                return Ok(inventoryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inventory fetching");
                return StatusCode(500, "Internal server error.");
            }
        }
    }

    [ApiController]
    [Route("api/value/steam")]
    public class SteamValueController : ControllerBase
    {
        private readonly ISteamMarketService _steam;

        public SteamValueController(ISteamMarketService steam)
        {
            _steam = steam;
        }

        [HttpGet("item")]
        public async Task<IActionResult> GetItemValue([FromQuery(Name = "marketHashName")] string marketHashName)
        {
            var price = await _steam.GetItemPriceAsync(marketHashName);

            if (price == null) return NotFound("Price not found.");

            return Ok(new
            {
                marketHashName,
                priceUsd = price
            });
        }

        [HttpPost("inventory")]
        public async Task<IActionResult> GetInventoryValue([FromBody] List<InventoryItem> inventory)
        {
            if (inventory == null || inventory.Count == 0) return BadRequest("Inventory is empty.");

            var tasks = inventory.Select(async item =>
            {
                var price = await _steam.GetItemPriceAsync(item.MarketHashName);

                return new
                {
                    item.MarketHashName,
                    priceUsd = price
                };
            });

            var results = await Task.WhenAll(tasks);

            var total = results.Where(r => r.priceUsd.HasValue).Sum(r => r.priceUsd!.Value);

            return Ok(new
            {
                totalValueUsd = total,
                items = results
            });
        }
    }
}