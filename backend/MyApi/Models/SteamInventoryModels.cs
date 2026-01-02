using System.Text.Json.Serialization;

namespace MyApi.Models
{
    public class SteamInventoryResponse
    {
        [JsonPropertyName("assets")]
        public List<SteamAsset>? Assets { get; set; }

        [JsonPropertyName("descriptions")]
        public List<SteamDescription>? Descriptions { get; set; }

        [JsonPropertyName("total_inventory_count")]
        public int Total_inventory_count { get; set; }

        [JsonPropertyName("success")]
        public int Success { get; set; }
    }

    public class SteamAsset
    {
        [JsonPropertyName("appid")]
        public int Appid { get; set; } 

        [JsonPropertyName("contextid")]
        public string? Contextid { get; set; }

        [JsonPropertyName("assetid")]
        public string? Assetid { get; set; }

        [JsonPropertyName("classid")]
        public string? Classid { get; set; }

        [JsonPropertyName("instanceid")]
        public string? Instanceid { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }
    }

    public class SteamDescription
    {
        [JsonPropertyName("appid")]
        public int Appid { get; set; }

        [JsonPropertyName("classid")]
        public string? Classid { get; set; }

        [JsonPropertyName("instanceid")]
        public string? Instanceid { get; set; }

        [JsonPropertyName("market_hash_name")]
        public string? MarketHashName { get; set; }

        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        [JsonPropertyName("tradable")]
        public int Tradable { get; set; }
        
        [JsonPropertyName("marketable")]
        public int Marketable { get; set; }
    }

    public class InventoryItem
    {
        public string MarketHashName { get; set; } = string.Empty;
    }

    public class SteamPriceResponse
    {
        public bool success { get; set; }
        public string? lowest_price { get; set; }
        public string? median_price { get; set; }
    }
}