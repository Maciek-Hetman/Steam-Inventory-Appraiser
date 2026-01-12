using System.Text.Json;
using System.Xml.Serialization;
using MyApi.Entities;
using MyApi.Models;

namespace MyApi.Services;

public interface IImportExportService
{
    string ExportToJson(List<InventoryValuation> valuations);
    string ExportToXml(List<InventoryValuation> valuations);
    List<InventoryValuationExportDto> ImportFromJson(string json);
    List<InventoryValuationExportDto> ImportFromXml(string xml);
}

public class ImportExportService : IImportExportService
{
    private readonly ILogger<ImportExportService> _logger;

    public ImportExportService(ILogger<ImportExportService> logger)
    {
        _logger = logger;
    }

    public string ExportToJson(List<InventoryValuation> valuations)
    {
        try
        {
            var dtos = valuations.Select(v => new InventoryValuationExportDto
            {
                SteamId64 = v.SteamId64,
                TotalValueUsd = v.TotalValueUsd,
                CreatedAt = v.CreatedAt,
                Items = v.Items.Select(i => new InventoryValuationItemExportDto
                {
                    MarketHashName = i.MarketHashName,
                    Amount = i.Amount,
                    ValueUsd = i.ValueUsd
                }).ToList()
            }).ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(dtos, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to JSON");
            throw;
        }
    }

    public string ExportToXml(List<InventoryValuation> valuations)
    {
        try
        {
            var dtos = valuations.Select(v => new InventoryValuationExportDto
            {
                SteamId64 = v.SteamId64,
                TotalValueUsd = v.TotalValueUsd,
                CreatedAt = v.CreatedAt,
                Items = v.Items.Select(i => new InventoryValuationItemExportDto
                {
                    MarketHashName = i.MarketHashName,
                    Amount = i.Amount,
                    ValueUsd = i.ValueUsd
                }).ToList()
            }).ToList();

            var serializer = new XmlSerializer(typeof(List<InventoryValuationExportDto>));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, dtos);
                return writer.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to XML");
            throw;
        }
    }

    public List<InventoryValuationExportDto> ImportFromJson(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtos = JsonSerializer.Deserialize<List<InventoryValuationExportDto>>(json, options);
            return dtos ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from JSON");
            throw;
        }
    }

    public List<InventoryValuationExportDto> ImportFromXml(string xml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(List<InventoryValuationExportDto>));
            using (var reader = new StringReader(xml))
            {
                var result = serializer.Deserialize(reader) as List<InventoryValuationExportDto>;
                return result ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from XML");
            throw;
        }
    }
}
