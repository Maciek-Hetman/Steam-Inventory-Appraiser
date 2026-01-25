using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Entities;
using MyApi.Services;

namespace MyApi.Controllers;

[ApiController]
[Route("api/import-export")]
public class ImportExportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IImportExportService _importExport;
    private readonly ILogger<ImportExportController> _logger;

    public ImportExportController(
        AppDbContext db,
        IImportExportService importExport,
        ILogger<ImportExportController> logger)
    {
        _db = db;
        _importExport = importExport;
        _logger = logger;
    }

    [HttpGet("export/json")]
    public async Task<IActionResult> ExportToJson()
    {
        try
        {
            var valuations = await Task.Run(() => _db.InventoryValuations
                .AsEnumerable()
                .ToList());

            var json = _importExport.ExportToJson(valuations);

            return Ok(new
            {
                success = true,
                data = json,
                count = valuations.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to JSON");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpGet("export/xml")]
    public async Task<IActionResult> ExportToXml()
    {
        try
        {
            var valuations = await Task.Run(() => _db.InventoryValuations
                .AsEnumerable()
                .ToList());

            var xml = _importExport.ExportToXml(valuations);

            return Ok(new
            {
                success = true,
                data = xml,
                count = valuations.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to XML");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpGet("export/json/{steamId64}")]
    public async Task<IActionResult> ExportProfileToJson(string steamId64)
    {
        try
        {
            var valuations = await Task.Run(() => _db.InventoryValuations
                .Where(v => v.SteamId64 == steamId64)
                .AsEnumerable()
                .ToList());

            if (!valuations.Any())
                return NotFound(new { success = false, error = "No valuations found for this Steam ID" });

            var json = _importExport.ExportToJson(valuations);

            return Ok(new
            {
                success = true,
                data = json,
                count = valuations.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile to JSON");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpGet("export/xml/{steamId64}")]
    public async Task<IActionResult> ExportProfileToXml(string steamId64)
    {
        try
        {
            var valuations = await Task.Run(() => _db.InventoryValuations
                .Where(v => v.SteamId64 == steamId64)
                .AsEnumerable()
                .ToList());

            if (!valuations.Any())
                return NotFound(new { success = false, error = "No valuations found for this Steam ID" });

            var xml = _importExport.ExportToXml(valuations);

            return Ok(new
            {
                success = true,
                data = xml,
                count = valuations.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile to XML");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("import/json")]
    public async Task<IActionResult> ImportFromJson([FromBody] ImportJsonRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.JsonData))
                return BadRequest(new { success = false, error = "JSON data is required" });

            var dtos = _importExport.ImportFromJson(request.JsonData);

            if (!dtos.Any())
                return BadRequest(new { success = false, error = "No valid data found in JSON" });

            var importedCount = 0;

            foreach (var dto in dtos)
            {
                var existingValuation = await _db.InventoryValuations
                    .FirstOrDefaultAsync(v => v.SteamId64 == dto.SteamId64);

                if (existingValuation != null)
                {
                    existingValuation.TotalValueUsd = dto.TotalValueUsd;
                    existingValuation.CreatedAt = dto.CreatedAt;
                    existingValuation.Items = dto.Items.Select(i => new InventoryValuationItem
                    {
                        MarketHashName = i.MarketHashName,
                        Amount = i.Amount,
                        ValueUsd = i.ValueUsd
                    }).ToList();
                    _db.InventoryValuations.Update(existingValuation);
                }
                else
                {
                    var valuation = new InventoryValuation
                    {
                        SteamId64 = dto.SteamId64,
                        TotalValueUsd = dto.TotalValueUsd,
                        CreatedAt = dto.CreatedAt,
                        Items = dto.Items.Select(i => new InventoryValuationItem
                        {
                            MarketHashName = i.MarketHashName,
                            Amount = i.Amount,
                            ValueUsd = i.ValueUsd
                        }).ToList()
                    };
                    _db.InventoryValuations.Add(valuation);
                }
                importedCount++;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Successfully imported {importedCount} valuations",
                count = importedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from JSON");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("import/xml")]
    public async Task<IActionResult> ImportFromXml([FromBody] ImportXmlRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.XmlData))
                return BadRequest(new { success = false, error = "XML data is required" });

            var dtos = _importExport.ImportFromXml(request.XmlData);

            if (!dtos.Any())
                return BadRequest(new { success = false, error = "No valid data found in XML" });

            var importedCount = 0;

            foreach (var dto in dtos)
            {
                var existingValuation = await _db.InventoryValuations
                    .FirstOrDefaultAsync(v => v.SteamId64 == dto.SteamId64);

                if (existingValuation != null)
                {
                    existingValuation.TotalValueUsd = dto.TotalValueUsd;
                    existingValuation.CreatedAt = dto.CreatedAt;
                    existingValuation.Items = dto.Items.Select(i => new InventoryValuationItem
                    {
                        MarketHashName = i.MarketHashName,
                        Amount = i.Amount,
                        ValueUsd = i.ValueUsd
                    }).ToList();
                    _db.InventoryValuations.Update(existingValuation);
                }
                else
                {
                    var valuation = new InventoryValuation
                    {
                        SteamId64 = dto.SteamId64,
                        TotalValueUsd = dto.TotalValueUsd,
                        CreatedAt = dto.CreatedAt,
                        Items = dto.Items.Select(i => new InventoryValuationItem
                        {
                            MarketHashName = i.MarketHashName,
                            Amount = i.Amount,
                            ValueUsd = i.ValueUsd
                        }).ToList()
                    };
                    _db.InventoryValuations.Add(valuation);
                }
                importedCount++;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Successfully imported {importedCount} valuations",
                count = importedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from XML");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("reset")]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            var count = await _db.InventoryValuations.CountAsync();
            
            if (count == 0)
            {
                return Ok(new { success = true, message = "Database is already empty", deletedCount = 0 });
            }

            _db.InventoryValuations.RemoveRange(_db.InventoryValuations);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Successfully deleted {count} valuations",
                deletedCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting database");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}

public class ImportJsonRequest
{
    public string JsonData { get; set; } = null!;
}

public class ImportXmlRequest
{
    public string XmlData { get; set; } = null!;
}