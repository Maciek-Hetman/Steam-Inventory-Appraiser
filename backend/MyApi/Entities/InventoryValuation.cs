namespace MyApi.Entities;

public class InventoryValuation
{
    public int Id { get; set; }
    public string SteamId64 { get; set; } = null!;
    public decimal TotalValueUsd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InventoryValuationItem> Items { get; set; } = new();
}
