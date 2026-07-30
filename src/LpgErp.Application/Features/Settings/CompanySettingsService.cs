using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Settings.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Settings;

public interface ICompanySettingsService
{
    Task<Result<CompanySettingsDto>> GetAsync(CancellationToken ct = default);
    Task<Result<CompanySettingsDto>> UpdateAsync(UpdateCompanySettingsRequest request, CancellationToken ct = default);
}

/// <summary>
/// The distributor's own business profile — a singleton, unlike every other feature in this app.
/// There is no create or delete: the row is made on first read if it doesn't exist yet, and from
/// then on only ever updated.
/// </summary>
public class CompanySettingsService : ICompanySettingsService
{
    private readonly IApplicationDbContext _context;

    public CompanySettingsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CompanySettingsDto>> GetAsync(CancellationToken ct = default)
    {
        var settings = await GetOrCreateAsync(ct);
        return Result<CompanySettingsDto>.Success(ToDto(settings));
    }

    public async Task<Result<CompanySettingsDto>> UpdateAsync(UpdateCompanySettingsRequest request, CancellationToken ct = default)
    {
        var settings = await GetOrCreateAsync(ct);

        settings.Name = request.Name;
        settings.Address = request.Address;
        settings.Phone = request.Phone;
        settings.Email = request.Email;
        settings.Website = request.Website;

        await _context.SaveChangesAsync(ct);
        return Result<CompanySettingsDto>.Success(ToDto(settings));
    }

    /// <summary>
    /// Fixed, well-known id for the one row that should ever exist — not a real entity identity,
    /// just a way to give two concurrent first-ever requests (this endpoint is anonymous, so the
    /// login page and every page's sidebar can all hit it at once right after a fresh deploy) a
    /// primary key to collide on, instead of each quietly inserting its own row.
    /// </summary>
    private static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    private async Task<CompanySettings> GetOrCreateAsync(CancellationToken ct)
    {
        var existing = await _context.CompanySettings.FirstOrDefaultAsync(s => s.Id == SingletonId, ct);
        if (existing is not null) return existing;

        // A row from before this fixed id existed is still the real one — use it rather than
        // creating a second row and losing whatever was already configured.
        var legacy = await _context.CompanySettings.FirstOrDefaultAsync(ct);
        if (legacy is not null) return legacy;

        var created = new CompanySettings { Id = SingletonId, Name = "My Company" };
        await _context.CompanySettings.AddAsync(created, ct);
        try
        {
            await _context.SaveChangesAsync(ct);
            return created;
        }
        catch (DbUpdateException)
        {
            // Lost the race — someone else's insert landed first. Removing an entity that was never
            // actually saved (still in the Added state) just detaches it from tracking; nothing to
            // delete. Their row, not this one, is the real one now.
            _context.CompanySettings.Remove(created);
            return await _context.CompanySettings.FirstAsync(s => s.Id == SingletonId, ct);
        }
    }

    private static CompanySettingsDto ToDto(CompanySettings s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Address = s.Address,
        Phone = s.Phone,
        Email = s.Email,
        Website = s.Website,
    };
}
