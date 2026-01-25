namespace MyApi.Entities;

public class InventoryValuationItem
{
    public int Id { get; set; }
    public string MarketHashName { get; set; } = null!;
    public int Amount { get; set; }
    public decimal? ValueUsd { get; set; }

    public int InventoryValuationId { get; set; }
}
