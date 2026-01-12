namespace MyApi.Models;

public class InventoryValuationExportDto
{
    public string SteamId64 { get; set; } = null!;
    public decimal TotalValueUsd { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InventoryValuationItemExportDto> Items { get; set; } = new();
}

public class InventoryValuationItemExportDto
{
    public string MarketHashName { get; set; } = null!;
    public int Amount { get; set; }
    public decimal? ValueUsd { get; set; }
}
